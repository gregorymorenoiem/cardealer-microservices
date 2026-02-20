using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Video360Service.Application.DTOs;
using Video360Service.Application.Features.Commands;
using Video360Service.Application.Features.Queries;
using Video360Service.Domain.Enums;

namespace Video360Service.Api.Controllers;

/// <summary>
/// Controller para procesamiento de video 360° de vehículos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class Video360Controller : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<Video360Controller> _logger;

    public Video360Controller(IMediator mediator, ILogger<Video360Controller> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Crea un nuevo job de procesamiento de video 360°
    /// </summary>
    /// <param name="request">Configuración del job</param>
    /// <returns>El job creado</returns>
    [HttpPost("jobs")]
    [Authorize]
    [ProducesResponseType(typeof(Video360JobResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Video360JobResponse>> CreateJob([FromBody] CreateVideo360JobRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        var tenantId = User.FindFirst("tenant")?.Value;
        
        var command = new CreateVideo360JobCommand
        {
            Request = request,
            UserId = userId != null ? Guid.Parse(userId) : null,
            TenantId = tenantId
        };
        
        var result = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetJob), new { id = result.JobId }, result);
    }

    /// <summary>
    /// Sube un video directamente para procesamiento (multipart/form-data)
    /// </summary>
    /// <param name="file">Archivo de video</param>
    /// <param name="vehicleId">ID del vehículo (opcional)</param>
    /// <param name="frameCount">Número de frames a extraer (default: 6)</param>
    /// <param name="imageFormat">Formato de imagen (Jpeg, Png, WebP)</param>
    /// <param name="quality">Calidad de video (Low, Medium, High, Ultra)</param>
    /// <returns>El job creado</returns>
    [HttpPost("jobs/upload")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(100_000_000)] // 100MB max
    [ProducesResponseType(typeof(Video360JobResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Video360JobResponse>> UploadAndProcess(
        IFormFile file,
        [FromQuery] Guid? vehicleId = null,
        [FromQuery] int frameCount = 6,
        [FromQuery] ImageFormat imageFormat = ImageFormat.Jpeg,
        [FromQuery] VideoQuality quality = VideoQuality.High)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No video file provided" });
        }
        
        var allowedTypes = new[] { "video/mp4", "video/quicktime", "video/x-msvideo", "video/webm" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return BadRequest(new { error = "Invalid video format. Allowed: MP4, MOV, AVI, WebM" });
        }
        
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        var tenantId = User.FindFirst("tenant")?.Value;

        // Leer bytes del archivo
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var videoBytes = ms.ToArray();
        
        var request = new CreateVideo360JobRequest
        {
            VehicleId = vehicleId,
            FrameCount = frameCount,
            OutputFormat = imageFormat,
            OutputQuality = quality,
            FileName = file.FileName,
            ProcessSync = true
        };
        
        var command = new CreateVideo360JobCommand
        {
            Request = request,
            UserId = userId != null ? Guid.Parse(userId) : null,
            TenantId = tenantId,
            VideoBytes = videoBytes
        };
        
        var result = await _mediator.Send(command);
        
        _logger.LogInformation("Video uploaded for job {JobId}, size: {Size} bytes", result.JobId, file.Length);
        
        return CreatedAtAction(nameof(GetJob), new { id = result.JobId }, result);
    }

    /// <summary>
    /// Obtiene un job por su ID
    /// </summary>
    /// <param name="id">ID del job</param>
    /// <returns>El job con sus frames</returns>
    [HttpGet("jobs/{id:guid}")]
    [ProducesResponseType(typeof(Video360JobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Video360JobResponse>> GetJob(Guid id)
    {
        var query = new GetVideo360JobByIdQuery { JobId = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound(new { error = $"Job {id} not found" });
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Lista todos los jobs del usuario
    /// </summary>
    /// <param name="vehicleId">Filtrar por vehículo</param>
    /// <param name="status">Filtrar por estado</param>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <returns>Lista paginada de jobs</returns>
    [HttpGet("jobs")]
    [Authorize]
    [ProducesResponseType(typeof(Video360JobListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<Video360JobListResponse>> GetJobs(
        [FromQuery] Guid? vehicleId = null,
        [FromQuery] ProcessingStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        
        var query = new GetVideo360JobsQuery
        {
            UserId = userId != null ? Guid.Parse(userId) : null,
            VehicleId = vehicleId,
            Status = status,
            Page = page,
            PageSize = pageSize
        };
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene la vista 360 de un vehículo
    /// </summary>
    /// <param name="vehicleId">ID del vehículo</param>
    /// <returns>Vista 360 con todos los frames</returns>
    [HttpGet("vehicle/{vehicleId:guid}")]
    [ProducesResponseType(typeof(Vehicle360ViewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Vehicle360ViewResponse>> GetVehicle360View(Guid vehicleId)
    {
        var query = new GetVehicle360ViewQuery { VehicleId = vehicleId };
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound(new { error = $"No 360 view found for vehicle {vehicleId}" });
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Cancela un job en progreso
    /// </summary>
    /// <param name="id">ID del job</param>
    /// <returns>Resultado de la cancelación</returns>
    [HttpPost("jobs/{id:guid}/cancel")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CancelJob(Guid id)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        
        var command = new CancelVideo360JobCommand
        {
            JobId = id,
            UserId = userId != null ? Guid.Parse(userId) : null
        };
        
        var result = await _mediator.Send(command);
        
        if (!result)
        {
            return NotFound(new { error = $"Job {id} not found or cannot be cancelled" });
        }
        
        return Ok(new { message = "Job cancelled successfully" });
    }

    /// <summary>
    /// Reintenta un job fallido
    /// </summary>
    /// <param name="id">ID del job</param>
    /// <returns>El job actualizado</returns>
    [HttpPost("jobs/{id:guid}/retry")]
    [Authorize]
    [ProducesResponseType(typeof(Video360JobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Video360JobResponse>> RetryJob(Guid id)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        
        var command = new RetryVideo360JobCommand
        {
            JobId = id,
            UserId = userId != null ? Guid.Parse(userId) : null
        };
        
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Elimina un job y sus archivos
    /// </summary>
    /// <param name="id">ID del job</param>
    /// <returns>Resultado de la eliminación</returns>
    [HttpDelete("jobs/{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteJob(Guid id)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        
        var command = new DeleteVideo360JobCommand
        {
            JobId = id,
            UserId = userId != null ? Guid.Parse(userId) : null
        };
        
        var result = await _mediator.Send(command);
        
        if (!result)
        {
            return NotFound(new { error = $"Job {id} not found" });
        }
        
        return NoContent();
    }
}
