using Microsoft.Extensions.Diagnostics.HealthChecks;
public class ApplicationHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificaciones básicas de la aplicación
            var isHealthy = true;
            var data = new Dictionary<string, object>
            {
                { "timestamp", DateTime.UtcNow },
                { "version", "1.0.0" }
            };

            if (isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Application is healthy", data));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Application is unhealthy", data: data));
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Application health check failed", ex));
        }
    }
}
