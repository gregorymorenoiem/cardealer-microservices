namespace BillingService.Application.DTOs;

public record SubscriptionDto(
    Guid Id,
    Guid DealerId,
    string Plan,
    string Status,
    string Cycle,
    decimal PricePerCycle,
    string Currency,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime? TrialEndDate,
    DateTime? NextBillingDate,
    string? StripeCustomerId,
    string? StripeSubscriptionId,
    int MaxUsers,
    int MaxVehicles,
    string? Features,
    DateTime CreatedAt
);

public record CreateSubscriptionRequest(
    string Plan,
    string Cycle,
    decimal PricePerCycle,
    int MaxUsers,
    int MaxVehicles,
    int TrialDays = 0,
    string? Features = null
);

public record UpgradeSubscriptionRequest(
    string Plan,
    decimal NewPrice,
    int NewMaxUsers,
    int NewMaxVehicles
);

public record ChangeBillingCycleRequest(
    string Cycle,
    decimal NewPrice
);
