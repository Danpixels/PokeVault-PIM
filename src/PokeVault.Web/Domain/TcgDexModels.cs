using System.Text.Json;

namespace PokeVault.Web.Domain;

public sealed record TcgJson(string Json, string Idioma, string Url, DateTime ColetadoEmUtc, bool Cache, bool Offline);
public sealed record TcgCartaDocumento(TcgJson Dados, string? ImagemRemota, string? ImagemLocal, string? IdiomaImagem);
public sealed record TcgReferencia(string Id, string Numero, string Nome, string? Raridade, string Idioma, string? ImagemLocal, string? ImagemRemota,
    string? IdiomaImagem, string SnapshotJson, string PrecosJson, DateTime ImportadoEmUtc, string FonteUrl, int ReferenciaId = 0, bool Detalhada = true, DateTime? PrecosColetadosUtc = null);
public sealed record TcgPagina(IReadOnlyList<TcgReferencia> Itens, int Pagina, int Total);
public sealed record TcgPreco(string Fornecedor, string Moeda, string Variante, string Campo, decimal? Valor, string? Atualizado);
public sealed record TcgImportStatus(bool EmExecucao, long? ImportacaoId, int Processados, int Importados, int Ausentes, int ImagensLocais,
    int FallbackIngles, bool ModoOffline, string Mensagem, DateTime? InicioUtc, DateTime? FimUtc);

public static class TcgPrecos
{
    public static IReadOnlyList<TcgPreco> Ler(string snapshot)
    {
        List<TcgPreco> result = [];
        try
        {
            using var doc = JsonDocument.Parse(snapshot);
            if (doc.RootElement.TryGetProperty("pricing", out var root)) LerFornecedores(root, "Referência principal", result);
            if (doc.RootElement.TryGetProperty("variants_detailed", out var variants) && variants.ValueKind == JsonValueKind.Array)
                foreach (var v in variants.EnumerateArray())
                    if (v.TryGetProperty("pricing", out var pricing))
                    {
                        var titulo = Texto(v, "type") ?? "Variante";
                        if (Texto(v, "foil") is string foil) titulo += " / " + foil;
                        if (v.TryGetProperty("stamp", out var stamp) && stamp.ValueKind == JsonValueKind.Array) titulo += " / " + string.Join(", ", stamp.EnumerateArray().Select(s => s.ToString()));
                        if (Texto(v, "variantId") is string id) titulo += " [" + id + "]";
                        LerFornecedores(pricing, titulo, result);
                    }
        }
        catch (JsonException) { }
        return result;
    }
    private static void LerFornecedores(JsonElement root, string variante, List<TcgPreco> result)
    {
        if (root.ValueKind != JsonValueKind.Object) return;
        foreach (var p in root.EnumerateObject())
            if (p.Name is "cardmarket" or "tcgplayer" && p.Value.ValueKind == JsonValueKind.Object)
                LerCampos(p.Value, p.Name == "cardmarket" ? "Cardmarket" : "TCGplayer", Texto(p.Value, "unit") ?? "Não informada", variante, Texto(p.Value, "updated"), result);
    }
    private static void LerCampos(JsonElement value, string fornecedor, string moeda, string variante, string? data, List<TcgPreco> result)
    {
        foreach (var p in value.EnumerateObject())
        {
            if (p.Name is "unit" or "updated" or "productId" or "idProduct" || p.Name.EndsWith("Id", StringComparison.Ordinal)) continue;
            if (p.Value.ValueKind == JsonValueKind.Object) LerCampos(p.Value, fornecedor, moeda, variante + " / " + p.Name, data, result);
            else if (p.Value.ValueKind == JsonValueKind.Number && p.Value.TryGetDecimal(out var numero)) result.Add(new(fornecedor, moeda, variante, p.Name, numero, data));
            else if (p.Value.ValueKind == JsonValueKind.Null) result.Add(new(fornecedor, moeda, variante, p.Name, null, data));
        }
    }
    public static string? Texto(JsonElement root, string property) => root.ValueKind == JsonValueKind.Object && root.TryGetProperty(property, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;
}
