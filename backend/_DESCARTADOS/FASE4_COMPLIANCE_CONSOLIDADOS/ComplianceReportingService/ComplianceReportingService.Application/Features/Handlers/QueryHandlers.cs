// ComplianceReportingService - Query Handlers
// Handlers para consultas de reportería regulatoria

namespace ComplianceReportingService.Application.Features.Handlers;

using MediatR;
using ComplianceReportingService.Application.DTOs;
using ComplianceReportingService.Application.Features.Queries;
using ComplianceReportingService.Domain.Entities;
using ComplianceReportingService.Domain.Interfaces;

#region Report Query Handlers

public class GetReportByIdHandler : IRequestHandler<GetReportByIdQuery, ReportDto?>
{
    private readonly IReportRepository _reportRepo;

    public GetReportByIdHandler(IReportRepository reportRepo)
    {
        _reportRepo = reportRepo;
    }

    public async Task<ReportDto?> Handle(GetReportByIdQuery request, CancellationToken ct)
    {
        var report = await _reportRepo.GetByIdAsync(request.ReportId, ct);
        return report == null ? null : MapToDto(report);
    }

    private ReportDto MapToDto(Report r) => new(
        r.Id, r.ReportNumber, r.Type, r.Status, r.Name, r.Description,
        r.PeriodStart, r.PeriodEnd, r.Format, r.GeneratedAt, r.FilePath,
        r.FileSize, r.Destination, r.SubmittedAt, r.SubmissionReference,
        r.RecordCount, r.TotalAmount, r.Currency, r.CreatedAt, r.DueDate);
}

public class GetReportByNumberHandler : IRequestHandler<GetReportByNumberQuery, ReportDto?>
{
    private readonly IReportRepository _reportRepo;

    public GetReportByNumberHandler(IReportRepository reportRepo)
    {
        _reportRepo = reportRepo;
    }

    public async Task<ReportDto?> Handle(GetReportByNumberQuery request, CancellationToken ct)
    {
        var report = await _reportRepo.GetByReportNumberAsync(request.ReportNumber, ct);
        return report == null ? null : MapToDto(report);
    }

    private ReportDto MapToDto(Report r) => new(
        r.Id, r.ReportNumber, r.Type, r.Status, r.Name, r.Description,
        r.PeriodStart, r.PeriodEnd, r.Format, r.GeneratedAt, r.FilePath,
        r.FileSize, r.Destination, r.SubmittedAt, r.SubmissionReference,
        r.RecordCount, r.TotalAmount, r.Currency, r.CreatedAt, r.DueDate);
}

public class GetReportsPagedHandler : IRequestHandler<GetReportsPagedQuery, ReportPagedResultDto>
{
    private readonly IReportRepository _reportRepo;

    public GetReportsPagedHandler(IReportRepository reportRepo)
    {
        _reportRepo = reportRepo;
    }

    public async Task<ReportPagedResultDto> Handle(GetReportsPagedQuery request, CancellationToken ct)
    {
        var (reports, total) = await _reportRepo.GetPagedAsync(
            request.Page, request.PageSize, request.Type, request.Status,
            request.Destination, request.FromDate, request.ToDate, 
            request.SearchTerm, ct);

        var items = reports.Select(r => new ReportSummaryDto(
            r.Id, r.ReportNumber, r.Type, r.Status, r.Name,
            r.PeriodStart, r.PeriodEnd, r.GeneratedAt, r.SubmittedAt, r.DueDate
        )).ToList();

        return new ReportPagedResultDto(items, total, request.Page, request.PageSize);
    }
}

public class GetPendingReportsHandler : IRequestHandler<GetPendingReportsQuery, List<ReportSummaryDto>>
{
    private readonly IReportRepository _reportRepo;

    public GetPendingReportsHandler(IReportRepository reportRepo)
    {
        _reportRepo = reportRepo;
    }

    public async Task<List<ReportSummaryDto>> Handle(GetPendingReportsQuery request, CancellationToken ct)
    {
        var reports = await _reportRepo.GetPendingSubmissionAsync(request.Destination, ct);
        return reports.Select(r => new ReportSummaryDto(
            r.Id, r.ReportNumber, r.Type, r.Status, r.Name,
            r.PeriodStart, r.PeriodEnd, r.GeneratedAt, r.SubmittedAt, r.DueDate
        )).ToList();
    }
}

