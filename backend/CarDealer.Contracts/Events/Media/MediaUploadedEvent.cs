using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Media;

/// <summary>
/// Event published when a media file is uploaded.
/// </summary>
public class MediaUploadedEvent : EventBase
{
    public override string EventType => "media.uploaded";

    public Guid MediaId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
}
