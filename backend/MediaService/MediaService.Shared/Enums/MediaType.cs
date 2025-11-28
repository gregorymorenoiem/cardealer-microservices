namespace MediaService.Shared.Enums;

/// <summary>
/// Types of media supported by the service
/// </summary>
public enum MediaType
{
    /// <summary>
    /// Image files (JPEG, PNG, GIF, WebP, etc.)
    /// </summary>
    Image = 1,

    /// <summary>
    /// Video files (MP4, AVI, MOV, etc.)
    /// </summary>
    Video = 2,

    /// <summary>
    /// Document files (PDF, DOC, DOCX, etc.)
    /// </summary>
    Document = 3,

    /// <summary>
    /// Audio files (MP3, WAV, AAC, etc.)
    /// </summary>
    Audio = 4
}

/// <summary>
/// Status of media processing
/// </summary>
public enum MediaStatus
{
    /// <summary>
    /// Media has been uploaded but not processed
    /// </summary>
    Uploaded = 1,

    /// <summary>
    /// Media is currently being processed
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Media has been successfully processed
    /// </summary>
    Processed = 3,

    /// <summary>
    /// Media processing failed
    /// </summary>
    Failed = 4
}

/// <summary>
/// Status of media processing operations
/// </summary>
public enum ProcessingStatus
{
    /// <summary>
    /// Processing has been queued
    /// </summary>
    Queued = 1,

    /// <summary>
    /// Processing is in progress
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Processing completed successfully
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Processing failed
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Processing was cancelled
    /// </summary>
    Cancelled = 5
}