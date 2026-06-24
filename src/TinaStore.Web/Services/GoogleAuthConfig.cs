namespace TinaStore.Web.Services;

/// <summary>
/// Indica si el login con Google está disponible en esta instancia.
/// Se registra como singleton en Program.cs según la presencia de ClientId y ClientSecret.
/// </summary>
public sealed class GoogleAuthConfig(bool enabled)
{
    public bool Enabled { get; } = enabled;
}
