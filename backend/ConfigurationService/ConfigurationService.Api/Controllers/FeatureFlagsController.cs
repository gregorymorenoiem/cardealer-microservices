using ConfigurationService.Application.Commands;
using ConfigurationService.Application.Queries;
using ConfigurationService.Domain.Entities;
using ConfigurationService.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigurationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class FeatureFlagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeatureFlagsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous] // Allow services to check feature flags
    public async Task<IActionResult> GetAll([FromQuery] string? environment = null)
    {
        var query = new GetAllFeatureFlagsQuery(environment);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeatureFlagCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFeatureFlagCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteFeatureFlagCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("{key}/enabled")]
    [AllowAnonymous] // Allow services to check feature flags
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
