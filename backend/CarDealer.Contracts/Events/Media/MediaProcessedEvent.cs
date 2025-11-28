using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Media;

/// <summary>
/// Event published when media processing completes successfully.
/// </summary>
public class MediaProcessedEvent : EventBase
{
    public override string EventType => "media.processed";
    
    public Guid MediaId { get; set; }
    public string ProcessingType { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public Dictionary<string, object>? Results { get; set; }
}
