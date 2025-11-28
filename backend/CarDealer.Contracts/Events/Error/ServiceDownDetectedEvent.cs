using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Error;

/// <summary>
/// Event published when a service is detected as being down or unresponsive.
/// </summary>
public class ServiceDownDetectedEvent : EventBase
{
    public override string EventType => "error.service.down";

    /// <summary>
    /// Name of the service that is down.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Reason or error message indicating why the service is down.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the service was detected as down.
    /// </summary>
    public DateTime DetectedAt { get; set; }

    /// <summary>
    /// Last successful health check timestamp.
    /// </summary>
    public DateTime? LastHealthyAt { get; set; }
}
