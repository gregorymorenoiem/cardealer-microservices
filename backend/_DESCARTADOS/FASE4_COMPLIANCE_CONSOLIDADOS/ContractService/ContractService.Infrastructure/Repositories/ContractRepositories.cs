// ContractService - Repository Implementations

namespace ContractService.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using ContractService.Domain.Entities;
using ContractService.Domain.Interfaces;
using ContractService.Infrastructure.Persistence;

#region Contract Template Repository

public class ContractTemplateRepository : IContractTemplateRepository
{
    private readonly ContractDbContext _context;

    public ContractTemplateRepository(ContractDbContext context)
    {
        _context = context;
    }

    public async Task<ContractTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ContractTemplates
            .Include(t => t.Clauses.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<ContractTemplate?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.ContractTemplates
            .Include(t => t.Clauses.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(t => t.Code == code, ct);
    }

    public async Task<List<ContractTemplate>> GetByTypeAsync(ContractType type, CancellationToken ct = default)
    {
        return await _context.ContractTemplates
            .Include(t => t.Clauses.OrderBy(c => c.Order))
            .Where(t => t.Type == type && t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<List<ContractTemplate>> GetActiveTemplatesAsync(CancellationToken ct = default)
    {
        return await _context.ContractTemplates
            .Include(t => t.Clauses.OrderBy(c => c.Order))
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.ContractTemplates.AnyAsync(t => t.Code == code, ct);
    }

    public async Task AddAsync(ContractTemplate template, CancellationToken ct = default)
    {
        await _context.ContractTemplates.AddAsync(template, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ContractTemplate template, CancellationToken ct = default)
    {
        _context.ContractTemplates.Update(template);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Contract Repository

public class ContractRepository : IContractRepository
{
    private readonly ContractDbContext _context;

    public ContractRepository(ContractDbContext context)
    {
        _context = context;
    }

    public async Task<Contract?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Contracts
            .Include(c => c.Parties)
            .Include(c => c.Signatures)
            .Include(c => c.Clauses.OrderBy(cl => cl.Order))
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Contract?> GetByContractNumberAsync(string contractNumber, CancellationToken ct = default)
    {
        return await _context.Contracts
            .Include(c => c.Parties)
            .Include(c => c.Signatures)
            .FirstOrDefaultAsync(c => c.ContractNumber == contractNumber, ct);
    }

    public async Task<List<Contract>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        return await _context.Contracts
            .Include(c => c.Parties)
            .Where(c => c.Parties.Any(p => p.UserId == userId))
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<List<Contract>> GetBySubjectAsync(string subjectType, Guid subjectId, CancellationToken ct = default)
    {
        return await _context.Contracts
            .Where(c => c.SubjectType == subjectType && c.SubjectId == subjectId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<(List<Contract> Items, int Total)> GetByStatusAsync(ContractStatus status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Contracts.Where(c => c.Status == status);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<List<Contract>> GetExpiringContractsAsync(int daysUntilExpiration, CancellationToken ct = default)
    {
        var expirationDate = DateTime.UtcNow.AddDays(daysUntilExpiration);
        return await _context.Contracts
            .Where(c => c.ExpirationDate.HasValue && 
                        c.ExpirationDate <= expirationDate && 
                        c.Status == ContractStatus.FullySigned)
            .OrderBy(c => c.ExpirationDate)
            .ToListAsync(ct);
    }

    public async Task<string> GenerateContractNumberAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"OKLA-{year}-";
        
        var lastContract = await _context.Contracts
            .Where(c => c.ContractNumber.StartsWith(prefix))
            .OrderByDescending(c => c.ContractNumber)
            .FirstOrDefaultAsync(ct);

        int sequence = 1;
        if (lastContract != null)
        {
            var lastNumber = lastContract.ContractNumber.Replace(prefix, "");
            if (int.TryParse(lastNumber, out int parsed))
            {
                sequence = parsed + 1;
            }
        }

        return $"{prefix}{sequence:D6}";
    }

    public async Task AddAsync(Contract contract, CancellationToken ct = default)
    {
        await _context.Contracts.AddAsync(contract, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Contract contract, CancellationToken ct = default)
    {
        _context.Contracts.Update(contract);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Contract Party Repository

public class ContractPartyRepository : IContractPartyRepository
{
    private readonly ContractDbContext _context;

    public ContractPartyRepository(ContractDbContext context)
    {
        _context = context;
    }

    public async Task<ContractParty?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ContractParties.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<List<ContractParty>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default)
    {
        return await _context.ContractParties
            .Where(p => p.ContractId == contractId)
            .ToListAsync(ct);
    }

    public async Task<ContractParty?> GetByUserAndContractAsync(Guid userId, Guid contractId, CancellationToken ct = default)
    {
        return await _context.ContractParties
            .FirstOrDefaultAsync(p => p.UserId == userId && p.ContractId == contractId, ct);
    }

    public async Task AddAsync(ContractParty party, CancellationToken ct = default)
    {
        await _context.ContractParties.AddAsync(party, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ContractParty party, CancellationToken ct = default)
    {
        _context.ContractParties.Update(party);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Contract Signature Repository

public class ContractSignatureRepository : IContractSignatureRepository
{
    private readonly ContractDbContext _context;

    public ContractSignatureRepository(ContractDbContext context)
    {
        _context = context;
    }

    public async Task<ContractSignature?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ContractSignatures
            .Include(s => s.Party)
            .Include(s => s.CertificationAuthority)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<List<ContractSignature>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default)
    {
        return await _context.ContractSignatures
            .Include(s => s.Party)
            .Where(s => s.ContractId == contractId)
            .ToListAsync(ct);
    }

    public async Task<List<ContractSignature>> GetPendingSignaturesAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.ContractSignatures
            .Include(s => s.Contract)
            .Include(s => s.Party)
            .Where(s => s.Party.UserId == userId && 
                        (s.Status == SignatureStatus.Pending || s.Status == SignatureStatus.Requested))
            .ToListAsync(ct);
    }

    public async Task<bool> AllPartiesSignedAsync(Guid contractId, CancellationToken ct = default)
    {
        var allSignatures = await _context.ContractSignatures
            .Where(s => s.ContractId == contractId)
            .ToListAsync(ct);

        return allSignatures.Any() && allSignatures.All(s => s.Status == SignatureStatus.Signed);
    }

    public async Task AddAsync(ContractSignature signature, CancellationToken ct = default)
    {
        await _context.ContractSignatures.AddAsync(signature, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ContractSignature signature, CancellationToken ct = default)
    {
        _context.ContractSignatures.Update(signature);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Contract Clause Repository

public class ContractClauseRepository : IContractClauseRepository
{
    private readonly ContractDbContext _context;

    public ContractClauseRepository(ContractDbContext context)
    {
        _context = context;
    }

    public async Task<ContractClause?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ContractClauses.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<List<ContractClause>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default)
    {
        return await _context.ContractClauses
            .Where(c => c.ContractId == contractId)
            .OrderBy(c => c.Order)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ContractClause clause, CancellationToken ct = default)
    {
        await _context.ContractClauses.AddAsync(clause, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(List<ContractClause> clauses, CancellationToken ct = default)
    {
        await _context.ContractClauses.AddRangeAsync(clauses, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ContractClause clause, CancellationToken ct = default)
    {
        _context.ContractClauses.Update(clause);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Contract Version Repository

public class ContractVersionRepository : IContractVersionRepository
{
    private readonly ContractDbContext _context;

    public ContractVersionRepository(ContractDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContractVersion>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default)
    {
        return await _context.ContractVersions
            .Where(v => v.ContractId == contractId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(ct);
    }

    public async Task<ContractVersion?> GetLatestVersionAsync(Guid contractId, CancellationToken ct = default)
    {
        return await _context.ContractVersions
            .Where(v => v.ContractId == contractId)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(ContractVersion version, CancellationToken ct = default)
    {
        // Get next version number
        var latest = await GetLatestVersionAsync(version.ContractId, ct);
        version.VersionNumber = (latest?.VersionNumber ?? 0) + 1;

        await _context.ContractVersions.AddAsync(version, ct);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Contract Document Repository

public class ContractDocumentRepository : IContractDocumentRepository
{
    private readonly ContractDbContext _context;

    public ContractDocumentRepository(ContractDbContext context)
    {
        _context = context;
    }

    public async Task<ContractDocument?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ContractDocuments.FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<List<ContractDocument>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default)
    {
        return await _context.ContractDocuments
            .Where(d => d.ContractId == contractId)
            .OrderBy(d => d.UploadedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ContractDocument document, CancellationToken ct = default)
    {
        await _context.ContractDocuments.AddAsync(document, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var document = await GetByIdAsync(id, ct);
        if (document != null)
        {
            _context.ContractDocuments.Remove(document);
            await _context.SaveChangesAsync(ct);
        }
    }
}

#endregion

#region Contract Audit Log Repository

public class ContractAuditLogRepository : IContractAuditLogRepository
{
    private readonly ContractDbContext _context;

    public ContractAuditLogRepository(ContractDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContractAuditLog>> GetByContractIdAsync(Guid contractId, CancellationToken ct = default)
    {
        return await _context.ContractAuditLogs
            .Where(a => a.ContractId == contractId)
            .OrderByDescending(a => a.PerformedAt)
            .ToListAsync(ct);
    }

    public async Task<List<ContractAuditLog>> GetByUserAsync(string userId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _context.ContractAuditLogs
            .Where(a => a.PerformedBy == userId && a.PerformedAt >= from && a.PerformedAt <= to)
            .OrderByDescending(a => a.PerformedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ContractAuditLog auditLog, CancellationToken ct = default)
    {
        await _context.ContractAuditLogs.AddAsync(auditLog, ct);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Certification Authority Repository

public class CertificationAuthorityRepository : ICertificationAuthorityRepository
{
    private readonly ContractDbContext _context;

    public CertificationAuthorityRepository(ContractDbContext context)
    {
        _context = context;
    }

    public async Task<CertificationAuthority?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.CertificationAuthorities.FirstOrDefaultAsync(ca => ca.Id == id, ct);
    }

    public async Task<CertificationAuthority?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.CertificationAuthorities.FirstOrDefaultAsync(ca => ca.Code == code, ct);
    }

    public async Task<List<CertificationAuthority>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.CertificationAuthorities.OrderBy(ca => ca.Name).ToListAsync(ct);
    }

    public async Task<List<CertificationAuthority>> GetActiveAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _context.CertificationAuthorities
            .Where(ca => ca.IsActive && 
                        (!ca.ValidUntil.HasValue || ca.ValidUntil > now))
            .OrderBy(ca => ca.Name)
            .ToListAsync(ct);
    }
}

#endregion
