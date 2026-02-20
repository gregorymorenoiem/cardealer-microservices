using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Enums;

namespace AdvertisingService.Domain.Interfaces;

public interface IAdRotationEngine
{
    Task<HomepageRotationResult> ComputeRotationAsync(AdPlacementType section, RotationConfig config, CancellationToken ct = default);
}

public class HomepageRotationResult
{
    public AdPlacementType Section { get; set; }
    public List<RotatedVehicleItem> Vehicles { get; set; } = new();
    public RotationAlgorithmType AlgorithmUsed { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime NextRefreshAt { get; set; }
}

public class RotatedVehicleItem
{
    public Guid VehicleId { get; set; }
    public Guid? CampaignId { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerType { get; set; } = string.Empty;
    public bool IsPaidAd { get; set; }
    public int Position { get; set; }
    public decimal Score { get; set; }
    public AdPlacementType PlacementType { get; set; }
}
