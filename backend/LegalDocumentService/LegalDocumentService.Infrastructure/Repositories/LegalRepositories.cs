using Microsoft.EntityFrameworkCore;
using LegalDocumentService.Domain.Entities;
using LegalDocumentService.Domain.Interfaces;
using LegalDocumentService.Domain.Enums;
using LegalDocumentService.Infrastructure.Persistence;

namespace LegalDocumentService.Infrastructure.Repositories;

public class LegalDocumentRepository : ILegalDocumentRepository
{
    private readonly LegalDocumentDbContext _context;

    public LegalDocumentRepository(LegalDocumentDbContext context)
    {
        _context = context;
    }

    public async Task<LegalDocument?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.LegalDocuments
            .Include(d => d.Versions)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct);

    public async Task<LegalDocument?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => await _context.LegalDocuments
            .FirstOrDefaultAsync(d => d.Slug == slug && !d.IsDeleted, ct);

    public async Task<List<LegalDocument>> GetAllAsync(CancellationToken ct = default)
        => await _context.LegalDocuments
            .Where(d => !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<LegalDocument>> GetActiveAsync(CancellationToken ct = default)
        => await _context.LegalDocuments
            .Where(d => d.IsActive && !d.IsDeleted)
            .OrderBy(d => d.Title)
            .ToListAsync(ct);

    public async Task<List<LegalDocument>> GetByTypeAsync(LegalDocumentType type, CancellationToken ct = default)
        => await _context.LegalDocuments
            .Where(d => d.DocumentType == type && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<LegalDocument>> GetByStatusAsync(LegalDocumentStatus status, CancellationToken ct = default)
        => await _context.LegalDocuments
            .Where(d => d.Status == status && !d.IsDeleted)
            .ToListAsync(ct);

    public async Task<List<LegalDocument>> GetByJurisdictionAsync(Jurisdiction jurisdiction, CancellationToken ct = default)
        => await _context.LegalDocuments
            .Where(d => d.Jurisdiction == jurisdiction && !d.IsDeleted)
            .ToListAsync(ct);

    public async Task<List<LegalDocument>> GetRequiringAcceptanceAsync(CancellationToken ct = default)
        => await _context.LegalDocuments
            .Where(d => d.RequiresAcceptance && d.IsActive && !d.IsDeleted)
            .OrderBy(d => d.Title)
            .ToListAsync(ct);

    public async Task<LegalDocument?> GetLatestVersionAsync(LegalDocumentType type, DocumentLanguage language, CancellationToken ct = default)
        => await _context.LegalDocuments
            .Where(d => d.DocumentType == type && d.Language == language && d.IsActive && !d.IsDeleted)
            .OrderByDescending(d => d.VersionMajor)
            .ThenByDescending(d => d.VersionMinor)
            .FirstOrDefaultAsync(ct);

    public async Task<LegalDocument> AddAsync(LegalDocument document, CancellationToken ct = default)
    {
        await _context.LegalDocuments.AddAsync(document, ct);
        await _context.SaveChangesAsync(ct);
        return document;
    }

    public async Task<LegalDocument> UpdateAsync(LegalDocument document, CancellationToken ct = default)
    {
        _context.LegalDocuments.Update(document);
        await _context.SaveChangesAsync(ct);
        return document;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var document = await GetByIdAsync(id, ct);
        if (document != null)
        {
            document.MarkAsDeleted();
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await _context.LegalDocuments.AnyAsync(d => d.Id == id && !d.IsDeleted, ct);

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default)
        => await _context.LegalDocuments.AnyAsync(d => d.Slug == slug && !d.IsDeleted, ct);

    public async Task<int> GetCountByTypeAsync(LegalDocumentType type, CancellationToken ct = default)
        => await _context.LegalDocuments.CountAsync(d => d.DocumentType == type && !d.IsDeleted, ct);
}

public class LegalDocumentVersionRepository : ILegalDocumentVersionRepository
{
    private readonly LegalDocumentDbContext _context;

    public LegalDocumentVersionRepository(LegalDocumentDbContext context)
    {
        _context = context;
    }

    public async Task<LegalDocumentVersion?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.LegalDocumentVersions.FindAsync(new object[] { id }, ct);

    public async Task<List<LegalDocumentVersion>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct = default)
        => await _context.LegalDocumentVersions
            .Where(v => v.LegalDocumentId == documentId)
            .OrderByDescending(v => v.VersionMajor)
            .ThenByDescending(v => v.VersionMinor)
            .ToListAsync(ct);

    public async Task<LegalDocumentVersion?> GetLatestAsync(Guid documentId, CancellationToken ct = default)
        => await _context.LegalDocumentVersions
            .Where(v => v.LegalDocumentId == documentId)
            .OrderByDescending(v => v.VersionMajor)
            .ThenByDescending(v => v.VersionMinor)
            .FirstOrDefaultAsync(ct);

    public async Task<LegalDocumentVersion> AddAsync(LegalDocumentVersion version, CancellationToken ct = default)
    {
        await _context.LegalDocumentVersions.AddAsync(version, ct);
        await _context.SaveChangesAsync(ct);
        return version;
    }

    public async Task<int> GetVersionCountAsync(Guid documentId, CancellationToken ct = default)
        => await _context.LegalDocumentVersions.CountAsync(v => v.LegalDocumentId == documentId, ct);
}

public class UserAcceptanceRepository : IUserAcceptanceRepository
{
    private readonly LegalDocumentDbContext _context;

    public UserAcceptanceRepository(LegalDocumentDbContext context)
    {
        _context = context;
    }

    public async Task<UserAcceptance?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.UserAcceptances
            .Include(a => a.LegalDocument)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);

    public async Task<List<UserAcceptance>> GetByUserIdAsync(string userId, CancellationToken ct = default)
        => await _context.UserAcceptances
            .Include(a => a.LegalDocument)
            .Where(a => a.UserId == userId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

    public async Task<List<UserAcceptance>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct = default)
        => await _context.UserAcceptances
            .Where(a => a.LegalDocumentId == documentId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

    public async Task<UserAcceptance?> GetUserDocumentAcceptanceAsync(string userId, Guid documentId, CancellationToken ct = default)
        => await _context.UserAcceptances
            .Where(a => a.UserId == userId && a.LegalDocumentId == documentId && !a.IsDeleted)
            .OrderByDescending(a => a.AcceptedAt)
            .FirstOrDefaultAsync(ct);

    public async Task<List<UserAcceptance>> GetPendingAcceptancesAsync(string userId, CancellationToken ct = default)
        => await _context.UserAcceptances
            .Include(a => a.LegalDocument)
            .Where(a => a.UserId == userId && a.Status == AcceptanceStatus.Pending && !a.IsDeleted)
            .ToListAsync(ct);

    public async Task<List<UserAcceptance>> GetAcceptedByUserAsync(string userId, CancellationToken ct = default)
        => await _context.UserAcceptances
            .Where(a => a.UserId == userId && a.Status == AcceptanceStatus.Accepted && !a.IsDeleted)
            .ToListAsync(ct);

    public async Task<bool> HasUserAcceptedAsync(string userId, Guid documentId, CancellationToken ct = default)
        => await _context.UserAcceptances
            .AnyAsync(a => a.UserId == userId && a.LegalDocumentId == documentId &&
                a.Status == AcceptanceStatus.Accepted && !a.IsDeleted, ct);

    public async Task<bool> HasUserAcceptedVersionAsync(string userId, Guid documentId, string version, CancellationToken ct = default)
        => await _context.UserAcceptances
            .AnyAsync(a => a.UserId == userId && a.LegalDocumentId == documentId &&
                a.DocumentVersionAccepted == version && a.Status == AcceptanceStatus.Accepted && !a.IsDeleted, ct);

    public async Task<UserAcceptance> AddAsync(UserAcceptance acceptance, CancellationToken ct = default)
    {
        await _context.UserAcceptances.AddAsync(acceptance, ct);
        await _context.SaveChangesAsync(ct);
        return acceptance;
    }

    public async Task<UserAcceptance> UpdateAsync(UserAcceptance acceptance, CancellationToken ct = default)
    {
        _context.UserAcceptances.Update(acceptance);
        await _context.SaveChangesAsync(ct);
        return acceptance;
    }

    public async Task<int> GetAcceptanceCountAsync(Guid documentId, CancellationToken ct = default)
        => await _context.UserAcceptances
            .CountAsync(a => a.LegalDocumentId == documentId && a.Status == AcceptanceStatus.Accepted && !a.IsDeleted, ct);

    public async Task<List<UserAcceptance>> GetByTransactionIdAsync(string transactionId, CancellationToken ct = default)
        => await _context.UserAcceptances
            .Where(a => a.TransactionId == transactionId && !a.IsDeleted)
            .ToListAsync(ct);

    public async Task<List<UserAcceptance>> GetRecentAcceptancesAsync(int days, CancellationToken ct = default)
    {
        var fromDate = DateTime.UtcNow.AddDays(-days);
        return await _context.UserAcceptances
            .Where(a => a.AcceptedAt >= fromDate && !a.IsDeleted)
            .OrderByDescending(a => a.AcceptedAt)
            .ToListAsync(ct);
    }
}

public class DocumentTemplateRepository : IDocumentTemplateRepository
{
    private readonly LegalDocumentDbContext _context;

    public DocumentTemplateRepository(LegalDocumentDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.DocumentTemplates
            .Include(t => t.Variables)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);

    public async Task<DocumentTemplate?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await _context.DocumentTemplates
            .Include(t => t.Variables)
            .FirstOrDefaultAsync(t => t.Code == code && !t.IsDeleted, ct);

    public async Task<List<DocumentTemplate>> GetAllAsync(CancellationToken ct = default)
        => await _context.DocumentTemplates
            .Include(t => t.Variables)
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

    public async Task<List<DocumentTemplate>> GetActiveAsync(CancellationToken ct = default)
        => await _context.DocumentTemplates
            .Include(t => t.Variables)
            .Where(t => t.IsActive && !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

    public async Task<List<DocumentTemplate>> GetByTypeAsync(LegalDocumentType type, CancellationToken ct = default)
        => await _context.DocumentTemplates
            .Include(t => t.Variables)
            .Where(t => t.DocumentType == type && !t.IsDeleted)
            .ToListAsync(ct);

    public async Task<DocumentTemplate> AddAsync(DocumentTemplate template, CancellationToken ct = default)
    {
        await _context.DocumentTemplates.AddAsync(template, ct);
        await _context.SaveChangesAsync(ct);
        return template;
    }

    public async Task<DocumentTemplate> UpdateAsync(DocumentTemplate template, CancellationToken ct = default)
    {
        _context.DocumentTemplates.Update(template);
        await _context.SaveChangesAsync(ct);
        return template;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var template = await GetByIdAsync(id, ct);
        if (template != null)
        {
            template.Deactivate();
            await _context.SaveChangesAsync(ct);
        }
    }
}

public class TemplateVariableRepository : ITemplateVariableRepository
{
    private readonly LegalDocumentDbContext _context;

    public TemplateVariableRepository(LegalDocumentDbContext context)
    {
        _context = context;
    }

    public async Task<TemplateVariable?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.TemplateVariables.FindAsync(new object[] { id }, ct);

    public async Task<List<TemplateVariable>> GetByTemplateIdAsync(Guid templateId, CancellationToken ct = default)
        => await _context.TemplateVariables
            .Where(v => v.DocumentTemplateId == templateId && !v.IsDeleted)
            .ToListAsync(ct);

    public async Task<List<TemplateVariable>> GetRequiredByTemplateIdAsync(Guid templateId, CancellationToken ct = default)
        => await _context.TemplateVariables
            .Where(v => v.DocumentTemplateId == templateId && v.IsRequired && !v.IsDeleted)
            .ToListAsync(ct);

    public async Task<TemplateVariable> AddAsync(TemplateVariable variable, CancellationToken ct = default)
    {
        await _context.TemplateVariables.AddAsync(variable, ct);
        await _context.SaveChangesAsync(ct);
        return variable;
    }

    public async Task<TemplateVariable> UpdateAsync(TemplateVariable variable, CancellationToken ct = default)
    {
        _context.TemplateVariables.Update(variable);
        await _context.SaveChangesAsync(ct);
        return variable;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var variable = await GetByIdAsync(id, ct);
        if (variable != null)
        {
            variable.MarkAsDeleted();
            await _context.SaveChangesAsync(ct);
        }
    }
}

public class ComplianceRequirementRepository : IComplianceRequirementRepository
{
    private readonly LegalDocumentDbContext _context;

    public ComplianceRequirementRepository(LegalDocumentDbContext context)
    {
        _context = context;
    }

    public async Task<ComplianceRequirement?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.ComplianceRequirements
            .Include(r => r.RequiredDocuments)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);

    public async Task<ComplianceRequirement?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await _context.ComplianceRequirements
            .Include(r => r.RequiredDocuments)
            .FirstOrDefaultAsync(r => r.Code == code && !r.IsDeleted, ct);

    public async Task<List<ComplianceRequirement>> GetAllAsync(CancellationToken ct = default)
        => await _context.ComplianceRequirements
            .Include(r => r.RequiredDocuments)
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

    public async Task<List<ComplianceRequirement>> GetActiveAsync(CancellationToken ct = default)
        => await _context.ComplianceRequirements
            .Include(r => r.RequiredDocuments)
            .Where(r => r.IsActive && !r.IsDeleted)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

    public async Task<List<ComplianceRequirement>> GetMandatoryAsync(CancellationToken ct = default)
        => await _context.ComplianceRequirements
            .Include(r => r.RequiredDocuments)
            .Where(r => r.IsMandatory && r.IsActive && !r.IsDeleted)
            .ToListAsync(ct);

    public async Task<List<ComplianceRequirement>> GetByJurisdictionAsync(Jurisdiction jurisdiction, CancellationToken ct = default)
        => await _context.ComplianceRequirements
            .Include(r => r.RequiredDocuments)
            .Where(r => r.Jurisdiction == jurisdiction && !r.IsDeleted)
            .ToListAsync(ct);

    public async Task<ComplianceRequirement> AddAsync(ComplianceRequirement requirement, CancellationToken ct = default)
    {
        await _context.ComplianceRequirements.AddAsync(requirement, ct);
        await _context.SaveChangesAsync(ct);
        return requirement;
    }

    public async Task<ComplianceRequirement> UpdateAsync(ComplianceRequirement requirement, CancellationToken ct = default)
    {
        _context.ComplianceRequirements.Update(requirement);
        await _context.SaveChangesAsync(ct);
        return requirement;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var requirement = await GetByIdAsync(id, ct);
        if (requirement != null)
        {
            requirement.MarkAsDeleted();
            await _context.SaveChangesAsync(ct);
        }
    }
}

public class RequiredDocumentRepository : IRequiredDocumentRepository
{
    private readonly LegalDocumentDbContext _context;

    public RequiredDocumentRepository(LegalDocumentDbContext context)
    {
        _context = context;
    }

    public async Task<RequiredDocument?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.RequiredDocuments.FindAsync(new object[] { id }, ct);

    public async Task<List<RequiredDocument>> GetByRequirementIdAsync(Guid requirementId, CancellationToken ct = default)
        => await _context.RequiredDocuments
            .Where(r => r.ComplianceRequirementId == requirementId && !r.IsDeleted)
            .OrderBy(r => r.DisplayOrder)
            .ToListAsync(ct);

    public async Task<List<RequiredDocument>> GetMandatoryByRequirementIdAsync(Guid requirementId, CancellationToken ct = default)
        => await _context.RequiredDocuments
            .Where(r => r.ComplianceRequirementId == requirementId && r.IsMandatory && !r.IsDeleted)
            .OrderBy(r => r.DisplayOrder)
            .ToListAsync(ct);

    public async Task<RequiredDocument> AddAsync(RequiredDocument document, CancellationToken ct = default)
    {
        await _context.RequiredDocuments.AddAsync(document, ct);
        await _context.SaveChangesAsync(ct);
        return document;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var document = await GetByIdAsync(id, ct);
        if (document != null)
        {
            document.MarkAsDeleted();
            await _context.SaveChangesAsync(ct);
        }
    }
}
