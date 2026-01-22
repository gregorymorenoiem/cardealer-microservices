// ReportingService - DGII Controller
// Gestión de reportes DGII (Formatos 606, 607, 608, 609, IT1)

namespace ReportingService.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportingService.Application.DTOs;
using ReportingService.Application.Features.Commands;
using ReportingService.Application.Features.Queries;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Compliance")]
public class DGIIController : ControllerBase
{
    private readonly IMediator _mediator;

    public DGIIController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Genera formato 606 (Compras de bienes y servicios)
    /// </summary>
    [HttpPost("606")]
    public async Task<ActionResult<DGIISubmissionDto>> Generate606([FromBody] DGII606Request request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "system";
        var result = await _mediator.Send(new GenerateDGII606Command(
            request.PeriodStart, request.PeriodEnd, request.RNC,
            request.AutoSubmit, userId));
        return Created($"/api/dgii/submissions/{result.Id}", result);
    }

    /// <summary>
    /// Genera formato 607 (Ventas de bienes y servicios)
    /// </summary>
    [HttpPost("607")]
    public async Task<ActionResult<DGIISubmissionDto>> Generate607([FromBody] DGII607Request request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "system";
        var result = await _mediator.Send(new GenerateDGII607Command(
            request.PeriodStart, request.PeriodEnd, request.RNC,
            request.AutoSubmit, userId));
        return Created($"/api/dgii/submissions/{result.Id}", result);
    }

    /// <summary>
    /// Genera formato 608 (Comprobantes anulados)
    /// </summary>
    [HttpPost("608")]
    public async Task<ActionResult<DGIISubmissionDto>> Generate608([FromBody] DGII608Request request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "system";
        var result = await _mediator.Send(new GenerateDGII608Command(
            request.PeriodStart, request.PeriodEnd, request.RNC,
            request.AutoSubmit, userId));
        return Created($"/api/dgii/submissions/{result.Id}", result);
    }

    /// <summary>
    /// Genera formato IT1 (Declaración ITBIS)
    /// </summary>
    [HttpPost("it1")]
    public async Task<ActionResult<DGIISubmissionDto>> GenerateIT1([FromBody] DGIIT1Request request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "system";
        var result = await _mediator.Send(new GenerateDGIIIT1Command(
            request.PeriodStart, request.PeriodEnd, request.RNC,
            request.AutoSubmit, userId));
        return Created($"/api/dgii/submissions/{result.Id}", result);
    }

    /// <summary>
    /// Obtiene una presentación DGII por ID
    /// </summary>
    [HttpGet("submissions/{id:guid}")]
    public async Task<ActionResult<DGIISubmissionDto>> GetSubmission(Guid id)
    {
        var result = await _mediator.Send(new GetDGIISubmissionByIdQuery(id));
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Lista presentaciones DGII por período
    /// </summary>
    [HttpGet("submissions")]
    public async Task<ActionResult<List<DGIISubmissionDto>>> GetByPeriod(
        [FromQuery] string period,
        [FromQuery] string? reportCode = null)
    {
        var result = await _mediator.Send(new GetDGIISubmissionsByPeriodQuery(period, reportCode));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene presentaciones pendientes
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<List<DGIISubmissionDto>>> GetPending()
    {
        var result = await _mediator.Send(new GetPendingDGIISubmissionsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Obtiene estado de cumplimiento por año
    /// </summary>
    [HttpGet("compliance/{rnc}/{year:int}")]
    public async Task<ActionResult<Dictionary<string, string>>> GetCompliance(string rnc, int year)
    {
        var result = await _mediator.Send(new GetDGIIComplianceStatusQuery(rnc, year));
        return Ok(result);
    }
}

public record DGII606Request(DateTime PeriodStart, DateTime PeriodEnd, string RNC, bool AutoSubmit = false);
public record DGII607Request(DateTime PeriodStart, DateTime PeriodEnd, string RNC, bool AutoSubmit = false);
public record DGII608Request(DateTime PeriodStart, DateTime PeriodEnd, string RNC, bool AutoSubmit = false);
public record DGIIT1Request(DateTime PeriodStart, DateTime PeriodEnd, string RNC, bool AutoSubmit = false);
