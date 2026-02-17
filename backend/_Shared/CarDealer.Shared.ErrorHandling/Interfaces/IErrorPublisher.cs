using CarDealer.Shared.ErrorHandling.Models;

namespace CarDealer.Shared.ErrorHandling.Interfaces;

/// <summary>
/// Interface for publishing errors to ErrorService
/// </summary>
public interface IErrorPublisher
{
    /// <summary>
    /// Publishes an error event asynchronously
    /// </summary>
    /// <param name="errorEvent">The error event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync(ErrorEvent errorEvent, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Publishes an error from an exception
    /// </summary>
    /// <param name="exception">The exception</param>
    /// <param name="context">Additional context information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishExceptionAsync(
        Exception exception, 
        ErrorContext? context = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Additional context for error events
/// </summary>
public class ErrorContext
{
    /// <summary>
    /// HTTP request path
    /// </summary>
    public string? RequestPath { get; set; }
    
    /// <summary>
    /// HTTP method
    /// </summary>
    public string? RequestMethod { get; set; }
    
    /// <summary>
    /// User ID
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// Correlation ID
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Trace ID
    /// </summary>
    public string? TraceId { get; set; }
    
    /// <summary>
    /// Span ID
    /// </summary>
    public string? SpanId { get; set; }
    
    /// <summary>
    /// Client IP
    /// </summary>
    public string? ClientIp { get; set; }
    
    /// <summary>
    /// User agent
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Error category override
    /// </summary>
    public ErrorCategory? Category { get; set; }
    
    /// <summary>
    /// Error severity override
    /// </summary>
    public ErrorSeverity? Severity { get; set; }
    
    /// <summary>
    /// Additional data
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }
}
