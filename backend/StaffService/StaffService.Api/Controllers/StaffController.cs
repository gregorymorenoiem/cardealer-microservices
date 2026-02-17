using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffService.Application.DTOs;
using StaffService.Application.Features.Staff.Commands;
using StaffService.Application.Features.Staff.Queries;
using StaffService.Domain.Entities;

namespace StaffService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Staff")]
public class StaffController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StaffController> _logger;

    public StaffController(IMediator mediator, ILogger<StaffController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all staff members with filtering and pagination.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<StaffListItemDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] StaffStatus? status,
        [FromQuery] StaffRole? role,
        [FromQuery] Guid? departmentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var query = new SearchStaffQuery(search, status, role, departmentId, page, pageSize);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get staff member by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StaffDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStaffByIdQuery(id), ct);
        if (result == null)
            return NotFound(new { message = "Staff member not found" });
        
        return Ok(result);
    }

    /// <summary>
    /// Get staff member by AuthService user ID.
    /// </summary>
    [HttpGet("auth/{authUserId:guid}")]
    public async Task<ActionResult<StaffDto>> GetByAuthUserId(Guid authUserId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStaffByAuthUserIdQuery(authUserId), ct);
        if (result == null)
            return NotFound(new { message = "Staff member not found" });
        
        return Ok(result);
    }

    /// <summary>
    /// Get staff summary/statistics.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<StaffSummaryDto>> GetSummary(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStaffSummaryQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Get direct reports for a supervisor.
    /// </summary>
    [HttpGet("{id:guid}/direct-reports")]
    public async Task<ActionResult<IEnumerable<StaffListItemDto>>> GetDirectReports(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDirectReportsQuery(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Update staff member details.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<StaffDto>> Update(Guid id, [FromBody] UpdateStaffRequest request, CancellationToken ct)
    {
        var command = new UpdateStaffCommand(
            id,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.EmployeeCode,
            request.DepartmentId,
            request.PositionId,
            request.SupervisorId,
            request.Role,
            request.Notes
        );

        var result = await _mediator.Send(command, ct);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(result.Value);
    }

    /// <summary>
    /// Change staff status (active, suspended, on leave).
    /// </summary>
    [HttpPost("{id:guid}/status")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request, CancellationToken ct)
    {
        var command = new ChangeStaffStatusCommand(id, request.Status, request.Reason);
        var result = await _mediator.Send(command, ct);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(new { message = "Status updated successfully" });
    }

    /// <summary>
    /// Terminate a staff member.
    /// </summary>
    [HttpPost("{id:guid}/terminate")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<ActionResult> Terminate(Guid id, [FromBody] TerminateRequest request, CancellationToken ct)
    {
        var command = new TerminateStaffCommand(id, request.Reason);
        var result = await _mediator.Send(command, ct);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(new { message = "Staff member terminated successfully" });
    }

    /// <summary>
    /// Delete staff member (hard delete - SuperAdmin only).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var command = new DeleteStaffCommand(id);
        var result = await _mediator.Send(command, ct);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return NoContent();
    }
}

// Request DTOs
public record UpdateStaffRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? EmployeeCode,
    Guid? DepartmentId,
    Guid? PositionId,
    Guid? SupervisorId,
    StaffRole? Role,
    string? Notes
);

public record ChangeStatusRequest(StaffStatus Status, string? Reason);
public record TerminateRequest(string Reason);
