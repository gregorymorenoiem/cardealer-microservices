namespace HealthCheckService.Domain.Enums;

/// <summary>
/// Represents the overall health status of a service or dependency
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// Service is fully operational
    /// </summary>
    Healthy = 0,

    /// <summary>
    /// Service is operational but with some degraded functionality
    /// </summary>
    Degraded = 1,

    /// <summary>
    /// Service is not operational
    /// </summary>
    Unhealthy = 2,

    /// <summary>
    /// Health status cannot be determined
    /// </summary>
    Unknown = 3
}
