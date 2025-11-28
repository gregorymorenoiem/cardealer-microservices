using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AuditService.Infrastructure.HealthChecks;

public class ApplicationHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ApplicationHealthCheck(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar que la aplicación puede hacer requests HTTP básicos
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("http://google.com", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("Application is healthy and can make external requests");
            }

            return HealthCheckResult.Degraded($"External request failed with status: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Application cannot make external requests", ex);
        }
    }
}