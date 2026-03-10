using CarDealer.Shared.MultiTenancy;

namespace BillingService.Domain.Entities;

// ═══════════════════════════════════════════════════════════════════════════════
// RECONCILIATION REPORT ENTITY — CONTRA #6 FIX
//
// Stores the result of each daily Stripe↔OKLA reconciliation run.
// Tracks:
//   1. Payments received in Stripe without active subscription in OKLA
//   2. Active subscriptions in OKLA without payment registered in Stripe
//   3. Invoice amount mismatches between Stripe and OKLA
//   4. Orphaned Stripe customers without OKLA dealer records
//
// Each run produces a report with discrepancies that require admin review.
// ═══════════════════════════════════════════════════════════════════════════════

public enum ReconciliationStatus
{
    /// <summary>Reconciliation is currently running.</summary>
    InProgress,

    /// <summary>Completed with no discrepancies found.</summary>
    Clean,

    /// <summary>Completed with discrepancies that need review.</summary>
    DiscrepanciesFound,

    /// <summary>All discrepancies have been reviewed and resolved.</summary>
    Resolved,

    /// <summary>Reconciliation failed (Stripe API error, DB error, etc.).</summary>
    Failed
}

public enum DiscrepancyType
{
    /// <summary>Payment in Stripe but no matching record in OKLA DB.</summary>
    PaymentWithoutRecord,

    /// <summary>Active subscription in OKLA but no successful payment in the billing period.</summary>
    SubscriptionWithoutPayment,

    /// <summary>Invoice amount in Stripe differs from OKLA DB amount.</summary>
    AmountMismatch,

    /// <summary>Stripe customer exists but no corresponding dealer in OKLA.</summary>
    OrphanedStripeCustomer,

    /// <summary>OKLA subscription status differs from Stripe subscription status.</summary>
    StatusMismatch,

    /// <summary>Payment in OKLA marked as succeeded but Stripe shows failed/refunded.</summary>
    PaymentStatusMismatch,

    /// <summary>Stripe subscription exists without corresponding OKLA subscription.</summary>
    StripeSubscriptionWithoutRecord
}

public enum DiscrepancySeverity
{
    /// <summary>Low impact — informational, can be auto-resolved.</summary>
    Low,

    /// <summary>Medium impact — needs review but no revenue loss.</summary>
    Medium,

    /// <summary>High impact — potential revenue loss or billing error.</summary>
    High,

    /// <summary>Critical — immediate attention required.</summary>
    Critical
}

/// <summary>
/// A single reconciliation report covering one billing period.
/// Contains all discrepancies found during the reconciliation run.
/// </summary>
public class ReconciliationReport
{
    public Guid Id { get; private set; }

    /// <summary>Period being reconciled (YYYY-MM format).</summary>
    public string Period { get; private set; } = string.Empty;

    /// <summary>When this reconciliation run started.</summary>
    public DateTime StartedAt { get; private set; }

    /// <summary>When this reconciliation run completed.</summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>Current status of the reconciliation.</summary>
    public ReconciliationStatus Status { get; private set; }

    /// <summary>Total subscriptions checked in OKLA.</summary>
    public int TotalSubscriptionsChecked { get; set; }

    /// <summary>Total payments checked in Stripe.</summary>
    public int TotalPaymentsChecked { get; set; }

    /// <summary>Total invoices checked.</summary>
    public int TotalInvoicesChecked { get; set; }

    /// <summary>Number of discrepancies found.</summary>
    public int DiscrepancyCount { get; set; }

    /// <summary>Total dollar amount of discrepancies (potential revenue impact).</summary>
    public decimal TotalDiscrepancyAmount { get; set; }

    /// <summary>Number of auto-resolved discrepancies.</summary>
    public int AutoResolvedCount { get; set; }

    /// <summary>Error message if reconciliation failed.</summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>Who triggered this reconciliation (system = daily job, admin = manual).</summary>
    public string TriggeredBy { get; private set; } = "system";

    public DateTime CreatedAt { get; private set; }

    /// <summary>All discrepancies found in this reconciliation run.</summary>
    public ICollection<ReconciliationDiscrepancy> Discrepancies { get; private set; }
        = new List<ReconciliationDiscrepancy>();

    private ReconciliationReport() { }

    public ReconciliationReport(string period, string triggeredBy = "system")
    {
        Id = Guid.NewGuid();
        Period = period;
        TriggeredBy = triggeredBy;
        Status = ReconciliationStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        CompletedAt = DateTime.UtcNow;
        Status = DiscrepancyCount > 0
            ? ReconciliationStatus.DiscrepanciesFound
            : ReconciliationStatus.Clean;
    }

