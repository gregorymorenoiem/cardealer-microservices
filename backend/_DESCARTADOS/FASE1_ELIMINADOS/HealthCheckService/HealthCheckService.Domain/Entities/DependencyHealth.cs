using HealthCheckService.Domain.Enums;

namespace HealthCheckService.Domain.Entities;

/// <summary>
/// Represents a health check result for a specific dependency
/// </summary>
public class DependencyHealth
{
    public string Name { get; set; } = string.Empty;
    public DependencyType Type { get; set; }
    public HealthStatus Status { get; set; }
    public string? Description { get; set; }
    public long? ResponseTimeMs { get; set; }
    public DateTime CheckedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();

    public bool IsHealthy() => Status == HealthStatus.Healthy;
    public bool IsDegraded() => Status == HealthStatus.Degraded;
    public bool IsUnhealthy() => Status == HealthStatus.Unhealthy;
}
