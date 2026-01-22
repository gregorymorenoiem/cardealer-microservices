// DisputeService - Query Handlers
// Pro-Consumidor RD + Ley 126-02

namespace DisputeService.Application.Handlers;

using MediatR;
using DisputeService.Application.DTOs;
using DisputeService.Application.Queries;
using DisputeService.Domain.Entities;
using DisputeService.Domain.Interfaces;

#region Dispute Query Handlers

public class GetDisputeByIdHandler : IRequestHandler<GetDisputeByIdQuery, DisputeDto?>
{
    private readonly IDisputeRepository _repo;
    public GetDisputeByIdHandler(IDisputeRepository repo) => _repo = repo;

    public async Task<DisputeDto?> Handle(GetDisputeByIdQuery request, CancellationToken ct)
    {
        var d = await _repo.GetByIdAsync(request.Id, ct);
        if (d == null) return null;
        return MapToDto(d);
    }

    private static DisputeDto MapToDto(Dispute d) => new(
        d.Id, d.CaseNumber, d.Type, d.Status, d.Priority,
        d.ComplainantId, d.ComplainantName, d.ComplainantEmail,
        d.RespondentId, d.RespondentName, d.RespondentEmail,
        d.Title, d.Description, d.DisputedAmount, d.Currency,
        d.AssignedMediatorId, d.AssignedMediatorName, d.AssignedAt,
        d.Resolution, d.ResolutionSummary, d.ResolvedAt,
        d.IsEscalated, d.ReferredToProConsumidor, d.ProConsumidorCaseNumber,
        d.FiledAt, d.ResponseDueDate, d.ResolutionDueDate, d.CreatedAt);
}

public class GetDisputeByCaseNumberHandler : IRequestHandler<GetDisputeByCaseNumberQuery, DisputeDto?>
{
    private readonly IDisputeRepository _repo;
    public GetDisputeByCaseNumberHandler(IDisputeRepository repo) => _repo = repo;

    public async Task<DisputeDto?> Handle(GetDisputeByCaseNumberQuery request, CancellationToken ct)
    {
        var d = await _repo.GetByCaseNumberAsync(request.CaseNumber, ct);
        if (d == null) return null;
        return new DisputeDto(
            d.Id, d.CaseNumber, d.Type, d.Status, d.Priority,
            d.ComplainantId, d.ComplainantName, d.ComplainantEmail,
            d.RespondentId, d.RespondentName, d.RespondentEmail,
            d.Title, d.Description, d.DisputedAmount, d.Currency,
            d.AssignedMediatorId, d.AssignedMediatorName, d.AssignedAt,
            d.Resolution, d.ResolutionSummary, d.ResolvedAt,
            d.IsEscalated, d.ReferredToProConsumidor, d.ProConsumidorCaseNumber,
            d.FiledAt, d.ResponseDueDate, d.ResolutionDueDate, d.CreatedAt);
    }
}

public class GetDisputesByComplainantHandler : IRequestHandler<GetDisputesByComplainantQuery, List<DisputeSummaryDto>>
{
    private readonly IDisputeRepository _repo;
    public GetDisputesByComplainantHandler(IDisputeRepository repo) => _repo = repo;

    public async Task<List<DisputeSummaryDto>> Handle(GetDisputesByComplainantQuery request, CancellationToken ct)
    {
        var list = await _repo.GetByComplainantAsync(request.ComplainantId, ct);
        return list.Select(d => new DisputeSummaryDto(
            d.Id, d.CaseNumber, d.Type, d.Status, d.Priority,
            d.ComplainantName, d.RespondentName, d.Title,
            d.DisputedAmount, d.FiledAt, d.IsEscalated)).ToList();
    }
}

public class GetDisputesByRespondentHandler : IRequestHandler<GetDisputesByRespondentQuery, List<DisputeSummaryDto>>
{
    private readonly IDisputeRepository _repo;
    public GetDisputesByRespondentHandler(IDisputeRepository repo) => _repo = repo;