public class GetOverdueReportsHandler : IRequestHandler<GetOverdueReportsQuery, List<ReportSummaryDto>>
{
    private readonly IReportRepository _reportRepo;

    public GetOverdueReportsHandler(IReportRepository reportRepo)
    {
        _reportRepo = reportRepo;
    }

    public async Task<List<ReportSummaryDto>> Handle(GetOverdueReportsQuery request, CancellationToken ct)
    {
        var reports = await _reportRepo.GetOverdueAsync(request.AsOfDate, request.DaysAhead, ct);
        return reports.Select(r => new ReportSummaryDto(
            r.Id, r.ReportNumber, r.Type, r.Status, r.Name,
            r.PeriodStart, r.PeriodEnd, r.GeneratedAt, r.SubmittedAt, r.DueDate
        )).ToList();
    }
}

public class GetReportsByPeriodHandler : IRequestHandler<GetReportsByPeriodQuery, List<ReportSummaryDto>>
{
    private readonly IReportRepository _reportRepo;

    public GetReportsByPeriodHandler(IReportRepository reportRepo)
    {
        _reportRepo = reportRepo;
    }

    public async Task<List<ReportSummaryDto>> Handle(GetReportsByPeriodQuery request, CancellationToken ct)
    {
        var reports = await _reportRepo.GetByPeriodAsync(request.PeriodStart, request.PeriodEnd, request.Type, ct);
        return reports.Select(r => new ReportSummaryDto(
            r.Id, r.ReportNumber, r.Type, r.Status, r.Name,
            r.PeriodStart, r.PeriodEnd, r.GeneratedAt, r.SubmittedAt, r.DueDate
        )).ToList();
    }
}

#endregion

#region Schedule Query Handlers

public class GetScheduleByIdHandler : IRequestHandler<GetScheduleByIdQuery, ReportScheduleDto?>
{
    private readonly IReportScheduleRepository _scheduleRepo;

    public GetScheduleByIdHandler(IReportScheduleRepository scheduleRepo)
    {
        _scheduleRepo = scheduleRepo;
    }

    public async Task<ReportScheduleDto?> Handle(GetScheduleByIdQuery request, CancellationToken ct)
    {
        var schedule = await _scheduleRepo.GetByIdAsync(request.ScheduleId, ct);
        return schedule == null ? null : MapToDto(schedule);
    }

    private ReportScheduleDto MapToDto(ReportSchedule s) => new(
        s.Id, s.Name, s.ReportType, s.Frequency, s.Format, s.Destination,
        s.CronExpression, s.NextRunAt, s.LastRunAt, s.AutoSubmit,
        s.NotificationEmail, s.IsActive);
}

public class GetActiveSchedulesHandler : IRequestHandler<GetActiveSchedulesQuery, List<ReportScheduleDto>>
{
    private readonly IReportScheduleRepository _scheduleRepo;

    public GetActiveSchedulesHandler(IReportScheduleRepository scheduleRepo)
    {
        _scheduleRepo = scheduleRepo;
    }

    public async Task<List<ReportScheduleDto>> Handle(GetActiveSchedulesQuery request, CancellationToken ct)
    {
        var schedules = await _scheduleRepo.GetActiveAsync(ct);
        return schedules.Select(s => new ReportScheduleDto(
            s.Id, s.Name, s.ReportType, s.Frequency, s.Format, s.Destination,
            s.CronExpression, s.NextRunAt, s.LastRunAt, s.AutoSubmit,
            s.NotificationEmail, s.IsActive
        )).ToList();
    }
}

public class GetSchedulesByTypeHandler : IRequestHandler<GetSchedulesByTypeQuery, List<ReportScheduleDto>>
{
    private readonly IReportScheduleRepository _scheduleRepo;

    public GetSchedulesByTypeHandler(IReportScheduleRepository scheduleRepo)
    {
        _scheduleRepo = scheduleRepo;
    }

