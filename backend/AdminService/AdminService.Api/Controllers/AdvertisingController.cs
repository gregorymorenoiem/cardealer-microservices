using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.Api.Controllers;

/// <summary>
/// Controller for advertising campaign and space management (consolidated from AdvertisingService).
/// TODO: Implement full CQRS handlers with MediatR when AdvertisingService logic is migrated.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdvertisingController : ControllerBase
{
    private readonly ILogger<AdvertisingController> _logger;

    public AdvertisingController(ILogger<AdvertisingController> logger)
    {
        _logger = logger;
    }

    // ========================================
    // CAMPAIGNS ENDPOINTS
    // ========================================

    /// <summary>
    /// Get all advertising campaigns with optional filtering
    /// </summary>
    /// <param name="status">Filter by campaign status (active, paused, completed, draft)</param>
    /// <param name="page">Page number (default 1)</param>
    /// <param name="pageSize">Page size (default 20)</param>
    /// <returns>Paginated list of campaigns</returns>
    [HttpGet("campaigns")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCampaigns(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Getting advertising campaigns with status={Status}, page={Page}", status, page);

        // TODO: Replace with MediatR query when AdvertisingService logic is migrated
        var stubData = new
        {
            Items = new[]
            {
                new
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Campaign",
                    Status = status ?? "active",
                    AdvertiserId = Guid.NewGuid(),
                    AdvertiserName = "OKLA Dealers",
                    SpaceId = Guid.NewGuid(),
                    StartDate = DateTime.UtcNow.AddDays(-7),
                    EndDate = DateTime.UtcNow.AddDays(23),
                    Budget = 5000.00m,
                    SpentAmount = 1200.00m,
                    Impressions = 15420,
                    Clicks = 342,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
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
    /// Get a specific campaign by ID
    /// </summary>
    /// <param name="id">Campaign ID</param>
    /// <returns>Campaign details</returns>
    [HttpGet("campaigns/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetCampaignById(Guid id)
    {
        _logger.LogInformation("Getting campaign {CampaignId}", id);

        // TODO: Replace with MediatR query when AdvertisingService logic is migrated
        var stubData = new
        {
            Id = id,
            Name = "Sample Campaign",
            Status = "active",
            AdvertiserId = Guid.NewGuid(),
            AdvertiserName = "OKLA Dealers",
            SpaceId = Guid.NewGuid(),
            SpaceName = "Homepage Banner",
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow.AddDays(23),
            Budget = 5000.00m,
            SpentAmount = 1200.00m,
            Impressions = 15420,
            Clicks = 342,
            ClickThroughRate = 2.22,
            TargetUrl = "https://okla.do/dealers/premium",
            ImageUrl = "https://cdn.okla.do/ads/sample-banner.webp",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow
        };

        return Ok(stubData);
    }

    /// <summary>
    /// Create a new advertising campaign
    /// </summary>
    /// <param name="request">Campaign creation data</param>
    /// <returns>Created campaign</returns>
    [HttpPost("campaigns")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateCampaign([FromBody] CreateCampaignRequest request)
    {
        _logger.LogInformation("Creating campaign: {Name}", request.Name);

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Campaign name is required." });

        // TODO: Replace with MediatR command when AdvertisingService logic is migrated
        var stubData = new
        {
            Id = Guid.NewGuid(),
            request.Name,
            Status = "draft",
            request.AdvertiserId,
            request.SpaceId,
            request.StartDate,
            request.EndDate,
            request.Budget,
            SpentAmount = 0m,
            Impressions = 0,
            Clicks = 0,
            request.TargetUrl,
            request.ImageUrl,
            CreatedAt = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(GetCampaignById), new { id = stubData.Id }, stubData);
    }

    /// <summary>
    /// Update an existing campaign
    /// </summary>
    /// <param name="id">Campaign ID</param>
    /// <param name="request">Updated campaign data</param>
    /// <returns>Updated campaign</returns>
    [HttpPut("campaigns/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateCampaign(Guid id, [FromBody] UpdateCampaignRequest request)
    {
        _logger.LogInformation("Updating campaign {CampaignId}", id);

        // TODO: Replace with MediatR command when AdvertisingService logic is migrated
        var stubData = new
        {
            Id = id,
            request.Name,
            request.Status,
            request.StartDate,
            request.EndDate,
            request.Budget,
            request.TargetUrl,
            request.ImageUrl,
            UpdatedAt = DateTime.UtcNow
        };

        return Ok(stubData);
    }

    /// <summary>
    /// Delete a campaign
    /// </summary>
    /// <param name="id">Campaign ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("campaigns/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteCampaign(Guid id)
    {
        _logger.LogInformation("Deleting campaign {CampaignId}", id);

        // TODO: Replace with MediatR command when AdvertisingService logic is migrated
        return NoContent();
    }

    // ========================================
    // AD SPACES ENDPOINTS
    // ========================================

    /// <summary>
    /// Get all advertising spaces
    /// </summary>
    /// <param name="location">Filter by location (homepage, search, detail, sidebar)</param>
    /// <returns>List of advertising spaces</returns>
    [HttpGet("spaces")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetSpaces([FromQuery] string? location = null)
    {
        _logger.LogInformation("Getting advertising spaces with location={Location}", location);

        // TODO: Replace with MediatR query when AdvertisingService logic is migrated
        var stubData = new[]
        {
            new
            {
                Id = Guid.NewGuid(),
                Name = "Homepage Banner",
                Location = "homepage",
                Width = 728,
                Height = 90,
                Format = "banner",
                PricePerDay = 50.00m,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new
            {
                Id = Guid.NewGuid(),
                Name = "Search Results Sidebar",
                Location = "search",
                Width = 300,
                Height = 250,
                Format = "rectangle",
                PricePerDay = 30.00m,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            }
        };

        var result = location != null
            ? stubData.Where(s => s.Location == location).ToArray()
            : stubData;

        return Ok(result);
    }

    /// <summary>
    /// Create a new advertising space
    /// </summary>
    /// <param name="request">Ad space creation data</param>
    /// <returns>Created ad space</returns>
    [HttpPost("spaces")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateSpace([FromBody] CreateAdSpaceRequest request)
    {
        _logger.LogInformation("Creating ad space: {Name}", request.Name);

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Ad space name is required." });

        // TODO: Replace with MediatR command when AdvertisingService logic is migrated
        var stubData = new
        {
            Id = Guid.NewGuid(),
            request.Name,
            request.Location,
            request.Width,
            request.Height,
            request.Format,
            request.PricePerDay,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(GetSpaces), stubData);
    }

    /// <summary>
    /// Update an existing ad space
    /// </summary>
    /// <param name="id">Ad space ID</param>
    /// <param name="request">Updated ad space data</param>
    /// <returns>Updated ad space</returns>
    [HttpPut("spaces/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateSpace(Guid id, [FromBody] UpdateAdSpaceRequest request)
    {
        _logger.LogInformation("Updating ad space {SpaceId}", id);

        // TODO: Replace with MediatR command when AdvertisingService logic is migrated
        var stubData = new
        {
            Id = id,
            request.Name,
            request.Location,
            request.Width,
            request.Height,
            request.Format,
            request.PricePerDay,
            request.IsAvailable,
            UpdatedAt = DateTime.UtcNow
        };

        return Ok(stubData);
    }

    /// <summary>
    /// Delete an ad space
    /// </summary>
    /// <param name="id">Ad space ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("spaces/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteSpace(Guid id)
    {
        _logger.LogInformation("Deleting ad space {SpaceId}", id);

        // TODO: Replace with MediatR command when AdvertisingService logic is migrated
        return NoContent();
    }

    // ========================================
    // STATS ENDPOINT
    // ========================================

    /// <summary>
    /// Get advertising statistics summary
    /// </summary>
    /// <param name="startDate">Start date for stats period</param>
    /// <param name="endDate">End date for stats period</param>
    /// <returns>Advertising statistics</returns>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var from = startDate ?? DateTime.UtcNow.AddDays(-30);
        var to = endDate ?? DateTime.UtcNow;

        _logger.LogInformation("Getting advertising stats from {StartDate} to {EndDate}", from, to);

        // TODO: Replace with MediatR query when AdvertisingService logic is migrated
        var stubData = new
        {
            Period = new { StartDate = from, EndDate = to },
            TotalCampaigns = 12,
            ActiveCampaigns = 5,
            TotalImpressions = 245000,
            TotalClicks = 4830,
            AverageClickThroughRate = 1.97,
            TotalRevenue = 15400.00m,
            TotalSpaces = 8,
            OccupiedSpaces = 5,
            TopCampaigns = new[]
            {
                new { Name = "Premium Dealers Promo", Impressions = 45000, Clicks = 1200 },
                new { Name = "New Year Sale", Impressions = 38000, Clicks = 950 }
            }
        };

        return Ok(stubData);
    }
}

/// <summary>
/// Request to create a campaign
/// TODO: Move to Application/DTOs when fully implemented
/// </summary>
public record CreateCampaignRequest(
    string Name,
    Guid AdvertiserId,
    Guid? SpaceId,
    DateTime StartDate,
    DateTime EndDate,
    decimal Budget,
    string? TargetUrl = null,
    string? ImageUrl = null
);

/// <summary>
/// Request to update a campaign
/// TODO: Move to Application/DTOs when fully implemented
/// </summary>
public record UpdateCampaignRequest(
    string? Name,
    string? Status,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? Budget,
    string? TargetUrl = null,
    string? ImageUrl = null
);

/// <summary>
/// Request to create an ad space
/// TODO: Move to Application/DTOs when fully implemented
/// </summary>
public record CreateAdSpaceRequest(
    string Name,
    string Location,
    int Width,
    int Height,
    string Format,
    decimal PricePerDay
);

/// <summary>
/// Request to update an ad space
/// TODO: Move to Application/DTOs when fully implemented
/// </summary>
public record UpdateAdSpaceRequest(
    string? Name,
    string? Location,
    int? Width,
    int? Height,
    string? Format,
    decimal? PricePerDay,
    bool? IsAvailable
);
