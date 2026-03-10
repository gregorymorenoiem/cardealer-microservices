using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PricingAgent.Application.DTOs;
using PricingAgent.Application.Features.Pricing.Queries;

namespace PricingAgent.Api.Controllers;

[ApiController]
[Route("api/pricing-agent")]
public sealed class PricingAgentController : ControllerBase
{
    private readonly IMediator _mediator;

    public PricingAgentController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Analyze a vehicle's fair market price using LLM + market data.
    /// </summary>
    [HttpPost("analyze")]
    [Authorize]
    public async Task<ActionResult<PricingAnalysisResponse>> AnalyzePrice(
        [FromBody] PricingAnalysisRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new AnalyzeVehiclePriceQuery(request), ct);
        return Ok(result);
    }

    /// <summary>
    /// Quick price check without full analysis (cached, faster).
    /// </summary>
    [HttpGet("quick-check")]
    [AllowAnonymous]
    public async Task<ActionResult<PricingAnalysisResponse>> QuickCheck(
        [FromQuery] string make,
        [FromQuery] string model,
        [FromQuery] int year,
        [FromQuery] int? mileage,
        [FromQuery] string? condition,
        CancellationToken ct)
    {
        var request = new PricingAnalysisRequest
        {
            Make = make,
            Model = model,
            Year = year,
            Mileage = mileage,
            Condition = condition
        };

        var result = await _mediator.Send(new AnalyzeVehiclePriceQuery(request), ct);
        return Ok(result);
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health() => Ok(new { status = "healthy", agent = "PricingAgent", version = "1.0.0" });
}
