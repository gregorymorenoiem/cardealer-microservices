using System.Globalization;
using BillingService.Application.DTOs;
using BillingService.Domain.Entities;
using BillingService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BillingService.Application.UseCases.Reconciliation;

// ═══════════════════════════════════════════════════════════════════════════════
// RECONCILIATION HANDLERS — CONTRA #6 FIX
//
// MediatR handlers for reconciliation operations.
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Handler: trigger a full reconciliation run.
/// </summary>
public sealed class TriggerReconciliationCommandHandler
    : IRequestHandler<TriggerReconciliationCommand, ReconciliationReportDetailDto>
{
    private readonly IPaymentReconciliationService _reconciliationService;
    private readonly ILogger<TriggerReconciliationCommandHandler> _logger;

    public TriggerReconciliationCommandHandler(
        IPaymentReconciliationService reconciliationService,
        ILogger<TriggerReconciliationCommandHandler> logger)
    {
        _reconciliationService = reconciliationService;
        _logger = logger;
    }

    public async Task<ReconciliationReportDetailDto> Handle(
        TriggerReconciliationCommand request, CancellationToken ct)
    {
        _logger.LogInformation(
            "[Reconciliation] Triggering reconciliation for period={Period} by={TriggeredBy}",
            request.Period ?? "current", request.TriggeredBy);

        var report = await _reconciliationService.RunReconciliationAsync(
            request.Period, request.TriggeredBy, request.AutoResolve, ct);

        return ReconciliationMapper.MapToDetail(report);
    }
}

