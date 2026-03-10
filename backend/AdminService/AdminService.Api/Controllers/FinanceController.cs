using AdminService.Application.UseCases.Finance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Api.Controllers;

/// <summary>
/// Financial dashboard controller for OKLA admin internal view.
///
/// CONTRA #5 FIX: Unified financial dashboard showing expenses by category,
/// revenue by plan/source, real-time net margin, and runway projection.
///
/// Endpoints:
///   GET /api/admin/finance/dashboard        → Full financial dashboard
///   GET /api/admin/finance/dashboard?period=2026-01  → Specific month
/// </summary>
[ApiController]
[Route("api/admin/finance")]
[Authorize(Roles = "admin,superadmin")]
public class FinanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public FinanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get the full financial dashboard with expenses, revenue, margin, and runway.
    /// </summary>
    /// <param name="period">Optional period in YYYY-MM format. Defaults to current month.</param>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(FinancialDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<FinancialDashboardDto>> GetFinancialDashboard(
        [FromQuery] string? period = null)
    {
        var result = await _mediator.Send(new GetFinancialDashboardQuery(period));
        return Ok(result);
    }
}
