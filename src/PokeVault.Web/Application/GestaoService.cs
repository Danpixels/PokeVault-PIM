using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using PokeVault.Web.Domain;
using PokeVault.Web.Models;

namespace PokeVault.Web.Application;

public interface IGestaoRepository
{
    Task<IReadOnlyList<UsuarioGestao>> Usuarios(string perfil, int pagina, CancellationToken ct);
    Task<UsuarioGestao?> UsuarioGestao(int id, string perfil, CancellationToken ct);
    Task<int> CriarUsuario(int administradorId, string nome, string email, string normalizado, string hash, string perfil, CancellationToken ct);
    Task DefinirSaldo(int administradorId, int clienteId, decimal saldo, string motivo, byte[] versao, CancellationToken ct);
    Task DefinirAtendenteAtivo(int administradorId, int usuarioId, bool ativo, CancellationToken ct);
    Task<IReadOnlyList<ColecaoGestao>> Colecoes(CancellationToken ct);

    Task<IReadOnlyList<CartaGestao>> Cartas(int pagina, CancellationToken ct);
    Task<CartaGestao?> Carta(int id, CancellationToken ct);
    Task<int> CadastrarCarta(int responsavelId, CadastroCartaModel model, CancellationToken ct);
    Task AjustarCarta(int responsavelId, int cartaId, AjusteCartaModel model, byte[] versao, CancellationToken ct);
}

public sealed class GestaoService(IGestaoRepository repository, ILojaRepository usuarios, IPasswordHasher<Usuario> hasher)
{
    public async Task<UsuarioGestao> Usuario(int ator, int id, string perfil, CancellationToken ct)
    {
        await Autorizar(ator, false, ct); ValidarPerfil(perfil);
        return await repository.UsuarioGestao(id, perfil, ct) ?? throw new RegraException("NAO_ENCONTRADO", "Conta não encontrada.", 404);
    }
    public async Task<CartaGestao> Carta(int ator, int id, CancellationToken ct)
    {
        await Autorizar(ator, true, ct);
        return await repository.Carta(id, ct) ?? throw new RegraException("NAO_ENCONTRADO", "Carta não encontrada.", 404);
    }
    public async Task<IReadOnlyList<UsuarioGestao>> ListarUsuarios(int ator, string perfil, int pagina, CancellationToken ct)
    {
        await Autorizar(ator, false, ct); ValidarPerfil(perfil); Pagina(pagina);
        return await repository.Usuarios(perfil, pagina, ct);
    }
    public async Task<int> CriarUsuario(int ator, string perfil, CadastroUsuarioModel model, CancellationToken ct)
    {
        await Autorizar(ator, false, ct); ValidarPerfil(perfil); Validar(model);
        var email = model.Email.Trim();
        if (!new EmailAddressAttribute().IsValid(email)) throw new RegraException("DADOS_INVALIDOS", "Email inválido.", 400);
        var usuario = new Usuario(0, model.Nome.Trim(), email, "", perfil, true);
        var hash = hasher.HashPassword(usuario, model.SenhaInicial);
        return await repository.CriarUsuario(ator, usuario.Nome, email, LojaService.NormalizarEmail(email), hash, perfil, ct);
    }
    public async Task DefinirSaldo(int ator, int clienteId, SaldoModel model, CancellationToken ct)
    {
        await Autorizar(ator, false, ct); Validar(model); Dinheiro(model.NovoSaldo!.Value);
        if (clienteId <= 0) throw new RegraException("DADOS_INVALIDOS", "Cliente inválido.", 400);
        await repository.DefinirSaldo(ator, clienteId, model.NovoSaldo.Value, model.Motivo.Trim(), Versao(model.VersaoEsperada), ct);
    }
    public async Task DefinirAtendenteAtivo(int ator, int usuarioId, AtivacaoModel model, CancellationToken ct)
    {
        await Autorizar(ator, false, ct); Validar(model);
        var usuario = await usuarios.UsuarioPorId(usuarioId, ct);
        if (usuario is null || usuario.Perfil != "ATENDENTE")
            throw new RegraException("ACESSO_NEGADO", "Esta operação permite somente ativar ou desativar atendentes.", 403);
        await repository.DefinirAtendenteAtivo(ator, usuarioId, model.Ativo!.Value, ct);
    }
    public async Task<IReadOnlyList<CartaGestao>> Cartas(int ator, int pagina, CancellationToken ct)
    {
        await Autorizar(ator, true, ct); Pagina(pagina); return await repository.Cartas(pagina, ct);
    }
    public async Task<IReadOnlyList<ColecaoGestao>> Colecoes(int ator, CancellationToken ct)
    {
        await Autorizar(ator, true, ct); return await repository.Colecoes(ct);
    }

    public async Task<int> CadastrarCarta(int ator, CadastroCartaModel model, CancellationToken ct)
    {
        await Autorizar(ator, true, ct); Validar(model); Dinheiro(model.Preco!.Value);
        if (!Conservacoes.Todas.Contains(model.Conservacao)) throw new RegraException("DADOS_INVALIDOS", "Conservação inválida.", 400);
        return await repository.CadastrarCarta(ator, model, ct);
    }
    public async Task AjustarCarta(int ator, int cartaId, AjusteCartaModel model, CancellationToken ct)
    {
        await Autorizar(ator, true, ct); Validar(model); Dinheiro(model.Preco!.Value);
        if (cartaId <= 0) throw new RegraException("DADOS_INVALIDOS", "Carta inválida.", 400);
        await repository.AjustarCarta(ator, cartaId, model, Versao(model.VersaoEsperada), ct);
    }
    private async Task Autorizar(int ator, bool catalogo, CancellationToken ct)
    {
        var usuario = await usuarios.UsuarioPorId(ator, ct);
        if (usuario is not { Ativo: true } || (usuario.Perfil != "ADMINISTRADOR" && !(catalogo && usuario.Perfil == "ATENDENTE")))
            throw new RegraException("ACESSO_NEGADO", "Operação não permitida.", 403);
    }
    private static void ValidarPerfil(string perfil)
    {
        if (perfil is not ("CLIENTE" or "ATENDENTE")) throw new RegraException("DADOS_INVALIDOS", "Perfil inválido.", 400);
    }
    private static void Pagina(int pagina)
    {
        if (pagina is < 1 or > 100000) throw new RegraException("DADOS_INVALIDOS", "Página inválida.", 400);
    }
    private static void Validar(object model)
    {
        List<ValidationResult> erros = [];
        if (!Validator.TryValidateObject(model, new ValidationContext(model), erros, true))
            throw new RegraException("DADOS_INVALIDOS", "Revise os campos obrigatórios e seus limites.", 400);
    }
    private static void Dinheiro(decimal valor)
    {
        if (decimal.Round(valor, 2) != valor) throw new RegraException("DADOS_INVALIDOS", "Use no máximo duas casas decimais.", 400);
    }
    private static byte[] Versao(string texto)
    {
        try { var bytes = Convert.FromBase64String(texto); if (bytes.Length == 8) return bytes; }
        catch (FormatException) { }
        throw new RegraException("VERSAO_INVALIDA", "Versão inválida. Recarregue a tela.", 400);
    }
}



