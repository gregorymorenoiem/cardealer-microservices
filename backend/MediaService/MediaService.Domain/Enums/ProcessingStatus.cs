namespace MediaService.Domain.Enums;

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