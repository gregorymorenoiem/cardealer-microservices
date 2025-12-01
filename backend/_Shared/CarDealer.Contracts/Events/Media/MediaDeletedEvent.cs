using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Media;

/// <summary>
/// Event published when a media file is deleted.
/// </summary>
public class MediaDeletedEvent : EventBase
{
    public override string EventType => "media.deleted";

    public Guid MediaId { get; set; }
    public Guid DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}
