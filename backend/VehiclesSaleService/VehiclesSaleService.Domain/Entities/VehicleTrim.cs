namespace VehiclesSaleService.Domain.Entities;

/// <summary>
/// Versión/Trim del modelo (LE, SE, XLE, Sport, etc.)
/// </summary>
public class VehicleTrim
{
    public Guid Id { get; set; }
    public Guid ModelId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal? BaseMSRP { get; set; }

    // Engine specs
    public string? EngineSize { get; set; }
    public int? Horsepower { get; set; }
    public int? Torque { get; set; }
    public FuelType? FuelType { get; set; }
    public TransmissionType? Transmission { get; set; }
    public DriveType? DriveType { get; set; }

    // Fuel economy
    public int? MpgCity { get; set; }
    public int? MpgHighway { get; set; }
    public int? MpgCombined { get; set; }

    public bool IsActive { get; set; } = true;

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public VehicleModel? Model { get; set; }
}
