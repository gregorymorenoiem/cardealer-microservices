// DisputeService - Command Handlers
// Pro-Consumidor RD + Ley 126-02

namespace DisputeService.Application.Handlers;

using MediatR;
using DisputeService.Application.Commands;
using DisputeService.Domain.Entities;
using DisputeService.Domain.Interfaces;

#region Dispute Command Handlers

public class FileDisputeHandler : IRequestHandler<FileDisputeCommand, Guid>
{
    private readonly IDisputeRepository _repo;
    private readonly IDisputeSlaConfigurationRepository _slaRepo;
    private readonly IDisputeTimelineRepository _timelineRepo;

    public FileDisputeHandler(
        IDisputeRepository repo,
        IDisputeSlaConfigurationRepository slaRepo,
        IDisputeTimelineRepository timelineRepo)
    {
        _repo = repo;
        _slaRepo = slaRepo;
        _timelineRepo = timelineRepo;
    }

    public async Task<Guid> Handle(FileDisputeCommand request, CancellationToken ct)
    {
        var caseNumber = await _repo.GenerateCaseNumberAsync(ct);
        
        var sla = await _slaRepo.GetByTypeAndPriorityAsync(request.Type, DisputePriority.Normal, ct);
        
        var dispute = new Dispute
        {
            CaseNumber = caseNumber,
            Type = request.Type,
            ComplainantId = request.ComplainantId,
            ComplainantName = request.ComplainantName,
            ComplainantEmail = request.ComplainantEmail,
            RespondentId = request.RespondentId,
            RespondentName = request.RespondentName,
            RespondentEmail = request.RespondentEmail,
            Title = request.Title,
            Description = request.Description,
            DisputedAmount = request.DisputedAmount,
            Currency = request.Currency,
            ResponseDueDate = sla != null ? DateTime.UtcNow.AddHours(sla.ResponseDeadlineHours) : DateTime.UtcNow.AddDays(2),
            ResolutionDueDate = sla != null ? DateTime.UtcNow.AddHours(sla.ResolutionDeadlineHours) : DateTime.UtcNow.AddDays(30)
        };

        await _repo.AddAsync(dispute, ct);

        await _timelineRepo.AddAsync(new DisputeTimelineEvent
        {
            DisputeId = dispute.Id,
            EventType = "DisputeFiled",
            Description = $"Disputa registrada por {request.ComplainantName}",
            PerformedBy = request.ComplainantName,
            PerformerRole = ParticipantRole.Complainant
        }, ct);

        return dispute.Id;
    }
}

public class AcknowledgeDisputeHandler : IRequestHandler<AcknowledgeDisputeCommand, bool>
{
    private readonly IDisputeRepository _repo;
    private readonly IDisputeTimelineRepository _timelineRepo;

    public AcknowledgeDisputeHandler(IDisputeRepository repo, IDisputeTimelineRepository timelineRepo)
    {
        _repo = repo;
        _timelineRepo = timelineRepo;
    }

    public async Task<bool> Handle(AcknowledgeDisputeCommand request, CancellationToken ct)
    {
        var dispute = await _repo.GetByIdAsync(request.DisputeId, ct);
        if (dispute == null) return false;

        dispute.Status = DisputeStatus.Acknowledged;
        dispute.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(dispute, ct);

        await _timelineRepo.AddAsync(new DisputeTimelineEvent
        {
            DisputeId = dispute.Id,
            EventType = "DisputeAcknowledged",
            Description = "Disputa reconocida",
            OldValue = DisputeStatus.Filed.ToString(),
            NewValue = DisputeStatus.Acknowledged.ToString(),
            PerformedBy = "System"
        }, ct);

        return true;
    }
}

public class AssignMediatorHandler : IRequestHandler<AssignMediatorCommand, bool>
{
    private readonly IDisputeRepository _repo;
    private readonly IDisputeTimelineRepository _timelineRepo;

