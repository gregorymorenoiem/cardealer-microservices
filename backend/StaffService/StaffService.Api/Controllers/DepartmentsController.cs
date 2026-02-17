using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffService.Application.DTOs;
using StaffService.Application.Features.Departments.Commands;
using StaffService.Application.Features.Departments.Queries;

namespace StaffService.Api.Controllers;

[ApiController]
[Route("api/staff/departments")]
[Authorize(Policy = "Staff")]
public class DepartmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(IMediator mediator, ILogger<DepartmentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all departments.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllDepartmentsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Get department hierarchy tree.
    /// </summary>
    [HttpGet("tree")]
    public async Task<ActionResult<IEnumerable<DepartmentTreeDto>>> GetTree(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDepartmentTreeQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Get department by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DepartmentDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDepartmentByIdQuery(id), ct);
        if (result == null)
            return NotFound(new { message = "Department not found" });
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new department.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<DepartmentDto>> Create([FromBody] CreateDepartmentRequest request, CancellationToken ct)
    {
        var command = new CreateDepartmentCommand(
            request.Name,
            request.Description,
            request.Code,
            request.ParentDepartmentId,
            request.HeadId
        );

        var result = await _mediator.Send(command, ct);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Update department.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<DepartmentDto>> Update(Guid id, [FromBody] UpdateDepartmentRequest request, CancellationToken ct)
    {
        var command = new UpdateDepartmentCommand(
            id,
            request.Name,
            request.Description,
            request.Code,
            request.ParentDepartmentId,
            request.HeadId,
            request.IsActive
        );

        var result = await _mediator.Send(command, ct);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(result.Value);
    }

    /// <summary>
    /// Delete department.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteDepartmentCommand(id), ct);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return NoContent();
    }
}

// Request DTOs
public record CreateDepartmentRequest(
    string Name,
    string? Description,
    string? Code,
    Guid? ParentDepartmentId,
    Guid? HeadId
);

public record UpdateDepartmentRequest(
    string Name,
    string? Description,
    string? Code,
    Guid? ParentDepartmentId,
    Guid? HeadId,
    bool IsActive
);
