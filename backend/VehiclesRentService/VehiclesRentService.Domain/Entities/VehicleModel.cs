namespace VehiclesRentService.Domain.Entities;

/// <summary>
/// Modelo de vehículo (Camry, Civic, F-150, etc.)
/// </summary>
public class VehicleModel
{
    public Guid Id { get; set; }
    public Guid MakeId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; } = VehicleType.Car;
    public BodyStyle? DefaultBodyStyle { get; set; }
    public int? StartYear { get; set; }
    public int? EndYear { get; set; } // null = still in production
    public bool IsActive { get; set; } = true;
    public bool IsPopular { get; set; } = false;

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public VehicleMake? Make { get; set; }
    public ICollection<VehicleTrim> Trims { get; set; } = new List<VehicleTrim>();
}
