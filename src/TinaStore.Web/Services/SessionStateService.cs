using Microsoft.JSInterop;

namespace TinaStore.Web.Services;

/// <summary>
/// Mantiene el estado de sesión del usuario en el circuito Blazor Server.
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

    public void Clear()
    {
        Token = null;
        UserName = null;
        UserEmail = null;
        UserRole = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    /// <summary>Persiste el token JWT en una cookie HTTP-only vía fetch al endpoint /session/set.</summary>
    public static async Task PersistAsync(IJSRuntime js, string token)
    {
        try
        {
            await js.InvokeVoidAsync("eval",
                $"fetch('/session/set',{{method:'POST',headers:{{'Content-Type':'application/json'}},body:JSON.stringify({{token:'{token}'}})}})");
        }
        catch { /* Si JS no está disponible, se omite */ }
    }
}