    public async Task<List<ReportScheduleDto>> Handle(GetSchedulesByTypeQuery request, CancellationToken ct)
    {
        var schedules = await _scheduleRepo.GetByReportTypeAsync(request.ReportType, ct);
        return schedules.Select(s => new ReportScheduleDto(
            s.Id, s.Name, s.ReportType, s.Frequency, s.Format, s.Destination,
            s.CronExpression, s.NextRunAt, s.LastRunAt, s.AutoSubmit,
            s.NotificationEmail, s.IsActive
        )).ToList();
    }
}

public class GetUpcomingSchedulesHandler : IRequestHandler<GetUpcomingSchedulesQuery, List<ReportScheduleDto>>
{
    private readonly IReportScheduleRepository _scheduleRepo;

    public GetUpcomingSchedulesHandler(IReportScheduleRepository scheduleRepo)
    {
        _scheduleRepo = scheduleRepo;
    }

    public async Task<List<ReportScheduleDto>> Handle(GetUpcomingSchedulesQuery request, CancellationToken ct)
    {
        var schedules = await _scheduleRepo.GetUpcomingAsync(request.Until, ct);
        return schedules.Select(s => new ReportScheduleDto(
            s.Id, s.Name, s.ReportType, s.Frequency, s.Format, s.Destination,
            s.CronExpression, s.NextRunAt, s.LastRunAt, s.AutoSubmit,
            s.NotificationEmail, s.IsActive
        )).ToList();
    }
}

#endregion

#region Template Query Handlers

public class GetTemplateByIdHandler : IRequestHandler<GetTemplateByIdQuery, ReportTemplateDto?>
{
    private readonly IReportTemplateRepository _templateRepo;

    public GetTemplateByIdHandler(IReportTemplateRepository templateRepo)
    {
        _templateRepo = templateRepo;
    }

    public async Task<ReportTemplateDto?> Handle(GetTemplateByIdQuery request, CancellationToken ct)
    {
        var template = await _templateRepo.GetByIdAsync(request.TemplateId, ct);
        return template == null ? null : new ReportTemplateDto(
            template.Id, template.Code, template.Name, template.ForReportType,
            template.Description, template.Version, template.IsActive);
    }
}

public class GetTemplateByCodeHandler : IRequestHandler<GetTemplateByCodeQuery, ReportTemplateDto?>
{
    private readonly IReportTemplateRepository _templateRepo;

    public GetTemplateByCodeHandler(IReportTemplateRepository templateRepo)
    {
        _templateRepo = templateRepo;
    }

    public async Task<ReportTemplateDto?> Handle(GetTemplateByCodeQuery request, CancellationToken ct)
    {
        var template = await _templateRepo.GetByCodeAsync(request.Code, ct);
        return template == null ? null : new ReportTemplateDto(
            template.Id, template.Code, template.Name, template.ForReportType,
            template.Description, template.Version, template.IsActive);
    }
}

public class GetActiveTemplatesHandler : IRequestHandler<GetActiveTemplatesQuery, List<ReportTemplateDto>>
{
    private readonly IReportTemplateRepository _templateRepo;

    public GetActiveTemplatesHandler(IReportTemplateRepository templateRepo)
    {
        _templateRepo = templateRepo;
    }

    public async Task<List<ReportTemplateDto>> Handle(GetActiveTemplatesQuery request, CancellationToken ct)
    {
        var templates = await _templateRepo.GetActiveAsync(ct);
        return templates.Select(t => new ReportTemplateDto(
            t.Id, t.Code, t.Name, t.ForReportType, t.Description, t.Version, t.IsActive
        )).ToList();
    }
}

public class GetTemplatesByTypeHandler : IRequestHandler<GetTemplatesByTypeQuery, List<ReportTemplateDto>>
{
    private readonly IReportTemplateRepository _templateRepo;

    public GetTemplatesByTypeHandler(IReportTemplateRepository templateRepo)
    {
        _templateRepo = templateRepo;
    }

    public async Task<List<ReportTemplateDto>> Handle(GetTemplatesByTypeQuery request, CancellationToken ct)
    {
        var templates = await _templateRepo.GetByReportTypeAsync(request.ForReportType, ct);
        return templates.Select(t => new ReportTemplateDto(
            t.Id, t.Code, t.Name, t.ForReportType, t.Description, t.Version, t.IsActive
        )).ToList();
    }
}

#endregion

#region Execution Query Handlers

public class GetExecutionsByReportHandler : IRequestHandler<GetExecutionsByReportQuery, List<ReportExecutionDto>>
{
    private readonly IReportExecutionRepository _executionRepo;

