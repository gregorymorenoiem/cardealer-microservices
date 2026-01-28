using DealerManagementService.Application.DTOs;
using DealerManagementService.Application.Features.Locations.Commands;
using DealerManagementService.Application.Features.Locations.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommandUpdateRequest = DealerManagementService.Application.Features.Locations.Commands.UpdateLocationRequest;

namespace DealerManagementService.Api.Controllers;

/// <summary>
/// Manage dealer locations (branches, showrooms, etc.)
/// </summary>
[ApiController]
[Route("api/dealers/{dealerId:guid}/[controller]")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LocationsController> _logger;

    public LocationsController(IMediator mediator, ILogger<LocationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all locations for a dealer
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<DealerLocationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DealerLocationDto>>> GetLocations(Guid dealerId)
    {
        var query = new GetDealerLocationsQuery(dealerId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific location by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DealerLocationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DealerLocationDto>> GetLocation(Guid dealerId, Guid id)
    {
        var query = new GetLocationByIdQuery(dealerId, id);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound(new { message = $"Location {id} not found" });
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new location
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DealerLocationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DealerLocationDto>> CreateLocation(
        Guid dealerId,
        [FromBody] CreateLocationRequest request)
    {
        try
        {
            var command = new CreateLocationCommand(dealerId, request);
            var result = await _mediator.Send(command);
            
            return CreatedAtAction(
                nameof(GetLocation),
                new { dealerId, id = result.Id },
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating location for dealer {DealerId}", dealerId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update a location
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DealerLocationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DealerLocationDto>> UpdateLocation(
        Guid dealerId,
        Guid id,
        [FromBody] CommandUpdateRequest request)
    {
        try
        {
            var command = new UpdateLocationCommand(dealerId, id, request);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating location {LocationId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a location
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteLocation(Guid dealerId, Guid id)
    {
        try
        {
            var command = new DeleteLocationCommand(dealerId, id);
            await _mediator.Send(command);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Set a location as primary
    /// </summary>
    [HttpPost("{id:guid}/set-primary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetPrimaryLocation(Guid dealerId, Guid id)
    {
        try
        {
            var command = new SetPrimaryLocationCommand(dealerId, id);
            await _mediator.Send(command);
            return Ok(new { message = "Location set as primary" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
