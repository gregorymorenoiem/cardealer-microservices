using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Domain.Entities;

/// <summary>
/// Represents an image transformation request/result from Spyne AI
/// </summary>
public class ImageTransformation
{
    public Guid Id { get; set; }
    
    /// <summary>Vehicle this transformation belongs to</summary>
    public Guid VehicleId { get; set; }
    
    /// <summary>Dealer who owns the vehicle (for quota tracking)</summary>
    public Guid? DealerId { get; set; }
    
    /// <summary>Original image URL before transformation</summary>
    public string OriginalImageUrl { get; set; } = string.Empty;
    
    /// <summary>Alias for OriginalImageUrl</summary>
    public string SourceImageUrl 
    { 
        get => OriginalImageUrl; 
        set => OriginalImageUrl = value; 
    }
    
    /// <summary>Transformed image URL from Spyne</summary>
    public string? TransformedImageUrl { get; set; }
    
    /// <summary>High resolution transformed image URL</summary>
    public string? TransformedImageUrlHd { get; set; }
    
    /// <summary>Thumbnail URL</summary>
    public string? ThumbnailUrl { get; set; }
    
    /// <summary>Spyne's internal job ID for tracking</summary>
    public string? SpyneJobId { get; set; }
    
    /// <summary>Background preset used</summary>
    public BackgroundPreset BackgroundPreset { get; set; }
    
    /// <summary>Type of transformation</summary>
    public TransformationType TransformationType { get; set; }
    
    /// <summary>Custom background ID if using custom preset</summary>
    public string? CustomBackgroundId { get; set; }
    
    /// <summary>Current transformation status</summary>
    public TransformationStatus Status { get; set; }
    
    /// <summary>Whether license plate was masked</summary>
    public bool LicensePlateMasked { get; set; }
    
    /// <summary>Whether image quality was enhanced</summary>
    public bool QualityEnhanced { get; set; }
    
    /// <summary>Error message if transformation failed</summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>Number of retry attempts</summary>
    public int RetryCount { get; set; }
    
    /// <summary>Processing time in milliseconds</summary>
    public int? ProcessingTimeMs { get; set; }
    
    /// <summary>Credits/cost consumed for this transformation</summary>
    public decimal CreditsCost { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ImageTransformation()
    {
        Id = Guid.NewGuid();
        Status = TransformationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessing(string spyneJobId)
    {
        SpyneJobId = spyneJobId;
        Status = TransformationStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted(string transformedUrl, string? transformedUrlHd, int processingTimeMs)
    {
        TransformedImageUrl = transformedUrl;
        TransformedImageUrlHd = transformedUrlHd;
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

    public bool CanRetry() => RetryCount < 3 && Status == TransformationStatus.Failed;

    public void IncrementRetry()
    {
        RetryCount++;
        Status = TransformationStatus.Pending;
        ErrorMessage = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
