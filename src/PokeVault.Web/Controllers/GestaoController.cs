using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeVault.Web.Application;
using PokeVault.Web.Domain;
using PokeVault.Web.Infrastructure;
using PokeVault.Web.Models;

namespace PokeVault.Web.Controllers;

public sealed class GestaoController(GestaoService gestao) : Controller
{
    [HttpGet("/admin/clientes"), Authorize(Policy = "GerirUsuarios")]
    public Task<IActionResult> Clientes(int pagina = 1, CancellationToken ct = default) => ListarUsuarios("CLIENTE", pagina, ct);
    [HttpGet("/admin/funcionarios"), Authorize(Policy = "GerirUsuarios")]
    public Task<IActionResult> Funcionarios(int pagina = 1, CancellationToken ct = default) => ListarUsuarios("ATENDENTE", pagina, ct);
    private async Task<IActionResult> ListarUsuarios(string perfil, int pagina, CancellationToken ct)
    {
        ViewBag.Perfil = perfil; ViewBag.Pagina = pagina;
        return View("Usuarios", await gestao.ListarUsuarios(User.UsuarioId(), perfil, pagina, ct));
    }
    [HttpGet("/admin/clientes/novo"), Authorize(Policy = "GerirUsuarios")]
    public IActionResult NovoCliente() { ViewBag.Perfil = "CLIENTE"; return View("UsuarioNovo", new CadastroUsuarioModel()); }
    [HttpGet("/admin/funcionarios/novo"), Authorize(Policy = "GerirUsuarios")]
    public IActionResult NovoFuncionario() { ViewBag.Perfil = "ATENDENTE"; return View("UsuarioNovo", new CadastroUsuarioModel()); }
    [HttpPost("/admin/clientes/novo"), Authorize(Policy = "GerirUsuarios")]
    public Task<IActionResult> NovoCliente(CadastroUsuarioModel model, CancellationToken ct) => CriarUsuario("CLIENTE", model, ct);
    [HttpPost("/admin/funcionarios/novo"), Authorize(Policy = "GerirUsuarios")]
    public Task<IActionResult> NovoFuncionario(CadastroUsuarioModel model, CancellationToken ct) => CriarUsuario("ATENDENTE", model, ct);
    private async Task<IActionResult> CriarUsuario(string perfil, CadastroUsuarioModel model, CancellationToken ct)
    {
        ViewBag.Perfil = perfil;
        if (ModelState.IsValid)
            try
            {
                await gestao.CriarUsuario(User.UsuarioId(), perfil, model, ct);
                TempData["Sucesso"] = "Conta criada. Compartilhe a senha inicial pelo meio local combinado.";
                return Redirect(perfil == "CLIENTE" ? "/admin/clientes" : "/admin/funcionarios");
            }
            catch (RegraException ex) { Response.StatusCode = ex.Status; ModelState.AddModelError("", ex.Message); }
        model.SenhaInicial = ""; ModelState.Remove(nameof(model.SenhaInicial));
        return View("UsuarioNovo", model);
    }
    [HttpGet("/admin/clientes/{id:int}/saldo"), Authorize(Policy = "GerirUsuarios")]
    public async Task<IActionResult> Saldo(int id, CancellationToken ct)
    {
        var usuario = await gestao.Usuario(User.UsuarioId(), id, "CLIENTE", ct);
        ViewBag.Usuario = usuario;
        return View(new SaldoModel { NovoSaldo = usuario.Saldo, VersaoEsperada = usuario.Versao! });
    }
    [HttpPost("/admin/clientes/{id:int}/saldo"), Authorize(Policy = "GerirUsuarios")]
    public async Task<IActionResult> Saldo(int id, SaldoModel model, CancellationToken ct)
    {
        if (ModelState.IsValid)
            try { await gestao.DefinirSaldo(User.UsuarioId(), id, model, ct); TempData["Sucesso"] = "Saldo fictício definido e diferença auditada."; return Redirect("/admin/clientes"); }
            catch (RegraException ex) { Response.StatusCode = ex.Status; ViewBag.Conflito = ex.Status == 409; ModelState.AddModelError("", ex.Message); }
        ViewBag.Usuario = await gestao.Usuario(User.UsuarioId(), id, "CLIENTE", ct);
        return View(model); // Preserva a versão enviada; nova versão exige recarga explícita.
    }
    [HttpPost("/admin/funcionarios/{id:int}/ativo"), Authorize(Policy = "GerirUsuarios")]
    public async Task<IActionResult> Ativo(int id, AtivacaoModel model, CancellationToken ct)
    {
        try { await gestao.DefinirAtendenteAtivo(User.UsuarioId(), id, model, ct); TempData["Sucesso"] = "Situação do atendente atualizada."; }
        catch (RegraException ex) { TempData["Erro"] = ex.Message; }
        return Redirect("/admin/funcionarios");
    }
    [HttpGet("/gestao/ofertas"), Authorize(Policy = "GerirCatalogo")]
    public IActionResult Cartas() => Redirect("/gestao/tcgdex151");
    [HttpGet("/gestao/ofertas/nova"), Authorize(Policy = "GerirCatalogo")]
    public IActionResult NovaCarta() => Redirect("/gestao/tcgdex151");
    [HttpPost("/gestao/ofertas/nova"), Authorize(Policy = "GerirCatalogo")]
    public async Task<IActionResult> NovaCarta(CadastroCartaModel model, CancellationToken ct)
    {
        if (ModelState.IsValid)
            try { var id = await gestao.CadastrarCarta(User.UsuarioId(), model, ct); return Redirect($"/gestao/ofertas/{id}/ajustar"); }
            catch (RegraException ex) { Response.StatusCode = ex.Status; ModelState.AddModelError("", ex.Message); }
        ViewBag.Colecoes = await gestao.Colecoes(User.UsuarioId(), ct); return View(model);
    }
    [HttpGet("/gestao/ofertas/{id:int}/ajustar"), Authorize(Policy = "GerirCatalogo")]
    public async Task<IActionResult> Ajustar(int id, CancellationToken ct)
    {
        var carta = await gestao.Carta(User.UsuarioId(), id, ct); ViewBag.Carta = carta;
        return View(new AjusteCartaModel { Preco = carta.Preco, NovoEstoque = carta.Estoque, Ativo = carta.Ativo, VersaoEsperada = carta.Versao });
    }
    [HttpPost("/gestao/ofertas/{id:int}/ajustar"), Authorize(Policy = "GerirCatalogo")]
    public async Task<IActionResult> Ajustar(int id, AjusteCartaModel model, CancellationToken ct)
    {
        if (ModelState.IsValid)
            try { await gestao.AjustarCarta(User.UsuarioId(), id, model, ct); TempData["Sucesso"] = "Preço, estoque e ativação atualizados com auditoria."; return Redirect("/gestao/ofertas"); }
            catch (RegraException ex) { Response.StatusCode = ex.Status; ViewBag.Conflito = ex.Status == 409; ModelState.AddModelError("", ex.Message); }
        ViewBag.Carta = await gestao.Carta(User.UsuarioId(), id, ct); return View(model);
    }
}

