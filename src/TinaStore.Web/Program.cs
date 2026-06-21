using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using TinaStore.Web.Auth;
using TinaStore.Web.Components;
using TinaStore.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Puerto dinámico Railway ───────────────────────────────────────────
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ─── Blazor Server ────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ─── Autenticación ────────────────────────────────────────────────────────────
// Cookie se usa solo como transporte temporal para el callback OAuth de Google.
// La sesión real la gestiona SessionStateService con el JWT de TinaStore.
var googleClientId     = builder.Configuration["Google:ClientId"];
var googleClientSecret = builder.Configuration["Google:ClientSecret"];
var googleEnabled      = !string.IsNullOrWhiteSpace(googleClientId)
                      && !string.IsNullOrWhiteSpace(googleClientSecret);

var authBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        // Solo usa Google como challenge si las credenciales están configuradas.
        options.DefaultChallengeScheme = googleEnabled
            ? GoogleDefaults.AuthenticationScheme
            : CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie();

if (googleEnabled)
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId     = googleClientId!;
        options.ClientSecret = googleClientSecret!;
        options.CallbackPath = "/auth/google-callback";
        options.SaveTokens   = true;
        // El OAuthHandler base no guarda id_token automáticamente; hay que extraerlo manualmente.
        options.Events.OnCreatingTicket = ctx =>
        {
            var idToken = ctx.TokenResponse.Response?.RootElement
                .GetProperty("id_token").GetString() ?? string.Empty;
            if (!string.IsNullOrEmpty(idToken))
                ctx.Properties.StoreTokens(ctx.Properties.GetTokens().Append(
                    new Microsoft.AspNetCore.Authentication.AuthenticationToken
                    {
                        Name  = "id_token",
                        Value = idToken
                    }));
            return Task.CompletedTask;
        };
    });
}

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SessionRestoreGate>();
builder.Services.AddScoped<SessionStateService>();
builder.Services.AddScoped<AuthenticationStateProvider, TinaStoreAuthStateProvider>();

// Expone si el login con Google está disponible para que los componentes puedan consultarlo.
builder.Services.AddSingleton(new GoogleAuthConfig(googleEnabled));

// Servicio singleton para propagar cambios de logo entre componentes sin recargar la app.
builder.Services.AddSingleton<LogoStateService>();

// ─── HttpClient apuntando a la API ───────────────────────────────────────────
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5172";
builder.Services.AddScoped(sp =>
{
    var session = sp.GetRequiredService<SessionStateService>();
    return new TinaStoreApiClient(new HttpClient { BaseAddress = new Uri(apiBaseUrl) }, session);
});

// ─── ForwardedHeaders (Railway termina TLS en el edge) ─────────────────────
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Solo redirige a HTTPS en desarrollo local; en Railway Railway ya se encarga del TLS.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// UseStaticFiles sirve los archivos físicos de wwwroot.
// Los archivos de _framework (blazor.web.js etc.) los sirve
// directamente el middleware de Blazor mediante MapRazorComponents.
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// ─── Restauración de sesión desde cookie ─────────────────────────────────────
// En cada request HTTP, si existe la cookie tinastore_session, se pasa como
// query-string a la página de arranque para que App.razor restaure SessionStateService.
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Cookies.TryGetValue("tinastore_session", out var jwt)
        && !string.IsNullOrWhiteSpace(jwt)
        && !ctx.Request.Path.StartsWithSegments("/session")
        && !ctx.Request.Path.StartsWithSegments("/auth"))
    {
        ctx.Items["restored_token"] = jwt;
    }
    await next();
});

// ─── Rutas OAuth de Google ────────────────────────────────────────────────────
// Solo se registran si Google está configurado con ClientId y ClientSecret.
app.MapGet("/auth/google", async (HttpContext ctx) =>
{
    if (!googleEnabled)
    {
        ctx.Response.Redirect("/login?error=google_not_configured");
        return;
    }
    var props = new AuthenticationProperties
    {
        RedirectUri = "/auth/google-complete"
    };
    props.Parameters["prompt"] = "select_account";
    await ctx.ChallengeAsync(GoogleDefaults.AuthenticationScheme, props);
});

// El middleware OAuth intercepta /auth/google-callback (CallbackPath), intercambia el código
// por tokens, firma la cookie y redirige aquí con la sesión ya establecida.
app.MapGet("/auth/google-complete", async (HttpContext ctx) =>
{
    var result = await ctx.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    if (!result.Succeeded)
    {
        ctx.Response.Redirect("/login?error=google_failed");
        return;
    }

    var idToken = result.Properties?.GetTokenValue("id_token") ?? string.Empty;
    if (string.IsNullOrEmpty(idToken))
    {
        ctx.Response.Redirect("/login?error=no_id_token");
        return;
    }

    ctx.Response.Redirect($"/auth/google-complete-blazor?idToken={Uri.EscapeDataString(idToken)}");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();

// ─── Endpoints de cookie de sesión ───────────────────────────────────────────
// Permiten que Blazor (vía JS fetch) persista y borre el JWT en una cookie HTTP-only.

app.MapPost("/session/set", (HttpContext ctx, SessionTokenDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Token)) return Results.BadRequest();
    ctx.Response.Cookies.Append("tinastore_session", dto.Token, new CookieOptions
    {
        HttpOnly = true,
        Secure   = true,
        SameSite = SameSiteMode.Strict,
        MaxAge   = TimeSpan.FromDays(1)
    });
    return Results.Ok();
}).AllowAnonymous();

app.MapPost("/session/clear", (HttpContext ctx) =>
{
    ctx.Response.Cookies.Delete("tinastore_session");
    return Results.Ok();
}).AllowAnonymous();

// Devuelve el token almacenado en la cookie (HTTP-only) para restaurar la sesión en Blazor.
app.MapGet("/session/get", (HttpContext ctx) =>
{
    var token = ctx.Request.Cookies["tinastore_session"] ?? string.Empty;
    return Results.Ok(new { token });
}).AllowAnonymous();

app.Run();

// DTO interno para el endpoint de sesión
record SessionTokenDto(string Token);