    public GetExecutionsByReportHandler(IReportExecutionRepository executionRepo)
    {
        _executionRepo = executionRepo;
    }

    public async Task<List<ReportExecutionDto>> Handle(GetExecutionsByReportQuery request, CancellationToken ct)
    {
        var executions = await _executionRepo.GetByReportIdAsync(request.ReportId, ct);
        return executions.Select(e => new ReportExecutionDto(
            e.Id, e.ReportId, e.ScheduleId, e.StartedAt, e.CompletedAt,
            e.DurationMs, e.Success, e.ErrorMessage, e.ExecutedBy
        )).ToList();
    }
}

public class GetExecutionsByScheduleHandler : IRequestHandler<GetExecutionsByScheduleQuery, List<ReportExecutionDto>>
{
    private readonly IReportExecutionRepository _executionRepo;

    public GetExecutionsByScheduleHandler(IReportExecutionRepository executionRepo)
    {
        _executionRepo = executionRepo;
    }

    public async Task<List<ReportExecutionDto>> Handle(GetExecutionsByScheduleQuery request, CancellationToken ct)
    {
        var executions = await _executionRepo.GetByScheduleIdAsync(request.ScheduleId, request.Limit, ct);
        return executions.Select(e => new ReportExecutionDto(
            e.Id, e.ReportId, e.ScheduleId, e.StartedAt, e.CompletedAt,
            e.DurationMs, e.Success, e.ErrorMessage, e.ExecutedBy
        )).ToList();
    }
}

public class GetFailedExecutionsHandler : IRequestHandler<GetFailedExecutionsQuery, List<ReportExecutionDto>>
{
    private readonly IReportExecutionRepository _executionRepo;

    public GetFailedExecutionsHandler(IReportExecutionRepository executionRepo)
    {
        _executionRepo = executionRepo;
    }

    public async Task<List<ReportExecutionDto>> Handle(GetFailedExecutionsQuery request, CancellationToken ct)
    {
        var executions = await _executionRepo.GetFailedAsync(request.Since, ct);
        return executions.Select(e => new ReportExecutionDto(
            e.Id, e.ReportId, e.ScheduleId, e.StartedAt, e.CompletedAt,
            e.DurationMs, e.Success, e.ErrorMessage, e.ExecutedBy
        )).ToList();
    }
}

#endregion

#region Subscription Query Handlers

public class GetSubscriptionByIdHandler : IRequestHandler<GetSubscriptionByIdQuery, ReportSubscriptionDto?>
{
    private readonly IReportSubscriptionRepository _subscriptionRepo;

    public GetSubscriptionByIdHandler(IReportSubscriptionRepository subscriptionRepo)
    {
        _subscriptionRepo = subscriptionRepo;
    }

    public async Task<ReportSubscriptionDto?> Handle(GetSubscriptionByIdQuery request, CancellationToken ct)
    {
        var sub = await _subscriptionRepo.GetByIdAsync(request.SubscriptionId, ct);
        return sub == null ? null : new ReportSubscriptionDto(
            sub.Id, sub.UserId, sub.ReportType, sub.Frequency,
            sub.DeliveryMethod, sub.DeliveryAddress, sub.IsActive);
    }
}

public class GetUserSubscriptionsHandler : IRequestHandler<GetUserSubscriptionsQuery, List<ReportSubscriptionDto>>
{
    private readonly IReportSubscriptionRepository _subscriptionRepo;

    public GetUserSubscriptionsHandler(IReportSubscriptionRepository subscriptionRepo)
    {
        _subscriptionRepo = subscriptionRepo;
    }

    public async Task<List<ReportSubscriptionDto>> Handle(GetUserSubscriptionsQuery request, CancellationToken ct)
    {
        var subs = await _subscriptionRepo.GetByUserIdAsync(request.UserId, ct);
        return subs.Select(s => new ReportSubscriptionDto(
            s.Id, s.UserId, s.ReportType, s.Frequency,
            s.DeliveryMethod, s.DeliveryAddress, s.IsActive
        )).ToList();
    }
}

public class GetSubscribersByTypeHandler : IRequestHandler<GetSubscribersByTypeQuery, List<ReportSubscriptionDto>>
{
    private readonly IReportSubscriptionRepository _subscriptionRepo;

