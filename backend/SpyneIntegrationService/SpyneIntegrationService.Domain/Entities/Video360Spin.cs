using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Domain.Entities;

/// <summary>
/// Represents a video-based 360° spin generation from Spyne AI.
/// 
/// FLUJO:
/// 1. Usuario graba un video haciendo recorrido 360° alrededor del vehículo
/// 2. Video se sube a S3/storage y se obtiene URL
/// 3. URL del video se envía a Spyne API (merchandise/process con spin=true)
/// 4. Spyne extrae automáticamente los frames del video
/// 5. Spyne procesa los frames (background replacement, enhancement)
/// 6. Spyne genera el viewer 360° interactivo
/// 7. Webhook/polling devuelve URLs de: spin viewer, frames extraídos, thumbnail
/// </summary>
public class Video360Spin
{
    public Guid Id { get; set; }
    
    /// <summary>Vehicle this 360° spin belongs to</summary>
    public Guid VehicleId { get; set; }
    
    /// <summary>Dealer who owns the vehicle</summary>
    public Guid? DealerId { get; set; }
    
    /// <summary>User who uploaded the video</summary>
    public Guid? UserId { get; set; }
    
    /// <summary>Spyne's internal dealerVinId for tracking</summary>
    public string? SpyneJobId { get; set; }
    
    /// <summary>Spyne's SKU/Stock identifier</summary>
    public string? SpyneSkuId { get; set; }

    #region Input Video Properties (Lo que el usuario sube)
    
    /// <summary>Original video URL uploaded by user (video 360° del vehículo)</summary>
    public string SourceVideoUrl { get; set; } = string.Empty;
    
    /// <summary>Video duration in seconds</summary>
    public int? VideoDurationSeconds { get; set; }
    
    /// <summary>Video file size in bytes</summary>
    public long? VideoFileSizeBytes { get; set; }
    
    /// <summary>Video format (mp4, mov, etc.)</summary>
    public string? VideoFormat { get; set; }
    
    /// <summary>Video resolution (1080p, 4K, etc.)</summary>
    public string? VideoResolution { get; set; }
    
    #endregion

    #region Processing Options (Opciones para Spyne)
    
    /// <summary>Type of 360° view to generate</summary>
    public Video360SpinType Type { get; set; } = Video360SpinType.Exterior;
    
    /// <summary>Background preset to use for extracted frames</summary>
    public BackgroundPreset BackgroundPreset { get; set; } = BackgroundPreset.Studio;
    
    /// <summary>Custom Spyne background ID (ej: "20883" showroom, "16570" white)</summary>
    public string? CustomBackgroundId { get; set; }
    
    /// <summary>Number of frames to extract from video (24, 36, 72). Default 36 for 10° increments</summary>
    public int FrameCount { get; set; } = 36;
    
    /// <summary>Whether to enable interactive hotspots on the 360° view</summary>
    public bool EnableHotspots { get; set; } = true;
    
    /// <summary>Whether to mask/blur license plate in extracted frames</summary>
    public bool MaskLicensePlate { get; set; } = true;
    
    #endregion

    #region Output Properties (Lo que Spyne devuelve)
    
    /// <summary>Interactive 360° spin viewer URL from Spyne</summary>
    public string? SpinViewerUrl { get; set; }
    
    /// <summary>HTML embed code for spin viewer</summary>
    public string? SpinEmbedCode { get; set; }
    
    /// <summary>Fallback static image if spin viewer can't load</summary>
    public string? FallbackImageUrl { get; set; }
    
    /// <summary>Preview GIF URL for thumbnail/preview</summary>
    public string? PreviewGifUrl { get; set; }
    
    /// <summary>Main thumbnail URL for the 360° view</summary>
    public string? ThumbnailUrl { get; set; }
    
    /// <summary>
    /// List of extracted frame URLs from the video (processed by Spyne).
    /// These are the individual images that Spyne extracted and processed.
    /// Typically 36-72 images at different angles.
    /// </summary>
    public List<string> ExtractedFrameUrls { get; set; } = new();
    
    /// <summary>Number of frames successfully extracted by Spyne</summary>
    public int ExtractedFrameCount { get; set; }
    
