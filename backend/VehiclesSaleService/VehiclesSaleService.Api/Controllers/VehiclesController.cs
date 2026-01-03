using Microsoft.AspNetCore.Mvc;
using VehiclesSaleService.Domain.Entities;
using VehiclesSaleService.Domain.Interfaces;

namespace VehiclesSaleService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(
        IVehicleRepository vehicleRepository,
        ICategoryRepository categoryRepository,
        ILogger<VehiclesController> logger)
    {
        _vehicleRepository = vehicleRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    /// <summary>
    /// Search vehicles with filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<VehicleSearchResult>> Search([FromQuery] VehicleSearchRequest request)
    {
        var parameters = new VehicleSearchParameters
        {
            SearchTerm = request.Search,
            CategoryId = request.CategoryId,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            Make = request.Make,
            Model = request.Model,
            MinYear = request.MinYear,
            MaxYear = request.MaxYear,
            MinMileage = request.MinMileage,
            MaxMileage = request.MaxMileage,
            VehicleType = request.VehicleType,
            BodyStyle = request.BodyStyle,
            FuelType = request.FuelType,
            Transmission = request.Transmission,
            DriveType = request.DriveType,
            Condition = request.Condition,
            ExteriorColor = request.ExteriorColor,
            State = request.State,
            City = request.City,
            ZipCode = request.ZipCode,
            IsCertified = request.IsCertified,
            HasCleanTitle = request.HasCleanTitle,
            Skip = request.Page * request.PageSize,
            Take = request.PageSize,
            SortBy = request.SortBy ?? "CreatedAt",
            SortDescending = request.SortDescending ?? true
        };

        var vehicles = await _vehicleRepository.SearchAsync(parameters);
        var totalCount = await _vehicleRepository.GetCountAsync(parameters);

        return Ok(new VehicleSearchResult
        {
            Vehicles = vehicles,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        });
    }

    /// <summary>
    /// Get vehicle by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Vehicle>> GetById(Guid id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);

        if (vehicle == null)
            return NotFound(new { message = "Vehicle not found" });

        return Ok(vehicle);
    }

    /// <summary>
    /// Get vehicle by VIN
    /// </summary>
    [HttpGet("vin/{vin}")]
    public async Task<ActionResult<Vehicle>> GetByVin(string vin)
    {
        var vehicle = await _vehicleRepository.GetByVINAsync(vin);

        if (vehicle == null)
            return NotFound(new { message = "Vehicle not found" });

        return Ok(vehicle);
    }

    /// <summary>
    /// Get featured vehicles
    /// </summary>
    [HttpGet("featured")]
    public async Task<ActionResult<IEnumerable<Vehicle>>> GetFeatured([FromQuery] int take = 10)
    {
        var vehicles = await _vehicleRepository.GetFeaturedAsync(take);
        return Ok(vehicles);
    }

    /// <summary>
    /// Get vehicles by seller
    /// </summary>
    [HttpGet("seller/{sellerId:guid}")]
    public async Task<ActionResult<IEnumerable<Vehicle>>> GetBySeller(Guid sellerId)
    {
        var vehicles = await _vehicleRepository.GetBySellerAsync(sellerId);
        return Ok(vehicles);
    }

    /// <summary>
    /// Get vehicles by dealer
    /// </summary>
    [HttpGet("dealer/{dealerId:guid}")]
    public async Task<ActionResult<IEnumerable<Vehicle>>> GetByDealer(Guid dealerId)
    {
        var vehicles = await _vehicleRepository.GetByDealerAsync(dealerId);
        return Ok(vehicles);
    }

    /// <summary>
    /// Create new vehicle
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Vehicle>> Create([FromBody] CreateVehicleRequest request)
    {
        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
            return BadRequest(new { message = "Category not found" });

        var vehicle = new Vehicle
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            Currency = request.Currency ?? "USD",
            Status = VehicleStatus.Draft,
            VIN = request.VIN,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            Mileage = request.Mileage,
            MileageUnit = request.MileageUnit ?? "miles",
            VehicleType = request.VehicleType,
            BodyStyle = request.BodyStyle,
            FuelType = request.FuelType,
            Transmission = request.Transmission,
            DriveType = request.DriveType,
            EngineSize = request.EngineSize,
            EngineCylinders = request.EngineCylinders,
            Horsepower = request.Horsepower,
            ExteriorColor = request.ExteriorColor,
            InteriorColor = request.InteriorColor,
            Condition = request.Condition,
            IsCertified = request.IsCertified ?? false,
            HasCleanTitle = request.HasCleanTitle ?? true,
            StreetAddress = request.StreetAddress,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            Country = request.Country ?? "USA",
            SellerId = request.SellerId,
            SellerName = request.SellerName,
            DealerId = request.DealerId,
            CategoryId = request.CategoryId,
            CategoryName = category.Name
        };

        // Add images
        if (request.Images != null)
        {
            var sortOrder = 0;
            foreach (var imageUrl in request.Images)
            {
                vehicle.Images.Add(new VehicleImage
                {
                    Url = imageUrl,
                    SortOrder = sortOrder,
                    IsPrimary = sortOrder == 0
                });
                sortOrder++;
            }
        }

        var created = await _vehicleRepository.CreateAsync(vehicle);

        _logger.LogInformation("Vehicle created: {VehicleId} - {Title}", created.Id, created.Title);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update vehicle
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Vehicle>> Update(Guid id, [FromBody] UpdateVehicleRequest request)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);

        if (vehicle == null)
            return NotFound(new { message = "Vehicle not found" });

        // Update fields
        if (request.Title != null) vehicle.Title = request.Title;
        if (request.Description != null) vehicle.Description = request.Description;
        if (request.Price.HasValue) vehicle.Price = request.Price.Value;
        if (request.Status.HasValue) vehicle.Status = request.Status.Value;
        if (request.Mileage.HasValue) vehicle.Mileage = request.Mileage.Value;
        if (request.ExteriorColor != null) vehicle.ExteriorColor = request.ExteriorColor;
        if (request.InteriorColor != null) vehicle.InteriorColor = request.InteriorColor;
        if (request.Condition.HasValue) vehicle.Condition = request.Condition.Value;
        if (request.IsCertified.HasValue) vehicle.IsCertified = request.IsCertified.Value;
        if (request.HasCleanTitle.HasValue) vehicle.HasCleanTitle = request.HasCleanTitle.Value;
        if (request.IsFeatured.HasValue) vehicle.IsFeatured = request.IsFeatured.Value;

        await _vehicleRepository.UpdateAsync(vehicle);

        _logger.LogInformation("Vehicle updated: {VehicleId}", id);

        return Ok(vehicle);
    }

    /// <summary>
    /// Delete vehicle (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var exists = await _vehicleRepository.ExistsAsync(id);

        if (!exists)
            return NotFound(new { message = "Vehicle not found" });

        await _vehicleRepository.DeleteAsync(id);

        _logger.LogInformation("Vehicle deleted: {VehicleId}", id);

        return NoContent();
    }
}

