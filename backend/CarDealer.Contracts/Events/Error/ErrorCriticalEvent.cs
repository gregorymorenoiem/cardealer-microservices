using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Error;

/// <summary>
/// Event published when a critical error occurs in any service (HTTP 500+).
/// This triggers alerts to Microsoft Teams and other monitoring systems.
/// </summary>
public class ErrorCriticalEvent : EventBase
{
    public override string EventType => "error.critical";

    /// <summary>
    /// Unique identifier of the error log entry.
    /// </summary>
    public Guid ErrorId { get; set; }

    /// <summary>
    /// Name of the service where the error occurred.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Type of exception that was thrown.
    /// </summary>
    public string ExceptionType { get; set; } = string.Empty;

    /// <summary>
    /// Error message describing what went wrong.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Stack trace of the exception (optional for security).
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// HTTP status code (500, 503, etc.).
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// API endpoint where the error occurred.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// User ID associated with the request (if available).
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Additional metadata about the error context.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}
