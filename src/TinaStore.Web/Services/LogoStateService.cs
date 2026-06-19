namespace TinaStore.Web.Services;

/// <summary>
/// Servicio singleton que mantiene la URL del logo actual de la tienda
/// y notifica a los componentes suscritos cuando cambia.
/// </summary>
public sealed class LogoStateService
{
    private string? _logoUrl;

    public string? LogoUrl
    {
        get => _logoUrl;
        private set
        {
            _logoUrl = value;
            NotifyChanged();
        }
    }

    /// <summary>Se dispara cuando la URL del logo cambia.</summary>
    public event Action? OnChange;

    public void SetLogo(string? url)
    {
        LogoUrl = url;
    }

    private void NotifyChanged() => OnChange?.Invoke();
}
