using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace AuthService.Infrastructure.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;

    public RedisHealthCheck(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var result = await db.PingAsync();

            if (result < TimeSpan.FromMilliseconds(100))
            {
                return HealthCheckResult.Healthy("Redis is healthy");
            }
            else
            {
                return HealthCheckResult.Degraded("Redis is slow");
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis is unavailable", ex);
        }
    }
}