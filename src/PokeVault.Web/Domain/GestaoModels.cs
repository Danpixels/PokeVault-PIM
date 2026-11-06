namespace PokeVault.Web.Domain;

public sealed record UsuarioGestao(int UsuarioId, string Nome, string Email, string Perfil, bool Ativo, decimal? Saldo, string? Versao);
public sealed record ColecaoGestao(int ColecaoId, string Codigo, string Nome);
public sealed record CartaGestao(int OfertaId, int ColecaoId, string Colecao, string Numero, string Nome, string Raridade, string Conservacao, decimal Preco, int Estoque, bool Ativo, string Versao);


