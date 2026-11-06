using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PokeVault.Web.Application;
using PokeVault.Web.Infrastructure;
using PokeVault.Web.Models;

namespace PokeVault.Web.Controllers;

[ApiController, Route("api/v1")]
public sealed class LojaApiController(LojaService loja) : ControllerBase
{
    [HttpGet("antiforgery"), AllowAnonymous]
    public IActionResult Antiforgery([FromServices] IAntiforgery antiforgery)
    {
        Response.Headers.CacheControl = "no-cache, no-store";
        return Ok(new { token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken, header = "X-CSRF-TOKEN" });
    }

    [HttpPost("sessao"), AllowAnonymous, EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginModel model, CancellationToken ct)
    {
        var usuario = await loja.Autenticar(model.Email, model.Senha, ct);
        if (usuario is null) return Problem(statusCode: 401, title: "Email ou senha inválidos.", extensions: new Dictionary<string, object?> { ["codigo"] = "LOGIN_INVALIDO" });
        await Sessao.Entrar(HttpContext, usuario);
        return Ok(new { usuario.Id, usuario.Nome, usuario.Perfil });
    }

    [HttpGet("sessao"), Authorize]
    public IActionResult Identidade() => Ok(new { id = User.UsuarioId(), nome = User.Identity!.Name, perfil = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value });

    [HttpDelete("sessao"), Authorize]
    public async Task<IActionResult> Sair() { await HttpContext.SignOutAsync(); return NoContent(); }

    [HttpGet("ofertas"), AllowAnonymous]
    public async Task<IActionResult> Catalogo(string? nome, string? colecao, string? conservacao, int pagina = 1, CancellationToken ct = default) =>
        Ok(await loja.Catalogo(nome, colecao, conservacao, pagina, ct));

    [HttpGet("carrinho"), Authorize(Roles = "CLIENTE")]
    public async Task<IActionResult> Carrinho(CancellationToken ct) => Ok(await loja.Carrinho(User.UsuarioId(), ct));

    [HttpGet("me/saldo"), Authorize(Roles = "CLIENTE")]
    public async Task<IActionResult> Saldo(CancellationToken ct) => Ok(new { saldo = (await loja.Carrinho(User.UsuarioId(), ct)).Saldo, ficticio = true });

    [HttpPut("carrinho/itens/{ofertaId:int}"), Authorize(Roles = "CLIENTE")]
    public async Task<IActionResult> Alterar(int ofertaId, QuantidadeModel model, CancellationToken ct)
    {
        await loja.AlterarItem(User.UsuarioId(), ofertaId, model.Quantidade, ct); return NoContent();
    }

    [HttpDelete("carrinho/itens/{ofertaId:int}"), Authorize(Roles = "CLIENTE")]
    public async Task<IActionResult> Remover(int ofertaId, CancellationToken ct)
    {
        await loja.AlterarItem(User.UsuarioId(), ofertaId, 0, ct); return NoContent();
    }

    [HttpPost("compras"), Authorize(Roles = "CLIENTE")]
    public async Task<IActionResult> Comprar(CompraModel model, CancellationToken ct)
    {
        var resultado = await loja.Comprar(User.UsuarioId(), model.ChaveOperacao, model.Itens, ct);
        return Ok(resultado); // Pode ser reenvio idempotente; não presumir criação nova.
    }

    [HttpGet("compras"), Authorize(Roles = "CLIENTE,ADMINISTRADOR")]
    public async Task<IActionResult> Historico(CancellationToken ct) => Ok(await loja.Historico(User.UsuarioId(), ct));

    [HttpGet("compras/{compraId:long}"), Authorize(Roles = "CLIENTE,ADMINISTRADOR")]
    public async Task<IActionResult> Compra(long compraId, CancellationToken ct) => Ok(await loja.Compra(compraId, User.UsuarioId(), ct));

    [HttpPost("compras/{compraId:long}/cancelamento"), Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Cancelar(long compraId, CancelamentoModel model, CancellationToken ct) =>
        Ok(await loja.Cancelar(compraId, User.UsuarioId(), model.Motivo, ct));
}

