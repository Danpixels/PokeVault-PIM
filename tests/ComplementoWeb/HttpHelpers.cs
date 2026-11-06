using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
partial class Suite
{
    sealed class Session : IDisposable
    {
        public HttpClient Client {get;} = new(new HttpClientHandler { CookieContainer=new CookieContainer(),AllowAutoRedirect=false }) { BaseAddress=new Uri("http://localhost:5188"),Timeout=TimeSpan.FromSeconds(20) };
        public string Token="";
        public void Dispose()=>Client.Dispose();
    }
    static async Task<(int code,JsonElement data,string text)> Send(Session s,HttpMethod method,string path,object? body=null,bool token=true)
    {
        using var q=new HttpRequestMessage(method,path);if(body!=null)q.Content=JsonContent.Create(body);
        if(token && s.Token!="")q.Headers.Add("X-CSRF-TOKEN",s.Token);
        using var r=await s.Client.SendAsync(q);var text=await r.Content.ReadAsStringAsync();JsonElement data=default;
        if(r.Content.Headers.ContentType?.MediaType?.Contains("json")==true && text.Length>0)data=JsonDocument.Parse(text).RootElement.Clone();
        return ((int)r.StatusCode,data,text);
    }
    static void Status(int got,int expected)=>Require(got==expected,$"HTTP esperado{expected}, observado{got}");
    static int I(JsonElement j,string key)=>j.GetProperty(key).GetInt32();
    static string S(JsonElement j,string key)=>j.GetProperty(key).GetString()!;
    static decimal D(JsonElement j,string key)=>j.GetProperty(key).GetDecimal();
    static async Task Token(Session s) {var r=await Send(s,HttpMethod.Get,"/api/v1/antiforgery",token:false);Status(r.code,200);s.Token=S(r.data,"token");}
    static async Task Login(Session s,string email,string secret)
    {await Token(s);var r=await Send(s,HttpMethod.Post,"/api/v1/sessao",new {email,senha=secret});Status(r.code,200);await Token(s);}
    async Task<(int id,string email,string secret)> HttpUser(Session adm,string type)
    {
        var email=tag+"-"+Guid.NewGuid().ToString("N")[..8]+"@tests.pokevault.local";var secret=Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24));
        var r=await Send(adm,HttpMethod.Post,"/api/v1/admin/"+type,new{nome=tag,email,senhaInicial=secret});Status(r.code,201);return (I(r.data,"usuarioId"),email,secret);
    }
    static async Task<JsonElement> Get(Session s,string path) {var r=await Send(s,HttpMethod.Get,path);Status(r.code,200);return r.data;}
    static async Task<JsonElement> SetBalance(Session adm,int id,decimal value,string version)
    {var r=await Send(adm,HttpMethod.Put,$"/api/v1/admin/clientes/{id}/saldo",new{novoSaldo=value,motivo="Testes Sprint2",versaoEsperada=version});Status(r.code,200);return r.data;}
    static object Purchase(Guid uuid,params (int id,decimal preco)[] items)=>new{chaveOperacao=uuid,itens=items.Select(i=>new{ofertaId=i.id,quantidade=1,precoConfirmado=i.preco}).ToArray()};
    static async Task HCart(Session s,int card,int qty=1) {var r=await Send(s,HttpMethod.Put,$"/api/v1/carrinho/itens/{card}",new{quantidade=qty});Status(r.code,204);}
}
