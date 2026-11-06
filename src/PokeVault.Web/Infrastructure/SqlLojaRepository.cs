using System.Data;
using Microsoft.Data.SqlClient;
using PokeVault.Web.Application;
using PokeVault.Web.Domain;

namespace PokeVault.Web.Infrastructure;

public sealed partial class SqlLojaRepository(IConfiguration config, TcgLocalImages? images = null) : ILojaRepository, IGestaoRepository, ITcgDexStore
{
    private SqlConnection Conexao() => new(config.GetConnectionString("PokeVault")
        ?? throw new InvalidOperationException("Configure ConnectionStrings:PokeVault."));

    private async Task<T> Executar<T>(string sql, CommandType tipo, Action<SqlParameterCollection> parametros,
        Func<SqlDataReader, Task<T>> ler, CancellationToken ct)
    {
        try
        {
            await using var conexao = Conexao();
            await conexao.OpenAsync(ct);
            await using var comando = new SqlCommand(sql, conexao) { CommandType = tipo, CommandTimeout = 30 };
            parametros(comando.Parameters);
            await using var reader = await comando.ExecuteReaderAsync(ct);
            return await ler(reader);
        }
        catch (SqlException ex) when (ex.Number is >= 51020 and <= 51023)
        {
            throw new RegraException("TCGDEX_" + ex.Number, ex.Number switch
            {
                51020 => "Dados TCGdex inválidos ou fora da coleção151.",
                51021 => "Importação TCGdex não encontrada.",
                51022 => "Identidade da oferta diverge da referência; vínculo não aplicado.",
                _ => "Chave de importação já utilizada com outro manifesto."
            }, ex.Number == 51020 ? 400 : ex.Number == 51021 ? 404 : 409);
        }
        catch (SqlException ex) when (ex.Number is >= 51000 and <= 51012)
        {
            var erros = new (string Codigo, string Mensagem, int Status)[]
            {
                ("ACESSO_NEGADO", "Operação não permitida.", 403),
                ("USUARIO_INATIVO_OU_INVALIDO", "Conta inativa ou inválida.", 403),
                ("DADOS_INVALIDOS", "Revise os dados enviados.", 400),
                ("COMPRA_NAO_ENCONTRADA", "Compra não encontrada.", 404),
                ("CHAVE_REUTILIZADA", "Esta chave já foi usada para outra seleção. Faça uma nova confirmação.", 409),
                ("CARRINHO_ALTERADO", "O carrinho mudou. Revise as quantidades e confirme novamente.", 409),
                ("CARTA_INDISPONIVEL", "Uma carta selecionada está indisponível.", 409),
                ("PRECO_ALTERADO", "O preço mudou. Recarregue o carrinho e confirme o novo valor.", 409),
                ("ESTOQUE_INSUFICIENTE", "Estoque insuficiente para a seleção.", 409),
                ("SALDO_INSUFICIENTE", "Saldo fictício insuficiente.", 409),
                ("CADASTRO_DUPLICADO", "Cadastro já existente.", 409),
                ("ADMINISTRADOR_FIXO", "O administrador fixo não pode ser desativado.", 409),
                ("VERSAO_DESATUALIZADA", "Os dados mudaram desde a abertura do formulário. Recarregue e confirme novamente.", 409)
            };
            var erro = erros[ex.Number - 51000];
            throw new RegraException(erro.Codigo, erro.Mensagem, erro.Status);
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627 or 1205)
        {
            throw new RegraException(ex.Number == 1205 ? "CONCORRENCIA" : "CADASTRO_DUPLICADO",
                "Conflito de operação. Se for uma compra, repita a mesma chave e seleção.");
        }
        catch (SqlException)
        {
            throw new RegraException("FALHA_SQL", "Não foi possível confirmar o resultado no banco. Para compra, mantenha a mesma chave e seleção ao repetir.", 503);
        }
    }

