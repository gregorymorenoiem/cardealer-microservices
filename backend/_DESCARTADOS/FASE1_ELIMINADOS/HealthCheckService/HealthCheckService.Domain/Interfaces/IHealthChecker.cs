using HealthCheckService.Domain.Entities;

namespace HealthCheckService.Domain.Interfaces;

/// <summary>
/// Interface for performing health checks on individual services
/// </summary>
public interface IHealthChecker
{
    /// <summary>
    /// Performs a health check on a service endpoint
    /// </summary>
    Task<ServiceHealth> CheckServiceHealthAsync(string serviceUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a service endpoint is reachable
    /// </summary>
    Task<bool> IsServiceReachableAsync(string serviceUrl, CancellationToken cancellationToken = default);
}
