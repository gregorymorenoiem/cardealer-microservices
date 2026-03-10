using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModerationAgent.Application.DTOs;
using ModerationAgent.Application.Features.Moderation.Commands;

namespace ModerationAgent.Api.Controllers;

[ApiController]
[Route("api/moderation-agent")]
public sealed class ModerationAgentController : ControllerBase
{
    private readonly IMediator _mediator;
    public ModerationAgentController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Moderate content (listing, review, message, profile) for policy violations.
    /// </summary>
    [HttpPost("moderate")]
    [Authorize(Policy = "InternalService")]
    public async Task<ActionResult<ModerationResponse>> Moderate(
        [FromBody] ModerationRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ModerateContentCommand(request), ct);
        return Ok(result);
    }

    /// <summary>
    /// Batch moderate multiple items.
    /// </summary>
    [HttpPost("moderate/batch")]
    [Authorize(Policy = "InternalService")]
    public async Task<ActionResult<IEnumerable<ModerationResponse>>> ModerateBatch(
        [FromBody] IEnumerable<ModerationRequest> requests, CancellationToken ct)
    {
        var tasks = requests.Select(r => _mediator.Send(new ModerateContentCommand(r), ct));
        var results = await Task.WhenAll(tasks);
        return Ok(results);
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health() => Ok(new { status = "healthy", agent = "ModerationAgent", version = "1.0.0" });
}
