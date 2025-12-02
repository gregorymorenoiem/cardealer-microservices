using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ServiceDiscovery.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.HealthChecks;

public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly IServiceDiscovery _serviceDiscovery;
    private readonly ILogger<ExternalServiceHealthCheck> _logger;

    public ExternalServiceHealthCheck(
        HttpClient httpClient,
        IServiceDiscovery serviceDiscovery,
        ILogger<ExternalServiceHealthCheck> logger)
    {
        _httpClient = httpClient;
        _serviceDiscovery = serviceDiscovery;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var checks = new List<(string service, string url, bool isHealthy)>();

        try
        {
            // Check ErrorService via Service Discovery
            var errorServiceUrl = await GetServiceUrlAsync("ErrorService");
            if (!string.IsNullOrEmpty(errorServiceUrl))
            {
                var isHealthy = await CheckServiceHealthAsync($"{errorServiceUrl}/health");
                checks.Add(("ErrorService", errorServiceUrl, isHealthy));
            }

            // Check NotificationService via Service Discovery
            var notificationServiceUrl = await GetServiceUrlAsync("NotificationService");
            if (!string.IsNullOrEmpty(notificationServiceUrl))
            {
                var isHealthy = await CheckServiceHealthAsync($"{notificationServiceUrl}/health");
                checks.Add(("NotificationService", notificationServiceUrl, isHealthy));
            }

            var unhealthyServices = checks.Where(c => !c.isHealthy).ToList();
            if (unhealthyServices.Any())
            {
                return HealthCheckResult.Degraded(
                    $"Some external services are unhealthy: {string.Join(", ", unhealthyServices.Select(s => s.service))}",
                    data: new Dictionary<string, object>
                    {
                        { "services", checks }
                    });
            }

            return HealthCheckResult.Healthy("All external services are healthy",
                new Dictionary<string, object>
                {
                    { "services", checks }
                });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("External services health check failed", ex);
        }
    }

    private async Task<string?> GetServiceUrlAsync(string serviceName)
    {
        try
        {
            var instance = await _serviceDiscovery.FindServiceInstanceAsync(serviceName);
            if (instance != null)
            {
                return $"http://{instance.Host}:{instance.Port}";
            }
            _logger.LogWarning("{ServiceName} not found in Consul", serviceName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error resolving {ServiceName} from Consul", serviceName);
            return null;
        }
    }

    private async Task<bool> CheckServiceHealthAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