    public void Fail(string errorMessage)
    {
        CompletedAt = DateTime.UtcNow;
        Status = ReconciliationStatus.Failed;
        ErrorMessage = errorMessage;
    }

    public void MarkResolved()
    {
        Status = ReconciliationStatus.Resolved;
    }

    public void AddDiscrepancy(ReconciliationDiscrepancy discrepancy)
    {
        Discrepancies.Add(discrepancy);
        DiscrepancyCount++;
        TotalDiscrepancyAmount += discrepancy.AmountDifference;
    }
}

/// <summary>
/// A single discrepancy found during reconciliation.
/// </summary>
public class ReconciliationDiscrepancy
{
    public Guid Id { get; private set; }
    public Guid ReportId { get; private set; }

    /// <summary>Type of discrepancy found.</summary>
    public DiscrepancyType Type { get; private set; }

    /// <summary>Severity level based on potential revenue impact.</summary>
    public DiscrepancySeverity Severity { get; private set; }

    /// <summary>Dealer affected by this discrepancy.</summary>
    public Guid? DealerId { get; private set; }

    /// <summary>Stripe Payment Intent ID (if applicable).</summary>
    public string? StripePaymentIntentId { get; private set; }

    /// <summary>Stripe Subscription ID (if applicable).</summary>
    public string? StripeSubscriptionId { get; private set; }

    /// <summary>Stripe Invoice ID (if applicable).</summary>
    public string? StripeInvoiceId { get; private set; }

    /// <summary>Stripe Customer ID (if applicable).</summary>
    public string? StripeCustomerId { get; private set; }

    /// <summary>OKLA Payment record ID (if exists).</summary>
    public Guid? OklaPaymentId { get; private set; }

    /// <summary>OKLA Subscription record ID (if exists).</summary>
    public Guid? OklaSubscriptionId { get; private set; }

    /// <summary>Amount in Stripe (cents converted to USD).</summary>
    public decimal StripeAmount { get; private set; }

    /// <summary>Amount in OKLA DB (USD).</summary>
    public decimal OklaAmount { get; private set; }

    /// <summary>Absolute difference between Stripe and OKLA amounts.</summary>
    public decimal AmountDifference { get; private set; }

    /// <summary>Human-readable description of the discrepancy.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Suggested resolution action.</summary>
    public string SuggestedAction { get; private set; } = string.Empty;

    /// <summary>Whether this discrepancy was auto-resolved.</summary>
    public bool IsAutoResolved { get; private set; }

    /// <summary>Resolution notes (filled when resolved).</summary>
    public string? ResolutionNotes { get; private set; }

    /// <summary>When this discrepancy was resolved.</summary>
    public DateTime? ResolvedAt { get; private set; }

    /// <summary>Who resolved it (admin email or "system").</summary>
    public string? ResolvedBy { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public ReconciliationReport? Report { get; private set; }

    private ReconciliationDiscrepancy() { }

    public ReconciliationDiscrepancy(
        Guid reportId,
        DiscrepancyType type,
        DiscrepancySeverity severity,
        string description,
        string suggestedAction,
        Guid? dealerId = null,
        string? stripePaymentIntentId = null,
        string? stripeSubscriptionId = null,
        string? stripeInvoiceId = null,
        string? stripeCustomerId = null,
        Guid? oklaPaymentId = null,
        Guid? oklaSubscriptionId = null,
        decimal stripeAmount = 0,
        decimal oklaAmount = 0)
    {
        Id = Guid.NewGuid();
        ReportId = reportId;
        Type = type;
        Severity = severity;
        Description = description;
        SuggestedAction = suggestedAction;
        DealerId = dealerId;
        StripePaymentIntentId = stripePaymentIntentId;
        StripeSubscriptionId = stripeSubscriptionId;
        StripeInvoiceId = stripeInvoiceId;
        StripeCustomerId = stripeCustomerId;
        OklaPaymentId = oklaPaymentId;
        OklaSubscriptionId = oklaSubscriptionId;
        StripeAmount = stripeAmount;
        OklaAmount = oklaAmount;
        AmountDifference = Math.Abs(stripeAmount - oklaAmount);
        CreatedAt = DateTime.UtcNow;
    }

    public void AutoResolve(string notes)
    {
        IsAutoResolved = true;
        ResolutionNotes = notes;
        ResolvedAt = DateTime.UtcNow;
        ResolvedBy = "system";
    }

    public void Resolve(string resolvedBy, string notes)
    {
        ResolutionNotes = notes;
        ResolvedAt = DateTime.UtcNow;
        ResolvedBy = resolvedBy;
    }
}
