using MediatR;
using SpyneIntegrationService.Application.DTOs;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Application.Features.Video360Spins.Commands;

/// <summary>
/// Command to generate a 360° spin from a video walkthrough.
/// 
/// FLUJO:
/// 1. Usuario ya subió el video a S3/storage y tiene la URL
/// 2. Este comando envía la URL del video a Spyne API
/// 3. Spyne extrae automáticamente los frames del video
/// 4. Spyne procesa los frames (background replacement, enhancement)
/// 5. Spyne genera el viewer 360° interactivo
/// 6. Webhook o polling devuelve el resultado
/// </summary>
public record GenerateVideo360SpinCommand : IRequest<GenerateVideo360SpinResponse>
{
    /// <summary>Vehicle ID for this 360° spin</summary>
    public Guid VehicleId { get; init; }
    
    /// <summary>Dealer ID (optional)</summary>
    public Guid? DealerId { get; init; }
    
    /// <summary>User ID who initiated the request</summary>
    public Guid? UserId { get; init; }
    
    /// <summary>URL of the uploaded video (must be publicly accessible)</summary>
    public string VideoUrl { get; init; } = string.Empty;
    
    /// <summary>Video duration in seconds</summary>
    public int? VideoDurationSeconds { get; init; }
    
    /// <summary>Video file size in bytes</summary>
    public long? VideoFileSizeBytes { get; init; }
    
    /// <summary>Video format (mp4, mov, etc.)</summary>
    public string? VideoFormat { get; init; }
    
    /// <summary>Video resolution</summary>
    public string? VideoResolution { get; init; }
    
    /// <summary>Type of 360° view to generate</summary>
    public Video360SpinType Type { get; init; } = Video360SpinType.Exterior;
    
    /// <summary>Background preset for processed frames</summary>
    public BackgroundPreset BackgroundPreset { get; init; } = BackgroundPreset.Studio;
    
    /// <summary>Custom Spyne background ID</summary>
    public string? CustomBackgroundId { get; init; }
    
    /// <summary>Number of frames to extract (24, 36, 72)</summary>
    public int FrameCount { get; init; } = 36;
    
    /// <summary>Enable interactive hotspots</summary>
    public bool EnableHotspots { get; init; } = true;
    
    /// <summary>Mask license plate</summary>
    public bool MaskLicensePlate { get; init; } = true;
    
    /// <summary>VIN for Spyne tracking</summary>
    public string? Vin { get; init; }
    
    /// <summary>Stock number for Spyne tracking</summary>
    public string? StockNumber { get; init; }
    
    /// <summary>Webhook URL for status notifications</summary>
    public string? WebhookUrl { get; init; }
}
