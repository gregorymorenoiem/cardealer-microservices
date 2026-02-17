using CarDealer.Shared.MultiTenancy;

namespace PropertiesRentService.Domain.Entities;

/// <summary>
/// Categoría de propiedades (Houses, Apartments, Condos, Land, Commercial)
/// </summary>
public class Category : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; } // Para categorías personalizadas por agencia
    public Guid? ParentId { get; set; } // Para categorías jerárquicas

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsSystem { get; set; } = false; // True para categorías predefinidas

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
