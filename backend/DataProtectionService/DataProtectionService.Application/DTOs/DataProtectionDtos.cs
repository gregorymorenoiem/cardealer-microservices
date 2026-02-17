namespace DataProtectionService.Application.DTOs;

public record UserConsentDto(
    Guid Id,
    Guid UserId,
    string Type,
    string Version,
    bool Granted,
    DateTime GrantedAt,
    DateTime? RevokedAt,
    string? RevokeReason,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateConsentRequest(
    Guid UserId,
    string Type,
    string Version,
    string DocumentHash,
    bool Granted,
    string? CollectionMethod
);

public record RevokeConsentRequest(
    string Reason
);

public record ARCORequestDto(
    Guid Id,
    Guid UserId,
    string RequestNumber,
    string Type,
    string Status,
    string Description,
    DateTime RequestedAt,
    DateTime Deadline,
    DateTime? CompletedAt,
    string? ProcessedByName,
    string? Resolution,
    string? RejectionReason,
    string? ExportFileUrl,
    bool IsOverdue,
    int DaysRemaining,
    List<ARCOAttachmentDto> Attachments,
    List<ARCOStatusHistoryDto> StatusHistory
);

public record ARCOAttachmentDto(
    Guid Id,
    string FileName,
    string FileUrl,
    string FileType,
    long FileSize,
    DateTime UploadedAt
);

public record ARCOStatusHistoryDto(
    string OldStatus,
    string NewStatus,
    string? Comment,
    string ChangedByName,
    DateTime ChangedAt
);

public record CreateARCORequest(
    Guid UserId,
    string Type,
    string Description,
    string? SpecificDataRequested,
    string? ProposedChanges,
    string? OppositionReason
);

public record ProcessARCORequest(
    string Status,
    string? Resolution,
    string? RejectionReason,
    string? InternalNotes
);

public record DataChangeLogDto(
    Guid Id,
    Guid UserId,
    string DataField,
    string DataCategory,
    string? OldValueMasked,
    string? NewValueMasked,
    string ChangedByType,
    string? ChangedByName,
    string? Reason,
    string SourceService,
    DateTime ChangedAt
);

public record CreateDataChangeLogRequest(
    Guid UserId,
    string DataField,
    string DataCategory,
    string OldValue,
    string NewValue,
    string ChangedByType,
    Guid? ChangedById,
    string? Reason,
    string SourceService
);

public record PrivacyPolicyDto(
    Guid Id,
    string Version,
    string DocumentType,
    string Content,
    string? ChangesSummary,
    string Language,
    DateTime EffectiveDate,
    bool IsActive,
    bool RequiresReAcceptance,
    DateTime CreatedAt
);

public record CreatePrivacyPolicyRequest(
    string Version,
    string DocumentType,
    string Content,
    string? ChangesSummary,
    string Language,
    DateTime EffectiveDate,
    bool RequiresReAcceptance
);

public record UserDataExportDto(
    Guid UserId,
    string Email,
    DateTime ExportedAt,
    Dictionary<string, object> PersonalData,
    List<UserConsentDto> Consents,
    List<DataChangeLogDto> ChangeHistory
);

public record DataProtectionStatsDto(
    int TotalConsents,
    int ActiveConsents,
    int RevokedConsents,
    int PendingARCORequests,
    int CompletedARCORequests,
    int OverdueARCORequests,
    Dictionary<string, int> ARCOByType,
    Dictionary<string, int> ConsentsByType,
    double AverageARCOProcessingDays
);

// Data Export DTOs
public record DataExportDto(
    Guid Id,
    Guid UserId,
    Guid? ARCORequestId,
    string Status,
    string Format,
    bool IncludeTransactions,
    bool IncludeMessages,
    bool IncludeVehicleHistory,
    bool IncludeUserActivity,
    DateTime RequestedAt,
    DateTime? CompletedAt,
    string? DownloadUrl,
    DateTime? DownloadExpiresAt,
    long? FileSizeBytes,
    string? ErrorMessage
);

// Anonymization DTOs
public record AnonymizationRecordDto(
    Guid Id,
    Guid UserId,
    Guid? ARCORequestId,
    Guid? RequestedBy,
    DateTime AnonymizedAt,
    string Reason,
    List<string> AffectedTables,
    int AffectedRecordsCount,
    bool IsComplete
);

public record AnonymizationResultDto(
    bool Success,
    string Message,
    int AffectedRecordsCount,
    DateTime? AnonymizedAt
);

// ARCO Statistics DTOs
public class ARCOStatisticsDto
{
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int CompletedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int OverdueRequests { get; set; }
    public double AverageProcessingDays { get; set; }
    public Dictionary<string, int> RequestsByType { get; set; } = new();
    public Dictionary<string, int> RequestsByStatus { get; set; } = new();
    public double ComplianceRate { get; set; }
}
