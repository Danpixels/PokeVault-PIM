using System.Net;
using System.Text.Json;
using System.Security.Cryptography;
partial class Suite
{
    static async Task<(int code,string html)> Form(Session s,string path,Dictionary<string,string> fields)
    {
        using var req=new HttpRequestMessage(HttpMethod.Post,path);req.Headers.Add("X-CSRF-TOKEN",s.Token);req.Content=new FormUrlEncodedContent(fields);using var r=await s.Client.SendAsync(req);return ((int)r.StatusCode,await r.Content.ReadAsStringAsync());
    }
    async Task Additional(Session adm,Session att,Session cli,int user)
    {
        await Case("HTTP-14/S2-valores-catalogo","Escala3/preco0/estoque-negativo,fracionario/conservacao invalida recusados400 sem cadastro",async()=>{
            var numero=Guid.NewGuid().ToString("N")[..16];var cases=new[]{
                new{preco=1.001m,quantidadeInicial=1m,conservacao="NEAR_MINT"},new{preco=0m,quantidadeInicial=1m,conservacao="NEAR_MINT"},
                new{preco=30m,quantidadeInicial=-1m,conservacao="NEAR_MINT"},new{preco=30m,quantidadeInicial=1.5m,conservacao="NEAR_MINT"},new{preco=30m,quantidadeInicial=1m,conservacao="LP"}};
            foreach(var row in cases)Status((await Send(att,HttpMethod.Post,"/api/v1/gestao/ofertas",new{colecaoId=collection,numero,nome=tag,raridade="Teste",row.preco,row.quantidadeInicial,row.conservacao})).code,400);
            await using var c=await Open();Require(await Count(c,"Carta","ColecaoId=@col AND Numero=@n",P("@col",collection),P("@n",numero))==0,"Entrada invalida cadastrada");return new{recusas=cases.Length,cadastros=0};
        });
        await Case("HTTP-15/S2-ativacao-catalogo","Atendente inativa carta; publico oculta,gestao preserva; ajuste sem mudanca sem movimento/token",async()=>{
            var a=await Card();var row=await Get(att,$"/api/v1/gestao/ofertas/{a}");var r=await Send(att,HttpMethod.Patch,$"/api/v1/gestao/ofertas/{a}",new{preco=30m,novoEstoque=2,ativo=false,motivo=tag,versaoEsperada=S(row,"versao")});Status(r.code,200);var v=S(r.data,"versao");
            using var anon=new Session();var cat=await Get(anon,$"/api/v1/ofertas?nome={tag}&pagina=1");Require(!cat.GetProperty("itens").EnumerateArray().Any(x=>I(x,"ofertaId")==a),"Carta inativa no publico");
            await using var c=await Open();var n=await Count(c,"MovimentacaoEstoque","CartaId=@a",P("@a",a));var repeat=await Send(att,HttpMethod.Patch,$"/api/v1/gestao/ofertas/{a}",new{preco=30m,novoEstoque=2,ativo=false,motivo=tag,versaoEsperada=v});Status(repeat.code,200);
            Require(S(repeat.data,"versao")==v && n==await Count(c,"MovimentacaoEstoque","CartaId=@a",P("@a",a)),"No-op alterou token/movimento");return new{a,ativo=false,movimentos=n,noOpSemEfeito=true};
        });
        await Case("HTTP-16/S2-auditoria","Ajuste saldo registra autor/motivo/delta e nenhum segredo em detalhe",async()=>{
            var row=await Get(adm,$"/api/v1/admin/clientes/{user}");var before=D(row,"saldo");await SetBalance(adm,user,before+5,S(row,"versao"));await using var c=await Open();
            var n=Convert.ToInt32(await Scalar(c,"SELECT COUNT(*) FROM dbo.MovimentacaoSaldo WHERE UsuarioId=@u AND ValorDelta=5 AND ResponsavelId=@a AND Motivo=N'Testes Sprint2'",P("@u",user),P("@a",admin)));Require(n>=1,"Auditoria ausente");return new{user,autor=admin,delta=5,motivo="Testes Sprint2",movimentosCorrespondentes=n};
        });
        await Case("MVC-03/S2-cadastro-cliente","Formulario MVC cria cliente com saldo0/carrinho vazio e hash",async()=>{
            var email=tag+"-mvc@tests.pokevault.local";var secret=Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));var r=await Form(adm,"/admin/clientes/novo",new(){{"Nome",tag},{"Email",email},{"SenhaInicial",secret}});Status(r.code,302);
            await using var c=await Open();var id=Convert.ToInt32(await Scalar(c,"SELECT UsuarioId FROM dbo.Usuario WHERE EmailNormalizado=UPPER(@e)",P("@e",email)));Require(id>0 && await Saldo(c,id)==0 && await Count(c,"Carrinho","UsuarioId=@u",P("@u",id))==1,"MVC cliente incompleto");return new{id,status=r.code,saldo=0};
        });
        await Case("MVC-04/S2-cadastro-atendente","Formulario cria atendente e desativa/reativa sem exclusao",async()=>{
            var email=tag+"-mvc-att@tests.pokevault.local";var secret=Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));Status((await Form(adm,"/admin/funcionarios/novo",new(){{"Nome",tag},{"Email",email},{"SenhaInicial",secret}})).code,302);
            await using var c=await Open();var id=Convert.ToInt32(await Scalar(c,"SELECT UsuarioId FROM dbo.Usuario WHERE EmailNormalizado=UPPER(@e)",P("@e",email)));
            foreach(var active in new[]{false,true}){Status((await Form(adm,$"/admin/funcionarios/{id}/ativo",new(){{"Ativo",active.ToString()}})).code,302);Require(Convert.ToBoolean(await Scalar(c,"SELECT Ativo FROM dbo.Usuario WHERE UsuarioId=@u",P("@u",id)))==active,"Ativacao MVC falhou");}return new{id,ativoFinal=true,status=302};
        });
        await Case("MVC-05/S2-catalogo","Atendente cadastra carta MVC e ajusta decimal/preco/estoque com versao",async()=>{
            var numero=Guid.NewGuid().ToString("N")[..16];var f=new Dictionary<string,string>{{"ColecaoId",collection.ToString()},{"Numero",numero},{"Nome",tag},{"Raridade","Teste"},{"Conservacao","NEAR_MINT"},{"Preco","15.50"},{"QuantidadeInicial","1"}};
            Status((await Form(att,"/gestao/ofertas/nova",f)).code,302);await using var c=await Open();var id=Convert.ToInt32(await Scalar(c,"SELECT CartaId FROM dbo.Carta WHERE ColecaoId=@c AND Numero=@n",P("@c",collection),P("@n",numero)));var row=await Get(att,$"/api/v1/gestao/ofertas/{id}");var v=S(row,"versao");
            var updated=await Form(att,$"/gestao/ofertas/{id}/ajustar",new(){{"Preco","20.50"},{"NovoEstoque","4"},{"Ativo","true"},{"Motivo",tag},{"VersaoEsperada",v}});Status(updated.code,302);Require(await Stock(c,id)==4 && Convert.ToDecimal(await Scalar(c,"SELECT Preco FROM dbo.Carta WHERE CartaId=@a",P("@a",id)))==20.50m,"Decimal MVC divergente");
            var stale=await Form(att,$"/gestao/ofertas/{id}/ajustar",new(){{"Preco","15.50"},{"NovoEstoque","1"},{"Ativo","true"},{"Motivo",tag},{"VersaoEsperada",v}});Status(stale.code,409);Require(stale.html.Contains("recarreg",StringComparison.OrdinalIgnoreCase)&&await Stock(c,id)==4,"Conflito MVC nao preservou ajuste");return new{id,preco=20.5m,estoque=4,desatualizado=409};
        });
        await Case("MVC-06/S2-saldo","Formulario saldo absoluto funciona sem repetir credito",async()=>{
            var row=await Get(adm,$"/api/v1/admin/clientes/{user}");Status((await Form(adm,$"/admin/clientes/{user}/saldo",new(){{"NovoSaldo","60.50"},{"Motivo",tag},{"VersaoEsperada",S(row,"versao")}})).code,302);await using var c=await Open();Require(await Saldo(c,user)==60.50m,"Saldo MVC incorreto");return new{user,saldo=60.5m,status=302};
        });
        await ProfileInjection(adm);
    }
    async Task ProfileInjection(Session adm)
    {
        await Case("HTTP-18/S2-identidade-perfil","Anonimo com CSRF valido nao cria conta; campos perfil/admin extra recusados400 sem cadastro",async()=>{
            using var anon=new Session();await Token(anon);var email=tag+"-anon@tests.pokevault.local";
            var bad=await Send(anon,HttpMethod.Post,"/api/v1/admin/clientes",new{nome=tag,email,senhaInicial="nao-criada-123"});Status(bad.code,401);
            await using var c=await Open();Require(await Count(c,"Usuario","EmailNormalizado=UPPER(@e)",P("@e",email))==0,"Anonimo criou usuario");
            var secret=Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));var extraEmail=tag+"-perfil@tests.pokevault.local";var created=await Send(adm,HttpMethod.Post,"/api/v1/admin/clientes",new{nome=tag,email=extraEmail,senhaInicial=secret,perfil="ADMINISTRADOR",administradorFixo=true});Status(created.code,400);
            Require(await Count(c,"Usuario","EmailNormalizado=UPPER(@e)",P("@e",extraEmail))==0,"Campos extra criaram conta");return new{anonimo=401,camposExtras=400,cadastrosCriados=0};
        });
    }
}
