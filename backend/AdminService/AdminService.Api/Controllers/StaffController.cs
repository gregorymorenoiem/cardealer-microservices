using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Api.Controllers;

/// <summary>
/// Controller for platform staff management (consolidated from StaffService).
/// TODO: Implement full CQRS handlers with MediatR when StaffService logic is migrated.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class StaffController : ControllerBase
{
    private readonly ILogger<StaffController> _logger;

    public StaffController(ILogger<StaffController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all staff members with optional filtering
    /// </summary>
    /// <param name="role">Filter by role</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="page">Page number (default 1)</param>
    /// <param name="pageSize">Page size (default 20)</param>
    /// <returns>Paginated list of staff members</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAll(
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Getting staff members with filters: role={Role}, isActive={IsActive}, page={Page}", role, isActive, page);

        // TODO: Replace with MediatR query when StaffService logic is migrated
        var stubData = new
        {
            Items = new[]
            {
                new
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Stub",
                    LastName = "Staff",
                    Email = "stub@okla.do",
                    Role = role ?? "Support",
                    IsActive = isActive ?? true,
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize,
            TotalPages = 1
        };

        return Ok(stubData);
    }

    /// <summary>
    /// Get a staff member by ID
    /// </summary>
    /// <param name="id">Staff member ID</param>
    /// <returns>Staff member details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        _logger.LogInformation("Getting staff member {StaffId}", id);

        // TODO: Replace with MediatR query when StaffService logic is migrated
        var stubData = new
        {
            Id = id,
            FirstName = "Stub",
            LastName = "Staff",
            Email = "stub@okla.do",
            Role = "Support",
            IsActive = true,
            Department = "Customer Support",
            HireDate = DateTime.UtcNow.AddMonths(-6),
            CreatedAt = DateTime.UtcNow.AddMonths(-6),
            UpdatedAt = DateTime.UtcNow
        };

        return Ok(stubData);
    }

    /// <summary>
    /// Create a new staff member
    /// </summary>
    /// <param name="request">Staff member creation data</param>
    /// <returns>Created staff member</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateStaffRequest request)
    {
        _logger.LogInformation("Creating staff member with email {Email}", request.Email);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.FirstName))
            return BadRequest(new { error = "Email and FirstName are required." });

        // TODO: Replace with MediatR command when StaffService logic is migrated
        var stubData = new
        {
            Id = Guid.NewGuid(),
            request.FirstName,
            request.LastName,
            request.Email,
            request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(GetById), new { id = stubData.Id }, stubData);
    }

    /// <summary>
    /// Update an existing staff member
    /// </summary>
    /// <param name="id">Staff member ID</param>
    /// <param name="request">Updated staff member data</param>
    /// <returns>Updated staff member</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] UpdateStaffRequest request)
    {
        _logger.LogInformation("Updating staff member {StaffId}", id);

        // TODO: Replace with MediatR command when StaffService logic is migrated
        var stubData = new
        {
            Id = id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Role,
            request.IsActive,
            UpdatedAt = DateTime.UtcNow
        };

        return Ok(stubData);
    }

    /// <summary>
    /// Delete a staff member
    /// </summary>
    /// <param name="id">Staff member ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        _logger.LogInformation("Deleting staff member {StaffId}", id);

        // TODO: Replace with MediatR command when StaffService logic is migrated
        return NoContent();
    }
}

/// <summary>
/// DTO for creating a staff member
/// TODO: Move to Application/DTOs when fully implemented
/// </summary>
public record CreateStaffRequest(
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string? Department = null,
    string? Phone = null
);

/// <summary>
/// DTO for updating a staff member
/// TODO: Move to Application/DTOs when fully implemented
/// </summary>
public record UpdateStaffRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Role,
    bool? IsActive,
    string? Department = null,
    string? Phone = null
);