    public AssignMediatorHandler(IDisputeRepository repo, IDisputeTimelineRepository timelineRepo)
    {
        _repo = repo;
        _timelineRepo = timelineRepo;
    }

    public async Task<bool> Handle(AssignMediatorCommand request, CancellationToken ct)
    {
        var dispute = await _repo.GetByIdAsync(request.DisputeId, ct);
        if (dispute == null) return false;

        dispute.AssignedMediatorId = request.MediatorId;
        dispute.AssignedMediatorName = request.MediatorName;
        dispute.AssignedAt = DateTime.UtcNow;
        dispute.Status = DisputeStatus.InMediation;
        dispute.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(dispute, ct);

        await _timelineRepo.AddAsync(new DisputeTimelineEvent
        {
            DisputeId = dispute.Id,
            EventType = "MediatorAssigned",
            Description = $"Mediador asignado: {request.MediatorName}",
            NewValue = request.MediatorName,
            PerformedBy = "Admin",
            PerformerRole = ParticipantRole.Mediator
        }, ct);

        return true;
    }
}

public class EscalateDisputeHandler : IRequestHandler<EscalateDisputeCommand, bool>
{
    private readonly IDisputeRepository _repo;
    private readonly IDisputeTimelineRepository _timelineRepo;

    public EscalateDisputeHandler(IDisputeRepository repo, IDisputeTimelineRepository timelineRepo)
    {
        _repo = repo;
        _timelineRepo = timelineRepo;
    }

    public async Task<bool> Handle(EscalateDisputeCommand request, CancellationToken ct)
    {
        var dispute = await _repo.GetByIdAsync(request.DisputeId, ct);
        if (dispute == null) return false;

        dispute.IsEscalated = true;
        dispute.EscalatedAt = DateTime.UtcNow;
        dispute.Status = DisputeStatus.Escalated;
        dispute.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(dispute, ct);

        await _timelineRepo.AddAsync(new DisputeTimelineEvent
        {
            DisputeId = dispute.Id,
            EventType = "DisputeEscalated",
            Description = $"Disputa escalada: {request.Reason}",
            PerformedBy = "System"
        }, ct);

        return true;
    }
}

public class ResolveDisputeHandler : IRequestHandler<ResolveDisputeCommand, bool>
{
    private readonly IDisputeRepository _repo;
    private readonly IDisputeTimelineRepository _timelineRepo;

    public ResolveDisputeHandler(IDisputeRepository repo, IDisputeTimelineRepository timelineRepo)
    {
        _repo = repo;
        _timelineRepo = timelineRepo;
    }

    public async Task<bool> Handle(ResolveDisputeCommand request, CancellationToken ct)
    {
        var dispute = await _repo.GetByIdAsync(request.DisputeId, ct);
        if (dispute == null) return false;

        dispute.Resolution = request.Resolution;
        dispute.ResolutionSummary = request.ResolutionSummary;
        dispute.ResolvedAt = DateTime.UtcNow;
        dispute.ResolvedBy = request.ResolvedBy;
        dispute.Status = DisputeStatus.Resolved;
        dispute.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(dispute, ct);

        await _timelineRepo.AddAsync(new DisputeTimelineEvent
        {
            DisputeId = dispute.Id,
            EventType = "DisputeResolved",
            Description = $"Disputa resuelta: {request.Resolution}",
            NewValue = request.Resolution.ToString(),
            PerformedBy = request.ResolvedBy,
            PerformerRole = ParticipantRole.Mediator
        }, ct);

        return true;
    }
}

public class CloseDisputeHandler : IRequestHandler<CloseDisputeCommand, bool>
{
    private readonly IDisputeRepository _repo;
    private readonly IDisputeTimelineRepository _timelineRepo;

    public CloseDisputeHandler(IDisputeRepository repo, IDisputeTimelineRepository timelineRepo)
    {
        _repo = repo;
        _timelineRepo = timelineRepo;
    }