    public async Task<List<DisputeSummaryDto>> Handle(GetDisputesByRespondentQuery request, CancellationToken ct)
    {
        var list = await _repo.GetByRespondentAsync(request.RespondentId, ct);
        return list.Select(d => new DisputeSummaryDto(
            d.Id, d.CaseNumber, d.Type, d.Status, d.Priority,
            d.ComplainantName, d.RespondentName, d.Title,
            d.DisputedAmount, d.FiledAt, d.IsEscalated)).ToList();
    }
}

public class GetDisputesByMediatorHandler : IRequestHandler<GetDisputesByMediatorQuery, List<DisputeSummaryDto>>
{
    private readonly IDisputeRepository _repo;
    public GetDisputesByMediatorHandler(IDisputeRepository repo) => _repo = repo;

    public async Task<List<DisputeSummaryDto>> Handle(GetDisputesByMediatorQuery request, CancellationToken ct)
    {
        var list = await _repo.GetByMediatorAsync(request.MediatorId, ct);
        return list.Select(d => new DisputeSummaryDto(
            d.Id, d.CaseNumber, d.Type, d.Status, d.Priority,
            d.ComplainantName, d.RespondentName, d.Title,
            d.DisputedAmount, d.FiledAt, d.IsEscalated)).ToList();
    }
}

public class GetDisputesPagedHandler : IRequestHandler<GetDisputesPagedQuery, DisputePagedResultDto>
{
    private readonly IDisputeRepository _repo;
    public GetDisputesPagedHandler(IDisputeRepository repo) => _repo = repo;

    public async Task<DisputePagedResultDto> Handle(GetDisputesPagedQuery request, CancellationToken ct)
    {
        var (items, total) = await _repo.GetPagedAsync(request.Page, request.PageSize, request.Status, request.Type, ct);
        var dtos = items.Select(d => new DisputeSummaryDto(
            d.Id, d.CaseNumber, d.Type, d.Status, d.Priority,
            d.ComplainantName, d.RespondentName, d.Title,
            d.DisputedAmount, d.FiledAt, d.IsEscalated)).ToList();
        return new DisputePagedResultDto(dtos, total, request.Page, request.PageSize);
    }
}

public class GetPendingDisputesHandler : IRequestHandler<GetPendingDisputesQuery, List<DisputeSummaryDto>>
{
    private readonly IDisputeRepository _repo;
    public GetPendingDisputesHandler(IDisputeRepository repo) => _repo = repo;

    public async Task<List<DisputeSummaryDto>> Handle(GetPendingDisputesQuery request, CancellationToken ct)
    {
        var list = await _repo.GetPendingAsync(ct);
        return list.Select(d => new DisputeSummaryDto(
            d.Id, d.CaseNumber, d.Type, d.Status, d.Priority,
            d.ComplainantName, d.RespondentName, d.Title,
            d.DisputedAmount, d.FiledAt, d.IsEscalated)).ToList();
    }
}

public class GetOverdueDisputesHandler : IRequestHandler<GetOverdueDisputesQuery, List<DisputeSummaryDto>>
{
    private readonly IDisputeRepository _repo;
    public GetOverdueDisputesHandler(IDisputeRepository repo) => _repo = repo;

    public async Task<List<DisputeSummaryDto>> Handle(GetOverdueDisputesQuery request, CancellationToken ct)
    {
        var list = await _repo.GetOverdueAsync(ct);
        return list.Select(d => new DisputeSummaryDto(
            d.Id, d.CaseNumber, d.Type, d.Status, d.Priority,
            d.ComplainantName, d.RespondentName, d.Title,
            d.DisputedAmount, d.FiledAt, d.IsEscalated)).ToList();
    }
}

public class GetEscalatedDisputesHandler : IRequestHandler<GetEscalatedDisputesQuery, List<DisputeSummaryDto>>
{
    private readonly IDisputeRepository _repo;
    public GetEscalatedDisputesHandler(IDisputeRepository repo) => _repo = repo;

    public async Task<List<DisputeSummaryDto>> Handle(GetEscalatedDisputesQuery request, CancellationToken ct)
    {
        var list = await _repo.GetEscalatedAsync(ct);
        return list.Select(d => new DisputeSummaryDto(
            d.Id, d.CaseNumber, d.Type, d.Status, d.Priority,
            d.ComplainantName, d.RespondentName, d.Title,
            d.DisputedAmount, d.FiledAt, d.IsEscalated)).ToList();
    }
}

