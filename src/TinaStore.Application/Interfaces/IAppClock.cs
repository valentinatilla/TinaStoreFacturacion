namespace TinaStore.Application.Interfaces;

/// <summary>
/// Abstracción del reloj de la aplicación. Devuelve la hora actual en la
/// zona horaria configurada para el negocio (UTC-5 Colombia / Ecuador / Perú).
/// </summary>
public interface IAppClock
{
    DateTime Now { get; }
}
