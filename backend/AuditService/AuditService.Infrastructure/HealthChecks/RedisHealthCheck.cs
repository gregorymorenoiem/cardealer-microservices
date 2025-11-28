using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AuditService.Infrastructure.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IDistributedCache _cache;

    public RedisHealthCheck(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar que Redis está respondiendo
            var testKey = "health_check_test";
            var testValue = DateTime.UtcNow.ToString("O");

            await _cache.SetStringAsync(testKey, testValue, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            }, cancellationToken);

            var retrievedValue = await _cache.GetStringAsync(testKey, cancellationToken);

            if (retrievedValue == testValue)
            {
                await _cache.RemoveAsync(testKey, cancellationToken);
                return HealthCheckResult.Healthy("Redis is healthy and responding correctly");
            }

            return HealthCheckResult.Unhealthy("Redis returned incorrect value");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis health check failed", ex);
        }
    }
}