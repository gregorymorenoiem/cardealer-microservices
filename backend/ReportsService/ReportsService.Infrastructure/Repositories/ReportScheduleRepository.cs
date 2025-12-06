using Microsoft.EntityFrameworkCore;
using ReportsService.Domain.Entities;
using ReportsService.Domain.Interfaces;
using ReportsService.Infrastructure.Persistence;

namespace ReportsService.Infrastructure.Repositories;

public class ReportScheduleRepository : IReportScheduleRepository
{
    private readonly ReportsDbContext _context;

    public ReportScheduleRepository(ReportsDbContext context)
    {
        _context = context;
    }

    public async Task<ReportSchedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ReportSchedules
            .Include(s => s.Report)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ReportSchedule>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ReportSchedules.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ReportSchedule>> GetByReportIdAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        return await _context.ReportSchedules
            .Where(s => s.ReportId == reportId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ReportSchedule>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ReportSchedules.Where(s => s.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ReportSchedule>> GetDueAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.ReportSchedules
            .Where(s => s.IsActive && s.NextRunAt != null && s.NextRunAt <= now)
            .ToListAsync(cancellationToken);
    }

    public async Task<ReportSchedule> AddAsync(ReportSchedule schedule, CancellationToken cancellationToken = default)
    {
        await _context.ReportSchedules.AddAsync(schedule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return schedule;
    }

    public async Task UpdateAsync(ReportSchedule schedule, CancellationToken cancellationToken = default)
    {
        _context.ReportSchedules.Update(schedule);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var schedule = await GetByIdAsync(id, cancellationToken);
        if (schedule != null)
        {
            _context.ReportSchedules.Remove(schedule);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ReportSchedules.AnyAsync(s => s.Id == id, cancellationToken);
    }
}
