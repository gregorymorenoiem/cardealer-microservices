using Microsoft.AspNetCore.Mvc;
using MediatR;
using CacheService.Application.Commands;
using CacheService.Application.Queries;

namespace CacheService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CacheController : ControllerBase
{
    private readonly IMediator _mediator;

    public CacheController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a cached value by key
    /// </summary>
    [HttpGet("{key}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string key, [FromQuery] string? tenantId = null)
    {
        var query = new GetCacheQuery(key, tenantId);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"Key '{key}' not found in cache" });

        return Ok(new { key, value = result, tenantId });
    }

    /// <summary>
    /// Sets a value in cache
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Set([FromBody] SetCacheRequest request)
    {
        var command = new SetCacheCommand(
            request.Key,
            request.Value,
            request.TenantId,
            request.TtlSeconds
        );

        var result = await _mediator.Send(command);

        if (!result)
            return BadRequest(new { message = "Failed to set cache value" });

        return Ok(new { message = "Cache value set successfully", key = request.Key });
    }

    /// <summary>
    /// Deletes a value from cache
    /// </summary>
    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string key)
    {
        var command = new DeleteCacheCommand(key);
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound(new { message = $"Key '{key}' not found in cache" });

        return Ok(new { message = "Cache value deleted successfully", key });
    }

    /// <summary>
    /// Flushes all cache data
    /// </summary>
    [HttpDelete("flush")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Flush()
    {
        var command = new FlushCacheCommand();
        await _mediator.Send(command);

        return Ok(new { message = "Cache flushed successfully" });
    }
}

public record SetCacheRequest(
    string Key,
    string Value,
    string? TenantId = null,
    int? TtlSeconds = null
);
