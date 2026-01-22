// =====================================================
// ComplianceReportingService - Controllers
// Reportes Consolidados de Cumplimiento RD
// =====================================================

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ComplianceReportingService.Application.Commands;
using ComplianceReportingService.Application.Queries;
using ComplianceReportingService.Application.DTOs;
using ComplianceReportingService.Domain.Enums;

namespace ComplianceReportingService.Api.Controllers;

// ==================== Reportes ====================
[ApiController]
[Route("api/compliance-reports")]
public class ComplianceReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ComplianceReportsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetReportByIdQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("number/{reportNumber}")]
    public async Task<IActionResult> GetByNumber(string reportNumber)
    {
        var result = await _mediator.Send(new GetReportByNumberQuery(reportNumber));
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("body/{body}")]
    public async Task<IActionResult> GetByBody(RegulatoryBody body)
    {
        var result = await _mediator.Send(new GetReportsByBodyQuery(body));
        return Ok(result);
    }

    [HttpGet("period/{period}")]
    public async Task<IActionResult> GetByPeriod(string period)
    {
        var result = await _mediator.Send(new GetReportsByPeriodQuery(period));
        return Ok(result);
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(ReportStatus status)
    {
        var result = await _mediator.Send(new GetReportsByStatusQuery(status));
        return Ok(result);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var result = await _mediator.Send(new GetPendingReportsQuery());
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Generate([FromBody] GenerateReportDto dto)
    {
        var result = await _mediator.Send(new GenerateReportCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/validate")]
    [Authorize]
    public async Task<IActionResult> Validate(Guid id)
    {
        var result = await _mediator.Send(new ValidateReportCommand(id));
        return result ? Ok() : BadRequest();
    }

    [HttpPost("submit")]
    [Authorize]
    public async Task<IActionResult> Submit([FromBody] SubmitReportDto dto)
    {
        var result = await _mediator.Send(new SubmitReportCommand(dto));
        return Ok(result);
    }

    [HttpPost("{id:guid}/regenerate")]
    [Authorize]
    public async Task<IActionResult> Regenerate(Guid id)
    {
        var result = await _mediator.Send(new RegenerateReportCommand(id));
        return Ok(result);
    }
}

// ==================== Schedules ====================
[ApiController]
[Route("api/report-schedules")]
[Authorize]
public class ReportSchedulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportSchedulesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetScheduleByIdQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _mediator.Send(new GetActiveSchedulesQuery());
        return Ok(result);
    }

    [HttpGet("due")]
    public async Task<IActionResult> GetDue()
    {
        var result = await _mediator.Send(new GetDueSchedulesQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateScheduleDto dto)
    {
        var result = await _mediator.Send(new CreateScheduleCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateScheduleDto dto)
    {
        var result = await _mediator.Send(new UpdateScheduleCommand(id, dto));
        return result ? Ok() : NotFound();
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _mediator.Send(new ActivateScheduleCommand(id));
        return result ? Ok() : NotFound();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _mediator.Send(new DeactivateScheduleCommand(id));
        return result ? Ok() : NotFound();
    }

    [HttpPost("run")]
    public async Task<IActionResult> RunScheduled()
    {
        var result = await _mediator.Send(new RunScheduledReportsCommand());
        return Ok(result);
    }
}

// ==================== Templates ====================
[ApiController]
[Route("api/report-templates")]
[Authorize]
public class ReportTemplatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportTemplatesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTemplateByIdQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive([FromQuery] ReportType type, [FromQuery] RegulatoryBody body)
    {
        var result = await _mediator.Send(new GetActiveTemplateQuery(type, body));
        return result != null ? Ok(result) : NotFound();
    }

    [HttpGet("body/{body}")]
    public async Task<IActionResult> GetByBody(RegulatoryBody body)
    {
        var result = await _mediator.Send(new GetTemplatesByBodyQuery(body));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTemplateDto dto)
    {
        var result = await _mediator.Send(new CreateTemplateCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _mediator.Send(new ActivateTemplateCommand(id));
        return result ? Ok() : NotFound();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _mediator.Send(new DeactivateTemplateCommand(id));
        return result ? Ok() : NotFound();
    }
}

// ==================== Statistics ====================
[ApiController]
[Route("api/reporting-statistics")]
public class ReportingStatisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportingStatisticsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _mediator.Send(new GetReportingStatisticsQuery());
        return Ok(result);
    }
}
