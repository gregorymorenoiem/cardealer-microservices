using Microsoft.EntityFrameworkCore;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;
using MessageBusService.Domain.Enums;
using MessageBusService.Infrastructure.Data;

namespace MessageBusService.Infrastructure.Repositories;

public class SagaRepository : ISagaRepository
{
    private readonly MessageBusDbContext _context;

    public SagaRepository(MessageBusDbContext context)
    {
        _context = context;
    }

    public async Task<Saga> CreateAsync(Saga saga, CancellationToken cancellationToken = default)
    {
        await _context.Sagas.AddAsync(saga, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return saga;
    }

    public async Task<Saga> UpdateAsync(Saga saga, CancellationToken cancellationToken = default)
    {
        _context.Sagas.Update(saga);
        await _context.SaveChangesAsync(cancellationToken);
        return saga;
    }

    public async Task<Saga?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sagas
            .Include(s => s.Steps.OrderBy(step => step.Order))
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Saga?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        return await _context.Sagas
            .Include(s => s.Steps.OrderBy(step => step.Order))
            .FirstOrDefaultAsync(s => s.CorrelationId == correlationId, cancellationToken);
    }

    public async Task<List<Saga>> GetByStatusAsync(SagaStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Sagas
            .Include(s => s.Steps.OrderBy(step => step.Order))
            .Where(s => s.Status == status)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Saga>> GetTimedOutSagasAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Sagas
            .Include(s => s.Steps.OrderBy(step => step.Order))
            .Where(s => s.Status == SagaStatus.Running && s.Timeout != null && s.StartedAt != null)
            .AsEnumerable()
            .Where(s => s.HasTimedOut())
            .ToList()
            .AsTask();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var saga = await _context.Sagas.FindAsync(new object[] { id }, cancellationToken);
        if (saga != null)
        {
            _context.Sagas.Remove(saga);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

// Extension to convert IEnumerable to Task
public static class EnumerableExtensions
{
    public static Task<List<T>> AsTask<T>(this List<T> list)
    {
        return Task.FromResult(list);
    }
}
