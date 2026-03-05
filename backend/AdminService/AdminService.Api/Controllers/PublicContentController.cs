using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminService.Application.UseCases.Content;

namespace AdminService.Api.Controllers;

/// <summary>
/// Public (no-auth) banner endpoint — consumed by the Next.js frontend
/// to display configured banners on public-facing pages like /vehiculos.
/// </summary>
[ApiController]
[Route("api/content")]
[Produces("application/json")]
[AllowAnonymous]
public class PublicContentController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicContentController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get active banners for a specific placement.
    /// Query: ?placement=search_leaderboard
    /// Called by the /vehiculos page to render configurable ad slots.
    /// </summary>
    [HttpGet("banners")]
    public async Task<IActionResult> GetPublicBanners([FromQuery] string placement = "search_leaderboard")
    {
        var result = await _mediator.Send(new GetPublicBannersQuery(placement));
        return Ok(result);
    }

    /// <summary>Record a banner click (called when user clicks an ad on the public site)</summary>
    [HttpPost("banners/{id}/click")]
    public async Task<IActionResult> RecordClick(string id)
    {
        await _mediator.Send(new RecordBannerClickCommand(id));
        return Ok();
    }

    /// <summary>Record a banner view impression</summary>
    [HttpPost("banners/{id}/view")]
    public async Task<IActionResult> RecordView(string id)
    {
        await _mediator.Send(new RecordBannerViewCommand(id));
        return Ok();
    }
}
