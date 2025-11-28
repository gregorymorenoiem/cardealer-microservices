using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AuthService.Infrastructure.HealthChecks;

public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ExternalServiceHealthCheck(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var checks = new List<(string service, string url, bool isHealthy)>();

        try
        {
            // Check ErrorService
            var errorServiceUrl = _configuration["ErrorService:BaseUrl"];
            if (!string.IsNullOrEmpty(errorServiceUrl))
            {
                var isHealthy = await CheckServiceHealthAsync($"{errorServiceUrl}/health");
                checks.Add(("ErrorService", errorServiceUrl, isHealthy));
            }

            // Check NotificationService
            var notificationServiceUrl = _configuration["NotificationService:BaseUrl"];
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