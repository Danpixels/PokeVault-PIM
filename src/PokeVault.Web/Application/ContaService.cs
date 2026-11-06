using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using PokeVault.Web.Domain;
using PokeVault.Web.Infrastructure;
using PokeVault.Web.Models;

namespace PokeVault.Web.Application;

public sealed class ContaService(SqlLojaRepository repository,IPasswordHasher<Usuario> hasher)
{
    public async Task<Usuario> Registrar(CriarContaModel model,CancellationToken ct)
    {
        Validar(model);
        var email=model.Email.Trim();
        var nome=email.Split('@')[0];
        if(string.IsNullOrWhiteSpace(nome)) nome="Cliente";
        if(nome.Length>120) nome=nome[..120];
        var usuario=new Usuario(0,nome,email,"","CLIENTE",true);
        var id=await repository.RegistrarCliente(nome,email,LojaService.NormalizarEmail(email),hasher.HashPassword(usuario,model.Senha),ct);
        return await repository.UsuarioPorId(id,ct) ?? throw new InvalidOperationException("Conta não encontrada após cadastro.");
    }
    public async Task<Usuario> AlterarSenha(int usuarioId,AlterarSenhaModel model,CancellationToken ct)
    {
        Validar(model);
        var usuario=await repository.UsuarioPorId(usuarioId,ct);
        if(usuario is not { Ativo:true }) throw new RegraException("ACESSO_NEGADO","Conta indisponível.",403);
        if(hasher.VerifyHashedPassword(usuario,usuario.SenhaHash,model.SenhaAtual)==PasswordVerificationResult.Failed)
            throw new RegraException("SENHA_ATUAL_INVALIDA","A senha atual está incorreta.",400);
        var hash=hasher.HashPassword(usuario,model.NovaSenha);
        await repository.AlterarSenha(usuario.Id,usuario.SenhaHash,hash,ct);
        return usuario with { SenhaHash=hash };
    }
    private static void Validar(object model)
    {
        if(!Validator.TryValidateObject(model,new ValidationContext(model),null,true))
            throw new RegraException("DADOS_INVALIDOS","Revise os campos. A nova senha deve ter de 6 a 128 caracteres e a confirmação deve coincidir.",400);
    }
}
