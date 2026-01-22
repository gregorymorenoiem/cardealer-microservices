// DisputeService - Domain Entities
// Pro-Consumidor RD + Ley 126-02 Comercio Electrónico

namespace DisputeService.Domain.Entities;

#region Enums

public enum DisputeType
{
    VehicleDefect,
    PaymentIssue,
    ContractBreach,
    DeliveryIssue,
    FraudClaim,
    WarrantyClaim,
    RefundRequest,
    ServiceQuality,
    Other
}

public enum DisputeStatus
{
    Filed,
    Acknowledged,
    InReview,
    InMediation,
    PendingResolution,
    Resolved,
    Closed,
    Escalated,
    ReferredToProConsumidor
}

public enum DisputePriority
{
    Low,
    Normal,
    High,
    Critical
}

public enum ResolutionType
{
    FullRefundBuyer,
    PartialRefund,
    Replacement,
    Repair,
    FavorSeller,
    MutualAgreement,
    NoResolution
}

public enum ParticipantRole
{
    Complainant,
    Respondent,
    Mediator,
    Witness,
    LegalRepresentative
}

public enum EvidenceStatus
{
    Submitted,
    UnderReview,
    Accepted,
    Rejected
}

public enum CommunicationChannel
{
    Platform,
    Email,
    Phone,
    VideoCall
}

#endregion

#region Entities

public class Dispute
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CaseNumber { get; set; } = string.Empty;
    public DisputeType Type { get; set; }
    public DisputeStatus Status { get; set; } = DisputeStatus.Filed;
    public DisputePriority Priority { get; set; } = DisputePriority.Normal;
    
    // Complainant Info
    public Guid ComplainantId { get; set; }
    public string ComplainantName { get; set; } = string.Empty;
    public string ComplainantEmail { get; set; } = string.Empty;
    
    // Respondent Info
    public Guid RespondentId { get; set; }
    public string RespondentName { get; set; } = string.Empty;
    public string RespondentEmail { get; set; } = string.Empty;
    
    // Dispute Details
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? DisputedAmount { get; set; }
    public string Currency { get; set; } = "DOP";
    
    // Mediator
    public string? AssignedMediatorId { get; set; }
    public string? AssignedMediatorName { get; set; }
    public DateTime? AssignedAt { get; set; }
    
    // Resolution
    public ResolutionType? Resolution { get; set; }
    public string? ResolutionSummary { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    
    // Pro-Consumidor
    public bool IsEscalated { get; set; }
    public DateTime? EscalatedAt { get; set; }
    public bool ReferredToProConsumidor { get; set; }
    public DateTime? ProConsumidorReferralDate { get; set; }
    public string? ProConsumidorCaseNumber { get; set; }
    
    // Dates
    public DateTime FiledAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResponseDueDate { get; set; }
    public DateTime? ResolutionDueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class DisputeEvidence
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DisputeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string EvidenceType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public Guid SubmittedById { get; set; }
    public string SubmittedByName { get; set; } = string.Empty;
    public ParticipantRole SubmitterRole { get; set; }
    public EvidenceStatus Status { get; set; } = EvidenceStatus.Submitted;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public string? ReviewNotes { get; set; }
}

public class DisputeComment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DisputeId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public ParticipantRole AuthorRole { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public bool IsOfficial { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public class DisputeTimelineEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DisputeId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public ParticipantRole? PerformerRole { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
}

public class MediationSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DisputeId { get; set; }
    public string SessionNumber { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public CommunicationChannel Channel { get; set; }
    public string? MeetingLink { get; set; }
    public string? Location { get; set; }
    public string MediatorId { get; set; } = string.Empty;
    public string MediatorName { get; set; } = string.Empty;
    public string Status { get; set; } = "Scheduled";
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? Summary { get; set; }
    public string? Notes { get; set; }
    public bool ComplainantAttended { get; set; }
    public bool RespondentAttended { get; set; }
    public bool PartiesAgreed { get; set; }
}

public class DisputeParticipant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DisputeId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public ParticipantRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public CommunicationChannel PreferredChannel { get; set; } = CommunicationChannel.Platform;
}

public class ResolutionTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DisputeType ForDisputeType { get; set; }
    public ResolutionType ResolutionType { get; set; }
    public string TemplateContent { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class DisputeSlaConfiguration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DisputeType DisputeType { get; set; }
    public DisputePriority Priority { get; set; }
    public int ResponseDeadlineHours { get; set; } = 48;
    public int ResolutionDeadlineHours { get; set; } = 720; // 30 days
    public int EscalationThresholdHours { get; set; } = 168; // 7 days
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

#endregion
