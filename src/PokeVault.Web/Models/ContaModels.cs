using System.ComponentModel.DataAnnotations;

namespace PokeVault.Web.Models;

public sealed class CriarContaModel
{
    [Required(ErrorMessage="Informe seu email."), EmailAddress(ErrorMessage="Informe um email válido."), StringLength(254)]
    public string Email { get; set; } = "";
    [Required(ErrorMessage="Informe uma senha."), StringLength(128,MinimumLength=6,ErrorMessage="Use uma senha de 6 a 128 caracteres.")]
    public string Senha { get; set; } = "";
}

public sealed class AlterarSenhaModel
{
    [Required(ErrorMessage="Informe a senha atual."), StringLength(256)]
    public string SenhaAtual { get; set; } = "";
    [Required(ErrorMessage="Informe a nova senha."), StringLength(128,MinimumLength=6,ErrorMessage="Use uma senha de 6 a 128 caracteres.")]
    public string NovaSenha { get; set; } = "";
    [Required(ErrorMessage="Confirme a nova senha."), Compare(nameof(NovaSenha),ErrorMessage="A confirmação deve ser igual à nova senha.")]
    public string Confirmacao { get; set; } = "";
}
