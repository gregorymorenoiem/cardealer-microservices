using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Video360Service.Application.DTOs;
using Video360Service.Application.Features.Commands;
using Video360Service.Application.Features.Queries;
using Video360Service.Domain.Entities;

namespace Video360Service.Api.Controllers;

/// <summary>
/// Controlador para procesamiento de videos 360 de vehículos
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
    /// Procesa un video 360 desde una URL (para uso interno/API)
    /// </summary>
    /// <remarks>
    /// Este endpoint es usado por el orquestador Vehicle360ProcessingService.
    /// Espera una URL del video ya subido a S3.
    /// </remarks>
    [HttpPost("process")]
    [ProducesResponseType(typeof(ProcessVideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessVideo(
        [FromBody] ProcessVideoFromUrlRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.VideoUrl))
        {
            return BadRequest(new { error = "VideoUrl es requerido" });
        }

        _logger.LogInformation(
            "Procesando video desde URL. VehicleId: {VehicleId}, Frames: {FrameCount}",
            request.VehicleId, request.FrameCount);

        var options = new ProcessingOptions
        {
            FrameCount = request.FrameCount > 0 ? request.FrameCount : 6,
            OutputWidth = request.OutputWidth ?? 1920,
            OutputHeight = request.OutputHeight ?? 1080,
            JpegQuality = request.JpegQuality ?? 90,
            OutputFormat = request.OutputFormat ?? "jpg",
            GenerateThumbnails = request.GenerateThumbnails,
            SmartFrameSelection = request.SmartFrameSelection,
            AutoCorrectExposure = request.AutoCorrectExposure
        };

        // Usar un UserId de sistema para requests de API
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            userId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // System user
        }

        var command = new CreateVideo360JobCommand
        {
            UserId = userId,
            VehicleId = request.VehicleId,
            VideoUrl = request.VideoUrl,
            OriginalFileName = $"video_{request.VehicleId}.mp4",
            FileSizeBytes = 0,
            Options = options
        };

        var result = await _mediator.Send(command, cancellationToken);

        // Si el worker está procesando síncrono, podemos esperar el resultado
        // Por ahora retornamos el job creado para polling
        return Ok(new ProcessVideoResponse
        {
            JobId = result.Id,
            Status = result.Status,
            Frames = result.Frames?.Select(f => new FrameInfo
            {
                SequenceNumber = f.SequenceNumber,
                ViewName = f.ViewName,
                AngleDegrees = f.AngleDegrees,
                ImageUrl = f.ImageUrl ?? string.Empty,
                ThumbnailUrl = f.ThumbnailUrl,
                Width = f.Width,
                Height = f.Height
            }).ToList() ?? new List<FrameInfo>()
        });
    }

    /// <summary>
    /// Obtiene el resultado completo de un job (con frames)
    /// </summary>
    [HttpGet("jobs/{jobId:guid}/result")]
    [ProducesResponseType(typeof(ProcessVideoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobResult(Guid jobId, CancellationToken cancellationToken)
    {
        var query = new GetVideo360JobQuery { JobId = jobId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { error = "Trabajo no encontrado" });
        }

        return Ok(new ProcessVideoResponse
        {
            JobId = result.Id,
            Status = result.Status,
            Progress = result.Progress,
            Frames = result.Frames?.Select(f => new FrameInfo
            {
                SequenceNumber = f.SequenceNumber,
                ViewName = f.ViewName,
                AngleDegrees = f.AngleDegrees,
                ImageUrl = f.ImageUrl ?? string.Empty,
                ThumbnailUrl = f.ThumbnailUrl,
                Width = f.Width,
                Height = f.Height
            }).ToList() ?? new List<FrameInfo>(),
            ErrorMessage = result.ErrorMessage
        });
    }

    /// <summary>
    /// Sube un video 360 para procesamiento
    /// </summary>
    /// <param name="file">Archivo de video (mp4, mov, avi, webm, mkv - max 500MB)</param>
    /// <param name="request">Opciones de procesamiento</param>
    /// <returns>Información del trabajo creado</returns>
    [HttpPost("upload")]
    [Authorize]
    [RequestSizeLimit(524_288_000)] // 500 MB
    [ProducesResponseType(typeof(UploadVideoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadVideo(
        IFormFile file,
        [FromForm] CreateVideo360JobRequest request,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No se proporcionó archivo de video" });
        }

        var allowedExtensions = new[] { ".mp4", ".mov", ".avi", ".webm", ".mkv" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { error = $"Formato no soportado. Use: {string.Join(", ", allowedExtensions)}" });
        }

        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { error = "Usuario no autenticado" });
        }

        _logger.LogInformation("Recibiendo video 360: {FileName}, {Size} bytes", 
            file.FileName, file.Length);

        // Guardar temporalmente el archivo
        var tempPath = Path.Combine(Path.GetTempPath(), "video360-uploads", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempPath);
        var filePath = Path.Combine(tempPath, file.FileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        // TODO: Subir a S3 y obtener URL
        var videoUrl = $"temp://{filePath}"; // Por ahora usar ruta local

        var options = new ProcessingOptions
        {
            FrameCount = request.FrameCount > 0 ? request.FrameCount : 6,
            OutputWidth = request.OutputWidth ?? 1920,
            OutputHeight = request.OutputHeight ?? 1080,
            JpegQuality = request.JpegQuality ?? 90,
            OutputFormat = request.OutputFormat ?? "jpg",
            GenerateThumbnails = request.GenerateThumbnails,
            SmartFrameSelection = request.SmartFrameSelection,
            AutoCorrectExposure = request.AutoCorrectExposure
        };

        var command = new CreateVideo360JobCommand
        {
            UserId = userId,
            VehicleId = request.VehicleId,
            VideoUrl = videoUrl,
            OriginalFileName = file.FileName,
            FileSizeBytes = file.Length,
            Options = options
        };

        var result = await _mediator.Send(command, cancellationToken);

        var response = new UploadVideoResponse
        {
            JobId = result.Id,
            Message = "Video recibido correctamente. Procesamiento en cola.",
            Status = result.Status,
            QueuePosition = result.QueuePosition ?? 0,
            EstimatedWaitSeconds = (result.QueuePosition ?? 0) * 60 // Estimado 1 min por video
        };

        return CreatedAtAction(nameof(GetJob), new { jobId = result.Id }, response);
    }

    /// <summary>
    /// Obtiene información de un trabajo de procesamiento
    /// </summary>
    [HttpGet("jobs/{jobId:guid}")]
    [ProducesResponseType(typeof(Video360JobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJob(Guid jobId, CancellationToken cancellationToken)
    {
        var query = new GetVideo360JobQuery { JobId = jobId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { error = "Trabajo no encontrado" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Obtiene el estado de un trabajo
    /// </summary>
    [HttpGet("jobs/{jobId:guid}/status")]
    [ProducesResponseType(typeof(JobStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobStatus(Guid jobId, CancellationToken cancellationToken)
    {
        var query = new GetJobStatusQuery { JobId = jobId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { error = "Trabajo no encontrado" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Obtiene todos los trabajos de un vehículo
    /// </summary>
    [HttpGet("vehicles/{vehicleId:guid}/jobs")]
    [ProducesResponseType(typeof(IEnumerable<Video360JobResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobsByVehicle(Guid vehicleId, CancellationToken cancellationToken)
    {
        var query = new GetJobsByVehicleQuery { VehicleId = vehicleId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene los datos del viewer 360 para un vehículo
    /// </summary>
    /// <remarks>
    /// Retorna las 6 imágenes del último procesamiento completado,
    /// listas para usar en un visor 360 interactivo.
    /// </remarks>
    [HttpGet("vehicles/{vehicleId:guid}/viewer")]
    [ProducesResponseType(typeof(Vehicle360ViewerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVehicle360Viewer(Guid vehicleId, CancellationToken cancellationToken)
    {
        var query = new GetVehicle360ViewerQuery { VehicleId = vehicleId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { error = "No se encontró vista 360 para este vehículo" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Obtiene los trabajos del usuario actual
    /// </summary>
    [HttpGet("my-jobs")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<Video360JobResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyJobs([FromQuery] int? limit, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var query = new GetJobsByUserQuery { UserId = userId, Limit = limit };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cancela un trabajo pendiente o en proceso
    /// </summary>
    [HttpPost("jobs/{jobId:guid}/cancel")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelJob(Guid jobId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new CancelVideo360JobCommand { JobId = jobId, UserId = userId };
        var result = await _mediator.Send(command, cancellationToken);

        if (!result)
        {
            return BadRequest(new { error = "No se pudo cancelar el trabajo" });
        }

        return Ok(new { message = "Trabajo cancelado" });
    }

    /// <summary>
    /// Reintenta un trabajo fallido
    /// </summary>
    [HttpPost("jobs/{jobId:guid}/retry")]
    [Authorize]
    [ProducesResponseType(typeof(Video360JobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetryJob(Guid jobId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new RetryVideo360JobCommand { JobId = jobId, UserId = userId };
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Elimina un trabajo y sus imágenes
    /// </summary>
    [HttpDelete("jobs/{jobId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteJob(Guid jobId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new DeleteVideo360JobCommand { JobId = jobId, UserId = userId };
        var result = await _mediator.Send(command, cancellationToken);

        if (!result)
        {
            return NotFound(new { error = "Trabajo no encontrado" });
        }

        return NoContent();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value 
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
