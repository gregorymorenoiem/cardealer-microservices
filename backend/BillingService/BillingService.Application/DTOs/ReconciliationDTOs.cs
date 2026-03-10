using BillingService.Domain.Entities;

namespace BillingService.Application.DTOs;

// ═══════════════════════════════════════════════════════════════════════════════
// RECONCILIATION DTOs — CONTRA #6 FIX
//
// DTOs for the payment reconciliation audit system.
// Used by the API endpoints and the MediatR handlers.
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Summary of a reconciliation report for list views.
/// </summary>
public sealed class ReconciliationReportSummaryDto
{
    public Guid Id { get; set; }
    public string Period { get; set; } = string.Empty;
    public ReconciliationStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalSubscriptionsChecked { get; set; }
    public int TotalPaymentsChecked { get; set; }
    public int DiscrepancyCount { get; set; }
    public decimal TotalDiscrepancyAmount { get; set; }
    public int AutoResolvedCount { get; set; }
    public string TriggeredBy { get; set; } = "system";
    public TimeSpan? Duration => CompletedAt.HasValue
        ? CompletedAt.Value - StartedAt
        : null;
}

/// <summary>
/// Full reconciliation report with all discrepancy details.
/// </summary>
public sealed class ReconciliationReportDetailDto
{
    public Guid Id { get; set; }
    public string Period { get; set; } = string.Empty;
    public ReconciliationStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalSubscriptionsChecked { get; set; }
    public int TotalPaymentsChecked { get; set; }
    public int TotalInvoicesChecked { get; set; }
    public int DiscrepancyCount { get; set; }
    public decimal TotalDiscrepancyAmount { get; set; }
    public int AutoResolvedCount { get; set; }
    public int UnresolvedCount => DiscrepancyCount - AutoResolvedCount;
    public string TriggeredBy { get; set; } = "system";
    public string? ErrorMessage { get; set; }
    public TimeSpan? Duration => CompletedAt.HasValue
        ? CompletedAt.Value - StartedAt
        : null;

    public List<ReconciliationDiscrepancyDto> Discrepancies { get; set; } = [];

    /// <summary>Breakdown of discrepancies by type.</summary>
    public List<DiscrepancyTypeSummaryDto> ByType { get; set; } = [];

    /// <summary>Breakdown of discrepancies by severity.</summary>
    public List<DiscrepancySeveritySummaryDto> BySeverity { get; set; } = [];
}

/// <summary>
/// A single discrepancy found during reconciliation.
/// </summary>
public sealed class ReconciliationDiscrepancyDto
{
    public Guid Id { get; set; }
    public DiscrepancyType Type { get; set; }
    public DiscrepancySeverity Severity { get; set; }
    public Guid? DealerId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string? StripeInvoiceId { get; set; }
    public string? StripeCustomerId { get; set; }
    public Guid? OklaPaymentId { get; set; }
    public Guid? OklaSubscriptionId { get; set; }
    public decimal StripeAmount { get; set; }
    public decimal OklaAmount { get; set; }
    public decimal AmountDifference { get; set; }
    public string Description { get; set; } = string.Empty;
    public string SuggestedAction { get; set; } = string.Empty;
    public bool IsAutoResolved { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Summary of discrepancies by type.
/// </summary>
public sealed class DiscrepancyTypeSummaryDto
{
    public DiscrepancyType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// Summary of discrepancies by severity.
/// </summary>
public sealed class DiscrepancySeveritySummaryDto
{
    public DiscrepancySeverity Severity { get; set; }
    public string SeverityName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// Request to trigger a manual reconciliation.
/// </summary>
public sealed class TriggerReconciliationRequest
{
    /// <summary>Period to reconcile (YYYY-MM). Null = current month.</summary>
    public string? Period { get; set; }

    /// <summary>Whether to auto-resolve minor discrepancies.</summary>
    public bool AutoResolve { get; set; } = true;
}

/// <summary>
/// Request to resolve a specific discrepancy.
/// </summary>
public sealed class ResolveDiscrepancyRequest
{
    /// <summary>Resolution notes explaining how it was resolved.</summary>
    public string Notes { get; set; } = string.Empty;
}
