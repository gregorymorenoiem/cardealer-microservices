using BankReconciliationService.Application.DTOs;
using BankReconciliationService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankReconciliationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DiscrepanciesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DiscrepanciesController> _logger;

    public DiscrepanciesController(IMediator mediator, ILogger<DiscrepanciesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get discrepancies with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReconciliationDiscrepancyDto>>> GetDiscrepancies(
        CancellationToken cancellationToken,
        [FromQuery] Guid? reconciliationId = null,
        [FromQuery] DiscrepancyStatus? status = null)
    {
        _logger.LogInformation("Getting discrepancies with filters");
        
        // TODO: Implement GetDiscrepanciesQuery
        await Task.CompletedTask;
        return Ok(Array.Empty<ReconciliationDiscrepancyDto>());
    }

    /// <summary>
    /// Get a specific discrepancy by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReconciliationDiscrepancyDto>> GetDiscrepancy(
        Guid id, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting discrepancy {DiscrepancyId}", id);
        
        // TODO: Implement GetDiscrepancyByIdQuery
        await Task.CompletedTask;
        return NotFound();
    }

    /// <summary>
    /// Resolve a discrepancy
    /// </summary>
    [HttpPost("{id:guid}/resolve")]
    [Authorize(Roles = "Admin,Accountant")]
    public async Task<ActionResult<ReconciliationDiscrepancyDto>> ResolveDiscrepancy(
        Guid id,
        [FromBody] ResolveDiscrepancyRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Resolving discrepancy {DiscrepancyId}", id);
        
        // TODO: Implement ResolveDiscrepancyCommand
        await Task.CompletedTask;
        return NotFound();
    }

    /// <summary>
    /// Mark a discrepancy as ignored
    /// </summary>
    [HttpPost("{id:guid}/ignore")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<ActionResult<ReconciliationDiscrepancyDto>> IgnoreDiscrepancy(
        Guid id,
        [FromBody] string? reason,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ignoring discrepancy {DiscrepancyId}", id);
        
        // TODO: Implement IgnoreDiscrepancyCommand
        await Task.CompletedTask;
        return NotFound();
    }

    /// <summary>
    /// Get discrepancies summary
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<DiscrepanciesSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting discrepancies summary");
        
        // TODO: Implement GetDiscrepanciesSummaryQuery
        await Task.CompletedTask;
        return Ok(new DiscrepanciesSummaryDto(0, 0, 0, 0, 0m, 0m));
    }
}
