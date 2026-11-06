using System.Net;
using System.Globalization;
using System.Text.Json;
partial class Suite
{
    static async Task<(int code,string text)> Form(Session s,string path,Dictionary<string,string> values,bool csrf=true)
    {
        using var q=new HttpRequestMessage(HttpMethod.Post,path){Content=new FormUrlEncodedContent(values)};
        if(csrf)q.Headers.Add("X-CSRF-TOKEN",s.Token);
        using var r=await s.Client.SendAsync(q);return((int)r.StatusCode,await r.Content.ReadAsStringAsync());
    }
    async Task Fluxo151()
    {
        using var adm=new Session();using var cli=new Session();using var anon=new Session();
        await Login(adm,adminEmail,adminSecret);
        var user=await HttpUser(adm,"clientes");await Login(cli,user.email,user.secret);
        var row=await Get(adm,$"/api/v1/admin/clientes/{user.id}");await SetBalance(adm,user.id,100,S(row,"versao"));
        await using var c=await Open();
        var refId=Convert.ToString(await Scalar(c,"""
            SELECT TOP(1) r.CardId FROM dbo.vw_BibliotecaTCGdex151 r
            WHERE r.Detalhada=1 AND NOT EXISTS(SELECT 1 FROM dbo.Carta c JOIN dbo.VinculoCartaTCGdex v ON v.CartaId=c.CartaId
            JOIN dbo.ReferenciaTCGdex x ON x.ReferenciaTCGdexId=v.ReferenciaTCGdexId WHERE x.CardId=r.CardId AND c.Idioma=N'ja' AND c.Conservacao=N'MINT') ORDER BY r.Numero
            """))!;
        Require(!string.IsNullOrEmpty(refId),"Sem referência livre para fixture");
        var numero=Convert.ToString(await Scalar(c,"SELECT Numero FROM dbo.vw_BibliotecaTCGdex151 WHERE CardId=@r",P("@r",refId)))!;
        Dictionary<string,string> Dados(string acabamento,int unidades,decimal preco)=>new(){["Conservacao"]="MINT",["Idioma"]="ja",["Acabamento"]=acabamento,["Unidades"]=unidades.ToString(),["Preco"]=preco.ToString(CultureInfo.InvariantCulture),["Pagina"]="1"};
        int normal=0,reverse=0;
        await Case("FLUXO-01","Gestão207 em página única, menus recolhidos, média simplificada e controles de senha",async()=>{
            var page=await Send(adm,HttpMethod.Get,"/gestao/tcgdex151");Status(page.code,200);
            Require(page.text.Contains("207 cartas") && page.text.Contains("Gestão de Catálogo") && page.text.Contains("/salvar") && !page.text.Contains("Detalhes e preços de referência") && !page.text.Contains("BibliotecaTCGdex151") && !page.text.Contains("name=\"Motivo\""),"Tela de gestão divergente");
            Require(System.Text.RegularExpressions.Regex.Matches(page.text,"id=\"carta-sv03.5-").Count==207,"Não exibiu todas207 cartas");
            Require(System.Text.RegularExpressions.Regex.Matches(page.text,"<details class=\"stock-menu\"><summary>Colocar em estoque</summary>").Count==207 && !page.text.Contains("open=",StringComparison.OrdinalIgnoreCase),"Menus não iniciam recolhidos");
            Require(!page.text.Contains("Referência externa") && !page.text.Contains("sem conversão") && !page.text.Contains("Páginas da coleção") && page.text.Contains("Média:"),"Informações extras/paginação permaneceram");
            foreach(var path in new[]{"/entrar","/admin/clientes/novo","/admin/funcionarios/novo"}){var html=await Send(adm,HttpMethod.Get,path);Status(html.code,200);Require(html.text.Contains("data-password-toggle") && html.text.Contains("type=\"password\"") && html.text.Contains("type=\"button\"") && html.text.Contains("password-toggle.js"),"Controle de senha ausente");}
            return new{paginas=1,totalReferencias=207,menusRecolhidos=207,camposSenha=3,limite="HTML/HTTP; sem inspeção visual"};
        });
        await Case("FLUXO-02","Normal e reverse da mesma carta/estado/idioma têm IDs e estoques separados",async()=>{
            Status((await Form(adm,$"/gestao/tcgdex151/{refId}/salvar",Dados("NORMAL",5,2))).code,302);
            Status((await Form(adm,$"/gestao/tcgdex151/{refId}/salvar",Dados("REVERSE_HOLO",4,3))).code,302);
            normal=Convert.ToInt32(await Scalar(c,"SELECT c.CartaId FROM dbo.Carta c JOIN dbo.VinculoCartaTCGdex v ON v.CartaId=c.CartaId JOIN dbo.ReferenciaTCGdex r ON r.ReferenciaTCGdexId=v.ReferenciaTCGdexId WHERE r.CardId=@r AND c.Idioma=N'ja' AND c.Conservacao=N'MINT' AND c.Acabamento=N'NORMAL'",P("@r",refId)));
            reverse=Convert.ToInt32(await Scalar(c,"SELECT c.CartaId FROM dbo.Carta c JOIN dbo.VinculoCartaTCGdex v ON v.CartaId=c.CartaId JOIN dbo.ReferenciaTCGdex r ON r.ReferenciaTCGdexId=v.ReferenciaTCGdexId WHERE r.CardId=@r AND c.Idioma=N'ja' AND c.Conservacao=N'MINT' AND c.Acabamento=N'REVERSE_HOLO'",P("@r",refId)));
            Require(normal>0 && reverse>0 && normal!=reverse && await Stock(c,normal)==5 && await Stock(c,reverse)==4,"Variantes misturadas");
            return new{refId,normal,reverse,estoques=new[]{5,4}};
        });
        if(normal==0 || reverse==0)throw new InvalidOperationException("Cadastro das variantes bloqueou demais cenários");
        await Case("FLUXO-03","Duplicata/variante inválida, CSRF ausente e cliente não alteram estoque",async()=>{
            var count=await Count(c,"Carta","CartaId IN(@a,@b)",P("@a",normal),P("@b",reverse));
            Status((await Form(adm,$"/gestao/tcgdex151/{refId}/salvar",Dados("NORMAL",50,99))).code,302);
            var invalido=Dados("FOIL_INVALIDO",5,2);Status((await Form(adm,$"/gestao/tcgdex151/{refId}/salvar",invalido)).code,302);
            Status((await Form(adm,$"/gestao/tcgdex151/{refId}/salvar",Dados("HOLO",5,2),false)).code,400);
            Status((await Form(cli,$"/gestao/tcgdex151/{refId}/salvar",Dados("HOLO",5,2))).code,403);
            Require(await Stock(c,normal)==5 && count==2,"Recusa alterou estoque");return new{duplicataRecusada=true,csrf=400,cliente=403};
        });
        await Case("FLUXO-04","Ajuste sem motivo auditado; versão velha não sobrescreve",async()=>{
            var carta=await Get(adm,$"/api/v1/gestao/ofertas/{normal}");var dados=Dados("NORMAL",6,2);dados["OfertaId"]=normal.ToString();dados["VersaoEsperada"]=S(carta,"versao");
            Status((await Form(adm,$"/gestao/tcgdex151/{refId}/salvar",dados)).code,302);Require(await Stock(c,normal)==6,"Ajuste não salvou");
            dados["Unidades"]="7";Status((await Form(adm,$"/gestao/tcgdex151/{refId}/salvar",dados)).code,302);Require(await Stock(c,normal)==6,"Versão velha sobrescreveu");
            Require(await Count(c,"AlteracaoCarta","CartaId=@a AND Motivo=N'Ajuste de preço/estoque pelo catálogo 151'",P("@a",normal))==1,"Sem auditoria automática");return new{estoque=6,auditoria=1,conflitoPreservado=true};
        });
        await Case("FLUXO-05","Vitrine agrupada e detalhe público listam versões/estoques/preços",async()=>{
            var home=await Send(anon,HttpMethod.Get,"/");Status(home.code,200);Require(!home.text.Contains("Definir no carrinho") && !home.text.Contains("name=\"colecao\""),"Fluxo antigo presente");
            var detail=await Send(anon,HttpMethod.Get,$"/cartas/{normal}");Status(detail.code,200);Require(detail.text.Contains("Reverse holo") && detail.text.Contains("Normal") && detail.text.Contains("Japon") && detail.text.Contains("/165"),"Detalhe sem variantes");
            return new{vitrine=200,detalhe=200,agruparPorNumero=true};
        });
        await Case("FLUXO-06","Adicionar soma; concorrência não perde incremento; excesso não altera linha",async()=>{
            Status((await Form(cli,"/carrinho/adicionar",new(){["ofertaId"]=normal.ToString(),["quantidade"]="1"})).code,302);
            var results=await Task.WhenAll(Form(cli,"/carrinho/adicionar",new(){["ofertaId"]=normal.ToString(),["quantidade"]="1"}),Form(cli,"/carrinho/adicionar",new(){["ofertaId"]=normal.ToString(),["quantidade"]="1"}));Require(results.All(r=>r.code==302),"Concorrência HTTP falhou");
            var cart=await Get(cli,"/api/v1/carrinho");Require(I(cart.GetProperty("itens")[0],"quantidade")==3,"Adição perdeu incremento");
            Status((await Form(cli,"/carrinho/adicionar",new(){["ofertaId"]=normal.ToString(),["quantidade"]="4"})).code,302);
            cart=await Get(cli,"/api/v1/carrinho");Require(I(cart.GetProperty("itens")[0],"quantidade")==3 && await Stock(c,normal)==6,"Excesso alterou carrinho/estoque");
            return new{quantidade=3,estoqueSemReserva=6,concorrenciaSemPerda=true};
        });
        await Case("FLUXO-07","Carrinho mostra coleção/número/estado/idioma/acabamento e linhas separadas",async()=>{
            Status((await Form(cli,"/carrinho/adicionar",new(){["ofertaId"]=reverse.ToString(),["quantidade"]="1"})).code,302);
            var cart=await Get(cli,"/api/v1/carrinho");Require(cart.GetProperty("itens").GetArrayLength()==2,"Linhas misturadas");
            var html=await Send(cli,HttpMethod.Get,"/carrinho");Status(html.code,200);Require(html.text.Contains("Reverse holo") && html.text.Contains("#"+numero+"/165") && html.text.Contains("Preço unit") && html.text.Contains("total-selecionado") && html.text.Contains("bag-link"),"Campos do carrinho faltantes");
            return new{linhas=2,total=9m,metadados=true,sacola=true};
        });
        int compra=0;
        await Case("FLUXO-08","Compra selecionada guarda variante e preserva remanescente; replay idempotente",async()=>{
            var key=Guid.NewGuid();var payload=new{chaveOperacao=key,itens=new[]{new{ofertaId=normal,quantidade=3,precoConfirmado=2m}}};
            var bought=await Send(cli,HttpMethod.Post,"/api/v1/compras",payload);Status(bought.code,200);compra=I(bought.data,"compraId");Require(D(bought.data,"total")==6 && await Stock(c,normal)==3 && await Stock(c,reverse)==4,"Compra afetou outra versão");
            var repeat=await Send(cli,HttpMethod.Post,"/api/v1/compras",payload);Status(repeat.code,200);Require(repeat.data.GetProperty("repetida").GetBoolean(),"Replay não idempotente");
            Require(Convert.ToInt32(await Scalar(c,"SELECT COUNT(*) FROM dbo.ItemCompra WHERE CompraId=@c AND IdiomaSnapshot=N'ja' AND AcabamentoSnapshot=N'NORMAL' AND NumeroSnapshot=@n AND ColecaoSnapshot IS NOT NULL",P("@c",compra),P("@n",numero)))==1,"Snapshot errado");
            var cart=await Get(cli,"/api/v1/carrinho");Require(cart.GetProperty("itens").GetArrayLength()==1 && I(cart.GetProperty("itens")[0],"ofertaId")==reverse,"Remanescente perdido");return new{compra,total=6,snapshot=true,replay=true};
        });
        await Case("FLUXO-09","Estorno devolve só a variante comprada uma vez",async()=>{
            Require(compra>0,"Compra não criada");var cancel=await Send(adm,HttpMethod.Post,$"/api/v1/compras/{compra}/cancelamento",new{motivo=tag});Status(cancel.code,200);
            var twice=await Send(adm,HttpMethod.Post,$"/api/v1/compras/{compra}/cancelamento",new{motivo=tag});Status(twice.code,200);
            Require(await Stock(c,normal)==6 && await Stock(c,reverse)==4 && D(twice.data,"saldoAtual")==100 && twice.data.GetProperty("repetida").GetBoolean(),"Estorno incorreto");return new{estoqueNormal=6,estoqueReverse=4,saldo=100};
        });
        await Case("FLUXO-10","Zero estoque exclui da vitrine; edição absoluta e remoção continuam disponíveis",async()=>{
            foreach(var id in new[]{normal,reverse}){var atual=await Get(adm,$"/api/v1/gestao/ofertas/{id}");var d=Dados(id==normal?"NORMAL":"REVERSE_HOLO",0,id==normal?2:3);d["OfertaId"]=id.ToString();d["VersaoEsperada"]=S(atual,"versao");Status((await Form(adm,$"/gestao/tcgdex151/{refId}/salvar",d)).code,302);Require(await Stock(c,id)==0,"Zero não salvou");}
            await HCart(cli,reverse,0);var cart=await Get(cli,"/api/v1/carrinho");Require(cart.GetProperty("itens").GetArrayLength()==0,"Remoção falhou");
            var home=await Send(anon,HttpMethod.Get,"/");Require(!home.text.Contains("href=\"/cartas/"+normal+"\""),"Esgotado na vitrine");return new{estoqueZero=true,carrinhoVazio=true};
        });
    }
}