    public GetSubscribersByTypeHandler(IReportSubscriptionRepository subscriptionRepo)
    {
        _subscriptionRepo = subscriptionRepo;
    }

    public async Task<List<ReportSubscriptionDto>> Handle(GetSubscribersByTypeQuery request, CancellationToken ct)
    {
        var subs = await _subscriptionRepo.GetByReportTypeAsync(request.ReportType, ct);
        return subs.Select(s => new ReportSubscriptionDto(
            s.Id, s.UserId, s.ReportType, s.Frequency,
            s.DeliveryMethod, s.DeliveryAddress, s.IsActive
        )).ToList();
    }
}

#endregion

#region DGII Query Handlers

public class GetDGIISubmissionByIdHandler : IRequestHandler<GetDGIISubmissionByIdQuery, DGIISubmissionDto?>
{
    private readonly IDGIISubmissionRepository _dgiiRepo;

    public GetDGIISubmissionByIdHandler(IDGIISubmissionRepository dgiiRepo)
    {
        _dgiiRepo = dgiiRepo;
    }

    public async Task<DGIISubmissionDto?> Handle(GetDGIISubmissionByIdQuery request, CancellationToken ct)
    {
        var sub = await _dgiiRepo.GetByIdAsync(request.SubmissionId, ct);
        return sub == null ? null : new DGIISubmissionDto(
            sub.Id, sub.ReportId, sub.ReportCode, sub.RNC, sub.Period,
            sub.SubmissionDate, sub.Status, sub.ConfirmationNumber,
            sub.ResponseMessage, sub.Attempts);
    }
}

public class GetDGIISubmissionsByPeriodHandler : IRequestHandler<GetDGIISubmissionsByPeriodQuery, List<DGIISubmissionDto>>
{
    private readonly IDGIISubmissionRepository _dgiiRepo;

    public GetDGIISubmissionsByPeriodHandler(IDGIISubmissionRepository dgiiRepo)
    {
        _dgiiRepo = dgiiRepo;
    }

    public async Task<List<DGIISubmissionDto>> Handle(GetDGIISubmissionsByPeriodQuery request, CancellationToken ct)
    {
        var subs = await _dgiiRepo.GetByPeriodAsync(request.Period, request.ReportCode, ct);
        return subs.Select(s => new DGIISubmissionDto(
            s.Id, s.ReportId, s.ReportCode, s.RNC, s.Period,
            s.SubmissionDate, s.Status, s.ConfirmationNumber,
            s.ResponseMessage, s.Attempts
        )).ToList();
    }
}

public class GetDGIIComplianceStatusHandler : IRequestHandler<GetDGIIComplianceStatusQuery, Dictionary<string, string>>
{
    private readonly IDGIISubmissionRepository _dgiiRepo;

    public GetDGIIComplianceStatusHandler(IDGIISubmissionRepository dgiiRepo)
    {
        _dgiiRepo = dgiiRepo;
    }

    public async Task<Dictionary<string, string>> Handle(GetDGIIComplianceStatusQuery request, CancellationToken ct)
    {
        return await _dgiiRepo.GetComplianceStatusAsync(request.RNC, request.Year, ct);
    }
}

public class GetPendingDGIISubmissionsHandler : IRequestHandler<GetPendingDGIISubmissionsQuery, List<DGIISubmissionDto>>
{
    private readonly IDGIISubmissionRepository _dgiiRepo;

    public GetPendingDGIISubmissionsHandler(IDGIISubmissionRepository dgiiRepo)
    {
        _dgiiRepo = dgiiRepo;
    }

    public async Task<List<DGIISubmissionDto>> Handle(GetPendingDGIISubmissionsQuery request, CancellationToken ct)
    {
        var subs = await _dgiiRepo.GetPendingAsync(ct);
        return subs.Select(s => new DGIISubmissionDto(
            s.Id, s.ReportId, s.ReportCode, s.RNC, s.Period,
            s.SubmissionDate, s.Status, s.ConfirmationNumber,
            s.ResponseMessage, s.Attempts
        )).ToList();
    }
}

#endregion

#region UAF Query Handlers

