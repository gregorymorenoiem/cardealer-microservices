using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminService.Application.UseCases.Dealers;

namespace AdminService.Api.Controllers;

/// <summary>
/// Controller for admin management of dealers.
/// Routes: /api/admin/dealers/*
/// </summary>
[ApiController]
[Route("api/admin/dealers")]
[Produces("application/json")]
public class DealersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DealersController> _logger;

    public DealersController(IMediator mediator, ILogger<DealersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get dealers with filtering and pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(PaginatedDealerResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedDealerResult>> GetDealers(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? plan = null,
        [FromQuery] bool? verified = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation(
            "Getting dealers - search={Search}, status={Status}, plan={Plan}, page={Page}",
            search, status, plan, page);

        var result = await _mediator.Send(
            new GetDealersQuery(search, status, plan, verified, page, pageSize));

        return Ok(result);
    }

    /// <summary>
    /// Get dealer statistics (total, active, pending, suspended, MRR, by plan)
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(DealerStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DealerStatsDto>> GetDealerStats()
    {
        _logger.LogInformation("Getting dealer statistics");

        var result = await _mediator.Send(new GetDealerStatsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Get dealer by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(AdminDealerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminDealerDto>> GetDealer(Guid id)
    {
        _logger.LogInformation("Getting dealer {DealerId}", id);

        var result = await _mediator.Send(new GetDealerByIdQuery(id));

        if (result == null)
            return NotFound(new { error = "Dealer not found" });

        return Ok(result);
    }

    /// <summary>
    /// Verify a dealer (change status to active + verified)
    /// </summary>
    [HttpPost("{id}/verify")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> VerifyDealer(Guid id)
    {
        _logger.LogInformation("Verifying dealer {DealerId}", id);

        await _mediator.Send(new VerifyDealerCommand(id));
        return NoContent();
    }

    /// <summary>
    /// Suspend a dealer
    /// </summary>
    [HttpPost("{id}/suspend")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SuspendDealer(Guid id, [FromBody] SuspendDealerRequest? request = null)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { Error = "A suspension reason is required" });

        _logger.LogInformation("Suspending dealer {DealerId} for reason: {Reason}", id, request.Reason);

        await _mediator.Send(new SuspendDealerCommand(id, request.Reason));
        return NoContent();
    }

    /// <summary>
    /// Reactivate a suspended dealer
    /// </summary>
    [HttpPost("{id}/reactivate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ReactivateDealer(Guid id)
    {
        _logger.LogInformation("Reactivating dealer {DealerId}", id);

        await _mediator.Send(new ReactivateDealerCommand(id));
        return NoContent();
    }

    /// <summary>
    /// Delete a dealer
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteDealer(Guid id)
    {
        _logger.LogInformation("Deleting dealer {DealerId}", id);

        await _mediator.Send(new DeleteDealerCommand(id));
        return NoContent();
    }
}

// ============================================================================
// Request DTOs
// ============================================================================

public class SuspendDealerRequest
{
    public string Reason { get; set; } = string.Empty;
}
