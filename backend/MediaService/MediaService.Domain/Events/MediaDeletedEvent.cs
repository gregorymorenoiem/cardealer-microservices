using MediaService.Domain.Common;

namespace MediaService.Domain.Events;

/// <summary>
/// Event raised when a media file is deleted
/// </summary>
public class MediaDeletedEvent : DomainEvent
{
    /// <summary>
    /// ID of the deleted media
    /// </summary>
    public string MediaId { get; }

    /// <summary>
    /// ID of the media owner
    /// </summary>
    public string OwnerId { get; }

    /// <summary>
    /// Type of media deleted
    /// </summary>
    public Enums.MediaType MediaType { get; }

    /// <summary>
    /// Storage keys that were deleted
    /// </summary>
    public List<string> DeletedStorageKeys { get; }

    /// <summary>
    /// Reason for deletion
    /// </summary>
    public string? Reason { get; }

    /// <summary>
    /// User ID who performed the deletion
    /// </summary>
    public string DeletedBy { get; }

    public MediaDeletedEvent(
        string mediaId,
        string ownerId,
        Enums.MediaType mediaType,
        List<string> deletedStorageKeys,
        string deletedBy,
        string? reason = null)
    {
        MediaId = mediaId;
        OwnerId = ownerId;
        MediaType = mediaType;
        DeletedStorageKeys = deletedStorageKeys;
        DeletedBy = deletedBy;
        Reason = reason;

        // Set metadata
        Metadata["mediaId"] = mediaId;
        Metadata["ownerId"] = ownerId;
        Metadata["mediaType"] = mediaType.ToString();
        Metadata["deletedStorageKeys"] = deletedStorageKeys;
        Metadata["deletedBy"] = deletedBy;
        Metadata["storageKeysCount"] = deletedStorageKeys.Count;

        if (!string.IsNullOrEmpty(reason))
            Metadata["reason"] = reason;
    }
}