public class GetUAFSubmissionByIdHandler : IRequestHandler<GetUAFSubmissionByIdQuery, UAFSubmissionDto?>
{
    private readonly IUAFSubmissionRepository _uafRepo;

    public GetUAFSubmissionByIdHandler(IUAFSubmissionRepository uafRepo)
    {
        _uafRepo = uafRepo;
    }

    public async Task<UAFSubmissionDto?> Handle(GetUAFSubmissionByIdQuery request, CancellationToken ct)
    {
        var sub = await _uafRepo.GetByIdAsync(request.SubmissionId, ct);
        return sub == null ? null : new UAFSubmissionDto(
            sub.Id, sub.ReportId, sub.ReportCode, sub.EntityRNC,
            sub.ReportingPeriod, sub.SubmissionDate, sub.Status,
            sub.UAFCaseNumber, sub.IsUrgent);
    }
}

public class GetUAFSubmissionsByPeriodHandler : IRequestHandler<GetUAFSubmissionsByPeriodQuery, List<UAFSubmissionDto>>
{
    private readonly IUAFSubmissionRepository _uafRepo;

    public GetUAFSubmissionsByPeriodHandler(IUAFSubmissionRepository uafRepo)
    {
        _uafRepo = uafRepo;
    }

    public async Task<List<UAFSubmissionDto>> Handle(GetUAFSubmissionsByPeriodQuery request, CancellationToken ct)
    {
        var subs = await _uafRepo.GetByPeriodAsync(request.ReportingPeriod, ct);
        return subs.Select(s => new UAFSubmissionDto(
            s.Id, s.ReportId, s.ReportCode, s.EntityRNC,
            s.ReportingPeriod, s.SubmissionDate, s.Status,
            s.UAFCaseNumber, s.IsUrgent
        )).ToList();
    }
}

public class GetUrgentROSPendingHandler : IRequestHandler<GetUrgentROSPendingQuery, List<UAFSubmissionDto>>
{
    private readonly IUAFSubmissionRepository _uafRepo;

    public GetUrgentROSPendingHandler(IUAFSubmissionRepository uafRepo)
    {
        _uafRepo = uafRepo;
    }

    public async Task<List<UAFSubmissionDto>> Handle(GetUrgentROSPendingQuery request, CancellationToken ct)
    {
        var subs = await _uafRepo.GetUrgentPendingAsync(ct);
        return subs.Select(s => new UAFSubmissionDto(
            s.Id, s.ReportId, s.ReportCode, s.EntityRNC,
            s.ReportingPeriod, s.SubmissionDate, s.Status,
            s.UAFCaseNumber, s.IsUrgent
        )).ToList();
    }
}

public class GetUAFComplianceStatusHandler : IRequestHandler<GetUAFComplianceStatusQuery, Dictionary<string, object>>
{
    private readonly IUAFSubmissionRepository _uafRepo;

    public GetUAFComplianceStatusHandler(IUAFSubmissionRepository uafRepo)
    {
        _uafRepo = uafRepo;
    }

    public async Task<Dictionary<string, object>> Handle(GetUAFComplianceStatusQuery request, CancellationToken ct)
    {
        return await _uafRepo.GetComplianceStatusAsync(request.Year, ct);
    }
}

#endregion

#region Compliance Query Handlers

public class GetCurrentMetricsHandler : IRequestHandler<GetCurrentMetricsQuery, List<ComplianceMetricDto>>
{
    private readonly IComplianceMetricRepository _metricRepo;

    public GetCurrentMetricsHandler(IComplianceMetricRepository metricRepo)
    {
        _metricRepo = metricRepo;
    }

    public async Task<List<ComplianceMetricDto>> Handle(GetCurrentMetricsQuery request, CancellationToken ct)
    {
        var metrics = await _metricRepo.GetCurrentAsync(request.Category, ct);
        return metrics.Select(m => new ComplianceMetricDto(
            m.Id, m.MetricCode, m.MetricName, m.Category, m.Value,
            m.Threshold, m.Unit, m.MeasuredAt, m.IsAlert, m.AlertMessage
        )).ToList();
    }
}

public class GetComplianceDashboardHandler : IRequestHandler<GetComplianceDashboardQuery, ComplianceDashboardDto>
{
    private readonly IReportRepository _reportRepo;
    private readonly IComplianceMetricRepository _metricRepo;

