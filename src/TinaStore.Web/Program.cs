using Microsoft.AspNetCore.Authentication.Cookies;
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
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

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
// ⚠ UseAuthorization() intencionalmente ausente:
//   los [Authorize] en páginas Blazor son metadatos de endpoint que el middleware HTTP
//   procesaría antes del circuito Blazor, causando redirect a /Account/Login.
//   AuthorizeRouteView en Routes.razor protege las páginas dentro del circuito.
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();


