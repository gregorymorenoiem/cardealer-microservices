using Microsoft.Extensions.Diagnostics.HealthChecks;
using MediaService.Domain.Interfaces.Services;

namespace MediaService.Infrastructure.HealthChecks;

public class StorageHealthCheck : IHealthCheck
{
    private readonly IMediaStorageService _storageService;

    public StorageHealthCheck(IMediaStorageService storageService)
    {
        _storageService = storageService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var isHealthy = await _storageService.IsHealthyAsync();
            if (isHealthy)
            {
                return HealthCheckResult.Healthy("Storage service is available.");
            }
            else
            {
                return HealthCheckResult.Unhealthy("Storage service is unavailable.");
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Storage health check failed.", ex);
        }
    }
}