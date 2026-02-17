using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Domain.Entities;

/// <summary>
/// Represents a 360° spin generation request/result from Spyne AI
/// </summary>
public class SpinGeneration
{
    public Guid Id { get; set; }
    
    /// <summary>Vehicle this spin belongs to</summary>
    public Guid VehicleId { get; set; }
    
    /// <summary>Dealer who owns the vehicle</summary>
    public Guid? DealerId { get; set; }
    
    /// <summary>Spyne's internal job ID</summary>
    public string? SpyneJobId { get; set; }
    
    /// <summary>Source image URLs used to generate spin (36-72 images)</summary>
    public List<string> SourceImageUrls { get; set; } = new();
    
    /// <summary>Interactive spin viewer URL</summary>
    public string? SpinViewerUrl { get; set; }
    
    /// <summary>Embed code for spin viewer</summary>
    public string? SpinEmbedCode { get; set; }
    
    /// <summary>Fallback static image if spin can't load</summary>
    public string? FallbackImageUrl { get; set; }
    
    /// <summary>Background preset used</summary>
    public BackgroundPreset BackgroundPreset { get; set; }
    
    /// <summary>Custom background ID if using custom</summary>
    public string? CustomBackgroundId { get; set; }
    
    /// <summary>Number of images required for this spin type</summary>
    public int RequiredImageCount { get; set; }
    
    /// <summary>Number of images actually received</summary>
    public int ReceivedImageCount { get; set; }
    
    /// <summary>Current generation status</summary>
    public TransformationStatus Status { get; set; }
    
    /// <summary>Error message if failed</summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>Processing time in milliseconds</summary>
    public int? ProcessingTimeMs { get; set; }
    
    /// <summary>Credits/cost consumed</summary>
    public decimal CreditsCost { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public SpinGeneration()
    {
        Id = Guid.NewGuid();
        Status = TransformationStatus.Pending;
        RequiredImageCount = 36; // Default for 10° increments
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessing(string spyneJobId)
    {
        SpyneJobId = spyneJobId;
        Status = TransformationStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted(string spinViewerUrl, string embedCode, string? fallbackUrl, int processingTimeMs)
    {
        SpinViewerUrl = spinViewerUrl;
        SpinEmbedCode = embedCode;
        FallbackImageUrl = fallbackUrl;
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

    public bool HasSufficientImages() => ReceivedImageCount >= RequiredImageCount;
}
