// EscrowService - Repository Implementations

namespace EscrowService.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using EscrowService.Domain.Entities;
using EscrowService.Domain.Interfaces;

#region EscrowAccount Repository

public class EscrowAccountRepository : IEscrowAccountRepository
{
    private readonly EscrowDbContext _context;

    public EscrowAccountRepository(EscrowDbContext context)
    {
        _context = context;
    }

    public async Task<EscrowAccount?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.EscrowAccounts.FindAsync(new object[] { id }, ct);
    }

    public async Task<EscrowAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default)
    {
        return await _context.EscrowAccounts.FirstOrDefaultAsync(e => e.AccountNumber == accountNumber, ct);
    }

    public async Task<EscrowAccount?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.EscrowAccounts.FindAsync(new object[] { id }, ct);
    }

    public async Task<List<EscrowAccount>> GetByBuyerIdAsync(Guid buyerId, CancellationToken ct = default)
    {
        return await _context.EscrowAccounts.Where(e => e.BuyerId == buyerId).OrderByDescending(e => e.CreatedAt).ToListAsync(ct);
    }

    public async Task<List<EscrowAccount>> GetBySellerIdAsync(Guid sellerId, CancellationToken ct = default)
    {
        return await _context.EscrowAccounts.Where(e => e.SellerId == sellerId).OrderByDescending(e => e.CreatedAt).ToListAsync(ct);
    }

    public async Task<List<EscrowAccount>> GetByStatusAsync(EscrowStatus status, CancellationToken ct = default)
    {
        return await _context.EscrowAccounts.Where(e => e.Status == status).ToListAsync(ct);
    }

    public async Task<(List<EscrowAccount> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, EscrowStatus? status = null, EscrowTransactionType? type = null, CancellationToken ct = default)
    {
        var query = _context.EscrowAccounts.AsQueryable();
        
        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);
        if (type.HasValue)
            query = query.Where(e => e.TransactionType == type.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<List<EscrowAccount>> GetExpiringAsync(DateTime before, CancellationToken ct = default)
    {
        return await _context.EscrowAccounts
            .Where(e => e.ExpiresAt <= before && e.Status == EscrowStatus.Funded)
            .ToListAsync(ct);
    }

    public async Task<List<EscrowAccount>> GetPendingReleaseAsync(CancellationToken ct = default)
    {
        return await _context.EscrowAccounts
            .Where(e => e.Status == EscrowStatus.ConditionsMet && e.BuyerApproved && e.SellerApproved)
            .ToListAsync(ct);
    }

    public async Task<int> GetCountByStatusAsync(EscrowStatus status, CancellationToken ct = default)
    {
        return await _context.EscrowAccounts.CountAsync(e => e.Status == status, ct);
    }

    public async Task<EscrowAccount> AddAsync(EscrowAccount account, CancellationToken ct = default)
    {
        _context.EscrowAccounts.Add(account);
        await _context.SaveChangesAsync(ct);
        return account;
    }

    public async Task UpdateAsync(EscrowAccount account, CancellationToken ct = default)
    {
        _context.EscrowAccounts.Update(account);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<decimal> GetTotalFundedAmountAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _context.EscrowAccounts
            .Where(e => e.FundedAt >= from && e.FundedAt <= to)
            .SumAsync(e => e.FundedAmount, ct);
    }

    public async Task<string> GenerateAccountNumberAsync(CancellationToken ct = default)
    {
        var prefix = "ESC";
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await _context.EscrowAccounts.CountAsync(ct) + 1;
        return $"{prefix}-{date}-{count:D6}";
    }
}

#endregion

#region ReleaseCondition Repository

public class ReleaseConditionRepository : IReleaseConditionRepository
{
    private readonly EscrowDbContext _context;

    public ReleaseConditionRepository(EscrowDbContext context)
    {
        _context = context;
    }

    public async Task<ReleaseCondition?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ReleaseConditions.FindAsync(new object[] { id }, ct);
    }

    public async Task<List<ReleaseCondition>> GetByEscrowAccountIdAsync(Guid escrowAccountId, CancellationToken ct = default)
    {
        return await _context.ReleaseConditions
            .Where(c => c.EscrowAccountId == escrowAccountId)
            .OrderBy(c => c.Order)
            .ToListAsync(ct);
    }

    public async Task<List<ReleaseCondition>> GetPendingByEscrowAccountIdAsync(Guid escrowAccountId, CancellationToken ct = default)
    {
        return await _context.ReleaseConditions
            .Where(c => c.EscrowAccountId == escrowAccountId && c.Status == ConditionStatus.Pending)
            .ToListAsync(ct);
    }

    public async Task<bool> AllMandatoryConditionsMetAsync(Guid escrowAccountId, CancellationToken ct = default)
    {
        var mandatoryConditions = await _context.ReleaseConditions
            .Where(c => c.EscrowAccountId == escrowAccountId && c.IsMandatory)
            .ToListAsync(ct);

        return mandatoryConditions.All(c => c.Status == ConditionStatus.Met || c.Status == ConditionStatus.Waived);
    }

    public async Task<ReleaseCondition> AddAsync(ReleaseCondition condition, CancellationToken ct = default)
    {
        _context.ReleaseConditions.Add(condition);
        await _context.SaveChangesAsync(ct);
        return condition;
    }

    public async Task UpdateAsync(ReleaseCondition condition, CancellationToken ct = default)
    {
        _context.ReleaseConditions.Update(condition);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var condition = await _context.ReleaseConditions.FindAsync(new object[] { id }, ct);
        if (condition != null)
        {
            _context.ReleaseConditions.Remove(condition);
            await _context.SaveChangesAsync(ct);
        }
    }
}

