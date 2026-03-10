using BillingService.Application.DTOs;
using MediatR;

namespace BillingService.Application.UseCases.Reconciliation;

// ═══════════════════════════════════════════════════════════════════════════════
// RECONCILIATION QUERIES & COMMANDS — CONTRA #6 FIX
//
// CQRS operations for the payment reconciliation audit system.
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Trigger a reconciliation run for the specified period.
/// </summary>
public sealed record TriggerReconciliationCommand(
    string? Period = null,
    bool AutoResolve = true,
    string TriggeredBy = "system"
) : IRequest<ReconciliationReportDetailDto>;

/// <summary>
/// Get the latest reconciliation report for a period.
/// </summary>
public sealed record GetReconciliationReportQuery(
    string? Period = null
) : IRequest<ReconciliationReportDetailDto?>;

/// <summary>
/// Get all reconciliation reports (paginated, most recent first).
/// </summary>
public sealed record GetReconciliationHistoryQuery(
    int Limit = 30
) : IRequest<List<ReconciliationReportSummaryDto>>;

/// <summary>
/// Get all unresolved discrepancies across all reports.
/// </summary>
public sealed record GetUnresolvedDiscrepanciesQuery
    : IRequest<List<ReconciliationDiscrepancyDto>>;

/// <summary>
/// Resolve a specific discrepancy manually.
/// </summary>
public sealed record ResolveDiscrepancyCommand(
    Guid DiscrepancyId,
    string ResolvedBy,
    string Notes
) : IRequest<bool>;

/// <summary>
/// Reconcile a single dealer's payments.
/// </summary>
public sealed record ReconcileDealerCommand(
    Guid DealerId,
    string? Period = null
) : IRequest<List<ReconciliationDiscrepancyDto>>;
