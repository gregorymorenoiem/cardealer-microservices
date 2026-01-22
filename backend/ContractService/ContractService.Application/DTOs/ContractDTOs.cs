// ContractService - DTOs (Consistent with Handlers)

namespace ContractService.Application.DTOs;

using ContractService.Domain.Entities;

#region Template DTOs

public record ContractTemplateDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    ContractType Type,
    string ContentHtml,
    string? ContentJson,
    List<string> RequiredVariables,
    List<string> OptionalVariables,
    string Language,
    string? LegalBasis,
    bool RequiresWitness,
    int MinimumSignatures,
    bool RequiresNotarization,
    int? ValidityDays,
    bool IsActive,
    int Version,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt
);

#endregion

#region Contract DTOs

public record ContractDto(
    Guid Id,
    string ContractNumber,
    Guid? TemplateId,
    ContractType Type,
    string Title,
    string? Description,
    string ContentHtml,
    string? ContentHash,
    DateTime EffectiveDate,
    DateTime? ExpirationDate,
    ContractStatus Status,
    string? SubjectType,
    Guid? SubjectId,
    string? SubjectDescription,
    decimal? ContractValue,
    string Currency,
    string? LegalJurisdiction,
    DateTime? SignedAt,
    DateTime? TerminatedAt,
    string? TerminationReason,
    bool AcceptedTerms,
    bool AcceptedPrivacyPolicy,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt
);

public record ContractSummaryDto(
    Guid Id,
    string ContractNumber,
    ContractType Type,
    string Title,
    ContractStatus Status,
    DateTime EffectiveDate,
    DateTime? ExpirationDate,
    decimal? ContractValue,
    string Currency,
    DateTime CreatedAt
);

#endregion

#region Party DTOs

public record ContractPartyDto(
    Guid Id,
    Guid ContractId,
    PartyType Type,
    PartyRole Role,
    Guid? UserId,
    string FullName,
    string DocumentType,
    string DocumentNumber,
    string Email,
    string? Phone,
    string? Address,
    string? CompanyName,
    string? RNC,
    string? LegalRepresentative,
    string? PowerOfAttorneyNumber,
    bool HasSigned,
    DateTime? SignedAt
);

public record CreatePartyDto(
    PartyType Type,
    PartyRole Role,
    Guid? UserId,
    string FullName,
    string DocumentType,
    string DocumentNumber,
    string Email,
    string? Phone,
    string? CompanyName,
    string? RNC
);

#endregion

#region Signature DTOs

public record ContractSignatureDto(
    Guid Id,
    Guid ContractId,
    Guid PartyId,
    SignatureType Type,
    SignatureStatus Status,
    string? SignatureData,
    string? SignatureImage,
    string? CertificateData,
    Guid? CertificationAuthorityId,
    string? DocumentHash,
    DateTime? TimestampDate,
    DateTime? SignedAt,
    string? IPAddress,
    string? GeoLocation,
    bool? BiometricVerified,
    string? BiometricType,
    SignatureVerificationStatus VerificationStatus,
    DateTime? ExpiresAt,
    string? DeclineReason,
    DateTime? DeclinedAt
);

#endregion

#region Clause DTOs

public record ContractClauseDto(
    Guid Id,
    Guid ContractId,
    Guid? TemplateClauseId,
    string Code,
    string Title,
    string Content,
    string? OriginalContent,
    ClauseType Type,
    int Order,
    bool IsModified,
    string? ModifiedBy,
    DateTime? ModifiedAt,
    string? ModificationReason
);

#endregion

#region Version DTOs

public record ContractVersionDto(
    Guid Id,
    Guid ContractId,
    int VersionNumber,
    string ContentHtml,
    string? ContentHash,
    string? ChangeDescription,
    DateTime CreatedAt,
    string CreatedBy
);

#endregion

#region Document DTOs

public record ContractDocumentDto(
    Guid Id,
    Guid ContractId,
    string Name,
    string? Description,
    string DocumentType,
    string FileName,
    string ContentType,
    long FileSize,
    string StoragePath,
    string? FileHash,
    bool IsRequired,
    DateTime UploadedAt,
    string UploadedBy,
    DateTime? VerifiedAt,
    string? VerifiedBy
);

#endregion

#region Audit DTOs

public record ContractAuditLogDto(
    Guid Id,
    Guid ContractId,
    ContractAuditEventType EventType,
    string Description,
    string? OldValue,
    string? NewValue,
    string PerformedBy,
    string? IPAddress,
    string? UserAgent,
    DateTime PerformedAt
);

#endregion

#region Certification Authority DTOs

public record CertificationAuthorityDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Country,
    string? Website,
    string? CertificateUrl,
    string? PublicKey,
    bool IsActive,
    bool IsGovernmentApproved,
    string? AccreditationNumber,
    DateTime? ValidFrom,
    DateTime? ValidUntil
);

#endregion

#region Response DTOs

public record CreateContractResponse(Guid ContractId, string ContractNumber);

public record SignContractResponse(
    bool Success,
    Guid SignatureId,
    string Message,
    bool ContractFullySigned
);

public record SignatureVerificationDto(
    Guid SignatureId,
    bool IsValid,
    SignatureVerificationStatus VerificationStatus,
    string? Message,
    DateTime? VerifiedAt,
    string? CertificateIssuer,
    DateTime? CertificateValidFrom,
    DateTime? CertificateValidTo,
    bool CertificateIsValid
);

#endregion

#region Pagination

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

#endregion
