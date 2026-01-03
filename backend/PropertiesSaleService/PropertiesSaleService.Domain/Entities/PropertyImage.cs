using CarDealer.Shared.MultiTenancy;

namespace PropertiesSaleService.Domain.Entities;

/// <summary>
/// Imagen de propiedad
/// </summary>
public class PropertyImage : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public Guid PropertyId { get; set; }

    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? Title { get; set; }
    public string? AltText { get; set; }
    public PropertyImageType ImageType { get; set; } = PropertyImageType.Exterior;
    public int SortOrder { get; set; } = 0;
    public bool IsPrimary { get; set; } = false;
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Property? Property { get; set; }
}

public enum PropertyImageType
{
    Exterior,
    Interior,
    Kitchen,
    Bathroom,
    Bedroom,
    LivingRoom,
    DiningRoom,
    Backyard,
    Pool,
    Garage,
    FloorPlan,
    Aerial,
    Other
}
