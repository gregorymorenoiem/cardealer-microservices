namespace MediaService.Domain.Enums;

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