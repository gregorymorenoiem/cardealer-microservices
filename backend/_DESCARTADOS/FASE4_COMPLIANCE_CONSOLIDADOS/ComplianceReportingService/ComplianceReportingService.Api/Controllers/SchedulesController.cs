// ComplianceReportingService - Schedules Controller
// Programación automática de reportes

namespace ComplianceReportingService.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ComplianceReportingService.Application.DTOs;
using ComplianceReportingService.Application.Features.Commands;
using ComplianceReportingService.Application.Features.Queries;
using ComplianceReportingService.Domain.Entities;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Compliance")]
public class SchedulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SchedulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Crea una nueva programación de reporte
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ReportScheduleDto>> Create([FromBody] CreateScheduleDto request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "system";
        var result = await _mediator.Send(new CreateScheduleCommand(
            request.Name, request.ReportType, request.Frequency,
            request.Format, request.Destination, request.CronExpression,
            request.AutoSubmit, request.NotificationEmail, request.ParametersJson, userId));
        return Created($"/api/schedules/{result.Id}", result);
    }

    /// <summary>
    /// Obtiene una programación por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReportScheduleDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetScheduleByIdQuery(id));
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Lista programaciones activas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ReportScheduleDto>>> GetActive()
    {
        var result = await _mediator.Send(new GetActiveSchedulesQuery());
        return Ok(result);
    }

    /// <summary>
    /// Obtiene programaciones por tipo de reporte
    /// </summary>
    [HttpGet("by-type/{type}")]
    public async Task<ActionResult<List<ReportScheduleDto>>> GetByType(ReportType type)
    {
        var result = await _mediator.Send(new GetSchedulesByTypeQuery(type));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene programaciones próximas a ejecutar
    /// </summary>
    [HttpGet("upcoming")]
    public async Task<ActionResult<List<ReportScheduleDto>>> GetUpcoming(
        [FromQuery] int hoursAhead = 24)
    {
        var until = DateTime.UtcNow.AddHours(hoursAhead);
        var result = await _mediator.Send(new GetUpcomingSchedulesQuery(until));
        return Ok(result);
    }

    /// <summary>
    /// Activa/desactiva una programación
    /// </summary>
    [HttpPatch("{id:guid}/toggle")]
    public async Task<ActionResult> Toggle(Guid id, [FromBody] ToggleRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "system";
        var result = await _mediator.Send(new ToggleScheduleCommand(id, request.IsActive, userId));
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Ejecuta manualmente una programación
    /// </summary>
    [HttpPost("{id:guid}/execute")]
    public async Task<ActionResult<ReportDto>> Execute(Guid id)
    {
        var userId = User.FindFirst("sub")?.Value ?? "system";
        var result = await _mediator.Send(new ExecuteScheduleCommand(id, userId));
        return Ok(result);
    }

    /// <summary>
    /// Elimina una programación
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = User.FindFirst("sub")?.Value ?? "system";
        var result = await _mediator.Send(new DeleteScheduleCommand(id, userId));
        return result ? NoContent() : NotFound();
    }
}

public record ToggleRequest(bool IsActive);
