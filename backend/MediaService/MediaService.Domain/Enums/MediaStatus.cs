namespace MediaService.Domain.Enums;

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