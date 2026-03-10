using Microsoft.AspNetCore.Mvc;
using VehiclesSaleService.Domain.Interfaces;

namespace VehiclesSaleService.Api.Controllers;

/// <summary>
/// Internal API endpoints for inter-service communication.
/// NO JWT authentication required (service-to-service calls within Docker network / k8s cluster).
/// </summary>
[ApiController]
[Route("api/internal/vehicles")]
public class InternalVehiclesController : ControllerBase
{
    private readonly IVehicleRepository _vehicleRepository;

    public InternalVehiclesController(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    /// <summary>
    /// Returns active, non-featured, non-sold vehicles published more than N days ago.
    /// Used by NotificationService's listing inactivity upsell worker to detect stale inventory.
    /// </summary>
    /// <param name="daysOld">Minimum days since publication (default: 45)</param>
    /// <param name="skip">Pagination offset</param>
    /// <param name="take">Page size (max 200)</param>
    [HttpGet("stale")]
    public async Task<IActionResult> GetStaleListings(
        [FromQuery] int daysOld = 45,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100)
    {
        if (daysOld < 1) return BadRequest(new { error = "daysOld must be >= 1" });
        if (take > 200) take = 200;

        var vehicles = await _vehicleRepository.GetStaleActiveListingsAsync(daysOld, skip, take);

        var result = vehicles.Select(v => new
        {
            v.Id,
            v.SellerId,
            v.DealerId,
            v.Title,
            v.Make,
            v.Model,
            v.Year,
            v.Price,
            v.Currency,
            v.PublishedAt,
            DaysSincePublished = v.PublishedAt.HasValue
                ? (int)(DateTime.UtcNow - v.PublishedAt.Value).TotalDays
                : 0,
            v.ViewCount,
            v.InquiryCount,
            v.FavoriteCount,
            v.SellerName,
            v.SellerEmail,
            PrimaryImageUrl = v.Images.FirstOrDefault()?.Url
        });

        return Ok(result);
    }
}
