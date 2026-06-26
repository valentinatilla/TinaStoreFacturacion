using TinaStore.Application.Interfaces;

namespace TinaStore.Infrastructure.Services;

/// <summary>
/// Implementación del reloj de la aplicación en UTC-5
/// (Colombia, Ecuador, Perú - sin horario de verano).
/// </summary>
public sealed class AppClock : IAppClock
{
    private static readonly TimeZoneInfo Utc5 =
        TimeZoneInfo.CreateCustomTimeZone("UTC-5", TimeSpan.FromHours(-5), "UTC-5", "UTC-5");

    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Utc5);
}
