using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Domain.Interfaces;

/// <summary>
/// HTTP client interface for Spyne AI API
/// </summary>
public interface ISpyneApiClient
{
    // ===== NEW: Unified Merchandise API (Recommended) =====
    Task<SpyneTransformResponse> TransformVehicleAsync(SpyneMerchandiseRequest request, CancellationToken cancellationToken = default);
    Task<SpyneMediaResult> GetVehicleMediaAsync(string dealerVinId, CancellationToken cancellationToken = default);

    // ===== Legacy Image Transformation =====
    Task<SpyneTransformResponse> TransformImageAsync(SpyneTransformRequest request, CancellationToken cancellationToken = default);
    Task<SpyneTransformResponse> TransformBatchAsync(List<SpyneTransformRequest> requests, CancellationToken cancellationToken = default);
    Task<SpyneJobStatusResponse> GetImageStatusAsync(string jobId, CancellationToken cancellationToken = default);

    // 360 Spin Generation
    Task<SpyneSpinResponse> GenerateSpinAsync(SpyneSpinRequest request, CancellationToken cancellationToken = default);
    Task<SpyneJobStatusResponse> GetSpinStatusAsync(string jobId, CancellationToken cancellationToken = default);

    // Video Generation
    Task<SpyneVideoResponse> GenerateVideoAsync(SpyneVideoRequest request, CancellationToken cancellationToken = default);
    Task<SpyneJobStatusResponse> GetVideoStatusAsync(string jobId, CancellationToken cancellationToken = default);

    // Chat AI (Fase 4)
    Task<SpyneChatInitResponse> InitializeChatAsync(SpyneChatRequest request, CancellationToken cancellationToken = default);
    Task<SpyneChatSessionResponse> StartChatSessionAsync(SpyneChatStartRequest request, CancellationToken cancellationToken = default);
    Task<SpyneChatMessageResponse> SendChatMessageAsync(string sessionToken, string message, CancellationToken cancellationToken = default);
    Task EndChatSessionAsync(string sessionToken, CancellationToken cancellationToken = default);

    // Background Presets
    Task<List<SpyneBackgroundPreset>> GetBackgroundPresetsAsync(CancellationToken cancellationToken = default);
}

#region Spyne API Request/Response Models

/// <summary>
/// Request for unified merchandise/process API (NEW - Recommended)
/// </summary>
public record SpyneMerchandiseRequest
{
    public string? Vin { get; init; }
    public string? StockNumber { get; init; }
    public string? RegistrationNumber { get; init; }
    public string? DealerId { get; init; }
    
    // Media types to process
    public bool ProcessImages { get; init; } = true;
    public bool Process360Spin { get; init; } = false;
    public bool ProcessFeatureVideo { get; init; } = false;
    
    // Input media
    public List<string> ImageUrls { get; init; } = new();
    public List<string>? VideoUrls { get; init; }
    
    // Processing options
    public string? BackgroundId { get; init; }
    public bool MaskLicensePlate { get; init; } = true;
    public string? BannerId { get; init; }
    public int? ExtractCatalogCount { get; init; }
    
    // 360 Spin options
    public bool EnableHotspots { get; init; } = true;
    public int? SpinFrameCount { get; init; }
    
    // Video options
    public string? VideoTemplateId { get; init; }
    public string? VideoMusicId { get; init; }
    public string? VideoVoiceId { get; init; }
}

/// <summary>
/// Result from get-media API (pv1/merchandise)
/// Structure: { stockNumber, dealerVinID, inputData, mediaData }
/// </summary>
public record SpyneMediaResult
{
    public string? StockNumber { get; init; }
    public string? DealerVinID { get; init; }
    public string? Source { get; init; }
    public string? DealerId { get; init; }
    public string? ErrorMessage { get; init; }
    
    // Input data (what was submitted)
    public SpyneInputData? InputData { get; init; }
    
    // Media data with processing results (Spyne API returns "mediaData" not "outputData")
    public SpyneMediaData? MediaData { get; init; }
    
    // For backward compatibility - maps to MediaData
    public SpyneOutputData? OutputData => MediaData != null ? new SpyneOutputData
    {
        Image = MediaData.Image != null ? new SpyneImageOutput
        {
            ImageData = MediaData.Image.ImageData?.Select(i => new SpyneImageItemResult
            {
                ImageId = i.ImageId,
                InputUrl = i.InputImage,
                OutputUrl = i.OutputImage,
                AiStatus = i.Status,
                Category = i.Category
            }).ToList()
        } : null,
        Spin = MediaData.Spin,
        Video = MediaData.Video
    } : null;
    
    // Computed status based on mediaData presence
    public string? Status => MediaData?.Image?.AiStatus == "DONE" 
        ? "completed" 
        : "processing";
}

/// <summary>
/// Media data from Spyne API response (actual field name)
/// </summary>
public record SpyneMediaData
{
    public SpyneImageData? Image { get; init; }
    public SpyneSpinOutput? Spin { get; init; }
    public SpyneVideoOutput? Video { get; init; }
}

/// <summary>
/// Image processing data from Spyne
/// </summary>
public record SpyneImageData
{
    public string? SkuId { get; init; }
    public string? AiStatus { get; init; }
    public List<SpyneImageDataItem>? ImageData { get; init; }
}

