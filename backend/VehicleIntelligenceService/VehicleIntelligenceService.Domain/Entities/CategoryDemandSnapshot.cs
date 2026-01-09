namespace VehicleIntelligenceService.Domain.Entities;

public class CategoryDemandSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Category { get; set; } = string.Empty;
    public int DemandScore { get; set; }
    public string Trend { get; set; } = "stable"; // up | down | stable
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
