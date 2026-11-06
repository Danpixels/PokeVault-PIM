using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net;
partial class Suite
{
    static readonly string[] Rares=["Common","Uncommon","Rare","Double Rare","Ace Spec Rare","Illustration Rare","Ultra Rare","Special Illustration Rare","Hyper Rare","Shiny Rare","Shiny Ultra Rare","Promo"];
    private partial async Task Complemento()
    {
        await SqlWeb();
        using var adm=new Session();using var att=new Session();using var cli=new Session();using var anon=new Session();
        await Login(adm,adminEmail,adminSecret);var su=await HttpUser(adm,"funcionarios");var cu=await HttpUser(adm,"clientes");await Login(att,su.email,su.secret);await Login(cli,cu.email,cu.secret);
        await Case("WEB-H01/colecao","ADM/atendente criam colecao; homonimas permitidas com ID/codigo proprios",async()=>{
            var r=await Send(adm,HttpMethod.Post,"/api/v1/gestao/colecoes",new{nome=tag,quantidadeTotalCartas=2});Status(r.code,201);var a=I(r.data,"colecaoId");var code=S(r.data,"codigo");Require(code.StartsWith("COL-") && code.Length<=30,"Codigo tecnico invalido");
            var two=await Send(att,HttpMethod.Post,"/api/v1/gestao/colecoes",new{nome=tag,quantidadeTotalCartas=2});Status(two.code,201);Require(I(two.data,"colecaoId")!=a && S(two.data,"codigo")!=code,"Codigo/ID colidiu");
            var card=await Send(att,HttpMethod.Post,"/api/v1/gestao/ofertas",new{colecaoId=a,numero="999",nome=tag,raridade="Common",conservacao="NEAR_MINT",preco=30m,quantidadeInicial=4});Status(card.code,201);await using var c=await Open();var id=I(card.data,"ofertaId");Require(await Stock(c,id)==4,"Total colecao interferiu no estoque");return new{colecao=a,codigo=code,carta=id,numero="999",total=2,estoque=4,homonima=I(two.data,"colecaoId")};
        });
        await Case("WEB-H02/colecao-invalidos","Nome/total invalidos rejeitados400; cliente403,anonimo401 e CSRF400",async()=>{
            foreach(var v in new[]{0m,-1m,1.5m,2147483648m})Status((await Send(att,HttpMethod.Post,"/api/v1/gestao/colecoes",new{nome=tag,quantidadeTotalCartas=v})).code,400);
            foreach(var n in new[]{" ",new string('x',121)})Status((await Send(att,HttpMethod.Post,"/api/v1/gestao/colecoes",new{nome=n,quantidadeTotalCartas=1})).code,400);
            var body=new{nome=tag,quantidadeTotalCartas=1};Status((await Send(cli,HttpMethod.Post,"/api/v1/gestao/colecoes",body)).code,403);await Token(anon);Status((await Send(anon,HttpMethod.Post,"/api/v1/gestao/colecoes",body)).code,401);Status((await Send(att,HttpMethod.Post,"/api/v1/gestao/colecoes",body,false)).code,400);return new{invalidos=6,cliente=403,anonimo=401,csrf=400};
        });
        await Case("WEB-H03/raridades-12","API aceita12 raridades exatas; catalogo apresenta nome e SVG local ingles",async()=>{
            var ids=new List<int>();foreach(var rarity in Rares){var r=await Send(att,HttpMethod.Post,"/api/v1/gestao/ofertas",new{colecaoId=collection,numero=Guid.NewGuid().ToString("N")[..16],nome=tag+"-rarities",raridade=rarity,conservacao="NEAR_MINT",preco=1m,quantidadeInicial=1});Status(r.code,201);ids.Add(I(r.data,"ofertaId"));}
            var page=await Send(anon,HttpMethod.Get,"/?nome="+tag+"-rarities");Status(page.code,200);var html=WebUtility.HtmlDecode(page.text);foreach(var name in Rares)Require(html.Contains(name),"Nome de raridade ausente: "+name);
            var symbols=Regex.Matches(html,"<use href=\"/images/rarities.svg#[^\"]+\"").Count;Require(symbols==12,"Quantidade de simbolos diverge");var svg=await Send(anon,HttpMethod.Get,"/images/rarities.svg");Status(svg.code,200);Require(!svg.text.Contains("http://") || svg.text.Contains("http://www.w3.org/2000/svg"),"SVG externo inesperado");return new{ids,raridades=Rares,simbolosLocais=symbols,limite="Nomes em HTML; verificacao visual separada"};
        });
        await Case("WEB-H04/raridade-invalidos","API rejeita raridade fora da lista400 sem criar carta",async()=>{
            foreach(var rarity in new[]{"Secret Rare","Teste","C","common","Commom"}){var r=await Send(att,HttpMethod.Post,"/api/v1/gestao/ofertas",new{colecaoId=collection,numero=Guid.NewGuid().ToString("N")[..16],nome=tag+"-invalid",raridade=rarity,conservacao="NEAR_MINT",preco=1m,quantidadeInicial=1});Status(r.code,400);}await using var c=await Open();Require(await Count(c,"Carta","Nome=@n",P("@n",tag+"-invalid"))==0,"Raridade invalida persistiu");return new{recusas=5,cartasCriadas=0};
        });
        await Case("WEB-H05/aditivo-absoluto","POST1+2=3; PUT2 substitui; DELETE remove; sem reserva/debito",async()=>{
            var a=await Card(30,0);await HAdd(cli,a,1);await HAdd(cli,a,2);var qty=await HQty(cli,a);Require(qty==3,"POST nao somou");await HCart(cli,a,2);Require(await HQty(cli,a)==2,"PUT nao substituiu");Status((await Send(cli,HttpMethod.Delete,$"/api/v1/carrinho/itens/{a}")).code,204);Require(await HQty(cli,a)==0,"DELETE nao removeu");await using var c=await Open();Require(await Saldo(c,cu.id)==0 && await Stock(c,a)==0,"Carrinho reservou/debitou");return new{carta=a,aposSoma=qty,aposPut=2,aposDelete=0,saldo=0,estoque=0};
        });
        await Case("WEB-H06/aditivo-concorrente","Duas inclusoes HTTP concorrentes somam3 sem lost update",async()=>{
            var a=await Card();var start=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);async Task Req(int qty){await start.Task;await HAdd(cli,a,qty);}var ta=Req(1);var tb=Req(2);start.SetResult();await Task.WhenAll(ta,tb);var qty=await HQty(cli,a);Require(qty==3,"Inclusao HTTP perdida");return new{a,quantidade=qty,requisicoes=2};
        });
        await Case("WEB-H07/aditivo-limites","Teto10000; excesso409; negativos/zero/fracao/intoverflow400 sem efeito",async()=>{
            var a=await Card();await HCart(cli,a,9999);await HAdd(cli,a,1);var r=await Send(cli,HttpMethod.Post,$"/api/v1/carrinho/itens/{a}/adicionar",new{quantidade=1});Status(r.code,409);Require(S(r.data,"codigo")=="QUANTIDADE_CARRINHO_EXCEDIDA","Codigo excesso incorreto");
            foreach(var value in new[]{0m,-1m,1.5m,2147483648m})Status((await Send(cli,HttpMethod.Post,$"/api/v1/carrinho/itens/{a}/adicionar",new{quantidade=value})).code,400);
            Require(await HQty(cli,a)==10000,"Quantidade mudou na recusa");return new{a,limite=10000,excesso=409,invalidos=4};
        });
        await Case("WEB-H08/aditivo-autorizacao","POST aditivo exige cliente e CSRF; recusas nao criam linha",async()=>{
            var a=await Card();var path=$"/api/v1/carrinho/itens/{a}/adicionar";var b=new{quantidade=1};Status((await Send(cli,HttpMethod.Post,path,b,false)).code,400);Status((await Send(att,HttpMethod.Post,path,b)).code,403);Status((await Send(adm,HttpMethod.Post,path,b)).code,403);Status((await Send(anon,HttpMethod.Post,path,b)).code,401);Require(await HQty(cli,a)==0,"Negacao criou linha");return new{a,csrf=400,perfis=403,anonimo=401};
        });
        await Case("WEB-H09/senha-markup","Login/cadastros tem senha oculta e toggle button/aria sem credencial exposta",async()=>{
            foreach(var path in new[]{"/entrar","/admin/clientes/novo","/admin/funcionarios/novo"}){var r=await Send(adm,HttpMethod.Get,path);Status(r.code,200);Require(Regex.IsMatch(r.text,"<input[^>]+type=\"password\""),"Campo nao oculto");Require(r.text.Contains("data-password-toggle") && r.text.Contains("type=\"button\"") && r.text.Contains("aria-pressed=\"false\"") && r.text.Contains("Mostrar senha"),"Toggle/aria ausente");}return new{paginas=3,ocultoInicial=true,botao="type=button",ariaPressed=false,limite="Markup; interacao real de login separada"};
        });
        await Case("WEB-M01/colecao-selecionada","POST formulario colecao redireciona carta com nova colecao selecionada",async()=>{
            using var req=new HttpRequestMessage(HttpMethod.Post,"/gestao/colecoes/nova");req.Headers.Add("X-CSRF-TOKEN",att.Token);req.Content=new FormUrlEncodedContent(new Dictionary<string,string>{{"Nome",tag+"-mvc"},{"QuantidadeTotalCartas","1"}});using var response=await att.Client.SendAsync(req);Status((int)response.StatusCode,302);var loc=response.Headers.Location!.ToString();Require(loc.Contains("colecaoId="),"Redirecionamento sem colecao");var r=await Send(att,HttpMethod.Get,loc);Status(r.code,200);var id=Regex.Match(loc,"colecaoId=(\\d+)").Groups[1].Value;Require(Regex.IsMatch(r.text,"<option[^>]*(?:selected=\"selected\"[^>]*value=\""+id+"\"|value=\""+id+"\"[^>]*selected=\"selected\")"),"Nova colecao nao selecionada");
            var rarities=Rares.Count(x=>r.text.Contains(WebUtility.HtmlEncode(x)));Require(rarities==12 && !r.text.Contains("Secret Rare"),"Select nao tem taxonomia pedida");return new{location=loc,colecaoSelecionada=id,raridades=rarities};
        });
        await Case("WEB-M02/legado-gestao","Raridade desconhecida permanece rotulada legada na gestao",async()=>{
            await using var c=await Open();var a=Convert.ToInt32(await Scalar(c,"SELECT TOP(1) CartaId FROM dbo.Carta WHERE Raridade=N'Teste' ORDER BY CartaId"));var r=await Send(att,HttpMethod.Get,$"/gestao/ofertas/{a}/ajustar");Status(r.code,200);Require(WebUtility.HtmlDecode(r.text).Contains("raridade legada"),"Legado sem identificacao");return new{cartaLegada=a,raridade="Teste",rotuloLegado=true};
        });
    }
    static async Task HAdd(Session s,int id,int qty){Status((await Send(s,HttpMethod.Post,$"/api/v1/carrinho/itens/{id}/adicionar",new{quantidade=qty})).code,204);}
    static async Task<int> HQty(Session s,int id){var c=await Get(s,"/api/v1/carrinho");var line=c.GetProperty("itens").EnumerateArray().FirstOrDefault(x=>I(x,"ofertaId")==id);return line.ValueKind==JsonValueKind.Undefined?0:I(line,"quantidade");}
    async Task SqlWeb()
    {
        await Case("WEB-S01/colecao-sql","Colecao gerada e totalNULL legado; total1 nao limita numero999/estoque4",async()=>{
            await using var c=await Open();var r=await Proc(c,"dbo.usp_CadastrarColecao",P("@ResponsavelId",admin),P("@Nome","  "+tag+"  "),P("@QuantidadeTotalCartas",1));var id=Convert.ToInt32(r["ColecaoId"]);Require((string)r["Nome"]! ==tag,"Nome nao saneado");Require((await Scalar(c,"SELECT QuantidadeTotalCartas FROM dbo.Colecao WHERE ColecaoId=@id",P("@id",collection))) is DBNull,"Total legado inventado");
            var row=await Proc(c,"dbo.usp_CadastrarCarta",P("@ResponsavelId",admin),P("@ColecaoId",id),P("@Numero","999"),P("@Nome",tag),P("@Raridade","Common"),P("@Conservacao","NEAR_MINT"),Money("@Preco",30),P("@QuantidadeInicial",4));return new{colecao=id,nome=r["Nome"],codigo=r["Codigo"],total=1,carta=row["CartaId"],numero="999",estoque=4,totalLegado="NULL"};
        });
        await Case("WEB-S02/raridade-sql","SQL rejeita valores nao canonicos51014 sem cadastrar",async()=>{
            await using var c=await Open();foreach(var rare in new[]{"Secret Rare","Teste","C","common","Commom"})await Error(51014,async()=>{await Proc(c,"dbo.usp_CadastrarCarta",P("@ResponsavelId",admin),P("@ColecaoId",collection),P("@Numero",Guid.NewGuid().ToString("N")[..16]),P("@Nome",tag),P("@Raridade",rare),P("@Conservacao","NEAR_MINT"),Money("@Preco",30),P("@QuantidadeInicial",1));});return new{recusas=5,erro=51014};
        });
        await Case("WEB-S03/aditivo-sql","SQL1+2=3,absoluto2,remocao0 sem debito/baixa",async()=>{
            var u=await User();var a=await Card(30,0);await using var c=await Open();await AddSql(c,u,a,1);await AddSql(c,u,a,2);Require(await Qty(c,u,a)==3,"SQL nao somou");await Cart(c,u,a,2);Require(await Qty(c,u,a)==2,"SQL absoluto somou");await Cart(c,u,a,0);Require(await Qty(c,u,a)==0 && await Saldo(c,u)==100 && await Stock(c,a)==0,"Carrinho efeito indevido");return new{u,a,sequencia=new[]{3,2,0},saldo=100,estoque=0};
        });
        await Case("WEB-S04/aditivo-concorrente","10 disputas em duas conexoes,linha ausente/existente sem lostupdate",async()=>{
            var u=await User();var a=await Card();await using var c=await Open();for(var k=0;k<10;k++){await Cart(c,u,a,k%2);var codes=await Pair(async x=>{await AddSql(x,u,a,1);},async x=>{await AddSql(x,u,a,2);});Require(codes.All(x=>x==0)&&await Qty(c,u,a)==3+k%2,"Lost update aditivo");}return new{u,a,disputas=10,conexoesPorDisputa=2,ultimaQuantidade=4};
        });
        await Case("WEB-S05/aditivo-limites","SQL teto10000/excesso51013 e entradasinvalidas51002 preservamlinha",async()=>{
            var u=await User();var a=await Card();await using var c=await Open();await AddSql(c,u,a,9999);await AddSql(c,u,a,1);await Error(51013,async()=>{await AddSql(c,u,a,1);});foreach(var q in new[]{0,-1,10001,int.MaxValue})await Error(51002,async()=>{await AddSql(c,u,a,q);});await Error(51002,async()=>{await Cart(c,u,a,10001);});Require(await Qty(c,u,a)==10000,"Limite alteroulinha");return new{u,a,quantidade=10000,excesso=51013,entradaInvalida=51002};
        });
        await Case("WEB-S06/preservacao-legados","112 raridades desconhecidas do baseline preservadas por ID/valor",async()=>{
            using var doc=JsonDocument.Parse(await File.ReadAllTextAsync("docs/evidencias/complemento-web/baseline-legados.json"));var chunks=doc.RootElement.GetProperty("saida").EnumerateArray().Select(x=>x.GetString()).Skip(2);using var rows=JsonDocument.Parse(string.Concat(chunks));await using var c=await Open();int n=0;foreach(var row in rows.RootElement.EnumerateArray()){var id=I(row,"CartaId");var old=S(row,"Raridade");Require(Convert.ToString(await Scalar(c,"SELECT Raridade FROM dbo.Carta WHERE CartaId=@id",P("@id",id)))==old,"Legado reescrito: "+id);n++;}return new{legadosComparados=n,preservados=true};
        });
    }
    static async Task AddSql(Microsoft.Data.SqlClient.SqlConnection c,int user,int card,int qty)=>_=await Proc(c,"dbo.usp_AdicionarItemCarrinho",P("@ClienteId",user),P("@CartaId",card),P("@QuantidadeAdicionar",qty));
    static async Task<int> Qty(Microsoft.Data.SqlClient.SqlConnection c,int user,int card)=>Convert.ToInt32(await Scalar(c,"SELECT COALESCE((SELECT i.Quantidade FROM dbo.ItemCarrinho i JOIN dbo.Carrinho c ON c.CarrinhoId=i.CarrinhoId WHERE c.UsuarioId=@u AND i.CartaId=@a),0)",P("@u",user),P("@a",card)));
}

