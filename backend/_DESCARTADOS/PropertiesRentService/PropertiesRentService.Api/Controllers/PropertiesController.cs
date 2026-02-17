using Microsoft.AspNetCore.Mvc;
using PropertiesRentService.Domain.Entities;
using PropertiesRentService.Domain.Interfaces;

namespace PropertiesRentService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<PropertiesController> _logger;

    public PropertiesController(
        IPropertyRepository propertyRepository,
        ICategoryRepository categoryRepository,
        ILogger<PropertiesController> logger)
    {
        _propertyRepository = propertyRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    /// <summary>
    /// Search properties for rent with filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PropertySearchResult>> Search([FromQuery] PropertySearchRequest request)
    {
        var parameters = new PropertySearchParameters
        {
            SearchTerm = request.Search,
            CategoryId = request.CategoryId,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            PropertyType = request.PropertyType,
            PropertySubType = request.PropertySubType,
            MinBedrooms = request.MinBedrooms,
            MaxBedrooms = request.MaxBedrooms,
            MinBathrooms = request.MinBathrooms,
            MaxBathrooms = request.MaxBathrooms,
            MinSquareFeet = request.MinSquareFeet,
            MaxSquareFeet = request.MaxSquareFeet,
            MinYearBuilt = request.MinYearBuilt,
            MaxYearBuilt = request.MaxYearBuilt,
            State = request.State,
            City = request.City,
            ZipCode = request.ZipCode,
            Neighborhood = request.Neighborhood,
            HasPool = request.HasPool,
            HasGarage = request.HasGarage,
            HasBasement = request.HasBasement,
            HasFireplace = request.HasFireplace,
            HeatingType = request.HeatingType,
            CoolingType = request.CoolingType,
            Skip = request.Page * request.PageSize,
            Take = request.PageSize,
            SortBy = request.SortBy ?? "CreatedAt",
            SortDescending = request.SortDescending ?? true
        };

        var properties = await _propertyRepository.SearchAsync(parameters);
        var totalCount = await _propertyRepository.GetCountAsync(parameters);

        return Ok(new PropertySearchResult
        {
            Properties = properties,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        });
    }

    /// <summary>
    /// Get property by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Property>> GetById(Guid id)
    {
        var property = await _propertyRepository.GetByIdAsync(id);

        if (property == null)
            return NotFound(new { message = "Property not found" });

        return Ok(property);
    }

    /// <summary>
    /// Get property by MLS Number
    /// </summary>
    [HttpGet("mls/{mlsNumber}")]
    public async Task<ActionResult<Property>> GetByMLS(string mlsNumber)
    {
        var property = await _propertyRepository.GetByMLSNumberAsync(mlsNumber);

        if (property == null)
            return NotFound(new { message = "Property not found" });

        return Ok(property);
    }

    /// <summary>
    /// Get featured rental properties
    /// </summary>
    [HttpGet("featured")]
    public async Task<ActionResult<IEnumerable<Property>>> GetFeatured([FromQuery] int take = 10)
    {
        var properties = await _propertyRepository.GetFeaturedAsync(take);
        return Ok(properties);
    }

    /// <summary>
    /// Get properties by agent
    /// </summary>
    [HttpGet("agent/{agentId:guid}")]
    public async Task<ActionResult<IEnumerable<Property>>> GetByAgent(Guid agentId)
    {
        var properties = await _propertyRepository.GetByAgentAsync(agentId);
        return Ok(properties);
    }

    /// <summary>
    /// Get properties by dealer/agency
    /// </summary>
    [HttpGet("dealer/{dealerId:guid}")]
    public async Task<ActionResult<IEnumerable<Property>>> GetByDealer(Guid dealerId)
    {
        var properties = await _propertyRepository.GetByDealerAsync(dealerId);
        return Ok(properties);
    }

    /// <summary>
    /// Create new rental property listing
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Property>> Create([FromBody] CreatePropertyRequest request)
    {
        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
            return BadRequest(new { message = "Category not found" });

        var property = new Property
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            Currency = request.Currency ?? "USD",
            Status = PropertyStatus.Draft,
            MLSNumber = request.MLSNumber,
            PropertyType = request.PropertyType,
            PropertySubType = request.PropertySubType,
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms,
            HalfBathrooms = request.HalfBathrooms,
            SquareFeet = request.SquareFeet,
            LotSize = request.LotSize,
            LotSizeUnit = request.LotSizeUnit ?? "sqft",
            YearBuilt = request.YearBuilt,
            Stories = request.Stories,
            GarageSpaces = request.GarageSpaces,
            HasPool = request.HasPool ?? false,
            HasBasement = request.HasBasement ?? false,
            HasFireplace = request.HasFireplace ?? false,
            HeatingType = request.HeatingType,
            CoolingType = request.CoolingType,
            StreetAddress = request.StreetAddress,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            Country = request.Country ?? "USA",
            Neighborhood = request.Neighborhood,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AgentId = request.AgentId,
            AgentName = request.AgentName,
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
                property.Images.Add(new PropertyImage
                {
                    Url = imageUrl,
                    SortOrder = sortOrder,
                    IsPrimary = sortOrder == 0
                });
                sortOrder++;
            }
        }

        var created = await _propertyRepository.CreateAsync(property);

        _logger.LogInformation("Rental property created: {PropertyId} - {Title}", created.Id, created.Title);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update property
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Property>> Update(Guid id, [FromBody] UpdatePropertyRequest request)
    {
        var property = await _propertyRepository.GetByIdAsync(id);

        if (property == null)
            return NotFound(new { message = "Property not found" });

        // Update fields
        if (request.Title != null) property.Title = request.Title;
        if (request.Description != null) property.Description = request.Description;
        if (request.Price.HasValue) property.Price = request.Price.Value;
        if (request.Status.HasValue) property.Status = request.Status.Value;
        if (request.Bedrooms.HasValue) property.Bedrooms = request.Bedrooms.Value;
        if (request.Bathrooms.HasValue) property.Bathrooms = request.Bathrooms.Value;
        if (request.SquareFeet.HasValue) property.SquareFeet = request.SquareFeet.Value;
        if (request.HasPool.HasValue) property.HasPool = request.HasPool.Value;
        if (request.HasBasement.HasValue) property.HasBasement = request.HasBasement.Value;
        if (request.HasFireplace.HasValue) property.HasFireplace = request.HasFireplace.Value;
        if (request.IsFeatured.HasValue) property.IsFeatured = request.IsFeatured.Value;

        await _propertyRepository.UpdateAsync(property);

        _logger.LogInformation("Rental property updated: {PropertyId}", id);

        return Ok(property);
    }

    /// <summary>
    /// Delete property (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var exists = await _propertyRepository.ExistsAsync(id);

        if (!exists)
            return NotFound(new { message = "Property not found" });

        await _propertyRepository.DeleteAsync(id);

        _logger.LogInformation("Rental property deleted: {PropertyId}", id);

        return NoContent();
    }
}

