using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Components.Authorization;
using TinaStore.Web.Auth;
using TinaStore.Web.Components;
using TinaStore.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Blazor Server ────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ─── Autenticación ────────────────────────────────────────────────────────────
// Solo se registra para que IAuthenticationService esté disponible.
// La autorización real la gestiona AuthorizeRouteView dentro del circuito Blazor.
// NO se llama a UseAuthorization() para no interceptar peticiones HTTP con [Authorize].
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Google:ClientSecret"] ?? "";
    options.CallbackPath = "/auth/google-callback";
    options.SaveTokens = true;
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<SessionStateService>();
builder.Services.AddScoped<AuthenticationStateProvider, TinaStoreAuthStateProvider>();

// ─── HttpClient apuntando a la API ───────────────────────────────────────────
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5172";
builder.Services.AddScoped(sp =>
{
    var session = sp.GetRequiredService<SessionStateService>();
    return new TinaStoreApiClient(new HttpClient { BaseAddress = new Uri(apiBaseUrl) }, session);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// ─── Rutas OAuth de Google ─────────────────────────────────────────
app.MapGet("/auth/google", async (HttpContext ctx) =>
{
    var props = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
    {
        RedirectUri = "/auth/google-complete"
    };
    await ctx.ChallengeAsync(GoogleDefaults.AuthenticationScheme, props);
});

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

// ─── Endpoints de cookie de sesión ────────────────────────────────────
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

app.MapGet("/session/get", (HttpContext ctx) =>
{
    var token = ctx.Request.Cookies["tinastore_session"] ?? string.Empty;
    return Results.Ok(new { token });
}).AllowAnonymous();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous(); // Permite acceso HTTP a todos los endpoints Razor;
                       // la autorización real la gestiona AuthorizeRouteView
                       // dentro del circuito Blazor con [Authorize] en los componentes.

app.Run();

// DTO interno para el endpoint de sesión
record SessionTokenDto(string Token);