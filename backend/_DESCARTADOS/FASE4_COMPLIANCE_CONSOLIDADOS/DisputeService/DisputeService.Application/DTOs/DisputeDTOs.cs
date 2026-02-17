// DisputeService - DTOs
// Pro-Consumidor RD + Ley 126-02

namespace DisputeService.Application.DTOs;

using DisputeService.Domain.Entities;

#region Dispute DTOs

public record DisputeDto(
    Guid Id,
    string CaseNumber,
    DisputeType Type,
    DisputeStatus Status,
    DisputePriority Priority,
    Guid ComplainantId,
    string ComplainantName,
    string ComplainantEmail,
    Guid RespondentId,
    string RespondentName,
    string RespondentEmail,
    string Title,
    string Description,
    decimal? DisputedAmount,
    string Currency,
    string? AssignedMediatorId,
    string? AssignedMediatorName,
    DateTime? AssignedAt,
    ResolutionType? Resolution,
    string? ResolutionSummary,
    DateTime? ResolvedAt,
    bool IsEscalated,
    bool ReferredToProConsumidor,
    string? ProConsumidorCaseNumber,
    DateTime FiledAt,
    DateTime? ResponseDueDate,
    DateTime? ResolutionDueDate,
    DateTime CreatedAt);

public record DisputeSummaryDto(
    Guid Id,
    string CaseNumber,
    DisputeType Type,
    DisputeStatus Status,
    DisputePriority Priority,
    string ComplainantName,
    string RespondentName,
    string Title,
    decimal? DisputedAmount,
    DateTime FiledAt,
    bool IsEscalated);

public record CreateDisputeDto(
    DisputeType Type,
    Guid RespondentId,
    string RespondentName,
    string RespondentEmail,
    string Title,
    string Description,
    decimal? DisputedAmount,
    string Currency);

public record UpdateDisputeDto(
    string? Title,
    string? Description,
    DisputePriority? Priority);

public record DisputePagedResultDto(
    List<DisputeSummaryDto> Items,
    int Total,
    int Page,
    int PageSize);

#endregion

#region Evidence DTOs

public record DisputeEvidenceDto(
    Guid Id,
    Guid DisputeId,
    string Name,
    string? Description,
    string EvidenceType,
    string FileName,
    string ContentType,
    long FileSize,
    string StoragePath,
    Guid SubmittedById,
    string SubmittedByName,
    ParticipantRole SubmitterRole,
    EvidenceStatus Status,
    DateTime SubmittedAt,
    DateTime? ReviewedAt,
    string? ReviewNotes);

public record SubmitEvidenceDto(
    string Name,
    string? Description,
    string EvidenceType,
    string FileName,
    string ContentType,
    long FileSize,
    string StoragePath);

public record ReviewEvidenceDto(
    EvidenceStatus Status,
    string? ReviewNotes);

#endregion

#region Comment DTOs

public record DisputeCommentDto(
    Guid Id,
    Guid DisputeId,
    Guid? ParentCommentId,
    Guid AuthorId,
    string AuthorName,
    ParticipantRole AuthorRole,
    string Content,
    bool IsInternal,
    bool IsOfficial,
    DateTime CreatedAt,
    DateTime? EditedAt);

public record CreateCommentDto(
    string Content,
    Guid? ParentCommentId,
    bool IsInternal,
    bool IsOfficial);

#endregion

#region Timeline DTOs

public record DisputeTimelineEventDto(
    Guid Id,
    Guid DisputeId,
    string EventType,
    string Description,
    string? OldValue,
    string? NewValue,
    string PerformedBy,
    ParticipantRole? PerformerRole,
    DateTime OccurredAt);

#endregion

#region Mediation DTOs

public record MediationSessionDto(
    Guid Id,
    Guid DisputeId,
    string SessionNumber,
    DateTime ScheduledAt,
    int DurationMinutes,
    CommunicationChannel Channel,
    string? MeetingLink,
    string? Location,
    string MediatorId,
    string MediatorName,
    string Status,
    DateTime? StartedAt,
    DateTime? EndedAt,
    string? Summary,
    bool ComplainantAttended,
    bool RespondentAttended,
    bool PartiesAgreed);

public record ScheduleMediationDto(
    DateTime ScheduledAt,
    int DurationMinutes,
    CommunicationChannel Channel,
    string? MeetingLink,
    string? Location);

public record CompleteMediationDto(
    string Summary,
    string? Notes,
    bool ComplainantAttended,
    bool RespondentAttended,
    bool PartiesAgreed);

#endregion

#region Participant DTOs

public record DisputeParticipantDto(
    Guid Id,
    Guid DisputeId,
    Guid UserId,
    string UserName,
    string UserEmail,
    ParticipantRole Role,
    bool IsActive,
    DateTime JoinedAt,
    CommunicationChannel PreferredChannel);

public record AddParticipantDto(
    Guid UserId,
    string UserName,
    string UserEmail,
    ParticipantRole Role,
    CommunicationChannel PreferredChannel);

#endregion

#region Resolution DTOs

public record ResolveDisputeDto(
    ResolutionType Resolution,
    string Summary);

public record ResolutionTemplateDto(
    Guid Id,
    string Name,
    DisputeType ForDisputeType,
    ResolutionType ResolutionType,
    string TemplateContent,
    bool IsActive);

#endregion

#region SLA DTOs

public record DisputeSlaConfigurationDto(
    Guid Id,
    DisputeType DisputeType,
    DisputePriority Priority,
    int ResponseDeadlineHours,
    int ResolutionDeadlineHours,
    int EscalationThresholdHours,
    bool IsActive);

#endregion

#region Statistics DTOs

public record DisputeStatisticsDto(
    int TotalDisputes,
    int OpenDisputes,
    int ResolvedDisputes,
    int EscalatedDisputes,
    int ProConsumidorReferrals,
    decimal AverageResolutionDays,
    Dictionary<DisputeStatus, int> ByStatus,
    Dictionary<DisputeType, int> ByType);

#endregion
