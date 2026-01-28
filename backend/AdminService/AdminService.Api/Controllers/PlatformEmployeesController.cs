using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminService.Application.UseCases.PlatformEmployees;

namespace AdminService.Api.Controllers;

/// <summary>
/// Controller for managing platform employees (admin team members)
/// Only SuperAdmin and Admin can manage platform employees
/// </summary>
[ApiController]
[Route("api/platform/employees")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class PlatformEmployeesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PlatformEmployeesController> _logger;

    public PlatformEmployeesController(IMediator mediator, ILogger<PlatformEmployeesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all platform employees with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<PlatformEmployeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<PlatformEmployeeDto>>> GetEmployees(
        [FromQuery] string? status = null,
        [FromQuery] string? role = null,
        [FromQuery] string? department = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Getting platform employees with filters: status={Status}, role={Role}, department={Department}", 
            status, role, department);

        var result = await _mediator.Send(new GetPlatformEmployeesQuery(status, role, department, page, pageSize));
        return Ok(result);
    }

    /// <summary>
    /// Get a specific platform employee by ID
    /// </summary>
    [HttpGet("{employeeId}")]
    [ProducesResponseType(typeof(PlatformEmployeeDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlatformEmployeeDetailDto>> GetEmployee(Guid employeeId)
    {
        _logger.LogInformation("Getting platform employee {EmployeeId}", employeeId);

        var result = await _mediator.Send(new GetPlatformEmployeeQuery(employeeId));

        if (result == null)
            return NotFound(new { error = "Employee not found" });

        return Ok(result);
    }

    /// <summary>
    /// Invite a new platform employee
    /// </summary>
    [HttpPost("invite")]
    [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can invite platform employees
    [ProducesResponseType(typeof(PlatformEmployeeInvitationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PlatformEmployeeInvitationDto>> InviteEmployee(
        [FromBody] InvitePlatformEmployeeRequest request)
    {
        _logger.LogInformation("Inviting platform employee {Email} with role {Role}", request.Email, request.Role);

        // Get current user ID from claims
        var currentUserId = GetCurrentUserId();

        var command = new InvitePlatformEmployeeCommand(
            Email: request.Email,
            Role: request.Role,
            Permissions: request.Permissions,
            Department: request.Department,
            Notes: request.Notes,
            InvitedBy: currentUserId
        );

        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetEmployee), new { employeeId = result.Id }, result);
    }

    /// <summary>
    /// Update platform employee role and permissions
    /// </summary>
    [HttpPut("{employeeId}")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> UpdateEmployee(
        Guid employeeId,
        [FromBody] UpdatePlatformEmployeeRequest request)
    {
        _logger.LogInformation("Updating platform employee {EmployeeId}", employeeId);

        var command = new UpdatePlatformEmployeeCommand(
            EmployeeId: employeeId,
            Role: request.Role,
            Permissions: request.Permissions,
            Department: request.Department,
            Notes: request.Notes,
            Status: request.Status
        );

        await _mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Suspend a platform employee
    /// </summary>
    [HttpPost("{employeeId}/suspend")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SuspendEmployee(Guid employeeId, [FromBody] SuspendEmployeeRequest? request = null)
    {
        _logger.LogInformation("Suspending platform employee {EmployeeId}", employeeId);

        await _mediator.Send(new SuspendPlatformEmployeeCommand(employeeId, request?.Reason));

        return NoContent();
    }

    /// <summary>
    /// Reactivate a suspended platform employee
    /// </summary>
    [HttpPost("{employeeId}/reactivate")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ReactivateEmployee(Guid employeeId)
    {
        _logger.LogInformation("Reactivating platform employee {EmployeeId}", employeeId);

        await _mediator.Send(new ReactivatePlatformEmployeeCommand(employeeId));

        return NoContent();
    }

    /// <summary>
    /// Remove a platform employee (soft delete)
    /// </summary>
    [HttpDelete("{employeeId}")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveEmployee(Guid employeeId)
    {
        _logger.LogInformation("Removing platform employee {EmployeeId}", employeeId);

        await _mediator.Send(new RemovePlatformEmployeeCommand(employeeId));

        return NoContent();
    }

    /// <summary>
    /// Get pending invitations
    /// </summary>
    [HttpGet("invitations")]
    [ProducesResponseType(typeof(IEnumerable<PlatformEmployeeInvitationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PlatformEmployeeInvitationDto>>> GetInvitations(
        [FromQuery] string? status = null)
    {
        _logger.LogInformation("Getting platform employee invitations with status {Status}", status);

        var result = await _mediator.Send(new GetPlatformInvitationsQuery(status));
        return Ok(result);
    }

    /// <summary>
    /// Cancel an invitation
    /// </summary>
    [HttpDelete("invitations/{invitationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CancelInvitation(Guid invitationId)
    {
        _logger.LogInformation("Cancelling platform employee invitation {InvitationId}", invitationId);

        await _mediator.Send(new CancelPlatformInvitationCommand(invitationId));

        return NoContent();
    }

    /// <summary>
    /// Resend an invitation
    /// </summary>
    [HttpPost("invitations/{invitationId}/resend")]
    [ProducesResponseType(typeof(PlatformEmployeeInvitationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlatformEmployeeInvitationDto>> ResendInvitation(Guid invitationId)
    {
        _logger.LogInformation("Resending platform employee invitation {InvitationId}", invitationId);

        var currentUserId = GetCurrentUserId();
        var result = await _mediator.Send(new ResendPlatformInvitationCommand(invitationId, currentUserId));

        return Ok(result);
    }

    /// <summary>
    /// Get available platform roles
    /// </summary>
    [HttpGet("~/api/platform-roles")]
    [ProducesResponseType(typeof(IEnumerable<PlatformRoleDefinitionDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<PlatformRoleDefinitionDto>> GetAvailableRoles()
    {
        var roles = new List<PlatformRoleDefinitionDto>
        {
            new()
            {
                Id = "SuperAdmin",
                Name = "Super Administrador",
                Description = "Acceso total a todas las funcionalidades de la plataforma",
                Permissions = new[] { "all" }
            },
            new()
            {
                Id = "Admin",
                Name = "Administrador",
                Description = "Gestión de dealers, usuarios y configuración de la plataforma",
                Permissions = new[] { "dealers", "users", "marketplace", "moderation", "billing", "reports" }
            },
            new()
            {
                Id = "Moderator",
                Name = "Moderador",
                Description = "Aprobación de contenido y moderación de listings",
                Permissions = new[] { "moderation", "listings", "dealers_verify" }
            },
            new()
            {
                Id = "Support",
                Name = "Soporte",
                Description = "Atención al cliente y resolución de tickets",
                Permissions = new[] { "support", "tickets", "users_view" }
            },
            new()
            {
                Id = "Analyst",
                Name = "Analista",
                Description = "Acceso de solo lectura a analytics y reportes",
                Permissions = new[] { "analytics", "reports_view" }
            }
        };

        return Ok(roles);
    }

    /// <summary>
    /// Get activity log for a platform employee
    /// </summary>
    [HttpGet("{employeeId}/activity")]
    [ProducesResponseType(typeof(PaginatedResult<EmployeeActivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<EmployeeActivityDto>>> GetEmployeeActivity(
        Guid employeeId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        _logger.LogInformation("Getting activity log for platform employee {EmployeeId}", employeeId);

        var result = await _mediator.Send(new GetPlatformEmployeeActivityQuery(employeeId, from, to, page, pageSize));
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId");
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) 
            ? userId 
            : Guid.Empty;
    }
}

// ============================================================================
// REQUEST DTOs
// ============================================================================

public class InvitePlatformEmployeeRequest
{
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Support";
    public string[]? Permissions { get; set; }
    public string? Department { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePlatformEmployeeRequest
{
    public string? Role { get; set; }
    public string[]? Permissions { get; set; }
    public string? Department { get; set; }
    public string? Notes { get; set; }
    public string? Status { get; set; }
}

public class SuspendEmployeeRequest
{
    public string? Reason { get; set; }
}

// ============================================================================
// RESPONSE DTOs
// ============================================================================

public class PlatformRoleDefinitionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Permissions { get; set; } = Array.Empty<string>();
}
