using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Interfaces;
using FeatureToggleService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FeatureToggleService.Infrastructure.Repositories;

public class ExperimentMetricRepository : IExperimentMetricRepository
{
    private readonly FeatureToggleDbContext _context;

    public ExperimentMetricRepository(FeatureToggleDbContext context)
    {
        _context = context;
    }

    public async Task<ExperimentMetric?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentMetrics
            .Include(m => m.Experiment)
            .Include(m => m.Variant)
            .Include(m => m.Assignment)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ExperimentMetric>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentMetrics
            .OrderByDescending(m => m.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExperimentMetric>> GetByExperimentAsync(Guid experimentId, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentMetrics
            .Where(m => m.ExperimentId == experimentId)
            .OrderBy(m => m.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExperimentMetric>> GetByVariantAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentMetrics
            .Where(m => m.VariantId == variantId)
            .OrderBy(m => m.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExperimentMetric>> GetByAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentMetrics
            .Where(m => m.AssignmentId == assignmentId)
            .OrderBy(m => m.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExperimentMetric>> GetByMetricKeyAsync(Guid experimentId, string metricKey, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentMetrics
            .Where(m => m.ExperimentId == experimentId && m.MetricKey == metricKey)
            .OrderBy(m => m.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExperimentMetric>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentMetrics
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ExperimentMetric> AddAsync(ExperimentMetric metric, CancellationToken cancellationToken = default)
    {
        await _context.ExperimentMetrics.AddAsync(metric, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return metric;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var metric = await _context.ExperimentMetrics.FindAsync(new object[] { id }, cancellationToken);
        if (metric == null)
            return false;

        _context.ExperimentMetrics.Remove(metric);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> CountByExperimentAsync(Guid experimentId, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentMetrics
            .CountAsync(m => m.ExperimentId == experimentId, cancellationToken);
    }

    public async Task<int> CountByVariantAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentMetrics
            .CountAsync(m => m.VariantId == variantId, cancellationToken);
    }

    public async Task<double> SumValueByVariantAsync(Guid variantId, string metricKey, CancellationToken cancellationToken = default)
    {
        var metrics = await _context.ExperimentMetrics
            .Where(m => m.VariantId == variantId && m.MetricKey == metricKey)
            .ToListAsync(cancellationToken);

        return metrics.Any() ? metrics.Sum(m => m.Value) : 0;
    }

    public async Task<double> AverageValueByVariantAsync(Guid variantId, string metricKey, CancellationToken cancellationToken = default)
    {
        var metrics = await _context.ExperimentMetrics
            .Where(m => m.VariantId == variantId && m.MetricKey == metricKey)
            .ToListAsync(cancellationToken);

        return metrics.Any() ? metrics.Average(m => m.Value) : 0;
    }
}
