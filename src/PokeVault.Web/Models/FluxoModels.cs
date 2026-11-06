using System.ComponentModel.DataAnnotations;
using PokeVault.Web.Domain;

namespace PokeVault.Web.Models;

public sealed class LoginModel
{
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Required, StringLength(256)] public string Senha { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
public sealed class CompraModel
{
    public Guid ChaveOperacao { get; set; }
    public List<ItemSelecionado> Itens { get; set; } = [];
}
public sealed record QuantidadeModel(int Quantidade);
public sealed record CancelamentoModel(string? Motivo);
