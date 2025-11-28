using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Error;

/// <summary>
/// Event published when an unusual spike in errors is detected.
/// </summary>
public class ErrorSpikeDetectedEvent : EventBase
{
    public override string EventType => "error.spike.detected";
    
    /// <summary>
    /// Name of the service experiencing the error spike.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of errors in the detection window.
    /// </summary>
    public int ErrorCount { get; set; }
    
    /// <summary>
    /// Duration of the detection window in minutes.
    /// </summary>
    public int WindowMinutes { get; set; }
    
    /// <summary>
    /// Threshold that was exceeded to trigger this event.
    /// </summary>
    public int Threshold { get; set; }
    
    /// <summary>
    /// Timestamp when the spike was detected.
    /// </summary>
    public DateTime DetectedAt { get; set; }
}
