using MediatR;
using Microsoft.AspNetCore.Mvc;
using DataProtectionService.Application.Commands;
using DataProtectionService.Application.Queries;
using DataProtectionService.Application.DTOs;

namespace DataProtectionService.Api.Controllers;

/// <summary>
/// Controller para exportación y anonimización de datos personales
/// Implementa derechos de Acceso y Cancelación según Ley 172-13
/// </summary>
[ApiController]
[Route("api/data")]
public class DataExportController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DataExportController> _logger;

    public DataExportController(IMediator mediator, ILogger<DataExportController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Solicitar exportación de datos personales (Derecho de Acceso)
    /// </summary>
    [HttpPost("exports")]
    [ProducesResponseType(typeof(DataExportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataExportDto>> RequestDataExport(
        [FromBody] RequestDataExportRequest request,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken = default)
    {
        var command = new RequestDataExportCommand(
            userId,
            request.ARCORequestId,
            request.Format,
            request.IncludeTransactions,
            request.IncludeMessages,
            request.IncludeVehicleHistory,
            request.IncludeUserActivity,
            GetClientIpAddress(),
            Request.Headers.UserAgent.ToString()
        );

        var result = await _mediator.Send(command, cancellationToken);
        
        _logger.LogInformation("Data export requested for user {UserId}, format: {Format}", userId, request.Format);
        
        return Created($"/api/data/exports/{result.Id}", result);
    }

    /// <summary>
    /// Obtener estado de una exportación
    /// </summary>
    [HttpGet("exports/{exportId}")]
    [ProducesResponseType(typeof(DataExportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataExportDto>> GetExport(
        Guid exportId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetDataExportQuery(exportId), cancellationToken);
        if (result == null)
            return NotFound("Export not found");
        return Ok(result);
    }

    /// <summary>
    /// Obtener exportaciones de un usuario
    /// </summary>
    [HttpGet("exports/user/{userId}")]
    [ProducesResponseType(typeof(List<DataExportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DataExportDto>>> GetUserExports(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetDataExportsByUserQuery(userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Anonimizar datos de un usuario (Derecho de Cancelación - Ley 172-13)
    /// </summary>
    [HttpPost("user/{userId}/anonymize")]
    [ProducesResponseType(typeof(AnonymizationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AnonymizationResultDto>> AnonymizeUserData(
        Guid userId,
        [FromBody] AnonymizeRequest request,
        [FromHeader(Name = "X-Admin-Id")] Guid? adminId,
        CancellationToken cancellationToken = default)
    {
        var command = new AnonymizeUserDataCommand(
            userId,
            request.ARCORequestId,
            adminId,
            request.Reason,
            request.OriginalEmail,
            request.OriginalPhone
        );

        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result.Message);
        
        _logger.LogWarning(
            "User data anonymized: {UserId} - Reason: {Reason} - ProcessedBy: {AdminId}", 
            userId, request.Reason, adminId);
        
        return Ok(result);
    }

    /// <summary>
    /// Verificar si un usuario ha sido anonimizado
    /// </summary>
    [HttpGet("user/{userId}/is-anonymized")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> IsUserAnonymized(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new CheckAnonymizationStatusQuery(userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtener registros de anonimización (Admin)
    /// </summary>
    [HttpGet("anonymization-records")]
    [ProducesResponseType(typeof(List<AnonymizationRecordDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AnonymizationRecordDto>>> GetAnonymizationRecords(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAnonymizationRecordsQuery(fromDate, toDate), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtener estadísticas de protección de datos
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(DataProtectionStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DataProtectionStatsDto>> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetDataProtectionStatsQuery(fromDate, toDate), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtener calendario de retención de datos
    /// </summary>
    [HttpGet("retention-schedule")]
    [ProducesResponseType(typeof(List<RetentionScheduleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RetentionScheduleDto>>> GetRetentionSchedule(
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetRetentionScheduleQuery(), cancellationToken);
        return Ok(result);
    }

    private string GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

// Request DTOs
public record RequestDataExportRequest(
    Guid? ARCORequestId = null,
    string? Format = "JSON",
    bool? IncludeTransactions = true,
    bool? IncludeMessages = true,
    bool? IncludeVehicleHistory = true,
    bool? IncludeUserActivity = true
);

public record AnonymizeRequest(
    Guid? ARCORequestId = null,
    string? Reason = null,
    string? OriginalEmail = null,
    string? OriginalPhone = null
);
