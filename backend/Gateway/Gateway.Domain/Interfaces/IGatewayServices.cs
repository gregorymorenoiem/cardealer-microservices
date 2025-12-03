namespace Gateway.Domain.Interfaces;

/// <summary>
/// Service for managing gateway routing
/// </summary>
public interface IRoutingService
{
    Task<bool> RouteExists(string upstreamPath);
    Task<string> ResolveDownstreamPath(string upstreamPath);
}

/// <summary>
/// Service for tracking gateway metrics
/// </summary>
public interface IMetricsService
{
    void RecordRequest(string route, string method, int statusCode, double durationSeconds);
    void RecordDownstreamCall(string service, double latencySeconds, bool success);
}

/// <summary>
/// Service for health checking downstream services
/// </summary>
public interface IHealthCheckService
{
    Task<bool> IsServiceHealthy(string serviceName);
    Task<Dictionary<string, bool>> GetAllServicesHealth();
}
