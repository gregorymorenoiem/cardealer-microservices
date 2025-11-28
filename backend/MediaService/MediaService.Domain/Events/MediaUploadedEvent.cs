using MediaService.Domain.Common;

namespace MediaService.Domain.Events;

/// <summary>
/// Event raised when a media file is uploaded
/// </summary>
public class MediaUploadedEvent : DomainEvent
{
    /// <summary>
    /// ID of the uploaded media
    /// </summary>
    public string MediaId { get; }

    /// <summary>
    /// ID of the user who uploaded the media
    /// </summary>
    public string OwnerId { get; }

    /// <summary>
    /// Context of the upload (profile, product, etc.)
    /// </summary>
    public string? Context { get; }

    /// <summary>
    /// Type of media uploaded
    /// </summary>
    public Enums.MediaType MediaType { get; }

    /// <summary>
    /// Original file name
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Content type of the uploaded file
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// Size of the uploaded file in bytes
    /// </summary>
    public long FileSize { get; }

    /// <summary>
    /// Storage key for the uploaded file
    /// </summary>
    public string StorageKey { get; }

    public MediaUploadedEvent(
        string mediaId,
        string ownerId,
        string? context,
        Enums.MediaType mediaType,
        string fileName,
        string contentType,
        long fileSize,
        string storageKey)
    {
        MediaId = mediaId;
        OwnerId = ownerId;
        Context = context;
        MediaType = mediaType;
        FileName = fileName;
        ContentType = contentType;
        FileSize = fileSize;
        StorageKey = storageKey;

        // Set metadata
        Metadata["mediaId"] = mediaId;
        Metadata["ownerId"] = ownerId;
        Metadata["mediaType"] = mediaType.ToString();
        Metadata["fileSize"] = fileSize;

        if (!string.IsNullOrEmpty(context))
            Metadata["context"] = context;
    }
}