using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokeVault.Web.Application;
using PokeVault.Web.Infrastructure;
using PokeVault.Web.Models;

namespace PokeVault.Web.Controllers;

[ApiController, Route("api/v1")]
public sealed class GestaoApiController(GestaoService gestao) : ControllerBase
{
    [HttpGet("admin/clientes"), Authorize(Policy = "GerirUsuarios")]
    public async Task<IActionResult> Clientes(int pagina = 1, CancellationToken ct = default) => Ok(await gestao.ListarUsuarios(User.UsuarioId(), "CLIENTE", pagina, ct));
    [HttpGet("admin/funcionarios"), Authorize(Policy = "GerirUsuarios")]
    public async Task<IActionResult> Funcionarios(int pagina = 1, CancellationToken ct = default) => Ok(await gestao.ListarUsuarios(User.UsuarioId(), "ATENDENTE", pagina, ct));
    [HttpGet("admin/clientes/{id:int}"), Authorize(Policy = "GerirUsuarios")]
    public async Task<IActionResult> Cliente(int id, CancellationToken ct) => Ok(await gestao.Usuario(User.UsuarioId(), id, "CLIENTE", ct));
    [HttpGet("admin/funcionarios/{id:int}"), Authorize(Policy = "GerirUsuarios")]
    public async Task<IActionResult> Funcionario(int id, CancellationToken ct) => Ok(await gestao.Usuario(User.UsuarioId(), id, "ATENDENTE", ct));
    [HttpPost("admin/clientes"), Authorize(Policy = "GerirUsuarios")]
    public async Task<IActionResult> CriarCliente(CadastroUsuarioModel model, CancellationToken ct)
    {
        var id = await gestao.CriarUsuario(User.UsuarioId(), "CLIENTE", model, ct);
        return CreatedAtAction(nameof(Cliente), new { id }, new { usuarioId = id });
    }
    [HttpPost("admin/funcionarios"), Authorize(Policy = "GerirUsuarios")]
    public async Task<IActionResult> CriarFuncionario(CadastroUsuarioModel model, CancellationToken ct)
    {
        var id = await gestao.CriarUsuario(User.UsuarioId(), "ATENDENTE", model, ct);
        return CreatedAtAction(nameof(Funcionario), new { id }, new { usuarioId = id });
    }
    [HttpPut("admin/clientes/{id:int}/saldo"), Authorize(Policy = "GerirUsuarios")]
    public async Task<IActionResult> Saldo(int id, SaldoModel model, CancellationToken ct)
    {
        await gestao.DefinirSaldo(User.UsuarioId(), id, model, ct);
        return Ok(await gestao.Usuario(User.UsuarioId(), id, "CLIENTE", ct));
    }
    [HttpPatch("admin/funcionarios/{id:int}"), Authorize(Policy = "GerirUsuarios")]
    public async Task<IActionResult> Ativo(int id, AtivacaoModel model, CancellationToken ct)
    {
        await gestao.DefinirAtendenteAtivo(User.UsuarioId(), id, model, ct);
        return Ok(await gestao.Usuario(User.UsuarioId(), id, "ATENDENTE", ct));
    }
    [HttpGet("gestao/ofertas"), Authorize(Policy = "GerirCatalogo")]
    public async Task<IActionResult> Cartas(int pagina = 1, CancellationToken ct = default) => Ok(await gestao.Cartas(User.UsuarioId(), pagina, ct));
    [HttpGet("gestao/ofertas/{id:int}"), Authorize(Policy = "GerirCatalogo")]
    public async Task<IActionResult> Carta(int id, CancellationToken ct) => Ok(await gestao.Carta(User.UsuarioId(), id, ct));
    [HttpGet("gestao/ofertas/colecoes"), Authorize(Policy = "GerirCatalogo")]
    public async Task<IActionResult> Colecoes(CancellationToken ct) => Ok(await gestao.Colecoes(User.UsuarioId(), ct));


    [HttpPost("gestao/ofertas"), Authorize(Policy = "GerirCatalogo")]
    public async Task<IActionResult> CriarCarta(CadastroCartaModel model, CancellationToken ct)
    {
        var id = await gestao.CadastrarCarta(User.UsuarioId(), model, ct);
        return CreatedAtAction(nameof(Carta), new { id }, new { ofertaId = id });
    }
    [HttpPatch("gestao/ofertas/{id:int}"), Authorize(Policy = "GerirCatalogo")]
    public async Task<IActionResult> AjustarCarta(int id, AjusteCartaModel model, CancellationToken ct)
    {
        await gestao.AjustarCarta(User.UsuarioId(), id, model, ct);
        return Ok(await gestao.Carta(User.UsuarioId(), id, ct));
    }
}

