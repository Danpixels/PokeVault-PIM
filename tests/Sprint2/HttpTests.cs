using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
partial class Suite
{
    sealed class Session : IDisposable
    {
        public HttpClient Client {get;} = new(new HttpClientHandler { CookieContainer=new CookieContainer(),AllowAutoRedirect=false }) { BaseAddress=new Uri("http://localhost:5188"),Timeout=TimeSpan.FromSeconds(20) };
        public string Token="";
        public void Dispose()=>Client.Dispose();
    }
    static async Task<(int code,JsonElement data,string text)> Send(Session s,HttpMethod method,string path,object? body=null,bool token=true)
    {
        using var q=new HttpRequestMessage(method,path);if(body!=null)q.Content=JsonContent.Create(body);
        if(token && s.Token!="")q.Headers.Add("X-CSRF-TOKEN",s.Token);
        using var r=await s.Client.SendAsync(q);var text=await r.Content.ReadAsStringAsync();JsonElement data=default;
        if(r.Content.Headers.ContentType?.MediaType?.Contains("json")==true && text.Length>0)data=JsonDocument.Parse(text).RootElement.Clone();
        return ((int)r.StatusCode,data,text);
    }
    static void Status(int got,int expected)=>Require(got==expected,$"HTTP esperado{expected}, observado{got}");
    static int I(JsonElement j,string key)=>j.GetProperty(key).GetInt32();
    static string S(JsonElement j,string key)=>j.GetProperty(key).GetString()!;
    static decimal D(JsonElement j,string key)=>j.GetProperty(key).GetDecimal();
    static async Task Token(Session s) {var r=await Send(s,HttpMethod.Get,"/api/v1/antiforgery",token:false);Status(r.code,200);s.Token=S(r.data,"token");}
    static async Task Login(Session s,string email,string secret)
    {await Token(s);var r=await Send(s,HttpMethod.Post,"/api/v1/sessao",new {email,senha=secret});Status(r.code,200);await Token(s);}
    async Task<(int id,string email,string secret)> HttpUser(Session adm,string type)
    {
        var email=tag+"-"+Guid.NewGuid().ToString("N")[..8]+"@tests.pokevault.local";var secret=Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24));
        var r=await Send(adm,HttpMethod.Post,"/api/v1/admin/"+type,new{nome=tag,email,senhaInicial=secret});Status(r.code,201);return (I(r.data,"usuarioId"),email,secret);
    }
    static async Task<JsonElement> Get(Session s,string path) {var r=await Send(s,HttpMethod.Get,path);Status(r.code,200);return r.data;}
    static async Task<JsonElement> SetBalance(Session adm,int id,decimal value,string version)
    {var r=await Send(adm,HttpMethod.Put,$"/api/v1/admin/clientes/{id}/saldo",new{novoSaldo=value,motivo="Testes Sprint2",versaoEsperada=version});Status(r.code,200);return r.data;}
    static object Purchase(Guid uuid,params (int id,decimal preco)[] items)=>new{chaveOperacao=uuid,itens=items.Select(i=>new{ofertaId=i.id,quantidade=1,precoConfirmado=i.preco}).ToArray()};
    static async Task HCart(Session s,int card,int qty=1) {var r=await Send(s,HttpMethod.Put,$"/api/v1/carrinho/itens/{card}",new{quantidade=qty});Status(r.code,204);}
    private partial async Task HttpTests()
    {
        if(Environment.GetEnvironmentVariable("PV_TEST_HTTP")!="1") {results.Add(new{id="HTTP-INFRA",status="bloqueado",observado="Requer processo5188 isolado para PokeVaultTests."});return;}
        using var adm=new Session();using var anon=new Session();using var cli=new Session();using var att=new Session();
        await Login(adm,adminEmail,adminSecret); // Conta exclusiva: falha se servidor estiver na base demo.
        (int id,string email,string secret) user=(0,"",""),staff=(0,"","");
        await Case("HTTP-01/T01,T09","Catalogo publico200; protegido401; ADM login valido",async()=>{
            var cat=await Send(anon,HttpMethod.Get,"/api/v1/ofertas");Status(cat.code,200);var p=await Send(anon,HttpMethod.Get,"/api/v1/admin/clientes");Status(p.code,401);return new{catalogo=cat.code,protegido=p.code};
        });
        await Case("HTTP-02/T03,T05","ADM cria cliente saldo0/carrinho vazio e atendente; listagens sem segredo",async()=>{
            user=await HttpUser(adm,"clientes");staff=await HttpUser(adm,"funcionarios");await Login(cli,user.email,user.secret);await Login(att,staff.email,staff.secret);
            var cliente=await Get(adm,$"/api/v1/admin/clientes/{user.id}");Require(D(cliente,"saldo")==0,"Cliente nao iniciou zero");var cart=await Get(cli,"/api/v1/carrinho");Require(cart.GetProperty("itens").GetArrayLength()==0,"Carrinho nao vazio");
            foreach(var path in new[]{"/api/v1/admin/clientes","/api/v1/admin/funcionarios"}){var r=await Send(adm,HttpMethod.Get,path);Status(r.code,200);Require(!r.text.Contains("senha",StringComparison.OrdinalIgnoreCase)&&!r.text.Contains("hash",StringComparison.OrdinalIgnoreCase)&&!r.text.Contains(user.secret),"Listagem expoe segredo");}
            await using var c=await Open();var hash=Convert.ToString(await Scalar(c,"SELECT SenhaHash FROM dbo.Usuario WHERE UsuarioId=@u",P("@u",user.id)))!;Require(hash!=user.secret && new PasswordHasher<object>().VerifyHashedPassword(new(),hash,user.secret)!=PasswordVerificationResult.Failed,"Hash invalido");
            return new{cliente=user.id,atendente=staff.id,saldoInicial=0,carrinhoInicial=0,listagemSemSegredo=true,hashValidadoSemExposicao=true};
        });
        if(user.id==0 || staff.id==0){results.Add(new{id="HTTP-DEPENDENTES",status="bloqueado",observado="Provisionamento HTTP-02 nao concluiu"});return;}
        await Case("HTTP-03/T06","Saldo substitui0→100→40; repetir40 sem movimento/UPDATE",async()=>{
            var before=await Get(adm,$"/api/v1/admin/clientes/{user.id}");var hundred=await SetBalance(adm,user.id,100,S(before,"versao"));var forty=await SetBalance(adm,user.id,40,S(hundred,"versao"));
            await using var c=await Open();var n=await Count(c,"MovimentacaoSaldo","UsuarioId=@u",P("@u",user.id));var repeat=await SetBalance(adm,user.id,40,S(forty,"versao"));
            Require(D(repeat,"saldo")==40 && S(repeat,"versao")==S(forty,"versao") && n==await Count(c,"MovimentacaoSaldo","UsuarioId=@u",P("@u",user.id)),"Saldo/no-op diverge");return new{user.id,saldo=40,movimentos=n,semDuplicacao=true};
        });
        await Case("HTTP-04/T04","Desativacao invalida cookie; reativacao exige novo login",async()=>{
            Status((await Send(adm,HttpMethod.Patch,$"/api/v1/admin/funcionarios/{staff.id}",new{ativo=false})).code,200);
            Status((await Send(att,HttpMethod.Get,"/api/v1/gestao/ofertas")).code,401);
            Status((await Send(adm,HttpMethod.Patch,$"/api/v1/admin/funcionarios/{staff.id}",new{ativo=true})).code,200);
            Status((await Send(att,HttpMethod.Get,"/api/v1/gestao/ofertas")).code,401);await Login(att,staff.email,staff.secret);return new{staff.id,inativo=401,cookieAnterior=401,novoLogin=200};
        });
        object? cardBody=null;
        await Case("HTTP-05/T36","Atendente cadastra/ajusta preco/estoque; duplicidade409",async()=>{
            cardBody=new{colecaoId=collection,numero=Guid.NewGuid().ToString("N")[..16],nome=tag,raridade="Teste",conservacao="NEAR_MINT",preco=30m,quantidadeInicial=2};
            var r=await Send(att,HttpMethod.Post,"/api/v1/gestao/ofertas",cardBody);Status(r.code,201);var id=I(r.data,"ofertaId");var row=await Get(att,$"/api/v1/gestao/ofertas/{id}");
            r=await Send(att,HttpMethod.Patch,$"/api/v1/gestao/ofertas/{id}",new{preco=35m,novoEstoque=4,ativo=true,motivo=tag,versaoEsperada=S(row,"versao")});Status(r.code,200);Require(D(r.data,"preco")==35 && I(r.data,"estoque")==4,"Ajuste nao aplicado");
            Status((await Send(att,HttpMethod.Post,"/api/v1/gestao/ofertas",cardBody)).code,409);return new{id,preco=35,estoque=4,duplicidade=409};
        });
        await Case("HTTP-06/T02,T36","Cliente/atendente negados em saldo,usuarios,estorno; cliente nao cadastra carta",async()=>{
            foreach(var who in new[]{cli,att}){
                foreach(var path in new[]{"/api/v1/admin/clientes","/api/v1/admin/funcionarios"})Status((await Send(who,HttpMethod.Get,path)).code,403);
                Status((await Send(who,HttpMethod.Put,$"/api/v1/admin/clientes/{user.id}/saldo",new{novoSaldo=999m,motivo=tag,versaoEsperada="AAAAAAAAAAA="})).code,403);
                Status((await Send(who,HttpMethod.Post,"/api/v1/admin/funcionarios",new{nome=tag,email="x@tests.local",senhaInicial="nao-usada-aqui"})).code,403);
                Status((await Send(who,HttpMethod.Post,"/api/v1/compras/2147483647/cancelamento",new{motivo=tag})).code,403);
            }
            Status((await Send(cli,HttpMethod.Post,"/api/v1/gestao/ofertas",cardBody)).code,403);Status((await Send(anon,HttpMethod.Get,"/api/v1/gestao/ofertas")).code,401);
            await using var c=await Open();Require(await Saldo(c,user.id)==40,"Negacao alterou saldo");return new{negacoes="401/403",saldo=40};
        });
        await Case("HTTP-07/T26","Mutacoes sem CSRF ou token de outra identidade recusadas400",async()=>{
            var row=await Get(adm,$"/api/v1/admin/clientes/{user.id}");var body=new{novoSaldo=99m,motivo=tag,versaoEsperada=S(row,"versao")};
            Status((await Send(adm,HttpMethod.Put,$"/api/v1/admin/clientes/{user.id}/saldo",body,false)).code,400);
            var saved=adm.Token;try{adm.Token=att.Token;Status((await Send(adm,HttpMethod.Put,$"/api/v1/admin/clientes/{user.id}/saldo",body)).code,400);}finally{adm.Token=saved;}
            Status((await Send(adm,HttpMethod.Post,"/api/v1/admin/clientes",new{nome=tag,email="csrf@tests.local",senhaInicial="invalida-csrf"},false)).code,400);
            Status((await Send(adm,HttpMethod.Patch,$"/api/v1/admin/funcionarios/{staff.id}",new{ativo=false},false)).code,400);
            Status((await Send(att,HttpMethod.Post,"/api/v1/gestao/ofertas",cardBody,false)).code,400);
            await using var c=await Open();Require(await Saldo(c,user.id)==40,"CSRF recusado alterou saldo");return new{recusas=5,saldo=40};
        });
        await Case("HTTP-08/T07","Saldo negativo,escala3,overflow,versao invalida e motivo vazio recusados400",async()=>{
            var row=await Get(adm,$"/api/v1/admin/clientes/{user.id}");var v=S(row,"versao");
            foreach(var val in new[]{-1m,1.001m,10000000000m})Status((await Send(adm,HttpMethod.Put,$"/api/v1/admin/clientes/{user.id}/saldo",new{novoSaldo=val,motivo=tag,versaoEsperada=v})).code,400);
            foreach(var token in new[]{"","!invalida!","AQ=="})Status((await Send(adm,HttpMethod.Put,$"/api/v1/admin/clientes/{user.id}/saldo",new{novoSaldo=99m,motivo=tag,versaoEsperada=token})).code,400);
            Status((await Send(adm,HttpMethod.Put,$"/api/v1/admin/clientes/{user.id}/saldo",new{novoSaldo=99m,motivo=" ",versaoEsperada=v})).code,400);
            await using var c=await Open();Require(await Saldo(c,user.id)==40,"Entrada invalida alterou saldo");return new{recusas=7,saldo=40};
        });
        await Case("HTTP-09/T05","Email normalizado duplicado409; cliente/admin nao sao alvo de desativacao",async()=>{
            Status((await Send(adm,HttpMethod.Post,"/api/v1/admin/clientes",new{nome=tag,email=user.email.ToUpperInvariant(),senhaInicial="qualquer-inicial"})).code,409);
            Status((await Send(adm,HttpMethod.Patch,$"/api/v1/admin/funcionarios/{user.id}",new{ativo=false})).code,403);Status((await Send(adm,HttpMethod.Patch,$"/api/v1/admin/funcionarios/{admin}",new{ativo=false})).code,403);return new{duplicidade=409,alvosProtegidos=new[]{user.id,admin}};
        });
        await Case("HTTP-10/T11,T17,T24,T28","Compra selecionada50/remanescente,UUID,isolamento e estorno unico",async()=>{
            var a=await Card();var b=await Card(20,3,"SLIGHTLY_PLAYED");var rem=await Card(15.5m,1);var row=await Get(adm,$"/api/v1/admin/clientes/{user.id}");await SetBalance(adm,user.id,100,S(row,"versao"));foreach(var id in new[]{a,b,rem})await HCart(cli,id);
            var key=Guid.NewGuid();var bought=await Send(cli,HttpMethod.Post,"/api/v1/compras",Purchase(key,(a,30m),(b,20m)));Status(bought.code,200);var idBuy=I(bought.data,"compraId");Require(D(bought.data,"total")==50 && D(bought.data,"saldoAtual")==50,"Compra valores errados");
            var cart=await Get(cli,"/api/v1/carrinho");Require(cart.GetProperty("itens").GetArrayLength()==1 && I(cart.GetProperty("itens")[0],"ofertaId")==rem,"Remanescente perdido");
            var again=await Send(cli,HttpMethod.Post,"/api/v1/compras",Purchase(key,(b,20m),(a,30m)));Status(again.code,200);Require(I(again.data,"compraId")==idBuy && again.data.GetProperty("repetida").GetBoolean(),"Replay diferente");
            var other=await HttpUser(adm,"clientes");using var otherSession=new Session();await Login(otherSession,other.email,other.secret);Status((await Send(otherSession,HttpMethod.Get,$"/api/v1/compras/{idBuy}")).code,404);
            Status((await Send(adm,HttpMethod.Post,$"/api/v1/compras/{idBuy}/cancelamento",new{motivo=tag})).code,200);var twice=await Send(adm,HttpMethod.Post,$"/api/v1/compras/{idBuy}/cancelamento",new{motivo="segunda tentativa"});Status(twice.code,200);Require(D(twice.data,"saldoAtual")==100 && twice.data.GetProperty("repetida").GetBoolean(),"Estorno duplicado");return new{cliente=user.id,a,b,rem,key,compra=idBuy,total=50,saldoFinal=100};
        });
        await Case("HTTP-11/T27","Preco alterado409 sem efeito; novo aceite cobra40",async()=>{
            var a=await Card();await HCart(cli,a);await using var c=await Open();await Adjust(c,a,40,2);var r=await Send(cli,HttpMethod.Post,"/api/v1/compras",Purchase(Guid.NewGuid(),(a,30m)));Status(r.code,409);Require(S(r.data,"codigo")=="PRECO_ALTERADO","Codigo preco errado");Require(await Saldo(c,user.id)==100 && await Stock(c,a)==2,"Conflito teve efeito");var next=await Send(cli,HttpMethod.Post,"/api/v1/compras",Purchase(Guid.NewGuid(),(a,40m)));Status(next.code,200);Require(D(next.data,"total")==40,"Novo preco errado");return new{a,erro="PRECO_ALTERADO",totalConfirmado=40};
        });
        await Case("HTTP-12/S2-edicao-desatualizada","Venda invalida saldo/estoque;API409 preserva venda",async()=>{
            var a=await Card();var oldUser=await Get(adm,$"/api/v1/admin/clientes/{user.id}");var oldCard=await Get(att,$"/api/v1/gestao/ofertas/{a}");await HCart(cli,a);Status((await Send(cli,HttpMethod.Post,"/api/v1/compras",Purchase(Guid.NewGuid(),(a,30m)))).code,200);
            var rs=await Send(adm,HttpMethod.Put,$"/api/v1/admin/clientes/{user.id}/saldo",new{novoSaldo=100m,motivo=tag,versaoEsperada=S(oldUser,"versao")});Status(rs.code,409);var rc=await Send(att,HttpMethod.Patch,$"/api/v1/gestao/ofertas/{a}",new{preco=30m,novoEstoque=2,ativo=true,motivo=tag,versaoEsperada=S(oldCard,"versao")});Status(rc.code,409);Require(S(rs.data,"codigo")=="VERSAO_DESATUALIZADA" && S(rc.data,"codigo")=="VERSAO_DESATUALIZADA","Conflito errado");await using var c=await Open();Require(await Saldo(c,user.id)==30 && await Stock(c,a)==1,"Sobrescrita de venda");return new{a,saldo=30,estoque=1,conflitos=2};
        });
        await Mvc(adm,user.id);
        await Additional(adm,att,cli,user.id);
        await HttpAudit(adm,att,staff.id);
        await Case("HTTP-13/T01","Logout invalida sessao; senha errada401",async()=>{
            Status((await Send(att,HttpMethod.Delete,"/api/v1/sessao")).code,204);Status((await Send(att,HttpMethod.Get,"/api/v1/gestao/ofertas")).code,401);using var wrong=new Session();await Token(wrong);Status((await Send(wrong,HttpMethod.Post,"/api/v1/sessao",new{email=staff.email,senha="incorreta"})).code,401);return new{logout=204,aposLogout=401,senhaErrada=401};
        });
    }
    async Task Mvc(Session adm,int user)
    {
        await Case("MVC-01/S2-telas","Telas administrativas200 sem hash; rotas antigas redirecionam à gestão única",async()=>{
            var paths=new[]{"/admin/clientes","/admin/funcionarios","/admin/clientes/novo","/admin/funcionarios/novo",$"/admin/clientes/{user}/saldo","/gestao/tcgdex151"};foreach(var path in paths){var r=await Send(adm,HttpMethod.Get,path);Status(r.code,200);Require(!r.text.Contains("SenhaHash",StringComparison.OrdinalIgnoreCase),"MVC expoe hash");}
            foreach(var path in new[]{"/gestao/ofertas","/gestao/ofertas/nova"}){using var response=await adm.Client.GetAsync(path);Status((int)response.StatusCode,302);Require(response.Headers.Location?.ToString()=="/gestao/tcgdex151","Redirecionamento divergente");}
            return new{rotas=paths,status=200,rotasAntigas=302,limite="HTML/HTTP, sem inspecao visual"};
        });
        await Case("MVC-02/S2-saldo-conflito","Formulario desatualizado409 exige recarga sem sobrescrever saldo",async()=>{
            var before=await Get(adm,$"/api/v1/admin/clientes/{user}");var old=S(before,"versao");await SetBalance(adm,user,45,old);using var req=new HttpRequestMessage(HttpMethod.Post,$"/admin/clientes/{user}/saldo");req.Headers.Add("X-CSRF-TOKEN",adm.Token);req.Content=new FormUrlEncodedContent(new Dictionary<string,string>{{"NovoSaldo","99.00"},{"Motivo",tag},{"VersaoEsperada",old}});using var r=await adm.Client.SendAsync(req);var html=await r.Content.ReadAsStringAsync();Status((int)r.StatusCode,409);Require(html.Contains("recarreg",StringComparison.OrdinalIgnoreCase),"Conflito sem orientacao");await using var c=await Open();Require(await Saldo(c,user)==45,"MVC sobrescreveu saldo");return new{user,status=409,saldo=45,limite="HTML/HTTP"};
        });
    }
}
