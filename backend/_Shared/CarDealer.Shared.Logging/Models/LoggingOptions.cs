namespace CarDealer.Shared.Logging.Models;

/// <summary>
/// Configuration options for centralized logging
/// </summary>
public class LoggingOptions
{
    public const string SectionName = "Logging";

    /// <summary>
    /// Name of the service for log enrichment
    /// </summary>
    public string ServiceName { get; set; } = "UnknownService";

    /// <summary>
    /// Environment name (Development, Staging, Production)
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// Minimum log level
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// Enable console logging
    /// </summary>
    public bool EnableConsole { get; set; } = true;

    // ============================================================================
    // Convenience properties for flat configuration
    // ============================================================================

    /// <summary>
    /// Enable Seq logging (convenience property)
    /// </summary>
    public bool SeqEnabled { get => Seq.Enabled; set => Seq.Enabled = value; }

    /// <summary>
    /// Seq server URL (convenience property)
    /// </summary>
    public string SeqServerUrl { get => Seq.ServerUrl; set => Seq.ServerUrl = value; }

    /// <summary>
    /// Enable file logging (convenience property)
    /// </summary>
    public bool FileEnabled { get => File.Enabled; set => File.Enabled = value; }

    /// <summary>
    /// File path for logs (convenience property)
    /// </summary>
    public string FilePath { get => File.Path; set => File.Path = value; }

    /// <summary>
    /// Enable RabbitMQ logging (convenience property)
    /// </summary>
    public bool RabbitMQEnabled { get => RabbitMQ.Enabled; set => RabbitMQ.Enabled = value; }

    /// <summary>
    /// RabbitMQ host (convenience property)
    /// </summary>
    public string RabbitMQHost { get; set; } = "rabbitmq";

    /// <summary>
    /// RabbitMQ port (convenience property)
    /// </summary>
    public int RabbitMQPort { get; set; } = 5672;

    /// <summary>
    /// RabbitMQ user (convenience property)
    /// </summary>
    public string RabbitMQUser { get; set; } = "guest";

    /// <summary>
    /// RabbitMQ password (convenience property)
    /// </summary>
    public string RabbitMQPassword { get; set; } = "guest";

    // ============================================================================
    // Nested configuration objects
    // ============================================================================

    /// <summary>
    /// Seq configuration
    /// </summary>
    public SeqOptions Seq { get; set; } = new();

    /// <summary>
    /// RabbitMQ configuration for log shipping
    /// </summary>
    public RabbitMQLogOptions RabbitMQ { get; set; } = new();

    /// <summary>
    /// File logging configuration
    /// </summary>
    public FileLogOptions File { get; set; } = new();
}

/// <summary>
/// Seq logging configuration
/// </summary>
public class SeqOptions
{
    /// <summary>
    /// Enable Seq logging
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Seq server URL
    /// </summary>
    public string ServerUrl { get; set; } = "http://seq:5341";

    /// <summary>
    /// API key for Seq (optional)
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Minimum level for Seq
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";
}

/// <summary>
/// RabbitMQ log sink configuration
/// </summary>
public class RabbitMQLogOptions
{
    /// <summary>
    /// Enable RabbitMQ log shipping
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// RabbitMQ hostnames
    /// </summary>
    public string[] Hostnames { get; set; } = { "rabbitmq" };

    /// <summary>
    /// RabbitMQ port
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// RabbitMQ username
    /// </summary>
    public string Username { get; set; } = "guest";

    /// <summary>
    /// RabbitMQ password
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Exchange name for logs
    /// </summary>
    public string Exchange { get; set; } = "logs.exchange";

    /// <summary>
    /// Exchange type
    /// </summary>
    public string ExchangeType { get; set; } = "topic";

    /// <summary>
    /// Routing key pattern
    /// </summary>
    public string RouteKey { get; set; } = "logs.{Level}";

    /// <summary>
    /// Minimum level for RabbitMQ
    /// </summary>
    public string MinimumLevel { get; set; } = "Warning";
}

/// <summary>
/// File logging configuration
/// </summary>
public class FileLogOptions
{
    /// <summary>
    /// Enable file logging
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Log file path
    /// </summary>
    public string Path { get; set; } = "logs/log-.txt";

    /// <summary>
    /// Rolling interval
    /// </summary>
    public string RollingInterval { get; set; } = "Day";

    /// <summary>
    /// Retained file count limit
    /// </summary>
    public int RetainedFileCountLimit { get; set; } = 7;
}
