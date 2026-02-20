using AdvertisingService.Domain.Enums;

namespace AdvertisingService.Domain.Entities;

public class AdCampaign
{
    public Guid Id { get; private set; }
    public Guid VehicleId { get; private set; }
    public Guid OwnerId { get; private set; }
    public string OwnerType { get; private set; } = string.Empty;
    public AdPlacementType PlacementType { get; private set; }
    public CampaignPricingModel PricingModel { get; private set; }
    public decimal? PricePerView { get; private set; }
    public decimal? FixedPrice { get; private set; }
    public decimal TotalBudget { get; private set; }
    public decimal SpentBudget { get; private set; }
    public int? TotalViewsPurchased { get; private set; }
    public int ViewsConsumed { get; private set; }
    public int Clicks { get; private set; }
    public CampaignStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public Guid? BillingReferenceId { get; private set; }
    public decimal QualityScore { get; private set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private AdCampaign() { }

    public static AdCampaign Create(
        Guid vehicleId,
        Guid ownerId,
        string ownerType,
        AdPlacementType placementType,
        CampaignPricingModel pricingModel,
        decimal totalBudget,
        decimal? pricePerView,
        decimal? fixedPrice,
        int? totalViewsPurchased,
        DateTime startDate,
        DateTime? endDate,
        decimal qualityScore)
    {
        return new AdCampaign
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            OwnerId = ownerId,
            OwnerType = ownerType,
            PlacementType = placementType,
            PricingModel = pricingModel,
            PricePerView = pricePerView,
            FixedPrice = fixedPrice,
            TotalBudget = totalBudget,
            SpentBudget = 0m,
            TotalViewsPurchased = totalViewsPurchased,
            ViewsConsumed = 0,
            Clicks = 0,
            Status = CampaignStatus.PendingPayment,
            StartDate = startDate,
            EndDate = endDate,
            QualityScore = qualityScore,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Activate(Guid billingReferenceId)
    {
        if (Status != CampaignStatus.PendingPayment)
            throw new InvalidOperationException($"Cannot activate campaign in status {Status}.");

        BillingReferenceId = billingReferenceId;
        Status = CampaignStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordView()
    {
        if (!IsActive()) return;

        ViewsConsumed++;

        if (PricingModel == CampaignPricingModel.PerView && PricePerView.HasValue)
        {
            SpentBudget += PricePerView.Value;

            if (TotalViewsPurchased.HasValue && ViewsConsumed >= TotalViewsPurchased.Value)
            {
                Status = CampaignStatus.Completed;
            }
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordClick()
    {
        Clicks++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pause()
    {
        if (Status != CampaignStatus.Active)
            throw new InvalidOperationException($"Cannot pause campaign in status {Status}.");

        Status = CampaignStatus.Paused;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Resume()
    {
        if (Status != CampaignStatus.Paused)
            throw new InvalidOperationException($"Cannot resume campaign in status {Status}.");

        Status = CampaignStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is CampaignStatus.Completed or CampaignStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel campaign in status {Status}.");

        Status = CampaignStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkExpired()
    {
        if (Status is CampaignStatus.Active or CampaignStatus.Paused)
        {
            Status = CampaignStatus.Expired;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateQualityScore(decimal score)
    {
        QualityScore = Math.Clamp(score, 0m, 1.00m);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActive() => Status == CampaignStatus.Active && !IsExpired();

    public bool IsExpired() => EndDate.HasValue && EndDate.Value < DateTime.UtcNow;

    public decimal GetCtr() => ViewsConsumed > 0 ? (decimal)Clicks / ViewsConsumed : 0m;

    public decimal GetRemainingBudgetRatio() => TotalBudget > 0 ? 1m - (SpentBudget / TotalBudget) : 0m;
}
