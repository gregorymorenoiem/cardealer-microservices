using Microsoft.Extensions.DependencyInjection;

namespace SearchService.Application;

/// <summary>
/// Configuración de servicios de la capa de aplicación
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Registrar MediatR handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
