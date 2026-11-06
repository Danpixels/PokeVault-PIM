using System.Text.Json;
using System.Text.Json.Nodes;
using System.Data;
using Microsoft.Data.SqlClient;
using PokeVault.Testes.Tcgdex;
partial class TcgSuite
{
    static SqlParameter MaxText(string name,string value)=>new(name,SqlDbType.NVarChar,-1){Value=value};
    async Task<Dictionary<string,object?>> Begin(SqlConnection c,string json,string lang,Guid key)=>await Proc(c,"dbo.usp_IniciarImportacaoTCGdex151",P("@ResponsavelId",admin),P("@ChaveImportacao",key),P("@Idioma",lang),MaxText("@SetJson",json),P("@ColetadoEmUtc",DateTime.UtcNow));
    async Task Update(SqlConnection c,int id,params JsonNode[] nodes){var items=nodes.Select(x=>new{coletadoEmUtc=DateTime.UtcNow,snapshot=JsonSerializer.Deserialize<JsonElement>(x.ToJsonString())}).ToArray();await Proc(c,"dbo.usp_AtualizarReferenciasTCGdex151",P("@ResponsavelId",admin),P("@ImportacaoId",id),MaxText("@LoteJson",JsonSerializer.Serialize(new{versao=1,cartas=items})));}
    async Task SqlTests()
    {
        await Case("DB-01/manifesto-idempotente-escopo","Mesmo manifesto/chave retorna mesma importacao; outro set recusado sem cache apagar",async()=>{
            var set=await File.ReadAllTextAsync("tests/Tcgdex151/Fixtures/set-pt-gravado.json");await using var c=await Open();var key=Guid.NewGuid();var first=await Begin(c,set,"pt",key);var again=await Begin(c,set,"pt",key);Require(Convert.ToInt32(first["ImportacaoId"])==Convert.ToInt32(again["ImportacaoId"])&&(bool)again["Repetida"]!,"Manifesto duplicou operacao");
            var bad=JsonNode.Parse(set)!;bad["id"]="sv01";await Error(51020,async()=>{await Begin(c,bad.ToJsonString(),"pt",Guid.NewGuid());});Require(await Count(c,"vw_BibliotecaTCGdex151","1=1")==207,"Manifesto invalido mudou biblioteca");return new{importacao=first["ImportacaoId"],repetida=true,referencias=207,outroSet=51020};
        });
        await Case("DB-02/pricing-ausente-cache","Atualizacao sem pricing preserva ultimo JSON/data/origem; lote invalido atomico",async()=>{
            await using var c=await Open();var lang=Convert.ToString(await Scalar(c,"SELECT Idioma FROM dbo.vw_BibliotecaTCGdex151 WHERE CardId=N'sv03.5-025'"))!;var raw=Convert.ToString(await Scalar(c,"SELECT SnapshotJson FROM dbo.ReferenciaTCGdex WHERE CardId=N'sv03.5-025' AND Idioma=@l",P("@l",lang)))!;
            var refId=Convert.ToInt32(await Scalar(c,"SELECT ReferenciaTCGdexId FROM dbo.ReferenciaTCGdex WHERE CardId=N'sv03.5-025' AND Idioma=@l",P("@l",lang)));
            var before=Convert.ToString(await Scalar(c,"SELECT(SELECT PrecosJson,PrecosColetadosEmUtc,OrigemPrecosUrl FROM dbo.ReferenciaTCGdex WHERE ReferenciaTCGdexId=@r FOR JSON PATH,INCLUDE_NULL_VALUES)",P("@r",refId)))!;
            var set=Convert.ToString(await Scalar(c,"SELECT SnapshotJson FROM dbo.ColecaoReferenciaTCGdex WHERE Idioma=@l AND SetId=N'sv03.5'",P("@l",lang)))!;var started=await Begin(c,set,lang,Guid.NewGuid());var id=Convert.ToInt32(started["ImportacaoId"]);
            try{
                var no=JsonNode.Parse(raw)!;no.AsObject().Remove("pricing");no.AsObject().Remove("variants_detailed");await Update(c,id,no);
                var after=Convert.ToString(await Scalar(c,"SELECT(SELECT PrecosJson,PrecosColetadosEmUtc,OrigemPrecosUrl FROM dbo.ReferenciaTCGdex WHERE ReferenciaTCGdexId=@r FOR JSON PATH,INCLUDE_NULL_VALUES)",P("@r",refId)))!;Require(before==after,"Ausencia apagou/alterou ultimo pricing valido");
                var snapshot=Convert.ToString(await Scalar(c,"SELECT SnapshotJson FROM dbo.ReferenciaTCGdex WHERE ReferenciaTCGdexId=@r",P("@r",refId)))!;var wrong=JsonNode.Parse(raw)!;wrong["id"]="sv03.5-999";wrong["localId"]="999";await Error(51020,async()=>{await Update(c,id,JsonNode.Parse(raw)!,wrong);});Require(Convert.ToString(await Scalar(c,"SELECT SnapshotJson FROM dbo.ReferenciaTCGdex WHERE ReferenciaTCGdexId=@r",P("@r",refId)))==snapshot,"Lote invalido gravou parcialmente");
                return new{referencia=refId,idioma=lang,pricingPreservado=true,loteInvalido=51020,restauracao="snapshot real anterior reaplicado no finally"};
            }finally{await Update(c,id,JsonNode.Parse(raw)!);}
        });
        await Case("DB-03/vinculo-seguro-compartilhado","PikachuNM/SP compartilham referencia sem mudarCarta; identidade divergente recusada",async()=>{
            await using var c=await Open();var reference=Convert.ToInt32(await Scalar(c,"SELECT ReferenciaTCGdexId FROM dbo.vw_BibliotecaTCGdex151 WHERE CardId=N'sv03.5-025' AND Detalhada=1"));
            var rows=new List<int>();await using(var command=new SqlCommand("SELECT c.CartaId FROM dbo.Carta c JOIN dbo.Colecao co ON co.ColecaoId=c.ColecaoId WHERE co.Codigo=N'MEW-151' AND c.Numero=N'025' AND c.Nome=N'Pikachu' AND c.Conservacao IN(N'NEAR_MINT',N'SLIGHTLY_PLAYED') ORDER BY CartaId",c)){await using var r=await command.ExecuteReaderAsync();while(await r.ReadAsync())rows.Add(r.GetInt32(0));}
            Require(rows.Count==2,"Massa original Pikachu nao identificada");var before=await BusinessSnapshot.Capture(c);
            foreach(var card in rows)await Proc(c,"dbo.usp_VincularOfertaTCGdex151",P("@ResponsavelId",admin),P("@CartaId",card),P("@ReferenciaTCGdexId",reference));
            Require(rows.All(card=>Convert.ToInt32(Scalar(c,"SELECT ReferenciaTCGdexId FROM dbo.VinculoCartaTCGdex WHERE CartaId=@id",P("@id",card)).GetAwaiter().GetResult())==reference),"Conservacoes divergiram de referencia");
            var wrong=Convert.ToInt32(await Scalar(c,"SELECT TOP(1) CartaId FROM dbo.Carta WHERE Nome<>N'Pikachu' AND TRY_CONVERT(int,Numero)<>25 ORDER BY CartaId"));await Error(51022,async()=>{await Proc(c,"dbo.usp_VincularOfertaTCGdex151",P("@ResponsavelId",admin),P("@CartaId",wrong),P("@ReferenciaTCGdexId",reference));});var after=await BusinessSnapshot.Capture(c);Require(before.All(x=>after[x.Key]==x.Value),"Vinculo mudou dados comerciais");return new{cartas=rows,referencia=reference,divergente=wrong,erro=51022,camposComerciaisPreservados=true};
        });
    }
}
