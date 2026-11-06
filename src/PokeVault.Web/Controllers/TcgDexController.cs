using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeVault.Web.Application;
using PokeVault.Web.Domain;
using PokeVault.Web.Infrastructure;
using PokeVault.Web.Models;
using System.Text.Json.Nodes;

namespace PokeVault.Web.Controllers;

[Authorize(Policy = "GerirCatalogo")]
public sealed class TcgDexController(TcgDexService biblioteca, TcgDexImportador importador, ITcgDexStore store, Catalogo151Service catalogo) : Controller
{
    private async Task<TcgImportStatus> Status(CancellationToken ct)
    {
        var atual = importador.Estado;
        return atual.InicioUtc is not null ? atual : await store.UltimaImportacao(ct) ?? atual;
    }
    [HttpGet("/gestao/tcgdex151")]
    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(new TcgBibliotecaModel(await catalogo.Biblioteca(User.UsuarioId(), ct), await Status(ct), null, null, await catalogo.Operacao(User.UsuarioId(), ct)));
    [HttpPost("/gestao/tcgdex151/{id}/salvar")]
    public async Task<IActionResult> Salvar(string id, SalvarOferta151Model model, CancellationToken ct)
    {
        if (!ModelState.IsValid) TempData["Erro"]="Revise os campos da carta. Nada foi salvo.";
        else try { await catalogo.Salvar(User.UsuarioId(),id,model,ct); TempData["Sucesso"]="Estoque e preço salvos."; }
        catch(RegraException ex) { TempData["Erro"]=ex.Message; }
        return Redirect($"/gestao/tcgdex151?pagina={Math.Clamp(model.Pagina,1,10000)}#carta-{id}");
    }
    [HttpGet("/gestao/tcgdex151/status")]
    public async Task<IActionResult> Progresso(CancellationToken ct)
    {
        await biblioteca.Autorizar(User.UsuarioId(), ct); return View(await Status(ct));
    }
    [HttpPost("/gestao/tcgdex151/atualizar")]
    public async Task<IActionResult> Atualizar(bool forcarRede, CancellationToken ct)
    {
        await biblioteca.Autorizar(User.UsuarioId(), ct);
        if (!ModelState.IsValid) return BadRequest();
        if (!importador.Iniciar(User.UsuarioId(), forcarRede)) TempData["Erro"] = "Uma importação já está em andamento. Aguarde o status.";
        return RedirectToAction(nameof(Progresso));
    }
    [HttpGet("/gestao/tcgdex151/{id}")]
    public async Task<IActionResult> Detalhe(string id, CancellationToken ct)
    {
        var referencia = await biblioteca.Obter(User.UsuarioId(), id, ct);
        // PrecosJson pode ter sido preservado pelo SQL de uma coleta anterior sem pricing.
        var node = JsonNode.Parse(referencia.SnapshotJson) as JsonObject ?? new JsonObject();
        node["pricing"] = JsonNode.Parse(referencia.PrecosJson);
        return View(new TcgDetalheModel(referencia, TcgPrecos.Ler(node.ToJsonString())));
    }
    [HttpGet("/gestao/tcgdex151/{id}/oferta")]
    public async Task<IActionResult> Oferta(string id, CancellationToken ct)
    {
        ViewBag.Referencia = await biblioteca.Obter(User.UsuarioId(), id, ct); return View(new TcgOfertaModel());
    }
    [HttpPost("/gestao/tcgdex151/{id}/oferta")]
    public async Task<IActionResult> Oferta(string id, TcgOfertaModel model, CancellationToken ct)
    {
        if (ModelState.IsValid)
            try { var carta = await biblioteca.CadastrarOferta(User.UsuarioId(), id, model, ct); return Redirect($"/gestao/ofertas/{carta}/ajustar"); }
            catch (RegraException ex) { Response.StatusCode = ex.Status; ModelState.AddModelError("", ex.Message); }
        ViewBag.Referencia = await biblioteca.Obter(User.UsuarioId(), id, ct); return View(model);
    }
}

[ApiController, Route("api/v1/gestao/tcgdex151"), Authorize(Policy = "GerirCatalogo")]
public sealed class TcgDexApiController(TcgDexService biblioteca, TcgDexImportador importador, ITcgDexStore store) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(string? nome, string? raridade, int pagina = 1, CancellationToken ct = default) => Ok(await biblioteca.Listar(User.UsuarioId(), nome, raridade, pagina, ct));
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct)
    {
        await biblioteca.Autorizar(User.UsuarioId(), ct);
        var atual = importador.Estado; return Ok(atual.InicioUtc is not null ? atual : await store.UltimaImportacao(ct) ?? atual);
    }
    [HttpPost("atualizar")]
    public async Task<IActionResult> Atualizar(TcgAtualizarModel model, CancellationToken ct)
    {
        await biblioteca.Autorizar(User.UsuarioId(), ct);
        if (!importador.Iniciar(User.UsuarioId(), model.ForcarRede)) throw new RegraException("IMPORTACAO_EM_ANDAMENTO", "Uma importação já está em andamento.");
        return Accepted(new { mensagem = "Importação sv03.5 iniciada; acompanhe o status.", status = "/api/v1/gestao/tcgdex151/status" });
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(string id, CancellationToken ct) => Ok(await biblioteca.Obter(User.UsuarioId(), id, ct));
    [HttpPost("{id}/ofertas")]
    public async Task<IActionResult> Oferta(string id, TcgOfertaModel model, CancellationToken ct)
    {
        var carta = await biblioteca.CadastrarOferta(User.UsuarioId(), id, model, ct);
        return StatusCode(201, new { ofertaId = carta, referenciaTcgDexId = id });
    }
}