    public GetComplianceDashboardHandler(
        IReportRepository reportRepo,
        IComplianceMetricRepository metricRepo)
    {
        _reportRepo = reportRepo;
        _metricRepo = metricRepo;
    }

    public async Task<ComplianceDashboardDto> Handle(GetComplianceDashboardQuery request, CancellationToken ct)
    {
        var stats = await _reportRepo.GetStatisticsAsync(request.AsOfDate.AddMonths(-1), request.AsOfDate, ct);
        var metrics = await _metricRepo.GetCurrentAsync(null, ct);
        var alerts = metrics.Where(m => m.IsAlert).ToList();

        var recentMetrics = metrics.OrderByDescending(m => m.MeasuredAt)
            .Take(10)
            .Select(m => new ComplianceMetricDto(
                m.Id, m.MetricCode, m.MetricName, m.Category, m.Value,
                m.Threshold, m.Unit, m.MeasuredAt, m.IsAlert, m.AlertMessage
            )).ToList();

        var reportsByType = stats.ByType;
        var alertsByCategory = alerts.GroupBy(a => a.Category)
            .ToDictionary(g => g.Key, g => g.Count());

        // Calcular score de cumplimiento
        var pendingCount = stats.TotalReports - stats.SubmittedReports - stats.AcceptedReports;
        var complianceScore = stats.TotalReports > 0 
            ? (int)((stats.AcceptedReports * 100.0 + stats.SubmittedReports * 50.0) / stats.TotalReports)
            : 100;

        return new ComplianceDashboardDto(
            stats.TotalReports,
            pendingCount,
            stats.TotalReports - stats.GeneratedReports,
            complianceScore,
            recentMetrics,
            reportsByType,
            alertsByCategory);
    }
}

public class GetReportingStatisticsHandler : IRequestHandler<GetReportingStatisticsQuery, ReportingStatisticsDto>
{
    private readonly IReportRepository _reportRepo;

    public GetReportingStatisticsHandler(IReportRepository reportRepo)
    {
        _reportRepo = reportRepo;
    }

    public async Task<ReportingStatisticsDto> Handle(GetReportingStatisticsQuery request, CancellationToken ct)
    {
        var stats = await _reportRepo.GetStatisticsAsync(request.FromDate, request.ToDate, ct);
        return new ReportingStatisticsDto(
            stats.TotalReports,
            stats.GeneratedReports,
            stats.SubmittedReports,
            stats.AcceptedReports,
            stats.RejectedReports,
            stats.AverageGenerationTimeMs,
            stats.ByType,
            stats.ByDestination);
    }
}

public class GetActiveAlertsHandler : IRequestHandler<GetActiveAlertsQuery, List<ComplianceMetricDto>>
{
    private readonly IComplianceMetricRepository _metricRepo;

    public GetActiveAlertsHandler(IComplianceMetricRepository metricRepo)
    {
        _metricRepo = metricRepo;
    }

    public async Task<List<ComplianceMetricDto>> Handle(GetActiveAlertsQuery request, CancellationToken ct)
    {
        var alerts = await _metricRepo.GetAlertsAsync(ct);
        return alerts.Select(m => new ComplianceMetricDto(
            m.Id, m.MetricCode, m.MetricName, m.Category, m.Value,
            m.Threshold, m.Unit, m.MeasuredAt, m.IsAlert, m.AlertMessage
        )).ToList();
    }
}

public class GetMetricHistoryHandler : IRequestHandler<GetMetricHistoryQuery, List<ComplianceMetricDto>>
{
    private readonly IComplianceMetricRepository _metricRepo;

    public GetMetricHistoryHandler(IComplianceMetricRepository metricRepo)
    {
        _metricRepo = metricRepo;
    }

    public async Task<List<ComplianceMetricDto>> Handle(GetMetricHistoryQuery request, CancellationToken ct)
    {
        var history = await _metricRepo.GetHistoryAsync(request.MetricCode, request.FromDate, request.ToDate, ct);
        return history.Select(m => new ComplianceMetricDto(
            m.Id, m.MetricCode, m.MetricName, m.Category, m.Value,
            m.Threshold, m.Unit, m.MeasuredAt, m.IsAlert, m.AlertMessage
        )).ToList();
    }
}

#endregion