    public async Task<bool> Handle(CloseDisputeCommand request, CancellationToken ct)
    {
        var dispute = await _repo.GetByIdAsync(request.DisputeId, ct);
        if (dispute == null) return false;

        dispute.Status = DisputeStatus.Closed;
        dispute.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(dispute, ct);

        await _timelineRepo.AddAsync(new DisputeTimelineEvent
        {
            DisputeId = dispute.Id,
            EventType = "DisputeClosed",
            Description = $"Disputa cerrada: {request.Reason}",
            PerformedBy = "Admin"
        }, ct);

        return true;
    }
}

public class ReferToProConsumidorHandler : IRequestHandler<ReferToProConsumidorCommand, bool>
{
    private readonly IDisputeRepository _repo;
    private readonly IDisputeTimelineRepository _timelineRepo;

    public ReferToProConsumidorHandler(IDisputeRepository repo, IDisputeTimelineRepository timelineRepo)
    {
        _repo = repo;
        _timelineRepo = timelineRepo;
    }

    public async Task<bool> Handle(ReferToProConsumidorCommand request, CancellationToken ct)
    {
        var dispute = await _repo.GetByIdAsync(request.DisputeId, ct);
        if (dispute == null) return false;

        dispute.ReferredToProConsumidor = true;
        dispute.ProConsumidorReferralDate = DateTime.UtcNow;
        dispute.Status = DisputeStatus.ReferredToProConsumidor;
        dispute.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(dispute, ct);

        await _timelineRepo.AddAsync(new DisputeTimelineEvent
        {
            DisputeId = dispute.Id,
            EventType = "ReferredToProConsumidor",
            Description = $"Referido a Pro-Consumidor: {request.Reason}",
            PerformedBy = "System"
        }, ct);

        return true;
    }
}

public class UpdateDisputeStatusHandler : IRequestHandler<UpdateDisputeStatusCommand, bool>
{
    private readonly IDisputeRepository _repo;
    private readonly IDisputeTimelineRepository _timelineRepo;

    public UpdateDisputeStatusHandler(IDisputeRepository repo, IDisputeTimelineRepository timelineRepo)
    {
        _repo = repo;
        _timelineRepo = timelineRepo;
    }

    public async Task<bool> Handle(UpdateDisputeStatusCommand request, CancellationToken ct)
    {
        var dispute = await _repo.GetByIdAsync(request.DisputeId, ct);
        if (dispute == null) return false;

        var oldStatus = dispute.Status;
        dispute.Status = request.NewStatus;
        dispute.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(dispute, ct);

        await _timelineRepo.AddAsync(new DisputeTimelineEvent
        {
            DisputeId = dispute.Id,
            EventType = "StatusChanged",
            Description = request.Reason,
            OldValue = oldStatus.ToString(),
            NewValue = request.NewStatus.ToString(),
            PerformedBy = "Admin"
        }, ct);

        return true;
    }
}

#endregion

#region Evidence Command Handlers

public class SubmitEvidenceHandler : IRequestHandler<SubmitEvidenceCommand, Guid>
{
    private readonly IDisputeEvidenceRepository _repo;
    private readonly IDisputeTimelineRepository _timelineRepo;

    public SubmitEvidenceHandler(IDisputeEvidenceRepository repo, IDisputeTimelineRepository timelineRepo)
    {
        _repo = repo;
        _timelineRepo = timelineRepo;
    }

