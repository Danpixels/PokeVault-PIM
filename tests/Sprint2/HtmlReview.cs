using System.Net;
partial class Suite
{
    async Task RevisarHtml()
    {
        using var adm=new Session();using var cli=new Session();using var att=new Session();using var anon=new Session();
        await Login(adm,adminEmail,adminSecret);var u=await HttpUser(adm,"clientes");var staff=await HttpUser(adm,"funcionarios");await Login(cli,u.email,u.secret);await Login(att,staff.email,staff.secret);
        await Case("REV-H01/publico-vazio-semantica","Estado vazio e elementos semanticos/viewport presentes; CSS local200",async()=>{
            var r=await Send(anon,HttpMethod.Get,"/?nome=nao-existe-"+tag);Status(r.code,200);var html=WebUtility.HtmlDecode(r.text);
            Require(html.Contains("Nenhuma carta nesta página") && html.Contains("Altere os filtros"),"Estado vazio sem orientacao");
            foreach(var text in new[]{"lang=\"pt-BR\"","name=\"viewport\"","href=\"#conteudo\"","id=\"conteudo\"","aria-label=\"Navegação principal\""})Require(html.Contains(text),"Semantica ausente: "+text);
            var css=await Send(anon,HttpMethod.Get,"/css/site.css");Status(css.code,200);Require(css.text.Contains("@media") && css.text.Contains("focus"),"CSS sem regras responsivas/foco");
            return new{http=200,estado="vazio com orientacao",lang="pt-BR",viewport=true,linkPularConteudo=true,css=200,limite="Verificacao de markup/CSS; nao comprova layout/foco visual"};
        });
        await Case("REV-H02/login-rotulos-validacao","Login tem rotulos/senha oculta/autocomplete; erro de autenticacao renderiza alerta sem ecoar senha",async()=>{
            var r=await Send(anon,HttpMethod.Get,"/entrar");Status(r.code,200);Require(r.text.Contains("for=\"Email\"") && r.text.Contains("type=\"password\"") && r.text.Contains("autocomplete=\"current-password\""),"Rotulos/atributos ausentes");
            await Token(anon);using var req=new HttpRequestMessage(HttpMethod.Post,"/entrar");req.Headers.Add("X-CSRF-TOKEN",anon.Token);var dummy="invalida-apenas-roteiro";req.Content=new FormUrlEncodedContent(new Dictionary<string,string>{{"Email",tag+"-naoexiste@tests.pokevault.local"},{"Senha",dummy}});using var resp=await anon.Client.SendAsync(req);Status((int)resp.StatusCode,200);var html=WebUtility.HtmlDecode(await resp.Content.ReadAsStringAsync());Require(html.Contains("role=\"alert\"")&&html.Contains("inválidos")&&!html.Contains(dummy),"Erro de login nao tratado/eco de senha");return new{status=200,alerta=true,senhaEcoada=false,limite="HTML/HTTP; teclado nao executado"};
        });
        await Case("REV-H03/cliente-vazio-rotulos","Cliente novo ve carrinho/historico vazios; linha com controles nomeados",async()=>{
            var cart=await Send(cli,HttpMethod.Get,"/carrinho");var hist=await Send(cli,HttpMethod.Get,"/compras");Status(cart.code,200);Status(hist.code,200);Require(WebUtility.HtmlDecode(cart.text).Contains("Seu carrinho está vazio")&&WebUtility.HtmlDecode(hist.text).Contains("Nenhuma compra registrada"),"Vazios nao apresentados");
            var a=await Card();await HCart(cli,a);var line=await Send(cli,HttpMethod.Get,"/carrinho");Status(line.code,200);Require(line.text.Contains("aria-label=\"Selecionar ")&&line.text.Contains("aria-label=\"Quantidade de "),"Controles sem nome");return new{cliente=u.id,carta=a,vazios=2,controlesNomeados=true,limite="Nao e navegacao visual"};
        });
        await Case("REV-H04/navegacao-perfis","Links de navegacao seguem cada perfil sem expor gestao a cliente/anonimo",async()=>{
            var ad=await Send(adm,HttpMethod.Get,"/");var at=await Send(att,HttpMethod.Get,"/");var cl=await Send(cli,HttpMethod.Get,"/");var an=await Send(anon,HttpMethod.Get,"/");foreach(var r in new[]{ad,at,cl,an})Status(r.code,200);
            Require(ad.text.Contains("href=\"/admin/clientes\"")&&ad.text.Contains("href=\"/gestao/ofertas\""),"ADM sem navegacao");Require(at.text.Contains("href=\"/gestao/ofertas\"")&&!at.text.Contains("href=\"/admin/clientes\""),"Navegacao atendente incorreta");Require(cl.text.Contains("href=\"/carrinho\"")&&!cl.text.Contains("href=\"/admin/clientes\""),"Navegacao cliente incorreta");Require(!an.text.Contains("href=\"/admin/clientes\""),"Anonimo com link administrativo");return new{perfis=4,linksCoerentes=true,limite="Autorizacao efetiva tambem testada nos45 cenarios"};
        });
        await Case("REV-H05/validacao-formulario","Cadastro MVC invalido apresenta alerta sem criar usuario",async()=>{
            await using var c=await Open();var n=await Count(c,"Usuario","1=1");using var req=new HttpRequestMessage(HttpMethod.Post,"/admin/clientes/novo");req.Headers.Add("X-CSRF-TOKEN",adm.Token);req.Content=new FormUrlEncodedContent(new Dictionary<string,string>{{"Nome",""},{"Email","invalido"},{"SenhaInicial",""}});using var resp=await adm.Client.SendAsync(req);Status((int)resp.StatusCode,200);var html=await resp.Content.ReadAsStringAsync();Require(html.Contains("role=\"alert\"")&&html.Contains("validation-summary-errors")&&await Count(c,"Usuario","1=1")==n,"Formulario invalido criou usuario ou sem erro");return new{http=200,usuarioCriado=false,alerta=true};
        });
        await Case("REV-H06/filtros-paginacao","Filtros combinados e paginas reais de registros proprios sem repeticao",async()=>{
            for(int i=0;i<13;i++)await Card();var query=$"/api/v1/ofertas?nome={tag}&colecao=MEW-151&conservacao=NEAR_MINT&pagina=";var a=await Get(anon,query+"1");var b=await Get(anon,query+"2");var first=a.GetProperty("itens").EnumerateArray().ToArray();var second=b.GetProperty("itens").EnumerateArray().ToArray();Require(first.Length==12&&second.Length>=1,"Paginacao divergente");Require(!first.Select(x=>I(x,"ofertaId")).Intersect(second.Select(x=>I(x,"ofertaId"))).Any(),"Paginas repetem item");Require(first.Concat(second).All(x=>S(x,"conservacao")=="NEAR_MINT"&&S(x,"nome")==tag),"Filtro divergente");return new{filtro=tag,pagina1=first.Length,pagina2=second.Length,semRepeticao=true};
        });
    }
}
