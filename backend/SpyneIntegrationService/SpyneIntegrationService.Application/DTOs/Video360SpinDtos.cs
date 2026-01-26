using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Application.DTOs;

/// <summary>
/// DTO for video-based 360° spin generation
/// </summary>
public record Video360SpinDto
{
    public Guid Id { get; init; }
    public Guid VehicleId { get; init; }
    public Guid? DealerId { get; init; }
    public Guid? UserId { get; init; }
    public string? SpyneJobId { get; init; }
    
    // Input video info
    public string SourceVideoUrl { get; init; } = string.Empty;
    public int? VideoDurationSeconds { get; init; }
    public long? VideoFileSizeBytes { get; init; }
    public string? VideoFormat { get; init; }
    public string? VideoResolution { get; init; }
    
    // Processing options
    public Video360SpinType Type { get; init; }
    public BackgroundPreset BackgroundPreset { get; init; }
    public int FrameCount { get; init; }
    public bool EnableHotspots { get; init; }
    public bool MaskLicensePlate { get; init; }
    
    // Output from Spyne
    public string? SpinViewerUrl { get; init; }
    public string? SpinEmbedCode { get; init; }
    public string? FallbackImageUrl { get; init; }
    public string? PreviewGifUrl { get; init; }
    public string? ThumbnailUrl { get; init; }
    public List<string> ExtractedFrameUrls { get; init; } = new();
    public int ExtractedFrameCount { get; init; }
    
    // Status
    public TransformationStatus Status { get; init; }
    public string? ErrorMessage { get; init; }
    public int ProgressPercent { get; init; }
    public int? ProcessingTimeMs { get; init; }
    public int? QualityScore { get; init; }
    public List<string> QualityIssues { get; init; } = new();
    
    // Timestamps
    public DateTime CreatedAt { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    
    /// <summary>
    /// Whether the 360° spin is ready for viewing
    /// </summary>
    public bool IsReady => Status == TransformationStatus.Completed && !string.IsNullOrEmpty(SpinViewerUrl);
    
    /// <summary>
    /// Whether processing failed
    /// </summary>
    public bool HasFailed => Status == TransformationStatus.Failed;
    
    /// <summary>
    /// Whether currently processing
    /// </summary>
    public bool IsProcessing => Status == TransformationStatus.Processing || 
                                 Status == TransformationStatus.Uploading;
}

/// <summary>
/// Request to generate a 360° spin from a video
/// </summary>
public record GenerateVideo360SpinRequest
{
    /// <summary>Vehicle ID for this 360° spin</summary>
    public Guid VehicleId { get; init; }
    
    /// <summary>Dealer ID (optional, for tracking)</summary>
    public Guid? DealerId { get; init; }
    
    /// <summary>
    /// URL of the uploaded video (must be publicly accessible or signed URL).
    /// Video should be a 360° walkthrough around the vehicle.
    /// Recommended: 30-90 seconds, horizontal orientation, steady movement.
    /// </summary>
    public string VideoUrl { get; init; } = string.Empty;
    
    /// <summary>Video duration in seconds (optional, for validation)</summary>
    public int? VideoDurationSeconds { get; init; }
    
    /// <summary>Video file size in bytes (optional, for validation)</summary>
    public long? VideoFileSizeBytes { get; init; }
    
    /// <summary>Video format (mp4, mov, etc.)</summary>
    public string? VideoFormat { get; init; }
    
    /// <summary>Video resolution (1080p, 4K, etc.)</summary>
    public string? VideoResolution { get; init; }
    
    /// <summary>Type of 360° view (Exterior, Interior, etc.)</summary>
    public Video360SpinType Type { get; init; } = Video360SpinType.Exterior;
    
    /// <summary>Background preset for processed frames</summary>
    public BackgroundPreset BackgroundPreset { get; init; } = BackgroundPreset.Studio;
    
    /// <summary>Custom Spyne background ID (overrides BackgroundPreset)</summary>
    public string? CustomBackgroundId { get; init; }
    
    /// <summary>Number of frames to extract (24, 36, 72). Default 36.</summary>
    public int FrameCount { get; init; } = 36;
    
    /// <summary>Enable interactive hotspots on the 360° view</summary>
    public bool EnableHotspots { get; init; } = true;
    
    /// <summary>Mask/blur license plate in extracted frames</summary>
    public bool MaskLicensePlate { get; init; } = true;
    
    /// <summary>VIN for tracking (optional)</summary>
    public string? Vin { get; init; }
    
    /// <summary>Stock number for tracking (optional)</summary>
    public string? StockNumber { get; init; }
    
