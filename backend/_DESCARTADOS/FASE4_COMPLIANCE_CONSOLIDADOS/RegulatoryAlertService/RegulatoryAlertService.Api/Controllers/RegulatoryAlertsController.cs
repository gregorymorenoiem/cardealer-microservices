using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegulatoryAlertService.Application.DTOs;
using RegulatoryAlertService.Application.Features.Alerts.Commands;
using RegulatoryAlertService.Application.Features.Alerts.Queries;
using RegulatoryAlertService.Domain.Enums;

namespace RegulatoryAlertService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegulatoryAlertsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RegulatoryAlertsController> _logger;

    public RegulatoryAlertsController(IMediator mediator, ILogger<RegulatoryAlertsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // ===== ALERTS =====

    /// <summary>
    /// Obtiene una alerta regulatoria por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RegulatoryAlertDto>> GetAlertById(Guid id)
    {
        var alert = await _mediator.Send(new GetAlertByIdQuery(id));
        if (alert == null) return NotFound();
        return Ok(alert);
    }

    /// <summary>
    /// Obtiene todas las alertas activas
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<List<RegulatoryAlertSummaryDto>>> GetActiveAlerts()
    {
        var alerts = await _mediator.Send(new GetActiveAlertsQuery());
        return Ok(alerts);
    }

    /// <summary>
    /// Obtiene alertas por ente regulador
    /// </summary>
    [HttpGet("by-body/{body}")]
    public async Task<ActionResult<List<RegulatoryAlertSummaryDto>>> GetAlertsByBody(RegulatoryBody body)
    {
        var alerts = await _mediator.Send(new GetAlertsByRegulatoryBodyQuery(body));
        return Ok(alerts);
    }

    /// <summary>
    /// Obtiene alertas con fechas límite próximas
    /// </summary>
    [HttpGet("upcoming-deadlines")]
    public async Task<ActionResult<List<RegulatoryAlertSummaryDto>>> GetUpcomingDeadlines([FromQuery] int days = 30)
    {
        var alerts = await _mediator.Send(new GetUpcomingDeadlinesQuery(days));
        return Ok(alerts);
    }

    /// <summary>
    /// Crea una nueva alerta regulatoria
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<RegulatoryAlertDto>> CreateAlert([FromBody] CreateAlertDto dto)
    {
        _logger.LogInformation("Creando alerta regulatoria: {Title}", dto.Title);
        var alert = await _mediator.Send(new CreateAlertCommand(dto));
        return CreatedAtAction(nameof(GetAlertById), new { id = alert.Id }, alert);
    }

    /// <summary>
    /// Publica una alerta (cambia status a Published)
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<IActionResult> PublishAlert(Guid id)
    {
        _logger.LogInformation("Publicando alerta: {AlertId}", id);
        await _mediator.Send(new PublishAlertCommand(id));
        return NoContent();
    }

    /// <summary>
    /// Marca una alerta como resuelta
    /// </summary>
    [HttpPost("{id:guid}/resolve")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<IActionResult> ResolveAlert(Guid id, [FromBody] string resolution)
    {
        _logger.LogInformation("Resolviendo alerta: {AlertId}", id);
        await _mediator.Send(new ResolveAlertCommand(id, resolution));
        return NoContent();
    }

    // ===== SUBSCRIPTIONS =====

    /// <summary>
    /// Obtiene las suscripciones del usuario actual
    /// </summary>
    [HttpGet("subscriptions/user/{userId}")]
    [Authorize]
    public async Task<ActionResult<List<AlertSubscriptionDto>>> GetUserSubscriptions(string userId)
    {
        var subscriptions = await _mediator.Send(new GetUserSubscriptionsQuery(userId));
        return Ok(subscriptions);
    }

    /// <summary>
    /// Crea una nueva suscripción a alertas
    /// </summary>
    [HttpPost("subscriptions")]
    [Authorize]
    public async Task<ActionResult<AlertSubscriptionDto>> CreateSubscription([FromBody] CreateSubscriptionDto dto)
    {
        _logger.LogInformation("Creando suscripción para usuario: {UserId}", dto.UserId);
        var subscription = await _mediator.Send(new CreateSubscriptionCommand(dto));
        return CreatedAtAction(nameof(GetUserSubscriptions), new { userId = dto.UserId }, subscription);
    }

    // ===== CALENDAR =====

    /// <summary>
    /// Obtiene entradas de calendario próximas
    /// </summary>
    [HttpGet("calendar/upcoming")]
    public async Task<ActionResult<List<RegulatoryCalendarEntryDto>>> GetUpcomingCalendarEntries([FromQuery] int days = 30)
    {
        var entries = await _mediator.Send(new GetUpcomingCalendarEntriesQuery(days));
        return Ok(entries);
    }

    /// <summary>
    /// Crea una entrada de calendario regulatorio
    /// </summary>
    [HttpPost("calendar")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<RegulatoryCalendarEntryDto>> CreateCalendarEntry([FromBody] CreateCalendarEntryDto dto)
    {
        _logger.LogInformation("Creando entrada de calendario: {Title}", dto.Title);
        var entry = await _mediator.Send(new CreateCalendarEntryCommand(dto));
        return Created($"/api/regulatoryalerts/calendar/{entry.Id}", entry);
    }

    // ===== DEADLINES =====

    /// <summary>
    /// Obtiene los deadlines del usuario
    /// </summary>
    [HttpGet("deadlines/user/{userId}")]
    [Authorize]
    public async Task<ActionResult<List<ComplianceDeadlineDto>>> GetUserDeadlines(
        string userId, [FromQuery] bool pendingOnly = true)
    {
        var deadlines = await _mediator.Send(new GetUserDeadlinesQuery(userId, pendingOnly));
        return Ok(deadlines);
    }

    /// <summary>
    /// Crea un deadline de compliance
    /// </summary>
    [HttpPost("deadlines")]
    [Authorize]
    public async Task<ActionResult<ComplianceDeadlineDto>> CreateDeadline([FromBody] CreateDeadlineDto dto)
    {
        _logger.LogInformation("Creando deadline: {Title} para usuario: {UserId}", dto.Title, dto.UserId);
        var deadline = await _mediator.Send(new CreateDeadlineCommand(dto));
        return Created($"/api/regulatoryalerts/deadlines/{deadline.Id}", deadline);
    }

    /// <summary>
    /// Marca un deadline como completado
    /// </summary>
    [HttpPost("deadlines/{id:guid}/complete")]
    [Authorize]
    public async Task<IActionResult> CompleteDeadline(Guid id, [FromBody] CompleteDeadlineDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");
        
        _logger.LogInformation("Completando deadline: {DeadlineId}", id);
        await _mediator.Send(new CompleteDeadlineCommand(dto));
        return NoContent();
    }

    // ===== STATISTICS =====

    /// <summary>
    /// Obtiene estadísticas de alertas regulatorias
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<RegulatoryStatisticsDto>> GetStatistics()
    {
        var stats = await _mediator.Send(new GetRegulatoryStatisticsQuery());
        return Ok(stats);
    }

    // ===== HEALTH =====

    /// <summary>
    /// Health check del servicio
    /// </summary>
    [HttpGet("/health")]
    [AllowAnonymous]
    public IActionResult Health() => Ok(new { status = "healthy", service = "RegulatoryAlertService" });
}
