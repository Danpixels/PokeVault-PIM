using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using PokeVault.Web.Application;
using PokeVault.Web.Domain;

namespace PokeVault.Web.Infrastructure;

public static class Sessao
{
    public static string VersaoSenha(Usuario usuario) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(usuario.SenhaHash)));
    public static int UsuarioId(this ClaimsPrincipal principal) => int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!, CultureInfo.InvariantCulture);
    public static Task Entrar(HttpContext context, Usuario usuario) => context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Name, usuario.Nome), new Claim(ClaimTypes.Role, usuario.Perfil),
            new Claim("versao_senha",VersaoSenha(usuario))
        ], CookieAuthenticationDefaults.AuthenticationScheme)), new AuthenticationProperties { IsPersistent = false });
}

public sealed class ValidarSessao(LojaService loja) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var usuario = int.TryParse(id, out var valor) ? await loja.Usuario(valor, context.HttpContext.RequestAborted) : null;
        if (usuario is not { Ativo: true } || usuario.Perfil != context.Principal?.FindFirstValue(ClaimTypes.Role)
            || context.Principal?.FindFirstValue("versao_senha") != Sessao.VersaoSenha(usuario))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
        }
    }

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        if (!context.Request.Path.StartsWithSegments("/api")) { context.Response.Redirect(context.RedirectUri); return Task.CompletedTask; }
        return Results.Problem(statusCode: 401, title: "Autenticação necessária", extensions: new Dictionary<string, object?> { ["codigo"] = "NAO_AUTENTICADO" }).ExecuteAsync(context.HttpContext);
    }

    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context) =>
        Results.Problem(statusCode: 403, title: "Operação não permitida", extensions: new Dictionary<string, object?> { ["codigo"] = "ACESSO_NEGADO" }).ExecuteAsync(context.HttpContext);
}
