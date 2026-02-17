using LegalDocumentService.Domain.Entities;
using LegalDocumentService.Domain.Enums;

namespace LegalDocumentService.Domain.Interfaces;

/// <summary>
/// Repository for legal documents
/// </summary>
public interface ILegalDocumentRepository
{
    Task<LegalDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<LegalDocument?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<List<LegalDocument>> GetAllAsync(CancellationToken ct = default);
    Task<List<LegalDocument>> GetActiveAsync(CancellationToken ct = default);
    Task<List<LegalDocument>> GetByTypeAsync(LegalDocumentType type, CancellationToken ct = default);
    Task<List<LegalDocument>> GetByStatusAsync(LegalDocumentStatus status, CancellationToken ct = default);
    Task<List<LegalDocument>> GetByJurisdictionAsync(Jurisdiction jurisdiction, CancellationToken ct = default);
    Task<List<LegalDocument>> GetRequiringAcceptanceAsync(CancellationToken ct = default);
    Task<LegalDocument?> GetLatestVersionAsync(LegalDocumentType type, DocumentLanguage language, CancellationToken ct = default);
    Task<LegalDocument> AddAsync(LegalDocument document, CancellationToken ct = default);
    Task<LegalDocument> UpdateAsync(LegalDocument document, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
    Task<int> GetCountByTypeAsync(LegalDocumentType type, CancellationToken ct = default);
}

/// <summary>
/// Repository for document versions
/// </summary>
public interface ILegalDocumentVersionRepository
{
    Task<LegalDocumentVersion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<LegalDocumentVersion>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct = default);
    Task<LegalDocumentVersion?> GetLatestAsync(Guid documentId, CancellationToken ct = default);
    Task<LegalDocumentVersion> AddAsync(LegalDocumentVersion version, CancellationToken ct = default);
    Task<int> GetVersionCountAsync(Guid documentId, CancellationToken ct = default);
}

/// <summary>
/// Repository for user acceptances
/// </summary>
public interface IUserAcceptanceRepository
{
    Task<UserAcceptance?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<UserAcceptance>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<List<UserAcceptance>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct = default);
    Task<UserAcceptance?> GetUserDocumentAcceptanceAsync(string userId, Guid documentId, CancellationToken ct = default);
    Task<List<UserAcceptance>> GetPendingAcceptancesAsync(string userId, CancellationToken ct = default);
    Task<List<UserAcceptance>> GetAcceptedByUserAsync(string userId, CancellationToken ct = default);
    Task<bool> HasUserAcceptedAsync(string userId, Guid documentId, CancellationToken ct = default);
    Task<bool> HasUserAcceptedVersionAsync(string userId, Guid documentId, string version, CancellationToken ct = default);
    Task<UserAcceptance> AddAsync(UserAcceptance acceptance, CancellationToken ct = default);
    Task<UserAcceptance> UpdateAsync(UserAcceptance acceptance, CancellationToken ct = default);
    Task<int> GetAcceptanceCountAsync(Guid documentId, CancellationToken ct = default);
    Task<List<UserAcceptance>> GetByTransactionIdAsync(string transactionId, CancellationToken ct = default);
    Task<List<UserAcceptance>> GetRecentAcceptancesAsync(int days, CancellationToken ct = default);
}

/// <summary>
/// Repository for document templates
/// </summary>
public interface IDocumentTemplateRepository
{
    Task<DocumentTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DocumentTemplate?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<List<DocumentTemplate>> GetAllAsync(CancellationToken ct = default);
    Task<List<DocumentTemplate>> GetActiveAsync(CancellationToken ct = default);
    Task<List<DocumentTemplate>> GetByTypeAsync(LegalDocumentType type, CancellationToken ct = default);
    Task<DocumentTemplate> AddAsync(DocumentTemplate template, CancellationToken ct = default);
    Task<DocumentTemplate> UpdateAsync(DocumentTemplate template, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Repository for template variables
/// </summary>
public interface ITemplateVariableRepository
{
    Task<TemplateVariable?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<TemplateVariable>> GetByTemplateIdAsync(Guid templateId, CancellationToken ct = default);
    Task<List<TemplateVariable>> GetRequiredByTemplateIdAsync(Guid templateId, CancellationToken ct = default);
    Task<TemplateVariable> AddAsync(TemplateVariable variable, CancellationToken ct = default);
    Task<TemplateVariable> UpdateAsync(TemplateVariable variable, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Repository for compliance requirements
/// </summary>
public interface IComplianceRequirementRepository
{
    Task<ComplianceRequirement?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ComplianceRequirement?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<List<ComplianceRequirement>> GetAllAsync(CancellationToken ct = default);
    Task<List<ComplianceRequirement>> GetActiveAsync(CancellationToken ct = default);
    Task<List<ComplianceRequirement>> GetMandatoryAsync(CancellationToken ct = default);
    Task<List<ComplianceRequirement>> GetByJurisdictionAsync(Jurisdiction jurisdiction, CancellationToken ct = default);
    Task<ComplianceRequirement> AddAsync(ComplianceRequirement requirement, CancellationToken ct = default);
    Task<ComplianceRequirement> UpdateAsync(ComplianceRequirement requirement, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Repository for required documents
/// </summary>
public interface IRequiredDocumentRepository
{
    Task<RequiredDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<RequiredDocument>> GetByRequirementIdAsync(Guid requirementId, CancellationToken ct = default);
    Task<List<RequiredDocument>> GetMandatoryByRequirementIdAsync(Guid requirementId, CancellationToken ct = default);
    Task<RequiredDocument> AddAsync(RequiredDocument document, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Statistics for legal documents
/// </summary>
public class LegalDocumentStatistics
{
    public int TotalDocuments { get; set; }
    public int ActiveDocuments { get; set; }
    public int DraftDocuments { get; set; }
    public int PublishedDocuments { get; set; }
    public int DocumentsRequiringAcceptance { get; set; }
    public int TotalAcceptances { get; set; }
    public int PendingAcceptances { get; set; }
    public int TotalTemplates { get; set; }
    public int TotalComplianceRequirements { get; set; }
    public Dictionary<LegalDocumentType, int> DocumentsByType { get; set; } = new();
    public Dictionary<AcceptanceStatus, int> AcceptancesByStatus { get; set; } = new();
}
