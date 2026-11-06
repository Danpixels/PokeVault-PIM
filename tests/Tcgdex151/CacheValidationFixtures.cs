using PokeVault.Web.Infrastructure;
using Microsoft.Extensions.Configuration;
using PokeVault.Testes.Tcgdex;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using System.Net;
partial class TcgSuite
{
    async Task CacheValidationFixtures()
    {
        var card=await File.ReadAllTextAsync("tests/Tcgdex151/Fixtures/pikachu-en-gravado.json");var set=await File.ReadAllTextAsync("tests/Tcgdex151/Fixtures/set-pt-gravado.json");
        static Dictionary<string,string> Hashes(string root)=>Directory.GetFiles(root,"*.json",SearchOption.AllDirectories).ToDictionary(x=>x,x=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(x))));
        await Case("FX-10/pricing-malformado-nao-envenena-cache","Resposta pricing string invalida nao substitui snapshot valido; sem cache retorna ausencia",async()=>{
            string payload=card;var handler=new FixtureHttpHandler(q=>q.RequestUri!.Host=="assets.tcgdex.net"||q.RequestUri.AbsolutePath.Contains("/pt/")?FixtureHttpHandler.Json("{}",HttpStatusCode.NotFound):FixtureHttpHandler.Json(payload));var env=new FixtureEnvironment("tests/.runtime/tcgdex-fixtures/"+tag+"-badpricing");var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:PokeVault",Cs}}).Build();var client=new TcgDexClient(new HttpClient(handler),env,config);var good=await client.Carta("sv03.5-025",true,false,default);Require(good is not null,"Cache nao preparado");var before=Hashes(env.ContentRootPath);var bad=JsonNode.Parse(card)!;bad["pricing"]="malformado";payload=bad.ToJsonString();var after=await client.Carta("sv03.5-025",true,false,default);Require(after is not null&&after.Dados.Cache&&after.Dados.Json==good!.Dados.Json&&before.All(x=>Hashes(env.ContentRootPath)[x.Key]==x.Value),"Resposta invalida substituiu cache");var empty=new FixtureEnvironment(env.ContentRootPath+"-vazio");var cold=await new TcgDexClient(new HttpClient(handler),empty,config).Carta("sv03.5-025",true,false,default);Require(cold is null,"Sem cache aceitou pricing malformado");return new{cacheValidoPreservado=true,arquivos=before.Count,semCache="ausencia",tipo="fixture sintetica - correcao6E"};
        });
        await Case("FX-11/manifesto-invalido-nao-envenena-cache","Manifesto com207itens mas IDs duplicados ou contagem206 preserva cache207 valido",async()=>{
            string payload=set;var handler=new FixtureHttpHandler(_=>FixtureHttpHandler.Json(payload));var env=new FixtureEnvironment("tests/.runtime/tcgdex-fixtures/"+tag+"-badset");var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:PokeVault",Cs}}).Build();var client=new TcgDexClient(new HttpClient(handler),env,config);var first=await client.Colecao(default);Require(first is not null,"Manifesto nao preparado");var before=Hashes(env.ContentRootPath);
            var duplicate=JsonNode.Parse(set)!;duplicate["cards"]![1]=duplicate["cards"]![0]!.DeepClone();var count=JsonNode.Parse(set)!;count["cardCount"]!["total"]=206;
            foreach(var invalid in new[]{duplicate,count}){payload=invalid.ToJsonString();var after=await client.Colecao(default);Require(after is not null&&after.Cache&&after.Json==first!.Json&&before.All(x=>Hashes(env.ContentRootPath)[x.Key]==x.Value),"Manifesto invalido substituiu cache");}
            return new{tentativasInvalidas=2,cache207Preservado=true,arquivos=before.Count,tipo="fixture sintetica - correcao6E"};
        });
    }
}
