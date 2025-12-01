using ConfigurationService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConfigurationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeatureFlagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeatureFlagsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{key}/enabled")]
    public async Task<IActionResult> IsEnabled(
        string key,
        [FromQuery] string? environment = null,
        [FromQuery] string? tenantId = null,
        [FromQuery] string? userId = null)
    {
        var query = new IsFeatureEnabledQuery(key, environment, tenantId, userId);
        var result = await _mediator.Send(query);
        return Ok(new { key, isEnabled = result });
    }
}