    private const string UsuarioSql = "SELECT u.UsuarioId,u.Nome,u.Email,u.SenhaHash,p.Codigo,u.Ativo FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId ";
    private static Usuario LerUsuario(SqlDataReader r) => new(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetString(4), r.GetBoolean(5));
    public Task<Usuario?> UsuarioPorEmail(string email, CancellationToken ct) => Executar<Usuario?>(UsuarioSql + "WHERE u.EmailNormalizado=@Email", CommandType.Text,
        p => p.Add("@Email", SqlDbType.NVarChar, 254).Value = email, async r => await r.ReadAsync(ct) ? LerUsuario(r) : null, ct);
    public Task<Usuario?> UsuarioPorId(int id, CancellationToken ct) => Executar<Usuario?>(UsuarioSql + "WHERE u.UsuarioId=@Id", CommandType.Text,
        p => p.Add("@Id", SqlDbType.Int).Value = id, async r => await r.ReadAsync(ct) ? LerUsuario(r) : null, ct);

    public Task<PaginaCatalogo> Catalogo(string? nome, string? colecao, string? conservacao, int pagina, CancellationToken ct) => Executar(
        """
        SELECT c.CartaId,c.Nome,co.Nome,c.Numero,c.Conservacao,c.Preco,c.QuantidadeEstoque,COUNT(*) OVER() AS Total,
        CASE WHEN TRIM(c.Nome) COLLATE Latin1_General_100_CI_AS=TRIM(r.Nome) COLLATE Latin1_General_100_CI_AS AND TRY_CONVERT(int,c.Numero)=TRY_CONVERT(int,r.Numero) THEN r.CardId END,r.Idioma
        FROM dbo.Carta c JOIN dbo.Colecao co ON co.ColecaoId=c.ColecaoId
        LEFT JOIN dbo.VinculoCartaTCGdex v ON v.CartaId=c.CartaId
        LEFT JOIN dbo.ReferenciaTCGdex r ON r.ReferenciaTCGdexId=v.ReferenciaTCGdexId
        WHERE c.Ativo=1 AND (@Nome='' OR CHARINDEX(@Nome,c.Nome)>0)
        AND (@Colecao='' OR CHARINDEX(@Colecao,co.Nome)>0 OR co.Codigo=@Colecao)
        AND (@Conservacao='' OR c.Conservacao=@Conservacao)
        ORDER BY c.CartaId OFFSET @Offset ROWS FETCH NEXT 12 ROWS ONLY
        """, CommandType.Text, p =>
        {
            p.Add("@Nome", SqlDbType.NVarChar, 120).Value = nome ?? "";
            p.Add("@Colecao", SqlDbType.NVarChar, 120).Value = colecao ?? "";
            p.Add("@Conservacao", SqlDbType.NVarChar, 30).Value = conservacao ?? "";
            p.Add("@Offset", SqlDbType.Int).Value = (pagina - 1) * 12;
        }, async r =>
        {
            List<Oferta> ofertas = []; int total = 0;
            while (await r.ReadAsync(ct))
            {
                var refId = r.IsDBNull(8) ? null : r.GetString(8);
                var idioma = r.IsDBNull(9) ? null : r.GetString(9);
                ofertas.Add(new(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetString(4), r.GetDecimal(5), r.GetInt32(6), images?.Resolver(refId, idioma).Url, refId));
                total = r.GetInt32(7);
            }
            return new PaginaCatalogo(ofertas, pagina, total, 12);
        }, ct);

    public async Task<Carrinho> Carrinho(int clienteId, CancellationToken ct)
    {
        var saldo = await Executar("SELECT Saldo FROM dbo.SaldoCliente WHERE UsuarioId=@Id", CommandType.Text,
            p => p.Add("@Id", SqlDbType.Int).Value = clienteId,
            async r => await r.ReadAsync(ct) ? r.GetDecimal(0) : 0m, ct);
        var itens = await Executar<IReadOnlyList<ItemCarrinho>>(
            """
            SELECT c.CartaId,c.Nome,c.Conservacao,i.Quantidade,c.Preco,c.QuantidadeEstoque,co.Nome,c.Numero,c.Idioma,c.Acabamento,r.CardId,r.Idioma
            FROM dbo.Carrinho ca JOIN dbo.ItemCarrinho i ON i.CarrinhoId=ca.CarrinhoId
            JOIN dbo.Carta c ON c.CartaId=i.CartaId JOIN dbo.Colecao co ON co.ColecaoId=c.ColecaoId
            LEFT JOIN dbo.VinculoCartaTCGdex v ON v.CartaId=c.CartaId LEFT JOIN dbo.ReferenciaTCGdex r ON r.ReferenciaTCGdexId=v.ReferenciaTCGdexId
            WHERE ca.UsuarioId=@Id ORDER BY c.CartaId
            """, CommandType.Text, p => p.Add("@Id", SqlDbType.Int).Value = clienteId,
            async r =>
            {
                List<ItemCarrinho> lista = [];
                while (await r.ReadAsync(ct)) lista.Add(new(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetInt32(3), r.GetDecimal(4), r.GetInt32(5),r.GetString(6),r.GetString(7),r.GetString(8),r.GetString(9),images?.Resolver(r.IsDBNull(10)?null:r.GetString(10),r.IsDBNull(11)?null:r.GetString(11)).Url));
                return lista;
            }, ct);
        return new(saldo, itens);
    }

    public async Task AlterarItem(int clienteId, int ofertaId, int quantidade, CancellationToken ct) =>
        _ = await Executar("dbo.usp_DefinirItemCarrinho", CommandType.StoredProcedure, p =>
        {
            p.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
            p.Add("@CartaId", SqlDbType.Int).Value = ofertaId;
            p.Add("@Quantidade", SqlDbType.Int).Value = quantidade;
        }, async r => await r.ReadAsync(ct), ct);

    public Task<ResultadoCompra> Comprar(int clienteId, Guid chave, IReadOnlyList<ItemSelecionado> itens, CancellationToken ct) =>
        Executar("dbo.usp_ConcluirCompra", CommandType.StoredProcedure, p =>
        {
            p.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
            p.Add("@ChaveOperacao", SqlDbType.UniqueIdentifier).Value = chave;
            var tabela = new DataTable();
            tabela.Columns.Add("CartaId", typeof(int));
            tabela.Columns.Add("Quantidade", typeof(int));
            tabela.Columns.Add("PrecoConfirmado", typeof(decimal));
            foreach (var item in itens) tabela.Rows.Add(item.OfertaId, item.Quantidade, item.PrecoConfirmado);
            var tvp = p.Add("@Itens", SqlDbType.Structured);
            tvp.TypeName = "dbo.ItemCompraSelecionadoType"; tvp.Value = tabela;
        }, r => LerResultado(r, ct), ct);

    public Task<ResultadoCompra> Cancelar(long compraId, int administradorId, string motivo, CancellationToken ct) =>
        Executar("dbo.usp_CancelarCompra", CommandType.StoredProcedure, p =>
        {
            p.Add("@CompraId", SqlDbType.Int).Value = checked((int)compraId);
            p.Add("@AdministradorId", SqlDbType.Int).Value = administradorId;
            p.Add("@Motivo", SqlDbType.NVarChar, 300).Value = motivo;
        }, r => LerResultado(r, ct), ct);

    private static async Task<ResultadoCompra> LerResultado(SqlDataReader r, CancellationToken ct)
    {
        if (!await r.ReadAsync(ct)) throw new InvalidOperationException("Procedure não retornou resultado.");
        return new(r.GetInt32(r.GetOrdinal("CompraId")), r.GetString(r.GetOrdinal("Status")), r.GetDecimal(r.GetOrdinal("Total")),
            r.GetDecimal(r.GetOrdinal("SaldoAtual")), r.GetBoolean(r.GetOrdinal("Repetida")));
    }