// ========================================
// Request/Response DTOs
// ========================================

public record PropertySearchRequest
{
    public string? Search { get; init; }
    public Guid? CategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public PropertyType? PropertyType { get; init; }
    public PropertySubType? PropertySubType { get; init; }
    public int? MinBedrooms { get; init; }
    public int? MaxBedrooms { get; init; }
    public decimal? MinBathrooms { get; init; }
    public decimal? MaxBathrooms { get; init; }
    public int? MinSquareFeet { get; init; }
    public int? MaxSquareFeet { get; init; }
    public int? MinYearBuilt { get; init; }
    public int? MaxYearBuilt { get; init; }
    public string? State { get; init; }
    public string? City { get; init; }
    public string? ZipCode { get; init; }
    public string? Neighborhood { get; init; }
    public bool? HasPool { get; init; }
    public bool? HasGarage { get; init; }
    public bool? HasBasement { get; init; }
    public bool? HasFireplace { get; init; }
    public HeatingType? HeatingType { get; init; }
    public CoolingType? CoolingType { get; init; }
    public int Page { get; init; } = 0;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; }
    public bool? SortDescending { get; init; }
}

public record PropertySearchResult
{
    public IEnumerable<Property> Properties { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

public record CreatePropertyRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string? Currency { get; init; }
    public string? MLSNumber { get; init; }
    public PropertyType PropertyType { get; init; }
    public PropertySubType? PropertySubType { get; init; }
    public int Bedrooms { get; init; }
    public decimal Bathrooms { get; init; }
    public int? HalfBathrooms { get; init; }
    public int SquareFeet { get; init; }
    public decimal? LotSize { get; init; }
    public string? LotSizeUnit { get; init; }
    public int? YearBuilt { get; init; }
    public int? Stories { get; init; }
    public int? GarageSpaces { get; init; }
    public bool? HasPool { get; init; }
    public bool? HasBasement { get; init; }
    public bool? HasFireplace { get; init; }
    public HeatingType? HeatingType { get; init; }
    public CoolingType? CoolingType { get; init; }
    public string StreetAddress { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string? Country { get; init; }
    public string? Neighborhood { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public Guid AgentId { get; init; }
    public string AgentName { get; init; } = string.Empty;
    public Guid? DealerId { get; init; }
    public Guid CategoryId { get; init; }
    public List<string>? Images { get; init; }
}

public record UpdatePropertyRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public PropertyStatus? Status { get; init; }
    public int? Bedrooms { get; init; }
    public decimal? Bathrooms { get; init; }
    public int? SquareFeet { get; init; }
    public bool? HasPool { get; init; }
    public bool? HasBasement { get; init; }
    public bool? HasFireplace { get; init; }
    public bool? IsFeatured { get; init; }
}
