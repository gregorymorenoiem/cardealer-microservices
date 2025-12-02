namespace TracingService.Domain.Enums;

/// <summary>
/// Defines the status of a span
/// Following OpenTelemetry StatusCode specification
/// </summary>
public enum SpanStatus
{
    /// <summary>
    /// The default status.
    /// </summary>
    Unset = 0,
    
    /// <summary>
    /// The operation completed successfully.
    /// </summary>
    Ok = 1,
    
    /// <summary>
    /// The operation contains an error.
    /// </summary>
    Error = 2
}
