using HealthCheckService.Domain.Enums;

namespace HealthCheckService.Domain.Entities;

/// <summary>
/// Represents the aggregated health status of the entire system
/// </summary>
public class SystemHealth
{
    public HealthStatus OverallStatus { get; set; }
    public DateTime CheckedAt { get; set; }
    public int TotalServices { get; set; }
    public int HealthyServices { get; set; }
    public int DegradedServices { get; set; }
    public int UnhealthyServices { get; set; }
    public List<ServiceHealth> Services { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();

    public double HealthPercentage()
    {
        if (TotalServices == 0)
            return 100.0;

        return (double)HealthyServices / TotalServices * 100.0;
    }

    public bool IsFullyOperational() => OverallStatus == HealthStatus.Healthy;
    public bool HasIssues() => OverallStatus != HealthStatus.Healthy;

    public void CalculateOverallStatus()
    {
        TotalServices = Services.Count;
        HealthyServices = Services.Count(s => s.IsHealthy());
        DegradedServices = Services.Count(s => s.IsDegraded());
        UnhealthyServices = Services.Count(s => s.IsUnhealthy());

        if (UnhealthyServices > 0)
        {
            OverallStatus = HealthStatus.Unhealthy;
        }
        else if (DegradedServices > 0)
        {
            OverallStatus = HealthStatus.Degraded;
        }
        else if (HealthyServices == TotalServices)
        {
            OverallStatus = HealthStatus.Healthy;
        }
        else
        {
            OverallStatus = HealthStatus.Unknown;
        }

        CheckedAt = DateTime.UtcNow;
    }
}
