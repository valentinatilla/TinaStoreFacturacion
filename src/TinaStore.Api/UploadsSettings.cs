/// <summary>
/// Configuración para la ruta base donde se almacenan las imágenes de productos y el logo.
/// En Railway (plan gratuito con 1 solo volumen en /app/data), configurar:
///   Uploads__BasePath = /app/data
/// Así las imágenes se guardan en /app/data/uploads/ dentro del volumen persistente.
/// En desarrollo no es necesario configurar esta variable.
/// </summary>
public sealed class UploadsSettings
{
    public string BasePath { get; set; } = string.Empty;
}
