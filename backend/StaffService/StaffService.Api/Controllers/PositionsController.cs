using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffService.Application.DTOs;
using StaffService.Application.Features.Positions.Commands;
using StaffService.Application.Features.Positions.Queries;
using StaffService.Domain.Entities;

namespace StaffService.Api.Controllers;

[ApiController]
[Route("api/staff/positions")]
[Authorize(Policy = "Staff")]
public class PositionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PositionsController> _logger;

    public PositionsController(IMediator mediator, ILogger<PositionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all positions.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PositionDto>>> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllPositionsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Get positions by department.
    /// </summary>
    [HttpGet("department/{departmentId:guid}")]
    public async Task<ActionResult<IEnumerable<PositionDto>>> GetByDepartment(Guid departmentId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPositionsByDepartmentQuery(departmentId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Get position by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PositionDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPositionByIdQuery(id), ct);
        if (result == null)
            return NotFound(new { message = "Position not found" });
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new position.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<PositionDto>> Create([FromBody] CreatePositionRequest request, CancellationToken ct)
    {
        var command = new CreatePositionCommand(
            request.Title,
            request.Description,
            request.Code,
            request.DepartmentId,
            request.DefaultRole,
            request.Level
        );

        var result = await _mediator.Send(command, ct);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Update position.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<PositionDto>> Update(Guid id, [FromBody] UpdatePositionRequest request, CancellationToken ct)
    {
        var command = new UpdatePositionCommand(
            id,
            request.Title,
            request.Description,
            request.Code,
            request.DepartmentId,
            request.DefaultRole,
            request.Level,
            request.IsActive
        );

        var result = await _mediator.Send(command, ct);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(result.Value);
    }

    /// <summary>
    /// Delete position.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeletePositionCommand(id), ct);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return NoContent();
    }
}

// Request DTOs
public record CreatePositionRequest(
    string Title,
    string? Description,
    string? Code,
    Guid? DepartmentId,
    StaffRole DefaultRole,
    int Level
);

public record UpdatePositionRequest(
    string Title,
    string? Description,
    string? Code,
    Guid? DepartmentId,
    StaffRole DefaultRole,
    int Level,
    bool IsActive
);
