using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vehicle360ProcessingService.Application.DTOs;
using Vehicle360ProcessingService.Application.Features.Commands;
using Vehicle360ProcessingService.Application.Features.Queries;
using Vehicle360ProcessingService.Domain.Entities;

namespace Vehicle360ProcessingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Vehicle360ProcessingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<Vehicle360ProcessingController> _logger;

    public Vehicle360ProcessingController(
        IMediator mediator,
        ILogger<Vehicle360ProcessingController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Inicia el procesamiento 360 de un vehículo
    /// </summary>
    /// <remarks>
    /// Recibe un video de 360 grados y orquesta:
    /// 1. Upload a S3 (MediaService)
    /// 2. Extracción de 6 frames (Video360Service)
    /// 3. Remoción de fondos (BackgroundRemovalService)
    /// 4. Upload de imágenes finales a S3
    /// </remarks>
    [HttpPost("process")]
    [Authorize]
    [RequestSizeLimit(524288000)] // 500 MB
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<StartProcessingResponse>> StartProcessing(
        [FromForm] IFormFile video,
        [FromForm] Guid vehicleId,
        [FromForm] int frameCount = 6,
        [FromForm] int? outputWidth = null,
        [FromForm] int? outputHeight = null,
        [FromForm] string outputFormat = "png",
        [FromForm] bool smartFrameSelection = true,
        [FromForm] bool autoCorrectExposure = true,
        [FromForm] bool generateThumbnails = true,
        [FromForm] string backgroundColor = "transparent")
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        if (video == null || video.Length == 0)
        {
            return BadRequest(new { message = "Video file is required" });
        }

        _logger.LogInformation(
            "Processing 360 video request. VehicleId: {VehicleId}, FileName: {FileName}, Size: {Size} bytes",
            vehicleId, video.FileName, video.Length);

        var command = new StartVehicle360ProcessingCommand
        {
            UserId = userId,
            VehicleId = vehicleId,
            VideoStream = video.OpenReadStream(),
            VideoFileName = video.FileName,
            VideoContentType = video.ContentType,
            VideoSize = video.Length,
            FrameCount = frameCount,
            Options = new ProcessingOptions
            {
                OutputWidth = outputWidth ?? 1920,
                OutputHeight = outputHeight ?? 1080,
                OutputFormat = outputFormat,
                SmartFrameSelection = smartFrameSelection,
                AutoCorrectExposure = autoCorrectExposure,
                GenerateThumbnails = generateThumbnails,
                BackgroundColor = backgroundColor
            },
            ClientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            CorrelationId = HttpContext.TraceIdentifier
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Inicia procesamiento desde URL de video ya existente
    /// </summary>
    [HttpPost("process-from-url")]
    [Authorize]
    public async Task<ActionResult<StartProcessingResponse>> StartProcessingFromUrl(
        [FromBody] ProcessFromUrlRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        _logger.LogInformation(
            "Processing 360 video from URL. VehicleId: {VehicleId}, URL: {Url}",
            request.VehicleId, request.VideoUrl);

        var command = new StartVehicle360ProcessingCommand
        {
            UserId = userId,
            VehicleId = request.VehicleId,
            VideoUrl = request.VideoUrl,
            FrameCount = request.FrameCount,
            Options = new ProcessingOptions
            {
                OutputWidth = request.OutputWidth ?? 1920,
                OutputHeight = request.OutputHeight ?? 1080,
                OutputFormat = request.OutputFormat ?? "png",
                SmartFrameSelection = request.SmartFrameSelection,
                AutoCorrectExposure = request.AutoCorrectExposure,
                GenerateThumbnails = request.GenerateThumbnails,
                BackgroundColor = request.BackgroundColor ?? "transparent"
            },
            ClientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            CorrelationId = HttpContext.TraceIdentifier
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el estado de un job de procesamiento
    /// </summary>
    [HttpGet("jobs/{jobId}/status")]
    public async Task<ActionResult<JobStatusResponse>> GetJobStatus(Guid jobId)
    {
        var result = await _mediator.Send(new GetJobStatusQuery { JobId = jobId });
        if (result == null)
        {
            return NotFound(new { message = $"Job {jobId} not found" });
        }
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el resultado completo de un job
    /// </summary>
    [HttpGet("jobs/{jobId}")]
    public async Task<ActionResult<Vehicle360JobResponse>> GetJob(Guid jobId)
    {
        var result = await _mediator.Send(new GetVehicle360JobQuery { JobId = jobId });
        if (result == null)
        {
            return NotFound(new { message = $"Job {jobId} not found" });
        }
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el resultado del procesamiento con frames
    /// </summary>
    [HttpGet("jobs/{jobId}/result")]
    public async Task<ActionResult<ProcessingResultResponse>> GetJobResult(Guid jobId)
    {
        var result = await _mediator.Send(new GetProcessingResultQuery { JobId = jobId });
        if (result == null)
        {
            return NotFound(new { message = $"Job {jobId} not found" });
        }
        return Ok(result);
    }

    /// <summary>
    /// Reintenta un job fallido
    /// </summary>
    [HttpPost("jobs/{jobId}/retry")]
    [Authorize]
    public async Task<ActionResult<Vehicle360JobResponse>> RetryJob(Guid jobId)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        try
        {
            var result = await _mediator.Send(new RetryVehicle360JobCommand 
            { 
                JobId = jobId, 
                UserId = userId 
            });
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancela un job en progreso
    /// </summary>
    [HttpPost("jobs/{jobId}/cancel")]
    [Authorize]
    public async Task<ActionResult> CancelJob(Guid jobId, [FromBody] CancelJobRequest? request = null)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        try
        {
            var success = await _mediator.Send(new CancelVehicle360JobCommand
            {
                JobId = jobId,
                UserId = userId,
                Reason = request?.Reason
            });

            if (!success)
            {
                return BadRequest(new { message = "Job cannot be cancelled" });
            }

            return Ok(new { message = "Job cancelled successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Obtiene los datos del visor 360 para un vehículo
    /// </summary>
    [HttpGet("viewer/{vehicleId}")]
    public async Task<ActionResult<Vehicle360ViewerResponse>> GetVehicleViewer(Guid vehicleId)
    {
        var result = await _mediator.Send(new GetVehicle360ViewerQuery { VehicleId = vehicleId });
        if (result == null)
        {
            return NotFound(new { message = $"No 360 data found for vehicle {vehicleId}" });
        }
        return Ok(result);
    }

    /// <summary>
    /// Obtiene todos los jobs de un vehículo
    /// </summary>
    [HttpGet("vehicles/{vehicleId}/jobs")]
    public async Task<ActionResult<List<Vehicle360JobResponse>>> GetVehicleJobs(Guid vehicleId)
    {
        var result = await _mediator.Send(new GetJobsByVehicleQuery { VehicleId = vehicleId });
        return Ok(result);
    }

    /// <summary>
    /// Obtiene los jobs del usuario actual
    /// </summary>
    [HttpGet("my-jobs")]
    [Authorize]
    public async Task<ActionResult<List<Vehicle360JobResponse>>> GetMyJobs(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var result = await _mediator.Send(new GetUserJobsQuery 
        { 
            UserId = userId,
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }

    /// <summary>
    /// Verifica la salud de los servicios dependientes
    /// </summary>
    [HttpGet("health/services")]
    public async Task<ActionResult<ServicesHealthResponse>> CheckServicesHealth()
    {
        var result = await _mediator.Send(new CheckServicesHealthQuery());
        return result.AllHealthy ? Ok(result) : StatusCode(503, result);
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst("sub") ?? 
                          User.FindFirst("userId") ?? 
                          User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return Guid.Empty;
    }
}

// Request models
public class ProcessFromUrlRequest
{
    public Guid VehicleId { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public int FrameCount { get; set; } = 6;
    public int? OutputWidth { get; set; }
    public int? OutputHeight { get; set; }
    public string? OutputFormat { get; set; }
    public bool SmartFrameSelection { get; set; } = true;
    public bool AutoCorrectExposure { get; set; } = true;
    public bool GenerateThumbnails { get; set; } = true;
    public string? BackgroundColor { get; set; }
}

public class CancelJobRequest
{
    public string? Reason { get; set; }
}
