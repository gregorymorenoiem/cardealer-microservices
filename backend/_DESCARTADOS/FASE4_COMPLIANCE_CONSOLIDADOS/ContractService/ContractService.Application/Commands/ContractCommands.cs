// ContractService - Commands

namespace ContractService.Application.Commands;

using MediatR;
using ContractService.Domain.Entities;
using ContractService.Application.DTOs;

#region Template Commands

public record CreateTemplateCommand(
    string Code,
    string Name,
    string? Description,
    ContractType Type,
    string ContentHtml,
    string? ContentJson,
    List<string>? RequiredVariables,
    List<string>? OptionalVariables,
    string Language,
    string? LegalBasis,
    bool RequiresWitness,
    int MinimumSignatures,
    bool RequiresNotarization,
    int? ValidityDays,
    string CreatedBy
) : IRequest<Guid>;

public record UpdateTemplateCommand(
    Guid Id,
    string Name,
    string? Description,
    string ContentHtml,
    string? ContentJson,
    List<string>? RequiredVariables,
    List<string>? OptionalVariables,
    string? LegalBasis,
    bool RequiresWitness,
    int MinimumSignatures,
    bool RequiresNotarization,
    int? ValidityDays,
    bool IsActive,
    string UpdatedBy
) : IRequest<bool>;

public record AddTemplateClauseCommand(
    Guid TemplateId,
    string Code,
    string Title,
    string Content,
    ClauseType Type,
    int Order,
    bool IsMandatory,
    bool IsEditable,
    string? LegalReference,
    string? HelpText
) : IRequest<Guid>;

#endregion

#region Contract Commands

public record CreateContractCommand(
    Guid? TemplateId,
    ContractType Type,
    string Title,
    string? Description,
    DateTime EffectiveDate,
    DateTime? ExpirationDate,
    string? SubjectType,
    Guid? SubjectId,
    string? SubjectDescription,
    decimal? ContractValue,
    string Currency,
    string? LegalJurisdiction,
    List<CreatePartyDto>? Parties,
    Dictionary<string, string>? TemplateVariables,
    string CreatedBy
) : IRequest<CreateContractResponse>;

public record CreatePartyDto(
    PartyType Type,
    PartyRole Role,
    Guid? UserId,
    string FullName,
    string? DocumentType,
    string? DocumentNumber,
    string Email,
    string? Phone,
    string? CompanyName,
    string? RNC
);

public record UpdateContractCommand(
    Guid Id,
    string Title,
    string? Description,
    DateTime? ExpirationDate,
    decimal? ContractValue,
    string? Notes,
    string UpdatedBy
) : IRequest<bool>;

public record AddPartyToContractCommand(
    Guid ContractId,
    PartyType Type,
    PartyRole Role,
    Guid? UserId,
    string FullName,
    string? DocumentType,
    string? DocumentNumber,
    string Email,
    string? Phone,
    string? CompanyName,
    string? RNC
) : IRequest<Guid>;

public record RemovePartyFromContractCommand(
    Guid ContractId,
    Guid PartyId,
    string RemovedBy
) : IRequest<bool>;

public record FinalizeContractCommand(
    Guid Id,
    string FinalizedBy
) : IRequest<bool>;

public record TerminateContractCommand(
    Guid Id,
    string Reason,
    string TerminatedBy
) : IRequest<bool>;

public record RenewContractCommand(
    Guid Id,
    DateTime NewExpirationDate,
    decimal? NewValue,
    string RenewedBy
) : IRequest<Guid>;  // Returns new contract ID

public record ArchiveContractCommand(
    Guid Id,
    string ArchivedBy
) : IRequest<bool>;

#endregion

#region Signature Commands

public record RequestSignatureCommand(
    Guid ContractId,
    Guid PartyId,
    SignatureType Type,
    DateTime? ExpiresAt,
    string RequestedBy
) : IRequest<Guid>;

public record RequestAllSignaturesCommand(
    Guid ContractId,
    SignatureType Type,
    int ExpirationDays,
    string RequestedBy
) : IRequest<List<Guid>>;

public record SignContractCommand(
    Guid SignatureId,
    string? SignatureData,
    string? SignatureImage,
    string? CertificateData,
    string IPAddress,
    string UserAgent,
    string? GeoLocation,
    string? DeviceFingerprint,
    bool BiometricVerified,
    string? BiometricType
) : IRequest<SignContractResponse>;

public record DeclineSignatureCommand(
    Guid SignatureId,
    string Reason,
    string DeclinedBy
) : IRequest<bool>;

public record VerifySignatureCommand(
    Guid SignatureId
) : IRequest<SignatureVerificationDto>;

#endregion

#region Clause Commands

public record UpdateClauseCommand(
    Guid ClauseId,
    string Content,
    string ModificationReason,
    string ModifiedBy
) : IRequest<bool>;

public record AcceptClauseCommand(
    Guid ClauseId,
    string AcceptedBy
) : IRequest<bool>;

public record AcceptAllClausesCommand(
    Guid ContractId,
    string AcceptedBy
) : IRequest<bool>;

#endregion

#region Document Commands

public record UploadDocumentCommand(
    Guid ContractId,
    string Name,
    string? Description,
    string DocumentType,
    string FileName,
    string ContentType,
    long FileSize,
    string StoragePath,
    string FileHash,
    bool IsRequired,
    string UploadedBy
) : IRequest<Guid>;

public record VerifyDocumentCommand(
    Guid DocumentId,
    string VerifiedBy
) : IRequest<bool>;

public record DeleteDocumentCommand(
    Guid DocumentId,
    string DeletedBy
) : IRequest<bool>;

#endregion

#region Terms Commands

public record AcceptTermsCommand(
    Guid ContractId,
    bool AcceptedTerms,
    bool AcceptedPrivacyPolicy,
    string AcceptedBy,
    string IPAddress
) : IRequest<bool>;

#endregion
