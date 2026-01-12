using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UserService.Api.Controllers;

/// <summary>
/// Controller for managing dealer team members (employees)
/// </summary>
[ApiController]
[Route("api/dealers/{dealerId}/employees")]
public class DealerEmployeesController : ControllerBase
{
    private readonly IDealerEmployeeRepository _employeeRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DealerEmployeesController> _logger;

    public DealerEmployeesController(
        IDealerEmployeeRepository employeeRepository,
        ApplicationDbContext context,
        ILogger<DealerEmployeesController> logger)
    {
        _employeeRepository = employeeRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all employees for a dealer
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DealerEmployeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DealerEmployeeDto>>> GetEmployees(Guid dealerId)
    {
        var employees = await _context.DealerEmployees
            .Where(e => e.DealerId == dealerId)
            .Include(e => e.User)
            .OrderByDescending(e => e.InvitationDate)
            .ToListAsync();

        var dtos = employees.Select(e => new DealerEmployeeDto
        {
            Id = e.Id,
            UserId = e.UserId,
            DealerId = e.DealerId,
            Name = e.User?.FullName ?? "Sin nombre",
            Email = e.User?.Email ?? "Sin email",
            Role = e.DealerRole.ToString(),
            Status = e.Status.ToString(),
            InvitationDate = e.InvitationDate,
            ActivationDate = e.ActivationDate,
            AvatarUrl = e.User?.ProfilePicture
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Get an employee by ID
    /// </summary>
    [HttpGet("{employeeId}")]
    [ProducesResponseType(typeof(DealerEmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DealerEmployeeDto>> GetEmployee(Guid dealerId, Guid employeeId)
    {
        var employee = await _context.DealerEmployees
            .Where(e => e.Id == employeeId && e.DealerId == dealerId)
            .Include(e => e.User)
            .FirstOrDefaultAsync();

        if (employee == null)
            return NotFound(new { error = "Employee not found" });

        return Ok(new DealerEmployeeDto
        {
            Id = employee.Id,
            UserId = employee.UserId,
            DealerId = employee.DealerId,
            Name = employee.User?.FullName ?? "Sin nombre",
            Email = employee.User?.Email ?? "Sin email",
            Role = employee.DealerRole.ToString(),
            Status = employee.Status.ToString(),
            InvitationDate = employee.InvitationDate,
            ActivationDate = employee.ActivationDate,
            AvatarUrl = employee.User?.ProfilePicture
        });
    }

    /// <summary>
    /// Invite a new team member
    /// </summary>
    [HttpPost("invite")]
    [ProducesResponseType(typeof(DealerEmployeeInvitationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DealerEmployeeInvitationDto>> InviteEmployee(
        Guid dealerId,
        [FromBody] InviteEmployeeRequest request)
    {
        // Check if user already exists by email
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        
        // Check if invitation already exists
        var existingInvitation = await _context.DealerEmployeeInvitations
            .FirstOrDefaultAsync(i => i.DealerId == dealerId && i.Email == request.Email && i.Status == InvitationStatus.Pending);
        
        if (existingInvitation != null)
            return BadRequest(new { error = "Ya existe una invitación pendiente para este email" });

        // Check if already an employee
        if (existingUser != null)
        {
            var existingEmployee = await _context.DealerEmployees
                .FirstOrDefaultAsync(e => e.UserId == existingUser.Id && e.DealerId == dealerId && e.Status == EmployeeStatus.Active);
            
            if (existingEmployee != null)
                return BadRequest(new { error = "Este usuario ya es miembro del equipo" });
        }

        // Parse role
        if (!Enum.TryParse<DealerRole>(request.Role, true, out var role))
            role = DealerRole.Salesperson;

        // Create invitation
        var invitation = new DealerEmployeeInvitation
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Email = request.Email,
            DealerRole = role,
            Permissions = request.Permissions ?? "[]",
            InvitedBy = request.InvitedBy,
            Status = InvitationStatus.Pending,
            InvitationDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(7),
            Token = Guid.NewGuid().ToString("N")
        };

        await _context.DealerEmployeeInvitations.AddAsync(invitation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Invitation sent to {Email} for dealer {DealerId}", request.Email, dealerId);

        // TODO: Send invitation email via NotificationService

        return CreatedAtAction(nameof(GetEmployee), new { dealerId, employeeId = invitation.Id }, new DealerEmployeeInvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            Role = invitation.DealerRole.ToString(),
            Status = invitation.Status.ToString(),
            InvitationDate = invitation.InvitationDate,
            ExpirationDate = invitation.ExpirationDate
        });
    }

    /// <summary>
    /// Update employee role
    /// </summary>
    [HttpPut("{employeeId}/role")]
    [ProducesResponseType(typeof(DealerEmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DealerEmployeeDto>> UpdateEmployeeRole(
        Guid dealerId,
        Guid employeeId,
        [FromBody] UpdateEmployeeRoleRequest request)
    {
        var employee = await _context.DealerEmployees
            .Where(e => e.Id == employeeId && e.DealerId == dealerId)
            .Include(e => e.User)
            .FirstOrDefaultAsync();

        if (employee == null)
            return NotFound(new { error = "Employee not found" });

        if (Enum.TryParse<DealerRole>(request.Role, true, out var role))
        {
            employee.DealerRole = role;
        }

        if (request.Permissions != null)
        {
            employee.Permissions = System.Text.Json.JsonSerializer.Serialize(request.Permissions);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated role for employee {EmployeeId} to {Role}", employeeId, request.Role);

        return Ok(new DealerEmployeeDto
        {
            Id = employee.Id,
            UserId = employee.UserId,
            DealerId = employee.DealerId,
            Name = employee.User?.FullName ?? "Sin nombre",
            Email = employee.User?.Email ?? "Sin email",
            Role = employee.DealerRole.ToString(),
            Status = employee.Status.ToString(),
            InvitationDate = employee.InvitationDate,
            ActivationDate = employee.ActivationDate,
            AvatarUrl = employee.User?.ProfilePicture
        });
    }

    /// <summary>
    /// Remove an employee from the team
    /// </summary>
    [HttpDelete("{employeeId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveEmployee(Guid dealerId, Guid employeeId)
    {
        var employee = await _context.DealerEmployees
            .FirstOrDefaultAsync(e => e.Id == employeeId && e.DealerId == dealerId);

        if (employee == null)
            return NotFound(new { error = "Employee not found" });

        employee.Status = EmployeeStatus.Suspended;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Employee {EmployeeId} removed from dealer {DealerId}", employeeId, dealerId);

        return NoContent();
    }

    /// <summary>
    /// Get pending invitations
    /// </summary>
    [HttpGet("invitations")]
    [ProducesResponseType(typeof(IEnumerable<DealerEmployeeInvitationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DealerEmployeeInvitationDto>>> GetInvitations(Guid dealerId)
    {
        var invitations = await _context.DealerEmployeeInvitations
            .Where(i => i.DealerId == dealerId && i.Status == InvitationStatus.Pending)
            .OrderByDescending(i => i.InvitationDate)
            .ToListAsync();

        return Ok(invitations.Select(i => new DealerEmployeeInvitationDto
        {
            Id = i.Id,
            Email = i.Email,
            Role = i.DealerRole.ToString(),
            Status = i.Status.ToString(),
            InvitationDate = i.InvitationDate,
            ExpirationDate = i.ExpirationDate
        }));
    }

    /// <summary>
    /// Cancel an invitation
    /// </summary>
    [HttpDelete("invitations/{invitationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CancelInvitation(Guid dealerId, Guid invitationId)
    {
        var invitation = await _context.DealerEmployeeInvitations
            .FirstOrDefaultAsync(i => i.Id == invitationId && i.DealerId == dealerId);

        if (invitation == null)
            return NotFound(new { error = "Invitation not found" });

        invitation.Status = InvitationStatus.Revoked;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Get available roles
    /// </summary>
    [HttpGet("~/api/dealer-roles")]
    [ProducesResponseType(typeof(IEnumerable<RoleDefinitionDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<RoleDefinitionDto>> GetAvailableRoles()
    {
        var roles = new List<RoleDefinitionDto>
        {
            new RoleDefinitionDto
            {
                Id = "Owner",
                Name = "Propietario",
                Description = "Control total del dealer",
                Permissions = new[] { "all" }
            },
            new RoleDefinitionDto
            {
                Id = "Admin",
                Name = "Administrador",
                Description = "Gestión completa excepto facturación",
                Permissions = new[] { "inventory", "leads", "users", "reports", "settings" }
            },
            new RoleDefinitionDto
            {
                Id = "SalesManager",
                Name = "Gerente de Ventas",
                Description = "Gestión de ventas y equipo de vendedores",
                Permissions = new[] { "inventory", "leads", "sales_team", "reports" }
            },
            new RoleDefinitionDto
            {
                Id = "Salesperson",
                Name = "Vendedor",
                Description = "Gestión de leads y vehículos asignados",
                Permissions = new[] { "inventory_view", "leads_assigned", "own_stats" }
            },
            new RoleDefinitionDto
            {
                Id = "InventoryManager",
                Name = "Gestor de Inventario",
                Description = "Gestión del inventario de vehículos",
                Permissions = new[] { "inventory", "photos", "pricing" }
            },
            new RoleDefinitionDto
            {
                Id = "Viewer",
                Name = "Solo Lectura",
                Description = "Acceso de solo visualización",
                Permissions = new[] { "view_only" }
            }
        };

        return Ok(roles);
    }
}

// DTOs
public class DealerEmployeeDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid DealerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime InvitationDate { get; set; }
    public DateTime? ActivationDate { get; set; }
    public string? AvatarUrl { get; set; }
}

public class DealerEmployeeInvitationDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime InvitationDate { get; set; }
    public DateTime ExpirationDate { get; set; }
}

public class InviteEmployeeRequest
{
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Salesperson";
    public string? Permissions { get; set; }
    public Guid InvitedBy { get; set; }
}

public class UpdateEmployeeRoleRequest
{
    public string Role { get; set; } = string.Empty;
    public string[]? Permissions { get; set; }
}

public class RoleDefinitionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Permissions { get; set; } = Array.Empty<string>();
}
