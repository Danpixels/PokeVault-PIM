using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
namespace PokeVault.Testes.Tcgdex;
public sealed class FixtureEnvironment : IWebHostEnvironment
{
    public FixtureEnvironment(string root){ContentRootPath=Path.GetFullPath(root);WebRootPath=Path.Combine(ContentRootPath,"wwwroot");Directory.CreateDirectory(WebRootPath);}
    public string ApplicationName {get;set;}="PokeVault.Web";
    public string EnvironmentName {get;set;}="Development";
    public string ContentRootPath {get;set;}
    public IFileProvider ContentRootFileProvider {get;set;}=new NullFileProvider();
    public string WebRootPath {get;set;}
    public IFileProvider WebRootFileProvider {get;set;}=new NullFileProvider();
}
