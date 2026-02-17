// ComplianceReportingService - Reports Controller
// Gestión de reportes regulatorios DGII, UAF, Pro-Consumidor

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
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Genera un nuevo reporte regulatorio
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<ReportDto>> GenerateReport(
        [FromBody] GenerateReportDto request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "system";
        var result = await _mediator.Send(new GenerateReportCommand(
            request.Type, request.PeriodStart, request.PeriodEnd,
            request.Format, request.Destination, request.Description,
            request.ParametersJson, userId));
        return Created($"/api/reports/{result.Id}", result);
    }

    /// <summary>
    /// Obtiene un reporte por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReportDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetReportByIdQuery(id));
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Obtiene un reporte por número
    /// </summary>
    [HttpGet("by-number/{reportNumber}")]
    public async Task<ActionResult<ReportDto>> GetByNumber(string reportNumber)
    {
        var result = await _mediator.Send(new GetReportByNumberQuery(reportNumber));
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Lista reportes con paginación y filtros
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ReportPagedResultDto>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ReportType? type = null,
        [FromQuery] ReportStatus? status = null,
        [FromQuery] DestinationType? destination = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _mediator.Send(new GetReportsPagedQuery(
            page, pageSize, type, status, destination, fromDate, toDate, searchTerm));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene reportes pendientes de envío
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<List<ReportSummaryDto>>> GetPending(
        [FromQuery] DestinationType? destination = null)
    {
        var result = await _mediator.Send(new GetPendingReportsQuery(destination));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene reportes vencidos o próximos a vencer
    /// </summary>
    [HttpGet("overdue")]
    public async Task<ActionResult<List<ReportSummaryDto>>> GetOverdue(
        [FromQuery] int daysAhead = 7)
    {
        var result = await _mediator.Send(new GetOverdueReportsQuery(DateTime.UtcNow, daysAhead));
        return Ok(result);
    }

    /// <summary>
    /// Envía un reporte generado a la entidad regulatoria
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult<ReportDto>> SubmitReport(Guid id)
    {
        var userId = User.FindFirst("sub")?.Value ?? "system";
        var result = await _mediator.Send(new SubmitReportCommand(id, userId));
        return Ok(result);
    }

    /// <summary>
    /// Cancela un reporte pendiente
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Admin,Compliance")]
    public async Task<ActionResult> CancelReport(Guid id, [FromBody] CancelRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "system";
        var result = await _mediator.Send(new CancelReportCommand(id, request.Reason, userId));
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Confirma recepción de reporte por la entidad
    /// </summary>
    [HttpPost("{id:guid}/confirm")]
    [Authorize(Roles = "Admin,System")]
    public async Task<ActionResult> ConfirmSubmission(
        Guid id, [FromBody] ConfirmSubmissionRequest request)
    {
        var result = await _mediator.Send(new ConfirmSubmissionCommand(
            id, request.ConfirmationNumber, request.ResponseMessage));
        return result ? NoContent() : NotFound();
    }
}

public record CancelRequest(string Reason);
public record ConfirmSubmissionRequest(string ConfirmationNumber, string? ResponseMessage);
