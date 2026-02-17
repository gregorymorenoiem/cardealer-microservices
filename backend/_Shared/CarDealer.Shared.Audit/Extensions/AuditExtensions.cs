using CarDealer.Shared.Audit.Configuration;
using CarDealer.Shared.Audit.Interfaces;
using CarDealer.Shared.Audit.Middleware;
using CarDealer.Shared.Audit.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarDealer.Shared.Audit.Extensions;

/// <summary>
/// Extensiones para registrar el sistema de auditoría
/// </summary>
public static class AuditExtensions
{
    /// <summary>
    /// Agrega el publicador de eventos de auditoría con configuración automática
    /// </summary>
    public static IServiceCollection AddAuditPublisher(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(AuditOptions.SectionName);
        services.Configure<AuditOptions>(options => section.Bind(options));

        services.AddSingleton<IAuditPublisher, RabbitMqAuditPublisher>();

        return services;
    }

    /// <summary>
    /// Agrega el publicador de eventos de auditoría con opciones manuales
    /// </summary>
    public static IServiceCollection AddAuditPublisher(
        this IServiceCollection services,
        Action<AuditOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IAuditPublisher, RabbitMqAuditPublisher>();

        return services;
    }

    /// <summary>
    /// Usa el middleware de auditoría automática para requests HTTP
    /// </summary>
    public static IApplicationBuilder UseAuditMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuditMiddleware>();
    }
}
