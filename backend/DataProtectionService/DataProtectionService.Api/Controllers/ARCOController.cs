using Microsoft.AspNetCore.Mvc;
using MediatR;
using DataProtectionService.Application.DTOs;
using DataProtectionService.Application.Commands;
using DataProtectionService.Application.Queries;

namespace DataProtectionService.Api.Controllers;

/// <summary>
/// Controller para gestión de solicitudes ARCO (Acceso, Rectificación, Cancelación, Oposición)
/// Cumplimiento de Ley 172-13 de Protección de Datos Personales
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ARCOController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ARCOController> _logger;

    public ARCOController(IMediator mediator, ILogger<ARCOController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtener una solicitud ARCO por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ARCORequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ARCORequestDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetARCORequestByIdQuery(id), cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtener una solicitud ARCO por número de solicitud
    /// </summary>
    [HttpGet("by-number/{requestNumber}")]
    [ProducesResponseType(typeof(ARCORequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ARCORequestDto>> GetByNumber(string requestNumber, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetARCORequestByNumberQuery(requestNumber), cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Obtener todas las solicitudes ARCO de un usuario
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(PaginatedResult<ARCORequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<ARCORequestDto>>> GetUserRequests(
        Guid userId,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetUserARCORequestsQuery(userId, status, type, page, pageSize), 
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtener solicitudes ARCO pendientes (Admin)
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(PaginatedResult<ARCORequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<ARCORequestDto>>> GetPending(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool overdueOnly = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetPendingARCORequestsQuery(page, pageSize, overdueOnly), 
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtener estadísticas de solicitudes ARCO
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ARCOStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ARCOStatisticsDto>> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetARCOStatisticsQuery(fromDate, toDate), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Crear una nueva solicitud ARCO (Ley 172-13 - plazo de 30 días)
    /// Tipos: Access (Acceso), Rectification (Rectificación), Cancellation (Cancelación), Opposition (Oposición)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ARCORequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ARCORequestDto>> Create(
        [FromBody] CreateARCORequest request,
        CancellationToken cancellationToken = default)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var command = new CreateARCORequestCommand(
            request.UserId,
            request.Type,
            request.Description,
            request.SpecificDataRequested,
            request.ProposedChanges,
            request.OppositionReason,
            ipAddress
        );

        var result = await _mediator.Send(command, cancellationToken);
        
        _logger.LogInformation(
            "ARCO Request created: {RequestNumber} - Type: {Type} - User: {UserId}", 
            result.RequestNumber, result.Type, request.UserId);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Procesar una solicitud ARCO (Admin) - Cambiar estado
    /// </summary>
    [HttpPost("{id}/process")]
    [ProducesResponseType(typeof(ARCORequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ARCORequestDto>> Process(
        Guid id,
        [FromBody] ProcessARCORequest request,
        [FromHeader(Name = "X-User-Id")] Guid processedById,
        [FromHeader(Name = "X-User-Name")] string processedByName,
        CancellationToken cancellationToken = default)
    {
        var command = new ProcessARCORequestCommand(
            id,
            processedById,
            processedByName,
            request.Status,
            request.Resolution,
            request.RejectionReason,
            request.InternalNotes
        );

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation(
                "ARCO Request {RequestId} processed to status {Status} by {ProcessedBy}", 
                id, request.Status, processedByName);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Agregar un archivo adjunto a una solicitud ARCO
    /// </summary>
    [HttpPost("{id}/attachments")]
    [ProducesResponseType(typeof(ARCOAttachmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ARCOAttachmentDto>> AddAttachment(
        Guid id,
        [FromBody] AddAttachmentRequest request,
        [FromHeader(Name = "X-User-Id")] Guid uploadedById,
        CancellationToken cancellationToken = default)
    {
        var command = new AddARCOAttachmentCommand(
            id,
            request.FileName,
            request.FileUrl,
            request.FileType,
            request.FileSize,
            uploadedById
        );

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Created($"/api/arco/{id}/attachments/{result.Id}", result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Completar exportación de datos (para solicitudes de Acceso)
    /// </summary>
    [HttpPost("{id}/complete-export")]
    [ProducesResponseType(typeof(ARCORequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ARCORequestDto>> CompleteExport(
        Guid id,
        [FromBody] CompleteExportRequest request,
        [FromHeader(Name = "X-User-Id")] Guid processedById,
        [FromHeader(Name = "X-User-Name")] string processedByName,
        CancellationToken cancellationToken = default)
    {
        var command = new CompleteARCOExportCommand(
            id,
            request.ExportFileUrl,
            processedById,
            processedByName
        );

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation(
                "ARCO Export completed for request {RequestId}", id);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

public record AddAttachmentRequest(string FileName, string FileUrl, string FileType, long FileSize);
public record CompleteExportRequest(string ExportFileUrl);
