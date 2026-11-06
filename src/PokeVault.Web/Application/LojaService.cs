using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using PokeVault.Web.Domain;

namespace PokeVault.Web.Application;

public interface ILojaRepository
{
    Task<Usuario?> UsuarioPorEmail(string email, CancellationToken ct);
    Task<Usuario?> UsuarioPorId(int id, CancellationToken ct);
    Task<PaginaCatalogo> Catalogo(string? nome, string? colecao, string? conservacao, int pagina, CancellationToken ct);
    Task<Carrinho> Carrinho(int clienteId, CancellationToken ct);
    Task AlterarItem(int clienteId, int ofertaId, int quantidade, CancellationToken ct);

    Task<ResultadoCompra> Comprar(int clienteId, Guid chave, IReadOnlyList<ItemSelecionado> itens, CancellationToken ct);
    Task<IReadOnlyList<CompraResumo>> Historico(int usuarioId, bool administrador, CancellationToken ct);
    Task<CompraDetalhe?> Compra(long compraId, int usuarioId, bool administrador, CancellationToken ct);
    Task<ResultadoCompra> Cancelar(long compraId, int administradorId, string motivo, CancellationToken ct);
    Task Provisionar(string email, string nome, string hash, string perfil, CancellationToken ct);
}

public sealed class LojaService(ILojaRepository repository, IPasswordHasher<Usuario> hasher)
{
    public static string NormalizarEmail(string email) => email.Trim().ToUpperInvariant();

    public async Task<Usuario?> Autenticar(string email, string senha, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(senha) || email.Length > 254 || senha.Length > 256) return null;
        var usuario = await repository.UsuarioPorEmail(NormalizarEmail(email), ct);
        return usuario is { Ativo: true } && hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha) != PasswordVerificationResult.Failed
            ? usuario : null;
    }

    public Task<Usuario?> Usuario(int id, CancellationToken ct) => repository.UsuarioPorId(id, ct);

    public Task<PaginaCatalogo> Catalogo(string? nome, string? colecao, string? conservacao, int pagina, CancellationToken ct)
    {
        if (nome?.Length > 120 || colecao?.Length > 120 || pagina < 1 || pagina > 100000)
            throw new RegraException("FILTRO_INVALIDO", "Revise os filtros e a página.", 400);
        if (!string.IsNullOrEmpty(conservacao) && !Conservacoes.Todas.Contains(conservacao))
            throw new RegraException("CONSERVACAO_INVALIDA", "Conservação inválida.", 400);
        return repository.Catalogo(nome?.Trim(), colecao?.Trim(), conservacao, pagina, ct);
    }

    public async Task<Carrinho> Carrinho(int clienteId, CancellationToken ct)
    {
        await Exigir(clienteId, "CLIENTE", ct);
        return await repository.Carrinho(clienteId, ct);
    }

    public async Task AlterarItem(int clienteId, int ofertaId, int quantidade, CancellationToken ct)
    {
        await Exigir(clienteId, "CLIENTE", ct);
        if (ofertaId <= 0 || quantidade < 0 || quantidade > 10000)
            throw new RegraException("QUANTIDADE_INVALIDA", "Quantidade deve estar entre 1 e 10.000; zero remove a linha.", 400);
        await repository.AlterarItem(clienteId, ofertaId, quantidade, ct);
    }


    public async Task<ResultadoCompra> Comprar(int clienteId, Guid chave, IReadOnlyList<ItemSelecionado>? itens, CancellationToken ct)
    {
        await Exigir(clienteId, "CLIENTE", ct);
        if (chave == Guid.Empty || itens is null || itens.Count is < 1 or > 100 || itens.Any(i => i is null) || itens.Select(i => i.OfertaId).Distinct().Count() != itens.Count
            || itens.Any(i => i.OfertaId <= 0 || i.Quantidade is < 1 or > 10000 || i.PrecoConfirmado <= 0 || i.PrecoConfirmado > 9999999999.99m || decimal.Round(i.PrecoConfirmado, 2) != i.PrecoConfirmado))
            throw new RegraException("COMPRA_INVALIDA", "Selecione até 100 linhas distintas com quantidade e preço válidos e uma chave UUID.", 400);
        return await repository.Comprar(clienteId, chave, itens.OrderBy(i => i.OfertaId).ToArray(), ct);
    }

    public async Task<IReadOnlyList<CompraResumo>> Historico(int id, CancellationToken ct)
    {
        var usuario = await ExigirConsulta(id, ct);
        return await repository.Historico(id, usuario.Perfil == "ADMINISTRADOR", ct);
    }

    public async Task<CompraDetalhe> Compra(long compraId, int id, CancellationToken ct)
    {
        var usuario = await ExigirConsulta(id, ct);
        return await repository.Compra(compraId, id, usuario.Perfil == "ADMINISTRADOR", ct)
            ?? throw new RegraException("COMPRA_NAO_ENCONTRADA", "Compra não encontrada.", 404);
    }

    public async Task<ResultadoCompra> Cancelar(long compraId, int id, string? motivo, CancellationToken ct)
    {
        await Exigir(id, "ADMINISTRADOR", ct);
        if (compraId <= 0 || compraId > int.MaxValue || string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length > 300)
            throw new RegraException("MOTIVO_INVALIDO", "Informe um motivo de até 300 caracteres.", 400);
        return await repository.Cancelar(compraId, id, motivo.Trim(), ct);
    }

    private async Task<Usuario> Exigir(int id, string perfil, CancellationToken ct)
    {
        var usuario = await repository.UsuarioPorId(id, ct);
        if (usuario is not { Ativo: true } || usuario.Perfil != perfil)
            throw new RegraException("ACESSO_NEGADO", "Operação não permitida.", 403);
        return usuario;
    }

    private async Task<Usuario> ExigirConsulta(int id, CancellationToken ct)
    {
        var usuario = await repository.UsuarioPorId(id, ct);
        if (usuario is not { Ativo: true } || usuario.Perfil is not ("CLIENTE" or "ADMINISTRADOR"))
            throw new RegraException("ACESSO_NEGADO", "Operação não permitida.", 403);
        return usuario;
    }
}


