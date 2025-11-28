namespace MediaService.Shared.Settings;

/// <summary>
/// Configuration settings for database connectivity
/// </summary>
public class DatabaseSettings
{
    /// <summary>
    /// Database connection string
    /// </summary>
    public string ConnectionString { get; set; } = "Host=localhost;Database=mediaservice;Username=postgres;Password=password";

    /// <summary>
    /// Database provider (PostgreSQL, SQLServer, SQLite, etc.)
    /// </summary>
    public string Provider { get; set; } = "PostgreSQL";

    /// <summary>
    /// Whether to enable sensitive data logging (should be false in production)
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Whether to enable detailed errors
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Command timeout in seconds
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Maximum number of retry attempts for database operations
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Whether to enable query tracking
    /// </summary>
    public bool EnableQueryTracking { get; set; } = false;

    /// <summary>
    /// Maximum number of records to return in a single query
    /// </summary>
    public int MaxPageSize { get; set; } = 1000;

    /// <summary>
    /// Default page size for paginated queries
    /// </summary>
    public int DefaultPageSize { get; set; } = 50;

    /// <summary>
    /// Database schema to use
    /// </summary>
    public string Schema { get; set; } = "public";

    /// <summary>
    /// Whether to automatically apply migrations on startup
    /// </summary>
    public bool AutoMigrate { get; set; } = true;

    /// <summary>
    /// Whether to enable database health checks
    /// </summary>
    public bool EnableHealthChecks { get; set; } = true;
}