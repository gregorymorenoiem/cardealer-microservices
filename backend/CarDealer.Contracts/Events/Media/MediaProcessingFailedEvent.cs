using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Media;

/// <summary>
/// Event published when media processing fails.
/// </summary>
public class MediaProcessingFailedEvent : EventBase
{
    public override string EventType => "media.processing.failed";
    
    public Guid MediaId { get; set; }
    public string ProcessingType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
}
