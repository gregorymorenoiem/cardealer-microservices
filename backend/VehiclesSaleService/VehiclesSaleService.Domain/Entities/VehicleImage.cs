using CarDealer.Shared.MultiTenancy;

namespace VehiclesSaleService.Domain.Entities;

/// <summary>
/// Imagen de vehículo
/// </summary>
public class VehicleImage : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public Guid VehicleId { get; set; }

    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? Caption { get; set; }
    public ImageType ImageType { get; set; } = ImageType.Exterior;
    public int SortOrder { get; set; } = 0;
    public bool IsPrimary { get; set; } = false;
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Vehicle? Vehicle { get; set; }
}
