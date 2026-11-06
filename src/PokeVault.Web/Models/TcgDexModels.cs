using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PokeVault.Web.Domain;

namespace PokeVault.Web.Models;

public sealed record TcgBibliotecaModel(TcgPagina Pagina, TcgImportStatus Estado, string? Nome, string? Raridade, IReadOnlyList<OfertaOperacao>? Ofertas = null);
public sealed record TcgDetalheModel(TcgReferencia Referencia, IReadOnlyList<TcgPreco> Precos);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class TcgAtualizarModel { public bool ForcarRede { get; set; } }
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class TcgOfertaModel
{
    [Required, StringLength(30)] public string Conservacao { get; set; } = "";
    [Required, Range(typeof(decimal), "0.01", "9999999999.99")] public decimal? Preco { get; set; }
    [Required, Range(0, int.MaxValue)] public int? QuantidadeInicial { get; set; }
}

public sealed record OfertaOperacao(int OfertaId,string ReferenciaId,string Conservacao,string Idioma,string Acabamento,decimal Preco,int Estoque,bool Ativo,string Versao);
public sealed class SalvarOferta151Model
{
    public int? OfertaId { get; set; }
    [Required] public string Conservacao { get; set; } = "NEAR_MINT";
    [Required] public string Idioma { get; set; } = "pt";
    [Required] public string Acabamento { get; set; } = "NORMAL";
    [Required, Range(typeof(decimal), "0.01", "9999999999.99")] public decimal? Preco { get; set; }
    [Required, Range(0,int.MaxValue)] public int? Unidades { get; set; }
    public string? VersaoEsperada { get; set; }
    public int Pagina { get; set; } = 1;
}
