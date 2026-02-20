using AdvertisingService.Domain.Enums;

namespace AdvertisingService.Domain.Entities;

public class RotationConfig
{
    public Guid Id { get; set; }
    public AdPlacementType Section { get; set; }
    public RotationAlgorithmType AlgorithmType { get; set; }
    public int RefreshIntervalMinutes { get; set; }
    public int MaxVehiclesShown { get; set; }
    public decimal WeightRemainingBudget { get; set; }
    public decimal WeightCtr { get; set; }
    public decimal WeightQualityScore { get; set; }
    public decimal WeightRecency { get; set; }
    public bool IsActive { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
