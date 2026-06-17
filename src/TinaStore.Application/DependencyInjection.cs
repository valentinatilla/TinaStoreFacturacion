using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace TinaStore.Application;

/// <summary>
/// Registro de todos los servicios de la capa Application en el contenedor de dependencias.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registra automáticamente todos los validadores de FluentValidation de este ensamblado
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
