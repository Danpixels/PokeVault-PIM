using System.Threading.RateLimiting;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;
using PokeVault.Web.Application;
using PokeVault.Web.Domain;
using PokeVault.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false).AddEnvironmentVariables();
var database = builder.Configuration["POKEVAULT_DATABASE"]
    ?? new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(builder.Configuration.GetConnectionString("PokeVault")).InitialCatalog;
if (database is not ("PokeVault" or "PokeVaultTests")) throw new InvalidOperationException("Base local inválida.");
foreach (var nome in new[] { "PokeVault", "Preparacao" })
{
    var texto = builder.Configuration.GetConnectionString(nome);
    if (texto is null) continue;
    var conexao = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(texto) { InitialCatalog = database };
    builder.Configuration[$"ConnectionStrings:{nome}"] = conexao.ConnectionString;
}
builder.Services.AddControllersWithViews(options => { options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); options.Filters.Add(new AntiforgeryProblemFilter()); });
builder.Services.AddAntiforgery(options => { options.HeaderName = "X-CSRF-TOKEN"; options.Cookie.Name = $"{database}.Csrf"; options.Cookie.SameSite = SameSiteMode.Strict; });
builder.Services.AddScoped<ILojaRepository, SqlLojaRepository>();
builder.Services.AddScoped<IGestaoRepository, SqlLojaRepository>();
builder.Services.AddScoped<GestaoService>();
builder.Services.AddScoped<LocalReadiness>();
builder.Services.AddSingleton<TcgLocalImages>();
builder.Services.AddScoped<ITcgDexStore, SqlLojaRepository>();
builder.Services.AddScoped<TcgDexService>();
builder.Services.AddSingleton<TcgDexImportador>();
builder.Services.AddHttpClient<TcgDexClient>(client =>
{
    client.Timeout = Timeout.InfiniteTimeSpan;
    client.DefaultRequestHeaders.UserAgent.ParseAdd("PokeVault-Academic-Local/1.0");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = false, MaxConnectionsPerServer = 3,
    AutomaticDecompression = System.Net.DecompressionMethods.All
});
builder.Services.AddScoped<LojaService>();
builder.Services.AddScoped<SqlLojaRepository>();
builder.Services.AddScoped<Catalogo151Service>();
builder.Services.AddScoped<ContaService>();
builder.Services.AddScoped<ValidarSessao>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = $"{database}.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.LoginPath = "/entrar";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.EventsType = typeof(ValidarSessao);
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GerirUsuarios", policy => policy.RequireRole("ADMINISTRADOR"));
    options.AddPolicy("GerirCatalogo", policy => policy.RequireRole("ADMINISTRADOR", "ATENDENTE"));
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy("cadastro", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy("senha", context => RateLimitPartition.GetFixedWindowLimiter(context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonimo",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
var keyDirectory = new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys", database));
var protection = builder.Services.AddDataProtection().SetApplicationName($"PokeVault.Local.{database}").PersistKeysToFileSystem(keyDirectory);
if (OperatingSystem.IsWindows()) protection.ProtectKeysWithDpapi();

var app = builder.Build();
if (args.Contains("--bootstrap") || args.Contains("--credentials"))
{
    using var scope = app.Services.CreateScope();
    await Bootstrap.Executar(app.Configuration, app.Environment, scope.ServiceProvider, args.Contains("--credentials"));
    return;
}
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "same-origin";
    context.Response.Headers.CacheControl = "no-cache, no-store";
    try { await next(context); }
    catch (RegraException ex)
    {
        if (context.Response.HasStarted) throw;
        if (context.Request.Path.StartsWithSegments("/api"))
            await Results.Problem(statusCode: ex.Status, title: ex.Message, extensions: new Dictionary<string, object?> { ["codigo"] = ex.Codigo, ["traceId"] = context.TraceIdentifier }).ExecuteAsync(context);
        else
        {
            context.Response.StatusCode = ex.Status;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync("<!doctype html><html lang='pt-BR'><meta charset='utf-8'><title>PokéVault</title><h1>Não foi possível continuar</h1><p>" + System.Net.WebUtility.HtmlEncode(ex.Message) + "</p><a href='/'>Voltar ao catálogo</a></html>");
        }
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        app.Logger.LogError("Falha técnica {Tipo}; referência {TraceId}", ex.GetType().Name, context.TraceIdentifier);
        if (context.Response.HasStarted) throw;
        await Results.Problem(statusCode: 500, title: "Falha técnica. Tente novamente com a mesma chave se estava concluindo uma compra.",
            extensions: new Dictionary<string, object?> { ["codigo"] = "FALHA_TECNICA", ["traceId"] = context.TraceIdentifier }).ExecuteAsync(context);
    }
});
app.UseStaticFiles();
// Campos numéricos de formulário usam ponto decimal; moeda visível é formatada explicitamente em pt-BR.
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo("pt-BR")),
    SupportedCultures = [CultureInfo.InvariantCulture],
    SupportedUICultures = [CultureInfo.GetCultureInfo("pt-BR")],
    RequestCultureProviders = []
});
app.UseRouting();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(name: "default", pattern: "{controller=Loja}/{action=Catalogo}/{id?}");
app.MapGet("/health", () => Results.Ok(new { status = "aplicacao_ativa", bancoVerificado = false })).AllowAnonymous();
app.MapGet("/health/ready", async (LocalReadiness readiness, CancellationToken ct) =>
{
    var ready = await readiness.Verificar(ct);
    return Results.Json(new { aplicacao = "PokeVault", status = ready ? "pronto" : "banco_indisponivel_ou_schema_incompativel", schemaRequerido = 12 }, statusCode: ready ? 200 : 503);
}).AllowAnonymous();
app.Run();

public partial class Program { }

