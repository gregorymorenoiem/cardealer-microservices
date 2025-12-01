using ConfigurationService.Application.Commands;
using ConfigurationService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConfigurationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigurationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConfigurationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key, [FromQuery] string environment, [FromQuery] string? tenantId = null)
    {
        var query = new GetConfigurationQuery(key, environment, tenantId);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string environment, [FromQuery] string? tenantId = null)
    {
        var query = new GetAllConfigurationsQuery(environment, tenantId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConfigurationCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { key = result.Key, environment = result.Environment }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateConfigurationCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteConfigurationCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }
}