    private static CompraResumo LerResumo(SqlDataReader r) => new(r.GetInt32(0), r.GetString(1), r.GetDecimal(2), DateTime.SpecifyKind(r.GetDateTime(3), DateTimeKind.Utc), r.GetInt32(4));
    public Task<IReadOnlyList<CompraResumo>> Historico(int usuarioId, bool administrador, CancellationToken ct) =>
        Executar<IReadOnlyList<CompraResumo>>("SELECT TOP (100) CompraId,Status,Total,CriadaEmUtc,ClienteId FROM dbo.Compra WHERE (@Admin=1 OR ClienteId=@Id) ORDER BY CompraId DESC", CommandType.Text,
            p => { p.Add("@Id", SqlDbType.Int).Value = usuarioId; p.Add("@Admin", SqlDbType.Bit).Value = administrador; }, async r =>
            {
                List<CompraResumo> lista = []; while (await r.ReadAsync(ct)) lista.Add(LerResumo(r)); return lista;
            }, ct);

    public async Task<CompraDetalhe?> Compra(long compraId, int usuarioId, bool administrador, CancellationToken ct)
    {
        if (compraId is <= 0 or > int.MaxValue) return null;
        var cabecalho = await Executar<(CompraResumo Resumo, string? Motivo)?>(
            "SELECT CompraId,Status,Total,CriadaEmUtc,ClienteId,MotivoCancelamento FROM dbo.Compra WHERE CompraId=@CompraId AND (@Admin=1 OR ClienteId=@Id)", CommandType.Text,
            p => { p.Add("@CompraId", SqlDbType.Int).Value = (int)compraId; p.Add("@Id", SqlDbType.Int).Value = usuarioId; p.Add("@Admin", SqlDbType.Bit).Value = administrador; },
            async r => await r.ReadAsync(ct) ? (LerResumo(r), r.IsDBNull(5) ? null : r.GetString(5)) : null, ct);
        if (cabecalho is null) return null;
        var itens = await Executar<IReadOnlyList<ItemComprado>>(
            "SELECT CartaId,NomeCartaSnapshot,ConservacaoSnapshot,Quantidade,PrecoUnitario,IdiomaSnapshot,AcabamentoSnapshot,ColecaoSnapshot,NumeroSnapshot FROM dbo.ItemCompra WHERE CompraId=@CompraId ORDER BY CartaId", CommandType.Text,
            p => p.Add("@CompraId", SqlDbType.Int).Value = (int)compraId, async r =>
            {
                List<ItemComprado> lista = []; while (await r.ReadAsync(ct)) lista.Add(new(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetInt32(3), r.GetDecimal(4),r.IsDBNull(5)?null:r.GetString(5),r.IsDBNull(6)?null:r.GetString(6),r.IsDBNull(7)?null:r.GetString(7),r.IsDBNull(8)?null:r.GetString(8))); return lista;
            }, ct);
        return new(cabecalho.Value.Resumo, itens, cabecalho.Value.Motivo);
    }

    public async Task Provisionar(string email, string nome, string hash, string perfil, CancellationToken ct)
    {
        var adminId = await Executar("SELECT UsuarioId FROM dbo.Usuario WHERE AdministradorFixo=1 AND Ativo=1", CommandType.Text,
            _ => { }, async r => await r.ReadAsync(ct) ? r.GetInt32(0) : throw new InvalidOperationException("Administrador fixo ausente."), ct);
        _ = await Executar("dbo.usp_CriarUsuario", CommandType.StoredProcedure, p =>
        {
            p.Add("@AdministradorId", SqlDbType.Int).Value = adminId;
            p.Add("@Nome", SqlDbType.NVarChar, 120).Value = nome;
            p.Add("@Email", SqlDbType.NVarChar, 254).Value = email;
            p.Add("@EmailNormalizado", SqlDbType.NVarChar, 254).Value = LojaService.NormalizarEmail(email);
            p.Add("@SenhaHash", SqlDbType.NVarChar, 512).Value = hash;
            p.Add("@PerfilCodigo", SqlDbType.NVarChar, 20).Value = perfil;
        }, async r => await r.ReadAsync(ct), ct);
    }
}


