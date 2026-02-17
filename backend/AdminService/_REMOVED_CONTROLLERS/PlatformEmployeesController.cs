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
    /// Accept a platform employee invitation (PUBLIC - no auth required)
    /// This endpoint is used when a new admin accepts their invitation via email link
    /// </summary>
    [HttpPost("invitations/{token}/accept")]
    [AllowAnonymous] // Public endpoint - invitation token provides security
    [ProducesResponseType(typeof(AcceptInvitationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status410Gone)] // Invitation expired
    public async Task<ActionResult<AcceptInvitationResponse>> AcceptInvitation(
        string token,
        [FromBody] AcceptPlatformInvitationRequest request)
    {
        _logger.LogInformation("Accepting platform employee invitation with token");

        var command = new AcceptPlatformInvitationCommand(
            Token: token,
            FirstName: request.FirstName,
            LastName: request.LastName,
            Password: request.Password,
            PhoneNumber: request.PhoneNumber
        );

        var result = await _mediator.Send(command);

        return Ok(result);
    }

    /// <summary>
    /// Validate invitation token (PUBLIC - no auth required)
    /// Used by frontend to check if invitation is valid before showing the form
    /// </summary>
    [HttpGet("invitations/{token}/validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ValidateInvitationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public async Task<ActionResult<ValidateInvitationResponse>> ValidateInvitation(string token)
    {
        _logger.LogInformation("Validating platform employee invitation token");

        var result = await _mediator.Send(new ValidatePlatformInvitationQuery(token));

        if (result == null)
            return NotFound(new { error = "Invitación no encontrada" });

        if (result.IsExpired)
            return StatusCode(410, new { error = "Invitación expirada" });

        return Ok(result);
    }

    /// <summary>
    /// Delete the default admin account (SECURITY - only after creating real admins)
    /// This should be called after the first real SuperAdmin has been created
    /// </summary>
    [HttpDelete("~/api/admin/security/default-admin")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteDefaultAdmin()
    {
        var currentUserId = GetCurrentUserId();
        _logger.LogWarning("SuperAdmin {UserId} is attempting to delete the default admin account", currentUserId);

        var result = await _mediator.Send(new DeleteDefaultAdminCommand(currentUserId));

        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        _logger.LogWarning("Default admin account has been successfully deleted by {UserId}", currentUserId);
        return NoContent();
    }

    /// <summary>
    /// Check platform security status (default admin still exists, etc.)
    /// </summary>
    [HttpGet("~/api/admin/security/status")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(typeof(SecurityStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SecurityStatusResponse>> GetSecurityStatus()
    {
        _logger.LogInformation("Checking platform security status");

        var result = await _mediator.Send(new GetSecurityStatusQuery());
        return Ok(result);
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

/// <summary>
/// Request to accept a platform employee invitation
/// </summary>
public class AcceptPlatformInvitationRequest
{
    /// <summary>First name of the new admin</summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>Last name of the new admin</summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>Password for the new account (min 8 chars, must include uppercase, lowercase, number, special char)</summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>Optional phone number</summary>
    public string? PhoneNumber { get; set; }
}

// ============================================================================
// RESPONSE DTOs
// ============================================================================

/// <summary>
/// Response after successfully accepting an invitation
/// </summary>
public class AcceptInvitationResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string[] Permissions { get; set; } = Array.Empty<string>();
    public string Message { get; set; } = string.Empty;
    public string? AccessToken { get; set; } // Optional: auto-login after accepting
}

/// <summary>
/// Response when validating an invitation token
/// </summary>
public class ValidateInvitationResponse
{
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string InvitedByName { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public bool IsExpired { get; set; }
    public bool IsValid { get; set; }
}

/// <summary>
/// Platform security status response
/// </summary>
public class SecurityStatusResponse
{
    /// <summary>True if the default admin@okla.local account still exists</summary>
    public bool DefaultAdminExists { get; set; }
    
    /// <summary>Number of real SuperAdmin accounts (excluding default)</summary>
    public int RealSuperAdminCount { get; set; }
    
    /// <summary>True if it's safe to delete the default admin</summary>
    public bool CanDeleteDefaultAdmin { get; set; }
    
    /// <summary>Security recommendations</summary>
    public string[] Recommendations { get; set; } = Array.Empty<string>();
    
    /// <summary>Last security check timestamp</summary>
    public DateTime CheckedAt { get; set; }
}

public class PlatformRoleDefinitionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Permissions { get; set; } = Array.Empty<string>();
}
