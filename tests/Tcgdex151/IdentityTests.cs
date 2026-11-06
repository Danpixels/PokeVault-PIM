using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Net;
using PokeVault.Testes.Tcgdex;
partial class TcgSuite
{
    async Task IdentityTests()
    {
        using var adm=new Session();await Login(adm,adminEmail,adminSecret);
        await Case("DB-04/identidade-remota-divergente","Nome remoto divergente em referencia ja vinculada recusa51022 sem mudar comercio/vinculo",async()=>{
            await using var c=await Open();var id=Convert.ToInt32(await Scalar(c,"SELECT ReferenciaTCGdexId FROM dbo.vw_BibliotecaTCGdex151 WHERE CardId=N'sv03.5-025'"));var raw=Convert.ToString(await Scalar(c,"SELECT SnapshotJson FROM dbo.ReferenciaTCGdex WHERE ReferenciaTCGdexId=@r",P("@r",id)))!;var lang=Convert.ToString(await Scalar(c,"SELECT Idioma FROM dbo.ReferenciaTCGdex WHERE ReferenciaTCGdexId=@r",P("@r",id)))!;var set=Convert.ToString(await Scalar(c,"SELECT SnapshotJson FROM dbo.ColecaoReferenciaTCGdex WHERE Idioma=@l",P("@l",lang)))!;var log=Convert.ToInt32((await Begin(c,set,lang,Guid.NewGuid()))["ImportacaoId"]);var card=Convert.ToInt32(await Scalar(c,"SELECT TOP(1) CartaId FROM dbo.VinculoCartaTCGdex WHERE ReferenciaTCGdexId=@r ORDER BY CartaId",P("@r",id)));var before=await BusinessSnapshot.Capture(c);var oldLink=Convert.ToString(await Scalar(c,"SELECT(SELECT * FROM dbo.VinculoCartaTCGdex WHERE CartaId=@c FOR JSON PATH)",P("@c",card)))!;
            try{var altered=JsonNode.Parse(raw)!;altered["name"]="Pikachu identidade divergente de teste";await Update(c,log,altered);await Error(51022,async()=>{await Proc(c,"dbo.usp_VincularOfertaTCGdex151",P("@ResponsavelId",admin),P("@CartaId",card),P("@ReferenciaTCGdexId",id));});var after=await BusinessSnapshot.Capture(c);Require(before.All(x=>after[x.Key]==x.Value)&&oldLink==Convert.ToString(await Scalar(c,"SELECT(SELECT * FROM dbo.VinculoCartaTCGdex WHERE CartaId=@c FOR JSON PATH)",P("@c",card))),"Identidade divergente modificou local");return new{carta=card,referencia=id,erro=51022,vinculoPreservado=true,comercioPreservado=true,fixture="nome remoto sintetico temporario; restaurado no finally"};}finally{await Update(c,log,JsonNode.Parse(raw)!);}
        });
        await Case("HTML-01/prefill-sem-valores-comerciais","Inputs deBRL/estoque estao efetivamente vazios, nao apenas campo generico",async()=>{
            var choice=await UnusedReference();var r=await Send(adm,HttpMethod.Get,"/gestao/tcgdex151/"+choice.card+"/oferta");Status(r.code,200);
            var price=Regex.IsMatch(r.text,"""<input\b(?=[^>]*name="Preco")(?=[^>]*value="")[^>]*>""");var qty=Regex.IsMatch(r.text,"""<input\b(?=[^>]*name="QuantidadeInicial")(?=[^>]*value="")[^>]*>""");Require(price&&qty,"Preco/quantidade pre-preenchidos ou sem valor vazio explicito");return new{card=choice.card,precoVazio=price,quantidadeVazia=qty,limite="HTML; nao visual"};
        });
        await PriceHistoryTest(adm);
    }
    async Task PriceHistoryTest(Session adm)
    {
        await Case("HTML-02/precos-historicos-cache","Novo snapshot sem pricing mostra precos anteriores com rotulo historico,EUR/USD e nao zeros sinteticos",async()=>{
            await using var c=await Open();var id=Convert.ToInt32(await Scalar(c,"SELECT ReferenciaTCGdexId FROM dbo.vw_BibliotecaTCGdex151 WHERE CardId=N'sv03.5-025'"));var raw=Convert.ToString(await Scalar(c,"SELECT SnapshotJson FROM dbo.ReferenciaTCGdex WHERE ReferenciaTCGdexId=@r",P("@r",id)))!;var lang=Convert.ToString(await Scalar(c,"SELECT Idioma FROM dbo.ReferenciaTCGdex WHERE ReferenciaTCGdexId=@r",P("@r",id)))!;var set=Convert.ToString(await Scalar(c,"SELECT SnapshotJson FROM dbo.ColecaoReferenciaTCGdex WHERE Idioma=@l",P("@l",lang)))!;var log=Convert.ToInt32((await Begin(c,set,lang,Guid.NewGuid()))["ImportacaoId"]);
            var precoAnterior=Convert.ToDateTime(await Scalar(c,"SELECT PrecosColetadosEmUtc FROM dbo.ReferenciaTCGdex WHERE ReferenciaTCGdexId=@r",P("@r",id)));if(Environment.GetEnvironmentVariable("PV_TCG_PRICE_DELAY")=="1")await Task.Delay(1200);
            try{var no=JsonNode.Parse(raw)!;no.AsObject().Remove("pricing");no.AsObject().Remove("variants_detailed");await Update(c,log,no);var r=await Send(adm,HttpMethod.Get,"/gestao/tcgdex151/sv03.5-025");Status(r.code,200);var html=WebUtility.HtmlDecode(r.text);var delta=(Convert.ToDateTime(await Scalar(c,"SELECT SnapshotColetadoEmUtc FROM dbo.ReferenciaTCGdex WHERE ReferenciaTCGdexId=@r",P("@r",id)))-precoAnterior).TotalSeconds;var historico=html.Contains("Preços preservados de uma coleta anterior");var fornecedores=html.Contains("Cardmarket")&&html.Contains("TCGplayer")&&html.Contains("EUR")&&html.Contains("USD");Require(historico&&fornecedores,"historico="+historico+";fornecedores="+fornecedores+";gapSegundos="+delta);return new{referencia=id,historicoIdentificado=historico,fornecedoresMantidos=fornecedores,intervaloColetasSegundos=delta,limite="Fixture ausencia de pricing; HTML/HTTP; intervalo medido; sem espera artificial por padrao"};}finally{await Update(c,log,JsonNode.Parse(raw)!);}
        });
    }
}


