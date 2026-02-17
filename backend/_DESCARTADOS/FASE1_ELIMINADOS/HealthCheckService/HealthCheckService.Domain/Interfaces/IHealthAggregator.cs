using HealthCheckService.Domain.Entities;

namespace HealthCheckService.Domain.Interfaces;

/// <summary>
/// Interface for aggregating health checks from multiple services
/// </summary>
public interface IHealthAggregator
{
    /// <summary>
    /// Gets the health status of all registered services
    /// </summary>
    Task<SystemHealth> GetSystemHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the health status of a specific service
    /// </summary>
    Task<ServiceHealth?> GetServiceHealthAsync(string serviceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a service for health checking
    /// </summary>
    void RegisterService(string serviceName, string serviceUrl);

    /// <summary>
    /// Unregisters a service from health checking
    /// </summary>
    void UnregisterService(string serviceName);

    /// <summary>
    /// Gets all registered services
    /// </summary>
    IEnumerable<string> GetRegisteredServices();
}