public class GetProConsumidorReferralsHandler : IRequestHandler<GetProConsumidorReferralsQuery, List<DisputeSummaryDto>>
{
    private readonly IDisputeRepository _repo;
    public GetProConsumidorReferralsHandler(IDisputeRepository repo) => _repo = repo;

    public async Task<List<DisputeSummaryDto>> Handle(GetProConsumidorReferralsQuery request, CancellationToken ct)
    {
        var list = await _repo.GetProConsumidorReferralsAsync(ct);
        return list.Select(d => new DisputeSummaryDto(
            d.Id, d.CaseNumber, d.Type, d.Status, d.Priority,
            d.ComplainantName, d.RespondentName, d.Title,
            d.DisputedAmount, d.FiledAt, d.IsEscalated)).ToList();
    }
}

#endregion

#region Evidence Query Handlers

public class GetEvidenceByIdHandler : IRequestHandler<GetEvidenceByIdQuery, DisputeEvidenceDto?>
{
    private readonly IDisputeEvidenceRepository _repo;
    public GetEvidenceByIdHandler(IDisputeEvidenceRepository repo) => _repo = repo;

    public async Task<DisputeEvidenceDto?> Handle(GetEvidenceByIdQuery request, CancellationToken ct)
    {
        var e = await _repo.GetByIdAsync(request.Id, ct);
        if (e == null) return null;
        return new DisputeEvidenceDto(
            e.Id, e.DisputeId, e.Name, e.Description, e.EvidenceType,
            e.FileName, e.ContentType, e.FileSize, e.StoragePath,
            e.SubmittedById, e.SubmittedByName, e.SubmitterRole,
            e.Status, e.SubmittedAt, e.ReviewedAt, e.ReviewNotes);
    }
}

public class GetEvidenceByDisputeHandler : IRequestHandler<GetEvidenceByDisputeQuery, List<DisputeEvidenceDto>>
{
    private readonly IDisputeEvidenceRepository _repo;
    public GetEvidenceByDisputeHandler(IDisputeEvidenceRepository repo) => _repo = repo;

    public async Task<List<DisputeEvidenceDto>> Handle(GetEvidenceByDisputeQuery request, CancellationToken ct)
    {
        var list = await _repo.GetByDisputeAsync(request.DisputeId, ct);
        return list.Select(e => new DisputeEvidenceDto(
            e.Id, e.DisputeId, e.Name, e.Description, e.EvidenceType,
            e.FileName, e.ContentType, e.FileSize, e.StoragePath,
            e.SubmittedById, e.SubmittedByName, e.SubmitterRole,
            e.Status, e.SubmittedAt, e.ReviewedAt, e.ReviewNotes)).ToList();
    }
}

public class GetPendingEvidenceReviewHandler : IRequestHandler<GetPendingEvidenceReviewQuery, List<DisputeEvidenceDto>>
{
    private readonly IDisputeEvidenceRepository _repo;
    public GetPendingEvidenceReviewHandler(IDisputeEvidenceRepository repo) => _repo = repo;

    public async Task<List<DisputeEvidenceDto>> Handle(GetPendingEvidenceReviewQuery request, CancellationToken ct)
    {
        var list = await _repo.GetPendingReviewAsync(ct);
        return list.Select(e => new DisputeEvidenceDto(
            e.Id, e.DisputeId, e.Name, e.Description, e.EvidenceType,
            e.FileName, e.ContentType, e.FileSize, e.StoragePath,
            e.SubmittedById, e.SubmittedByName, e.SubmitterRole,
            e.Status, e.SubmittedAt, e.ReviewedAt, e.ReviewNotes)).ToList();
    }
}

#endregion

#region Comment Query Handlers

public class GetCommentsByDisputeHandler : IRequestHandler<GetCommentsByDisputeQuery, List<DisputeCommentDto>>
{
    private readonly IDisputeCommentRepository _repo;
    public GetCommentsByDisputeHandler(IDisputeCommentRepository repo) => _repo = repo;

    public async Task<List<DisputeCommentDto>> Handle(GetCommentsByDisputeQuery request, CancellationToken ct)
    {
        var list = await _repo.GetByDisputeAsync(request.DisputeId, ct);
        return list.Select(c => new DisputeCommentDto(
            c.Id, c.DisputeId, c.ParentCommentId, c.AuthorId,
            c.AuthorName, c.AuthorRole, c.Content, c.IsInternal,
            c.IsOfficial, c.CreatedAt, c.EditedAt)).ToList();
    }
}

#endregion

#region Timeline Query Handlers

public class GetTimelineByDisputeHandler : IRequestHandler<GetTimelineByDisputeQuery, List<DisputeTimelineEventDto>>
{
    private readonly IDisputeTimelineRepository _repo;
    public GetTimelineByDisputeHandler(IDisputeTimelineRepository repo) => _repo = repo;

    public async Task<List<DisputeTimelineEventDto>> Handle(GetTimelineByDisputeQuery request, CancellationToken ct)
    {
        var list = await _repo.GetByDisputeAsync(request.DisputeId, ct);
        return list.Select(e => new DisputeTimelineEventDto(
            e.Id, e.DisputeId, e.EventType, e.Description,
            e.OldValue, e.NewValue, e.PerformedBy, e.PerformerRole,
            e.OccurredAt)).ToList();
    }
}

#endregion

#region Mediation Query Handlers

public class GetMediationSessionByIdHandler : IRequestHandler<GetMediationSessionByIdQuery, MediationSessionDto?>
{
    private readonly IMediationSessionRepository _repo;
    public GetMediationSessionByIdHandler(IMediationSessionRepository repo) => _repo = repo;

    public async Task<MediationSessionDto?> Handle(GetMediationSessionByIdQuery request, CancellationToken ct)
    {
        var s = await _repo.GetByIdAsync(request.Id, ct);
        if (s == null) return null;
        return new MediationSessionDto(
            s.Id, s.DisputeId, s.SessionNumber, s.ScheduledAt, s.DurationMinutes,
            s.Channel, s.MeetingLink, s.Location, s.MediatorId, s.MediatorName,
            s.Status, s.StartedAt, s.EndedAt, s.Summary,
            s.ComplainantAttended, s.RespondentAttended, s.PartiesAgreed);
    }
}

public class GetMediationsByDisputeHandler : IRequestHandler<GetMediationsByDisputeQuery, List<MediationSessionDto>>
{
    private readonly IMediationSessionRepository _repo;
    public GetMediationsByDisputeHandler(IMediationSessionRepository repo) => _repo = repo;

    public async Task<List<MediationSessionDto>> Handle(GetMediationsByDisputeQuery request, CancellationToken ct)
    {
        var list = await _repo.GetByDisputeAsync(request.DisputeId, ct);
        return list.Select(s => new MediationSessionDto(
            s.Id, s.DisputeId, s.SessionNumber, s.ScheduledAt, s.DurationMinutes,
            s.Channel, s.MeetingLink, s.Location, s.MediatorId, s.MediatorName,
            s.Status, s.StartedAt, s.EndedAt, s.Summary,
            s.ComplainantAttended, s.RespondentAttended, s.PartiesAgreed)).ToList();
    }
}

public class GetUpcomingMediationsHandler : IRequestHandler<GetUpcomingMediationsQuery, List<MediationSessionDto>>
{
    private readonly IMediationSessionRepository _repo;
    public GetUpcomingMediationsHandler(IMediationSessionRepository repo) => _repo = repo;

    public async Task<List<MediationSessionDto>> Handle(GetUpcomingMediationsQuery request, CancellationToken ct)
    {
        var list = await _repo.GetUpcomingAsync(request.DaysAhead, ct);
        return list.Select(s => new MediationSessionDto(
            s.Id, s.DisputeId, s.SessionNumber, s.ScheduledAt, s.DurationMinutes,
            s.Channel, s.MeetingLink, s.Location, s.MediatorId, s.MediatorName,
            s.Status, s.StartedAt, s.EndedAt, s.Summary,
            s.ComplainantAttended, s.RespondentAttended, s.PartiesAgreed)).ToList();
    }
}

