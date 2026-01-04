using CarDealer.Shared.MultiTenancy;

namespace VehiclesSaleService.Domain.Entities;

/// <summary>
/// Categoría de vehículos (Cars, Trucks, SUVs, Motorcycles, Boats, RVs)
/// </summary>
public class Category : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; } // Para categorías personalizadas por dealer
    public Guid? ParentId { get; set; } // Para categorías jerárquicas

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public int Level { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsSystem { get; set; } = false; // True para categorías predefinidas

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
