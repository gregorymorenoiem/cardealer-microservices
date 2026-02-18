namespace CarDealer.Shared.ErrorHandling.Models;

/// <summary>
/// Configuration options for error handling
/// </summary>
public class ErrorHandlingOptions
{
    public const string SectionName = "ErrorHandling";

    /// <summary>
    /// Name of the service for error tracking
    /// </summary>
    public string ServiceName { get; set; } = "UnknownService";

    /// <summary>
    /// Environment name (Development, Staging, Production)
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// Include stack traces in production
    /// </summary>
    public bool IncludeStackTrace { get; set; } = false;

    /// <summary>
    /// Include exception details in response
    /// </summary>
    public bool IncludeExceptionDetails { get; set; } = false;

    /// <summary>
    /// Enable publishing errors to ErrorService
    /// </summary>
    public bool PublishToErrorService { get; set; } = true;

    /// <summary>
    /// RabbitMQ hostname (convenience property)
    /// </summary>
    public string RabbitMQHost { get; set; } = "rabbitmq";

    /// <summary>
    /// RabbitMQ port (convenience property)
    /// </summary>
    public int RabbitMQPort { get; set; } = 5672;

    /// <summary>
    /// RabbitMQ username (convenience property)
    /// </summary>
    public string RabbitMQUser { get; set; } = "guest";

    /// <summary>
    /// RabbitMQ password (convenience property)
    /// </summary>
    public string RabbitMQPassword { get; set; } = "guest";

    /// <summary>
    /// ErrorService API configuration
    /// </summary>
    public ErrorServiceOptions ErrorService { get; set; } = new();

    /// <summary>
    /// RabbitMQ configuration for error publishing
    /// </summary>
    public RabbitMQErrorOptions RabbitMQ { get; set; } = new();
}

/// <summary>
/// ErrorService HTTP client configuration
/// </summary>
public class ErrorServiceOptions
{
    /// <summary>
    /// Enable sending errors to ErrorService
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// ErrorService base URL
    /// </summary>
    public string BaseUrl { get; set; } = "http://errorservice:80";

    /// <summary>
    /// Timeout in seconds for HTTP requests
    /// </summary>
    public int TimeoutSeconds { get; set; } = 5;
}

/// <summary>
/// RabbitMQ configuration for error publishing
/// </summary>
public class RabbitMQErrorOptions
{
    /// <summary>
    /// Enable publishing errors to RabbitMQ
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// RabbitMQ hostname
    /// </summary>
    public string Hostname { get; set; } = "rabbitmq";

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
    /// Exchange name for errors
    /// </summary>
    public string Exchange { get; set; } = "errors.exchange";

    /// <summary>
    /// Queue name for errors
    /// </summary>
    public string Queue { get; set; } = "errors.queue";

    /// <summary>
    /// Routing key for errors
    /// </summary>
    public string RoutingKey { get; set; } = "error.created";
}