    public async Task<Guid> Handle(SubmitEvidenceCommand request, CancellationToken ct)
    {
        var evidence = new DisputeEvidence
        {
            DisputeId = request.DisputeId,
            Name = request.Name,
            Description = request.Description,
            EvidenceType = request.EvidenceType,
            FileName = request.FileName,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            StoragePath = request.StoragePath,
            SubmittedById = request.SubmittedById,
            SubmittedByName = request.SubmittedByName,
            SubmitterRole = request.SubmitterRole
        };

        await _repo.AddAsync(evidence, ct);

        await _timelineRepo.AddAsync(new DisputeTimelineEvent
        {
            DisputeId = request.DisputeId,
            EventType = "EvidenceSubmitted",
            Description = $"Evidencia presentada: {request.Name}",
            PerformedBy = request.SubmittedByName,
            PerformerRole = request.SubmitterRole
        }, ct);

        return evidence.Id;
    }
}

public class ReviewEvidenceHandler : IRequestHandler<ReviewEvidenceCommand, bool>
{
    private readonly IDisputeEvidenceRepository _repo;
    private readonly IDisputeTimelineRepository _timelineRepo;

    public ReviewEvidenceHandler(IDisputeEvidenceRepository repo, IDisputeTimelineRepository timelineRepo)
    {
        _repo = repo;
        _timelineRepo = timelineRepo;
    }

    public async Task<bool> Handle(ReviewEvidenceCommand request, CancellationToken ct)
    {
        var evidence = await _repo.GetByIdAsync(request.EvidenceId, ct);
        if (evidence == null) return false;

        evidence.Status = request.NewStatus;
        evidence.ReviewedAt = DateTime.UtcNow;
        evidence.ReviewedBy = request.ReviewedBy;
        evidence.ReviewNotes = request.ReviewNotes;
        await _repo.UpdateAsync(evidence, ct);

        await _timelineRepo.AddAsync(new DisputeTimelineEvent
        {
            DisputeId = evidence.DisputeId,
            EventType = "EvidenceReviewed",
            Description = $"Evidencia revisada: {evidence.Name} - {request.NewStatus}",
            NewValue = request.NewStatus.ToString(),
            PerformedBy = request.ReviewedBy,
            PerformerRole = ParticipantRole.Mediator
        }, ct);

        return true;
    }
}

#endregion

#region Comment Command Handlers

public class AddCommentHandler : IRequestHandler<AddCommentCommand, Guid>
{
    private readonly IDisputeCommentRepository _repo;

    public AddCommentHandler(IDisputeCommentRepository repo) => _repo = repo;

    public async Task<Guid> Handle(AddCommentCommand request, CancellationToken ct)
    {
        var comment = new DisputeComment
        {
            DisputeId = request.DisputeId,
            AuthorId = request.AuthorId,
            AuthorName = request.AuthorName,
            AuthorRole = request.AuthorRole,
            Content = request.Content,
            IsInternal = request.IsInternal,
            IsOfficial = request.IsOfficial
        };

        await _repo.AddAsync(comment, ct);
        return comment.Id;
    }
}

public class EditCommentHandler : IRequestHandler<EditCommentCommand, bool>
{
    private readonly IDisputeCommentRepository _repo;

    public EditCommentHandler(IDisputeCommentRepository repo) => _repo = repo;

    public async Task<bool> Handle(EditCommentCommand request, CancellationToken ct)
    {
        var comment = await _repo.GetByIdAsync(request.CommentId, ct);
        if (comment == null) return false;

        comment.Content = request.NewContent;
        comment.EditedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(comment, ct);
        return true;
    }
}

public class DeleteCommentHandler : IRequestHandler<DeleteCommentCommand, bool>
{
    private readonly IDisputeCommentRepository _repo;

    public DeleteCommentHandler(IDisputeCommentRepository repo) => _repo = repo;

    public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken ct)
    {
        var comment = await _repo.GetByIdAsync(request.CommentId, ct);
        if (comment == null) return false;

        comment.IsDeleted = true;
        await _repo.UpdateAsync(comment, ct);
        return true;
    }
}

#endregion

#region Mediation Command Handlers

public class ScheduleMediationHandler : IRequestHandler<ScheduleMediationCommand, Guid>
{
    private readonly IMediationSessionRepository _repo;
    private readonly IDisputeTimelineRepository _timelineRepo;

