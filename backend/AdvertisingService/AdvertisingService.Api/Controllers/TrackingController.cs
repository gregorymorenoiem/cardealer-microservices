using System.Threading.RateLimiting;
using AdvertisingService.Application.Features.Tracking.Commands.RecordClick;
using AdvertisingService.Application.Features.Tracking.Commands.RecordImpression;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AdvertisingService.Api.Controllers;

[ApiController]
[Route("api/advertising/tracking")]
[EnableRateLimiting("tracking")]
public class TrackingController : ControllerBase
{
    private readonly IMediator _mediator;

    public TrackingController(IMediator mediator) => _mediator = mediator;

    /// <summary>Record an ad impression (no auth required, rate-limited)</summary>
    [HttpPost("impression")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> RecordImpression([FromBody] RecordImpressionCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return Ok();
    }

    /// <summary>Record an ad click (no auth required, rate-limited)</summary>
    [HttpPost("click")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> RecordClick([FromBody] RecordClickCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return Ok();
    }
}
