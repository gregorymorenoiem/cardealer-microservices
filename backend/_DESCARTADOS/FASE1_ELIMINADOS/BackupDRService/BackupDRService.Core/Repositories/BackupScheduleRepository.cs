using BackupDRService.Core.Data;
using BackupDRService.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackupDRService.Core.Repositories;

public class BackupScheduleRepository : IBackupScheduleRepository
{
    private readonly BackupDbContext _context;

    public BackupScheduleRepository(BackupDbContext context)
    {
        _context = context;
    }

    public async Task<BackupSchedule?> GetByIdAsync(int id)
    {
        return await _context.BackupSchedules
            .Include(s => s.RetentionPolicy)
            .Include(s => s.BackupHistories.OrderByDescending(h => h.StartedAt).Take(10))
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<BackupSchedule?> GetByNameAsync(string name)
    {
        return await _context.BackupSchedules
            .Include(s => s.RetentionPolicy)
            .FirstOrDefaultAsync(s => s.Name == name);
    }

    public async Task<IEnumerable<BackupSchedule>> GetAllAsync()
    {
        return await _context.BackupSchedules
            .Include(s => s.RetentionPolicy)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<BackupSchedule>> GetEnabledAsync()
    {
        return await _context.BackupSchedules
            .Include(s => s.RetentionPolicy)
            .Where(s => s.IsEnabled)
            .OrderBy(s => s.NextRunAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<BackupSchedule>> GetByDatabaseNameAsync(string databaseName)
    {
        return await _context.BackupSchedules
            .Where(s => s.DatabaseName == databaseName)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<BackupSchedule>> GetDueForExecutionAsync(DateTime currentTime)
    {
        return await _context.BackupSchedules
            .Include(s => s.RetentionPolicy)
            .Where(s => s.IsEnabled &&
                       s.NextRunAt.HasValue &&
                       s.NextRunAt.Value <= currentTime)
            .ToListAsync();
    }

    public async Task<BackupSchedule> CreateAsync(BackupSchedule schedule)
    {
        _context.BackupSchedules.Add(schedule);
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task<BackupSchedule> UpdateAsync(BackupSchedule schedule)
    {
        schedule.UpdatedAt = DateTime.UtcNow;
        _context.BackupSchedules.Update(schedule);
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task DeleteAsync(int id)
    {
        var schedule = await _context.BackupSchedules.FindAsync(id);
        if (schedule != null)
        {
            _context.BackupSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateLastRunAsync(int id, DateTime lastRun, DateTime nextRun)
    {
        var schedule = await _context.BackupSchedules.FindAsync(id);
        if (schedule != null)
        {
            schedule.LastRunAt = lastRun;
            schedule.NextRunAt = nextRun;
            schedule.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateSuccessCountAsync(int id)
    {
        var schedule = await _context.BackupSchedules.FindAsync(id);
        if (schedule != null)
        {
            schedule.SuccessCount++;
            schedule.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateFailureCountAsync(int id)
    {
        var schedule = await _context.BackupSchedules.FindAsync(id);
        if (schedule != null)
        {
            schedule.FailureCount++;
            schedule.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
