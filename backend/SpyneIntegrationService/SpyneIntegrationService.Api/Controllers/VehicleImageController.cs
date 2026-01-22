using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpyneIntegrationService.Domain.Interfaces;
using SpyneIntegrationService.Infrastructure.Services;

namespace SpyneIntegrationService.Api.Controllers;

#region Background & Permission Constants

/// <summary>
/// Available Spyne background IDs with access control
/// </summary>
public static class SpyneBackgrounds
{
    /// <summary>Blanco Infinito - Disponible para TODOS los vendedores (gratis)</summary>
    public const string WhiteInfinite = "16570";
    
    /// <summary>Showroom Gris - Solo para Dealers con membresía</summary>
    public const string ShowroomGray = "20883";
    
    /// <summary>Backgrounds disponibles para vendedores individuales (sin membresía)</summary>
    public static readonly string[] FreeBackgrounds = { WhiteInfinite };
    
    /// <summary>Backgrounds disponibles para Dealers con membresía activa</summary>
    public static readonly string[] DealerBackgrounds = { WhiteInfinite, ShowroomGray };
    
    /// <summary>Default para vendedores individuales</summary>
    public const string DefaultFree = WhiteInfinite;
    
    /// <summary>Default para dealers</summary>
    public const string DefaultDealer = ShowroomGray;
}

/// <summary>
/// Tipo de cuenta del usuario (sincronizado con UserService)
/// </summary>
public enum AccountType
{
    /// <summary>Comprador o Vendedor Individual (acceso limitado a Spyne)</summary>
    Individual = 0,
    
    /// <summary>Dealer con membresía (acceso completo a Spyne)</summary>
    Dealer = 1,
    
    /// <summary>Administrador (acceso completo)</summary>
    Admin = 2
}

/// <summary>
/// Información del usuario para validar permisos de Spyne
/// </summary>
public class SpyneUserContext
{
    public Guid UserId { get; set; }
    public AccountType AccountType { get; set; }
    public bool HasActiveSubscription { get; set; }
    
    /// <summary>True si el usuario puede acceder a 360° Spin</summary>
    public bool CanUse360Spin => AccountType == AccountType.Dealer && HasActiveSubscription || AccountType == AccountType.Admin;
    
    /// <summary>True si puede usar el background Showroom Gris</summary>
    public bool CanUseShowroomBackground => AccountType == AccountType.Dealer && HasActiveSubscription || AccountType == AccountType.Admin;
    
    /// <summary>Obtiene los backgrounds disponibles para este usuario</summary>
    public string[] GetAvailableBackgrounds() => CanUseShowroomBackground 
        ? SpyneBackgrounds.DealerBackgrounds 
        : SpyneBackgrounds.FreeBackgrounds;
    
    /// <summary>Obtiene el background default para este usuario</summary>
    public string GetDefaultBackground() => CanUseShowroomBackground 
        ? SpyneBackgrounds.DefaultDealer 
        : SpyneBackgrounds.DefaultFree;
    
    /// <summary>Valida si el usuario puede usar un background específico</summary>
    public bool CanUseBackground(string backgroundId) => 
        GetAvailableBackgrounds().Contains(backgroundId);
}

#endregion

/// <summary>
/// Controller for vehicle image processing using Spyne AI
/// Provides endpoints for transforming vehicle images with professional backgrounds
/// </summary>
[ApiController]
[Route("api/vehicle-images")]
[Produces("application/json")]
public class VehicleImageController : ControllerBase
{
    private readonly ISpyneApiClient _spyneClient;
    private readonly ILogger<VehicleImageController> _logger;

    public VehicleImageController(
        ISpyneApiClient spyneClient,
        ILogger<VehicleImageController> logger)
    {
        _spyneClient = spyneClient;
        _logger = logger;
    }

