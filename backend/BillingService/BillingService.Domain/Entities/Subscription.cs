using CarDealer.Shared.MultiTenancy;

namespace BillingService.Domain.Entities;

public enum SubscriptionPlan
{
    Free,
    Basic,
    Professional,
    Enterprise,
    Custom
}

public enum SubscriptionStatus
{
    Trial,
    Active,
    PastDue,
    Cancelled,
    Suspended,
    Expired
}

public enum BillingCycle
{
    Monthly,
    Quarterly,
    Yearly
}

public class Subscription : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }

    public SubscriptionPlan Plan { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public BillingCycle Cycle { get; private set; }

    public decimal PricePerCycle { get; private set; }
    public string Currency { get; private set; } = "USD";

    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? TrialEndDate { get; private set; }
    public DateTime? NextBillingDate { get; private set; }

    public string? StripeCustomerId { get; private set; }
    public string? StripeSubscriptionId { get; private set; }
    public string? StripePaymentMethodId { get; private set; }

    public int MaxUsers { get; private set; }
    public int MaxVehicles { get; private set; }
    public string? Features { get; private set; } // JSON

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    // ✅ AUDIT FIX: Concurrency control
    public string ConcurrencyStamp { get; private set; } = Guid.NewGuid().ToString();

    // ✅ AUDIT FIX: Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // ✅ AUDIT FIX: Navigation properties for FK relationships
    public ICollection<Payment> Payments { get; private set; } = new List<Payment>();
    public ICollection<Invoice> Invoices { get; private set; } = new List<Invoice>();

    private Subscription() { }

    public Subscription(
        Guid dealerId,
        SubscriptionPlan plan,
        BillingCycle cycle,
        decimal pricePerCycle,
        int maxUsers,
        int maxVehicles,
        int trialDays = 0)
    {
        if (pricePerCycle < 0)
            throw new ArgumentException("Price cannot be negative", nameof(pricePerCycle));

        Id = Guid.NewGuid();
        DealerId = dealerId;
        Plan = plan;
        Cycle = cycle;
        PricePerCycle = pricePerCycle;
        MaxUsers = maxUsers;
        MaxVehicles = maxVehicles;
        StartDate = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;

        if (trialDays > 0)
        {
            Status = SubscriptionStatus.Trial;
            TrialEndDate = DateTime.UtcNow.AddDays(trialDays);
            NextBillingDate = TrialEndDate;
        }
        else
        {
            Status = SubscriptionStatus.Active;
            CalculateNextBillingDate();
        }
    }

    public void SetStripeInfo(string customerId, string subscriptionId, string? paymentMethodId = null)
    {
        StripeCustomerId = customerId;
        StripeSubscriptionId = subscriptionId;
        StripePaymentMethodId = paymentMethodId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetFeatures(string features)
    {
        Features = features;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = SubscriptionStatus.Active;
        CalculateNextBillingDate();
        UpdatedAt = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }

    public void MarkPastDue()
    {
        Status = SubscriptionStatus.PastDue;
        UpdatedAt = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }

    public void Suspend()
    {
        Status = SubscriptionStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }

    public void Cancel(string reason)
    {
        Status = SubscriptionStatus.Cancelled;
        CancellationReason = reason;
        CancelledAt = DateTime.UtcNow;
        EndDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }

    public void Upgrade(SubscriptionPlan newPlan, decimal newPrice, int newMaxUsers, int newMaxVehicles)
    {
        Plan = newPlan;
        PricePerCycle = newPrice;
        MaxUsers = newMaxUsers;
        MaxVehicles = newMaxVehicles;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeBillingCycle(BillingCycle newCycle, decimal newPrice)
    {
        Cycle = newCycle;
        PricePerCycle = newPrice;
        CalculateNextBillingDate();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RenewBilling()
    {
        CalculateNextBillingDate();
        Status = SubscriptionStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    private void CalculateNextBillingDate()
    {
        var baseDate = NextBillingDate ?? DateTime.UtcNow;
        NextBillingDate = Cycle switch
        {
            BillingCycle.Monthly => baseDate.AddMonths(1),
            BillingCycle.Quarterly => baseDate.AddMonths(3),
            BillingCycle.Yearly => baseDate.AddYears(1),
            _ => baseDate.AddMonths(1)
        };
    }
}
