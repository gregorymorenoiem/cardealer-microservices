using ComparisonService.Domain.Entities;
using ComparisonService.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComparisonService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehicleComparisonsController : ControllerBase
{
    private readonly IVehicleComparisonRepository _repository;

    public VehicleComparisonsController(IVehicleComparisonRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Get user's vehicle comparisons
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserComparisons()
    {
        var userId = GetCurrentUserId();
        var comparisons = await _repository.GetByUserIdAsync(userId);
        
        return Ok(comparisons.Select(c => new ComparisonSummaryDto
        {
            Id = c.Id,
            Name = c.Name,
            VehicleCount = c.VehicleIds.Count,
            CreatedAt = c.CreatedAt,
            IsShared = !string.IsNullOrEmpty(c.ShareToken)
        }));
    }

    /// <summary>
    /// Create a new vehicle comparison
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateComparison(CreateComparisonDto dto)
    {
        var userId = GetCurrentUserId();
        
        // Validate vehicle IDs (max 3)
        if (dto.VehicleIds.Count > 3)
        {
            return BadRequest(new { message = "Maximum 3 vehicles allowed per comparison" });
        }

        if (dto.VehicleIds.Count == 0)
        {
            return BadRequest(new { message = "At least 1 vehicle is required" });
        }

        // Check for duplicates
        if (dto.VehicleIds.Distinct().Count() != dto.VehicleIds.Count)
        {
            return BadRequest(new { message = "Duplicate vehicle IDs are not allowed" });
        }
        
        var comparison = new VehicleComparison(userId, dto.Name ?? "Vehicle Comparison");
        
        foreach (var vehicleId in dto.VehicleIds)
        {
            comparison.AddVehicle(vehicleId);
        }
        
        var created = await _repository.CreateAsync(comparison);
        
        return CreatedAtAction(nameof(GetComparison), new { id = created.Id }, new ComparisonDto
        {
            Id = created.Id,
            Name = created.Name,
            VehicleIds = created.VehicleIds,
            CreatedAt = created.CreatedAt,
            ShareToken = created.ShareToken
        });
    }

    /// <summary>
    /// Get specific comparison
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetComparison(Guid id)
    {
        var comparison = await _repository.GetByIdAsync(id);
        if (comparison == null) 
            return NotFound(new { message = "Comparison not found" });

        var userId = GetCurrentUserId();
        if (comparison.UserId != userId && string.IsNullOrEmpty(comparison.ShareToken))
        {
            return Forbid(new { message = "Access denied to this comparison" });
        }

        return Ok(new ComparisonDto
        {
            Id = comparison.Id,
            Name = comparison.Name,
            VehicleIds = comparison.VehicleIds,
            CreatedAt = comparison.CreatedAt,
            ShareToken = comparison.ShareToken
        });
    }

    /// <summary>
    /// Add vehicle to comparison
    /// </summary>
    [HttpPost("{id}/vehicles/{vehicleId}")]
    public async Task<IActionResult> AddVehicleToComparison(Guid id, Guid vehicleId)
    {
        var comparison = await _repository.GetByIdAsync(id);
        if (comparison == null) 
            return NotFound(new { message = "Comparison not found" });

        var userId = GetCurrentUserId();
        if (comparison.UserId != userId) 
            return Forbid(new { message = "Access denied" });

        try
        {
            comparison.AddVehicle(vehicleId);
            await _repository.UpdateAsync(comparison);

            return Ok(new { 
                message = "Vehicle added to comparison", 
                vehicleCount = comparison.VehicleIds.Count,
                vehicleIds = comparison.VehicleIds 
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove vehicle from comparison
    /// </summary>
    [HttpDelete("{id}/vehicles/{vehicleId}")]
    public async Task<IActionResult> RemoveVehicleFromComparison(Guid id, Guid vehicleId)
    {
        var comparison = await _repository.GetByIdAsync(id);
        if (comparison == null) 
            return NotFound(new { message = "Comparison not found" });

        var userId = GetCurrentUserId();
        if (comparison.UserId != userId) 
            return Forbid(new { message = "Access denied" });

        if (!comparison.VehicleIds.Contains(vehicleId))
        {
            return BadRequest(new { message = "Vehicle not found in this comparison" });
        }

        comparison.RemoveVehicle(vehicleId);
        await _repository.UpdateAsync(comparison);

        return Ok(new { 
            message = "Vehicle removed from comparison", 
            vehicleCount = comparison.VehicleIds.Count,
            vehicleIds = comparison.VehicleIds 
        });
    }

    /// <summary>
    /// Generate shareable link for comparison
    /// </summary>
    [HttpPost("{id}/share")]
    public async Task<IActionResult> ShareComparison(Guid id)
    {
        var comparison = await _repository.GetByIdAsync(id);
        if (comparison == null) 
            return NotFound(new { message = "Comparison not found" });

        var userId = GetCurrentUserId();
        if (comparison.UserId != userId) 
            return Forbid(new { message = "Access denied" });

        // Generate new share token
        comparison.GenerateShareToken();
        await _repository.UpdateAsync(comparison);

        var shareUrl = $"{Request.Scheme}://{Request.Host}/comparison/shared/{comparison.ShareToken}";

        return Ok(new ShareResponseDto
        { 
            ShareToken = comparison.ShareToken!,
            ShareUrl = shareUrl,
            Message = "Comparison shared successfully"
        });
    }

    /// <summary>
    /// Get shared comparison (public access)
    /// </summary>
    [HttpGet("shared/{shareToken}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSharedComparison(string shareToken)
    {
        if (string.IsNullOrEmpty(shareToken))
            return BadRequest(new { message = "Share token is required" });

        var comparison = await _repository.GetByShareTokenAsync(shareToken);
        if (comparison == null) 
            return NotFound(new { message = "Shared comparison not found or expired" });

        return Ok(new SharedComparisonDto
        {
            Id = comparison.Id,
            Name = comparison.Name,
            VehicleIds = comparison.VehicleIds,
            CreatedAt = comparison.CreatedAt,
            IsShared = true
        });
    }

    /// <summary>
    /// Update comparison name
    /// </summary>
    [HttpPatch("{id}/name")]
    public async Task<IActionResult> UpdateComparisonName(Guid id, UpdateNameDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Name is required" });

        var comparison = await _repository.GetByIdAsync(id);
        if (comparison == null) 
            return NotFound(new { message = "Comparison not found" });

        var userId = GetCurrentUserId();
        if (comparison.UserId != userId) 
            return Forbid(new { message = "Access denied" });

        comparison.UpdateName(dto.Name);
        await _repository.UpdateAsync(comparison);

        return Ok(new { message = "Comparison name updated", name = comparison.Name });
    }

    /// <summary>
    /// Delete comparison
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComparison(Guid id)
    {
        var comparison = await _repository.GetByIdAsync(id);
        if (comparison == null) 
            return NotFound(new { message = "Comparison not found" });

        var userId = GetCurrentUserId();
        if (comparison.UserId != userId) 
            return Forbid(new { message = "Access denied" });

        await _repository.DeleteAsync(id);
        return Ok(new { message = "Comparison deleted successfully" });
    }

    /// <summary>
    /// Get user's comparison statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetUserStats()
    {
        var userId = GetCurrentUserId();
        var totalComparisons = await _repository.GetUserComparisonCountAsync(userId);
        var recentComparisons = await _repository.GetRecentByUserIdAsync(userId, 5);

        return Ok(new StatsDto
        {
            TotalComparisons = totalComparisons,
            RecentComparisons = recentComparisons.Select(c => new ComparisonSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                VehicleCount = c.VehicleIds.Count,
                CreatedAt = c.CreatedAt,
                IsShared = !string.IsNullOrEmpty(c.ShareToken)
            }).ToList()
        });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            throw new UnauthorizedAccessException("User ID not found in token");
        
        return Guid.Parse(userIdClaim);
    }
}

// DTOs
public record CreateComparisonDto(string? Name, List<Guid> VehicleIds);
public record UpdateNameDto(string Name);

public class ComparisonDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Guid> VehicleIds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? ShareToken { get; set; }
}

public class ComparisonSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int VehicleCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsShared { get; set; }
}

public class SharedComparisonDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Guid> VehicleIds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsShared { get; set; } = true;
}

public class ShareResponseDto
{
    public string ShareToken { get; set; } = string.Empty;
    public string ShareUrl { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class StatsDto
{
    public int TotalComparisons { get; set; }
    public List<ComparisonSummaryDto> RecentComparisons { get; set; } = new();
}