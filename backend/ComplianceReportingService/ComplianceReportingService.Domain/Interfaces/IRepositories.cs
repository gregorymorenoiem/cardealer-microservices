// =====================================================
// ComplianceReportingService - Interfaces
// Reportes Consolidados de Cumplimiento RD
// =====================================================

using ComplianceReportingService.Domain.Entities;
using ComplianceReportingService.Domain.Enums;

namespace ComplianceReportingService.Domain.Interfaces;

public interface IComplianceReportRepository
{
    Task<ComplianceReport?> GetByIdAsync(Guid id);
    Task<ComplianceReport?> GetByReportNumberAsync(string reportNumber);
    Task<IEnumerable<ComplianceReport>> GetByRegulatoryBodyAsync(RegulatoryBody body);
    Task<IEnumerable<ComplianceReport>> GetByPeriodAsync(string period);
    Task<IEnumerable<ComplianceReport>> GetByStatusAsync(ReportStatus status);
    Task<IEnumerable<ComplianceReport>> GetPendingSubmissionAsync();
    Task<ComplianceReport> AddAsync(ComplianceReport report);
    Task UpdateAsync(ComplianceReport report);
    Task<int> GetCountAsync();
}

public interface IReportItemRepository
{
    Task<ReportItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReportItem>> GetByReportIdAsync(Guid reportId);
    Task<ReportItem> AddAsync(ReportItem item);
    Task AddRangeAsync(IEnumerable<ReportItem> items);
    Task DeleteByReportIdAsync(Guid reportId);
}

public interface IReportSubmissionRepository
{
    Task<ReportSubmission?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReportSubmission>> GetByReportIdAsync(Guid reportId);
    Task<ReportSubmission> AddAsync(ReportSubmission submission);
}

public interface IReportScheduleRepository
{
    Task<ReportSchedule?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReportSchedule>> GetActiveSchedulesAsync();
    Task<IEnumerable<ReportSchedule>> GetDueSchedulesAsync();
    Task<ReportSchedule> AddAsync(ReportSchedule schedule);
    Task UpdateAsync(ReportSchedule schedule);
}

public interface IReportTemplateRepository
{
    Task<ReportTemplate?> GetByIdAsync(Guid id);
    Task<ReportTemplate?> GetActiveTemplateAsync(ReportType type, RegulatoryBody body);
    Task<IEnumerable<ReportTemplate>> GetByRegulatoryBodyAsync(RegulatoryBody body);
    Task<ReportTemplate> AddAsync(ReportTemplate template);
    Task UpdateAsync(ReportTemplate template);
}
