using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities.Privacy;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Persistence;

/// <summary>
/// Implements IConsentRecordRepository using EF Core.
/// Stores immutable consent change records for Ley 172-13 audit compliance.
/// </summary>
public class ConsentRecordRepository : IConsentRecordRepository
{
    private readonly ApplicationDbContext _context;

    public ConsentRecordRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ConsentRecord record)
    {
        record.Timestamp = DateTime.UtcNow;
        await _context.ConsentRecords.AddAsync(record);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ConsentRecord>> GetByUserIdAsync(Guid userId, int limit = 100)
    {
        return await _context.ConsentRecords
            .Where(cr => cr.UserId == userId)
            .OrderByDescending(cr => cr.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<ConsentRecord?> GetLatestAsync(Guid userId, string consentType)
    {
        return await _context.ConsentRecords
            .Where(cr => cr.UserId == userId && cr.ConsentType == consentType)
            .OrderByDescending(cr => cr.Timestamp)
            .FirstOrDefaultAsync();
    }
}