#endregion

#region FundMovement Repository

public class FundMovementRepository : IFundMovementRepository
{
    private readonly EscrowDbContext _context;

    public FundMovementRepository(EscrowDbContext context)
    {
        _context = context;
    }

    public async Task<FundMovement?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.FundMovements.FindAsync(new object[] { id }, ct);
    }

    public async Task<FundMovement?> GetByTransactionNumberAsync(string transactionNumber, CancellationToken ct = default)
    {
        return await _context.FundMovements.FirstOrDefaultAsync(m => m.TransactionNumber == transactionNumber, ct);
    }

    public async Task<List<FundMovement>> GetByEscrowAccountIdAsync(Guid escrowAccountId, CancellationToken ct = default)
    {
        return await _context.FundMovements
            .Where(m => m.EscrowAccountId == escrowAccountId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<FundMovement>> GetByTypeAsync(FundMovementType type, DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _context.FundMovements
            .Where(m => m.Type == type && m.CreatedAt >= from && m.CreatedAt <= to)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetTotalByTypeAsync(Guid escrowAccountId, FundMovementType type, CancellationToken ct = default)
    {
        return await _context.FundMovements
            .Where(m => m.EscrowAccountId == escrowAccountId && m.Type == type && m.Status == "Completed")
            .SumAsync(m => m.Amount, ct);
    }

    public async Task<FundMovement> AddAsync(FundMovement movement, CancellationToken ct = default)
    {
        _context.FundMovements.Add(movement);
        await _context.SaveChangesAsync(ct);
        return movement;
    }

    public async Task UpdateAsync(FundMovement movement, CancellationToken ct = default)
    {
        _context.FundMovements.Update(movement);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<string> GenerateTransactionNumberAsync(CancellationToken ct = default)
    {
        var prefix = "TXN";
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await _context.FundMovements.CountAsync(ct) + 1;
        return $"{prefix}-{date}-{count:D8}";
    }
}

#endregion

#region EscrowDocument Repository

public class EscrowDocumentRepository : IEscrowDocumentRepository
{
    private readonly EscrowDbContext _context;

    public EscrowDocumentRepository(EscrowDbContext context)
    {
        _context = context;
    }

    public async Task<EscrowDocument?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.EscrowDocuments.FindAsync(new object[] { id }, ct);
    }

    public async Task<List<EscrowDocument>> GetByEscrowAccountIdAsync(Guid escrowAccountId, CancellationToken ct = default)
    {
        return await _context.EscrowDocuments
            .Where(d => d.EscrowAccountId == escrowAccountId && !d.IsDeleted)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(ct);
    }

    public async Task<List<EscrowDocument>> GetByTypeAsync(Guid escrowAccountId, string documentType, CancellationToken ct = default)
    {
        return await _context.EscrowDocuments
            .Where(d => d.EscrowAccountId == escrowAccountId && d.DocumentType == documentType && !d.IsDeleted)
            .ToListAsync(ct);
    }

    public async Task<EscrowDocument> AddAsync(EscrowDocument document, CancellationToken ct = default)
    {
        _context.EscrowDocuments.Add(document);
        await _context.SaveChangesAsync(ct);
        return document;
    }

    public async Task UpdateAsync(EscrowDocument document, CancellationToken ct = default)
    {
        _context.EscrowDocuments.Update(document);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var document = await _context.EscrowDocuments.FindAsync(new object[] { id }, ct);
        if (document != null)
        {
            document.IsDeleted = true;
            document.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }
}

#endregion

#region EscrowDispute Repository

public class EscrowDisputeRepository : IEscrowDisputeRepository
{
    private readonly EscrowDbContext _context;

    public EscrowDisputeRepository(EscrowDbContext context)
    {
        _context = context;
    }

    public async Task<EscrowDispute?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.EscrowDisputes.FindAsync(new object[] { id }, ct);
    }

    public async Task<EscrowDispute?> GetByDisputeNumberAsync(string disputeNumber, CancellationToken ct = default)
    {
        return await _context.EscrowDisputes.FirstOrDefaultAsync(d => d.DisputeNumber == disputeNumber, ct);
    }

    public async Task<List<EscrowDispute>> GetByEscrowAccountIdAsync(Guid escrowAccountId, CancellationToken ct = default)
    {
        return await _context.EscrowDisputes
            .Where(d => d.EscrowAccountId == escrowAccountId)
            .OrderByDescending(d => d.FiledAt)
            .ToListAsync(ct);
    }

    public async Task<List<EscrowDispute>> GetByStatusAsync(EscrowDisputeStatus status, CancellationToken ct = default)
    {
        return await _context.EscrowDisputes.Where(d => d.Status == status).ToListAsync(ct);
    }

    public async Task<List<EscrowDispute>> GetAssignedToAsync(string assignedTo, CancellationToken ct = default)
    {
        return await _context.EscrowDisputes.Where(d => d.AssignedTo == assignedTo).ToListAsync(ct);
    }

    public async Task<(List<EscrowDispute> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, EscrowDisputeStatus? status = null, CancellationToken ct = default)
    {
        var query = _context.EscrowDisputes.AsQueryable();
        
        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(d => d.FiledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<EscrowDispute> AddAsync(EscrowDispute dispute, CancellationToken ct = default)
    {
        _context.EscrowDisputes.Add(dispute);
        await _context.SaveChangesAsync(ct);
        return dispute;
    }

    public async Task UpdateAsync(EscrowDispute dispute, CancellationToken ct = default)
    {
        _context.EscrowDisputes.Update(dispute);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<string> GenerateDisputeNumberAsync(CancellationToken ct = default)
    {
        var prefix = "DSP";
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await _context.EscrowDisputes.CountAsync(ct) + 1;
        return $"{prefix}-{date}-{count:D6}";
    }
}

#endregion

#region EscrowAuditLog Repository

public class EscrowAuditLogRepository : IEscrowAuditLogRepository
{
    private readonly EscrowDbContext _context;

    public EscrowAuditLogRepository(EscrowDbContext context)
    {
        _context = context;
    }

    public async Task<List<EscrowAuditLog>> GetByEscrowAccountIdAsync(Guid escrowAccountId, CancellationToken ct = default)
    {
        return await _context.EscrowAuditLogs
            .Where(l => l.EscrowAccountId == escrowAccountId)
            .OrderByDescending(l => l.PerformedAt)
            .ToListAsync(ct);
    }

    public async Task<List<EscrowAuditLog>> GetByEventTypeAsync(EscrowAuditEventType eventType, DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _context.EscrowAuditLogs
            .Where(l => l.EventType == eventType && l.PerformedAt >= from && l.PerformedAt <= to)
            .OrderByDescending(l => l.PerformedAt)
            .ToListAsync(ct);
    }

    public async Task<EscrowAuditLog> AddAsync(EscrowAuditLog log, CancellationToken ct = default)
    {
        _context.EscrowAuditLogs.Add(log);
        await _context.SaveChangesAsync(ct);
        return log;
    }
}

#endregion

#region EscrowFeeConfiguration Repository

public class EscrowFeeConfigurationRepository : IEscrowFeeConfigurationRepository
{
    private readonly EscrowDbContext _context;

    public EscrowFeeConfigurationRepository(EscrowDbContext context)
    {
        _context = context;
    }

    public async Task<EscrowFeeConfiguration?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.EscrowFeeConfigurations.FindAsync(new object[] { id }, ct);
    }

    public async Task<EscrowFeeConfiguration?> GetActiveForAmountAsync(EscrowTransactionType type, decimal amount, CancellationToken ct = default)
    {
        return await _context.EscrowFeeConfigurations
            .Where(c => c.TransactionType == type 
                     && c.IsActive 
                     && c.MinAmount <= amount 
                     && c.MaxAmount >= amount
                     && c.EffectiveFrom <= DateTime.UtcNow
                     && (c.EffectiveTo == null || c.EffectiveTo >= DateTime.UtcNow))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<EscrowFeeConfiguration>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await _context.EscrowFeeConfigurations
            .Where(c => c.IsActive)
            .OrderBy(c => c.TransactionType)
            .ThenBy(c => c.MinAmount)
            .ToListAsync(ct);
    }

    public async Task<EscrowFeeConfiguration> AddAsync(EscrowFeeConfiguration config, CancellationToken ct = default)
    {
        _context.EscrowFeeConfigurations.Add(config);
        await _context.SaveChangesAsync(ct);
        return config;
    }

    public async Task UpdateAsync(EscrowFeeConfiguration config, CancellationToken ct = default)
    {
        _context.EscrowFeeConfigurations.Update(config);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion
