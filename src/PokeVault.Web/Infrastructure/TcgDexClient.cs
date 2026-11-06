using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using PokeVault.Web.Domain;

namespace PokeVault.Web.Infrastructure;

public sealed class TcgDexClient(HttpClient http, IWebHostEnvironment env, IConfiguration config)
{
    public const string SetId = "sv03.5";
    private string Base => new SqlConnectionStringBuilder(config.GetConnectionString("PokeVault")).InitialCatalog;
    private string CacheRoot => Path.Combine(env.ContentRootPath, ".local", "tcgdex151", Base);
    public bool RedeHabilitada => config.GetValue("TcgDex:RedeHabilitada", true);
    public static bool IdValido(string id) => Regex.IsMatch(id, "^sv03\\.5-[0-9]{3}$") && int.TryParse(id[7..], out var n) && n is >= 1 and <= 207;
    private sealed record Envelope(string Json, string Idioma, string Url, DateTime ColetadoEmUtc);

    public Task<TcgJson?> Colecao(CancellationToken ct) => JsonPreferido("sets/" + SetId, true, !RedeHabilitada, ct);
    public Task<TcgJson?> ColecaoIdioma(string idioma, bool somenteCache, CancellationToken ct)
    {
        if (idioma is not ("pt" or "en")) throw new ArgumentException("Idioma inválido.");
        return ObterJson(idioma, "sets/" + SetId, true, somenteCache, ct);
    }
    public async Task<TcgCartaDocumento?> Carta(string id, bool forcar, bool somenteCache, CancellationToken ct)
    {
        if (!IdValido(id)) throw new RegraException("REFERENCIA_INVALIDA", "Referência fora da coleção151.", 400);
        var data = await JsonPreferido("cards/" + id, forcar, somenteCache, ct);
        if (data is null) return null;
        using var doc = JsonDocument.Parse(data.Json);
        var remote = TcgPrecos.Texto(doc.RootElement, "image");
        var local = await Imagem(remote, data.Idioma, id, forcar && !somenteCache, somenteCache, ct);
        var imageLanguage = local is null ? null : data.Idioma;
        if (local is null && data.Idioma == "pt")
        {
            var english = await ObterJson("en", "cards/" + id, false, somenteCache, ct);
            if (english is not null)
            {
                using var en = JsonDocument.Parse(english.Json);
                var alternate = TcgPrecos.Texto(en.RootElement, "image");
                local = await Imagem(alternate, "en", id, false, somenteCache, ct);
                if (local is not null) { remote = alternate; imageLanguage = "en"; }
            }
        }
        return new(data, remote, local, imageLanguage);
    }
    private async Task<TcgJson?> JsonPreferido(string endpoint, bool force, bool offline, CancellationToken ct) =>
        await ObterJson("pt", endpoint, force, offline, ct) ?? await ObterJson("en", endpoint, force, offline, ct);

    private async Task<TcgJson?> ObterJson(string idioma, string endpoint, bool force, bool somenteCache, CancellationToken ct)
    {
        var path = Path.Combine(CacheRoot, idioma, endpoint.Replace('/', '_') + ".json");
        Envelope? cached = null;
        try
        {
            if (File.Exists(path))
            {
                cached = JsonSerializer.Deserialize<Envelope>(await File.ReadAllTextAsync(path, ct));
                if (cached is not null)
                {
                    Validar(cached.Json, endpoint);
                    if (cached.Idioma != idioma || cached.Url != "https://api.tcgdex.net/v2/" + idioma + "/" + endpoint
                        || cached.ColetadoEmUtc.Year < 2000 || cached.ColetadoEmUtc > DateTime.UtcNow.AddMinutes(5)) throw new InvalidDataException("Envelope de cache inválido.");
                }
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidDataException) { cached = null; }
        if (cached is not null && (somenteCache || !RedeHabilitada || !force && cached.ColetadoEmUtc > DateTime.UtcNow.AddHours(-24)))
            return new(cached.Json, cached.Idioma, cached.Url, cached.ColetadoEmUtc, true, somenteCache || !RedeHabilitada);
        if (RedeHabilitada && !somenteCache)
        {
            var url = "https://api.tcgdex.net/v2/" + idioma + "/" + endpoint;
            try
            {
                var bytes = await Baixar(new Uri(url), 2_000_000, false, ct);
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                Validar(json, endpoint);
                var now = DateTime.UtcNow;
                await Gravar(path, JsonSerializer.SerializeToUtf8Bytes(new Envelope(json, idioma, url, now)), ct);
                return new(json, idioma, url, now, false, false);
            }
            catch (Exception ex) when (ex is HttpRequestException or IOException or JsonException or InvalidDataException or OperationCanceledException && !ct.IsCancellationRequested) { }
        }
        return cached is null ? null : new(cached.Json, cached.Idioma, cached.Url, cached.ColetadoEmUtc, true, true);
    }
    private static void Validar(string json, string endpoint)
    {
        using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 64 });
        var root = doc.RootElement;
        var esperado = endpoint[(endpoint.IndexOf('/') + 1)..];
        var nome = TcgPrecos.Texto(root, "name");
        if (TcgPrecos.Texto(root, "id") != esperado || string.IsNullOrWhiteSpace(nome) || nome.Length > 120) throw new InvalidDataException("Identidade externa inválida.");
        if (endpoint.StartsWith("sets/", StringComparison.Ordinal))
        {
            if (!root.TryGetProperty("cardCount", out var count) || count.ValueKind != JsonValueKind.Object
                || !count.TryGetProperty("official", out var official) || official.ValueKind != JsonValueKind.Number || !official.TryGetInt32(out var oficiais) || oficiais != 165
                || !count.TryGetProperty("total", out var total) || total.ValueKind != JsonValueKind.Number || !total.TryGetInt32(out var totais) || totais != 207
                || !root.TryGetProperty("cards", out var cards) || cards.ValueKind != JsonValueKind.Array || cards.GetArrayLength() != 207)
                throw new InvalidDataException("Coleção externa inválida.");
            var ids = new HashSet<string>();
            foreach (var carta in cards.EnumerateArray())
            {
                var id = TcgPrecos.Texto(carta, "id") ?? ""; var name = TcgPrecos.Texto(carta, "name");
                if (!IdValido(id) || !ids.Add(id) || TcgPrecos.Texto(carta, "localId") != id[7..] || string.IsNullOrWhiteSpace(name) || name.Length > 120)
                    throw new InvalidDataException("Item do manifesto inválido.");
                ValidarImagem(carta, id);
            }
        }
        else
        {
            if (!root.TryGetProperty("set", out var set) || TcgPrecos.Texto(set, "id") != SetId || TcgPrecos.Texto(root, "localId") != esperado[7..]
                || TcgPrecos.Texto(root, "rarity")?.Length > 100
                || root.TryGetProperty("pricing", out var pricing) && pricing.ValueKind is not (JsonValueKind.Null or JsonValueKind.Object)) throw new InvalidDataException("Carta externa inválida.");
            ValidarImagem(root, esperado);
        }
    }
    private static void ValidarImagem(JsonElement root, string id)
    {
        if (!root.TryGetProperty("image", out var image) || image.ValueKind == JsonValueKind.Null) return;
        if (image.ValueKind != JsonValueKind.String || !Uri.TryCreate(image.GetString(), UriKind.Absolute, out var uri)
            || uri.Scheme != "https" || uri.Host != "assets.tcgdex.net" || !uri.IsDefaultPort || uri.UserInfo.Length > 0 || uri.Query.Length > 0 || uri.Fragment.Length > 0
            || uri.AbsolutePath != "/pt/sv/sv03.5/" + id[7..] && uri.AbsolutePath != "/en/sv/sv03.5/" + id[7..]) throw new InvalidDataException("Origem de imagem inválida.");
    }
    private async Task<string?> Imagem(string? origem, string idioma, string id, bool force, bool somenteCache, CancellationToken ct)
    {
        var relative = $"/cache/tcgdex151/{Base}/{idioma}/{id}.webp";
        var path = Path.Combine(env.WebRootPath, relative.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(path) && new FileInfo(path).Length > 0 && (!force || somenteCache || !RedeHabilitada)) return relative;
        if (origem is not null && Uri.TryCreate(origem, UriKind.Absolute, out var uri) && uri.Scheme == "https" && uri.Host == "assets.tcgdex.net" && uri.IsDefaultPort
            && uri.AbsolutePath == $"/{idioma}/sv/sv03.5/{id[7..]}" && string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment)
            && RedeHabilitada && !somenteCache)
            try
            {
                var bytes = await Baixar(new Uri(origem.TrimEnd('/') + "/low.webp"), 2_000_000, true, ct);
                if (bytes.Length < 12 || System.Text.Encoding.ASCII.GetString(bytes, 0, 4) != "RIFF" || System.Text.Encoding.ASCII.GetString(bytes, 8, 4) != "WEBP") throw new InvalidDataException("Imagem inválida.");
                await Gravar(path, bytes, ct); return relative;
            }
            catch (Exception ex) when (ex is HttpRequestException or IOException or InvalidDataException or OperationCanceledException && !ct.IsCancellationRequested) { }
        return File.Exists(path) && new FileInfo(path).Length > 0 ? relative : null;
    }
    private async Task<byte[]> Baixar(Uri uri, int limite, bool imagem, CancellationToken ct)
    {
        if (uri.Scheme != "https" || !uri.IsDefaultPort || uri.UserInfo.Length > 0 || uri.Host != (imagem ? "assets.tcgdex.net" : "api.tcgdex.net")) throw new InvalidDataException("Origem não autorizada.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct); timeout.CancelAfter(TimeSpan.FromSeconds(18));
        using var response = await http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength > limite) throw new InvalidDataException("Resposta excede limite.");
        var mime = response.Content.Headers.ContentType?.MediaType;
        if (imagem && mime != "image/webp") throw new InvalidDataException("Tipo de imagem inválido.");
        await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
        using var output = new MemoryStream(); var buffer = new byte[16384]; int size;
        while ((size = await stream.ReadAsync(buffer, timeout.Token)) > 0) { if (output.Length + size > limite) throw new InvalidDataException("Resposta excede limite."); await output.WriteAsync(buffer.AsMemory(0, size), timeout.Token); }
        return output.ToArray();
    }
    private static async Task Gravar(string path, byte[] bytes, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try { await File.WriteAllBytesAsync(temp, bytes, ct); File.Move(temp, path, true); }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
}