    /// <summary>Webhook URL for status notifications (optional)</summary>
    public string? WebhookUrl { get; init; }
}

/// <summary>
/// Response after initiating 360° spin generation
/// </summary>
public record GenerateVideo360SpinResponse
{
    /// <summary>ID of the created Video360Spin record</summary>
    public Guid SpinId { get; init; }
    
    /// <summary>Current status (typically "Processing" after creation)</summary>
    public TransformationStatus Status { get; init; }
    
    /// <summary>Estimated time to complete processing (in minutes)</summary>
    public int EstimatedCompletionMinutes { get; init; }
    
    /// <summary>Message with additional info</summary>
    public string? Message { get; init; }
    
    /// <summary>Spyne job ID for tracking</summary>
    public string? SpyneJobId { get; init; }
    
    /// <summary>Poll this endpoint for status updates</summary>
    public string? StatusCheckUrl { get; init; }
}

/// <summary>
/// Response for 360° spin status check
/// </summary>
public record Video360SpinStatusResponse
{
    public Guid SpinId { get; init; }
    public TransformationStatus Status { get; init; }
    public int ProgressPercent { get; init; }
    
    /// <summary>Available when completed</summary>
    public string? SpinViewerUrl { get; init; }
    
    /// <summary>Embed code for the spin viewer</summary>
    public string? SpinEmbedCode { get; init; }
    
    /// <summary>Thumbnail URL</summary>
    public string? ThumbnailUrl { get; init; }
    
    /// <summary>Number of frames extracted from video</summary>
    public int ExtractedFrameCount { get; init; }
    
    /// <summary>URLs of all extracted frames (for custom viewers)</summary>
    public List<string> ExtractedFrameUrls { get; init; } = new();
    
    /// <summary>Error message if failed</summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>Processing time in seconds</summary>
    public int? ProcessingTimeSeconds { get; init; }
    
    /// <summary>Quality score (0-100)</summary>
    public int? QualityScore { get; init; }
}

/// <summary>
/// Mapper extension methods for Video360Spin
/// </summary>
public static class Video360SpinMapper
{
    public static Video360SpinDto ToDto(this Video360Spin entity)
    {
        return new Video360SpinDto
        {
            Id = entity.Id,
            VehicleId = entity.VehicleId,
            DealerId = entity.DealerId,
            UserId = entity.UserId,
            SpyneJobId = entity.SpyneJobId,
            SourceVideoUrl = entity.SourceVideoUrl,
            VideoDurationSeconds = entity.VideoDurationSeconds,
            VideoFileSizeBytes = entity.VideoFileSizeBytes,
            VideoFormat = entity.VideoFormat,
            VideoResolution = entity.VideoResolution,
            Type = entity.Type,
            BackgroundPreset = entity.BackgroundPreset,
            FrameCount = entity.FrameCount,
            EnableHotspots = entity.EnableHotspots,
            MaskLicensePlate = entity.MaskLicensePlate,
            SpinViewerUrl = entity.SpinViewerUrl,
            SpinEmbedCode = entity.SpinEmbedCode,
            FallbackImageUrl = entity.FallbackImageUrl,
            PreviewGifUrl = entity.PreviewGifUrl,
            ThumbnailUrl = entity.ThumbnailUrl,
            ExtractedFrameUrls = entity.ExtractedFrameUrls,
            ExtractedFrameCount = entity.ExtractedFrameCount,
            Status = entity.Status,
            ErrorMessage = entity.ErrorMessage,
            ProgressPercent = entity.ProgressPercent,
            ProcessingTimeMs = entity.ProcessingTimeMs,
            QualityScore = entity.QualityScore,
            QualityIssues = entity.QualityIssues,
            CreatedAt = entity.CreatedAt,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt
        };
    }
    
    public static Video360SpinStatusResponse ToStatusResponse(this Video360Spin entity)
    {
        return new Video360SpinStatusResponse
        {
            SpinId = entity.Id,
            Status = entity.Status,
            ProgressPercent = entity.ProgressPercent,
            SpinViewerUrl = entity.SpinViewerUrl,
            SpinEmbedCode = entity.SpinEmbedCode,
            ThumbnailUrl = entity.ThumbnailUrl,
            ExtractedFrameCount = entity.ExtractedFrameCount,
            ExtractedFrameUrls = entity.ExtractedFrameUrls,
            ErrorMessage = entity.ErrorMessage,
            ProcessingTimeSeconds = entity.ProcessingTimeMs.HasValue 
                ? entity.ProcessingTimeMs.Value / 1000 
                : null,
            QualityScore = entity.QualityScore
        };
    }
}
