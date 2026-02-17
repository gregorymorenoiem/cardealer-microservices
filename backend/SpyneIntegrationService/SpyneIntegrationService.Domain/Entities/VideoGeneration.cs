using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Domain.Entities;

/// <summary>
/// Represents a video tour generation request/result from Spyne AI
/// </summary>
public class VideoGeneration
{
    public Guid Id { get; set; }
    
    /// <summary>Vehicle this video belongs to</summary>
    public Guid VehicleId { get; set; }
    
    /// <summary>Dealer who owns the vehicle</summary>
    public Guid? DealerId { get; set; }
    
    /// <summary>Spyne's internal job ID</summary>
    public string? SpyneJobId { get; set; }
    
    /// <summary>Source image URLs used to generate video</summary>
    public List<string> SourceImageUrls { get; set; } = new();
    
    /// <summary>Generated video URL</summary>
    public string? VideoUrl { get; set; }
    
    /// <summary>Video thumbnail URL</summary>
    public string? ThumbnailUrl { get; set; }
    
    /// <summary>Requested video duration in seconds</summary>
    public int? RequestedDuration { get; set; }
    
    /// <summary>Actual video duration in seconds</summary>
    public int DurationSeconds { get; set; }
    
    /// <summary>Video style used</summary>
    public VideoStyle Style { get; set; }
    
    /// <summary>Video format/orientation (alias for OutputFormat)</summary>
    public VideoFormat Format { get; set; }
    
    /// <summary>Output format including quality</summary>
    public VideoFormat OutputFormat 
    { 
        get => Format; 
        set => Format = value; 
    }
    
    /// <summary>Background preset used</summary>
    public BackgroundPreset BackgroundPreset { get; set; } = BackgroundPreset.Studio;
    
    /// <summary>Custom background ID if using custom</summary>
    public string? CustomBackgroundId { get; set; }
    
    /// <summary>Whether background music was included</summary>
    public bool IncludeMusic { get; set; }
    
    /// <summary>Music track ID if using specific track</summary>
    public string? MusicTrackId { get; set; }
    
    /// <summary>Branding logo URL overlaid on video</summary>
    public string? BrandingLogoUrl { get; set; }
    
    /// <summary>Current generation status</summary>
    public TransformationStatus Status { get; set; }
    
    /// <summary>Error message if failed</summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>Processing time in milliseconds</summary>
    public int? ProcessingTimeMs { get; set; }
    
    /// <summary>Video file size in bytes</summary>
    public long? FileSizeBytes { get; set; }
    
    /// <summary>Credits/cost consumed</summary>
    public decimal CreditsCost { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public VideoGeneration()
    {
        Id = Guid.NewGuid();
        Status = TransformationStatus.Pending;
        DurationSeconds = 30; // Default 30 seconds
        Style = VideoStyle.Cinematic;
        Format = VideoFormat.Horizontal;
        BackgroundPreset = BackgroundPreset.Studio;
        IncludeMusic = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessing(string spyneJobId)
    {
        SpyneJobId = spyneJobId;
        Status = TransformationStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted(string videoUrl, string thumbnailUrl, long fileSizeBytes, int processingTimeMs)
    {
        VideoUrl = videoUrl;
        ThumbnailUrl = thumbnailUrl;
        FileSizeBytes = fileSizeBytes;
        Status = TransformationStatus.Completed;
        ProcessingTimeMs = processingTimeMs;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        ErrorMessage = errorMessage;
        Status = TransformationStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }
}
