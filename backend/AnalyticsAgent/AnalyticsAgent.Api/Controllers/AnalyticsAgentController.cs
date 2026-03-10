using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AnalyticsAgent.Application.DTOs;
using AnalyticsAgent.Application.Features.Analytics.Queries;

namespace AnalyticsAgent.Api.Controllers;

[ApiController]
[Route("api/analytics-agent")]
public sealed class AnalyticsAgentController : ControllerBase
{
    private readonly IMediator _mediator;
    public AnalyticsAgentController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Generate LLM-powered analytics insights from raw metrics.
    /// </summary>
    [HttpPost("insights")]
    [Authorize]
    public async Task<ActionResult<AnalyticsInsightResponse>> GenerateInsights(
        [FromBody] AnalyticsInsightRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateInsightsQuery(request), ct);
        return Ok(result);
    }

    /// <summary>
    /// Quick daily summary for admin dashboard.
    /// </summary>
    [HttpGet("daily-summary")]
    [Authorize(Roles = "Admin,CTO")]
    public async Task<ActionResult<AnalyticsInsightResponse>> DailySummary(CancellationToken ct)
    {
        var request = new AnalyticsInsightRequest
        {
            ReportType = "daily_summary",
            Period = "today",
            Metrics = new Dictionary<string, object>
            {
                ["note"] = "Fetch real metrics from DealerAnalyticsService in production"
            }
        };
        var result = await _mediator.Send(new GenerateInsightsQuery(request), ct);
        return Ok(result);
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health() => Ok(new { status = "healthy", agent = "AnalyticsAgent", version = "1.0.0" });
}
