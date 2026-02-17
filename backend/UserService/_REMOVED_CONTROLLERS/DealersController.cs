using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using UserService.Application.DTOs;
using UserService.Application.UseCases.Dealers.CreateDealer;
using UserService.Application.UseCases.Dealers.GetDealer;
using UserService.Application.UseCases.Dealers.UpdateDealer;
using UserService.Application.UseCases.Dealers.VerifyDealer;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller for managing dealers (dealerships/concesionarios)
/// Requires authentication for write operations (OWASP A01:2021)
/// Read operations are public for marketplace visibility
/// </summary>
[ApiController]
[Route("api/dealers")]
[Authorize]
public class DealersController : ControllerBase
{
    private readonly IMediator _mediator;

    public DealersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new dealer
    /// </summary>
    /// <param name="request">Dealer creation data</param>
    /// <returns>The created dealer</returns>
    [HttpPost]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DealerDto>> CreateDealer([FromBody] CreateDealerRequest request)
    {
        try
        {
            var command = new CreateDealerCommand(request);
            var result = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetDealer),
                new { dealerId = result.Id },
                result
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get a dealer by ID
    /// Public endpoint for marketplace dealer profile viewing
    /// </summary>
    /// <param name="dealerId">The dealer ID</param>
    /// <returns>The dealer details</returns>
    [HttpGet("{dealerId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DealerDto>> GetDealer(Guid dealerId)
    {
        try
        {
            var query = new GetDealerQuery(dealerId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Shared.Exceptions.NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get dealer by owner user ID
    /// </summary>
    /// <param name="ownerUserId">The owner user ID</param>
    /// <returns>The dealer details</returns>
    [HttpGet("owner/{ownerUserId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DealerDto>> GetDealerByOwner(Guid ownerUserId)
    {
        var query = new GetDealerByOwnerQuery(ownerUserId);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { error = $"No dealer found for owner {ownerUserId}" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Update a dealer
    /// </summary>
    /// <param name="dealerId">The dealer ID</param>
    /// <param name="request">Updated dealer data</param>
    /// <returns>The updated dealer</returns>
    [HttpPut("{dealerId:guid}")]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DealerDto>> UpdateDealer(
        Guid dealerId,
        [FromBody] UpdateDealerRequest request)
    {
        try
        {
            var command = new UpdateDealerCommand(dealerId, request);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Shared.Exceptions.NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Verify a dealer (admin only)
    /// Requires Admin or SuperAdmin role
    /// </summary>
    /// <param name="dealerId">The dealer ID</param>
    /// <param name="request">Verification data</param>
    /// <returns>Updated dealer</returns>
    [HttpPost("{dealerId:guid}/verify")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DealerDto>> VerifyDealer(
        Guid dealerId,
        [FromBody] VerifyDealerRequest request)
    {
        try
        {
            var command = new VerifyDealerCommand(dealerId, request);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Shared.Exceptions.NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate a dealer (soft delete)
    /// Requires Admin or SuperAdmin role
    /// </summary>
    /// <param name="dealerId">The dealer ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{dealerId:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeactivateDealer(Guid dealerId)
    {
        try
        {
            var request = new UpdateDealerRequest { IsActive = false };
            var command = new UpdateDealerCommand(dealerId, request);
            await _mediator.Send(command);
            return NoContent();
        }
        catch (Shared.Exceptions.NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
