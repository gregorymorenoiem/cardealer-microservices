using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostgresDbService.Domain.Entities;
using PostgresDbService.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace PostgresDbService.Api.Controllers;

/// <summary>
/// Vehicle-specific controller with type-safe operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ILogger&lt;VehiclesController&gt; _logger;

    public VehiclesController(IVehicleRepository vehicleRepository, ILogger&lt;VehiclesController&gt; logger)
    {
        _vehicleRepository = vehicleRepository;
        _logger = logger;
    }

    /// &lt;summary&gt;
    /// Get vehicle by ID
    /// &lt;/summary&gt;
    [HttpGet("{vehicleId:guid}")]
    public async Task&lt;ActionResult&lt;GenericEntity&gt;&gt; GetById(Guid vehicleId)
    {
        try
        {
            var vehicle = await _vehicleRepository.GetVehicleByIdAsync(vehicleId);
            if (vehicle == null)
                return NotFound($"Vehicle not found: {vehicleId}");

            return Ok(vehicle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicle: {VehicleId}", vehicleId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Get vehicles by seller
    /// &lt;/summary&gt;
    [HttpGet("by-seller/{sellerId:guid}")]
    public async Task&lt;ActionResult&lt;List&lt;GenericEntity&gt;&gt;&gt; GetBySeller(Guid sellerId)
    {
        try
        {
            var vehicles = await _vehicleRepository.GetVehiclesBySellerAsync(sellerId);
            return Ok(vehicles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicles by seller: {SellerId}", sellerId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Search vehicles with advanced filters
    /// &lt;/summary&gt;
    [HttpGet("search")]
    public async Task&lt;ActionResult&lt;List&lt;GenericEntity&gt;&gt;&gt; Search(
        [FromQuery] string? make = null,
        [FromQuery] string? model = null,
        [FromQuery] int? yearFrom = null,
        [FromQuery] int? yearTo = null,
        [FromQuery] decimal? priceFrom = null,
        [FromQuery] decimal? priceTo = null)
    {
        try
        {
            var vehicles = await _vehicleRepository.SearchVehiclesAsync(
                make, model, yearFrom, yearTo, priceFrom, priceTo);
            return Ok(vehicles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching vehicles");
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Create a new vehicle
    /// &lt;/summary&gt;
    [HttpPost]
    public async Task&lt;ActionResult&lt;GenericEntity&gt;&gt; Create([FromBody] CreateVehicleRequest request)
    {
        try
        {
            var vehicleData = new
            {
                Make = request.Make,
                Model = request.Model,
                Year = request.Year,
                Price = request.Price,
                Mileage = request.Mileage,
                FuelType = request.FuelType,
                Transmission = request.Transmission,
                BodyType = request.BodyType,
                Color = request.Color,
                Status = request.Status,
                SellerId = request.SellerId,
                Description = request.Description,
                Images = request.Images ?? new List&lt;string&gt;(),
                Features = request.Features ?? new List&lt;string&gt;(),
                City = request.City,
                Province = request.Province,
                ContactPhone = request.ContactPhone,
                CreatedAt = DateTime.UtcNow
            };

            var vehicle = await _vehicleRepository.CreateVehicleAsync(vehicleData, User.Identity?.Name ?? "system");
            
            return CreatedAtAction(nameof(GetById), 
                new { vehicleId = Guid.Parse(vehicle.EntityId) }, 
                vehicle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle");
            return StatusCode(500, "Internal server error");
        }
    }

    /// &lt;summary&gt;
    /// Update an existing vehicle
    /// &lt;/summary&gt;
    [HttpPut("{vehicleId:guid}")]
    public async Task&lt;ActionResult&lt;GenericEntity&gt;&gt; Update(Guid vehicleId, [FromBody] UpdateVehicleRequest request)
    {
        try
        {
            var existingVehicle = await _vehicleRepository.GetVehicleByIdAsync(vehicleId);
            if (existingVehicle == null)
                return NotFound($"Vehicle not found: {vehicleId}");

            // Parse existing data and merge with updates
            var existingData = JsonSerializer.Deserialize&lt;Dictionary&lt;string, object&gt;&gt;(existingVehicle.DataJson);
            
            var vehicleData = new
            {
                Make = request.Make ?? existingData?["Make"]?.ToString(),
                Model = request.Model ?? existingData?["Model"]?.ToString(),
                Year = request.Year ?? Convert.ToInt32(existingData?["Year"] ?? 0),
                Price = request.Price ?? Convert.ToDecimal(existingData?["Price"] ?? 0),
                Mileage = request.Mileage ?? Convert.ToInt32(existingData?["Mileage"] ?? 0),
                FuelType = request.FuelType ?? existingData?["FuelType"]?.ToString(),
                Transmission = request.Transmission ?? existingData?["Transmission"]?.ToString(),
                BodyType = request.BodyType ?? existingData?["BodyType"]?.ToString(),
                Color = request.Color ?? existingData?["Color"]?.ToString(),
                Status = request.Status ?? existingData?["Status"]?.ToString(),
                SellerId = request.SellerId ?? existingData?["SellerId"]?.ToString(),
                Description = request.Description ?? existingData?["Description"]?.ToString(),
                Images = request.Images ?? existingData?["Images"] ?? new List&lt;string&gt;(),
                Features = request.Features ?? existingData?["Features"] ?? new List&lt;string&gt;(),
                City = request.City ?? existingData?["City"]?.ToString(),
                Province = request.Province ?? existingData?["Province"]?.ToString(),
                ContactPhone = request.ContactPhone ?? existingData?["ContactPhone"]?.ToString(),
                UpdatedAt = DateTime.UtcNow
            };

            var updatedVehicle = await _vehicleRepository.UpdateVehicleAsync(vehicleId, vehicleData, User.Identity?.Name ?? "system");
            return Ok(updatedVehicle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle: {VehicleId}", vehicleId);
            return StatusCode(500, "Internal server error");
        }
    }
}

/// &lt;summary&gt;
/// Request model for creating vehicles
/// &lt;/summary&gt;
public record CreateVehicleRequest(
    [Required] string Make,
    [Required] string Model,
    [Required] int Year,
    [Required] decimal Price,
    int Mileage = 0,
    string? FuelType = null,
    string? Transmission = null,
    string? BodyType = null,
    string? Color = null,
    string Status = "Active",
    [Required] string SellerId,
    string? Description = null,
    List&lt;string&gt;? Images = null,
    List&lt;string&gt;? Features = null,
    string? City = null,
    string? Province = null,
    string? ContactPhone = null
);

/// &lt;summary&gt;
/// Request model for updating vehicles
/// &lt;/summary&gt;
public record UpdateVehicleRequest(
    string? Make = null,
    string? Model = null,
    int? Year = null,
    decimal? Price = null,
    int? Mileage = null,
    string? FuelType = null,
    string? Transmission = null,
    string? BodyType = null,
    string? Color = null,
    string? Status = null,
    string? SellerId = null,
    string? Description = null,
    List&lt;string&gt;? Images = null,
    List&lt;string&gt;? Features = null,
    string? City = null,
    string? Province = null,
    string? ContactPhone = null
);