    public ScheduleMediationHandler(IMediationSessionRepository repo, IDisputeTimelineRepository timelineRepo)
    {
        _repo = repo;
        _timelineRepo = timelineRepo;
    }

    public async Task<Guid> Handle(ScheduleMediationCommand request, CancellationToken ct)
    {
        var sessionNumber = await _repo.GenerateSessionNumberAsync(request.DisputeId, ct);
        
        var session = new MediationSession
        {
            DisputeId = request.DisputeId,
            SessionNumber = sessionNumber,
            ScheduledAt = request.ScheduledAt,
            DurationMinutes = request.DurationMinutes,
            Channel = request.Channel,
            MeetingLink = request.MeetingLink,
            Location = request.Location,
            MediatorId = request.MediatorId,
            MediatorName = request.MediatorName
        };

        await _repo.AddAsync(session, ct);

        await _timelineRepo.AddAsync(new DisputeTimelineEvent
        {
            DisputeId = request.DisputeId,
            EventType = "MediationScheduled",
            Description = $"Sesión de mediación programada para {request.ScheduledAt:g}",
            PerformedBy = request.MediatorName,
            PerformerRole = ParticipantRole.Mediator
        }, ct);

        return session.Id;
    }
}

public class StartMediationHandler : IRequestHandler<StartMediationCommand, bool>
{
    private readonly IMediationSessionRepository _repo;

    public StartMediationHandler(IMediationSessionRepository repo) => _repo = repo;

    public async Task<bool> Handle(StartMediationCommand request, CancellationToken ct)
    {
        var session = await _repo.GetByIdAsync(request.SessionId, ct);
        if (session == null) return false;

        session.Status = "InProgress";
        session.StartedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(session, ct);
        return true;
    }
}

public class CompleteMediationHandler : IRequestHandler<CompleteMediationCommand, bool>
{
    private readonly IMediationSessionRepository _repo;
    private readonly IDisputeTimelineRepository _timelineRepo;

    public CompleteMediationHandler(IMediationSessionRepository repo, IDisputeTimelineRepository timelineRepo)
    {
        _repo = repo;
        _timelineRepo = timelineRepo;
    }

    public async Task<bool> Handle(CompleteMediationCommand request, CancellationToken ct)
    {
        var session = await _repo.GetByIdAsync(request.SessionId, ct);
        if (session == null) return false;

        session.Status = "Completed";
        session.EndedAt = DateTime.UtcNow;
        session.Summary = request.Summary;
        session.Notes = request.Notes;
        session.PartiesAgreed = request.PartiesAgreed;
        session.ComplainantAttended = request.ComplainantAttended;
        session.RespondentAttended = request.RespondentAttended;
        await _repo.UpdateAsync(session, ct);

        await _timelineRepo.AddAsync(new DisputeTimelineEvent
        {
            DisputeId = session.DisputeId,
            EventType = "MediationCompleted",
            Description = request.PartiesAgreed ? "Mediación completada - Partes llegaron a acuerdo" : "Mediación completada - Sin acuerdo",
            PerformedBy = session.MediatorName,
            PerformerRole = ParticipantRole.Mediator
        }, ct);

        return true;
    }
}

public class CancelMediationHandler : IRequestHandler<CancelMediationCommand, bool>
{
    private readonly IMediationSessionRepository _repo;
    private readonly IDisputeTimelineRepository _timelineRepo;

    public CancelMediationHandler(IMediationSessionRepository repo, IDisputeTimelineRepository timelineRepo)
    {
        _repo = repo;
        _timelineRepo = timelineRepo;
    }

