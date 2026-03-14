using AdminService.Domain.Entities.Advertising;

namespace AdminService.Application.Interfaces;

/// <summary>
/// Service interface for the OKLA advertising system.
/// Manages campaigns, rotation, tracking, homepage config, reports, and pricing.
/// </summary>
public interface IAdvertisingService
{
    // === Campaigns ===
    Task<AdCampaignEntity> CreateCampaignAsync(CreateCampaignDto dto);
    Task<AdCampaignEntity?> GetCampaignByIdAsync(Guid id);
    Task<(IReadOnlyList<AdCampaignEntity> Items, int TotalCount)> GetCampaignsByOwnerAsync(
        string ownerId, string ownerType, CampaignStatus? status, int page, int pageSize);
    Task<bool> PauseCampaignAsync(Guid id);
    Task<bool> ResumeCampaignAsync(Guid id);
    Task<bool> CancelCampaignAsync(Guid id);

    // === Rotation ===
    Task<HomepageRotationDto?> GetHomepageRotationAsync(AdPlacementType section);
    Task<RotationConfigEntity?> GetRotationConfigAsync(AdPlacementType section);
    Task<RotationConfigEntity> UpdateRotationConfigAsync(UpdateRotationConfigDto dto);
    Task RefreshRotationAsync(AdPlacementType? section);

    // === Tracking ===
    Task RecordImpressionAsync(TrackingEventDto dto);
    Task RecordClickAsync(TrackingEventDto dto);

    // === Homepage Config ===
    Task<IReadOnlyList<CategoryImageConfigEntity>> GetCategoriesAsync(bool includeHidden);
    Task<CategoryImageConfigEntity> UpdateCategoryAsync(UpdateCategoryDto dto);
    Task<IReadOnlyList<BrandConfigEntity>> GetBrandsAsync(bool includeHidden);
    Task<BrandConfigEntity> UpdateBrandAsync(UpdateBrandDto dto);

    // === Reports ===
    Task<CampaignReportDto> GetCampaignReportAsync(Guid campaignId, int daysBack);
    Task<PlatformReportDto> GetPlatformReportAsync(int daysBack);
    Task<OwnerReportDto> GetOwnerReportAsync(string ownerId, string ownerType, int daysBack);

    // === Pricing ===
    Task<PricingEstimateDto> GetPricingEstimateAsync(AdPlacementType placementType);
}

// === DTOs ===

public record CreateCampaignDto(
    string? Name,
    string OwnerId,
    string OwnerType,
    string? VehicleId,
    string[]? VehicleIds,
    AdPlacementType PlacementType,
    CampaignPricingModel PricingModel,
    decimal TotalBudget,
    decimal? DailyBudget,
    decimal? BidAmount,
    DateTime StartDate,
    DateTime EndDate,
    // Vehicle metadata (optional, for pre-populating rotation cache)
    string? VehicleTitle = null,
    string? VehicleSlug = null,
    string? VehicleImageUrl = null,
    decimal? VehiclePrice = null,
    string? VehicleCurrency = null,
    string? VehicleLocation = null
);

public record HomepageRotationDto(
    AdPlacementType Section,
    IReadOnlyList<RotatedVehicleDto> Items,
    DateTime GeneratedAt,
    DateTime NextRotationAt
);

public record RotatedVehicleDto(
    string VehicleId,
    string CampaignId,
    int Position,
    double QualityScore,
    string? Title,
    string? Slug,
    string? ImageUrl,
    decimal? Price,
    string? Currency,
    string? Location,
    bool? IsFeatured,
    bool? IsPremium
);

public record UpdateRotationConfigDto(
    AdPlacementType Section,
    RotationAlgorithmType? Algorithm,
    int? MaxSlots,
    int? RotationIntervalMinutes,
    double? MinQualityScore,
    bool? IsActive
);

public record TrackingEventDto(
    string CampaignId,
    string VehicleId,
    AdPlacementType Section,
    string? UserId,
    string? Ip,
    string? DestinationUrl
);

public record UpdateCategoryDto(
    string CategoryKey,
    string? DisplayName,
    string? ImageUrl,
    string? Description,
    string? Href,
    string? AccentColor,
    int? DisplayOrder,
    bool? IsActive,
    bool? IsTrending
);

public record UpdateBrandDto(
    string BrandKey,
    string? DisplayName,
    string? LogoUrl,
    int? VehicleCount,
    int? DisplayOrder,
    bool? IsActive
);

public record CampaignReportDto(
    string CampaignId,
    long TotalViews,
    long TotalClicks,
    double Ctr,
    decimal TotalSpent,
    decimal RemainingBudget,
    IReadOnlyList<DailyDataPointDto> DailyData
);

public record OwnerReportDto(
    string OwnerId,
    string OwnerType,
    int ActiveCampaigns,
    int TotalCampaigns,
    long TotalImpressions,
    long TotalClicks,
    double OverallCtr,
    decimal TotalSpent,
    IReadOnlyList<DailyDataPointDto> DailyImpressions,
    IReadOnlyList<DailyDataPointDto> DailyClicks
);

public record PlatformReportDto(
    int TotalActiveCampaigns,
    decimal TotalRevenue,
    double AverageCtr,
    long TotalImpressions,
    long TotalClicks,
    IReadOnlyList<DailyDataPointDto> DailyData
);

public record DailyDataPointDto(string Date, long Views, long Clicks, decimal Spent);

public record PricingEstimateDto(
    AdPlacementType PlacementType,
    IReadOnlyList<PricingModelOptionDto> PricingModels
);

public record PricingModelOptionDto(
    CampaignPricingModel Model,
    decimal PricePerUnit,
    string Currency,
    string Description,
    int EstimatedDailyViews
);