    /// <summary>Hotspots data in JSON format (if hotspots were enabled)</summary>
    public string? HotspotsJson { get; set; }
    
    #endregion

    #region Status & Tracking
    
    /// <summary>Current processing status</summary>
    public TransformationStatus Status { get; set; } = TransformationStatus.Pending;
    
    /// <summary>Error message if processing failed</summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>Processing progress percentage (0-100)</summary>
    public int ProgressPercent { get; set; }
    
    /// <summary>Processing time in milliseconds</summary>
    public int? ProcessingTimeMs { get; set; }
    
    /// <summary>Credits/cost consumed for this operation</summary>
    public decimal CreditsCost { get; set; }
    
    /// <summary>Quality score of the generated 360° (0-100)</summary>
    public int? QualityScore { get; set; }
    
    /// <summary>Quality issues identified during processing</summary>
    public List<string> QualityIssues { get; set; } = new();
    
    #endregion

    #region Timestamps
    
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    #endregion

    public Video360Spin()
    {
        Id = Guid.NewGuid();
        Status = TransformationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    #region State Transitions

    public void MarkAsUploading()
    {
        Status = TransformationStatus.Uploading;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessing(string spyneJobId, string? skuId = null)
    {
        SpyneJobId = spyneJobId;
        SpyneSkuId = skuId;
        Status = TransformationStatus.Processing;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProgress(int percent)
    {
        ProgressPercent = Math.Clamp(percent, 0, 100);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted(
        string spinViewerUrl, 
        List<string>? extractedFrameUrls = null,
        string? embedCode = null, 
        string? fallbackUrl = null,
        string? thumbnailUrl = null,
        string? previewGifUrl = null,
        int? processingTimeMs = null,
        int? qualityScore = null)
    {
        SpinViewerUrl = spinViewerUrl;
        SpinEmbedCode = embedCode;
        FallbackImageUrl = fallbackUrl;
        ThumbnailUrl = thumbnailUrl;
        PreviewGifUrl = previewGifUrl;
        
        if (extractedFrameUrls != null && extractedFrameUrls.Count > 0)
        {
            ExtractedFrameUrls = extractedFrameUrls;
            ExtractedFrameCount = extractedFrameUrls.Count;
        }
        
        Status = TransformationStatus.Completed;
        ProcessingTimeMs = processingTimeMs;
        QualityScore = qualityScore;
        ProgressPercent = 100;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        ErrorMessage = errorMessage;
        Status = TransformationStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Validation

    public bool IsVideoValid()
    {
        if (string.IsNullOrEmpty(SourceVideoUrl))
            return false;
            
        // Max 500MB
        if (VideoFileSizeBytes.HasValue && VideoFileSizeBytes.Value > 500 * 1024 * 1024)
            return false;
            
        // Max 5 minutes for 360° video
        if (VideoDurationSeconds.HasValue && VideoDurationSeconds.Value > 300)
            return false;
            
        return true;
    }

    public string? GetValidationError()
    {
        if (string.IsNullOrEmpty(SourceVideoUrl))
            return "Video URL is required";
            
        if (VideoFileSizeBytes.HasValue && VideoFileSizeBytes.Value > 500 * 1024 * 1024)
            return "Video file size exceeds 500MB limit";
            
        if (VideoDurationSeconds.HasValue && VideoDurationSeconds.Value > 300)
            return "Video duration exceeds 5 minutes limit. For 360° tours, 30-90 seconds is optimal.";
            
        return null;
    }

    #endregion
}

/// <summary>
/// Type of 360° view to generate from the video
/// </summary>
public enum Video360SpinType
{
    /// <summary>Exterior 360° view - video walking around the vehicle exterior</summary>
    Exterior = 0,
    
    /// <summary>Interior panoramic view - video inside the vehicle</summary>
    Interior = 1,
    
    /// <summary>Engine compartment view</summary>
    EngineCompartment = 2,
    
    /// <summary>Trunk/cargo area view</summary>
    Trunk = 3,
    
    /// <summary>Combined exterior and interior (requires longer video)</summary>
    Combined = 4
}
