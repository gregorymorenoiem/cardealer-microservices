using BillingService.Application.DTOs;
using BillingService.Application.UseCases.Reconciliation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

// ═══════════════════════════════════════════════════════════════════════════════
// RECONCILIATION CONTROLLER — CONTRA #6 FIX
//
// REST API for payment reconciliation audit.
//
// Endpoints:
//   POST /api/billing/reconciliation/trigger     — Run reconciliation (admin only)
//   GET  /api/billing/reconciliation/latest       — Get latest report
//   GET  /api/billing/reconciliation/history      — Get report history
//   GET  /api/billing/reconciliation/discrepancies — Get unresolved discrepancies
//   POST /api/billing/reconciliation/discrepancies/{id}/resolve — Resolve discrepancy
//   POST /api/billing/reconciliation/dealer/{dealerId} — Reconcile single dealer
// ═══════════════════════════════════════════════════════════════════════════════

[ApiController]
[Route("api/billing/reconciliation")]
[Authorize(Roles = "admin,superadmin")]
public class ReconciliationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReconciliationController> _logger;

    public ReconciliationController(IMediator mediator, ILogger<ReconciliationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Trigger a manual reconciliation run.
    /// </summary>
    [HttpPost("trigger")]
    [ProducesResponseType(typeof(ReconciliationReportDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TriggerReconciliation(
        [FromBody] TriggerReconciliationRequest? request,
        CancellationToken ct)
    {
        try
        {
            var userEmail = User.FindFirst("email")?.Value ?? "admin";

            var command = new TriggerReconciliationCommand(
                Period: request?.Period,
                AutoResolve: request?.AutoResolve ?? true,
                TriggeredBy: userEmail);

            var result = await _mediator.Send(command, ct);

            _logger.LogInformation(
                "[Reconciliation API] Manual reconciliation triggered by {User} for period={Period}: {Discrepancies} discrepancies",
                userEmail, request?.Period ?? "current", result.DiscrepancyCount);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Reconciliation API] Failed to trigger reconciliation");
            return StatusCode(500, new { error = "Reconciliation failed", message = ex.Message });
        }
    }

    /// <summary>
    /// Get the latest reconciliation report for a period.
    /// </summary>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(ReconciliationReportDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatestReport([FromQuery] string? period, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetReconciliationReportQuery(period), ct);

        if (result == null)
            return NotFound(new { message = $"No reconciliation report found for period {period ?? "current"}" });

        return Ok(result);
    }

    /// <summary>
    /// Get reconciliation report history.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<ReconciliationReportSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory([FromQuery] int limit = 30, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetReconciliationHistoryQuery(limit), ct);
        return Ok(result);
    }

    /// <summary>
    /// Get all unresolved discrepancies across all reports.
    /// </summary>
    [HttpGet("discrepancies")]
    [ProducesResponseType(typeof(List<ReconciliationDiscrepancyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnresolvedDiscrepancies(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUnresolvedDiscrepanciesQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Resolve a specific discrepancy manually.
    /// </summary>
    [HttpPost("discrepancies/{id:guid}/resolve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveDiscrepancy(
        Guid id,
        [FromBody] ResolveDiscrepancyRequest request,
        CancellationToken ct)
    {
        var userEmail = User.FindFirst("email")?.Value ?? "admin";

        var command = new ResolveDiscrepancyCommand(id, userEmail, request.Notes);
        var result = await _mediator.Send(command, ct);

        if (!result)
            return NotFound(new { message = $"Discrepancy {id} not found" });

        return Ok(new { message = "Discrepancy resolved", id, resolvedBy = userEmail });
    }

    /// <summary>
    /// Reconcile a single dealer's payments.
    /// </summary>
    [HttpPost("dealer/{dealerId:guid}")]
    [ProducesResponseType(typeof(List<ReconciliationDiscrepancyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReconcileDealer(
        Guid dealerId,
        [FromQuery] string? period,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new ReconcileDealerCommand(dealerId, period), ct);
        return Ok(new
        {
            dealerId,
            period = period ?? "current",
            discrepancyCount = result.Count,
            discrepancies = result,
        });
    }
}
