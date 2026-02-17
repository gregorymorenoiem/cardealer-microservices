using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Domain.Entities;

namespace Gateway.Api.Middleware;

public class ServiceRegistrationMiddleware
{
    private readonly RequestDelegate _next;
    private static bool _isRegistered = false;
    private static readonly object _lock = new object();

    public ServiceRegistrationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceRegistry serviceRegistry, IConfiguration configuration)
    {
        if (!_isRegistered)
        {
            await RegisterServiceOnceAsync(serviceRegistry, configuration);
        }

        await _next(context);
    }

    private static async Task RegisterServiceOnceAsync(IServiceRegistry serviceRegistry, IConfiguration configuration)
    {
        if (_isRegistered) return;

        try
        {
            var serviceName = configuration["Service:Name"] ?? "Gateway";
            var host = configuration["Service:Host"] ?? "localhost";
            var portString = configuration["Service:Port"] ?? "5008";
            var healthCheckUrl = configuration["Service:HealthCheckUrl"] ?? $"http://{host}:{portString}/health";

            if (!int.TryParse(portString, out var port))
            {
                port = 5008;
            }

            var instanceId = $"{serviceName}-{Guid.NewGuid()}";
            var tags = new[] { "gateway", "routing", "api-gateway", "consumer" };
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var instance = new ServiceInstance
            {
                Id = instanceId,
                ServiceName = serviceName,
                Host = host,
                Port = port,
                HealthCheckUrl = healthCheckUrl,
                HealthCheckInterval = 10,
                HealthCheckTimeout = 5,
                Tags = tags.ToList(),
                Metadata = new Dictionary<string, string>
                {
                    { "version", "1.0.0" },
                    { "environment", environment },
                    { "ocelot", "true" }
                }
            };

            await serviceRegistry.RegisterServiceAsync(instance);
            _isRegistered = true;
            Console.WriteLine($"[ServiceDiscovery] {serviceName} registered successfully with Consul. Instance: {instanceId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ServiceDiscovery] Error registering service: {ex.Message}");
        }
    }
}
