namespace AdvertisingService.Domain.Entities;

public class CategoryImageConfig
{
    public Guid Id { get; set; }
    public string CategoryKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public string Gradient { get; set; } = "from-blue-600 to-blue-800";
    public int VehicleCount { get; set; }
    public bool IsTrending { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; } = true;
    public string Route { get; set; } = string.Empty;
    public Guid? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
