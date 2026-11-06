using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PokeVault.Web.Infrastructure;

public sealed class AntiforgeryProblemFilter : IAlwaysRunResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is AntiforgeryValidationFailedResult && context.HttpContext.Request.Path.StartsWithSegments("/api"))
            context.Result = new ObjectResult(new ProblemDetails
            {
                Status = 400, Title = "Token de antifalsificação ausente ou inválido.",
                Extensions = { ["codigo"] = "ANTIFORGIA_INVALIDA" }
            }) { StatusCode = 400, ContentTypes = { "application/problem+json" } };
    }
    public void OnResultExecuted(ResultExecutedContext context) { }
}
