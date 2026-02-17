using Microsoft.EntityFrameworkCore;
using DataProtectionService.Domain.Entities;
using DataProtectionService.Domain.Interfaces;
using DataProtectionService.Infrastructure.Persistence;

namespace DataProtectionService.Infrastructure.Repositories;

public class DataChangeLogRepository : IDataChangeLogRepository
{
    private readonly DataProtectionDbContext _context;

    public DataChangeLogRepository(DataProtectionDbContext context)
    {
        _context = context;
    }

    public async Task<DataChangeLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DataChangeLogs
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<List<DataChangeLog>> GetByUserIdAsync(Guid userId, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.DataChangeLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.ChangedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DataChangeLog>> GetByCategoryAsync(Guid userId, string category, CancellationToken cancellationToken = default)
    {
        return await _context.DataChangeLogs
            .Where(l => l.UserId == userId && l.DataCategory == category)
            .OrderByDescending(l => l.ChangedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DataChangeLog>> GetByFieldAsync(Guid userId, string field, int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _context.DataChangeLogs
            .Where(l => l.UserId == userId && l.DataField == field)
            .OrderByDescending(l => l.ChangedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DataChangeLog log, CancellationToken cancellationToken = default)
    {
        await _context.DataChangeLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<DataChangeLog> logs, CancellationToken cancellationToken = default)
    {
        await _context.DataChangeLogs.AddRangeAsync(logs, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class PrivacyPolicyRepository : IPrivacyPolicyRepository
{
    private readonly DataProtectionDbContext _context;

    public PrivacyPolicyRepository(DataProtectionDbContext context)
    {
        _context = context;
    }

    public async Task<PrivacyPolicy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PrivacyPolicies
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PrivacyPolicy?> GetCurrentPolicyAsync(string language = "es", CancellationToken cancellationToken = default)
    {
        return await _context.PrivacyPolicies
            .Where(p => p.IsActive && p.Language == language && p.EffectiveDate <= DateTime.UtcNow)
            .OrderByDescending(p => p.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PrivacyPolicy?> GetByVersionAsync(string version, string language = "es", CancellationToken cancellationToken = default)
    {
        return await _context.PrivacyPolicies
            .FirstOrDefaultAsync(p => p.Version == version && p.Language == language, cancellationToken);
    }

    public async Task<List<PrivacyPolicy>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.PrivacyPolicies.AsQueryable();
        
        if (activeOnly)
            query = query.Where(p => p.IsActive);

        return await query.OrderByDescending(p => p.Version).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PrivacyPolicy policy, CancellationToken cancellationToken = default)
    {
        await _context.PrivacyPolicies.AddAsync(policy, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(PrivacyPolicy policy, CancellationToken cancellationToken = default)
    {
        _context.PrivacyPolicies.Update(policy);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class AnonymizationRecordRepository : IAnonymizationRecordRepository
{
    private readonly DataProtectionDbContext _context;

    public AnonymizationRecordRepository(DataProtectionDbContext context)
    {
        _context = context;
    }

    public async Task<AnonymizationRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AnonymizationRecords
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<AnonymizationRecord?> GetByOriginalUserIdAsync(Guid originalUserId, CancellationToken cancellationToken = default)
    {
        return await _context.AnonymizationRecords
            .FirstOrDefaultAsync(r => r.UserId == originalUserId, cancellationToken);
    }

    public async Task<bool> IsUserAnonymizedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.AnonymizationRecords
            .AnyAsync(r => r.UserId == userId, cancellationToken);
    }

    public async Task<List<AnonymizationRecord>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AnonymizationRecords.AsQueryable();
        
        if (fromDate.HasValue)
            query = query.Where(r => r.AnonymizedAt >= fromDate.Value);
        
        if (toDate.HasValue)
            query = query.Where(r => r.AnonymizedAt <= toDate.Value);
        
        return await query
            .OrderByDescending(r => r.AnonymizedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(AnonymizationRecord record, CancellationToken cancellationToken = default)
    {
        await _context.AnonymizationRecords.AddAsync(record, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<AnonymizationRecord>> GetExpiredRecordsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AnonymizationRecords
            .Where(r => r.RetentionEndDate < DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
}

public class DataExportRepository : IDataExportRepository
{
    private readonly DataProtectionDbContext _context;

    public DataExportRepository(DataProtectionDbContext context)
    {
        _context = context;
    }

    public async Task<DataExport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DataExports
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<DataExport>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.DataExports
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DataExport>> GetPendingExportsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DataExports
            .Where(e => e.Status == ExportStatus.Pending || e.Status == ExportStatus.Processing)
            .OrderBy(e => e.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(DataExport export, CancellationToken cancellationToken = default)
    {
        await _context.DataExports.AddAsync(export, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DataExport export, CancellationToken cancellationToken = default)
    {
        _context.DataExports.Update(export);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
