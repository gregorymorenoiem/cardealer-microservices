using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Domain.Entities;

namespace AuthService.Api.Middleware;

public class ServiceRegistrationMiddleware
{
    private readonly RequestDelegate _next;
    private static bool _isRegistered = false;
    private static readonly object _lock = new object();

    public ServiceRegistrationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IServiceRegistry registry, IConfiguration configuration, ILogger<ServiceRegistrationMiddleware> logger)
    {
        if (!_isRegistered)
        {
            lock (_lock)
            {
                if (!_isRegistered)
                {
                    try
                    {
                        var instance = new ServiceInstance
                        {
                            Id = Guid.NewGuid().ToString(),
                            ServiceName = configuration["Service:Name"] ?? "AuthService",
                            Host = configuration["Service:Host"] ?? "localhost",
                            Port = int.Parse(configuration["Service:Port"] ?? "5000"),
                            HealthCheckUrl = configuration["Service:HealthCheckUrl"] ?? "http://localhost:5000/health",
                            HealthCheckInterval = 10,
                            HealthCheckTimeout = 5,
                            Tags = new List<string> { "auth", "provider", "security" }
                        };
                        registry.RegisterServiceAsync(instance).GetAwaiter().GetResult();
                        logger.LogInformation("✅ AuthService registered with Consul");
                        _isRegistered = true;
                    }
                    catch (Exception ex) { logger.LogError(ex, "❌ Failed to register AuthService"); }
                }
            }
        }
        await _next(context);
    }
}
