// ComplianceReportingService - UAF Controller
// Gestión de reportes UAF (ROS, CTR) - Ley 155-17 PLD

namespace ComplianceReportingService.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ComplianceReportingService.Application.DTOs;
using ComplianceReportingService.Application.Features.Commands;
using ComplianceReportingService.Application.Features.Queries;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Compliance")]
public class UAFController : ControllerBase
{
    private readonly IMediator _mediator;

    public UAFController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Genera Reporte de Operación Sospechosa (ROS) - Art. 33 Ley 155-17
    /// </summary>
    [HttpPost("ros")]
    public async Task<ActionResult<UAFSubmissionDto>> GenerateROS([FromBody] ROSRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "system";
        var result = await _mediator.Send(new GenerateROSCommand(
            request.SubjectName, request.SubjectIdType, request.SubjectIdNumber,
            request.TransactionType, request.Amount, request.Currency,
            request.TransactionDate, request.SuspicionIndicators, request.Narrative,
            request.IsUrgent, userId));
        return Created($"/api/uaf/submissions/{result.Id}", result);
    }

    /// <summary>
    /// Genera Reporte de Transacción en Efectivo (CTR) - Ley 155-17
    /// </summary>
    [HttpPost("ctr")]
    public async Task<ActionResult<UAFSubmissionDto>> GenerateCTR([FromBody] CTRRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "system";
        var result = await _mediator.Send(new GenerateCTRCommand(
            request.PeriodStart, request.PeriodEnd, request.ThresholdAmount, userId));
        return Created($"/api/uaf/submissions/{result.Id}", result);
    }

    /// <summary>
    /// Obtiene una presentación UAF por ID
    /// </summary>
    [HttpGet("submissions/{id:guid}")]
    public async Task<ActionResult<UAFSubmissionDto>> GetSubmission(Guid id)
    {
        var result = await _mediator.Send(new GetUAFSubmissionByIdQuery(id));
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Lista presentaciones UAF por período
    /// </summary>
    [HttpGet("submissions")]
    public async Task<ActionResult<List<UAFSubmissionDto>>> GetByPeriod(
        [FromQuery] string period)
    {
        var result = await _mediator.Send(new GetUAFSubmissionsByPeriodQuery(period));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene ROS urgentes pendientes
    /// </summary>
    [HttpGet("urgent-pending")]
    public async Task<ActionResult<List<UAFSubmissionDto>>> GetUrgentPending()
    {
        var result = await _mediator.Send(new GetUrgentROSPendingQuery());
        return Ok(result);
    }

    /// <summary>
    /// Obtiene estado de cumplimiento PLD por año
    /// </summary>
    [HttpGet("compliance/{year:int}")]
    public async Task<ActionResult<Dictionary<string, object>>> GetCompliance(int year)
    {
        var result = await _mediator.Send(new GetUAFComplianceStatusQuery(year));
        return Ok(result);
    }
}

public record ROSRequest(
    string SubjectName,
    string SubjectIdType,
    string SubjectIdNumber,
    string TransactionType,
    decimal Amount,
    string Currency,
    DateTime TransactionDate,
    string SuspicionIndicators,
    string Narrative,
    bool IsUrgent = true);

public record CTRRequest(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal ThresholdAmount = 500000);
