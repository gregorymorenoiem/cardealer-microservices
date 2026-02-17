using MediatR;
using RegulatoryAlertService.Application.DTOs;
using RegulatoryAlertService.Domain.Entities;
using RegulatoryAlertService.Domain.Interfaces;
using RegulatoryAlertService.Domain.Enums;

namespace RegulatoryAlertService.Application.Features.Alerts.Queries;

// ===== GET ALERT BY ID =====

public record GetAlertByIdQuery(Guid Id) : IRequest<RegulatoryAlertDto?>;

public class GetAlertByIdHandler : IRequestHandler<GetAlertByIdQuery, RegulatoryAlertDto?>
{
    private readonly IRegulatoryAlertRepository _repository;

    public GetAlertByIdHandler(IRegulatoryAlertRepository repository)
    {
        _repository = repository;
    }

    public async Task<RegulatoryAlertDto?> Handle(GetAlertByIdQuery request, CancellationToken ct)
    {
        var alert = await _repository.GetByIdAsync(request.Id, ct);
        return alert == null ? null : MapToDto(alert);
    }

    private static RegulatoryAlertDto MapToDto(RegulatoryAlert a) => new(
        a.Id, a.Title, a.Description, a.DetailedContent, a.AlertType, a.Priority,
        a.Status, a.RegulatoryBody, a.Category, a.EffectiveDate, a.DeadlineDate,
        a.ExpirationDate, a.LegalReference, a.OfficialDocumentUrl, a.RequiresAction,
        a.ActionRequired, a.Tags, a.CreatedAt);
}

// ===== GET ACTIVE ALERTS =====

public record GetActiveAlertsQuery : IRequest<List<RegulatoryAlertSummaryDto>>;

public class GetActiveAlertsHandler : IRequestHandler<GetActiveAlertsQuery, List<RegulatoryAlertSummaryDto>>
{
    private readonly IRegulatoryAlertRepository _repository;

    public GetActiveAlertsHandler(IRegulatoryAlertRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<RegulatoryAlertSummaryDto>> Handle(GetActiveAlertsQuery request, CancellationToken ct)
    {
        var alerts = await _repository.GetActiveAsync(ct);
        return alerts.Select(a => new RegulatoryAlertSummaryDto(
            a.Id, a.Title, a.AlertType, a.Priority, a.Status, a.RegulatoryBody,
            a.DeadlineDate, a.RequiresAction)).ToList();
    }
}

// ===== GET ALERTS BY REGULATORY BODY =====

public record GetAlertsByRegulatoryBodyQuery(RegulatoryBody Body) : IRequest<List<RegulatoryAlertSummaryDto>>;

public class GetAlertsByRegulatoryBodyHandler : IRequestHandler<GetAlertsByRegulatoryBodyQuery, List<RegulatoryAlertSummaryDto>>
{
    private readonly IRegulatoryAlertRepository _repository;

    public GetAlertsByRegulatoryBodyHandler(IRegulatoryAlertRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<RegulatoryAlertSummaryDto>> Handle(GetAlertsByRegulatoryBodyQuery request, CancellationToken ct)
    {
        var alerts = await _repository.GetByRegulatoryBodyAsync(request.Body, ct);
        return alerts.Select(a => new RegulatoryAlertSummaryDto(
            a.Id, a.Title, a.AlertType, a.Priority, a.Status, a.RegulatoryBody,
            a.DeadlineDate, a.RequiresAction)).ToList();
    }
}

// ===== GET UPCOMING DEADLINES =====

public record GetUpcomingDeadlinesQuery(int Days = 30) : IRequest<List<RegulatoryAlertSummaryDto>>;

public class GetUpcomingDeadlinesHandler : IRequestHandler<GetUpcomingDeadlinesQuery, List<RegulatoryAlertSummaryDto>>
{
    private readonly IRegulatoryAlertRepository _repository;

    public GetUpcomingDeadlinesHandler(IRegulatoryAlertRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<RegulatoryAlertSummaryDto>> Handle(GetUpcomingDeadlinesQuery request, CancellationToken ct)
    {
        var alerts = await _repository.GetUpcomingDeadlinesAsync(request.Days, ct);
        return alerts.Select(a => new RegulatoryAlertSummaryDto(
            a.Id, a.Title, a.AlertType, a.Priority, a.Status, a.RegulatoryBody,
            a.DeadlineDate, a.RequiresAction)).ToList();
    }
}

// ===== GET USER SUBSCRIPTIONS =====

public record GetUserSubscriptionsQuery(string UserId) : IRequest<List<AlertSubscriptionDto>>;

public class GetUserSubscriptionsHandler : IRequestHandler<GetUserSubscriptionsQuery, List<AlertSubscriptionDto>>
{
    private readonly IAlertSubscriptionRepository _repository;

