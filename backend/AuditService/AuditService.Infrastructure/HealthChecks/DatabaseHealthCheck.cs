using AuditService.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace AuditService.Infrastructure.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AuditDbContext _dbContext;

    public DatabaseHealthCheck(AuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar que podemos conectar a la base de datos
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            if (canConnect)
            {
                // Verificar que podemos ejecutar una consulta simple
                var auditLogsCount = await _dbContext.AuditLogs.CountAsync(cancellationToken);
                return HealthCheckResult.Healthy($"Database is healthy. Total audit logs: {auditLogsCount}");
            }

            return HealthCheckResult.Unhealthy("Cannot connect to database");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database health check failed", ex);
        }
    }
}