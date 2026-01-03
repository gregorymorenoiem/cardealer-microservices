using PropertiesSaleService.Domain.Entities;

namespace PropertiesSaleService.Domain.Interfaces;

public interface IPropertyRepository
{
    Task<Property?> GetByIdAsync(Guid id);
    Task<Property?> GetByMLSNumberAsync(string mlsNumber);
    Task<IEnumerable<Property>> GetAllAsync(int skip = 0, int take = 100);
    Task<IEnumerable<Property>> SearchAsync(PropertySearchParameters parameters);
    Task<IEnumerable<Property>> GetBySellerAsync(Guid sellerId);
    Task<IEnumerable<Property>> GetByDealerAsync(Guid dealerId);
    Task<IEnumerable<Property>> GetFeaturedAsync(int take = 10);
    Task<Property> CreateAsync(Property property);
    Task UpdateAsync(Property property);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetCountAsync(PropertySearchParameters? parameters = null);
}

public class PropertySearchParameters
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public PropertyType? PropertyType { get; set; }
    public PropertySubType? PropertySubType { get; set; }
    public int? MinBedrooms { get; set; }
    public int? MaxBedrooms { get; set; }
    public decimal? MinBathrooms { get; set; }
    public decimal? MaxBathrooms { get; set; }
    public int? MinSquareFeet { get; set; }
    public int? MaxSquareFeet { get; set; }
    public int? MinYearBuilt { get; set; }
    public int? MaxYearBuilt { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public string? Neighborhood { get; set; }
    public bool? HasPool { get; set; }
    public bool? HasGarage { get; set; }
    public bool? HasBasement { get; set; }
    public bool? HasFireplace { get; set; }
    public HeatingType? HeatingType { get; set; }
    public CoolingType? CoolingType { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
