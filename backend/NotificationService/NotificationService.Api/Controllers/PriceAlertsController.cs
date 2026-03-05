using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace NotificationService.Api.Controllers;

/// <summary>
/// Controller for price alert management (consolidated from AlertService).
/// Allows users to set price drop alerts on vehicles they're interested in.
/// TODO: Implement full CQRS handlers with MediatR when AlertService logic is migrated.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PriceAlertsController : ControllerBase
{
    private readonly ILogger<PriceAlertsController> _logger;

    public PriceAlertsController(ILogger<PriceAlertsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all price alerts for the current user
    /// </summary>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="page">Page number (default 1)</param>
    /// <param name="pageSize">Page size (default 20)</param>
    /// <returns>Paginated list of price alerts</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAll(
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Getting price alerts for user {UserId}, isActive={IsActive}", userId, isActive);

        // TODO: Replace with MediatR query when AlertService logic is migrated
        var stubData = new
        {
            Items = new[]
            {
                new
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    VehicleId = Guid.NewGuid(),
                    VehicleTitle = "2024 Honda CR-V",
                    CurrentPrice = 35000.00m,
                    TargetPrice = 30000.00m,
                    IsActive = true,
                    LastNotifiedAt = (DateTime?)null,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
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
    /// Get a specific price alert by ID
    /// </summary>
    /// <param name="id">Price alert ID</param>
    /// <returns>Price alert details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        _logger.LogInformation("Getting price alert {AlertId}", id);

        // TODO: Replace with MediatR query when AlertService logic is migrated
        var stubData = new
        {
            Id = id,
            UserId = GetCurrentUserId(),
            VehicleId = Guid.NewGuid(),
            VehicleTitle = "2024 Honda CR-V",
            VehicleImageUrl = "https://cdn.okla.do/vehicles/sample.webp",
            CurrentPrice = 35000.00m,
            TargetPrice = 30000.00m,
            PriceDropPercentage = (decimal?)null,
            IsActive = true,
            NotifyByEmail = true,
            NotifyByPush = true,
            NotifyBySms = false,
            LastNotifiedAt = (DateTime?)null,
            TriggeredCount = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow
        };

        return Ok(stubData);
    }

    /// <summary>
    /// Create a new price alert
    /// </summary>
    /// <param name="request">Price alert creation data</param>
    /// <returns>Created price alert</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreatePriceAlertRequest request)
    {
        _logger.LogInformation("Creating price alert for vehicle {VehicleId}, target price {TargetPrice}",
            request.VehicleId, request.TargetPrice);

        if (request.VehicleId == Guid.Empty)
            return BadRequest(new { error = "VehicleId is required." });

        if (request.TargetPrice <= 0)
            return BadRequest(new { error = "Target price must be greater than zero." });

        // TODO: Replace with MediatR command when AlertService logic is migrated
        var stubData = new
        {
            Id = Guid.NewGuid(),
            UserId = GetCurrentUserId(),
            request.VehicleId,
            request.TargetPrice,
            request.PriceDropPercentage,
            IsActive = true,
            NotifyByEmail = request.NotifyByEmail ?? true,
            NotifyByPush = request.NotifyByPush ?? true,
            NotifyBySms = request.NotifyBySms ?? false,
            CreatedAt = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(GetById), new { id = stubData.Id }, stubData);
    }

    /// <summary>
    /// Update an existing price alert
    /// </summary>
    /// <param name="id">Price alert ID</param>
    /// <param name="request">Updated price alert data</param>
    /// <returns>Updated price alert</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] UpdatePriceAlertRequest request)
    {
        _logger.LogInformation("Updating price alert {AlertId}", id);

        // TODO: Replace with MediatR command when AlertService logic is migrated
        var stubData = new
        {
            Id = id,
            UserId = GetCurrentUserId(),
            request.TargetPrice,
            request.PriceDropPercentage,
            request.IsActive,
            request.NotifyByEmail,
            request.NotifyByPush,
            request.NotifyBySms,
            UpdatedAt = DateTime.UtcNow
        };

        return Ok(stubData);
    }

    /// <summary>
    /// Delete a price alert
    /// </summary>
    /// <param name="id">Price alert ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        _logger.LogInformation("Deleting price alert {AlertId}", id);

        // TODO: Replace with MediatR command when AlertService logic is migrated
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

/// <summary>
/// Request to create a price alert
/// TODO: Move to Application/DTOs when fully implemented
/// </summary>
public record CreatePriceAlertRequest(
    Guid VehicleId,
    decimal TargetPrice,
    decimal? PriceDropPercentage = null,
    bool? NotifyByEmail = true,
    bool? NotifyByPush = true,
    bool? NotifyBySms = false
);

/// <summary>
/// Request to update a price alert
/// TODO: Move to Application/DTOs when fully implemented
/// </summary>
public record UpdatePriceAlertRequest(
    decimal? TargetPrice = null,
    decimal? PriceDropPercentage = null,
    bool? IsActive = null,
    bool? NotifyByEmail = null,
    bool? NotifyByPush = null,
    bool? NotifyBySms = null
);
