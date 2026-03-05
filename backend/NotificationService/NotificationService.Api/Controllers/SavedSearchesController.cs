using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace NotificationService.Api.Controllers;

/// <summary>
/// Controller for saved searches management (consolidated from AlertService).
/// Allows users to save search criteria and receive notifications when new matches appear.
/// TODO: Implement full CQRS handlers with MediatR when AlertService logic is migrated.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SavedSearchesController : ControllerBase
{
    private readonly ILogger<SavedSearchesController> _logger;

    public SavedSearchesController(ILogger<SavedSearchesController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all saved searches for the current user
    /// </summary>
    /// <param name="page">Page number (default 1)</param>
    /// <param name="pageSize">Page size (default 20)</param>
    /// <returns>Paginated list of saved searches</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Getting saved searches for user {UserId}, page={Page}", userId, page);

        // TODO: Replace with MediatR query when AlertService logic is migrated
        var stubData = new
        {
            Items = new[]
            {
                new
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = "SUV bajo 30k",
                    Criteria = new
                    {
                        Make = "Toyota",
                        BodyType = "SUV",
                        MinYear = 2020,
                        MaxPrice = 30000m,
                        Location = "Santo Domingo"
                    },
                    NotifyOnNewResults = true,
                    NotificationFrequency = "daily",
                    MatchCount = 12,
                    LastMatchAt = DateTime.UtcNow.AddHours(-3),
                    CreatedAt = DateTime.UtcNow.AddDays(-14)
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
    /// Get a specific saved search by ID
    /// </summary>
    /// <param name="id">Saved search ID</param>
    /// <returns>Saved search details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        _logger.LogInformation("Getting saved search {SearchId}", id);

        // TODO: Replace with MediatR query when AlertService logic is migrated
        var stubData = new
        {
            Id = id,
            UserId = GetCurrentUserId(),
            Name = "SUV bajo 30k",
            Criteria = new
            {
                Make = "Toyota",
                Model = (string?)null,
                BodyType = "SUV",
                MinYear = 2020,
                MaxYear = (int?)null,
                MinPrice = (decimal?)null,
                MaxPrice = 30000m,
                FuelType = (string?)null,
                Transmission = (string?)null,
                Location = "Santo Domingo",
                MaxMileage = (int?)null
            },
            NotifyOnNewResults = true,
            NotifyByEmail = true,
            NotifyByPush = true,
            NotificationFrequency = "daily",
            MatchCount = 12,
            LastMatchAt = DateTime.UtcNow.AddHours(-3),
            LastNotifiedAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-14),
            UpdatedAt = DateTime.UtcNow
        };

        return Ok(stubData);
    }

    /// <summary>
    /// Create a new saved search
    /// </summary>
    /// <param name="request">Saved search creation data</param>
    /// <returns>Created saved search</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateSavedSearchRequest request)
    {
        _logger.LogInformation("Creating saved search: {Name}", request.Name);

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Search name is required." });

        // TODO: Replace with MediatR command when AlertService logic is migrated
        var stubData = new
        {
            Id = Guid.NewGuid(),
            UserId = GetCurrentUserId(),
            request.Name,
            request.Criteria,
            NotifyOnNewResults = request.NotifyOnNewResults ?? true,
            NotifyByEmail = request.NotifyByEmail ?? true,
            NotifyByPush = request.NotifyByPush ?? true,
            NotificationFrequency = request.NotificationFrequency ?? "daily",
            MatchCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(GetById), new { id = stubData.Id }, stubData);
    }

    /// <summary>
    /// Update an existing saved search
    /// </summary>
    /// <param name="id">Saved search ID</param>
    /// <param name="request">Updated saved search data</param>
    /// <returns>Updated saved search</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] UpdateSavedSearchRequest request)
    {
        _logger.LogInformation("Updating saved search {SearchId}", id);

        // TODO: Replace with MediatR command when AlertService logic is migrated
        var stubData = new
        {
            Id = id,
            UserId = GetCurrentUserId(),
            request.Name,
            request.Criteria,
            request.NotifyOnNewResults,
            request.NotifyByEmail,
            request.NotifyByPush,
            request.NotificationFrequency,
            UpdatedAt = DateTime.UtcNow
        };

        return Ok(stubData);
    }

    /// <summary>
    /// Delete a saved search
    /// </summary>
    /// <param name="id">Saved search ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        _logger.LogInformation("Deleting saved search {SearchId}", id);

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
/// Search criteria for saved searches
/// TODO: Move to Application/DTOs when fully implemented
/// </summary>
public record SavedSearchCriteria(
    string? Make = null,
    string? Model = null,
    string? BodyType = null,
    int? MinYear = null,
    int? MaxYear = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? FuelType = null,
    string? Transmission = null,
    string? Location = null,
    int? MaxMileage = null
);

/// <summary>
/// Request to create a saved search
/// TODO: Move to Application/DTOs when fully implemented
/// </summary>
public record CreateSavedSearchRequest(
    string Name,
    SavedSearchCriteria Criteria,
    bool? NotifyOnNewResults = true,
    bool? NotifyByEmail = true,
    bool? NotifyByPush = true,
    string? NotificationFrequency = "daily"
);

/// <summary>
/// Request to update a saved search
/// TODO: Move to Application/DTOs when fully implemented
/// </summary>
public record UpdateSavedSearchRequest(
    string? Name = null,
    SavedSearchCriteria? Criteria = null,
    bool? NotifyOnNewResults = null,
    bool? NotifyByEmail = null,
    bool? NotifyByPush = null,
    string? NotificationFrequency = null
);
