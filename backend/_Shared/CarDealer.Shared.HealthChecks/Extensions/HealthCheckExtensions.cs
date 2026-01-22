using CarDealer.Shared.HealthChecks.Checks;
using CarDealer.Shared.HealthChecks.Configuration;
using HealthChecks.NpgSql;
using HealthChecks.Redis;
using HealthChecks.RabbitMQ;
using HealthChecks.Uris;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CarDealer.Shared.HealthChecks.Extensions;

/// <summary>
/// Extensiones para registrar Health Checks estándar
/// </summary>
public static class HealthCheckExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Agrega Health Checks estándar con configuración automática
    /// </summary>
    public static IServiceCollection AddStandardHealthChecks(
        this IServiceCollection services,
        string serviceName,
        IConfiguration configuration,
        string? version = null)
    {
        var options = new StandardHealthCheckOptions();
        configuration.GetSection(StandardHealthCheckOptions.SectionName).Bind(options);
        
        return services.AddStandardHealthChecks(serviceName, options, version);
    }

    /// <summary>
    /// Agrega Health Checks estándar con opciones manuales
    /// </summary>
    public static IServiceCollection AddStandardHealthChecks(
        this IServiceCollection services,
        string serviceName,
        Action<StandardHealthCheckOptions> configure,
        string? version = null)
    {
        var options = new StandardHealthCheckOptions();
        configure(options);
        
        return services.AddStandardHealthChecks(serviceName, options, version);
    }

    /// <summary>
    /// Agrega Health Checks estándar con opciones
    /// </summary>
    public static IServiceCollection AddStandardHealthChecks(
        this IServiceCollection services,
        string serviceName,
        StandardHealthCheckOptions options,
        string? version = null)
    {
        if (!options.Enabled)
            return services;

        var builder = services.AddHealthChecks();
        
        // Liveness check (siempre activo - el servicio está vivo?)
        builder.AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });
        
        // Version info
        builder.AddCheck(
            name: "version",
            instance: new VersionHealthCheck(
                serviceName,
                version ?? "1.0.0",
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"),
            tags: new[] { "live", "ready" });
        
        // Memory check
        builder.AddCheck(
            name: "memory",
            instance: new MemoryHealthCheck(maxMemoryBytes: 1024L * 1024L * 1024L), // 1GB
            tags: new[] { "live" });
        
        // Uptime check
        builder.AddCheck(
            name: "uptime",
            instance: new UptimeHealthCheck(),
            tags: new[] { "live" });
        
        // PostgreSQL
        if (options.PostgreSQL.Enabled && !string.IsNullOrEmpty(options.PostgreSQL.ConnectionString))
        {
            builder.AddNpgSql(
                options.PostgreSQL.ConnectionString,
                name: options.PostgreSQL.Name,
                timeout: TimeSpan.FromSeconds(options.PostgreSQL.TimeoutSeconds),
                tags: options.PostgreSQL.Tags);
        }
        
        // Redis
        if (options.Redis.Enabled && !string.IsNullOrEmpty(options.Redis.ConnectionString))
        {
            builder.AddRedis(
                options.Redis.ConnectionString,
                name: options.Redis.Name,
                timeout: TimeSpan.FromSeconds(options.Redis.TimeoutSeconds),
                tags: options.Redis.Tags);
        }
        
        // RabbitMQ
        if (options.RabbitMQ.Enabled)
        {
            var rabbitUri = $"amqp://{options.RabbitMQ.Username}:{options.RabbitMQ.Password}@{options.RabbitMQ.Host}:{options.RabbitMQ.Port}{options.RabbitMQ.VirtualHost}";
            builder.AddRabbitMQ(
                rabbitConnectionString: rabbitUri,
                name: options.RabbitMQ.Name,
                timeout: TimeSpan.FromSeconds(options.RabbitMQ.TimeoutSeconds),
                tags: options.RabbitMQ.Tags);
        }
        
        // External Services (URLs)
        if (options.ExternalServices.Enabled && options.ExternalServices.Services.Any())
        {
            foreach (var (key, service) in options.ExternalServices.Services)
            {
                if (!string.IsNullOrEmpty(service.Url))
                {
                    builder.AddUrlGroup(
                        new Uri(service.Url),
                        name: string.IsNullOrEmpty(service.Name) ? key : service.Name,
                        timeout: TimeSpan.FromSeconds(service.TimeoutSeconds),
                        tags: service.Tags);
                }
            }
        }
        
        return services;
    }

    /// <summary>
    /// Mapea los endpoints de Health Check
    /// </summary>
    public static WebApplication MapStandardHealthChecks(
        this WebApplication app,
        IConfiguration? configuration = null)
    {
        var options = new StandardHealthCheckOptions();
        configuration?.GetSection(StandardHealthCheckOptions.SectionName).Bind(options);
        
        return app.MapStandardHealthChecks(options.Endpoints);
    }

    /// <summary>
    /// Mapea los endpoints de Health Check con opciones
    /// </summary>
    public static WebApplication MapStandardHealthChecks(
        this WebApplication app,
        HealthCheckEndpointsOptions endpoints)
    {
        // Liveness endpoint - solo verifica que el servicio esté corriendo
        app.MapHealthChecks(endpoints.LivePath, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = WriteMinimalResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });
        
        // Readiness endpoint - verifica que puede recibir tráfico
        app.MapHealthChecks(endpoints.ReadyPath, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteMinimalResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });
        
        // Health endpoint - verifica todo con detalles
        app.MapHealthChecks(endpoints.HealthPath, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = endpoints.IncludeDetails 
                ? UIResponseWriter.WriteHealthCheckUIResponse 
                : WriteMinimalResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });
        
        return app;
    }

    /// <summary>
    /// Escribe una respuesta mínima sin detalles
    /// </summary>
    private static Task WriteMinimalResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            status = report.Status.ToString().ToLower(),
            timestamp = DateTime.UtcNow
        };
        
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
