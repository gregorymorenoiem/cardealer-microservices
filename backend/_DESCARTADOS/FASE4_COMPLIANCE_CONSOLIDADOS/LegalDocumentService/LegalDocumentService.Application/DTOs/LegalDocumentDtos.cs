using LegalDocumentService.Domain.Enums;

namespace LegalDocumentService.Application.DTOs;

// ===== LEGAL DOCUMENT DTOs =====

public record LegalDocumentDto(
    Guid Id,
    string Title,
    string Slug,
    LegalDocumentType DocumentType,
    LegalDocumentStatus Status,
    string Content,
    string? ContentHtml,
    string? Summary,
    int VersionMajor,
    int VersionMinor,
    string VersionLabel,
    Jurisdiction Jurisdiction,
    DocumentLanguage Language,
    DateTime EffectiveDate,
    DateTime? ExpirationDate,
    DateTime? PublishedAt,
    bool RequiresAcceptance,
    bool IsActive,
    bool IsMandatory,
    string? CreatedBy,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    string? LegalReferences,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record LegalDocumentSummaryDto(
    Guid Id,
    string Title,
    string Slug,
    LegalDocumentType DocumentType,
    LegalDocumentStatus Status,
    string VersionLabel,
    bool IsActive,
    bool RequiresAcceptance,
    DateTime EffectiveDate);

public record CreateLegalDocumentDto(
    string Title,
    LegalDocumentType DocumentType,
    string Content,
    string? ContentHtml,
    string? Summary,
    Jurisdiction Jurisdiction,
    DocumentLanguage Language,
    bool RequiresAcceptance,
    bool IsMandatory,
    DateTime? EffectiveDate,
    DateTime? ExpirationDate,
    string? LegalReferences,
    string? CreatedBy);

public record UpdateLegalDocumentDto(
    Guid Id,
    string? Title,
    string? Content,
    string? ContentHtml,
    string? Summary,
    DateTime? EffectiveDate,
    DateTime? ExpirationDate,
    string? LegalReferences,
    bool? RequiresAcceptance,
    bool? IsMandatory);

public record PublishDocumentDto(
    Guid DocumentId,
    string ApprovedBy,
    bool IncrementMajorVersion);

// ===== VERSION DTOs =====

public record LegalDocumentVersionDto(
    Guid Id,
    Guid LegalDocumentId,
    int VersionMajor,
    int VersionMinor,
    string VersionLabel,
    string Content,
    string? ContentHtml,
    string ChangeNotes,
    DateTime CreatedAt);

// ===== USER ACCEPTANCE DTOs =====

public record UserAcceptanceDto(
    Guid Id,
    Guid LegalDocumentId,
    string UserId,
    string? TransactionId,
    AcceptanceStatus Status,
    AcceptanceMethod Method,
    DateTime? AcceptedAt,
    DateTime? DeclinedAt,
    DateTime? RevokedAt,
    string IpAddress,
    string UserAgent,
    string? GeoLocation,
    string DocumentVersionAccepted,
    DateTime CreatedAt);

public record CreateAcceptanceDto(
    Guid LegalDocumentId,
    string UserId,
    AcceptanceMethod Method,
    string IpAddress,
    string UserAgent,
    string? TransactionId,
    string? GeoLocation);

public record RecordAcceptanceDto(
    Guid AcceptanceId,
    string? SignatureData);

public record RevokeAcceptanceDto(
    Guid AcceptanceId,
    string Reason);

public record UserAcceptanceStatusDto(
    Guid LegalDocumentId,
    string DocumentTitle,
    string VersionLabel,
    bool HasAccepted,
    DateTime? AcceptedAt,
    bool NeedsReAcceptance);

// ===== TEMPLATE DTOs =====

public record DocumentTemplateDto(
    Guid Id,
    string Name,
    string Code,
    LegalDocumentType DocumentType,
    string TemplateContent,
    string? Description,
    DocumentLanguage Language,
    Jurisdiction Jurisdiction,
    bool IsActive,
    string? CreatedBy,
    List<TemplateVariableDto> Variables,
    DateTime CreatedAt);

public record CreateTemplateDto(
    string Name,
    string Code,
    LegalDocumentType DocumentType,
    string TemplateContent,
    string? Description,
    DocumentLanguage Language,
    Jurisdiction Jurisdiction,
    string? CreatedBy,
    List<CreateVariableDto>? Variables);

public record UpdateTemplateDto(
    Guid Id,
    string? Name,
    string? TemplateContent,
    string? Description,
    bool? IsActive);

public record TemplateVariableDto(
    Guid Id,
    string VariableName,
    string Placeholder,
    TemplateVariableType VariableType,
    bool IsRequired,
    string? DefaultValue,
    string? ValidationRegex,
    string? Description);

public record CreateVariableDto(
    string VariableName,
    string Placeholder,
    TemplateVariableType VariableType,
    bool IsRequired,
    string? DefaultValue,
    string? ValidationRegex,
    string? Description);

public record GenerateDocumentFromTemplateDto(
    Guid TemplateId,
    Dictionary<string, string> VariableValues,
    string? CreatedBy);

// ===== COMPLIANCE REQUIREMENT DTOs =====

public record ComplianceRequirementDto(
    Guid Id,
    string Name,
    string Code,
    string Description,
    string LegalBasis,
    string? ArticleReference,
    Jurisdiction Jurisdiction,
    bool IsMandatory,
    bool IsActive,
    DateTime EffectiveDate,
    DateTime? SunsetDate,
    List<RequiredDocumentDto> RequiredDocuments,
    DateTime CreatedAt);

public record CreateComplianceRequirementDto(
    string Name,
    string Code,
    string Description,
    string LegalBasis,
    string? ArticleReference,
    Jurisdiction Jurisdiction,
    bool IsMandatory,
    DateTime? EffectiveDate,
    DateTime? SunsetDate);

public record RequiredDocumentDto(
    Guid Id,
    LegalDocumentType DocumentType,
    string Description,
    bool IsMandatory,
    int DisplayOrder);

public record AddRequiredDocumentDto(
    Guid ComplianceRequirementId,
    LegalDocumentType DocumentType,
    string Description,
    bool IsMandatory);

// ===== STATISTICS DTOs =====

public record LegalStatisticsDto(
    int TotalDocuments,
    int ActiveDocuments,
    int DraftDocuments,
    int PublishedDocuments,
    int DocumentsRequiringAcceptance,
    int TotalAcceptances,
    int PendingAcceptances,
    int TotalTemplates,
    int TotalComplianceRequirements,
    Dictionary<string, int> DocumentsByType,
    Dictionary<string, int> AcceptancesByStatus);

// ===== QUERY/FILTER DTOs =====

public record DocumentFilterDto(
    LegalDocumentType? DocumentType,
    LegalDocumentStatus? Status,
    Jurisdiction? Jurisdiction,
    DocumentLanguage? Language,
    bool? IsActive,
    bool? RequiresAcceptance,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20);

public record AcceptanceFilterDto(
    string? UserId,
    Guid? DocumentId,
    AcceptanceStatus? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 20);
