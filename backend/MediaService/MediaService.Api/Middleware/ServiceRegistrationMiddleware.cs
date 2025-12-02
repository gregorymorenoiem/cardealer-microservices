using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Domain.Entities;

namespace MediaService.Api.Middleware;

/// <summary>
/// Middleware to auto-register this service with Consul on application startup
/// </summary>
public class ServiceRegistrationMiddleware
{
    private readonly RequestDelegate _next;
    private static bool _isRegistered = false;
    private static readonly object _lock = new object();

    public ServiceRegistrationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IServiceRegistry registry,
        IConfiguration configuration,
        ILogger<ServiceRegistrationMiddleware> logger)
    {
        if (!_isRegistered)
        {
            lock (_lock)
            {
                if (!_isRegistered)
                {
                    try
                    {
                        var serviceName = configuration["Service:Name"] ?? "MediaService";
                        var host = configuration["Service:Host"] ?? "localhost";
                        var port = int.Parse(configuration["Service:Port"] ?? "5000");
                        var healthCheckUrl = configuration["Service:HealthCheckUrl"] ?? $"http://{host}:{port}/health";

                        var instance = new ServiceInstance
                        {
                            Id = Guid.NewGuid().ToString(),
                            ServiceName = serviceName,
                            Host = host,
                            Port = port,
                            HealthCheckUrl = healthCheckUrl,
                            HealthCheckInterval = 10,
                            HealthCheckTimeout = 5,
                            Tags = new List<string> { "media", "storage", "provider" },
                            Metadata = new Dictionary<string, string>
                            {
                                { "version", "1.0.0" },
                                { "environment", configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development" }
                            }
                        };

                        registry.RegisterServiceAsync(instance).GetAwaiter().GetResult();
                        logger.LogInformation(
                            "✅ MediaService registered with Consul: {ServiceName} at {Host}:{Port}",
                            serviceName, host, port);

                        _isRegistered = true;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "❌ Failed to register MediaService with Consul");
                    }
                }
            }
        }

        await _next(context);
    }
}