    public async Task<bool> Handle(CancelMediationCommand request, CancellationToken ct)
    {
        var session = await _repo.GetByIdAsync(request.SessionId, ct);
        if (session == null) return false;

        session.Status = "Cancelled";
        await _repo.UpdateAsync(session, ct);

        await _timelineRepo.AddAsync(new DisputeTimelineEvent
        {
            DisputeId = session.DisputeId,
            EventType = "MediationCancelled",
            Description = $"Mediación cancelada: {request.Reason}",
            PerformedBy = session.MediatorName,
            PerformerRole = ParticipantRole.Mediator
        }, ct);

        return true;
    }
}

#endregion

#region Participant Command Handlers

public class AddParticipantHandler : IRequestHandler<AddParticipantCommand, Guid>
{
    private readonly IDisputeParticipantRepository _repo;
    private readonly IDisputeTimelineRepository _timelineRepo;

    public AddParticipantHandler(IDisputeParticipantRepository repo, IDisputeTimelineRepository timelineRepo)
    {
        _repo = repo;
        _timelineRepo = timelineRepo;
    }

    public async Task<Guid> Handle(AddParticipantCommand request, CancellationToken ct)
    {
        var participant = new DisputeParticipant
        {
            DisputeId = request.DisputeId,
            UserId = request.UserId,
            UserName = request.UserName,
            UserEmail = request.UserEmail,
            Role = request.Role
        };

        await _repo.AddAsync(participant, ct);

        await _timelineRepo.AddAsync(new DisputeTimelineEvent
        {
            DisputeId = request.DisputeId,
            EventType = "ParticipantAdded",
            Description = $"Participante agregado: {request.UserName} ({request.Role})",
            PerformedBy = "Admin"
        }, ct);

        return participant.Id;
    }
}

public class RemoveParticipantHandler : IRequestHandler<RemoveParticipantCommand, bool>
{
    private readonly IDisputeParticipantRepository _repo;

    public RemoveParticipantHandler(IDisputeParticipantRepository repo) => _repo = repo;

    public async Task<bool> Handle(RemoveParticipantCommand request, CancellationToken ct)
    {
        var list = await _repo.GetByDisputeAsync(Guid.Empty, ct);
        var participant = list.FirstOrDefault(p => p.Id == request.ParticipantId);
        if (participant == null) return false;

        participant.IsActive = false;
        participant.LeftAt = DateTime.UtcNow;
        await _repo.UpdateAsync(participant, ct);
        return true;
    }
}

#endregion

#region Configuration Command Handlers

public class CreateResolutionTemplateHandler : IRequestHandler<CreateResolutionTemplateCommand, Guid>
{
    private readonly IResolutionTemplateRepository _repo;

    public CreateResolutionTemplateHandler(IResolutionTemplateRepository repo) => _repo = repo;

    public async Task<Guid> Handle(CreateResolutionTemplateCommand request, CancellationToken ct)
    {
        var template = new ResolutionTemplate
        {
            Name = request.Name,
            ForDisputeType = request.ForDisputeType,
            ResolutionType = request.ResolutionType,
            TemplateContent = request.TemplateContent,
            CreatedBy = request.CreatedBy
        };

        await _repo.AddAsync(template, ct);
        return template.Id;
    }
}

public class CreateSlaConfigurationHandler : IRequestHandler<CreateSlaConfigurationCommand, Guid>
{
    private readonly IDisputeSlaConfigurationRepository _repo;

    public CreateSlaConfigurationHandler(IDisputeSlaConfigurationRepository repo) => _repo = repo;

    public async Task<Guid> Handle(CreateSlaConfigurationCommand request, CancellationToken ct)
    {
        var config = new DisputeSlaConfiguration
        {
            DisputeType = request.DisputeType,
            Priority = request.Priority,
            ResponseDeadlineHours = request.ResponseDeadlineHours,
            ResolutionDeadlineHours = request.ResolutionDeadlineHours,
            EscalationThresholdHours = request.EscalationThresholdHours
        };

        await _repo.AddAsync(config, ct);
        return config.Id;
    }
}

#endregion
