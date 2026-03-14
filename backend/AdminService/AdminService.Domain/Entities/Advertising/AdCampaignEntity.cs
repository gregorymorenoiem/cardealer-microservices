namespace AdminService.Domain.Entities.Advertising;

public enum AdPlacementType { FeaturedSpot, PremiumSpot }

public enum CampaignStatus { PendingPayment, Active, Paused, Cancelled, Completed, Expired }

public enum CampaignPricingModel { PerView, PerClick, PerDay, FixedMonthly, FlatFee }

public enum RotationAlgorithmType { WeightedRandom, RoundRobin, CTROptimized, BudgetPriority }

public class AdCampaignEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerType { get; set; } = "Individual";
    public string VehicleId { get; set; } = string.Empty;
    public AdPlacementType PlacementType { get; set; }
    public CampaignPricingModel PricingModel { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal TotalBudget { get; set; }
    public decimal RemainingBudget { get; set; }
    public CampaignStatus Status { get; set; } = CampaignStatus.Active;
    public double QualityScore { get; set; } = 50.0;
    public long TotalViews { get; set; }
    public long TotalClicks { get; set; }
    public double Ctr => TotalViews > 0 ? Math.Round((double)TotalClicks / TotalViews * 100, 2) : 0;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Vehicle metadata cached for rotation
    public string? VehicleTitle { get; set; }
    public string? VehicleSlug { get; set; }
    public string? VehicleImageUrl { get; set; }
    public decimal? VehiclePrice { get; set; }
    public string? VehicleCurrency { get; set; }
    public string? VehicleLocation { get; set; }
}

public class RotationConfigEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public AdPlacementType Section { get; set; }
    public RotationAlgorithmType Algorithm { get; set; } = RotationAlgorithmType.WeightedRandom;
    public int MaxSlots { get; set; } = 12;
    public int RotationIntervalMinutes { get; set; } = 30;
    public double MinQualityScore { get; set; } = 10.0;
    public bool IsActive { get; set; } = true;
}

public class CategoryImageConfigEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CategoryKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Href { get; set; } = string.Empty;
    public string AccentColor { get; set; } = "blue";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsTrending { get; set; }
}

public class BrandConfigEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BrandKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public int VehicleCount { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class TrackingEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CampaignId { get; set; } = string.Empty;
    public string VehicleId { get; set; } = string.Empty;
    public AdPlacementType Section { get; set; }
    public string EventType { get; set; } = string.Empty; // "impression" or "click"
    public string? UserId { get; set; }
    public string? Ip { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
