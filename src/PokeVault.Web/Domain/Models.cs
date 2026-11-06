namespace PokeVault.Web.Domain;

public sealed record Usuario(int Id, string Nome, string Email, string SenhaHash, string Perfil, bool Ativo);
public sealed record Oferta(int OfertaId, string Nome, string Colecao, string Numero, string Conservacao, decimal Preco, int Estoque, string? ImagemUrl = null, string? ReferenciaTcgDexId = null, string Idioma = "NAO_INFORMADO", string Acabamento = "NAO_INFORMADO");
public sealed record PaginaCatalogo(IReadOnlyList<Oferta> Itens, int Pagina, int Total, int Tamanho);
public sealed record ItemCarrinho(int OfertaId, string Nome, string Conservacao, int Quantidade, decimal Preco, int Estoque, string Colecao = "", string Numero = "", string Idioma = "NAO_INFORMADO", string Acabamento = "NAO_INFORMADO", string? ImagemUrl = null)
{
    public decimal Subtotal => Quantidade * Preco;
}
public sealed record Carrinho(decimal Saldo, IReadOnlyList<ItemCarrinho> Itens);
public sealed record ItemSelecionado(int OfertaId, int Quantidade, decimal PrecoConfirmado);
public sealed record CompraResumo(long CompraId, string Status, decimal Total, DateTime CriadaEmUtc, int ClienteId);
public sealed record ItemComprado(int OfertaId, string Nome, string Conservacao, int Quantidade, decimal PrecoUnitario, string? Idioma = null, string? Acabamento = null, string? Colecao = null, string? Numero = null);
public sealed record CompraDetalhe(CompraResumo Compra, IReadOnlyList<ItemComprado> Itens, string? MotivoCancelamento);
public sealed record ResultadoCompra(long CompraId, string Status, decimal Total, decimal SaldoAtual, bool Repetida);
public sealed class RegraException(string codigo, string mensagem, int status = 409) : Exception(mensagem)
{
    public string Codigo { get; } = codigo;
    public int Status { get; } = status;
}

public static class Conservacoes
{
    public static readonly string[] Todas = ["MINT", "NEAR_MINT", "SLIGHTLY_PLAYED", "MODERATELY_PLAYED", "HEAVILY_PLAYED", "DAMAGED"];
}


public sealed record CartaVitrine(string Nome,string Colecao,string Numero,int Estoque,decimal PrecoMinimo,bool VariosPrecos,string? ImagemUrl,int OfertaId);
public sealed record CartaLoja(IReadOnlyList<Oferta> Ofertas);
public static class Variantes
{
    public static readonly string[] Idiomas = ["pt", "en", "ja"];
    public static readonly string[] Acabamentos = ["NORMAL", "HOLO", "REVERSE_HOLO"];
    public static string Idioma(string? codigo) => codigo switch { "pt"=>"Português", "en"=>"Inglês", "ja"=>"Japonês", _=>"Idioma não informado" };
    public static string Acabamento(string? codigo) => codigo switch { "NORMAL"=>"Normal", "HOLO"=>"Holo", "REVERSE_HOLO"=>"Reverse holo", _=>"Acabamento não informado" };
    public static string Estado(string codigo) => codigo switch { "MINT"=>"Nova (M)", "NEAR_MINT"=>"Praticamente nova (NM)", "SLIGHTLY_PLAYED"=>"Pouco usada (SP)", "MODERATELY_PLAYED"=>"Usada (MP)", "HEAVILY_PLAYED"=>"Muito usada (HP)", "DAMAGED"=>"Danificada (D)", _=>codigo };
}
