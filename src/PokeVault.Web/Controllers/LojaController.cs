using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PokeVault.Web.Application;
using PokeVault.Web.Domain;
using PokeVault.Web.Infrastructure;
using PokeVault.Web.Models;

namespace PokeVault.Web.Controllers;

public sealed class LojaController(LojaService loja, Catalogo151Service catalogo) : Controller
{
    [HttpGet("/"), AllowAnonymous]
    public async Task<IActionResult> Catalogo(string? nome, string? colecao, string? conservacao, int pagina = 1, CancellationToken ct = default)
    {
        ViewBag.Nome = nome; ViewBag.Colecao = colecao; ViewBag.Conservacao = conservacao;
        return View(await catalogo.Vitrine(ct));
    }

    [HttpGet("/cartas/{ofertaId:int}"), AllowAnonymous]
    public async Task<IActionResult> Carta(int ofertaId,CancellationToken ct) => View(await catalogo.Carta(ofertaId,ct));

    [HttpPost("/carrinho/adicionar"), Authorize(Roles="CLIENTE")]
    public async Task<IActionResult> Adicionar(int ofertaId,int quantidade,CancellationToken ct)
    {
        if(!ModelState.IsValid) return BadRequest("Quantidade inválida.");
        try { await catalogo.Adicionar(User.UsuarioId(),ofertaId,quantidade,ct); TempData["Sucesso"]="Carta adicionada ao carrinho."; }
        catch(RegraException ex) { TempData["Erro"]=ex.Message; return Redirect($"/cartas/{ofertaId}"); }
        return RedirectToAction(nameof(Carrinho));
    }

    [HttpGet("/entrar"), AllowAnonymous]
    public IActionResult Login(string? returnUrl) => View(new LoginModel { ReturnUrl = returnUrl });

    [HttpPost("/entrar"), AllowAnonymous, EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var usuario = await loja.Autenticar(model.Email, model.Senha, ct);
        if (usuario is null) { ModelState.AddModelError("", "Email ou senha inválidos."); model.Senha = ""; return View(model); }
        await Sessao.Entrar(HttpContext, usuario);
        return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/");
    }

    [HttpPost("/sair"), Authorize]
    public async Task<IActionResult> Sair() { await HttpContext.SignOutAsync(); return Redirect("/"); }

    [HttpGet("/carrinho"), Authorize(Roles = "CLIENTE")]
    public async Task<IActionResult> Carrinho(CancellationToken ct) => View(await loja.Carrinho(User.UsuarioId(), ct));

    [HttpPost("/carrinho/item"), Authorize(Roles = "CLIENTE")]
    public async Task<IActionResult> Item(int ofertaId, int quantidade, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest("Quantidade inválida.");
        try { await loja.AlterarItem(User.UsuarioId(), ofertaId, quantidade, ct); }
        catch (RegraException ex) { TempData["Erro"] = ex.Message; }
        return RedirectToAction(nameof(Carrinho));
    }



    [HttpPost("/carrinho/confirmar"), Authorize(Roles = "CLIENTE")]
    public async Task<IActionResult> Confirmar(int[] selecionados, CancellationToken ct)
    {
        var carrinho = await loja.Carrinho(User.UsuarioId(), ct);
        if (selecionados.Length == 0 || selecionados.Distinct().Count() != selecionados.Length || selecionados.Any(id => !carrinho.Itens.Any(i => i.OfertaId == id)))
        { TempData["Erro"] = "Selecione ao menos uma linha válida do carrinho."; return RedirectToAction(nameof(Carrinho)); }
        var model = new CompraModel { ChaveOperacao = Guid.NewGuid(), Itens = carrinho.Itens.Where(i => selecionados.Contains(i.OfertaId)).Select(i => new ItemSelecionado(i.OfertaId, i.Quantidade, i.Preco)).ToList() };
        ViewBag.Saldo = carrinho.Saldo;
        ViewBag.Nomes = carrinho.Itens.ToDictionary(i => i.OfertaId, i => $"{i.Nome} · #{i.Numero}/165 · {Variantes.Estado(i.Conservacao)} · {Variantes.Idioma(i.Idioma)} · {Variantes.Acabamento(i.Acabamento)}");
        return View("Confirmacao", model);
    }

    [HttpPost("/comprar"), Authorize(Roles = "CLIENTE")]
    public async Task<IActionResult> Comprar(CompraModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest("Confirmação inválida. Volte ao carrinho.");
        try
        {
            var resultado = await loja.Comprar(User.UsuarioId(), model.ChaveOperacao, model.Itens, ct);
            return RedirectToAction(nameof(Compra), new { compraId = resultado.CompraId });
        }
        catch (RegraException ex)
        {
            Response.StatusCode = ex.Status;
            ViewBag.Erro = ex.Message;
            ViewBag.PodeRepetir = ex.Codigo == "CONCORRENCIA" || ex.Codigo == "FALHA_SQL";
            return View("Confirmacao", model);
        }
    }

    [HttpGet("/compras"), Authorize(Roles = "CLIENTE,ADMINISTRADOR")]
    public async Task<IActionResult> Historico(CancellationToken ct) => View(await loja.Historico(User.UsuarioId(), ct));

    [HttpGet("/compras/{compraId:long}"), Authorize(Roles = "CLIENTE,ADMINISTRADOR")]
    public async Task<IActionResult> Compra(long compraId, CancellationToken ct) => View(await loja.Compra(compraId, User.UsuarioId(), ct));

    [HttpPost("/compras/{compraId:long}/cancelamento"), Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Cancelar(long compraId, string? motivo, CancellationToken ct)
    {
        try { await loja.Cancelar(compraId, User.UsuarioId(), motivo, ct); TempData["Sucesso"] = "Compra cancelada. O estorno é aplicado uma única vez."; }
        catch (RegraException ex) { TempData["Erro"] = ex.Message; }
        return RedirectToAction(nameof(Compra), new { compraId });
    }
}

