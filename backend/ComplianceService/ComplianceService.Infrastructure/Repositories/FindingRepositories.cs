// ComplianceService - Finding and Remediation Repositories

namespace ComplianceService.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using ComplianceService.Domain.Entities;
using ComplianceService.Domain.Interfaces;
using ComplianceService.Infrastructure.Persistence;

#region Finding Repository

public class ComplianceFindingRepository : IComplianceFindingRepository
{
    private readonly ComplianceDbContext _context;

    public ComplianceFindingRepository(ComplianceDbContext context)
    {
        _context = context;
    }

    public async Task<ComplianceFinding?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ComplianceFindings
            .Include(f => f.RemediationActions)
            .FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    public async Task<IEnumerable<ComplianceFinding>> GetByAssessmentIdAsync(Guid assessmentId, CancellationToken ct = default)
    {
        return await _context.ComplianceFindings
            .Include(f => f.RemediationActions)
            .Where(f => f.AssessmentId == assessmentId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceFinding>> GetByStatusAsync(FindingStatus status, CancellationToken ct = default)
    {
        return await _context.ComplianceFindings
            .Include(f => f.RemediationActions)
            .Where(f => f.Status == status)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceFinding>> GetByTypeAsync(FindingType type, CancellationToken ct = default)
    {
        return await _context.ComplianceFindings
            .Include(f => f.RemediationActions)
            .Where(f => f.Type == type)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceFinding>> GetOverdueAsync(CancellationToken ct = default)
    {
        return await _context.ComplianceFindings
            .Where(f => f.DueDate != null && f.DueDate < DateTime.UtcNow &&
                       f.Status != FindingStatus.Resolved && f.Status != FindingStatus.Closed)
            .OrderBy(f => f.DueDate)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceFinding>> GetByAssignedToAsync(string userId, CancellationToken ct = default)
    {
        return await _context.ComplianceFindings
            .Where(f => f.AssignedTo == userId && f.Status != FindingStatus.Closed)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<(IEnumerable<ComplianceFinding> Items, int TotalCount)> GetPaginatedAsync(
        int page, int pageSize, FindingStatus? status = null, CancellationToken ct = default)
    {
        var query = _context.ComplianceFindings
            .Include(f => f.RemediationActions)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(f => f.Status == status.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(ComplianceFinding finding, CancellationToken ct = default)
    {
        await _context.ComplianceFindings.AddAsync(finding, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ComplianceFinding finding, CancellationToken ct = default)
    {
        _context.ComplianceFindings.Update(finding);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<FindingStatistics> GetStatisticsAsync(CancellationToken ct = default)
    {
        var findings = await _context.ComplianceFindings.ToListAsync(ct);
        var total = findings.Count;
        var resolved = findings.Where(f => f.ResolvedAt.HasValue).ToList();
        
        return new FindingStatistics
        {
            TotalFindings = total,
            OpenCount = findings.Count(f => f.Status == FindingStatus.Open),
            InProgressCount = findings.Count(f => f.Status == FindingStatus.InProgress),
            ResolvedCount = findings.Count(f => f.Status == FindingStatus.Resolved || f.Status == FindingStatus.Closed),
            OverdueCount = findings.Count(f => f.DueDate != null && f.DueDate < DateTime.UtcNow && 
                                               f.Status != FindingStatus.Resolved && f.Status != FindingStatus.Closed),
            CriticalCount = findings.Count(f => f.Criticality == CriticalityLevel.Critical),
            ResolutionRate = total > 0 
                ? (double)resolved.Count / total * 100 
                : 0,
            AverageResolutionDays = resolved.Any() 
                ? resolved.Average(f => (f.ResolvedAt!.Value - f.CreatedAt).TotalDays) 
                : 0
        };
    }
}

#endregion

#region Remediation Repository

public class RemediationActionRepository : IRemediationActionRepository
{
    private readonly ComplianceDbContext _context;

    public RemediationActionRepository(ComplianceDbContext context)
    {
        _context = context;
    }

    public async Task<RemediationAction?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.RemediationActions.FindAsync(new object[] { id }, ct);
    }

    public async Task<IEnumerable<RemediationAction>> GetByFindingIdAsync(Guid findingId, CancellationToken ct = default)
    {
        return await _context.RemediationActions
            .Where(a => a.FindingId == findingId)
            .OrderBy(a => a.Priority)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<RemediationAction>> GetByAssignedToAsync(string userId, CancellationToken ct = default)
    {
        return await _context.RemediationActions
            .Where(a => a.AssignedTo == userId && a.Status != TaskStatus.Completed && a.Status != TaskStatus.Cancelled)
            .OrderBy(a => a.DueDate)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<RemediationAction>> GetOverdueAsync(CancellationToken ct = default)
    {
        return await _context.RemediationActions
            .Where(a => a.DueDate != null && a.DueDate < DateTime.UtcNow &&
                       a.Status != TaskStatus.Completed && a.Status != TaskStatus.Cancelled)
            .OrderBy(a => a.DueDate)
            .ToListAsync(ct);
    }

    public async Task AddAsync(RemediationAction action, CancellationToken ct = default)
    {
        await _context.RemediationActions.AddAsync(action, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RemediationAction action, CancellationToken ct = default)
    {
        _context.RemediationActions.Update(action);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Report Repository

public class RegulatoryReportRepository : IRegulatoryReportRepository
{
    private readonly ComplianceDbContext _context;

    public RegulatoryReportRepository(ComplianceDbContext context)
    {
        _context = context;
    }

    public async Task<RegulatoryReport?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.RegulatoryReports.FindAsync(new object[] { id }, ct);
    }

    public async Task<RegulatoryReport?> GetByReportNumberAsync(string reportNumber, CancellationToken ct = default)
    {
        return await _context.RegulatoryReports
            .FirstOrDefaultAsync(r => r.ReportNumber == reportNumber, ct);
    }

    public async Task<IEnumerable<RegulatoryReport>> GetByTypeAsync(RegulatoryReportType type, CancellationToken ct = default)
    {
        return await _context.RegulatoryReports
            .Where(r => r.Type == type)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<RegulatoryReport>> GetByStatusAsync(ReportStatus status, CancellationToken ct = default)
    {
        return await _context.RegulatoryReports
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<RegulatoryReport>> GetByPeriodAsync(DateTime start, DateTime end, CancellationToken ct = default)
    {
        return await _context.RegulatoryReports
            .Where(r => r.PeriodStart >= start && r.PeriodEnd <= end)
            .OrderByDescending(r => r.PeriodEnd)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<RegulatoryReport>> GetPendingSubmissionAsync(CancellationToken ct = default)
    {
        return await _context.RegulatoryReports
            .Where(r => r.Status != ReportStatus.Submitted && r.Status != ReportStatus.Acknowledged)
            .OrderBy(r => r.SubmissionDeadline)
            .ToListAsync(ct);
    }

    public async Task<(IEnumerable<RegulatoryReport> Items, int TotalCount)> GetPaginatedAsync(
        int page, int pageSize, ReportStatus? status = null, CancellationToken ct = default)
    {
        var query = _context.RegulatoryReports.AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(RegulatoryReport report, CancellationToken ct = default)
    {
        await _context.RegulatoryReports.AddAsync(report, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RegulatoryReport report, CancellationToken ct = default)
    {
        _context.RegulatoryReports.Update(report);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<string> GenerateReportNumberAsync(RegulatoryReportType type, CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = type switch
        {
            RegulatoryReportType.AnnualCompliance => "ANC",
            RegulatoryReportType.QuarterlyPLD => "PLD",
            RegulatoryReportType.IncidentReport => "INC",
            RegulatoryReportType.AuditReport => "AUD",
            RegulatoryReportType.RiskAssessment => "RSK",
            RegulatoryReportType.TrainingReport => "TRN",
            RegulatoryReportType.TransactionReport => "TXN",
            RegulatoryReportType.UAFReport => "UAF",
            RegulatoryReportType.SIBReport => "SIB",
            RegulatoryReportType.DGIIReport => "DGI",
            _ => "RPT"
        };

        var count = await _context.RegulatoryReports
            .CountAsync(r => r.Type == type && r.CreatedAt.Year == year, ct);

        return $"{prefix}-{year}-{(count + 1):D4}";
    }
}

#endregion
