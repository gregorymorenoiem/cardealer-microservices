using AuditService.Domain.Entities;
using AuditService.Domain.Interfaces;
using AuditService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuditService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for persisting and querying audit events.
/// </summary>
public class AuditRepository : IAuditRepository
{
    private readonly AuditDbContext _context;

    public AuditRepository(AuditDbContext context)
    {
        _context = context;
    }

    public async Task SaveAuditEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        // CreatedAt is set automatically by EntityBase
        await _context.AuditEvents.AddAsync(auditEvent, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditEvent>> GetByEventTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        return await _context.AuditEvents
            .Where(e => e.EventType == eventType)
            .OrderByDescending(e => e.EventTimestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditEvent>> GetBySourceAsync(string source, CancellationToken cancellationToken = default)
    {
        return await _context.AuditEvents
            .Where(e => e.Source == source)
            .OrderByDescending(e => e.EventTimestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditEvent>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.AuditEvents
            .Where(e => e.EventTimestamp >= startDate && e.EventTimestamp <= endDate)
            .OrderByDescending(e => e.EventTimestamp)
            .ToListAsync(cancellationToken);
    }
}
