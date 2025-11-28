namespace MediaService.Domain.Exceptions;

/// <summary>
/// Exception thrown when media processing fails
/// </summary>
public class MediaProcessingException : Exception
{
    public string MediaId { get; }
    public string? ProcessingStep { get; }

    public MediaProcessingException(string mediaId, string message)
        : base($"Processing failed for media '{mediaId}': {message}")
    {
        MediaId = mediaId;
    }

    public MediaProcessingException(string mediaId, string processingStep, string message)
        : base($"Processing failed for media '{mediaId}' at step '{processingStep}': {message}")
    {
        MediaId = mediaId;
        ProcessingStep = processingStep;
    }

    public MediaProcessingException(string mediaId, string message, Exception innerException)
        : base($"Processing failed for media '{mediaId}': {message}", innerException)
    {
        MediaId = mediaId;
    }

    public MediaProcessingException(string mediaId, string processingStep, string message, Exception innerException)
        : base($"Processing failed for media '{mediaId}' at step '{processingStep}': {message}", innerException)
    {
        MediaId = mediaId;
        ProcessingStep = processingStep;
    }
}