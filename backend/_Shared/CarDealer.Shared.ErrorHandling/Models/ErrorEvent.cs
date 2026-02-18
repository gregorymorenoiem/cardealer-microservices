using System.Text.Json.Serialization;

namespace CarDealer.Shared.ErrorHandling.Models;

/// <summary>
/// Represents an error event to be sent to ErrorService
/// </summary>
public class ErrorEvent
{
    /// <summary>
    /// Unique identifier for this error event
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// When the error occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Service name where the error occurred
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Environment (Development, Staging, Production)
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Error severity level
    /// </summary>
    public ErrorSeverity Severity { get; set; } = ErrorSeverity.Error;

    /// <summary>
    /// Error category for classification
    /// </summary>
    public ErrorCategory Category { get; set; } = ErrorCategory.Unhandled;

    /// <summary>
    /// Exception type name
    /// </summary>
    public string ExceptionType { get; set; } = string.Empty;

    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Stack trace (if available)
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Inner exception details
    /// </summary>
    public string? InnerException { get; set; }

    /// <summary>
    /// HTTP request path (if applicable)
    /// </summary>
    public string? RequestPath { get; set; }

    /// <summary>
    /// HTTP method (if applicable)
    /// </summary>
    public string? RequestMethod { get; set; }

    /// <summary>
    /// User ID (if authenticated)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Correlation ID for distributed tracing
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// OpenTelemetry Trace ID
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// OpenTelemetry Span ID
    /// </summary>
    public string? SpanId { get; set; }

    /// <summary>
    /// Client IP address
    /// </summary>
    public string? ClientIp { get; set; }

    /// <summary>
    /// User agent string
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Additional context data
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }
}

/// <summary>
/// Error severity levels
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorSeverity
{
    /// <summary>Debug level - for development</summary>
    Debug,

    /// <summary>Information - not an actual error</summary>
    Info,

    /// <summary>Warning - potential issue</summary>
    Warning,

    /// <summary>Error - recoverable error</summary>
    Error,

    /// <summary>Critical - severe error, may need immediate attention</summary>
    Critical,

    /// <summary>Fatal - application cannot continue</summary>
    Fatal
}

/// <summary>
/// Error categories for classification
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorCategory
{
    /// <summary>Unhandled exception</summary>
    Unhandled,

    /// <summary>Validation error</summary>
    Validation,

    /// <summary>Authentication error</summary>
    Authentication,

    /// <summary>Authorization error</summary>
    Authorization,

    /// <summary>Not found error</summary>
    NotFound,

    /// <summary>Conflict error</summary>
    Conflict,

    /// <summary>Business logic error</summary>
    Business,

    /// <summary>Database error</summary>
    Database,

    /// <summary>External service error</summary>
    ExternalService,

    /// <summary>Timeout error</summary>
    Timeout,

    /// <summary>Rate limit exceeded</summary>
    RateLimit,

    /// <summary>Infrastructure error</summary>
    Infrastructure
}