/// <summary>
/// Handler: get the latest reconciliation report for a period.
/// </summary>
public sealed class GetReconciliationReportQueryHandler
    : IRequestHandler<GetReconciliationReportQuery, ReconciliationReportDetailDto?>
{
    private readonly IReconciliationRepository _repository;

    public GetReconciliationReportQueryHandler(IReconciliationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ReconciliationReportDetailDto?> Handle(
        GetReconciliationReportQuery request, CancellationToken ct)
    {
        var period = request.Period
            ?? DateTime.UtcNow.ToString("yyyy-MM", CultureInfo.InvariantCulture);

        var report = await _repository.GetLatestByPeriodAsync(period, ct);
        return report != null ? ReconciliationMapper.MapToDetail(report) : null;
    }
}

/// <summary>
/// Handler: get reconciliation history.
/// </summary>
public sealed class GetReconciliationHistoryQueryHandler
    : IRequestHandler<GetReconciliationHistoryQuery, List<ReconciliationReportSummaryDto>>
{
    private readonly IReconciliationRepository _repository;

    public GetReconciliationHistoryQueryHandler(IReconciliationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ReconciliationReportSummaryDto>> Handle(
        GetReconciliationHistoryQuery request, CancellationToken ct)
    {
        var reports = await _repository.GetAllAsync(request.Limit, ct);
        return reports.Select(r => new ReconciliationReportSummaryDto
        {
            Id = r.Id,
            Period = r.Period,
            Status = r.Status,
            StartedAt = r.StartedAt,
            CompletedAt = r.CompletedAt,
            TotalSubscriptionsChecked = r.TotalSubscriptionsChecked,
            TotalPaymentsChecked = r.TotalPaymentsChecked,
            DiscrepancyCount = r.DiscrepancyCount,
            TotalDiscrepancyAmount = r.TotalDiscrepancyAmount,
            AutoResolvedCount = r.AutoResolvedCount,
            TriggeredBy = r.TriggeredBy,
        }).ToList();
    }
}

/// <summary>
/// Handler: get all unresolved discrepancies.
/// </summary>
public sealed class GetUnresolvedDiscrepanciesQueryHandler
    : IRequestHandler<GetUnresolvedDiscrepanciesQuery, List<ReconciliationDiscrepancyDto>>
{
    private readonly IReconciliationRepository _repository;

    public GetUnresolvedDiscrepanciesQueryHandler(IReconciliationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ReconciliationDiscrepancyDto>> Handle(
        GetUnresolvedDiscrepanciesQuery request, CancellationToken ct)
    {
        var discrepancies = await _repository.GetUnresolvedDiscrepanciesAsync(ct);
        return discrepancies.Select(ReconciliationMapper.MapDiscrepancy).ToList();
    }
}

/// <summary>
/// Handler: resolve a specific discrepancy.
/// </summary>
public sealed class ResolveDiscrepancyCommandHandler
    : IRequestHandler<ResolveDiscrepancyCommand, bool>
{
    private readonly IReconciliationRepository _repository;
    private readonly ILogger<ResolveDiscrepancyCommandHandler> _logger;

    public ResolveDiscrepancyCommandHandler(
        IReconciliationRepository repository,
        ILogger<ResolveDiscrepancyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(ResolveDiscrepancyCommand request, CancellationToken ct)
    {
        var discrepancy = await _repository.GetDiscrepancyByIdAsync(request.DiscrepancyId, ct);
        if (discrepancy == null)
        {
            _logger.LogWarning("[Reconciliation] Discrepancy {Id} not found", request.DiscrepancyId);
            return false;
        }

        discrepancy.Resolve(request.ResolvedBy, request.Notes);
        await _repository.UpdateDiscrepancyAsync(discrepancy, ct);

        _logger.LogInformation(
            "[Reconciliation] Discrepancy {Id} resolved by {ResolvedBy}: {Notes}",
            request.DiscrepancyId, request.ResolvedBy, request.Notes);

        return true;
    }
}

/// <summary>
/// Handler: reconcile a single dealer.
/// </summary>
public sealed class ReconcileDealerCommandHandler
    : IRequestHandler<ReconcileDealerCommand, List<ReconciliationDiscrepancyDto>>
{
    private readonly IPaymentReconciliationService _reconciliationService;
    private readonly ILogger<ReconcileDealerCommandHandler> _logger;

    public ReconcileDealerCommandHandler(
        IPaymentReconciliationService reconciliationService,
        ILogger<ReconcileDealerCommandHandler> logger)
    {
        _reconciliationService = reconciliationService;
        _logger = logger;
    }

    public async Task<List<ReconciliationDiscrepancyDto>> Handle(
        ReconcileDealerCommand request, CancellationToken ct)
    {
        _logger.LogInformation(
            "[Reconciliation] Reconciling dealer {DealerId} for period={Period}",
            request.DealerId, request.Period ?? "current");

        var discrepancies = await _reconciliationService.ReconcileDealerAsync(
            request.DealerId, request.Period, ct);

        return discrepancies.Select(ReconciliationMapper.MapDiscrepancy).ToList();
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// SHARED MAPPING HELPERS
// ═══════════════════════════════════════════════════════════════════════════════

internal static class ReconciliationMapper
{
    public static ReconciliationReportDetailDto MapToDetail(ReconciliationReport report)
    {
        var discrepancies = report.Discrepancies.Select(MapDiscrepancy).ToList();

        return new ReconciliationReportDetailDto
        {
            Id = report.Id,
            Period = report.Period,
            Status = report.Status,
            StartedAt = report.StartedAt,
            CompletedAt = report.CompletedAt,
            TotalSubscriptionsChecked = report.TotalSubscriptionsChecked,
            TotalPaymentsChecked = report.TotalPaymentsChecked,
            TotalInvoicesChecked = report.TotalInvoicesChecked,
            DiscrepancyCount = report.DiscrepancyCount,
            TotalDiscrepancyAmount = report.TotalDiscrepancyAmount,
            AutoResolvedCount = report.AutoResolvedCount,
            TriggeredBy = report.TriggeredBy,
            ErrorMessage = report.ErrorMessage,
            Discrepancies = discrepancies,
            ByType = discrepancies
                .GroupBy(d => d.Type)
                .Select(g => new DiscrepancyTypeSummaryDto
                {
                    Type = g.Key,
                    TypeName = g.Key.ToString(),
                    Count = g.Count(),
                    TotalAmount = g.Sum(d => d.AmountDifference),
                })
                .ToList(),
            BySeverity = discrepancies
                .GroupBy(d => d.Severity)
                .Select(g => new DiscrepancySeveritySummaryDto
                {
                    Severity = g.Key,
                    SeverityName = g.Key.ToString(),
                    Count = g.Count(),
                    TotalAmount = g.Sum(d => d.AmountDifference),
                })
                .ToList(),
        };
    }

    public static ReconciliationDiscrepancyDto MapDiscrepancy(ReconciliationDiscrepancy d)
    {
        return new ReconciliationDiscrepancyDto
        {
            Id = d.Id,
            Type = d.Type,
            Severity = d.Severity,
            DealerId = d.DealerId,
            StripePaymentIntentId = d.StripePaymentIntentId,
            StripeSubscriptionId = d.StripeSubscriptionId,
            StripeInvoiceId = d.StripeInvoiceId,
            StripeCustomerId = d.StripeCustomerId,
            OklaPaymentId = d.OklaPaymentId,
            OklaSubscriptionId = d.OklaSubscriptionId,
            StripeAmount = d.StripeAmount,
            OklaAmount = d.OklaAmount,
            AmountDifference = d.AmountDifference,
            Description = d.Description,
            SuggestedAction = d.SuggestedAction,
            IsAutoResolved = d.IsAutoResolved,
            ResolutionNotes = d.ResolutionNotes,
            ResolvedAt = d.ResolvedAt,
            ResolvedBy = d.ResolvedBy,
            CreatedAt = d.CreatedAt,
        };
    }
}
