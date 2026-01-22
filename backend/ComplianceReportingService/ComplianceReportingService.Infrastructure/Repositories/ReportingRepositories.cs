// =====================================================
// ComplianceReportingService - Repositories
// Reportes Consolidados de Cumplimiento RD
// =====================================================

using Microsoft.EntityFrameworkCore;
using ComplianceReportingService.Domain.Entities;
using ComplianceReportingService.Domain.Interfaces;
using ComplianceReportingService.Domain.Enums;
using ComplianceReportingService.Infrastructure.Persistence;

namespace ComplianceReportingService.Infrastructure.Repositories;

public class ComplianceReportRepository : IComplianceReportRepository
{
    private readonly ReportingDbContext _context;

    public ComplianceReportRepository(ReportingDbContext context) => _context = context;

    public async Task<ComplianceReport?> GetByIdAsync(Guid id) =>
        await _context.ComplianceReports
            .Include(r => r.Items)
            .Include(r => r.Submissions)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<ComplianceReport?> GetByReportNumberAsync(string reportNumber) =>
        await _context.ComplianceReports.FirstOrDefaultAsync(r => r.ReportNumber == reportNumber);

    public async Task<IEnumerable<ComplianceReport>> GetByRegulatoryBodyAsync(RegulatoryBody body) =>
        await _context.ComplianceReports
            .Where(r => r.RegulatoryBody == body)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<ComplianceReport>> GetByPeriodAsync(string period) =>
        await _context.ComplianceReports.Where(r => r.Period == period).ToListAsync();

    public async Task<IEnumerable<ComplianceReport>> GetByStatusAsync(ReportStatus status) =>
        await _context.ComplianceReports.Where(r => r.Status == status).ToListAsync();

    public async Task<IEnumerable<ComplianceReport>> GetPendingSubmissionAsync() =>
        await _context.ComplianceReports
            .Where(r => r.Status == ReportStatus.Generated || r.Status == ReportStatus.Validated)
            .ToListAsync();

    public async Task<ComplianceReport> AddAsync(ComplianceReport report)
    {
        await _context.ComplianceReports.AddAsync(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task UpdateAsync(ComplianceReport report)
    {
        _context.ComplianceReports.Update(report);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync() => await _context.ComplianceReports.CountAsync();
}

public class ReportItemRepository : IReportItemRepository
{
    private readonly ReportingDbContext _context;

    public ReportItemRepository(ReportingDbContext context) => _context = context;

    public async Task<ReportItem?> GetByIdAsync(Guid id) =>
        await _context.ReportItems.FindAsync(id);

    public async Task<IEnumerable<ReportItem>> GetByReportIdAsync(Guid reportId) =>
        await _context.ReportItems.Where(i => i.ComplianceReportId == reportId).ToListAsync();

    public async Task<ReportItem> AddAsync(ReportItem item)
    {
        await _context.ReportItems.AddAsync(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task AddRangeAsync(IEnumerable<ReportItem> items)
    {
        await _context.ReportItems.AddRangeAsync(items);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByReportIdAsync(Guid reportId)
    {
        var items = await _context.ReportItems.Where(i => i.ComplianceReportId == reportId).ToListAsync();
        _context.ReportItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }
}

public class ReportSubmissionRepository : IReportSubmissionRepository
{
    private readonly ReportingDbContext _context;

    public ReportSubmissionRepository(ReportingDbContext context) => _context = context;

    public async Task<ReportSubmission?> GetByIdAsync(Guid id) =>
        await _context.ReportSubmissions.FindAsync(id);

    public async Task<IEnumerable<ReportSubmission>> GetByReportIdAsync(Guid reportId) =>
        await _context.ReportSubmissions
            .Where(s => s.ComplianceReportId == reportId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();

    public async Task<ReportSubmission> AddAsync(ReportSubmission submission)
    {
        await _context.ReportSubmissions.AddAsync(submission);
        await _context.SaveChangesAsync();
        return submission;
    }
}

public class ReportScheduleRepository : IReportScheduleRepository
{
    private readonly ReportingDbContext _context;

    public ReportScheduleRepository(ReportingDbContext context) => _context = context;

    public async Task<ReportSchedule?> GetByIdAsync(Guid id) =>
        await _context.ReportSchedules.FindAsync(id);

    public async Task<IEnumerable<ReportSchedule>> GetActiveSchedulesAsync() =>
        await _context.ReportSchedules.Where(s => s.IsActive).ToListAsync();

    public async Task<IEnumerable<ReportSchedule>> GetDueSchedulesAsync() =>
        await _context.ReportSchedules
            .Where(s => s.IsActive && s.NextRunAt != null && s.NextRunAt <= DateTime.UtcNow)
            .ToListAsync();

    public async Task<ReportSchedule> AddAsync(ReportSchedule schedule)
    {
        await _context.ReportSchedules.AddAsync(schedule);
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task UpdateAsync(ReportSchedule schedule)
    {
        _context.ReportSchedules.Update(schedule);
        await _context.SaveChangesAsync();
    }
}

public class ReportTemplateRepository : IReportTemplateRepository
{
    private readonly ReportingDbContext _context;

    public ReportTemplateRepository(ReportingDbContext context) => _context = context;

    public async Task<ReportTemplate?> GetByIdAsync(Guid id) =>
        await _context.ReportTemplates.FindAsync(id);

    public async Task<ReportTemplate?> GetActiveTemplateAsync(ReportType type, RegulatoryBody body) =>
        await _context.ReportTemplates
            .Where(t => t.ReportType == type && t.RegulatoryBody == body && t.IsActive)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<ReportTemplate>> GetByRegulatoryBodyAsync(RegulatoryBody body) =>
        await _context.ReportTemplates.Where(t => t.RegulatoryBody == body).ToListAsync();

    public async Task<ReportTemplate> AddAsync(ReportTemplate template)
    {
        await _context.ReportTemplates.AddAsync(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task UpdateAsync(ReportTemplate template)
    {
        _context.ReportTemplates.Update(template);
        await _context.SaveChangesAsync();
    }
}
