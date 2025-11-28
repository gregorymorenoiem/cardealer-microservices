namespace MediaService.Domain.Exceptions;

/// <summary>
/// Exception thrown when a media entity is not found
/// </summary>
public class MediaNotFoundException : Exception
{
    public string MediaId { get; }

    public MediaNotFoundException(string mediaId)
        : base($"Media with ID '{mediaId}' was not found.")
    {
        MediaId = mediaId;
    }

    public MediaNotFoundException(string mediaId, Exception innerException)
        : base($"Media with ID '{mediaId}' was not found.", innerException)
    {
        MediaId = mediaId;
    }
}