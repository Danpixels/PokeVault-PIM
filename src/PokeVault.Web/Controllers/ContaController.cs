using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PokeVault.Web.Application;
using PokeVault.Web.Domain;
using PokeVault.Web.Infrastructure;
using PokeVault.Web.Models;

namespace PokeVault.Web.Controllers;

public sealed class ContaController(ContaService contas) : Controller
{
    [HttpGet("/criar-conta"),AllowAnonymous]
    public IActionResult Criar() => User.Identity?.IsAuthenticated==true ? Redirect("/conta") : View(new CriarContaModel());

    [HttpPost("/criar-conta"),AllowAnonymous,EnableRateLimiting("cadastro")]
    public async Task<IActionResult> Criar(CriarContaModel model,CancellationToken ct)
    {
        if(User.Identity?.IsAuthenticated==true) return Redirect("/conta");
        if(ModelState.IsValid)
            try
            {
                var usuario=await contas.Registrar(model,ct);await Sessao.Entrar(HttpContext,usuario);
                TempData["Sucesso"]="Conta criada! Seu saldo inicial é zero.";return Redirect("/");
            }
            catch(RegraException ex){Response.StatusCode=ex.Status;ModelState.AddModelError("",ex.Codigo=="CADASTRO_DUPLICADO"?"Já existe uma conta com este email. Entre com sua senha.":ex.Message);}
        model.Senha="";return View(model);
    }

    [HttpGet("/conta"),Authorize]
    public IActionResult Index() => View(new AlterarSenhaModel());

    [HttpPost("/conta/senha"),Authorize,EnableRateLimiting("senha")]
    public async Task<IActionResult> Senha(AlterarSenhaModel model,CancellationToken ct)
    {
        if(ModelState.IsValid)
            try
            {
                var usuario=await contas.AlterarSenha(User.UsuarioId(),model,ct);
                await Sessao.Entrar(HttpContext,usuario);
                TempData["Sucesso"]="Senha alterada com sucesso.";return Redirect("/conta");
            }
            catch(RegraException ex){Response.StatusCode=ex.Status;ModelState.AddModelError("",ex.Message);}
        model.SenhaAtual="";model.NovaSenha="";model.Confirmacao="";return View("Index",model);
    }
}
