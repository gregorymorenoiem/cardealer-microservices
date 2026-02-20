using Microsoft.EntityFrameworkCore;
using Video360Service.Domain.Entities;
using Video360Service.Domain.Enums;
using Video360Service.Domain.Interfaces;

namespace Video360Service.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio de registros de uso
/// </summary>
public class UsageRecordRepository : IUsageRecordRepository
{
    private readonly Video360DbContext _context;

    public UsageRecordRepository(Video360DbContext context)
    {
        _context = context;
    }

    public async Task<UsageRecord> CreateAsync(UsageRecord record, CancellationToken cancellationToken = default)
    {
        record.CreatedAt = DateTime.UtcNow;
        record.BillingPeriod = DateTime.UtcNow.ToString("yyyy-MM");
        
        _context.UsageRecords.Add(record);
        await _context.SaveChangesAsync(cancellationToken);
        
        return record;
    }

    public async Task<UsageRecord> AddAsync(UsageRecord record, CancellationToken cancellationToken = default)
    {
        return await CreateAsync(record, cancellationToken);
    }

    public async Task<IEnumerable<UsageRecord>> GetByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return await _context.UsageRecords
            .Where(r => r.Video360JobId == jobId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UsageRecord>> GetByUserIdAsync(Guid userId, string? billingPeriod = null, CancellationToken cancellationToken = default)
    {
        var query = _context.UsageRecords.Where(r => r.UserId == userId);
        
        if (!string.IsNullOrEmpty(billingPeriod))
        {
            query = query.Where(r => r.BillingPeriod == billingPeriod);
        }
        
        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UsageRecord>> GetByTenantIdAsync(string tenantId, string? billingPeriod = null, CancellationToken cancellationToken = default)
    {
        var query = _context.UsageRecords.Where(r => r.TenantId == tenantId);
        
        if (!string.IsNullOrEmpty(billingPeriod))
        {
            query = query.Where(r => r.BillingPeriod == billingPeriod);
        }
        
        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalCostByUserAsync(Guid userId, string billingPeriod, CancellationToken cancellationToken = default)
    {
        return await _context.UsageRecords
            .Where(r => r.UserId == userId && r.BillingPeriod == billingPeriod && r.IsSuccess)
            .SumAsync(r => r.CostUsd, cancellationToken);
    }

    public async Task<int> GetUsageCountByProviderAsync(Video360Provider provider, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.UsageRecords
            .CountAsync(r => r.Provider == provider && r.CreatedAt >= fromDate && r.CreatedAt <= toDate, cancellationToken);
    }

    public async Task<IEnumerable<UsageRecord>> GetUnbilledAsync(string billingPeriod, CancellationToken cancellationToken = default)
    {
        return await _context.UsageRecords
            .Where(r => r.BillingPeriod == billingPeriod && !r.IsBilled && r.IsSuccess)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsBilledAsync(IEnumerable<Guid> recordIds, string invoiceId, CancellationToken cancellationToken = default)
    {
        var records = await _context.UsageRecords
            .Where(r => recordIds.Contains(r.Id))
            .ToListAsync(cancellationToken);
        
        foreach (var record in records)
        {
            record.IsBilled = true;
            record.BilledAt = DateTime.UtcNow;
            record.InvoiceId = invoiceId;
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}