#endregion

#region Participant Query Handlers

public class GetParticipantsByDisputeHandler : IRequestHandler<GetParticipantsByDisputeQuery, List<DisputeParticipantDto>>
{
    private readonly IDisputeParticipantRepository _repo;
    public GetParticipantsByDisputeHandler(IDisputeParticipantRepository repo) => _repo = repo;

    public async Task<List<DisputeParticipantDto>> Handle(GetParticipantsByDisputeQuery request, CancellationToken ct)
    {
        var list = await _repo.GetByDisputeAsync(request.DisputeId, ct);
        return list.Select(p => new DisputeParticipantDto(
            p.Id, p.DisputeId, p.UserId, p.UserName, p.UserEmail,
            p.Role, p.IsActive, p.JoinedAt, p.PreferredChannel)).ToList();
    }
}

#endregion

#region Resolution Template Query Handlers

public class GetResolutionTemplatesHandler : IRequestHandler<GetResolutionTemplatesQuery, List<ResolutionTemplateDto>>
{
    private readonly IResolutionTemplateRepository _repo;
    public GetResolutionTemplatesHandler(IResolutionTemplateRepository repo) => _repo = repo;

    public async Task<List<ResolutionTemplateDto>> Handle(GetResolutionTemplatesQuery request, CancellationToken ct)
    {
        var list = request.Type.HasValue
            ? await _repo.GetByTypeAsync(request.Type.Value, ct)
            : await _repo.GetActiveAsync(ct);
        
        return list.Select(t => new ResolutionTemplateDto(
            t.Id, t.Name, t.ForDisputeType, t.ResolutionType,
            t.TemplateContent, t.IsActive)).ToList();
    }
}

#endregion

#region SLA Query Handlers

public class GetSlaConfigurationsHandler : IRequestHandler<GetSlaConfigurationsQuery, List<DisputeSlaConfigurationDto>>
{
    private readonly IDisputeSlaConfigurationRepository _repo;
    public GetSlaConfigurationsHandler(IDisputeSlaConfigurationRepository repo) => _repo = repo;

    public async Task<List<DisputeSlaConfigurationDto>> Handle(GetSlaConfigurationsQuery request, CancellationToken ct)
    {
        var list = await _repo.GetActiveAsync(ct);
        return list.Select(c => new DisputeSlaConfigurationDto(
            c.Id, c.DisputeType, c.Priority, c.ResponseDeadlineHours,
            c.ResolutionDeadlineHours, c.EscalationThresholdHours, c.IsActive)).ToList();
    }
}

#endregion

#region Statistics Query Handlers

public class GetDisputeStatisticsHandler : IRequestHandler<GetDisputeStatisticsQuery, DisputeStatisticsDto>
{
    private readonly IDisputeRepository _repo;
    public GetDisputeStatisticsHandler(IDisputeRepository repo) => _repo = repo;

    public async Task<DisputeStatisticsDto> Handle(GetDisputeStatisticsQuery request, CancellationToken ct)
    {
        var (items, total) = await _repo.GetPagedAsync(1, 10000, null, null, ct);
        var filtered = items.Where(d => d.CreatedAt >= request.FromDate && d.CreatedAt <= request.ToDate).ToList();
        
        var byStatus = filtered.GroupBy(d => d.Status).ToDictionary(g => g.Key, g => g.Count());
        var byType = filtered.GroupBy(d => d.Type).ToDictionary(g => g.Key, g => g.Count());
        
        var resolved = filtered.Where(d => d.ResolvedAt.HasValue).ToList();
        var avgDays = resolved.Any()
            ? resolved.Average(d => (d.ResolvedAt!.Value - d.FiledAt).TotalDays)
            : 0;

        return new DisputeStatisticsDto(
            filtered.Count,
            filtered.Count(d => d.Status != DisputeStatus.Closed && d.Status != DisputeStatus.Resolved),
            filtered.Count(d => d.Status == DisputeStatus.Resolved),
            filtered.Count(d => d.IsEscalated),
            filtered.Count(d => d.ReferredToProConsumidor),
            (decimal)avgDays,
            byStatus,
            byType);
    }
}

#endregion
