using CRMService.Domain.Entities;
using CRMService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Infrastructure.Persistence.Repositories;

public class PipelineRepository : IPipelineRepository
{
    private readonly CRMDbContext _context;

    public PipelineRepository(CRMDbContext context)
    {
        _context = context;
    }

    public async Task<Pipeline?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Pipelines.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Pipeline?> GetByIdWithStagesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Pipelines
            .Include(p => p.Stages.OrderBy(s => s.Order))
                .ThenInclude(s => s.Deals)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Pipeline>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Pipelines
            .Include(p => p.Stages.OrderBy(s => s.Order))
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.IsDefault)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Pipeline?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Pipelines
            .Include(p => p.Stages.OrderBy(s => s.Order))
            .FirstOrDefaultAsync(p => p.IsDefault && p.IsActive, cancellationToken);
    }

    public async Task<Pipeline> AddAsync(Pipeline pipeline, CancellationToken cancellationToken = default)
    {
        await _context.Pipelines.AddAsync(pipeline, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return pipeline;
    }

    public async Task UpdateAsync(Pipeline pipeline, CancellationToken cancellationToken = default)
    {
        _context.Pipelines.Update(pipeline);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pipeline = await _context.Pipelines.FindAsync(new object[] { id }, cancellationToken);
        if (pipeline != null)
        {
            _context.Pipelines.Remove(pipeline);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Pipelines.AnyAsync(p => p.Id == id, cancellationToken);
    }
}
