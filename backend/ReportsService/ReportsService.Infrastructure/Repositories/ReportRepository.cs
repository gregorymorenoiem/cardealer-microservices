using Microsoft.EntityFrameworkCore;
using ReportsService.Domain.Entities;
using ReportsService.Domain.Interfaces;
using ReportsService.Infrastructure.Persistence;

namespace ReportsService.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly ReportsDbContext _context;

    public ReportRepository(ReportsDbContext context)
    {
        _context = context;
    }

    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reports.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Report>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reports.OrderByDescending(r => r.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Report>> GetByTypeAsync(ReportType type, CancellationToken cancellationToken = default)
    {
        return await _context.Reports.Where(r => r.Type == type).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Report>> GetByStatusAsync(ReportStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Reports.Where(r => r.Status == status).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Report>> GetReadyAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reports.Where(r => r.Status == ReportStatus.Ready).ToListAsync(cancellationToken);
    }

    public async Task<Report> AddAsync(Report report, CancellationToken cancellationToken = default)
    {
        await _context.Reports.AddAsync(report, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return report;
    }

    public async Task UpdateAsync(Report report, CancellationToken cancellationToken = default)
    {
        _context.Reports.Update(report);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await GetByIdAsync(id, cancellationToken);
        if (report != null)
        {
            _context.Reports.Remove(report);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reports.AnyAsync(r => r.Id == id, cancellationToken);
    }
}
