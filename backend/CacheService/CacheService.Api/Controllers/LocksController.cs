using Microsoft.AspNetCore.Mvc;
using MediatR;
using CacheService.Application.Commands;

namespace CacheService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocksController : ControllerBase
{
    private readonly IMediator _mediator;

    public LocksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Acquires a distributed lock
    /// </summary>
    [HttpPost("acquire")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AcquireLock([FromBody] AcquireLockRequest request)
    {
        var command = new AcquireLockCommand(
            request.Key,
            request.OwnerId,
            request.TtlSeconds
        );

        var result = await _mediator.Send(command);

        if (result == null)
            return Conflict(new { message = $"Lock '{request.Key}' is already acquired" });

        return Ok(new
        {
            message = "Lock acquired successfully",
            @lock = result
        });
    }

    /// <summary>
    /// Releases a distributed lock
    /// </summary>
    [HttpPost("release")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReleaseLock([FromBody] ReleaseLockRequest request)
    {
        var command = new ReleaseLockCommand(request.Key, request.OwnerId);
        var result = await _mediator.Send(command);

        if (!result)
            return BadRequest(new { message = $"Failed to release lock '{request.Key}'. Lock not found or not owned by '{request.OwnerId}'" });

        return Ok(new { message = "Lock released successfully", key = request.Key });
    }
}

public record AcquireLockRequest(
    string Key,
    string OwnerId,
    int TtlSeconds = 30
);

public record ReleaseLockRequest(
    string Key,
    string OwnerId
);
