using ConfigurationService.Application.Queries;
using ConfigurationService.Domain.Entities;
using ConfigurationService.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConfigurationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeatureFlagsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFeatureFlagManager _featureFlagManager;

    public FeatureFlagsController(IMediator mediator, IFeatureFlagManager featureFlagManager)
    {
        _mediator = mediator;
        _featureFlagManager = featureFlagManager;
    }

    [HttpPost]
    public async Task<IActionResult> CreateFeatureFlag([FromBody] FeatureFlag featureFlag)
    {
        var result = await _featureFlagManager.CreateFeatureFlagAsync(featureFlag);
        return Ok(result);
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
