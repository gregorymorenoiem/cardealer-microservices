using CarDealer.Shared.MultiTenancy;

namespace RealEstateService.Domain.Entities;

/// <summary>
/// Propiedad inmobiliaria (Casa, Apartamento, Terreno, Local Comercial)
/// </summary>
public class Property : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; } // Multi-tenant

    // ========================================
    // INFORMACIÓN BÁSICA
    // ========================================
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PropertyType Type { get; set; }
    public ListingType ListingType { get; set; } // Venta o Renta
    public PropertyStatus Status { get; set; } = PropertyStatus.Draft;

    // ========================================
    // PRECIO
    // ========================================
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? PricePerSqMeter { get; set; }
    public decimal? MaintenanceFee { get; set; } // Cuota de mantenimiento mensual
    public bool IsNegotiable { get; set; }

    // ========================================
    // UBICACIÓN
    // ========================================
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = "México";
    public string? Neighborhood { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // ========================================
    // CARACTERÍSTICAS FÍSICAS
    // ========================================
    public decimal TotalArea { get; set; } // Área total en m²
    public decimal? BuiltArea { get; set; } // Área construida en m²
    public decimal? LotArea { get; set; } // Área de terreno en m²
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int? HalfBathrooms { get; set; }
    public int? ParkingSpaces { get; set; }
    public int? Floor { get; set; } // Para apartamentos
    public int? TotalFloors { get; set; } // Total de pisos del edificio
    public int? YearBuilt { get; set; }

    // ========================================
    // CARACTERÍSTICAS ADICIONALES
    // ========================================
    public bool HasGarden { get; set; }
    public bool HasPool { get; set; }
    public bool HasGym { get; set; }
    public bool HasElevator { get; set; }
    public bool HasSecurity { get; set; }
    public bool IsFurnished { get; set; }
    public bool AllowsPets { get; set; }

    // ========================================
    // SELLER/AGENTE
    // ========================================
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string? SellerPhone { get; set; }
    public string? SellerEmail { get; set; }

    // ========================================
    // METADATOS
    // ========================================
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }
    public int FavoriteCount { get; set; }
    public int InquiryCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public bool IsDeleted { get; set; }

    // ========================================
    // NAVEGACIÓN
    // ========================================
    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public ICollection<PropertyAmenity> Amenities { get; set; } = new List<PropertyAmenity>();
}

/// <summary>
/// Tipo de propiedad
/// </summary>
public enum PropertyType
{
    House,           // Casa
    Apartment,       // Apartamento/Departamento
    Condo,           // Condominio
    Townhouse,       // Casa adosada
    Land,            // Terreno
    Commercial,      // Local comercial
    Office,          // Oficina
    Warehouse,       // Bodega
    Building         // Edificio completo
}

/// <summary>
/// Tipo de listado
/// </summary>
public enum ListingType
{
    Sale,            // En venta
    Rent,            // En renta
    SaleOrRent       // Venta o renta
}

/// <summary>
/// Estado de la propiedad
/// </summary>
public enum PropertyStatus
{
    Draft,           // Borrador
    Active,          // Activa/Publicada
    Pending,         // Pendiente de revisión
    Sold,            // Vendida
    Rented,          // Rentada
    Reserved,        // Reservada
    Archived         // Archivada
}

/// <summary>
/// Imágenes de la propiedad
/// </summary>
public class PropertyImage : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public Guid PropertyId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? Caption { get; set; }
    public ImageCategory Category { get; set; } = ImageCategory.General;
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Property? Property { get; set; }
}

/// <summary>
/// Categoría de imagen
/// </summary>
public enum ImageCategory
{
    General,
    Exterior,
    Interior,
    Kitchen,
    Bedroom,
    Bathroom,
    LivingRoom,
    DiningRoom,
    Garden,
    Pool,
    Garage,
    View,
    FloorPlan
}

/// <summary>
/// Amenidades de la propiedad
/// </summary>
public class PropertyAmenity : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public Guid PropertyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AmenityCategory Category { get; set; }
    public string? Icon { get; set; }

    public Property? Property { get; set; }
}

/// <summary>
/// Categoría de amenidad
/// </summary>
public enum AmenityCategory
{
    Security,        // Seguridad
    Recreation,      // Recreación
    Comfort,         // Comodidad
    Services,        // Servicios
    Outdoor,         // Exteriores
    Technology       // Tecnología
}