/// <summary>
/// Individual image item from Spyne response
/// </summary>
public record SpyneImageDataItem
{
    public string? Status { get; init; }
    public string? FrameNo { get; init; }
    public string? ImageId { get; init; }
    public string? ImageName { get; init; }
    public string? InputImage { get; init; }
    public string? OutputImage { get; init; }
    public string? BackgroundId { get; init; }
    public string? Category { get; init; }
    public int? Angle { get; init; }
}

public record SpyneInputData
{
    public SpyneAssetsInfo? AssetsInfo { get; init; }
}

public record SpyneAssetsInfo
{
    public List<SpyneInputAsset>? Images { get; init; }
    public List<SpyneInputAsset>? Videos { get; init; }
}

public record SpyneInputAsset
{
    public string? Id { get; init; }
    public string? Type { get; init; }
    public string? Url { get; init; }
    public string? Source { get; init; }
    public bool IsHidden { get; init; }
    public bool IsActive { get; init; }
    public bool IsUploaded { get; init; }
}

public record SpyneOutputData
{
    public SpyneImageOutput? Image { get; init; }
    public SpyneSpinOutput? Spin { get; init; }
    public SpyneVideoOutput? Video { get; init; }
}

public record SpyneImageOutput
{
    public List<SpyneImageItemResult>? ImageData { get; init; }
}

public record SpyneImageItemResult
{
    public string? ImageId { get; init; }
    public string? InputUrl { get; init; }
    public string? OutputUrl { get; init; }
    public string? AiStatus { get; init; }
    public string? QcStatus { get; init; }
    public string? Category { get; init; }
    public string? ViewAngle { get; init; }
}

public record SpyneSpinOutput
{
    public string? SpinId { get; init; }
    public string? EmbedUrl { get; init; }
    public string? SpinAiStatus { get; init; }
}

public record SpyneVideoOutput
{
    public string? VideoId { get; init; }
    public string? OutputUrl { get; init; }
    public string? VideoAiStatus { get; init; }
}

public record SpyneTransformRequest
{
    public string ImageUrl { get; init; } = string.Empty;
    public string BackgroundId { get; init; } = string.Empty;
    public bool MaskLicensePlate { get; init; }
    public bool EnhanceQuality { get; init; }
    public string? Vin { get; init; }
    public string? StockNumber { get; init; }
    public string? WebhookUrl { get; init; }
}

public record SpyneTransformResponse
{
    public string JobId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int? EstimatedSeconds { get; init; }
    public string? Message { get; init; }
}

public record SpyneSpinRequest
{
    public List<string> ImageUrls { get; init; } = new();
    public string BackgroundId { get; init; } = string.Empty;
    public string OutputFormat { get; init; } = "interactive";
    public string? Vin { get; init; }
    public string? WebhookUrl { get; init; }
}

public record SpyneSpinResponse
{
    public string JobId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int EstimatedMinutes { get; init; }
}

public record SpyneVideoRequest
{
    public List<string> ImageUrls { get; init; } = new();
    public int DurationSeconds { get; init; }
    public string Style { get; init; } = "cinematic";
    public string Format { get; init; } = "horizontal";
    public bool IncludeMusic { get; init; }
    public string? MusicTrackId { get; init; }
    public string? BrandingLogoUrl { get; init; }
    public string? WebhookUrl { get; init; }
}

public record SpyneVideoResponse
{
    public string JobId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int EstimatedMinutes { get; init; }
}

public record SpyneJobStatusResponse
{
    public string JobId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? ResultUrl { get; init; }
    public string? ResultUrlHd { get; init; }
    public string? EmbedCode { get; init; }
    public string? ThumbnailUrl { get; init; }
    public long? FileSizeBytes { get; init; }
    public int? ProcessingTimeMs { get; init; }
    public string? ErrorMessage { get; init; }
}

public record SpyneChatRequest
{
    public string? VehicleContext { get; init; }
    public string Language { get; init; } = "es";
    public bool InitialGreeting { get; init; } = true;
}

public record SpyneChatInitResponse
{
    public string ChatId { get; init; } = string.Empty;
    public string? WelcomeMessage { get; init; }
    public string? MessageId { get; init; }
}

public record SpyneChatStartRequest
{
    public string? VehicleContext { get; init; }
    public string? DealerInfo { get; init; }
    public string? VisitorId { get; init; }
}

public record SpyneChatSessionResponse
{
    public string SessionToken { get; init; } = string.Empty;
    public string WelcomeMessage { get; init; } = string.Empty;
}

public record SpyneChatMessageResponse
{
    public string Message { get; init; } = string.Empty;
    public double? Confidence { get; init; }
    public List<string> SuggestedQuestions { get; init; } = new();
    public bool LeadDetected { get; init; }
    public SpyneDetectedLeadInfo? DetectedLead { get; init; }
    public string? DetectedIntent { get; init; }
}

public record SpyneDetectedLeadInfo
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public int Score { get; init; }
}

public record SpyneBackgroundPreset
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string PreviewUrl { get; init; } = string.Empty;
    public decimal CreditsCost { get; init; }
}

#endregion
