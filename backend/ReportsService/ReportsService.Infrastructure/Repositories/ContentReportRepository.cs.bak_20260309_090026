using Microsoft.EntityFrameworkCore;
using ReportsService.Domain.Entities;
using ReportsService.Domain.Interfaces;
using ReportsService.Infrastructure.Persistence;

namespace ReportsService.Infrastructure.Repositories;

public class ContentReportRepository : IContentReportRepository
{
    private readonly ReportsDbContext _context;

    public ContentReportRepository(ReportsDbContext context)
    {
        _context = context;
    }

    public async Task<ContentReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ContentReports
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<ContentReport> Items, int Total)> GetPaginatedAsync(
        ContentReportType? type = null,
        ContentReportStatus? status = null,
        ContentReportPriority? priority = null,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ContentReports.AsQueryable();

        if (type.HasValue)
            query = query.Where(r => r.Type == type.Value);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(r => r.Priority == priority.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(r =>
                r.TargetTitle.ToLower().Contains(searchLower) ||
                r.Reason.ToLower().Contains(searchLower) ||
                r.Description.ToLower().Contains(searchLower) ||
                r.ReportedByEmail.ToLower().Contains(searchLower));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.Priority)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items.AsReadOnly(), total);
    }

    public async Task<ContentReportStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var reports = _context.ContentReports;

        var total = await reports.CountAsync(cancellationToken);
        var pending = await reports.CountAsync(r => r.Status == ContentReportStatus.Pending, cancellationToken);
        var investigating = await reports.CountAsync(r => r.Status == ContentReportStatus.Investigating, cancellationToken);
        var resolved = await reports.CountAsync(r => r.Status == ContentReportStatus.Resolved, cancellationToken);
        var dismissed = await reports.CountAsync(r => r.Status == ContentReportStatus.Dismissed, cancellationToken);
        var highPriority = await reports.CountAsync(r => r.Priority == ContentReportPriority.High, cancellationToken);

        return new ContentReportStats(total, pending, investigating, resolved, dismissed, highPriority);
    }

    public async Task<ContentReport> AddAsync(ContentReport report, CancellationToken cancellationToken = default)
    {
        await _context.ContentReports.AddAsync(report, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return report;
    }

    public async Task UpdateAsync(ContentReport report, CancellationToken cancellationToken = default)
    {
        _context.ContentReports.Update(report);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await GetByIdAsync(id, cancellationToken);
        if (report != null)
        {
            _context.ContentReports.Remove(report);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ContentReports.AnyAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<ContentReport?> FindByTargetAndReporterAsync(
        string targetId, Guid reportedById,
        CancellationToken cancellationToken = default)
    {
        return await _context.ContentReports
            .FirstOrDefaultAsync(r =>
                r.TargetId == targetId &&
                r.ReportedById == reportedById &&
                r.Status != ContentReportStatus.Resolved &&
                r.Status != ContentReportStatus.Dismissed,
                cancellationToken);
    }
}
