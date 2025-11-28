namespace AuditService.Shared.Settings;

/// <summary>
/// Configuration settings for health checks
/// </summary>
public class HealthCheckSettings
{
    /// <summary>
    /// Whether health checks are enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Database health check timeout in seconds
    /// </summary>
    public int DatabaseTimeout { get; set; } = 30;

    /// <summary>
    /// Redis health check timeout in seconds
    /// </summary>
    public int RedisTimeout { get; set; } = 10;

    /// <summary>
    /// External services health check timeout in seconds
    /// </summary>
    public int ExternalServicesTimeout { get; set; } = 30;

    /// <summary>
    /// Health check endpoint path
    /// </summary>
    public string EndpointPath { get; set; } = "/health";

    /// <summary>
    /// Health check UI endpoint path
    /// </summary>
    public string UiEndpointPath { get; set; } = "/health-ui";

    /// <summary>
    /// Health check database name for testing
    /// </summary>
    public string DatabaseName { get; set; } = "auditservice";

    /// <summary>
    /// Whether to include detailed health information
    /// </summary>
    public bool IncludeDetails { get; set; } = true;

    /// <summary>
    /// Health check evaluation time in seconds
    /// </summary>
    public int EvaluationTimeInSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum history entries to keep
    /// </summary>
    public int MaximumHistoryEntriesPerEndpoint { get; set; } = 100;

    /// <summary>
    /// Minimum seconds between health checks
    /// </summary>
    public int MinimumSecondsBetweenFailureNotifications { get; set; } = 60;
}