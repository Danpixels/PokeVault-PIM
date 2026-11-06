using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PokeVault.Web.Models;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CadastroUsuarioModel
{
    [Required, StringLength(120)] public string Nome { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Required, StringLength(128, MinimumLength = 8)] public string SenhaInicial { get; set; } = "";
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SaldoModel
{
    [Required, Range(typeof(decimal), "0", "9999999999.99")] public decimal? NovoSaldo { get; set; }
    [Required, StringLength(300)] public string Motivo { get; set; } = "";
    [Required] public string VersaoEsperada { get; set; } = "";
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class AtivacaoModel
{
    [Required] public bool? Ativo { get; set; }
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CadastroCartaModel
{
    [Range(1, int.MaxValue)] public int ColecaoId { get; set; }
    [Required, StringLength(20)] public string Numero { get; set; } = "";
    [Required, StringLength(120)] public string Nome { get; set; } = "";
    [Required, StringLength(50)] public string Raridade { get; set; } = "";
    [Required, StringLength(30)] public string Conservacao { get; set; } = "NEAR_MINT";
    [Required, Range(typeof(decimal), "0.01", "9999999999.99")] public decimal? Preco { get; set; }
    [Required, Range(0, int.MaxValue)] public int? QuantidadeInicial { get; set; } = 0;
}
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class AjusteCartaModel
{
    [Required, Range(typeof(decimal), "0.01", "9999999999.99")] public decimal? Preco { get; set; }
    [Required, Range(0, int.MaxValue)] public int? NovoEstoque { get; set; }
    [Required] public bool? Ativo { get; set; }
    [StringLength(300)] public string Motivo { get; set; } = "";
    [Required] public string VersaoEsperada { get; set; } = "";
}


