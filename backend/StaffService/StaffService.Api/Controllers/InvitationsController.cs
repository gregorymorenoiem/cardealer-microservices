using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StaffService.Application.DTOs;
using StaffService.Application.Features.Invitations.Commands;
using StaffService.Application.Features.Invitations.Queries;
using StaffService.Domain.Entities;

namespace StaffService.Api.Controllers;

[ApiController]
[Route("api/staff/invitations")]
public class InvitationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<InvitationsController> _logger;

    public InvitationsController(IMediator mediator, ILogger<InvitationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all invitations with filtering.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<PaginatedResponse<InvitationDto>>> GetAll(
        [FromQuery] InvitationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var query = new SearchInvitationsQuery(status, page, pageSize);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get invitation by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<InvitationDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetInvitationByIdQuery(id), ct);
        if (result == null)
            return NotFound(new { message = "Invitation not found" });
        
        return Ok(result);
    }

    /// <summary>
    /// Validate invitation token (public - no auth required).
    /// Used when a person clicks the invitation link.
    /// </summary>
    [HttpGet("validate/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<InvitationValidationDto>> Validate(string token, CancellationToken ct)
    {
        var result = await _mediator.Send(new ValidateInvitationTokenQuery(token), ct);
        if (result == null)
            return NotFound(new { message = "Invalid or expired invitation" });
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new staff invitation.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<InvitationDto>> Create([FromBody] CreateInvitationRequest request, CancellationToken ct)
    {
        // Get current user ID from claims
        // AuthService uses the standard .NET NameIdentifier claim type
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
            ?? User.FindFirst("sub") 
            ?? User.FindFirst("userId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var invitedBy))
        {
            return Unauthorized(new { message = "Unable to determine current user" });
        }

        var command = new CreateInvitationCommand(
            request.Email,
            request.Role,
            request.DepartmentId,
            request.PositionId,
            request.SupervisorId,
            request.Message,
            invitedBy
        );

        var result = await _mediator.Send(command, ct);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Accept an invitation and create staff account (public - no auth required).
    /// </summary>
    [HttpPost("accept")]
    [AllowAnonymous]
    public async Task<ActionResult<StaffDto>> Accept([FromBody] AcceptInvitationRequest request, CancellationToken ct)
    {
        var command = new AcceptInvitationCommand(
            request.Token,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber
        );

        var result = await _mediator.Send(command, ct);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(new
        {
            message = "Invitation accepted successfully",
            staff = result.Value
        });
    }

    /// <summary>
    /// Resend invitation email.
    /// </summary>
    [HttpPost("{id:guid}/resend")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult> Resend(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ResendInvitationCommand(id), ct);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(new { message = "Invitation resent successfully" });
    }

    /// <summary>
    /// Revoke a pending invitation.
    /// </summary>
    [HttpPost("{id:guid}/revoke")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult> Revoke(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new RevokeInvitationCommand(id), ct);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });
        
        return Ok(new { message = "Invitation revoked successfully" });
    }

    /// <summary>
    /// Get pending invitations count.
    /// </summary>
    [HttpGet("pending/count")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<int>> GetPendingCount(CancellationToken ct)
    {
        var count = await _mediator.Send(new GetPendingInvitationsCountQuery(), ct);
        return Ok(new { count });
    }
}

// Request DTOs
public record CreateInvitationRequest(
    string Email,
    StaffRole Role,
    Guid? DepartmentId,
    Guid? PositionId,
    Guid? SupervisorId,
    string? Message
);

public record AcceptInvitationRequest(
    string Token,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber
);
