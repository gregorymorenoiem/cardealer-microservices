using RealEstateService.Domain.Entities;

namespace RealEstateService.Domain.Interfaces;

/// <summary>
/// Repositorio de propiedades inmobiliarias
/// </summary>
public interface IPropertyRepository
{
    Task<Property?> GetByIdAsync(Guid id);
    Task<IEnumerable<Property>> GetAllAsync();
    Task<IEnumerable<Property>> GetByDealerIdAsync(Guid dealerId);
    Task<IEnumerable<Property>> GetActiveAsync();
    Task<IEnumerable<Property>> GetFeaturedAsync(int limit = 10);
    Task<IEnumerable<Property>> SearchAsync(PropertySearchCriteria criteria);
    Task<int> GetCountAsync(Guid? dealerId = null);
    Task<Property> AddAsync(Property property);
    Task<Property> UpdateAsync(Property property);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}

/// <summary>
/// Criterios de búsqueda de propiedades
/// </summary>
public class PropertySearchCriteria
{
    public string? Query { get; set; }
    public PropertyType? Type { get; set; }
    public ListingType? ListingType { get; set; }
    public PropertyStatus? Status { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinArea { get; set; }
    public decimal? MaxArea { get; set; }
    public int? MinBedrooms { get; set; }
    public int? MaxBedrooms { get; set; }
    public int? MinBathrooms { get; set; }
    public int? MaxBathrooms { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Neighborhood { get; set; }
    public bool? HasPool { get; set; }
    public bool? HasGarden { get; set; }
    public bool? HasGym { get; set; }
    public bool? HasSecurity { get; set; }
    public bool? IsFurnished { get; set; }
    public bool? AllowsPets { get; set; }
    public bool? IsFeatured { get; set; }
    public Guid? DealerId { get; set; }

    // Paginación
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    // Ordenamiento
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}

/// <summary>
/// Repositorio de imágenes de propiedad
/// </summary>
public interface IPropertyImageRepository
{
    Task<PropertyImage?> GetByIdAsync(Guid id);
    Task<IEnumerable<PropertyImage>> GetByPropertyIdAsync(Guid propertyId);
    Task<PropertyImage> AddAsync(PropertyImage image);
    Task<PropertyImage> UpdateAsync(PropertyImage image);
    Task DeleteAsync(Guid id);
    Task ReorderAsync(Guid propertyId, IEnumerable<Guid> orderedIds);
}

/// <summary>
/// Repositorio de amenidades
/// </summary>
public interface IPropertyAmenityRepository
{
    Task<IEnumerable<PropertyAmenity>> GetByPropertyIdAsync(Guid propertyId);
    Task<PropertyAmenity> AddAsync(PropertyAmenity amenity);
    Task DeleteAsync(Guid id);
    Task SyncAmenitiesAsync(Guid propertyId, IEnumerable<string> amenityNames);
}
