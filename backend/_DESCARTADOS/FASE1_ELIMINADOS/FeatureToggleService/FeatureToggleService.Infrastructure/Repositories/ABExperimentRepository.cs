using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;
using FeatureToggleService.Domain.Interfaces;
using FeatureToggleService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FeatureToggleService.Infrastructure.Repositories;

public class ABExperimentRepository : IABExperimentRepository
{
    private readonly FeatureToggleDbContext _context;

    public ABExperimentRepository(FeatureToggleDbContext context)
    {
        _context = context;
    }

    public async Task<ABExperiment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ABExperiments
            .Include(e => e.Variants)
            .Include(e => e.Assignments)
            .Include(e => e.Metrics)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<ABExperiment?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.ABExperiments
            .Include(e => e.Variants)
            .Include(e => e.Assignments)
            .Include(e => e.Metrics)
            .FirstOrDefaultAsync(e => e.Key == key, cancellationToken);
    }

    public async Task<IEnumerable<ABExperiment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ABExperiments
            .Include(e => e.Variants)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ABExperiment>> GetByStatusAsync(ExperimentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.ABExperiments
            .Include(e => e.Variants)
            .Where(e => e.Status == status)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ABExperiment>> GetRunningAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.ABExperiments
            .Include(e => e.Variants)
            .Where(e => e.Status == ExperimentStatus.Running)
            .Where(e => e.StartDate != null && e.StartDate <= now)
            .Where(e => e.EndDate == null || e.EndDate > now)
            .OrderBy(e => e.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ABExperiment>> GetByFeatureFlagAsync(Guid featureFlagId, CancellationToken cancellationToken = default)
    {
        return await _context.ABExperiments
            .Include(e => e.Variants)
            .Where(e => e.FeatureFlagId == featureFlagId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ABExperiment> AddAsync(ABExperiment experiment, CancellationToken cancellationToken = default)
    {
        await _context.ABExperiments.AddAsync(experiment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return experiment;
    }

    public async Task<ABExperiment> UpdateAsync(ABExperiment experiment, CancellationToken cancellationToken = default)
    {
        _context.ABExperiments.Update(experiment);
        await _context.SaveChangesAsync(cancellationToken);
        return experiment;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var experiment = await _context.ABExperiments.FindAsync(new object[] { id }, cancellationToken);
        if (experiment == null)
            return false;

        _context.ABExperiments.Remove(experiment);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.ABExperiments
            .AnyAsync(e => e.Key == key, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ABExperiments.CountAsync(cancellationToken);
    }

    public async Task<int> CountByStatusAsync(ExperimentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.ABExperiments
            .CountAsync(e => e.Status == status, cancellationToken);
    }
}