// ========================================
// Request/Response DTOs
// ========================================

public record VehicleSearchRequest
{
    public string? Search { get; init; }
    public Guid? CategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? Make { get; init; }
    public string? Model { get; init; }
    public int? MinYear { get; init; }
    public int? MaxYear { get; init; }
    public int? MinMileage { get; init; }
    public int? MaxMileage { get; init; }
    public VehicleType? VehicleType { get; init; }
    public BodyStyle? BodyStyle { get; init; }
    public FuelType? FuelType { get; init; }
    public Transmission? Transmission { get; init; }
    public DriveType? DriveType { get; init; }
    public VehicleCondition? Condition { get; init; }
    public string? ExteriorColor { get; init; }
    public string? State { get; init; }
    public string? City { get; init; }
    public string? ZipCode { get; init; }
    public bool? IsCertified { get; init; }
    public bool? HasCleanTitle { get; init; }
    public int Page { get; init; } = 0;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; }
    public bool? SortDescending { get; init; }
}

public record VehicleSearchResult
{
    public IEnumerable<Vehicle> Vehicles { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

public record CreateVehicleRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string? Currency { get; init; }
    public string VIN { get; init; } = string.Empty;
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public int Mileage { get; init; }
    public string? MileageUnit { get; init; }
    public VehicleType VehicleType { get; init; }
    public BodyStyle? BodyStyle { get; init; }
    public FuelType FuelType { get; init; }
    public Transmission Transmission { get; init; }
    public DriveType DriveType { get; init; }
    public string? EngineSize { get; init; }
    public int? EngineCylinders { get; init; }
    public int? Horsepower { get; init; }
    public string? ExteriorColor { get; init; }
    public string? InteriorColor { get; init; }
    public VehicleCondition Condition { get; init; }
    public bool? IsCertified { get; init; }
    public bool? HasCleanTitle { get; init; }
    public string? StreetAddress { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public string? Country { get; init; }
    public Guid SellerId { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public Guid? DealerId { get; init; }
    public Guid CategoryId { get; init; }
    public List<string>? Images { get; init; }
}

public record UpdateVehicleRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public VehicleStatus? Status { get; init; }
    public int? Mileage { get; init; }
    public string? ExteriorColor { get; init; }
    public string? InteriorColor { get; init; }
    public VehicleCondition? Condition { get; init; }
    public bool? IsCertified { get; init; }
    public bool? HasCleanTitle { get; init; }
    public bool? IsFeatured { get; init; }
}