    public GetUserSubscriptionsHandler(IAlertSubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AlertSubscriptionDto>> Handle(GetUserSubscriptionsQuery request, CancellationToken ct)
    {
        var subscriptions = await _repository.GetByUserIdAsync(request.UserId, ct);
        return subscriptions.Select(s => new AlertSubscriptionDto(
            s.Id, s.UserId, s.RegulatoryBody, s.Category, s.MinimumPriority,
            s.Frequency, s.PreferredChannel, s.IsActive, s.Email, s.PhoneNumber)).ToList();
    }
}

// ===== GET CALENDAR ENTRIES =====

public record GetUpcomingCalendarEntriesQuery(int Days = 30) : IRequest<List<RegulatoryCalendarEntryDto>>;

public class GetUpcomingCalendarEntriesHandler : IRequestHandler<GetUpcomingCalendarEntriesQuery, List<RegulatoryCalendarEntryDto>>
{
    private readonly IRegulatoryCalendarEntryRepository _repository;

    public GetUpcomingCalendarEntriesHandler(IRegulatoryCalendarEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<RegulatoryCalendarEntryDto>> Handle(GetUpcomingCalendarEntriesQuery request, CancellationToken ct)
    {
        var entries = await _repository.GetUpcomingAsync(request.Days, ct);
        return entries.Select(e => new RegulatoryCalendarEntryDto(
            e.Id, e.Title, e.Description, e.RegulatoryBody, e.Category, e.DueDate,
            e.IsRecurring, e.RecurrencePattern, e.LegalBasis, e.ReminderDaysBefore)).ToList();
    }
}

// ===== GET USER DEADLINES =====

public record GetUserDeadlinesQuery(string UserId, bool PendingOnly = true) : IRequest<List<ComplianceDeadlineDto>>;

public class GetUserDeadlinesHandler : IRequestHandler<GetUserDeadlinesQuery, List<ComplianceDeadlineDto>>
{
    private readonly IComplianceDeadlineRepository _repository;

    public GetUserDeadlinesHandler(IComplianceDeadlineRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ComplianceDeadlineDto>> Handle(GetUserDeadlinesQuery request, CancellationToken ct)
    {
        var deadlines = request.PendingOnly
            ? await _repository.GetPendingByUserAsync(request.UserId, ct)
            : await _repository.GetByUserIdAsync(request.UserId, ct);

        return deadlines.Select(d => new ComplianceDeadlineDto(
            d.Id, d.UserId, d.Title, d.Description, d.DueDate, d.IsCompleted,
            d.CompletedAt, d.Priority, !d.IsCompleted && d.DueDate < DateTime.UtcNow)).ToList();
    }
}

// ===== GET STATISTICS =====

public record GetRegulatoryStatisticsQuery : IRequest<RegulatoryStatisticsDto>;

public class GetRegulatoryStatisticsHandler : IRequestHandler<GetRegulatoryStatisticsQuery, RegulatoryStatisticsDto>
{
    private readonly IRegulatoryAlertRepository _alertRepository;
    private readonly IAlertSubscriptionRepository _subscriptionRepository;
    private readonly IRegulatoryCalendarEntryRepository _calendarRepository;

    public GetRegulatoryStatisticsHandler(
        IRegulatoryAlertRepository alertRepository,
        IAlertSubscriptionRepository subscriptionRepository,
        IRegulatoryCalendarEntryRepository calendarRepository)
    {
        _alertRepository = alertRepository;
        _subscriptionRepository = subscriptionRepository;
        _calendarRepository = calendarRepository;
    }

    public async Task<RegulatoryStatisticsDto> Handle(GetRegulatoryStatisticsQuery request, CancellationToken ct)
    {
        var allAlerts = await _alertRepository.GetAllAsync(ct);
        var activeAlerts = await _alertRepository.GetActiveAsync(ct);
        var upcomingDeadlines = await _alertRepository.GetUpcomingDeadlinesAsync(30, ct);
        var subscriptions = await _subscriptionRepository.GetActiveAsync(ct);
        var calendarEntries = await _calendarRepository.GetActiveAsync(ct);

        var alertsByBody = allAlerts
            .GroupBy(a => a.RegulatoryBody.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var alertsByPriority = allAlerts
            .GroupBy(a => a.Priority.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return new RegulatoryStatisticsDto(
            TotalAlerts: allAlerts.Count,
            ActiveAlerts: activeAlerts.Count,
            CriticalAlerts: allAlerts.Count(a => a.Priority == AlertPriority.Critical),
            UpcomingDeadlines: upcomingDeadlines.Count,
            AlertsRequiringAction: allAlerts.Count(a => a.RequiresAction),
            TotalSubscriptions: subscriptions.Count,
            TotalCalendarEntries: calendarEntries.Count,
            AlertsByBody: alertsByBody,
            AlertsByPriority: alertsByPriority);
    }
}
