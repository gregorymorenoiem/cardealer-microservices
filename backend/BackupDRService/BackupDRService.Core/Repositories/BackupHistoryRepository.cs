using BackupDRService.Core.Data;
using BackupDRService.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackupDRService.Core.Repositories;

public class BackupHistoryRepository : IBackupHistoryRepository
{
    private readonly BackupDbContext _context;

    public BackupHistoryRepository(BackupDbContext context)
    {
        _context = context;
    }

    public async Task<BackupHistory?> GetByIdAsync(int id)
    {
        return await _context.BackupHistories
            .Include(h => h.Schedule)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<BackupHistory?> GetByBackupIdAsync(string backupId)
    {
        return await _context.BackupHistories
            .Include(h => h.Schedule)
            .FirstOrDefaultAsync(h => h.BackupId == backupId);
    }

    public async Task<IEnumerable<BackupHistory>> GetAllAsync()
    {
        return await _context.BackupHistories
            .Include(h => h.Schedule)
            .OrderByDescending(h => h.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<BackupHistory>> GetByJobIdAsync(string jobId)
    {
        return await _context.BackupHistories
            .Where(h => h.JobId == jobId)
            .OrderByDescending(h => h.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<BackupHistory>> GetByDatabaseNameAsync(string databaseName)
    {
        return await _context.BackupHistories
            .Where(h => h.DatabaseName == databaseName)
            .OrderByDescending(h => h.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<BackupHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.BackupHistories
            .Where(h => h.StartedAt >= startDate && h.StartedAt <= endDate)
            .OrderByDescending(h => h.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<BackupHistory>> GetByStatusAsync(string status)
    {
        return await _context.BackupHistories
            .Where(h => h.Status == status)
            .OrderByDescending(h => h.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<BackupHistory>> GetRecentAsync(int count)
    {
        return await _context.BackupHistories
            .OrderByDescending(h => h.StartedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<BackupHistory> CreateAsync(BackupHistory history)
    {
        _context.BackupHistories.Add(history);
        await _context.SaveChangesAsync();
        return history;
    }

    public async Task<BackupHistory> UpdateAsync(BackupHistory history)
    {
        _context.BackupHistories.Update(history);
        await _context.SaveChangesAsync();
        return history;
    }

    public async Task DeleteAsync(int id)
    {
        var history = await _context.BackupHistories.FindAsync(id);
        if (history != null)
        {
            _context.BackupHistories.Remove(history);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<long> GetTotalStorageUsedAsync()
    {
        return await _context.BackupHistories
            .Where(h => h.Status == "Success")
            .SumAsync(h => h.FileSizeBytes);
    }

    public async Task<long> GetStorageUsedByDatabaseAsync(string databaseName)
    {
        return await _context.BackupHistories
            .Where(h => h.DatabaseName == databaseName && h.Status == "Success")
            .SumAsync(h => h.FileSizeBytes);
    }

    public async Task<int> GetBackupCountByStatusAsync(string status, DateTime? since = null)
    {
        var query = _context.BackupHistories.Where(h => h.Status == status);

        if (since.HasValue)
        {
            query = query.Where(h => h.StartedAt >= since.Value);
        }

        return await query.CountAsync();
    }
}
