// DisputeService - Commands

namespace DisputeService.Application.Commands;

using MediatR;
using DisputeService.Domain.Entities;

#region Dispute Commands

public record FileDisputeCommand(
    DisputeType Type,
    Guid ComplainantId,
    string ComplainantName,
    string ComplainantEmail,
    Guid RespondentId,
    string RespondentName,
    string RespondentEmail,
    string Title,
    string Description,
    decimal? DisputedAmount,
    string Currency = "DOP"
) : IRequest<Guid>;

public record AcknowledgeDisputeCommand(Guid DisputeId) : IRequest<bool>;

public record AssignMediatorCommand(Guid DisputeId, string MediatorId, string MediatorName) : IRequest<bool>;

public record EscalateDisputeCommand(Guid DisputeId, string Reason) : IRequest<bool>;

public record ResolveDisputeCommand(
    Guid DisputeId,
    ResolutionType Resolution,
    string ResolutionSummary,
    string ResolvedBy
) : IRequest<bool>;

public record CloseDisputeCommand(Guid DisputeId, string Reason) : IRequest<bool>;

public record ReferToProConsumidorCommand(Guid DisputeId, string Reason) : IRequest<bool>;

public record UpdateDisputeStatusCommand(Guid DisputeId, DisputeStatus NewStatus, string Reason) : IRequest<bool>;

#endregion

#region Evidence Commands

public record SubmitEvidenceCommand(
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
    ParticipantRole SubmitterRole
) : IRequest<Guid>;

public record ReviewEvidenceCommand(
    Guid EvidenceId,
    EvidenceStatus NewStatus,
    string ReviewedBy,
    string? ReviewNotes
) : IRequest<bool>;

#endregion

#region Comment Commands

public record AddCommentCommand(
    Guid DisputeId,
    Guid AuthorId,
    string AuthorName,
    ParticipantRole AuthorRole,
    string Content,
    bool IsInternal = false,
    bool IsOfficial = false
) : IRequest<Guid>;

public record EditCommentCommand(Guid CommentId, string NewContent) : IRequest<bool>;

public record DeleteCommentCommand(Guid CommentId) : IRequest<bool>;

#endregion

#region Mediation Commands

public record ScheduleMediationCommand(
    Guid DisputeId,
    DateTime ScheduledAt,
    int DurationMinutes,
    CommunicationChannel Channel,
    string? MeetingLink,
    string? Location,
    string MediatorId,
    string MediatorName
) : IRequest<Guid>;

public record StartMediationCommand(Guid SessionId) : IRequest<bool>;

public record CompleteMediationCommand(
    Guid SessionId,
    string Summary,
    string? Notes,
    bool PartiesAgreed,
    bool ComplainantAttended,
    bool RespondentAttended
) : IRequest<bool>;

public record CancelMediationCommand(Guid SessionId, string Reason) : IRequest<bool>;

#endregion

#region Participant Commands

public record AddParticipantCommand(
    Guid DisputeId,
    Guid UserId,
    string UserName,
    string UserEmail,
    ParticipantRole Role
) : IRequest<Guid>;

public record RemoveParticipantCommand(Guid ParticipantId) : IRequest<bool>;

#endregion

#region Configuration Commands

public record CreateResolutionTemplateCommand(
    string Name,
    DisputeType ForDisputeType,
    ResolutionType ResolutionType,
    string TemplateContent,
    string CreatedBy
) : IRequest<Guid>;

public record CreateSlaConfigurationCommand(
    DisputeType DisputeType,
    DisputePriority Priority,
    int ResponseDeadlineHours,
    int ResolutionDeadlineHours,
    int EscalationThresholdHours
) : IRequest<Guid>;

#endregion
