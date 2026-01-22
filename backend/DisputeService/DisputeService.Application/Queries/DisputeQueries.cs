// DisputeService - Queries
// Pro-Consumidor RD + Ley 126-02

namespace DisputeService.Application.Queries;

using DisputeService.Application.DTOs;
using DisputeService.Domain.Entities;
using MediatR;

#region Dispute Queries

public record GetDisputeByIdQuery(Guid Id) : IRequest<DisputeDto?>;

public record GetDisputeByCaseNumberQuery(string CaseNumber) : IRequest<DisputeDto?>;

public record GetDisputesByComplainantQuery(Guid ComplainantId) : IRequest<List<DisputeSummaryDto>>;

public record GetDisputesByRespondentQuery(Guid RespondentId) : IRequest<List<DisputeSummaryDto>>;

public record GetDisputesByMediatorQuery(string MediatorId) : IRequest<List<DisputeSummaryDto>>;

public record GetDisputesPagedQuery(
    int Page,
    int PageSize,
    DisputeStatus? Status,
    DisputeType? Type) : IRequest<DisputePagedResultDto>;

public record GetPendingDisputesQuery() : IRequest<List<DisputeSummaryDto>>;

public record GetOverdueDisputesQuery() : IRequest<List<DisputeSummaryDto>>;

public record GetEscalatedDisputesQuery() : IRequest<List<DisputeSummaryDto>>;

public record GetProConsumidorReferralsQuery() : IRequest<List<DisputeSummaryDto>>;

#endregion

#region Evidence Queries

public record GetEvidenceByIdQuery(Guid Id) : IRequest<DisputeEvidenceDto?>;

public record GetEvidenceByDisputeQuery(Guid DisputeId) : IRequest<List<DisputeEvidenceDto>>;

public record GetPendingEvidenceReviewQuery() : IRequest<List<DisputeEvidenceDto>>;

#endregion

#region Comment Queries

public record GetCommentsByDisputeQuery(Guid DisputeId) : IRequest<List<DisputeCommentDto>>;

#endregion

#region Timeline Queries

public record GetTimelineByDisputeQuery(Guid DisputeId) : IRequest<List<DisputeTimelineEventDto>>;

#endregion

#region Mediation Queries

public record GetMediationSessionByIdQuery(Guid Id) : IRequest<MediationSessionDto?>;

public record GetMediationsByDisputeQuery(Guid DisputeId) : IRequest<List<MediationSessionDto>>;

public record GetUpcomingMediationsQuery(int DaysAhead) : IRequest<List<MediationSessionDto>>;

#endregion

#region Participant Queries

public record GetParticipantsByDisputeQuery(Guid DisputeId) : IRequest<List<DisputeParticipantDto>>;

#endregion

#region Resolution Template Queries

public record GetResolutionTemplatesQuery(DisputeType? Type) : IRequest<List<ResolutionTemplateDto>>;

#endregion

#region SLA Queries

public record GetSlaConfigurationsQuery() : IRequest<List<DisputeSlaConfigurationDto>>;

#endregion

#region Statistics Queries

public record GetDisputeStatisticsQuery(DateTime FromDate, DateTime ToDate) : IRequest<DisputeStatisticsDto>;

#endregion
