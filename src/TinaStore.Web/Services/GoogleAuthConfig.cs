namespace TinaStore.Web.Services;

/// <summary>
/// Indica si el login con Google estÃ¡ disponible en esta instancia.
/// Se registra como singleton en Program.cs segÃºn la presencia de ClientId y ClientSecret.
/// </summary>
public sealed class GoogleAuthConfig(bool enabled)
{
    public bool Enabled { get; } = enabled;
}
