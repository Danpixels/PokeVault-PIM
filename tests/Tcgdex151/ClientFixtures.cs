using Microsoft.Extensions.Configuration;
using PokeVault.Web.Infrastructure;
using PokeVault.Web.Domain;
using PokeVault.Testes.Tcgdex;
using System.Net;
using System.Text.Json.Nodes;
using System.Security.Cryptography;

partial class TcgSuite
{
    async Task ClientFixtures()
    {
        var real=await File.ReadAllTextAsync("tests/Tcgdex151/Fixtures/pikachu-en-gravado.json");
        FixtureHttpHandler Handler(string json)=>new(q=>q.RequestUri!.Host=="assets.tcgdex.net"?FixtureHttpHandler.Json("{}",HttpStatusCode.NotFound):q.RequestUri.AbsolutePath.Contains("/pt/")?FixtureHttpHandler.Json("{}",HttpStatusCode.NotFound):FixtureHttpHandler.Json(json));
        TcgDexClient Client(FixtureHttpHandler h,string suffix)=>new(new HttpClient(h),new FixtureEnvironment("tests/.runtime/tcgdex-fixtures/"+tag+"-"+suffix),new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:PokeVault",Cs}}).Build());
        await Case("FX-01/alvo-fixo","ID de outra colecao recusado antes de rede",async()=>{
            var h=Handler(real);var client=Client(h,"scope");try{await client.Carta("sv01-001",true,false,default);throw new Exception("Outro set aceito");}catch(RegraException e)when(e.Status==400){}
            Require(h.Requests.Count==0,"Buscou outro set");return new{requisicoes=h.Requests.Count,recusa=400};
        });
        await Case("FX-02/indisponivel-sem-cache","Rede indisponivel sem cache retorna ausencia explicita",async()=>{
            var h=Handler(real);h.Offline=true;var doc=await Client(h,"offline-empty").Carta("sv03.5-025",true,false,default);Require(doc is null,"Inventou detalhe sem cache");return new{ausente=true,requisicoes=h.Requests.Count};
        });
        await Case("FX-03/timeout-sem-cache","Timeout controlado sem cache nao inventa referencia/preco",async()=>{
            var h=Handler(real);h.SimularTimeout=true;var doc=await Client(h,"timeout").Carta("sv03.5-025",true,false,default);Require(doc is null,"Timeout inventou detalhe");return new{ausente=true,requisicoes=h.Requests.Count};
        });
        await Case("FX-04/fallback-en-imagem-ausente","PT404/EN200 identifica ingles; erro de imagem preserva metadados",async()=>{
            var h=Handler(real);var doc=await Client(h,"fallback").Carta("sv03.5-025",true,false,default);Require(doc is not null&&doc.Dados.Idioma=="en"&&doc.ImagemLocal is null,"Fallback nao identificado");return new{idioma=doc!.Dados.Idioma,imagemLocal=doc.ImagemLocal,requests=h.Requests.ToArray(),fonte=doc.Dados.Url};
        });
        await Case("FX-05/cache-preservado","Falha apos sucesso conserva JSON/cache e retorna Offline/Cache",async()=>{
            var h=Handler(real);var client=Client(h,"cache");var first=await client.Carta("sv03.5-025",true,false,default);Require(first is not null,"Nao preparou cache");var root="tests/.runtime/tcgdex-fixtures/"+tag+"-cache";var files=Directory.GetFiles(root,"*.json",SearchOption.AllDirectories);var hashes=files.ToDictionary(x=>x,x=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(x))));h.Offline=true;
            var after=await client.Carta("sv03.5-025",true,false,default);Require(after is not null&&after.Dados.Cache&&after.Dados.Offline&&after.Dados.Json==first!.Dados.Json&&hashes.All(x=>File.Exists(x.Key)&&Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(x.Key)))==x.Value),"Cache apagado/alterado na falha");return new{arquivos=hashes.Count,cache=true,offline=true,jsonPreservado=true};
        });
        await Case("FX-06/precos-ausentes","Pricing ausente resulta lista vazia, nao preco zero",async()=>{
            var node=JsonNode.Parse(real)!;node.AsObject().Remove("pricing");node.AsObject().Remove("variants_detailed");var json=node.ToJsonString();var doc=await Client(Handler(json),"no-prices").Carta("sv03.5-025",true,false,default);Require(doc is not null&&TcgPrecos.Ler(doc.Dados.Json).Count==0,"Ausencia virou preco");return new{precos=0,significado="indisponivel; nenhum zero sintetico"};
        });
        await Case("FX-07/variante-dinamica-nulo","Variante nova USD preserva nome,valor9.99 e ausencia null",async()=>{
            var json="{\"pricing\":{\"tcgplayer\":{\"unit\":\"USD\",\"updated\":\"2026-11-03T00:00:00Z\",\"galaxy-new-holo\":{\"marketPrice\":null,\"lowPrice\":9.99}}}}";var rows=TcgPrecos.Ler(json);Require(rows.Count==2&&rows.All(x=>x.Moeda=="USD"&&x.Variante.Contains("galaxy-new-holo"))&&rows.Any(x=>x.Valor is null)&&rows.Any(x=>x.Valor==9.99m),"Variante/nulo alterados");return new{linhas=rows,tipo="fixture sintetica, nao preco atual"};
        });
        await Case("FX-08/precos-reais-gravados","Amostra real gravada mantem EUR/USD,origem de atualizacao e reverse-holofoil",async()=>{
            var rows=TcgPrecos.Ler(real);Require(rows.Any(x=>x.Moeda=="EUR")&&rows.Any(x=>x.Moeda=="USD"&&x.Variante.Contains("reverse-holofoil"))&&rows.Where(x=>x.Valor is not null).All(x=>!string.IsNullOrEmpty(x.Atualizado)),"Origem/moeda/variante ausente");return new{linhas=rows.Count,moedas=rows.Select(x=>x.Moeda).Distinct().ToArray(),reverseHolofoil=true,fonte="resposta real gravada por Banco; nao importacao atual"};
        });
    }
}
