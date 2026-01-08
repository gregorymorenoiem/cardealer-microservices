using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostgresDbService.Domain.Entities;
using PostgresDbService.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace PostgresDbService.Api.Controllers;

/// <summary>
/// User-specific controller with type-safe operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger&lt;UsersController&gt; _logger;

    public UsersController(IUserRepository userRepository, ILogger&lt;UsersController&gt; logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// &lt;summary&gt;
    /// Get user by ID
    /// &lt;/summary&gt;
    [HttpGet("{userId:guid}")]
    public async Task&lt;ActionResult&lt;GenericEntity&gt;&gt; GetById(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound($"User not found: {userId}");

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user: {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Get user by email
    /// &lt;/summary&gt;
    [HttpGet("by-email")]
    public async Task&lt;ActionResult&lt;GenericEntity&gt;&gt; GetByEmail([Required][FromQuery] string email)
    {
        try
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound($"User not found with email: {email}");

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Get users by role
    /// &lt;/summary&gt;
    [HttpGet("by-role")]
    public async Task&lt;ActionResult&lt;List&lt;GenericEntity&gt;&gt;&gt; GetByRole([Required][FromQuery] string role)
    {
        try
        {
            var users = await _userRepository.GetUsersByRoleAsync(role);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users by role: {Role}", role);
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Create a new user
    /// &lt;/summary&gt;
    [HttpPost]
    public async Task&lt;ActionResult&lt;GenericEntity&gt;&gt; Create([FromBody] CreateUserRequest request)
    {
        try
        {
            var userData = new
            {
                Email = request.Email,
                FullName = request.FullName,
                Role = request.Role,
                IsActive = request.IsActive,
                City = request.City,
                Province = request.Province,
                Phone = request.Phone,
                CreatedAt = DateTime.UtcNow
            };

            var user = await _userRepository.CreateUserAsync(userData, User.Identity?.Name ?? "system");
            
            return CreatedAtAction(nameof(GetById), 
                new { userId = Guid.Parse(user.EntityId) }, 
                user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Update an existing user
    /// &lt;/summary&gt;
    [HttpPut("{userId:guid}")]
    public async Task&lt;ActionResult&lt;GenericEntity&gt;&gt; Update(Guid userId, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var existingUser = await _userRepository.GetUserByIdAsync(userId);
            if (existingUser == null)
                return NotFound($"User not found: {userId}");

            // Parse existing data and merge with updates
            var existingData = JsonSerializer.Deserialize&lt;Dictionary&lt;string, object&gt;&gt;(existingUser.DataJson);
            
            var userData = new
            {
                Email = request.Email ?? existingData?["Email"]?.ToString(),
                FullName = request.FullName ?? existingData?["FullName"]?.ToString(),
                Role = request.Role ?? existingData?["Role"]?.ToString(),
                IsActive = request.IsActive ?? (bool)(existingData?["IsActive"] ?? true),
                City = request.City ?? existingData?["City"]?.ToString(),
                Province = request.Province ?? existingData?["Province"]?.ToString(),
                Phone = request.Phone ?? existingData?["Phone"]?.ToString(),
                UpdatedAt = DateTime.UtcNow
            };

            var updatedUser = await _userRepository.UpdateUserAsync(userId, userData, User.Identity?.Name ?? "system");
            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }
}

/// &lt;summary&gt;
/// Request model for creating users
/// &lt;/summary&gt;
public record CreateUserRequest(
    [Required] string Email,
    [Required] string FullName,
    [Required] string Role,
    bool IsActive = true,
    string? City = null,
    string? Province = null,
    string? Phone = null
);

/// &lt;summary&gt;
/// Request model for updating users
/// &lt;/summary&gt;
public record UpdateUserRequest(
    string? Email = null,
    string? FullName = null,
    string? Role = null,
    bool? IsActive = null,
    string? City = null,
    string? Province = null,
    string? Phone = null
);