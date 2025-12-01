using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Domain.Entities;

namespace ServiceDiscovery.Api.Middleware;

/// <summary>
/// Middleware to automatically register the service with Consul on startup
/// and deregister on shutdown
/// </summary>
public class ServiceRegistrationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ServiceRegistrationMiddleware> _logger;
    private string? _instanceId;

    public ServiceRegistrationMiddleware(
        RequestDelegate next,
        ILogger<ServiceRegistrationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceRegistry registry, IConfiguration configuration)
    {
        // Register service on first request if not already registered
        if (_instanceId == null)
        {
            await RegisterServiceAsync(registry, configuration);
        }

        await _next(context);
    }

    private async Task RegisterServiceAsync(IServiceRegistry registry, IConfiguration configuration)
    {
        try
        {
            var serviceName = configuration["Service:Name"] ?? "unknown-service";
            var host = configuration["Service:Host"] ?? "localhost";
            var portString = configuration["Service:Port"] ?? "80";
            var healthCheckUrl = configuration["Service:HealthCheckUrl"] ?? "/api/health";

            if (!int.TryParse(portString, out var port))
            {
                _logger.LogWarning("Invalid port configuration, using default 80");
                port = 80;
            }

            var instance = new ServiceInstance
            {
                Id = $"{serviceName}-{Guid.NewGuid()}",
                ServiceName = serviceName,
                Host = host,
                Port = port,
                HealthCheckUrl = healthCheckUrl,
                HealthCheckInterval = 10,
                HealthCheckTimeout = 5,
                Tags = new List<string> { "v1", "api" },
                Metadata = new Dictionary<string, string>
                {
                    { "environment", configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development" }
                }
            };

            var registered = await registry.RegisterServiceAsync(instance);
            _instanceId = registered.Id;

            _logger.LogInformation(
                "Service {ServiceName} registered with Consul. Instance ID: {InstanceId}, Address: {Address}",
                serviceName, _instanceId, instance.Address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register service with Consul");
        }
    }
}

/// <summary>
/// Extension method to add service registration middleware
/// </summary>
public static class ServiceRegistrationMiddlewareExtensions
{
    public static IApplicationBuilder UseServiceRegistration(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ServiceRegistrationMiddleware>();
    }
}
