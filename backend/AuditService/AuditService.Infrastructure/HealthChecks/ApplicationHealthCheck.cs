using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AuditService.Infrastructure.HealthChecks;

public class ApplicationHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Simple liveness check — verifies the application process is running and can respond.
        // Does NOT call external services (e.g., google.com) to avoid false negatives when
        // egress is blocked in K8s or network is partitioned. External connectivity checks
        // should be registered with tag "external" and excluded from /health endpoint.
        return Task.FromResult(HealthCheckResult.Healthy("Application process is running"));
    }
}