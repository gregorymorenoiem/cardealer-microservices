using HealthCheckService.Domain.Enums;

namespace HealthCheckService.Domain.Entities;

/// <summary>
/// Represents the health check result for a single service
/// </summary>
public class ServiceHealth
{
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceUrl { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime CheckedAt { get; set; }
    public long? ResponseTimeMs { get; set; }
    public List<DependencyHealth> Dependencies { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();

    public bool IsHealthy() => Status == HealthStatus.Healthy;
    public bool IsDegraded() => Status == HealthStatus.Degraded;
    public bool IsUnhealthy() => Status == HealthStatus.Unhealthy;

    public HealthStatus CalculateOverallStatus()
    {
        if (Dependencies.Count == 0)
            return Status;

        var hasUnhealthy = Dependencies.Any(d => d.IsUnhealthy());
        var hasDegraded = Dependencies.Any(d => d.IsDegraded());

        if (hasUnhealthy)
            return HealthStatus.Unhealthy;
        if (hasDegraded)
            return HealthStatus.Degraded;

        return HealthStatus.Healthy;
    }
}
