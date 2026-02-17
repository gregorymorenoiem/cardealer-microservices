// =====================================================
// C12: ComplianceIntegrationService - API Controller
// Endpoints REST para gestión de integraciones
// =====================================================

using ComplianceIntegrationService.Application.DTOs;
using ComplianceIntegrationService.Application.Features.Integrations.Commands;
using ComplianceIntegrationService.Application.Features.Integrations.Queries;
using ComplianceIntegrationService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceIntegrationService.Api.Controllers;

/// <summary>
/// Controller para gestión de integraciones con entes reguladores
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Compliance,System")]
public class IntegrationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<IntegrationsController> _logger;

    public IntegrationsController(IMediator mediator, ILogger<IntegrationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirst("sub")?.Value ?? "system";

    #region Integration Config Endpoints

    /// <summary>
    /// Obtener todas las integraciones configuradas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<IntegrationConfigDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<IntegrationConfigDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllIntegrationsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Obtener integración por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(IntegrationConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IntegrationConfigDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetIntegrationByIdQuery(id));
        if (result == null)
            return NotFound(new { Message = "Integración no encontrada" });
        return Ok(result);
    }

    /// <summary>
    /// Obtener integraciones por ente regulador
    /// </summary>
    [HttpGet("regulatory-body/{body}")]
    [ProducesResponseType(typeof(List<IntegrationConfigDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<IntegrationConfigDto>>> GetByRegulatoryBody(RegulatoryBody body)
    {
        var result = await _mediator.Send(new GetIntegrationsByRegulatoryBodyQuery(body));
        return Ok(result);
    }

    /// <summary>
    /// Obtener integraciones activas
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<IntegrationConfigDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<IntegrationConfigDto>>> GetActive()
    {
        var result = await _mediator.Send(new GetActiveIntegrationsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Crear nueva integración
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IntegrationConfigDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IntegrationConfigDto>> Create([FromBody] CreateIntegrationConfigDto dto)
    {
        try
        {
            var result = await _mediator.Send(new CreateIntegrationConfigCommand(dto, GetUserId()));
            _logger.LogInformation("Integración creada: {Id} - {Name}", result.Id, result.Name);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear integración");
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar integración existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IntegrationConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IntegrationConfigDto>> Update(Guid id, [FromBody] UpdateIntegrationConfigDto dto)
    {
        var result = await _mediator.Send(new UpdateIntegrationConfigCommand(id, dto, GetUserId()));
        if (result == null)
            return NotFound(new { Message = "Integración no encontrada" });
        return Ok(result);
    }

    /// <summary>
    /// Cambiar estado de integración
    /// </summary>
    [HttpPost("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request)
    {
        var success = await _mediator.Send(new ChangeIntegrationStatusCommand(id, request.Status, request.Reason, GetUserId()));
        if (!success)
            return NotFound(new { Message = "Integración no encontrada" });
        
        _logger.LogInformation("Estado de integración {Id} cambiado a {Status}", id, request.Status);
        return Ok(new { Message = $"Estado cambiado a {request.Status}" });
    }

    /// <summary>
    /// Eliminar integración (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new DeleteIntegrationConfigCommand(id, GetUserId()));
        if (!success)
            return NotFound(new { Message = "Integración no encontrada" });
        
        _logger.LogInformation("Integración eliminada: {Id}", id);
        return NoContent();
    }

    #endregion

    #region Credential Endpoints

    /// <summary>
    /// Crear credencial para integración
    /// </summary>
    [HttpPost("{integrationId:guid}/credentials")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IntegrationCredentialDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<IntegrationCredentialDto>> CreateCredential(Guid integrationId, [FromBody] CreateCredentialDto dto)
    {
        var dtoWithIntegration = dto with { IntegrationConfigId = integrationId };
        var result = await _mediator.Send(new CreateCredentialCommand(dtoWithIntegration, GetUserId()));
        return CreatedAtAction(nameof(GetById), new { id = integrationId }, result);
    }

    /// <summary>
    /// Eliminar credencial
    /// </summary>
    [HttpDelete("credentials/{credentialId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCredential(Guid credentialId)
    {
        var success = await _mediator.Send(new DeleteCredentialCommand(credentialId));
        if (!success)
            return NotFound(new { Message = "Credencial no encontrada" });
        return NoContent();
    }

    #endregion

    #region Transmission Endpoints

    /// <summary>
    /// Obtener transmisiones por integración
    /// </summary>
    [HttpGet("{integrationId:guid}/transmissions")]
    [ProducesResponseType(typeof(List<DataTransmissionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DataTransmissionDto>>> GetTransmissions(Guid integrationId, [FromQuery] int? limit = null)
    {
        var result = await _mediator.Send(new GetTransmissionsByIntegrationQuery(integrationId, limit));
        return Ok(result);
    }

    /// <summary>
    /// Obtener transmisión por ID
    /// </summary>
    [HttpGet("transmissions/{id:guid}")]
    [ProducesResponseType(typeof(DataTransmissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataTransmissionDto>> GetTransmission(Guid id)
    {
        var result = await _mediator.Send(new GetTransmissionByIdQuery(id));
        if (result == null)
            return NotFound(new { Message = "Transmisión no encontrada" });
        return Ok(result);
    }

    /// <summary>
    /// Crear nueva transmisión
    /// </summary>
    [HttpPost("transmissions")]
    [ProducesResponseType(typeof(DataTransmissionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<DataTransmissionDto>> CreateTransmission([FromBody] CreateTransmissionDto dto)
    {
        var result = await _mediator.Send(new CreateTransmissionCommand(dto, GetUserId()));
        _logger.LogInformation("Transmisión creada: {Code}", result.TransmissionCode);
        return CreatedAtAction(nameof(GetTransmission), new { id = result.Id }, result);
    }

    /// <summary>
    /// Actualizar estado de transmisión
    /// </summary>
    [HttpPatch("transmissions/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTransmissionStatus(Guid id, [FromBody] UpdateTransmissionStatusDto dto)
    {
        var success = await _mediator.Send(new UpdateTransmissionStatusCommand(id, dto, GetUserId()));
        if (!success)
            return NotFound(new { Message = "Transmisión no encontrada" });
        return Ok(new { Message = "Estado actualizado" });
    }

    /// <summary>
    /// Reintentar transmisión fallida
    /// </summary>
    [HttpPost("transmissions/{id:guid}/retry")]
    [ProducesResponseType(typeof(DataTransmissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataTransmissionDto>> RetryTransmission(Guid id)
    {
        var result = await _mediator.Send(new RetryTransmissionCommand(id, GetUserId()));
        if (result == null)
            return BadRequest(new { Message = "No se puede reintentar esta transmisión" });
        
        _logger.LogInformation("Reintentando transmisión: {Code}", result.TransmissionCode);
        return Ok(result);
    }

    /// <summary>
    /// Obtener transmisiones pendientes de reintento
    /// </summary>
    [HttpGet("transmissions/pending-retries")]
    [ProducesResponseType(typeof(List<DataTransmissionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DataTransmissionDto>>> GetPendingRetries()
    {
        var result = await _mediator.Send(new GetPendingRetriesQuery());
        return Ok(result);
    }

    /// <summary>
    /// Obtener transmisiones por rango de fechas
    /// </summary>
    [HttpGet("transmissions/by-date")]
    [ProducesResponseType(typeof(List<DataTransmissionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DataTransmissionDto>>> GetTransmissionsByDate(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var result = await _mediator.Send(new GetTransmissionsByDateRangeQuery(from, to));
        return Ok(result);
    }

    #endregion

    #region Field Mapping Endpoints

    /// <summary>
    /// Obtener mapeos de campos por integración
    /// </summary>
    [HttpGet("{integrationId:guid}/mappings")]
    [ProducesResponseType(typeof(List<FieldMappingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FieldMappingDto>>> GetFieldMappings(
        Guid integrationId,
        [FromQuery] ReportType? reportType = null)
    {
        var result = await _mediator.Send(new GetFieldMappingsByIntegrationQuery(integrationId, reportType));
        return Ok(result);
    }

    /// <summary>
    /// Crear mapeo de campo
    /// </summary>
    [HttpPost("mappings")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(FieldMappingDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<FieldMappingDto>> CreateFieldMapping([FromBody] CreateFieldMappingDto dto)
    {
        var result = await _mediator.Send(new CreateFieldMappingCommand(dto, GetUserId()));
        return Created("", result);
    }

    /// <summary>
    /// Eliminar mapeo de campo
    /// </summary>
    [HttpDelete("mappings/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteFieldMapping(Guid id)
    {
        var success = await _mediator.Send(new DeleteFieldMappingCommand(id));
        if (!success)
            return NotFound(new { Message = "Mapeo no encontrado" });
        return NoContent();
    }

    #endregion

    #region Webhook Endpoints

    /// <summary>
    /// Obtener webhooks por integración
    /// </summary>
    [HttpGet("{integrationId:guid}/webhooks")]
    [ProducesResponseType(typeof(List<WebhookConfigDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WebhookConfigDto>>> GetWebhooks(Guid integrationId)
    {
        var result = await _mediator.Send(new GetWebhooksByIntegrationQuery(integrationId));
        return Ok(result);
    }

    /// <summary>
    /// Crear webhook
    /// </summary>
    [HttpPost("webhooks")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WebhookConfigDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<WebhookConfigDto>> CreateWebhook([FromBody] CreateWebhookDto dto)
    {
        var result = await _mediator.Send(new CreateWebhookCommand(dto, GetUserId()));
        return Created("", result);
    }

    /// <summary>
    /// Toggle webhook habilitado/deshabilitado
    /// </summary>
    [HttpPatch("webhooks/{id:guid}/toggle")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleWebhook(Guid id, [FromQuery] bool enabled)
    {
        var success = await _mediator.Send(new ToggleWebhookCommand(id, enabled, GetUserId()));
        if (!success)
            return NotFound(new { Message = "Webhook no encontrado" });
        return Ok(new { Message = enabled ? "Webhook habilitado" : "Webhook deshabilitado" });
    }

    #endregion

    #region Log Endpoints

    /// <summary>
    /// Obtener logs por integración
    /// </summary>
    [HttpGet("{integrationId:guid}/logs")]
    [ProducesResponseType(typeof(List<IntegrationLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<IntegrationLogDto>>> GetLogs(Guid integrationId, [FromQuery] int limit = 100)
    {
        var result = await _mediator.Send(new GetLogsByIntegrationQuery(integrationId, limit));
        return Ok(result);
    }

    /// <summary>
    /// Obtener errores recientes
    /// </summary>
    [HttpGet("logs/errors")]
    [ProducesResponseType(typeof(List<IntegrationLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<IntegrationLogDto>>> GetRecentErrors([FromQuery] int count = 50)
    {
        var result = await _mediator.Send(new GetRecentErrorsQuery(count));
        return Ok(result);
    }

    /// <summary>
    /// Purgar logs antiguos
    /// </summary>
    [HttpPost("logs/purge")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PurgeLogs([FromQuery] int retentionDays = 90)
    {
        await _mediator.Send(new PurgeOldLogsCommand(retentionDays));
        _logger.LogInformation("Logs anteriores a {Days} días purgados", retentionDays);
        return Ok(new { Message = $"Logs anteriores a {retentionDays} días eliminados" });
    }

    #endregion

    #region Statistics Endpoints

    /// <summary>
    /// Obtener estadísticas generales de integraciones
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(IntegrationStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IntegrationStatisticsDto>> GetStatistics()
    {
        var result = await _mediator.Send(new GetIntegrationStatisticsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Obtener estadísticas detalladas de una integración
    /// </summary>
    [HttpGet("{id:guid}/statistics")]
    [ProducesResponseType(typeof(IntegrationDetailStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IntegrationDetailStatisticsDto>> GetDetailStatistics(Guid id)
    {
        var result = await _mediator.Send(new GetIntegrationDetailStatisticsQuery(id));
        if (result == null)
            return NotFound(new { Message = "Integración no encontrada" });
        return Ok(result);
    }

    /// <summary>
    /// Verificar salud de todas las integraciones
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<IntegrationHealthDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<IntegrationHealthDto>>> GetHealth()
    {
        var result = await _mediator.Send(new GetIntegrationsHealthQuery());
        return Ok(result);
    }

    #endregion
}

#region Request DTOs

public record ChangeStatusRequest
{
    public IntegrationStatus Status { get; init; }
    public string? Reason { get; init; }
}

#endregion
