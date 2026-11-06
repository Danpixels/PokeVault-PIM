using System.Data;
using Microsoft.Data.SqlClient;
using PokeVault.Web.Domain;
using PokeVault.Web.Models;

namespace PokeVault.Web.Infrastructure;

public sealed partial class SqlLojaRepository
{
    private const string UsuariosGestaoSql = """
        SELECT u.UsuarioId,u.Nome,u.Email,p.Codigo,u.Ativo,s.Saldo,s.Versao
        FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
        LEFT JOIN dbo.SaldoCliente s ON s.UsuarioId=u.UsuarioId
        WHERE p.Codigo=@Perfil
        """;
    private static UsuarioGestao LerUsuarioGestao(SqlDataReader r) => new(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3),
        r.GetBoolean(4), r.IsDBNull(5) ? null : r.GetDecimal(5), r.IsDBNull(6) ? null : Convert.ToBase64String((byte[])r.GetValue(6)));
    public Task<IReadOnlyList<UsuarioGestao>> Usuarios(string perfil, int pagina, CancellationToken ct) =>
        Executar<IReadOnlyList<UsuarioGestao>>(UsuariosGestaoSql + " ORDER BY u.UsuarioId OFFSET @Offset ROWS FETCH NEXT 50 ROWS ONLY", CommandType.Text,
            p => { p.Add("@Perfil", SqlDbType.NVarChar, 20).Value = perfil; p.Add("@Offset", SqlDbType.Int).Value = (pagina - 1) * 50; }, async r =>
            { List<UsuarioGestao> lista = []; while (await r.ReadAsync(ct)) lista.Add(LerUsuarioGestao(r)); return lista; }, ct);
    public Task<UsuarioGestao?> UsuarioGestao(int id, string perfil, CancellationToken ct) =>
        Executar<UsuarioGestao?>(UsuariosGestaoSql + " AND u.UsuarioId=@Id", CommandType.Text,
            p => { p.Add("@Perfil", SqlDbType.NVarChar, 20).Value = perfil; p.Add("@Id", SqlDbType.Int).Value = id; },
            async r => await r.ReadAsync(ct) ? LerUsuarioGestao(r) : null, ct);
    public Task<int> CriarUsuario(int administradorId, string nome, string email, string normalizado, string hash, string perfil, CancellationToken ct) =>
        Executar("dbo.usp_CriarUsuario", CommandType.StoredProcedure, p =>
        {
            p.Add("@AdministradorId", SqlDbType.Int).Value = administradorId;
            p.Add("@Nome", SqlDbType.NVarChar, 120).Value = nome;
            p.Add("@Email", SqlDbType.NVarChar, 254).Value = email;
            p.Add("@EmailNormalizado", SqlDbType.NVarChar, 254).Value = normalizado;
            p.Add("@SenhaHash", SqlDbType.NVarChar, 512).Value = hash;
            p.Add("@PerfilCodigo", SqlDbType.NVarChar, 20).Value = perfil;
        }, async r => await r.ReadAsync(ct) ? r.GetInt32(0) : throw new InvalidOperationException("Cadastro sem resultado."), ct);
    public async Task DefinirSaldo(int administradorId, int clienteId, decimal saldo, string motivo, byte[] versao, CancellationToken ct) =>
        _ = await Executar("dbo.usp_DefinirSaldoVersionado", CommandType.StoredProcedure, p =>
        {
            p.Add("@AdministradorId", SqlDbType.Int).Value = administradorId;
            p.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
            Monetario(p, "@NovoSaldo", saldo);
            p.Add("@Motivo", SqlDbType.NVarChar, 300).Value = motivo;
            p.Add("@VersaoEsperada", SqlDbType.Binary, 8).Value = versao;
        }, async r => await r.ReadAsync(ct), ct);
    public async Task DefinirAtendenteAtivo(int administradorId, int usuarioId, bool ativo, CancellationToken ct) =>
        _ = await Executar("dbo.usp_DefinirUsuarioAtivo", CommandType.StoredProcedure, p =>
        {
            p.Add("@AdministradorId", SqlDbType.Int).Value = administradorId;
            p.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;
            p.Add("@Ativo", SqlDbType.Bit).Value = ativo;
        }, async r => await r.ReadAsync(ct), ct);
    public Task<IReadOnlyList<ColecaoGestao>> Colecoes(CancellationToken ct) =>
        Executar<IReadOnlyList<ColecaoGestao>>("SELECT ColecaoId,Codigo,Nome FROM dbo.Colecao ORDER BY ColecaoId", CommandType.Text,
            _ => { }, async r => { List<ColecaoGestao> lista = []; while (await r.ReadAsync(ct)) lista.Add(new(r.GetInt32(0), r.GetString(1), r.GetString(2))); return lista; }, ct);
    private const string CartasGestaoSql = """
        SELECT c.CartaId,c.ColecaoId,co.Nome,c.Numero,c.Nome,c.Raridade,c.Conservacao,c.Preco,c.QuantidadeEstoque,c.Ativo,c.Versao
        FROM dbo.Carta c JOIN dbo.Colecao co ON co.ColecaoId=c.ColecaoId
        """;
    private static CartaGestao LerCartaGestao(SqlDataReader r) => new(r.GetInt32(0), r.GetInt32(1), r.GetString(2), r.GetString(3), r.GetString(4), r.GetString(5),
        r.GetString(6), r.GetDecimal(7), r.GetInt32(8), r.GetBoolean(9), Convert.ToBase64String((byte[])r.GetValue(10)));
    public Task<IReadOnlyList<CartaGestao>> Cartas(int pagina, CancellationToken ct) =>
        Executar<IReadOnlyList<CartaGestao>>(CartasGestaoSql + " ORDER BY c.CartaId OFFSET @Offset ROWS FETCH NEXT 50 ROWS ONLY", CommandType.Text,
            p => p.Add("@Offset", SqlDbType.Int).Value = (pagina - 1) * 50, async r =>
            { List<CartaGestao> lista = []; while (await r.ReadAsync(ct)) lista.Add(LerCartaGestao(r)); return lista; }, ct);
    public Task<CartaGestao?> Carta(int id, CancellationToken ct) => Executar<CartaGestao?>(CartasGestaoSql + " WHERE c.CartaId=@Id", CommandType.Text,
        p => p.Add("@Id", SqlDbType.Int).Value = id, async r => await r.ReadAsync(ct) ? LerCartaGestao(r) : null, ct);
    public Task<int> CadastrarCarta(int responsavelId, CadastroCartaModel model, CancellationToken ct) =>
        Executar("dbo.usp_CadastrarCarta", CommandType.StoredProcedure, p =>
        {
            p.Add("@ResponsavelId", SqlDbType.Int).Value = responsavelId;
            p.Add("@ColecaoId", SqlDbType.Int).Value = model.ColecaoId;
            p.Add("@Numero", SqlDbType.NVarChar, 20).Value = model.Numero.Trim();
            p.Add("@Nome", SqlDbType.NVarChar, 120).Value = model.Nome.Trim();
            p.Add("@Raridade", SqlDbType.NVarChar, 50).Value = model.Raridade.Trim();
            p.Add("@Conservacao", SqlDbType.NVarChar, 30).Value = model.Conservacao;
            Monetario(p, "@Preco", model.Preco!.Value);
            p.Add("@QuantidadeInicial", SqlDbType.Int).Value = model.QuantidadeInicial!.Value;
        }, async r => await r.ReadAsync(ct) ? r.GetInt32(0) : throw new InvalidOperationException("Cadastro sem resultado."), ct);
    public async Task AjustarCarta(int responsavelId, int cartaId, AjusteCartaModel model, byte[] versao, CancellationToken ct) =>
        _ = await Executar("dbo.usp_AjustarCartaVersionado", CommandType.StoredProcedure, p =>
        {
            p.Add("@ResponsavelId", SqlDbType.Int).Value = responsavelId;
            p.Add("@CartaId", SqlDbType.Int).Value = cartaId;
            Monetario(p, "@Preco", model.Preco!.Value);
            p.Add("@NovoEstoque", SqlDbType.Int).Value = model.NovoEstoque!.Value;
            p.Add("@Ativo", SqlDbType.Bit).Value = model.Ativo!.Value;
            p.Add("@Motivo", SqlDbType.NVarChar, 300).Value = string.IsNullOrWhiteSpace(model.Motivo) ? "Ajuste de preço/estoque pelo catálogo" : model.Motivo.Trim();
            p.Add("@VersaoEsperada", SqlDbType.Binary, 8).Value = versao;
        }, async r => await r.ReadAsync(ct), ct);
    private static void Monetario(SqlParameterCollection parametros, string nome, decimal valor)
    {
        var parametro = parametros.Add(nome, SqlDbType.Decimal); parametro.Precision = 12; parametro.Scale = 2; parametro.Value = valor;
    }
}

