namespace AdvertisingService.Domain.Entities;

public class BrandConfig
{
    public Guid Id { get; set; }
    public string BrandKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string LogoInitials { get; set; } = string.Empty;
    public int VehicleCount { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; } = true;
    public string Route { get; set; } = string.Empty;
    public Guid? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
