using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using TinaStore.Web.Services;

namespace TinaStore.Web.Auth;

/// <summary>
/// Proveedor de estado de autenticación para Blazor Server.
/// Lee el token JWT almacenado en SessionStateService y construye
/// el ClaimsPrincipal para que CascadingAuthenticationState lo propague.
/// </summary>
public class TinaStoreAuthStateProvider : AuthenticationStateProvider
{
    private readonly SessionStateService _session;

    public TinaStoreAuthStateProvider(SessionStateService session)
    {
        _session = session;
        _session.OnChange += NotifyAuthenticationStateChanged_Wrapper;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_session.IsAuthenticated || string.IsNullOrEmpty(_session.Token))
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(_session.Token);

            // Verificar expiración básica
            if (jwt.ValidTo < DateTime.UtcNow)
            {
                _session.Clear();
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            }

            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
        catch
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
    }

    private void NotifyAuthenticationStateChanged_Wrapper()
        => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
