using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Interfaces;
using FeatureToggleService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FeatureToggleService.Infrastructure.Repositories;

public class ExperimentAssignmentRepository : IExperimentAssignmentRepository
{
    private readonly FeatureToggleDbContext _context;

    public ExperimentAssignmentRepository(FeatureToggleDbContext context)
    {
        _context = context;
    }

    public async Task<ExperimentAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentAssignments
            .Include(a => a.Experiment)
            .Include(a => a.Variant)
            .Include(a => a.Metrics)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<ExperimentAssignment?> GetByExperimentAndUserAsync(Guid experimentId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentAssignments
            .Include(a => a.Variant)
            .FirstOrDefaultAsync(a => a.ExperimentId == experimentId && a.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<ExperimentAssignment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentAssignments
            .Include(a => a.Variant)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExperimentAssignment>> GetByExperimentAsync(Guid experimentId, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentAssignments
            .Include(a => a.Variant)
            .Where(a => a.ExperimentId == experimentId)
            .OrderBy(a => a.AssignedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExperimentAssignment>> GetByVariantAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentAssignments
            .Where(a => a.VariantId == variantId)
            .OrderBy(a => a.AssignedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExperimentAssignment>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentAssignments
            .Include(a => a.Experiment)
            .Include(a => a.Variant)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExperimentAssignment>> GetExposedAsync(Guid experimentId, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentAssignments
            .Include(a => a.Variant)
            .Where(a => a.ExperimentId == experimentId && a.IsExposed)
            .OrderBy(a => a.ExposedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ExperimentAssignment>> GetConvertedAsync(Guid experimentId, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentAssignments
            .Include(a => a.Variant)
            .Where(a => a.ExperimentId == experimentId && a.HasConverted)
            .OrderBy(a => a.ConvertedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ExperimentAssignment> AddAsync(ExperimentAssignment assignment, CancellationToken cancellationToken = default)
    {
        await _context.ExperimentAssignments.AddAsync(assignment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return assignment;
    }

    public async Task<ExperimentAssignment> UpdateAsync(ExperimentAssignment assignment, CancellationToken cancellationToken = default)
    {
        _context.ExperimentAssignments.Update(assignment);
        await _context.SaveChangesAsync(cancellationToken);
        return assignment;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await _context.ExperimentAssignments.FindAsync(new object[] { id }, cancellationToken);
        if (assignment == null)
            return false;

        _context.ExperimentAssignments.Remove(assignment);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> CountByExperimentAsync(Guid experimentId, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentAssignments
            .CountAsync(a => a.ExperimentId == experimentId, cancellationToken);
    }

    public async Task<int> CountByVariantAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        return await _context.ExperimentAssignments
            .CountAsync(a => a.VariantId == variantId, cancellationToken);
    }
}
