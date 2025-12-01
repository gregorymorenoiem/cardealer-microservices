using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Error;

/// <summary>
/// Event published when any error is logged in the system.
/// </summary>
public class ErrorLoggedEvent : EventBase
{
    public override string EventType => "error.logged";

    /// <summary>
    /// Unique identifier of the error log entry.
    /// </summary>
    public Guid ErrorId { get; set; }

    /// <summary>
    /// Name of the service where the error occurred.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the error was logged.
    /// </summary>
    public DateTime LoggedAt { get; set; }
}
