using Microsoft.JSInterop;

namespace TinaStore.Web.Services;

/// <summary>
/// Mantiene el estado de sesión del usuario en el circuito Blazor Server
/// y lo persiste en una cookie HTTP-only para sobrevivir recargas de página.
/// </summary>
public class SessionStateService
{
    public string? Token { get; private set; }
    public string? UserName { get; private set; }
    public string? UserEmail { get; private set; }
    public string? UserRole { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public event Action? OnChange;

    public void SetSession(string token, string userName, string email, string role)
    {
        Token = token;
        UserName = userName;
        UserEmail = email;
        UserRole = role;
        NotifyStateChanged();
    }

    /// <summary>
    /// Restaura la sesión desde un token ya validado (usado al arrancar desde cookie).
    /// No dispara persistencia porque la cookie ya existe.
    /// </summary>
    public void RestoreSession(string token, string userName, string email, string role)
    {
        Token = token;
        UserName = userName;
        UserEmail = email;
        UserRole = role;
        // No notifica para no provocar re-render en el arranque del circuito
    }

    public void Clear()
    {
        Token = null;
        UserName = null;
        UserEmail = null;
        UserRole = null;
        NotifyStateChanged();
    }

    /// <summary>
    /// Persiste el JWT en la cookie HTTP-only del servidor vía fetch JS.
    /// </summary>
    public static async Task PersistAsync(IJSRuntime js, string token)
    {
        await js.InvokeVoidAsync("tinaSession.save", token);
    }

    /// <summary>
    /// Borra la cookie de sesión del servidor vía fetch JS.
    /// </summary>
    public static async Task ClearPersistenceAsync(IJSRuntime js)
    {
        await js.InvokeVoidAsync("tinaSession.clear");
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
