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
}
