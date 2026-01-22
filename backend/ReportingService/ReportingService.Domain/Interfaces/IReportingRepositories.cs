// ReportingService - Repository Interfaces

namespace ReportingService.Domain.Interfaces;

using ReportingService.Domain.Entities;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Report?> GetByNumberAsync(string reportNumber, CancellationToken ct = default);
    Task<Report?> GetByReportNumberAsync(string reportNumber, CancellationToken ct = default);
    Task<List<Report>> GetByTypeAsync(ReportType type, CancellationToken ct = default);
    Task<List<Report>> GetByStatusAsync(ReportStatus status, CancellationToken ct = default);
    Task<List<Report>> GetByPeriodAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<List<Report>> GetByPeriodAsync(DateTime start, DateTime end, ReportType? type, CancellationToken ct = default);
    Task<List<Report>> GetPendingSubmissionAsync(CancellationToken ct = default);
    Task<List<Report>> GetPendingSubmissionAsync(DestinationType? destination, CancellationToken ct = default);
    Task<List<Report>> GetOverdueAsync(CancellationToken ct = default);
    Task<List<Report>> GetOverdueAsync(DateTime asOfDate, int daysAhead, CancellationToken ct = default);
    Task<(List<Report> Items, int Total)> GetPagedAsync(int page, int pageSize, ReportType? type = null, ReportStatus? status = null, CancellationToken ct = default);
    Task<(List<Report> Items, int Total)> GetPagedAsync(int page, int pageSize, ReportType? type, ReportStatus? status, DestinationType? destination, DateTime? fromDate, DateTime? toDate, string? searchTerm, CancellationToken ct = default);
    Task<ReportingStatisticsInternal> GetStatisticsAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    Task AddAsync(Report report, CancellationToken ct = default);
    Task UpdateAsync(Report report, CancellationToken ct = default);
    Task<string> GenerateReportNumberAsync(ReportType type, CancellationToken ct = default);
}

public interface IReportScheduleRepository
{
    Task<ReportSchedule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ReportSchedule>> GetActiveAsync(CancellationToken ct = default);
    Task<List<ReportSchedule>> GetDueAsync(DateTime asOf, CancellationToken ct = default);
    Task<List<ReportSchedule>> GetByTypeAsync(ReportType type, CancellationToken ct = default);
    Task<List<ReportSchedule>> GetByReportTypeAsync(ReportType type, CancellationToken ct = default);
    Task<List<ReportSchedule>> GetUpcomingAsync(DateTime until, CancellationToken ct = default);
    Task AddAsync(ReportSchedule schedule, CancellationToken ct = default);
    Task UpdateAsync(ReportSchedule schedule, CancellationToken ct = default);
}

public interface IReportTemplateRepository
{
    Task<ReportTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ReportTemplate?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<List<ReportTemplate>> GetActiveAsync(CancellationToken ct = default);
    Task<List<ReportTemplate>> GetByTypeAsync(ReportType type, CancellationToken ct = default);
    Task<List<ReportTemplate>> GetByReportTypeAsync(ReportType forReportType, CancellationToken ct = default);
    Task AddAsync(ReportTemplate template, CancellationToken ct = default);
    Task UpdateAsync(ReportTemplate template, CancellationToken ct = default);
}

public interface IReportExecutionRepository
{
    Task<List<ReportExecution>> GetByReportAsync(Guid reportId, CancellationToken ct = default);
    Task<List<ReportExecution>> GetByReportIdAsync(Guid reportId, CancellationToken ct = default);
    Task<List<ReportExecution>> GetByScheduleIdAsync(Guid scheduleId, int limit, CancellationToken ct = default);
    Task<List<ReportExecution>> GetRecentAsync(int count, CancellationToken ct = default);
    Task<List<ReportExecution>> GetFailedAsync(CancellationToken ct = default);
    Task<List<ReportExecution>> GetFailedAsync(DateTime since, CancellationToken ct = default);
    Task AddAsync(ReportExecution execution, CancellationToken ct = default);
    Task UpdateAsync(ReportExecution execution, CancellationToken ct = default);
}

public interface IReportSubscriptionRepository
{
    Task<ReportSubscription?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ReportSubscription>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<List<ReportSubscription>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<List<ReportSubscription>> GetByTypeAsync(ReportType type, CancellationToken ct = default);
    Task<List<ReportSubscription>> GetByReportTypeAsync(ReportType type, CancellationToken ct = default);
    Task AddAsync(ReportSubscription subscription, CancellationToken ct = default);
    Task UpdateAsync(ReportSubscription subscription, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IDGIISubmissionRepository
{
    Task<DGIISubmission?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<DGIISubmission>> GetByReportAsync(Guid reportId, CancellationToken ct = default);
    Task<List<DGIISubmission>> GetPendingAsync(CancellationToken ct = default);
    Task<List<DGIISubmission>> GetByPeriodAsync(string period, CancellationToken ct = default);
    Task<List<DGIISubmission>> GetByPeriodAsync(string period, string? reportCode, CancellationToken ct = default);
    Task<Dictionary<string, string>> GetComplianceStatusAsync(string rnc, int year, CancellationToken ct = default);
    Task AddAsync(DGIISubmission submission, CancellationToken ct = default);
    Task UpdateAsync(DGIISubmission submission, CancellationToken ct = default);
}

public interface IUAFSubmissionRepository
{
    Task<UAFSubmission?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<UAFSubmission>> GetByReportAsync(Guid reportId, CancellationToken ct = default);
    Task<List<UAFSubmission>> GetPendingAsync(CancellationToken ct = default);
    Task<List<UAFSubmission>> GetUrgentAsync(CancellationToken ct = default);
    Task<List<UAFSubmission>> GetByPeriodAsync(string reportingPeriod, CancellationToken ct = default);
    Task<List<UAFSubmission>> GetUrgentPendingAsync(CancellationToken ct = default);
    Task<Dictionary<string, object>> GetComplianceStatusAsync(int year, CancellationToken ct = default);
    Task AddAsync(UAFSubmission submission, CancellationToken ct = default);
    Task UpdateAsync(UAFSubmission submission, CancellationToken ct = default);
}

public interface IComplianceMetricRepository
{
    Task<List<ComplianceMetric>> GetByPeriodAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<List<ComplianceMetric>> GetByCategoryAsync(string category, CancellationToken ct = default);
    Task<List<ComplianceMetric>> GetAlertsAsync(CancellationToken ct = default);
    Task<ComplianceMetric?> GetLatestByCodeAsync(string metricCode, CancellationToken ct = default);
    Task<List<ComplianceMetric>> GetCurrentAsync(string? category, CancellationToken ct = default);
    Task<List<ComplianceMetric>> GetHistoryAsync(string metricCode, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    Task AddAsync(ComplianceMetric metric, CancellationToken ct = default);
    Task AddManyAsync(List<ComplianceMetric> metrics, CancellationToken ct = default);
}

// DTO para estadísticas de reportes (Domain-level interface)
public class ReportingStatisticsInternal
{
    public int TotalReports { get; set; }
    public int GeneratedReports { get; set; }
    public int SubmittedReports { get; set; }
    public int AcceptedReports { get; set; }
    public int RejectedReports { get; set; }
    public decimal AverageGenerationTimeMs { get; set; }
    public Dictionary<ReportType, int> ByType { get; set; } = new();
    public Dictionary<DestinationType, int> ByDestination { get; set; } = new();
}

