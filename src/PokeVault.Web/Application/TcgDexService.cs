using System.Text.Json;
using PokeVault.Web.Domain;
using PokeVault.Web.Infrastructure;
using PokeVault.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace PokeVault.Web.Application;

public interface ITcgDexStore
{
    Task<TcgPagina> Listar(string? nome, string? raridade, int pagina, CancellationToken ct);
    Task<TcgReferencia?> Obter(string id, CancellationToken ct);
    Task<TcgImportStatus?> UltimaImportacao(CancellationToken ct);
    Task<long> Iniciar(int ator, TcgJson dados, Guid chave, CancellationToken ct);
    Task SalvarCarta(long importacao, int ator, TcgReferencia carta, CancellationToken ct);
    Task Finalizar(long importacao, int ator, TcgImportStatus status, CancellationToken ct);
    Task VincularOriginais(int ator, CancellationToken ct);
    Task<int> CadastrarOfertaReferencia(int ator, int referenciaId, string conservacao, decimal preco, int quantidade, CancellationToken ct);
}

public sealed class TcgDexService(ITcgDexStore store, ILojaRepository usuarios)
{
    public async Task Autorizar(int ator, CancellationToken ct)
    {
        var user = await usuarios.UsuarioPorId(ator, ct);
        if (user is not { Ativo: true } || user.Perfil is not ("ADMINISTRADOR" or "ATENDENTE")) throw new RegraException("ACESSO_NEGADO", "Operação não permitida.", 403);
    }
    public async Task<TcgPagina> Listar(int ator, string? nome, string? raridade, int pagina, CancellationToken ct)
    {
        await Autorizar(ator, ct);
        if (nome?.Length > 120 || raridade?.Length > 50 || pagina is < 1 or > 10000) throw new RegraException("FILTROS_INVALIDOS", "Revise os filtros e a página.", 400);
        return await store.Listar(nome?.Trim(), raridade?.Trim(), pagina, ct);
    }
    public async Task<TcgReferencia> Obter(int ator, string id, CancellationToken ct)
    {
        await Autorizar(ator, ct);
        if (!TcgDexClient.IdValido(id)) throw new RegraException("REFERENCIA_INVALIDA", "Referência fora da coleção151.", 400);
        return await store.Obter(id, ct) ?? throw new RegraException("REFERENCIA_NAO_IMPORTADA", "Esta referência ainda não está no snapshot local.", 404);
    }
    public async Task<int> CadastrarOferta(int ator, string id, TcgOfertaModel model, CancellationToken ct)
    {
        var referencia = await Obter(ator, id, ct);
        if (!referencia.Detalhada) throw new RegraException("REFERENCIA_INCOMPLETA", "Aguarde importar os detalhes desta referência.", 409);
        if (!Validator.TryValidateObject(model, new ValidationContext(model), null, true) || !Conservacoes.Todas.Contains(model.Conservacao)
            || decimal.Round(model.Preco!.Value, 2) != model.Preco.Value)
            throw new RegraException("DADOS_INVALIDOS", "Informe conservação, preçoBRL com duas casas e quantidade inteira válida.", 400);
        return await store.CadastrarOfertaReferencia(ator, referencia.ReferenciaId, model.Conservacao, model.Preco.Value, model.QuantidadeInicial!.Value, ct);
    }
}

public sealed class TcgDexImportador(IServiceScopeFactory scopes, IHostApplicationLifetime lifetime, IWebHostEnvironment env, IConfiguration config)
{
    private readonly object sincronizacao = new();
    private bool ocupado;
    private TcgImportStatus estado = CarregarEstado(env, config);
    public TcgImportStatus Estado { get { lock (sincronizacao) return estado; } }
    public bool Iniciar(int ator, bool forcar)
    {
        lock (sincronizacao)
        {
            if (ocupado) return false;
            ocupado = true;
            estado = new(true, null, 0, 0, 0, 0, 0, false, "Preparando importação local de sv03.5.", DateTime.UtcNow, null);
        }
        _ = Task.Run(() => Executar(ator, forcar, lifetime.ApplicationStopping));
        return true;
    }
    private void Atualizar(Func<TcgImportStatus, TcgImportStatus> mudar) { lock (sincronizacao) estado = mudar(estado); }
    private async Task Executar(int ator, bool forcar, CancellationToken ct)
    {
        FileStream? bloqueio = null;
        using var scope = scopes.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<ITcgDexStore>();
        var client = scope.ServiceProvider.GetRequiredService<TcgDexClient>();
        long? logId = null;
        try
        {
            var db = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(config.GetConnectionString("PokeVault")).InitialCatalog;
            var root = Path.Combine(env.ContentRootPath, ".local", "tcgdex151", db); Directory.CreateDirectory(root);
            try { bloqueio = new FileStream(Path.Combine(root, "import.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch (IOException) { throw new RegraException("IMPORTACAO_OCUPADA", "Outro processo já está importando esta base."); }
            await scope.ServiceProvider.GetRequiredService<TcgDexService>().Autorizar(ator, ct);
            Atualizar(s => s with { Mensagem = "Consultando coleção151; dados locais válidos serão preservados." });
            var set = await client.Colecao(ct);
            if (set is null) throw new RegraException("TCGDEX_INDISPONIVEL", "TCGdex indisponível e sem cache da coleção. Snapshot SQL anterior preservado.", 503);
            using var setDoc = JsonDocument.Parse(set.Json);
            var counts = setDoc.RootElement.GetProperty("cardCount");
            var oficiais = counts.GetProperty("official").GetInt32(); var total = counts.GetProperty("total").GetInt32();
            if (oficiais != 165 || total != 207) throw new RegraException("COLECAO_DIVERGENTE", "A contagem da coleção diverge de165/207. Importação interrompida sem limpar dados anteriores.");
            async Task<long> AbrirManifesto(TcgJson dados)
            {
                var file = Path.Combine(root, "manifesto-" + dados.Idioma + ".json");
                Manifesto? saved = null;
                try { if (File.Exists(file)) saved = JsonSerializer.Deserialize<Manifesto>(await File.ReadAllTextAsync(file, ct)); } catch (JsonException) { }
                var manifesto = saved is not null && saved.Dados.Json == dados.Json ? saved : new Manifesto(Guid.NewGuid(), dados);
                await File.WriteAllTextAsync(file, JsonSerializer.Serialize(manifesto), ct);
                return await store.Iniciar(ator, manifesto.Dados, manifesto.Chave, ct);
            }
            logId = await AbrirManifesto(set);
            var logs = new System.Collections.Concurrent.ConcurrentDictionary<string, long>(); logs[set.Idioma] = logId.Value;
            using var gate = new SemaphoreSlim(1);
            async Task<long> LogIdioma(string idioma)
            {
                if (logs.TryGetValue(idioma, out var id)) return id;
                await gate.WaitAsync(ct);
                try
                {
                    if (logs.TryGetValue(idioma, out id)) return id;
                    var alternate = await client.ColecaoIdioma(idioma, set.Offline, ct) ?? throw new InvalidDataException("Manifesto de fallback indisponível.");
                    id = await AbrirManifesto(alternate); logs[idioma] = id; return id;
                }
                finally { gate.Release(); }
            }
            Atualizar(s => s with { ImportacaoId = logId });
            var ids = setDoc.RootElement.GetProperty("cards").EnumerateArray().Select(c => c.GetProperty("id").GetString()!).Distinct().Order().ToArray();
            Atualizar(s => s with { ModoOffline = set.Offline, Mensagem = set.Offline ? "TCGdex indisponível/rede desabilitada: retomando exclusivamente cache local." : "Importação em andamento. Dados recentes reutilizam cache; até3 requisições simultâneas." });
            await Parallel.ForEachAsync(ids, new ParallelOptions { MaxDegreeOfParallelism = 3, CancellationToken = ct }, async (id, token) =>
            {
                try
                {
                    var doc = await client.Carta(id, forcar, set.Offline, token);
                    if (doc is null) { Atualizar(s => s with { Processados = s.Processados + 1, Ausentes = s.Ausentes + 1 }); return; }
                    using var json = JsonDocument.Parse(doc.Dados.Json);
                    var root = json.RootElement;
                    var carta = new TcgReferencia(id, root.GetProperty("localId").GetString()!, root.GetProperty("name").GetString()!, TcgPrecos.Texto(root, "rarity"), doc.Dados.Idioma,
                        doc.ImagemLocal, doc.ImagemRemota, doc.IdiomaImagem, doc.Dados.Json, root.TryGetProperty("pricing", out var precos) ? precos.GetRawText() : "{}", doc.Dados.ColetadoEmUtc, doc.Dados.Url);
                    using var cardScope = scopes.CreateScope();
                    var idiomaLog = await LogIdioma(doc.Dados.Idioma);
                    await cardScope.ServiceProvider.GetRequiredService<ITcgDexStore>().SalvarCarta(idiomaLog, ator, carta, token);
                    Atualizar(s => s with { Processados = s.Processados + 1, Importados = s.Importados + 1, ImagensLocais = s.ImagensLocais + (doc.ImagemLocal is null ? 0 : 1),
                        FallbackIngles = s.FallbackIngles + (doc.Dados.Idioma == "en" || doc.IdiomaImagem == "en" ? 1 : 0), ModoOffline = s.ModoOffline || doc.Dados.Offline });
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Atualizar(s => s with { Processados = s.Processados + 1, Ausentes = s.Ausentes + 1, Mensagem = "Algumas referências não puderam ser atualizadas; snapshot válido anterior mantido. Retome a importação." });
                }
            });
            await store.VincularOriginais(ator, ct);
            Atualizar(s => s with { EmExecucao = false, FimUtc = DateTime.UtcNow, Mensagem = s.Ausentes > 0 ? "Importação parcial; referências ausentes mantêm o snapshot anterior, se existente. Pode retomar." : s.ModoOffline ? "207 referências processadas com cache; API indisponível/rede desabilitada. Nenhuma oferta comercial alterada." : "207 referências processadas. Preços externos são informativos; estoque e preçoBRL preservados." });
        }
        catch (Exception ex)
        {
            Atualizar(s => s with { EmExecucao = false, FimUtc = DateTime.UtcNow, Mensagem = ex is RegraException regra ? regra.Message : "Importação interrompida. Cache e referências anteriores preservados; verifique conexão e retome." });
        }
        finally
        {
            if (logId is long id)
                try { await store.Finalizar(id, ator, Estado, CancellationToken.None); }
                catch { Atualizar(s => s with { Mensagem = s.Mensagem + " Não foi possível registrar o encerramento no SQL." }); }
            bloqueio?.Dispose();
            try
            {
                var path = EstadoPath(env, config); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await File.WriteAllTextAsync(path, JsonSerializer.Serialize(Estado));
            }
            catch (IOException) { }
            lock (sincronizacao) ocupado = false;
        }
    }
    private sealed record Manifesto(Guid Chave, TcgJson Dados);
    private static string EstadoPath(IWebHostEnvironment environment, IConfiguration configuration) => Path.Combine(environment.ContentRootPath, ".local", "tcgdex151",
        new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(configuration.GetConnectionString("PokeVault")).InitialCatalog, "estado.json");
    private static TcgImportStatus CarregarEstado(IWebHostEnvironment environment, IConfiguration configuration)
    {
        try
        {
            var path = EstadoPath(environment, configuration);
            if (File.Exists(path) && JsonSerializer.Deserialize<TcgImportStatus>(File.ReadAllText(path)) is TcgImportStatus saved)
                return saved with { EmExecucao = false, Mensagem = saved.EmExecucao ? "Processo anterior interrompido; retome a partir do cache local." : saved.Mensagem };
        }
        catch (Exception ex) when (ex is IOException or JsonException) { }
        return new(false, null, 0, 0, 0, 0, 0, false, "Nenhuma importação iniciada neste processo.", null, null);
    }
}
