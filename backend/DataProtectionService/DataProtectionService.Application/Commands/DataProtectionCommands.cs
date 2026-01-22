using MediatR;
using DataProtectionService.Application.DTOs;

namespace DataProtectionService.Application.Commands;

public record LogDataChangeCommand(
    Guid UserId,
    string DataField,
    string DataCategory,
    string OldValue,
    string NewValue,
    string ChangedByType,
    Guid? ChangedById,
    string? Reason,
    string SourceService,
    string? IpAddress
) : IRequest<DataChangeLogDto>;

public record AnonymizeUserDataCommand(
    Guid UserId,
    Guid? ARCORequestId,
    Guid? ProcessedBy,
    string? Reason,
    string? OriginalEmail,
    string? OriginalPhone
) : IRequest<AnonymizationResultDto>;

public record CreatePrivacyPolicyCommand(
    string Version,
    string DocumentType,
    string Content,
    string? ChangesSummary,
    string Language,
    DateTime EffectiveDate,
    bool RequiresReAcceptance,
    Guid CreatedById
) : IRequest<PrivacyPolicyDto>;

public record UpdatePrivacyPolicyCommand(
    Guid PolicyId,
    string? Content,
    string? ChangesSummary,
    DateTime? EffectiveDate,
    bool? IsActive,
    Guid UpdatedById
) : IRequest<PrivacyPolicyDto>;

public record ExportUserDataCommand(
    Guid UserId,
    string Format,
    bool IncludeConsents,
    bool IncludeChangeHistory,
    Guid RequestedById
) : IRequest<UserDataExportDto>;

public record RequestDataExportCommand(
    Guid UserId,
    Guid? ARCORequestId,
    string? Format,
    bool? IncludeTransactions,
    bool? IncludeMessages,
    bool? IncludeVehicleHistory,
    bool? IncludeUserActivity,
    string? IpAddress,
    string? UserAgent
) : IRequest<DataExportDto>;

public record CompleteDataExportCommand(
    Guid ExportId,
    bool Success,
    string? DownloadUrl,
    DateTime? ExpiresAt,
    long? FileSizeBytes,
    string? ErrorMessage
) : IRequest<bool>;
