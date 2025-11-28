using MediaService.Domain.Common;

namespace MediaService.Domain.Events;

/// <summary>
/// Event raised when a media file is processed
/// </summary>
public class MediaProcessedEvent : DomainEvent
{
    /// <summary>
    /// ID of the processed media
    /// </summary>
    public string MediaId { get; }

    /// <summary>
    /// ID of the media owner
    /// </summary>
    public string OwnerId { get; }

    /// <summary>
    /// Type of media processed
    /// </summary>
    public Enums.MediaType MediaType { get; }

    /// <summary>
    /// Processing status
    /// </summary>
    public Enums.ProcessingStatus Status { get; }

    /// <summary>
    /// Error message if processing failed
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Number of variants generated
    /// </summary>
    public int VariantsGenerated { get; }

    /// <summary>
    /// CDN URL for the processed media
    /// </summary>
    public string? CdnUrl { get; }

    /// <summary>
    /// Processing duration in milliseconds
    /// </summary>
    public long? ProcessingDurationMs { get; }

    public MediaProcessedEvent(
        string mediaId,
        string ownerId,
        Enums.MediaType mediaType,
        Enums.ProcessingStatus status,
        int variantsGenerated = 0,
        string? cdnUrl = null,
        long? processingDurationMs = null,
        string? errorMessage = null)
    {
        MediaId = mediaId;
        OwnerId = ownerId;
        MediaType = mediaType;
        Status = status;
        VariantsGenerated = variantsGenerated;
        CdnUrl = cdnUrl;
        ProcessingDurationMs = processingDurationMs;
        ErrorMessage = errorMessage;

        // Set metadata
        Metadata["mediaId"] = mediaId;
        Metadata["ownerId"] = ownerId;
        Metadata["mediaType"] = mediaType.ToString();
        Metadata["status"] = status.ToString();
        Metadata["variantsGenerated"] = variantsGenerated;

        if (!string.IsNullOrEmpty(cdnUrl))
            Metadata["cdnUrl"] = cdnUrl;

        if (processingDurationMs.HasValue)
            Metadata["processingDurationMs"] = processingDurationMs.Value;

        if (!string.IsNullOrEmpty(errorMessage))
            Metadata["errorMessage"] = errorMessage;
    }
}