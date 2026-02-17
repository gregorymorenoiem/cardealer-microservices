// ComplianceReportingService - Repositories
// Implementación de repositorios para reportería regulatoria

namespace ComplianceReportingService.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using ComplianceReportingService.Application.DTOs;
using ComplianceReportingService.Domain.Entities;
using ComplianceReportingService.Domain.Interfaces;

#region Report Repository

public class ReportRepository : IReportRepository
{
    private readonly ReportingDbContext _context;

    public ReportRepository(ReportingDbContext context)
    {
        _context = context;
    }

    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Reports.FindAsync(new object[] { id }, ct);
    }

    public async Task<Report?> GetByNumberAsync(string reportNumber, CancellationToken ct = default)
    {
        return await _context.Reports
            .FirstOrDefaultAsync(r => r.ReportNumber == reportNumber, ct);
    }

    public async Task<Report?> GetByReportNumberAsync(string reportNumber, CancellationToken ct = default)
    {
        return await _context.Reports
            .FirstOrDefaultAsync(r => r.ReportNumber == reportNumber, ct);
    }

    public async Task<List<Report>> GetByTypeAsync(ReportType type, CancellationToken ct = default)
    {
        return await _context.Reports
            .Where(r => r.Type == type)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Report>> GetByStatusAsync(ReportStatus status, CancellationToken ct = default)
    {
        return await _context.Reports
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Report>> GetByPeriodAsync(DateTime start, DateTime end, CancellationToken ct = default)
    {
        return await _context.Reports
            .Where(r => r.PeriodStart >= start && r.PeriodEnd <= end)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Report>> GetByPeriodAsync(DateTime start, DateTime end, ReportType? type, CancellationToken ct = default)
    {
        var query = _context.Reports.Where(r => r.PeriodStart >= start && r.PeriodEnd <= end);
        if (type.HasValue) query = query.Where(r => r.Type == type.Value);
        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
    }

    public async Task<List<Report>> GetPendingSubmissionAsync(CancellationToken ct = default)
    {
        return await _context.Reports
            .Where(r => r.Status == ReportStatus.Generated)
            .OrderBy(r => r.DueDate)
            .ToListAsync(ct);
    }

    public async Task<List<Report>> GetPendingSubmissionAsync(DestinationType? destination, CancellationToken ct = default)
    {
        var query = _context.Reports.Where(r => r.Status == ReportStatus.Generated);
        if (destination.HasValue) query = query.Where(r => r.Destination == destination.Value);
        return await query.OrderBy(r => r.DueDate).ToListAsync(ct);
    }

    public async Task<List<Report>> GetOverdueAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Reports
            .Where(r => r.DueDate.HasValue && r.DueDate.Value <= now)
            .Where(r => r.Status != ReportStatus.Submitted && r.Status != ReportStatus.Accepted)
            .OrderBy(r => r.DueDate)
            .ToListAsync(ct);
    }

    public async Task<List<Report>> GetOverdueAsync(DateTime asOfDate, int daysAhead, CancellationToken ct = default)
    {
        var threshold = asOfDate.AddDays(daysAhead);
        return await _context.Reports
            .Where(r => r.DueDate.HasValue && r.DueDate.Value <= threshold)
            .Where(r => r.Status != ReportStatus.Submitted && r.Status != ReportStatus.Accepted)
            .OrderBy(r => r.DueDate)
            .ToListAsync(ct);
    }

    public async Task<(List<Report> Items, int Total)> GetPagedAsync(
        int page, int pageSize, ReportType? type = null, ReportStatus? status = null, CancellationToken ct = default)
    {
        var query = _context.Reports.AsQueryable();
        if (type.HasValue) query = query.Where(r => r.Type == type.Value);
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(List<Report> Items, int Total)> GetPagedAsync(
        int page, int pageSize, ReportType? type, ReportStatus? status,
        DestinationType? destination, DateTime? fromDate, DateTime? toDate,
        string? searchTerm, CancellationToken ct = default)
    {
        var query = _context.Reports.AsQueryable();

        if (type.HasValue) query = query.Where(r => r.Type == type.Value);
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);
        if (destination.HasValue) query = query.Where(r => r.Destination == destination.Value);
        if (fromDate.HasValue) query = query.Where(r => r.PeriodStart >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(r => r.PeriodEnd <= toDate.Value);
        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(r => r.Name.Contains(searchTerm) || r.ReportNumber.Contains(searchTerm));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<ReportingStatisticsInternal> GetStatisticsAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var reports = await _context.Reports
            .Where(r => r.CreatedAt >= from && r.CreatedAt <= to)
            .ToListAsync(ct);

        var executions = await _context.ReportExecutions
            .Where(e => e.StartedAt >= from && e.StartedAt <= to && e.Success)
            .ToListAsync(ct);

        var avgDuration = executions.Any() ? executions.Average(e => e.DurationMs) : 0;

        return new ReportingStatisticsInternal
        {
            TotalReports = reports.Count,
            GeneratedReports = reports.Count(r => r.Status == ReportStatus.Generated || r.Status == ReportStatus.Submitted || r.Status == ReportStatus.Accepted),
            SubmittedReports = reports.Count(r => r.Status == ReportStatus.Submitted || r.Status == ReportStatus.Accepted),
            AcceptedReports = reports.Count(r => r.Status == ReportStatus.Accepted),
            RejectedReports = reports.Count(r => r.Status == ReportStatus.Rejected),
            AverageGenerationTimeMs = (decimal)avgDuration,
            ByType = reports.GroupBy(r => r.Type).ToDictionary(g => g.Key, g => g.Count()),
            ByDestination = reports.GroupBy(r => r.Destination).ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public Task<string> GenerateReportNumberAsync(ReportType type, CancellationToken ct = default)
    {
        var prefix = type switch
        {
            ReportType.DGII_606 => "606",
            ReportType.DGII_607 => "607",
            ReportType.DGII_608 => "608",
            ReportType.DGII_609 => "609",
            ReportType.DGII_IT1 => "IT1",
            ReportType.UAF_ROS => "ROS",
            ReportType.UAF_CTR => "CTR",
            _ => "RPT"
        };
        return Task.FromResult($"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}");
    }

    public async Task AddAsync(Report report, CancellationToken ct = default)
    {
        await _context.Reports.AddAsync(report, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Report report, CancellationToken ct = default)
    {
        report.UpdatedAt = DateTime.UtcNow;
        _context.Reports.Update(report);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Schedule Repository

public class ReportScheduleRepository : IReportScheduleRepository
{
    private readonly ReportingDbContext _context;

    public ReportScheduleRepository(ReportingDbContext context)
    {
        _context = context;
    }

    public async Task<ReportSchedule?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ReportSchedules.FindAsync(new object[] { id }, ct);
    }

    public async Task<List<ReportSchedule>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _context.ReportSchedules
            .Where(s => s.IsActive)
            .OrderBy(s => s.NextRunAt)
            .ToListAsync(ct);
    }

    public async Task<List<ReportSchedule>> GetDueAsync(DateTime asOf, CancellationToken ct = default)
    {
        return await _context.ReportSchedules
            .Where(s => s.IsActive && s.NextRunAt.HasValue && s.NextRunAt.Value <= asOf)
            .OrderBy(s => s.NextRunAt)
            .ToListAsync(ct);
    }

    public async Task<List<ReportSchedule>> GetByTypeAsync(ReportType type, CancellationToken ct = default)
    {
        return await _context.ReportSchedules
            .Where(s => s.ReportType == type)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<List<ReportSchedule>> GetByReportTypeAsync(ReportType type, CancellationToken ct = default)
    {
        return await _context.ReportSchedules
            .Where(s => s.ReportType == type)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<List<ReportSchedule>> GetUpcomingAsync(DateTime until, CancellationToken ct = default)
    {
        return await _context.ReportSchedules
            .Where(s => s.IsActive && s.NextRunAt.HasValue && s.NextRunAt.Value <= until)
            .OrderBy(s => s.NextRunAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ReportSchedule schedule, CancellationToken ct = default)
    {
        await _context.ReportSchedules.AddAsync(schedule, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ReportSchedule schedule, CancellationToken ct = default)
    {
        _context.ReportSchedules.Update(schedule);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Template Repository

public class ReportTemplateRepository : IReportTemplateRepository
{
    private readonly ReportingDbContext _context;

    public ReportTemplateRepository(ReportingDbContext context)
    {
        _context = context;
    }

    public async Task<ReportTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ReportTemplates.FindAsync(new object[] { id }, ct);
    }

    public async Task<ReportTemplate?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.ReportTemplates
            .FirstOrDefaultAsync(t => t.Code == code, ct);
    }

    public async Task<List<ReportTemplate>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _context.ReportTemplates
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<List<ReportTemplate>> GetByTypeAsync(ReportType type, CancellationToken ct = default)
    {
        return await _context.ReportTemplates
            .Where(t => t.ForReportType == type && t.IsActive)
            .OrderBy(t => t.Version)
            .ToListAsync(ct);
    }

    public async Task<List<ReportTemplate>> GetByReportTypeAsync(ReportType forReportType, CancellationToken ct = default)
    {
        return await _context.ReportTemplates
            .Where(t => t.ForReportType == forReportType && t.IsActive)
            .OrderBy(t => t.Version)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ReportTemplate template, CancellationToken ct = default)
    {
        await _context.ReportTemplates.AddAsync(template, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ReportTemplate template, CancellationToken ct = default)
    {
        template.UpdatedAt = DateTime.UtcNow;
        _context.ReportTemplates.Update(template);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Execution Repository

public class ReportExecutionRepository : IReportExecutionRepository
{
    private readonly ReportingDbContext _context;

    public ReportExecutionRepository(ReportingDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReportExecution>> GetByReportAsync(Guid reportId, CancellationToken ct = default)
    {
        return await _context.ReportExecutions
            .Where(e => e.ReportId == reportId)
            .OrderByDescending(e => e.StartedAt)
            .ToListAsync(ct);
    }

    public async Task<List<ReportExecution>> GetByReportIdAsync(Guid reportId, CancellationToken ct = default)
    {
        return await _context.ReportExecutions
            .Where(e => e.ReportId == reportId)
            .OrderByDescending(e => e.StartedAt)
            .ToListAsync(ct);
    }

    public async Task<List<ReportExecution>> GetByScheduleIdAsync(Guid scheduleId, int limit, CancellationToken ct = default)
    {
        return await _context.ReportExecutions
            .Where(e => e.ScheduleId == scheduleId)
            .OrderByDescending(e => e.StartedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<List<ReportExecution>> GetRecentAsync(int count, CancellationToken ct = default)
    {
        return await _context.ReportExecutions
            .OrderByDescending(e => e.StartedAt)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<List<ReportExecution>> GetFailedAsync(CancellationToken ct = default)
    {
        return await _context.ReportExecutions
            .Where(e => !e.Success)
            .OrderByDescending(e => e.StartedAt)
            .ToListAsync(ct);
    }

    public async Task<List<ReportExecution>> GetFailedAsync(DateTime since, CancellationToken ct = default)
    {
        return await _context.ReportExecutions
            .Where(e => !e.Success && e.StartedAt >= since)
            .OrderByDescending(e => e.StartedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ReportExecution execution, CancellationToken ct = default)
    {
        await _context.ReportExecutions.AddAsync(execution, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ReportExecution execution, CancellationToken ct = default)
    {
        _context.ReportExecutions.Update(execution);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Subscription Repository

public class ReportSubscriptionRepository : IReportSubscriptionRepository
{
    private readonly ReportingDbContext _context;

    public ReportSubscriptionRepository(ReportingDbContext context)
    {
        _context = context;
    }

    public async Task<ReportSubscription?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ReportSubscriptions.FindAsync(new object[] { id }, ct);
    }

    public async Task<List<ReportSubscription>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.ReportSubscriptions
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.ReportType)
            .ToListAsync(ct);
    }

    public async Task<List<ReportSubscription>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.ReportSubscriptions
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.ReportType)
            .ToListAsync(ct);
    }

    public async Task<List<ReportSubscription>> GetByTypeAsync(ReportType type, CancellationToken ct = default)
    {
        return await _context.ReportSubscriptions
            .Where(s => s.ReportType == type && s.IsActive)
            .ToListAsync(ct);
    }

    public async Task<List<ReportSubscription>> GetByReportTypeAsync(ReportType type, CancellationToken ct = default)
    {
        return await _context.ReportSubscriptions
            .Where(s => s.ReportType == type && s.IsActive)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ReportSubscription subscription, CancellationToken ct = default)
    {
        await _context.ReportSubscriptions.AddAsync(subscription, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ReportSubscription subscription, CancellationToken ct = default)
    {
        _context.ReportSubscriptions.Update(subscription);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var sub = await _context.ReportSubscriptions.FindAsync(new object[] { id }, ct);
        if (sub != null)
        {
            _context.ReportSubscriptions.Remove(sub);
            await _context.SaveChangesAsync(ct);
        }
    }
}

#endregion

#region DGII Repository

public class DGIISubmissionRepository : IDGIISubmissionRepository
{
    private readonly ReportingDbContext _context;

    public DGIISubmissionRepository(ReportingDbContext context)
    {
        _context = context;
    }

    public async Task<DGIISubmission?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.DGIISubmissions.FindAsync(new object[] { id }, ct);
    }

    public async Task<List<DGIISubmission>> GetByReportAsync(Guid reportId, CancellationToken ct = default)
    {
        return await _context.DGIISubmissions
            .Where(s => s.ReportId == reportId)
            .OrderByDescending(s => s.SubmissionDate)
            .ToListAsync(ct);
    }

    public async Task<List<DGIISubmission>> GetPendingAsync(CancellationToken ct = default)
    {
        return await _context.DGIISubmissions
            .Where(s => s.Status == "Pendiente" || s.Status == "Enviado")
            .OrderBy(s => s.Period)
            .ToListAsync(ct);
    }

    public async Task<List<DGIISubmission>> GetByPeriodAsync(string period, CancellationToken ct = default)
    {
        return await _context.DGIISubmissions
            .Where(s => s.Period == period)
            .OrderByDescending(s => s.SubmissionDate)
            .ToListAsync(ct);
    }

    public async Task<List<DGIISubmission>> GetByPeriodAsync(string period, string? reportCode, CancellationToken ct = default)
    {
        var query = _context.DGIISubmissions.Where(s => s.Period == period);
        if (!string.IsNullOrEmpty(reportCode))
            query = query.Where(s => s.ReportCode == reportCode);
        return await query.OrderByDescending(s => s.SubmissionDate).ToListAsync(ct);
    }

    public async Task<Dictionary<string, string>> GetComplianceStatusAsync(string rnc, int year, CancellationToken ct = default)
    {
        var submissions = await _context.DGIISubmissions
            .Where(s => s.RNC == rnc && s.Period.StartsWith(year.ToString()))
            .ToListAsync(ct);

        var status = new Dictionary<string, string>();
        for (int month = 1; month <= 12; month++)
        {
            var period = $"{year}{month:D2}";
            var sub = submissions.FirstOrDefault(s => s.Period == period);
            status[period] = sub?.Status ?? "No Presentado";
        }
        return status;
    }

    public async Task AddAsync(DGIISubmission submission, CancellationToken ct = default)
    {
        await _context.DGIISubmissions.AddAsync(submission, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DGIISubmission submission, CancellationToken ct = default)
    {
        _context.DGIISubmissions.Update(submission);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region UAF Repository

public class UAFSubmissionRepository : IUAFSubmissionRepository
{
    private readonly ReportingDbContext _context;

    public UAFSubmissionRepository(ReportingDbContext context)
    {
        _context = context;
    }

    public async Task<UAFSubmission?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.UAFSubmissions.FindAsync(new object[] { id }, ct);
    }

    public async Task<List<UAFSubmission>> GetByReportAsync(Guid reportId, CancellationToken ct = default)
    {
        return await _context.UAFSubmissions
            .Where(s => s.ReportId == reportId)
            .OrderByDescending(s => s.SubmissionDate)
            .ToListAsync(ct);
    }

    public async Task<List<UAFSubmission>> GetPendingAsync(CancellationToken ct = default)
    {
        return await _context.UAFSubmissions
            .Where(s => s.Status == "Pendiente")
            .OrderBy(s => s.SubmissionDate)
            .ToListAsync(ct);
    }

    public async Task<List<UAFSubmission>> GetUrgentAsync(CancellationToken ct = default)
    {
        return await _context.UAFSubmissions
            .Where(s => s.IsUrgent)
            .OrderByDescending(s => s.SubmissionDate)
            .ToListAsync(ct);
    }

    public async Task<List<UAFSubmission>> GetByPeriodAsync(string reportingPeriod, CancellationToken ct = default)
    {
        return await _context.UAFSubmissions
            .Where(s => s.ReportingPeriod == reportingPeriod)
            .OrderByDescending(s => s.SubmissionDate)
            .ToListAsync(ct);
    }

    public async Task<List<UAFSubmission>> GetUrgentPendingAsync(CancellationToken ct = default)
    {
        return await _context.UAFSubmissions
            .Where(s => s.IsUrgent && s.Status == "Pendiente")
            .OrderBy(s => s.SubmissionDate)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<string, object>> GetComplianceStatusAsync(int year, CancellationToken ct = default)
    {
        var submissions = await _context.UAFSubmissions
            .Where(s => s.ReportingPeriod.StartsWith(year.ToString()))
            .ToListAsync(ct);

        return new Dictionary<string, object>
        {
            ["TotalSubmissions"] = submissions.Count,
            ["ROSCount"] = submissions.Count(s => s.ReportCode == "ROS"),
            ["CTRCount"] = submissions.Count(s => s.ReportCode == "CTR"),
            ["UrgentCount"] = submissions.Count(s => s.IsUrgent),
            ["PendingCount"] = submissions.Count(s => s.Status == "Pendiente")
        };
    }

    public async Task AddAsync(UAFSubmission submission, CancellationToken ct = default)
    {
        await _context.UAFSubmissions.AddAsync(submission, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(UAFSubmission submission, CancellationToken ct = default)
    {
        _context.UAFSubmissions.Update(submission);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Compliance Metric Repository

public class ComplianceMetricRepository : IComplianceMetricRepository
{
    private readonly ReportingDbContext _context;

    public ComplianceMetricRepository(ReportingDbContext context)
    {
        _context = context;
    }

    public async Task<List<ComplianceMetric>> GetByPeriodAsync(DateTime start, DateTime end, CancellationToken ct = default)
    {
        return await _context.ComplianceMetrics
            .Where(m => m.MeasuredAt >= start && m.MeasuredAt <= end)
            .OrderByDescending(m => m.MeasuredAt)
            .ToListAsync(ct);
    }

    public async Task<List<ComplianceMetric>> GetByCategoryAsync(string category, CancellationToken ct = default)
    {
        return await _context.ComplianceMetrics
            .Where(m => m.Category == category)
            .OrderByDescending(m => m.MeasuredAt)
            .ToListAsync(ct);
    }

    public async Task<List<ComplianceMetric>> GetAlertsAsync(CancellationToken ct = default)
    {
        return await _context.ComplianceMetrics
            .Where(m => m.IsAlert)
            .OrderByDescending(m => m.MeasuredAt)
            .Take(100)
            .ToListAsync(ct);
    }

    public async Task<ComplianceMetric?> GetLatestByCodeAsync(string metricCode, CancellationToken ct = default)
    {
        return await _context.ComplianceMetrics
            .Where(m => m.MetricCode == metricCode)
            .OrderByDescending(m => m.MeasuredAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<ComplianceMetric>> GetCurrentAsync(string? category, CancellationToken ct = default)
    {
        var query = _context.ComplianceMetrics.AsQueryable();
        if (!string.IsNullOrEmpty(category))
            query = query.Where(m => m.Category == category);

        // Obtener la métrica más reciente por código
        var latestIds = await query
            .GroupBy(m => m.MetricCode)
            .Select(g => g.OrderByDescending(m => m.MeasuredAt).First().Id)
            .ToListAsync(ct);

        return await _context.ComplianceMetrics
            .Where(m => latestIds.Contains(m.Id))
            .ToListAsync(ct);
    }

    public async Task<List<ComplianceMetric>> GetHistoryAsync(string metricCode, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        return await _context.ComplianceMetrics
            .Where(m => m.MetricCode == metricCode && m.MeasuredAt >= fromDate && m.MeasuredAt <= toDate)
            .OrderBy(m => m.MeasuredAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ComplianceMetric metric, CancellationToken ct = default)
    {
        await _context.ComplianceMetrics.AddAsync(metric, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddManyAsync(List<ComplianceMetric> metrics, CancellationToken ct = default)
    {
        await _context.ComplianceMetrics.AddRangeAsync(metrics, ct);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion
