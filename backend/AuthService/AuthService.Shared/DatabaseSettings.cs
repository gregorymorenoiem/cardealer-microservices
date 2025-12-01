namespace AuthService.Shared;

/// <summary>
/// Database configuration settings
/// </summary>
public class DatabaseSettings
{
    /// <summary>Database connection string</summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>Database provider (PostgreSQL, SQLServer, etc.)</summary>
    public string Provider { get; set; } = "PostgreSQL";

    /// <summary>Whether to enable sensitive data logging</summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>Whether to enable detailed errors</summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>Command timeout in seconds</summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>Maximum number of retry attempts</summary>
    public int MaxRetryCount { get; set; } = 3;
}
