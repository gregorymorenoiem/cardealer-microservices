namespace SearchService.Application.DTOs;

/// <summary>
/// DTO para indexar propiedades inmobiliarias en Elasticsearch
/// </summary>
public class PropertySearchDocument
{
    public string Id { get; set; } = string.Empty;
    public Guid DealerId { get; set; }

    // Texto para búsqueda
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Tipo y categoría
    public string PropertyType { get; set; } = string.Empty; // house, apartment, condo, land, commercial
    public string ListingType { get; set; } = string.Empty;  // sale, rent, sale-or-rent
    public string Status { get; set; } = "active";

    // Precios
    public decimal Price { get; set; }
    public string Currency { get; set; } = "MXN";
    public decimal? PricePerSqMeter { get; set; }
    public decimal? MaintenanceFee { get; set; }
    public bool IsNegotiable { get; set; }

    // Características físicas
    public double TotalArea { get; set; }
    public string AreaUnit { get; set; } = "sqm";
    public double? BuiltArea { get; set; }
    public double? LotArea { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int? HalfBathrooms { get; set; }
    public int? ParkingSpaces { get; set; }
    public int? Floor { get; set; }
    public int? TotalFloors { get; set; }
    public int? YearBuilt { get; set; }

    // Amenidades booleanas
    public bool HasGarden { get; set; }
    public bool HasPool { get; set; }
    public bool HasGym { get; set; }
    public bool HasElevator { get; set; }
    public bool HasSecurity { get; set; }
    public bool IsFurnished { get; set; }
    public bool AllowsPets { get; set; }

    // Amenidades adicionales
    public List<string> Amenities { get; set; } = new();

    // Ubicación
    public PropertyLocationDocument Location { get; set; } = new();

    // Imágenes
    public int ImageCount { get; set; }
    public string? PrimaryImageUrl { get; set; }

    // Seller/Agent
    public PropertySellerDocument Seller { get; set; } = new();

    // Métricas
    public int ViewCount { get; set; }
    public int FavoriteCount { get; set; }
    public int InquiryCount { get; set; }
    public bool IsFeatured { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
}

public class PropertyLocationDocument
{
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = "México";
    public string? Neighborhood { get; set; }
    public GeoPoint? Coordinates { get; set; }
}

public class GeoPoint
{
    public double Lat { get; set; }
    public double Lon { get; set; }
}

public class PropertySellerDocument
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public bool IsDealership { get; set; }
}