    /// <summary>
    /// Transform a single vehicle image with AI background replacement
    /// </summary>
    /// <remarks>
    /// Sends an image to Spyne AI for processing. Returns a job ID that can be used
    /// to check the status and retrieve the processed image.
    /// 
    /// **Background Access:**
    /// - **All users**: "Blanco Infinito" (16570) - mantiene calidad de la plataforma
    /// - **Dealers only**: "Showroom Gris" (20883) - requiere membresía activa
    /// 
    /// Processing typically takes 30-60 seconds.
    /// </remarks>
    [HttpPost("transform")]
    [ProducesResponseType(typeof(TransformResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TransformImage([FromBody] VehicleTransformRequest request)
    {
        if (string.IsNullOrEmpty(request.ImageUrl))
        {
            return BadRequest(new { error = "imageUrl is required" });
        }
        
        // Determine user context and validate background
        var userContext = new SpyneUserContext
        {
            AccountType = request.AccountType ?? AccountType.Individual,
            HasActiveSubscription = request.HasActiveSubscription ?? false
        };
        
        // Validate and set background
        var backgroundId = request.BackgroundId ?? userContext.GetDefaultBackground();
        if (!userContext.CanUseBackground(backgroundId))
        {
            backgroundId = userContext.GetDefaultBackground();
            _logger.LogInformation("Background {RequestedBg} not allowed for {AccountType}, using {FallbackBg}", 
                request.BackgroundId, userContext.AccountType, backgroundId);
        }

        _logger.LogInformation("Transform request for image: {ImageUrl}, Background: {Background}, AccountType: {AccountType}", 
            request.ImageUrl, backgroundId, userContext.AccountType);

        try
        {
            var spyneRequest = new SpyneMerchandiseRequest
            {
                StockNumber = request.StockNumber ?? $"okla-{Guid.NewGuid().ToString()[..8]}",
                Vin = request.Vin,
                ProcessImages = true,
                Process360Spin = false,
                ProcessFeatureVideo = false,
                ImageUrls = new List<string> { request.ImageUrl },
                BackgroundId = backgroundId,
                MaskLicensePlate = request.MaskLicensePlate ?? true
            };

            var result = await _spyneClient.TransformVehicleAsync(spyneRequest);

            return Accepted(new TransformResponse
            {
                JobId = result.JobId,
                Status = "processing",
                Message = "Image transformation started. Use the jobId to check status.",
                EstimatedSeconds = result.EstimatedSeconds ?? 60,
                CheckStatusUrl = $"/api/vehicle-images/status/{result.JobId}",
                BackgroundUsed = backgroundId
            });
        }
        catch (SpyneApiException ex)
        {
            _logger.LogError(ex, "Spyne API error during transform");
            return StatusCode(502, new { error = "Spyne API error", details = ex.Message });
        }
    }

    /// <summary>
    /// Transform multiple vehicle images in batch
    /// </summary>
    /// <remarks>
    /// Process multiple images for a single vehicle. All images will use the same
    /// background and processing settings.
    /// </remarks>
    [HttpPost("transform/batch")]
    [ProducesResponseType(typeof(TransformResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TransformBatch([FromBody] BatchTransformRequest request)
    {
        if (request.ImageUrls == null || request.ImageUrls.Count == 0)
        {
            return BadRequest(new { error = "imageUrls array is required and must not be empty" });
        }

        if (request.ImageUrls.Count > 50)
        {
            return BadRequest(new { error = "Maximum 50 images per batch" });
        }

        _logger.LogInformation("Batch transform request received for {Count} images", request.ImageUrls.Count);

        try
        {
            var spyneRequest = new SpyneMerchandiseRequest
            {
                StockNumber = request.StockNumber ?? $"okla-batch-{Guid.NewGuid().ToString()[..8]}",
                Vin = request.Vin,
                ProcessImages = true,
                Process360Spin = request.Generate360Spin ?? false,
                ProcessFeatureVideo = request.GenerateVideo ?? false,
                ImageUrls = request.ImageUrls,
                VideoUrls = request.VideoUrls,
                BackgroundId = request.BackgroundId ?? "20883",
                MaskLicensePlate = request.MaskLicensePlate ?? true,
                EnableHotspots = request.EnableHotspots ?? true,
                SpinFrameCount = request.SpinFrameCount
            };

            var result = await _spyneClient.TransformVehicleAsync(spyneRequest);

            return Accepted(new TransformResponse
            {
                JobId = result.JobId,
                Status = "processing",
                Message = $"Batch processing started for {request.ImageUrls.Count} images.",
                EstimatedSeconds = 60 + (request.ImageUrls.Count * 10),
                CheckStatusUrl = $"/api/vehicle-images/status/{result.JobId}"
            });
        }
        catch (SpyneApiException ex)
        {
            _logger.LogError(ex, "Spyne API error during batch transform");
            return StatusCode(502, new { error = "Spyne API error", details = ex.Message });
        }
    }

    /// <summary>
    /// Check the status of a processing job
    /// </summary>
    /// <remarks>
    /// Poll this endpoint to check if processing is complete. When status is "completed",
    /// the response will include the processed image URLs.
    /// </remarks>
    [HttpGet("status/{jobId}")]
    [ProducesResponseType(typeof(JobStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobStatus(string jobId)
    {
        _logger.LogDebug("Status check for job: {JobId}", jobId);

        try
        {
            var result = await _spyneClient.GetVehicleMediaAsync(jobId);

            var response = new JobStatusResponse
            {
                JobId = jobId,
                Status = result.Status ?? "processing",
                Images = result.OutputData?.Image?.ImageData?.Select(img => new ProcessedImage
                {
                    ImageId = img.ImageId,
                    OriginalUrl = img.InputUrl,
                    ProcessedUrl = img.OutputUrl,
                    Status = img.AiStatus,
                    Category = img.Category,
                    ViewAngle = img.ViewAngle
                }).ToList(),
                Spin = result.OutputData?.Spin != null ? new ProcessedSpin
                {
                    SpinId = result.OutputData.Spin.SpinId,
                    EmbedUrl = result.OutputData.Spin.EmbedUrl,
                    Status = result.OutputData.Spin.SpinAiStatus
                } : null,
                Video = result.OutputData?.Video != null ? new ProcessedVideo
                {
                    VideoId = result.OutputData.Video.VideoId,
                    OutputUrl = result.OutputData.Video.OutputUrl,
                    Status = result.OutputData.Video.VideoAiStatus
                } : null
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job status for {JobId}", jobId);
            return NotFound(new { error = "Job not found or expired", jobId });
        }
    }

    /// <summary>
    /// Get available background presets based on user type
    /// </summary>
    /// <remarks>
    /// Returns backgrounds available for the user's account type:
    /// - **Individual (Free)**: Only "Blanco Infinito" (16570)
    /// - **Dealer (Membership)**: "Blanco Infinito" + "Showroom Gris" (20883)
    /// 
    /// Pass accountType as query parameter (0=Individual, 1=Dealer)
    /// </remarks>
    [HttpGet("backgrounds")]
    [ProducesResponseType(typeof(BackgroundsResponse), StatusCodes.Status200OK)]
    public IActionResult GetBackgrounds([FromQuery] int accountType = 0, [FromQuery] bool hasActiveSubscription = false)
    {
        var userContext = new SpyneUserContext
        {
            AccountType = (AccountType)accountType,
            HasActiveSubscription = hasActiveSubscription
        };
        
        var availableBackgrounds = new List<VehicleBackgroundOption>();
        var allBackgrounds = GetAllBackgroundsInfo();
        
        foreach (var bg in allBackgrounds)
        {
            var isAvailable = userContext.CanUseBackground(bg.Id);
            bg.IsAvailable = isAvailable;
            bg.RequiresDealerMembership = bg.Id == SpyneBackgrounds.ShowroomGray;
            availableBackgrounds.Add(bg);
        }
        
        return Ok(new BackgroundsResponse
        {
            Backgrounds = availableBackgrounds,
            DefaultBackgroundId = userContext.GetDefaultBackground(),
            AccountType = userContext.AccountType.ToString(),
            HasDealerAccess = userContext.CanUseShowroomBackground
        });
    }
    
    /// <summary>
    /// Get available Spyne features based on user type
    /// </summary>
    /// <remarks>
    /// Returns which Spyne features are available for the user:
    /// - **Individual**: Background replacement (Blanco Infinito only)
    /// - **Dealer**: Background replacement (all) + 360° Spin + Feature Video
    /// </remarks>
    [HttpGet("features")]
    [ProducesResponseType(typeof(SpyneFeaturesResponse), StatusCodes.Status200OK)]
    public IActionResult GetAvailableFeatures([FromQuery] int accountType = 0, [FromQuery] bool hasActiveSubscription = false)
    {
        var userContext = new SpyneUserContext
        {
            AccountType = (AccountType)accountType,
            HasActiveSubscription = hasActiveSubscription
        };
        
        return Ok(new SpyneFeaturesResponse
        {
            AccountType = userContext.AccountType.ToString(),
            HasActiveSubscription = userContext.HasActiveSubscription,
            Features = new SpyneFeaturesList
            {
                BackgroundReplacement = new FeatureAccess
                {
                    Available = true, // Todos tienen acceso (al menos Blanco Infinito)
                    AvailableBackgrounds = userContext.GetAvailableBackgrounds().ToList(),
                    DefaultBackground = userContext.GetDefaultBackground(),
                    Description = userContext.CanUseShowroomBackground 
                        ? "Acceso a todos los fondos profesionales"
                        : "Fondo Blanco Infinito incluido para mantener calidad de la plataforma"
                },
                Spin360 = new FeatureAccess
                {
                    Available = userContext.CanUse360Spin,
                    RequiresDealerMembership = true,
                    Description = userContext.CanUse360Spin
                        ? "Vista 360° interactiva disponible"
                        : "Exclusivo para Dealers con membresía activa"
                },
                FeatureVideo = new FeatureAccess
                {
                    Available = userContext.CanUse360Spin, // Same permission as 360° spin
                    RequiresDealerMembership = true,
                    Description = userContext.CanUse360Spin
                        ? "Video promocional con IA disponible"
                        : "Exclusivo para Dealers con membresía activa"
                }
            }
        });
    }
    
    private static List<VehicleBackgroundOption> GetAllBackgroundsInfo()
    {
        return new List<VehicleBackgroundOption>
        {
            new()
            {
                Id = SpyneBackgrounds.WhiteInfinite,
                Name = "Blanco Infinito",
                Category = "Studio",
                PreviewUrl = "https://spyne-static.s3.amazonaws.com/backgrounds/16570-preview.jpg",
                Description = "Fondo blanco profesional. Disponible para todos los vendedores."
            },
            new()
            {
                Id = SpyneBackgrounds.ShowroomGray,
                Name = "Showroom Gris",
                Category = "Studio",
                PreviewUrl = "https://spyne-static.s3.amazonaws.com/backgrounds/20883-preview.jpg",
                Description = "Fondo de showroom profesional. Exclusivo para Dealers."
            }
        };
    }

    /// <summary>
    /// Generate a 360° interactive spin view of a vehicle
    /// </summary>
    /// <remarks>
    /// **🔒 REQUIRES DEALER MEMBERSHIP**
    /// 
    /// Creates an interactive 360° spin from multiple vehicle images.
    /// This feature is exclusive for Dealers with an active subscription.
    /// 
    /// **Requirements:**
    /// - Dealer account with active membership
    /// - Minimum 6 exterior images of the vehicle taken from different angles
    /// - Recommended: 36 images (every 10°) for smooth rotation
    /// - Maximum: 72 images for ultra-smooth spin
    /// - Images should be taken at consistent height and distance
    /// 
    /// **Processing:**
    /// - AI removes backgrounds and applies studio lighting
    /// - Creates seamless 360° rotation
    /// - Optional: Add interactive hotspots for damage/features
    /// 
    /// **Output:**
    /// - Interactive embed URL for web integration
    /// - Processing time: 3-10 minutes depending on image count
    /// </remarks>
    [HttpPost("spin")]
    [ProducesResponseType(typeof(SpinGenerateResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GenerateSpin([FromBody] SpinGenerateRequest request)
    {
        // Validate dealer membership
        var userContext = new SpyneUserContext
        {
            AccountType = request.AccountType ?? AccountType.Individual,
            HasActiveSubscription = request.HasActiveSubscription ?? false
        };
        
        if (!userContext.CanUse360Spin)
        {
            _logger.LogWarning("360° Spin access denied for AccountType: {AccountType}, Subscription: {HasSubscription}",
                userContext.AccountType, userContext.HasActiveSubscription);
                
            return StatusCode(403, new SpyneAccessDeniedResponse
            {
                Error = "360° Spin requires Dealer membership",
                Feature = "360° Spin",
                RequiredAccountType = "Dealer",
                RequiresActiveSubscription = true,
                Message = "Esta función está disponible exclusivamente para Dealers con membresía activa. " +
                          "Actualiza tu cuenta para acceder a vistas 360° interactivas.",
                UpgradeUrl = "/dealer/pricing"
            });
        }
        
        // Validate background permission
        var backgroundId = request.BackgroundId ?? userContext.GetDefaultBackground();
        if (!userContext.CanUseBackground(backgroundId))
        {
            backgroundId = userContext.GetDefaultBackground(); // Fallback to allowed background
            _logger.LogInformation("Background {RequestedBg} not allowed, using {FallbackBg}", 
                request.BackgroundId, backgroundId);
        }
        
        if (request.ImageUrls == null || request.ImageUrls.Count < 6)
        {
            return BadRequest(new { 
                error = "Minimum 6 images required for 360° spin",
                provided = request.ImageUrls?.Count ?? 0,
                recommended = "36 images for optimal quality"
            });
        }

        if (request.ImageUrls.Count > 72)
        {
            return BadRequest(new { 
                error = "Maximum 72 images allowed for 360° spin",
                provided = request.ImageUrls.Count
            });
        }

        _logger.LogInformation("360° Spin generation request for {Count} images, Stock: {Stock}, Background: {Background}", 
            request.ImageUrls.Count, request.StockNumber, backgroundId);

        try
        {
            var spyneRequest = new SpyneMerchandiseRequest
            {
                StockNumber = request.StockNumber ?? $"okla-spin-{Guid.NewGuid().ToString()[..8]}",
                Vin = request.Vin,
                ProcessImages = true, // Process images with background replacement
                Process360Spin = true, // Generate 360° spin
                ProcessFeatureVideo = false,
                ImageUrls = request.ImageUrls,
                BackgroundId = backgroundId, // Use validated background
                MaskLicensePlate = request.MaskLicensePlate ?? true,
                EnableHotspots = request.EnableHotspots ?? true,
                SpinFrameCount = request.ImageUrls.Count // Use provided image count as frame count
            };

            var result = await _spyneClient.TransformVehicleAsync(spyneRequest);

            var estimatedMinutes = request.ImageUrls.Count switch
            {
                <= 12 => 3,
                <= 24 => 5,
                <= 36 => 7,
                _ => 10
            };

            return Accepted(new SpinGenerateResponse
            {
                JobId = result.JobId,
                Status = "processing",
                Message = $"360° spin generation started with {request.ImageUrls.Count} images. " +
                          $"Estimated processing time: {estimatedMinutes} minutes.",
                ImageCount = request.ImageUrls.Count,
                EstimatedMinutes = estimatedMinutes,
                CheckStatusUrl = $"/api/vehicle-images/spin/status/{result.JobId}"
            });
        }
        catch (SpyneApiException ex)
        {
            _logger.LogError(ex, "Spyne API error during 360° spin generation");
            return StatusCode(502, new { error = "Spyne API error", details = ex.Message });
        }
    }

    /// <summary>
    /// Check the status of a 360° spin generation job
    /// </summary>
    /// <remarks>
    /// Poll this endpoint to check if 360° spin generation is complete.
    /// When status is "completed", the response includes:
    /// - EmbedUrl: URL to embed the interactive 360° viewer
    /// - Processed images with studio backgrounds
    /// </remarks>
    [HttpGet("spin/status/{jobId}")]
    [ProducesResponseType(typeof(SpinStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSpinStatus(string jobId)
    {
        _logger.LogDebug("360° Spin status check for job: {JobId}", jobId);

        try
        {
            var result = await _spyneClient.GetVehicleMediaAsync(jobId);

            // Determine overall spin status
            var spinStatus = result.MediaData?.Spin?.SpinAiStatus ?? 
                            (result.MediaData?.Image?.AiStatus == "DONE" ? "DONE" : "PROCESSING");

            var response = new SpinStatusResponse
            {
                JobId = jobId,
                Status = spinStatus == "DONE" ? "completed" : "processing",
                
                // Spin data
                SpinId = result.MediaData?.Spin?.SpinId,
                EmbedUrl = result.MediaData?.Spin?.EmbedUrl,
                SpinAiStatus = result.MediaData?.Spin?.SpinAiStatus,
                
                // Processed images (with backgrounds replaced)
                ProcessedImages = result.MediaData?.Image?.ImageData?.Select(img => new SpinProcessedImage
                {
                    ImageId = img.ImageId,
                    FrameNumber = img.FrameNo,
                    OriginalUrl = img.InputImage,
                    ProcessedUrl = img.OutputImage,
                    Status = img.Status,
                    Category = img.Category,
                    Angle = img.Angle
                }).ToList(),
                
                // Summary
                TotalFrames = result.MediaData?.Image?.ImageData?.Count ?? 0,
                CompletedFrames = result.MediaData?.Image?.ImageData?.Count(i => 
                    i.Status?.Equals("Done", StringComparison.OrdinalIgnoreCase) == true ||
                    i.Status?.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) == true) ?? 0
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 360° spin status for {JobId}", jobId);
            return NotFound(new { error = "Spin job not found or expired", jobId });
        }
    }

    /// <summary>
    /// Quick health check
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "VehicleImageController" });
    }
}

#region Request/Response DTOs for VehicleImageController

/// <summary>Request to transform a single vehicle image</summary>
public class VehicleTransformRequest
{
    /// <summary>URL of the image to transform</summary>
    public string ImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Background ID to use:
    /// - 16570 = Blanco Infinito (disponible para todos)
    /// - 20883 = Showroom Gris (solo Dealers con membresía)
    /// </summary>
    public string? BackgroundId { get; set; }
    
    /// <summary>VIN of the vehicle (optional)</summary>
    public string? Vin { get; set; }
    
    /// <summary>Stock number for tracking (optional)</summary>
    public string? StockNumber { get; set; }
    
    /// <summary>Whether to mask/blur the license plate (default: true)</summary>
    public bool? MaskLicensePlate { get; set; }
    
    /// <summary>Account type: 0=Individual, 1=Dealer, 2=Admin</summary>
    public AccountType? AccountType { get; set; }
    
    /// <summary>Whether the user has an active subscription (for Dealers)</summary>
    public bool? HasActiveSubscription { get; set; }
}

public class BatchTransformRequest
{
    /// <summary>List of image URLs to transform</summary>
    public List<string> ImageUrls { get; set; } = new();
    
    /// <summary>List of video URLs (optional, for 360 spin)</summary>
    public List<string>? VideoUrls { get; set; }
    
    /// <summary>Background ID to use</summary>
    public string? BackgroundId { get; set; }
    
    /// <summary>VIN of the vehicle</summary>
    public string? Vin { get; set; }
    
    /// <summary>Stock number for tracking</summary>
    public string? StockNumber { get; set; }
    
    /// <summary>Whether to mask the license plate</summary>
    public bool? MaskLicensePlate { get; set; }
    
    /// <summary>Generate 360 spin from images</summary>
    public bool? Generate360Spin { get; set; }
    
    /// <summary>Generate feature video</summary>
    public bool? GenerateVideo { get; set; }
    
    /// <summary>Enable hotspots in 360 spin</summary>
    public bool? EnableHotspots { get; set; }
    
    /// <summary>Number of frames for 360 spin (default: 72)</summary>
    public int? SpinFrameCount { get; set; }
}

public class TransformResponse
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int EstimatedSeconds { get; set; }
    public string CheckStatusUrl { get; set; } = string.Empty;
    public string? BackgroundUsed { get; set; }
}

public class JobStatusResponse
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<ProcessedImage>? Images { get; set; }
    public ProcessedSpin? Spin { get; set; }
    public ProcessedVideo? Video { get; set; }
}

public class ProcessedImage
{
    public string? ImageId { get; set; }
    public string? OriginalUrl { get; set; }
    public string? ProcessedUrl { get; set; }
    public string? Status { get; set; }
    public string? Category { get; set; }
    public string? ViewAngle { get; set; }
}

public class ProcessedSpin
{
    public string? SpinId { get; set; }
    public string? EmbedUrl { get; set; }
    public string? Status { get; set; }
}

public class ProcessedVideo
{
    public string? VideoId { get; set; }
    public string? OutputUrl { get; set; }
    public string? Status { get; set; }
}

public class VehicleBackgroundOption
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string PreviewUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool RequiresDealerMembership { get; set; }
}

// ===== PERMISSION & FEATURE DTOs =====

/// <summary>Response for backgrounds endpoint</summary>
public class BackgroundsResponse
{
    public List<VehicleBackgroundOption> Backgrounds { get; set; } = new();
    public string DefaultBackgroundId { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public bool HasDealerAccess { get; set; }
}

/// <summary>Response for features endpoint</summary>
public class SpyneFeaturesResponse
{
    public string AccountType { get; set; } = string.Empty;
    public bool HasActiveSubscription { get; set; }
    public SpyneFeaturesList Features { get; set; } = new();
}

public class SpyneFeaturesList
{
    public FeatureAccess BackgroundReplacement { get; set; } = new();
    public FeatureAccess Spin360 { get; set; } = new();
    public FeatureAccess FeatureVideo { get; set; } = new();
}

public class FeatureAccess
{
    public bool Available { get; set; }
    public bool RequiresDealerMembership { get; set; }
    public string? Description { get; set; }
    public List<string>? AvailableBackgrounds { get; set; }
    public string? DefaultBackground { get; set; }
}

/// <summary>Response when access is denied to a feature</summary>
public class SpyneAccessDeniedResponse
{
    public string Error { get; set; } = string.Empty;
    public string Feature { get; set; } = string.Empty;
    public string RequiredAccountType { get; set; } = string.Empty;
    public bool RequiresActiveSubscription { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? UpgradeUrl { get; set; }
}

// ===== 360° SPIN DTOs =====

/// <summary>
/// Request to generate a 360° spin
/// 🔒 REQUIRES: Dealer account with active membership
/// </summary>
public class SpinGenerateRequest
{
    /// <summary>
    /// List of image URLs taken around the vehicle.
    /// Minimum: 6 images, Recommended: 36, Maximum: 72
    /// </summary>
    public List<string> ImageUrls { get; set; } = new();
    
    /// <summary>VIN of the vehicle (optional)</summary>
    public string? Vin { get; set; }
    
    /// <summary>Stock number for tracking (optional)</summary>
    public string? StockNumber { get; set; }
    
    /// <summary>
    /// Background ID to use:
    /// - 16570 = Blanco Infinito
    /// - 20883 = Showroom Gris (default para Dealers)
    /// </summary>
    public string? BackgroundId { get; set; }
    
    /// <summary>Whether to mask/blur the license plate (default: true)</summary>
    public bool? MaskLicensePlate { get; set; }
    
    /// <summary>Enable interactive hotspots for damage/features (default: true)</summary>
    public bool? EnableHotspots { get; set; }
    
    /// <summary>Account type: 0=Individual, 1=Dealer, 2=Admin (required for permission check)</summary>
    public AccountType? AccountType { get; set; }
    
    /// <summary>Whether the user has an active subscription (required for Dealers)</summary>
    public bool? HasActiveSubscription { get; set; }
}

/// <summary>Response when 360° spin generation is started</summary>
public class SpinGenerateResponse
{
    /// <summary>Job ID to track progress</summary>
    public string JobId { get; set; } = string.Empty;
    
    /// <summary>Current status (processing, completed, failed)</summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>Human-readable message</summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>Number of images being processed</summary>
    public int ImageCount { get; set; }
    
    /// <summary>Estimated processing time in minutes</summary>
    public int EstimatedMinutes { get; set; }
    
    /// <summary>URL to check status</summary>
    public string CheckStatusUrl { get; set; } = string.Empty;
}

/// <summary>Response for 360° spin status check</summary>
public class SpinStatusResponse
{
    /// <summary>Job ID</summary>
    public string JobId { get; set; } = string.Empty;
    
    /// <summary>Current status: processing, completed, failed</summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>Spin ID from Spyne</summary>
    public string? SpinId { get; set; }
    
    /// <summary>
    /// URL to embed the interactive 360° viewer.
    /// Available when status is "completed".
    /// </summary>
    public string? EmbedUrl { get; set; }
    
    /// <summary>AI processing status from Spyne</summary>
    public string? SpinAiStatus { get; set; }
    
    /// <summary>List of processed images with studio backgrounds</summary>
    public List<SpinProcessedImage>? ProcessedImages { get; set; }
    
    /// <summary>Total number of frames in the spin</summary>
    public int TotalFrames { get; set; }
    
    /// <summary>Number of frames completed</summary>
    public int CompletedFrames { get; set; }
}

/// <summary>Individual processed image in a 360° spin</summary>
public class SpinProcessedImage
{
    /// <summary>Image ID</summary>
    public string? ImageId { get; set; }
    
    /// <summary>Frame number in the spin sequence</summary>
    public string? FrameNumber { get; set; }
    
    /// <summary>Original input image URL</summary>
    public string? OriginalUrl { get; set; }
    
    /// <summary>Processed image URL with studio background</summary>
    public string? ProcessedUrl { get; set; }
    
    /// <summary>Processing status</summary>
    public string? Status { get; set; }
    
    /// <summary>Image category (Exterior, Interior, Misc)</summary>
    public string? Category { get; set; }
    
    /// <summary>Detected angle in degrees</summary>
    public int? Angle { get; set; }
}

#endregion
