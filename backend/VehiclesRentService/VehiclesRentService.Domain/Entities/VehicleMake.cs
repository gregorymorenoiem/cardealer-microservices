namespace VehiclesRentService.Domain.Entities;

/// <summary>
/// Marca de vehículo (Toyota, Honda, Ford, etc.)
/// </summary>
public class VehicleMake
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Country { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsPopular { get; set; } = false;

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<VehicleModel> Models { get; set; } = new List<VehicleModel>();
}
