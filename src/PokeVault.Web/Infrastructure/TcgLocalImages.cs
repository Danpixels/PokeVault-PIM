using Microsoft.Data.SqlClient;

namespace PokeVault.Web.Infrastructure;

public sealed class TcgLocalImages(IWebHostEnvironment env, IConfiguration config)
{
    private string Banco => new SqlConnectionStringBuilder(config.GetConnectionString("PokeVault")).InitialCatalog;
    public (string? Url, string? Idioma) Resolver(string? id, string? idioma)
    {
        if (id is null || !TcgDexClient.IdValido(id)) return (null, null);
        foreach (var lang in new[] { idioma == "en" ? "en" : "pt", idioma == "en" ? "pt" : "en" })
        {
            var url = $"/cache/tcgdex151/{Banco}/{lang}/{id}.webp";
            var file = Path.Combine(env.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(file) && new FileInfo(file).Length > 0) return (url, lang);
        }
        return (null, null);
    }
}
