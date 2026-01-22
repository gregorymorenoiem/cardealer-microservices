// ComplianceService - Calendar, Training and Metric Repositories

namespace ComplianceService.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using ComplianceService.Domain.Entities;
using ComplianceService.Domain.Interfaces;
using ComplianceService.Infrastructure.Persistence;

#region Calendar Repository

public class ComplianceCalendarRepository : IComplianceCalendarRepository
{
    private readonly ComplianceDbContext _context;

    public ComplianceCalendarRepository(ComplianceDbContext context)
    {
        _context = context;
    }

    public async Task<ComplianceCalendar?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ComplianceCalendars.FindAsync(new object[] { id }, ct);
    }

    public async Task<IEnumerable<ComplianceCalendar>> GetUpcomingAsync(int daysAhead, CancellationToken ct = default)
    {
        var futureDate = DateTime.UtcNow.AddDays(daysAhead);
        return await _context.ComplianceCalendars
            .Where(c => c.DueDate >= DateTime.UtcNow && c.DueDate <= futureDate &&
                       c.Status != TaskStatus.Completed && c.Status != TaskStatus.Cancelled)
            .OrderBy(c => c.DueDate)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceCalendar>> GetByAssignedToAsync(string userId, CancellationToken ct = default)
    {
        return await _context.ComplianceCalendars
            .Where(c => c.AssignedTo == userId && c.Status != TaskStatus.Completed && c.Status != TaskStatus.Cancelled)
            .OrderBy(c => c.DueDate)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceCalendar>> GetOverdueAsync(CancellationToken ct = default)
    {
        return await _context.ComplianceCalendars
            .Where(c => c.DueDate < DateTime.UtcNow &&
                       c.Status != TaskStatus.Completed && c.Status != TaskStatus.Cancelled)
            .OrderBy(c => c.DueDate)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceCalendar>> GetByRegulationTypeAsync(RegulationType type, CancellationToken ct = default)
    {
        return await _context.ComplianceCalendars
            .Where(c => c.RegulationType == type)
            .OrderBy(c => c.DueDate)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ComplianceCalendar item, CancellationToken ct = default)
    {
        await _context.ComplianceCalendars.AddAsync(item, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ComplianceCalendar item, CancellationToken ct = default)
    {
        _context.ComplianceCalendars.Update(item);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var item = await GetByIdAsync(id, ct);
        if (item != null)
        {
            _context.ComplianceCalendars.Remove(item);
            await _context.SaveChangesAsync(ct);
        }
    }
}

#endregion

#region Training Repository

public class ComplianceTrainingRepository : IComplianceTrainingRepository
{
    private readonly ComplianceDbContext _context;

    public ComplianceTrainingRepository(ComplianceDbContext context)
    {
        _context = context;
    }

    public async Task<ComplianceTraining?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ComplianceTrainings
            .Include(t => t.Completions)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<IEnumerable<ComplianceTraining>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await _context.ComplianceTrainings
            .Include(t => t.Completions)
            .Where(t => t.IsActive)
            .OrderBy(t => t.Title)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceTraining>> GetByRegulationTypeAsync(RegulationType type, CancellationToken ct = default)
    {
        return await _context.ComplianceTrainings
            .Include(t => t.Completions)
            .Where(t => t.RegulationType == type && t.IsActive)
            .OrderBy(t => t.Title)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceTraining>> GetMandatoryAsync(CancellationToken ct = default)
    {
        return await _context.ComplianceTrainings
            .Include(t => t.Completions)
            .Where(t => t.IsMandatory && t.IsActive)
            .OrderBy(t => t.Title)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ComplianceTraining training, CancellationToken ct = default)
    {
        await _context.ComplianceTrainings.AddAsync(training, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ComplianceTraining training, CancellationToken ct = default)
    {
        _context.ComplianceTrainings.Update(training);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Training Completion Repository

public class TrainingCompletionRepository : ITrainingCompletionRepository
{
    private readonly ComplianceDbContext _context;

    public TrainingCompletionRepository(ComplianceDbContext context)
    {
        _context = context;
    }

    public async Task<TrainingCompletion?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.TrainingCompletions
            .Include(c => c.Training)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IEnumerable<TrainingCompletion>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.TrainingCompletions
            .Include(c => c.Training)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CompletedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<TrainingCompletion>> GetByTrainingIdAsync(Guid trainingId, CancellationToken ct = default)
    {
        return await _context.TrainingCompletions
            .Where(c => c.TrainingId == trainingId)
            .OrderByDescending(c => c.CompletedAt)
            .ToListAsync(ct);
    }

    public async Task<TrainingCompletion?> GetByUserAndTrainingAsync(Guid userId, Guid trainingId, CancellationToken ct = default)
    {
        return await _context.TrainingCompletions
            .Include(c => c.Training)
            .Where(c => c.UserId == userId && c.TrainingId == trainingId)
            .OrderByDescending(c => c.CompletedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<TrainingCompletion>> GetExpiringAsync(int daysAhead, CancellationToken ct = default)
    {
        var futureDate = DateTime.UtcNow.AddDays(daysAhead);
        return await _context.TrainingCompletions
            .Include(c => c.Training)
            .Where(c => c.ExpiresAt != null && c.ExpiresAt <= futureDate && c.ExpiresAt > DateTime.UtcNow)
            .OrderBy(c => c.ExpiresAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(TrainingCompletion completion, CancellationToken ct = default)
    {
        await _context.TrainingCompletions.AddAsync(completion, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TrainingCompletion completion, CancellationToken ct = default)
    {
        _context.TrainingCompletions.Update(completion);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<TrainingStatistics> GetStatisticsAsync(CancellationToken ct = default)
    {
        var trainings = await _context.ComplianceTrainings.Where(t => t.IsActive).CountAsync(ct);
        var completions = await _context.TrainingCompletions.ToListAsync(ct);
        
        return new TrainingStatistics
        {
            TotalTrainings = trainings,
            TotalCompletions = completions.Count,
            PassedCount = completions.Count(c => c.IsPassed),
            FailedCount = completions.Count(c => !c.IsPassed),
            ExpiringSoonCount = completions.Count(c => c.ExpiresAt != null && 
                                                       c.ExpiresAt <= DateTime.UtcNow.AddDays(30) && 
                                                       c.ExpiresAt > DateTime.UtcNow),
            PassRate = completions.Any() 
                ? (double)completions.Count(c => c.IsPassed) / completions.Count * 100 
                : 0
        };
    }
}

#endregion

#region Metric Repository

public class ComplianceMetricRepository : IComplianceMetricRepository
{
    private readonly ComplianceDbContext _context;

    public ComplianceMetricRepository(ComplianceDbContext context)
    {
        _context = context;
    }

    public async Task<ComplianceMetric?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ComplianceMetrics.FindAsync(new object[] { id }, ct);
    }

    public async Task<IEnumerable<ComplianceMetric>> GetByRegulationTypeAsync(RegulationType type, CancellationToken ct = default)
    {
        return await _context.ComplianceMetrics
            .Where(m => m.RegulationType == type)
            .OrderByDescending(m => m.PeriodEnd)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceMetric>> GetByPeriodAsync(DateTime start, DateTime end, CancellationToken ct = default)
    {
        return await _context.ComplianceMetrics
            .Where(m => m.PeriodStart >= start && m.PeriodEnd <= end)
            .OrderByDescending(m => m.PeriodEnd)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceMetric>> GetLatestByMetricNameAsync(string metricName, int count = 12, CancellationToken ct = default)
    {
        return await _context.ComplianceMetrics
            .Where(m => m.MetricName == metricName)
            .OrderByDescending(m => m.PeriodEnd)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ComplianceMetric>> GetOutOfTargetAsync(CancellationToken ct = default)
    {
        return await _context.ComplianceMetrics
            .Where(m => !m.IsWithinTarget && m.Target.HasValue)
            .OrderByDescending(m => m.CalculatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ComplianceMetric metric, CancellationToken ct = default)
    {
        await _context.ComplianceMetrics.AddAsync(metric, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<ComplianceMetric> metrics, CancellationToken ct = default)
    {
        await _context.ComplianceMetrics.AddRangeAsync(metrics, ct);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion
