using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using PokeVault.Web.Domain;

namespace PokeVault.Web.Infrastructure;

public sealed partial class SqlLojaRepository
{
    private const string RefSelect = """
        SELECT ReferenciaTCGdexId,CardId,Numero,Nome,Raridade,Idioma,ImagemUrl,SnapshotJson,PrecosJson,SnapshotColetadoEmUtc,OrigemUrl,Detalhada,PrecosColetadosEmUtc
        """;
    private TcgReferencia LerReferencia(SqlDataReader r)
    {
        var image = images?.Resolver(r.GetString(1), r.GetString(5)) ?? (null, null);
        return new(r.GetString(1), r.GetString(2), r.GetString(3), r.IsDBNull(4) ? null : r.GetString(4), r.GetString(5), image.Item1, r.IsDBNull(6) ? null : r.GetString(6), image.Item2,
            r.GetString(7), r.IsDBNull(8) ? "{}" : r.GetString(8), DateTime.SpecifyKind(r.GetDateTime(9), DateTimeKind.Utc), r.GetString(10), r.GetInt32(0), r.GetBoolean(11), r.IsDBNull(12) ? null : DateTime.SpecifyKind(r.GetDateTime(12), DateTimeKind.Utc));
    }
    public Task<TcgPagina> Listar(string? nome, string? raridade, int pagina, CancellationToken ct) => Executar(
        RefSelect + ",COUNT(*) OVER() FROM dbo.vw_BibliotecaTCGdex151 WHERE (@Nome='' OR CHARINDEX(@Nome,Nome)>0 OR Numero=@Nome) AND (@Raridade='' OR Raridade=@Raridade) ORDER BY Numero OFFSET @Offset ROWS FETCH NEXT 24 ROWS ONLY", CommandType.Text,
        p => { p.Add("@Nome", SqlDbType.NVarChar, 120).Value = nome ?? ""; p.Add("@Raridade", SqlDbType.NVarChar, 100).Value = raridade ?? ""; p.Add("@Offset", SqlDbType.Int).Value = (pagina - 1) * 24; }, async r =>
        { List<TcgReferencia> itens = []; int total = 0; while (await r.ReadAsync(ct)) { itens.Add(LerReferencia(r)); total = r.GetInt32(13); } return new TcgPagina(itens, pagina, total); }, ct);
    public Task<TcgReferencia?> Obter(string id, CancellationToken ct) => Executar<TcgReferencia?>(RefSelect + " FROM dbo.vw_BibliotecaTCGdex151 WHERE CardId=@Id", CommandType.Text,
        p => p.Add("@Id", SqlDbType.NVarChar, 30).Value = id, async r => await r.ReadAsync(ct) ? LerReferencia(r) : null, ct);
    public Task<TcgImportStatus?> UltimaImportacao(CancellationToken ct) => Executar<TcgImportStatus?>(
        """
        SELECT TOP(1) i.ImportacaoId,i.Status,i.CriadaEmUtc,i.AtualizadaEmUtc,i.UltimaMensagemFalha,
        (SELECT COUNT(*) FROM dbo.ItemImportacaoTCGdex151 x WHERE x.ImportacaoId=i.ImportacaoId AND x.Estado<>N'PENDENTE'),
        (SELECT COUNT(*) FROM dbo.ItemImportacaoTCGdex151 x WHERE x.ImportacaoId=i.ImportacaoId AND x.Estado=N'PENDENTE')
        FROM dbo.ImportacaoTCGdex151 i ORDER BY ImportacaoId DESC
        """, CommandType.Text, _ => { }, async r => await r.ReadAsync(ct) ? new(false, r.GetInt32(0), r.GetInt32(5), r.GetInt32(5), r.GetInt32(6), 0, 0, false,
            "Histórico SQL: " + r.GetString(1) + (r.IsDBNull(4) ? "" : ". " + r.GetString(4)), DateTime.SpecifyKind(r.GetDateTime(2), DateTimeKind.Utc), DateTime.SpecifyKind(r.GetDateTime(3), DateTimeKind.Utc)) : null, ct);
    public Task<long> Iniciar(int ator, TcgJson dados, Guid chave, CancellationToken ct) => Executar<long>("dbo.usp_IniciarImportacaoTCGdex151", CommandType.StoredProcedure,
        p => { p.Add("@ResponsavelId", SqlDbType.Int).Value = ator; p.Add("@ChaveImportacao", SqlDbType.UniqueIdentifier).Value = chave; p.Add("@Idioma", SqlDbType.NVarChar, 2).Value = dados.Idioma;
            p.Add("@SetJson", SqlDbType.NVarChar, -1).Value = dados.Json; p.Add("@ColetadoEmUtc", SqlDbType.DateTime2).Value = dados.ColetadoEmUtc; },
        async r => await r.ReadAsync(ct) ? r.GetInt32(0) : throw new InvalidOperationException("Importação sem resultado."), ct);
    public async Task SalvarCarta(long importacao, int ator, TcgReferencia carta, CancellationToken ct)
    {
        using var raw = JsonDocument.Parse(carta.SnapshotJson);
        var lote = JsonSerializer.Serialize(new { versao = 1, cartas = new[] { new { coletadoEmUtc = carta.ImportadoEmUtc, snapshot = raw.RootElement } } });
        _ = await Executar("dbo.usp_AtualizarReferenciasTCGdex151", CommandType.StoredProcedure, p =>
        { p.Add("@ResponsavelId", SqlDbType.Int).Value = ator; p.Add("@ImportacaoId", SqlDbType.Int).Value = checked((int)importacao); p.Add("@LoteJson", SqlDbType.NVarChar, -1).Value = lote; }, async r => await r.ReadAsync(ct), ct);
    }
    public async Task Finalizar(long importacao, int ator, TcgImportStatus status, CancellationToken ct)
    {
        if (status.Ausentes == 0 && status.Processados == 207) return;
        _ = await Executar("dbo.usp_RegistrarFalhaTCGdex151", CommandType.StoredProcedure, p =>
        {
            p.Add("@ResponsavelId", SqlDbType.Int).Value = ator; p.Add("@ImportacaoId", SqlDbType.Int).Value = checked((int)importacao);
            p.Add("@Codigo", SqlDbType.NVarChar, 80).Value = "IMPORTACAO_INTERROMPIDA_OU_PARCIAL";
            p.Add("@Mensagem", SqlDbType.NVarChar, 300).Value = status.Mensagem.Length > 300 ? status.Mensagem[..300] : status.Mensagem;
        }, async r => await r.ReadAsync(ct), ct);
    }
    public async Task VincularOriginais(int ator, CancellationToken ct)
    {
        var pares = await Executar<List<(int Carta, int Ref)>>(
            """
            SELECT c.CartaId,r.ReferenciaTCGdexId FROM dbo.Carta c JOIN dbo.Colecao co ON co.ColecaoId=c.ColecaoId
            JOIN dbo.vw_BibliotecaTCGdex151 r ON TRY_CONVERT(int,c.Numero)=TRY_CONVERT(int,r.Numero)
             AND TRIM(c.Nome) COLLATE Latin1_General_100_CI_AS=TRIM(r.Nome) COLLATE Latin1_General_100_CI_AS
            WHERE co.Codigo=N'MEW-151' AND r.Detalhada=1 AND
              ((c.Numero=N'025' AND c.Nome=N'Pikachu' AND c.Conservacao IN(N'NEAR_MINT',N'SLIGHTLY_PLAYED'))
               OR (c.Numero=N'004' AND c.Nome=N'Charmander' AND c.Conservacao=N'NEAR_MINT'))
              AND NOT EXISTS(SELECT 1 FROM dbo.VinculoCartaTCGdex v WHERE v.CartaId=c.CartaId)
            """, CommandType.Text, _ => { }, async r => { List<(int, int)> rows = []; while (await r.ReadAsync(ct)) rows.Add((r.GetInt32(0), r.GetInt32(1))); return rows; }, ct);
        foreach (var par in pares)
            _ = await Executar("dbo.usp_VincularOfertaTCGdex151", CommandType.StoredProcedure,
                p => { p.Add("@ResponsavelId", SqlDbType.Int).Value = ator; p.Add("@CartaId", SqlDbType.Int).Value = par.Carta; p.Add("@ReferenciaTCGdexId", SqlDbType.Int).Value = par.Ref; }, async r => await r.ReadAsync(ct), ct);
    }
    public Task<int> CadastrarOfertaReferencia(int ator, int referenciaId, string conservacao, decimal preco, int quantidade, CancellationToken ct) =>
        Executar("dbo.usp_CadastrarOfertaTCGdex151", CommandType.StoredProcedure, p =>
        {
            p.Add("@ResponsavelId", SqlDbType.Int).Value = ator; p.Add("@ReferenciaTCGdexId", SqlDbType.Int).Value = referenciaId;
            p.Add("@Conservacao", SqlDbType.NVarChar, 30).Value = conservacao; Monetario(p, "@Preco", preco); p.Add("@QuantidadeInicial", SqlDbType.Int).Value = quantidade;
        }, async r => await r.ReadAsync(ct) ? r.GetInt32(0) : throw new InvalidOperationException("Oferta sem resultado."), ct);
}
