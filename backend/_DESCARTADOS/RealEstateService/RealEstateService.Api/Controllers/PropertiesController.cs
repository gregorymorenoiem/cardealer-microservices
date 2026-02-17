using Microsoft.AspNetCore.Mvc;
using RealEstateService.Application.DTOs;
using RealEstateService.Application.Services;
using RealEstateService.Domain.Entities;

namespace RealEstateService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly PropertyService _propertyService;
    private readonly ILogger<PropertiesController> _logger;

    public PropertiesController(PropertyService propertyService, ILogger<PropertiesController> logger)
    {
        _propertyService = propertyService;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todas las propiedades
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PropertySummaryDto>>> GetAll()
    {
        var properties = await _propertyService.GetAllAsync();
        return Ok(properties);
    }

    /// <summary>
    /// Obtener propiedad por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PropertyDto>> GetById(Guid id)
    {
        var property = await _propertyService.GetByIdAsync(id);
        if (property == null)
            return NotFound(new { message = $"Property with ID {id} not found" });

        // Increment view count
        await _propertyService.IncrementViewCountAsync(id);

        return Ok(property);
    }

    /// <summary>
    /// Obtener propiedades destacadas
    /// </summary>
    [HttpGet("featured")]
    public async Task<ActionResult<IEnumerable<PropertySummaryDto>>> GetFeatured([FromQuery] int limit = 10)
    {
        var properties = await _propertyService.GetFeaturedAsync(limit);
        return Ok(properties);
    }

    /// <summary>
    /// Obtener propiedades por dealer
    /// </summary>
    [HttpGet("dealer/{dealerId:guid}")]
    public async Task<ActionResult<IEnumerable<PropertySummaryDto>>> GetByDealer(Guid dealerId)
    {
        var properties = await _propertyService.GetByDealerIdAsync(dealerId);
        return Ok(properties);
    }

    /// <summary>
    /// Buscar propiedades con filtros
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<PropertySearchResponse>> Search([FromQuery] PropertySearchRequest request)
    {
        var result = await _propertyService.SearchAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Crear nueva propiedad
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PropertyDto>> Create([FromBody] CreatePropertyRequest request)
    {
        // TODO: Get from JWT claims
        var dealerId = GetDealerIdFromHeader();
        var sellerId = Guid.NewGuid(); // TODO: Get from JWT
        var sellerName = "Current User"; // TODO: Get from JWT

        var property = await _propertyService.CreateAsync(request, dealerId, sellerId, sellerName);
        return CreatedAtAction(nameof(GetById), new { id = property.Id }, property);
    }

    /// <summary>
    /// Actualizar propiedad
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PropertyDto>> Update(Guid id, [FromBody] UpdatePropertyRequest request)
    {
        var property = await _propertyService.UpdateAsync(id, request);
        if (property == null)
            return NotFound(new { message = $"Property with ID {id} not found" });

        return Ok(property);
    }

    /// <summary>
    /// Actualizar estado de propiedad
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<PropertyDto>> UpdateStatus(Guid id, [FromBody] UpdatePropertyStatusRequest request)
    {
        var property = await _propertyService.UpdateStatusAsync(id, request.Status);
        if (property == null)
            return NotFound(new { message = $"Property with ID {id} not found" });

        return Ok(property);
    }

    /// <summary>
    /// Publicar propiedad (cambiar a Active)
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<PropertyDto>> Publish(Guid id)
    {
        var property = await _propertyService.UpdateStatusAsync(id, PropertyStatus.Active);
        if (property == null)
            return NotFound(new { message = $"Property with ID {id} not found" });

        return Ok(property);
    }

    /// <summary>
    /// Archivar propiedad
    /// </summary>
    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<PropertyDto>> Archive(Guid id)
    {
        var property = await _propertyService.UpdateStatusAsync(id, PropertyStatus.Archived);
        if (property == null)
            return NotFound(new { message = $"Property with ID {id} not found" });

        return Ok(property);
    }

    /// <summary>
    /// Marcar como vendida
    /// </summary>
    [HttpPost("{id:guid}/sold")]
    public async Task<ActionResult<PropertyDto>> MarkAsSold(Guid id)
    {
        var property = await _propertyService.UpdateStatusAsync(id, PropertyStatus.Sold);
        if (property == null)
            return NotFound(new { message = $"Property with ID {id} not found" });

        return Ok(property);
    }

    /// <summary>
    /// Marcar como rentada
    /// </summary>
    [HttpPost("{id:guid}/rented")]
    public async Task<ActionResult<PropertyDto>> MarkAsRented(Guid id)
    {
        var property = await _propertyService.UpdateStatusAsync(id, PropertyStatus.Rented);
        if (property == null)
            return NotFound(new { message = $"Property with ID {id} not found" });

        return Ok(property);
    }

    /// <summary>
    /// Eliminar propiedad (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _propertyService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = $"Property with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Helper: Get DealerId from header (multi-tenant)
    /// </summary>
    private Guid GetDealerIdFromHeader()
    {
        if (Request.Headers.TryGetValue("X-Dealer-Id", out var dealerIdHeader) &&
            Guid.TryParse(dealerIdHeader, out var dealerId))
        {
            return dealerId;
        }

        // Default for testing
        return Guid.Parse("00000000-0000-0000-0000-000000000001");
    }
}
