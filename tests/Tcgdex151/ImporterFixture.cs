using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using PokeVault.Web.Infrastructure;
using PokeVault.Web.Application;
using PokeVault.Web.Domain;
using PokeVault.Testes.Tcgdex;
using System.Reflection;
using System.Text.Json;
using System.Net;
using System.Collections.Concurrent;
partial class TcgSuite
{
    async Task ImporterFixture()
    {
        await Case("FX-09/importador-fallback-207","Importer real com pt404/en200 persiste207 detalhes em manifestoen, sem servidor/SQL de fixture",async()=>{
            var set=await File.ReadAllTextAsync("tests/Tcgdex151/Fixtures/set-pt-gravado.json");using var sd=JsonDocument.Parse(set);var cards=sd.RootElement.GetProperty("cards").EnumerateArray().ToDictionary(x=>x.GetProperty("id").GetString()!,x=>x.GetProperty("name").GetString()!);
            var handler=new FixtureHttpHandler(q=>{
                var path=q.RequestUri!.AbsolutePath;
                if(q.RequestUri.Host=="assets.tcgdex.net"||path.Contains("/pt/cards/"))return FixtureHttpHandler.Json("{}",HttpStatusCode.NotFound);
                if(path.Contains("/sets/"))return FixtureHttpHandler.Json(set);
                var id=path.Split('/').Last();return FixtureHttpHandler.Json(JsonSerializer.Serialize(new{id,localId=id[7..],name=cards[id],set=new{id="sv03.5"},rarity="Common"}));
            });
            var env=new FixtureEnvironment("tests/.runtime/tcgdex-fixtures/"+tag+"-importer");var cfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:PokeVault",Cs}}).Build();
            var store=new RecordingStore();var users=DispatchProxy.Create<ILojaRepository,UserProxy>();
            var services=new ServiceCollection().AddSingleton<ITcgDexStore>(store).AddSingleton<ILojaRepository>(users).AddScoped<TcgDexService>().AddScoped(_=>new TcgDexClient(new HttpClient(handler),env,cfg));
            using var provider=services.BuildServiceProvider();var importer=new TcgDexImportador(provider.GetRequiredService<IServiceScopeFactory>(),new TestLifetime(),env,cfg);Require(importer.Iniciar(admin,false),"Nao iniciou importer");
            var deadline=DateTime.UtcNow.AddSeconds(40);while(importer.Estado.EmExecucao&&DateTime.UtcNow<deadline)await Task.Delay(20);
            Require(!importer.Estado.EmExecucao&&importer.Estado.Importados==207&&store.Saved.Count==207&&store.Saved.All(x=>x.Idioma=="en")&&store.LanguageMismatch==0,"Fallback perdeu idioma/detalhe: "+importer.Estado.Mensagem);
            return new{status=importer.Estado,detalhesGravados=store.Saved.Count,idiomas=store.Saved.Select(x=>x.Idioma).Distinct().ToArray(),manifestos=store.Languages.Values.ToArray(),mismatch=store.LanguageMismatch,requisicoes=handler.Requests.Count,tipo="Fixture sintetica, importer real em DI de teste"};
        });
    }
}
public class UserProxy:DispatchProxy
{
    protected override object? Invoke(MethodInfo? targetMethod,object?[]? args)=>targetMethod?.Name=="UsuarioPorId"?Task.FromResult<Usuario?>(new Usuario(1,"Teste","nao-exposto@tests.local","","ADMINISTRADOR",true)):throw new NotSupportedException("Metodo de usuario nao esperado");
}
public sealed class TestLifetime:IHostApplicationLifetime
{
    public CancellationToken ApplicationStarted=>CancellationToken.None;public CancellationToken ApplicationStopping=>CancellationToken.None;public CancellationToken ApplicationStopped=>CancellationToken.None;public void StopApplication(){}
}
public sealed class RecordingStore:ITcgDexStore
{
    int seq;public ConcurrentDictionary<long,string> Languages {get;}=new();public ConcurrentBag<TcgReferencia> Saved {get;}=[];public int LanguageMismatch;
    public Task<long> Iniciar(int ator,TcgJson dados,Guid chave,CancellationToken ct){var id=Interlocked.Increment(ref seq);Languages[id]=dados.Idioma;return Task.FromResult((long)id);}
    public Task SalvarCarta(long importacao,int ator,TcgReferencia carta,CancellationToken ct){if(Languages[importacao]!=carta.Idioma){Interlocked.Increment(ref LanguageMismatch);throw new InvalidOperationException("Idioma do snapshot difere do manifesto");}Saved.Add(carta);return Task.CompletedTask;}
    public Task Finalizar(long id,int ator,TcgImportStatus status,CancellationToken ct)=>Task.CompletedTask;
    public Task VincularOriginais(int ator,CancellationToken ct)=>Task.CompletedTask;
    public Task<TcgImportStatus?> UltimaImportacao(CancellationToken ct)=>Task.FromResult<TcgImportStatus?>(null);
    public Task<TcgPagina> Listar(string? nome,string? raridade,int pagina,CancellationToken ct)=>Task.FromResult(new TcgPagina([],pagina,0));
    public Task<TcgReferencia?> Obter(string id,CancellationToken ct)=>Task.FromResult<TcgReferencia?>(Saved.FirstOrDefault(x=>x.Id==id));
    public Task<int> CadastrarOfertaReferencia(int ator,int referenciaId,string conservacao,decimal preco,int quantidade,CancellationToken ct)=>throw new NotSupportedException();
}
