using BillingService.Domain.Entities;

namespace BillingService.Application.DTOs;

/// <summary>
/// Request DTO for recording acquisition tracking for a new dealer.
/// Extracted from Infrastructure to Application layer (Clean Architecture fix).
/// </summary>
public class RecordAcquisitionRequest
{
    public Guid DealerId { get; set; }
    public AcquisitionChannel Channel { get; set; }
    public DateTime? RegisteredAt { get; set; }
    public string? CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public string? UtmContent { get; set; }
    public string? UtmTerm { get; set; }
    public decimal AcquisitionCostUsd { get; set; }
    public Guid? ReferredByDealerId { get; set; }
    public string? ReferralCode { get; set; }
    public string? LandingPage { get; set; }
    public string Country { get; set; } = "DO";
}

/// <summary>
/// Request DTO for marking a dealer as converted to a paid plan.
/// Extracted from Infrastructure to Application layer (Clean Architecture fix).
/// </summary>
public class MarkConvertedRequest
{
    public SubscriptionPlan Plan { get; set; }
}

/// <summary>
/// Request DTO for recording or updating monthly marketing spend.
/// Extracted from Infrastructure to Application layer (Clean Architecture fix).
/// </summary>
public class RecordMarketingSpendRequest
{
    public int Year { get; set; }
    public int Month { get; set; }
    public AcquisitionChannel Channel { get; set; }
    public decimal SpendUsd { get; set; }
    public string? CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public long Impressions { get; set; }
    public long Clicks { get; set; }
    public int Signups { get; set; }
    public int PaidConversions { get; set; }
    public string? Notes { get; set; }
}
