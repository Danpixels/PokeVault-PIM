using System.Net;
namespace PokeVault.Testes.Tcgdex;

// Somente em tests: injeta respostas de rede via HttpClient, sem servidor/backdoor no produto.
public sealed class FixtureHttpHandler(Func<HttpRequestMessage,HttpResponseMessage> response) : HttpMessageHandler
{
    public System.Collections.Concurrent.ConcurrentQueue<string> Requests {get;}=new();
    public bool Offline {get;set;}
    public bool SimularTimeout {get;set;}
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
    {
        Requests.Enqueue(request.RequestUri?.AbsoluteUri??"(sem URI)");
        if(SimularTimeout)throw new TaskCanceledException("Timeout sintetico do teste");
        if(Offline)throw new HttpRequestException("API indisponivel - fixture de teste");
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(response(request));
    }
    public static HttpResponseMessage Json(string json,HttpStatusCode status=HttpStatusCode.OK)=>new(status){Content=new StringContent(json,System.Text.Encoding.UTF8,"application/json")};
}

