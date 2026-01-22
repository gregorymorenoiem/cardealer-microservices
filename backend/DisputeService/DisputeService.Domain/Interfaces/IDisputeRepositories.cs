// DisputeService - Repository Interfaces

namespace DisputeService.Domain.Interfaces;

using DisputeService.Domain.Entities;

public interface IDisputeRepository
{
    Task<Dispute?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Dispute?> GetByCaseNumberAsync(string caseNumber, CancellationToken ct = default);
    Task<List<Dispute>> GetByComplainantAsync(Guid complainantId, CancellationToken ct = default);
    Task<List<Dispute>> GetByRespondentAsync(Guid respondentId, CancellationToken ct = default);
    Task<List<Dispute>> GetByStatusAsync(DisputeStatus status, CancellationToken ct = default);
    Task<List<Dispute>> GetByMediatorAsync(string mediatorId, CancellationToken ct = default);
    Task<List<Dispute>> GetPendingAsync(CancellationToken ct = default);
    Task<List<Dispute>> GetOverdueAsync(CancellationToken ct = default);
    Task<List<Dispute>> GetEscalatedAsync(CancellationToken ct = default);
    Task<List<Dispute>> GetProConsumidorReferralsAsync(CancellationToken ct = default);
    Task<(List<Dispute> Items, int Total)> GetPagedAsync(int page, int pageSize, DisputeStatus? status = null, DisputeType? type = null, CancellationToken ct = default);
    Task AddAsync(Dispute dispute, CancellationToken ct = default);
    Task UpdateAsync(Dispute dispute, CancellationToken ct = default);
    Task<string> GenerateCaseNumberAsync(CancellationToken ct = default);
}

public interface IDisputeEvidenceRepository
{
    Task<DisputeEvidence?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<DisputeEvidence>> GetByDisputeAsync(Guid disputeId, CancellationToken ct = default);
    Task<List<DisputeEvidence>> GetPendingReviewAsync(CancellationToken ct = default);
    Task AddAsync(DisputeEvidence evidence, CancellationToken ct = default);
    Task UpdateAsync(DisputeEvidence evidence, CancellationToken ct = default);
}

public interface IDisputeCommentRepository
{
    Task<DisputeComment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<DisputeComment>> GetByDisputeAsync(Guid disputeId, CancellationToken ct = default);
    Task AddAsync(DisputeComment comment, CancellationToken ct = default);
    Task UpdateAsync(DisputeComment comment, CancellationToken ct = default);
}

public interface IDisputeTimelineRepository
{
    Task<List<DisputeTimelineEvent>> GetByDisputeAsync(Guid disputeId, CancellationToken ct = default);
    Task AddAsync(DisputeTimelineEvent evt, CancellationToken ct = default);
}

public interface IMediationSessionRepository
{
    Task<MediationSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<MediationSession>> GetByDisputeAsync(Guid disputeId, CancellationToken ct = default);
    Task<List<MediationSession>> GetUpcomingAsync(int daysAhead, CancellationToken ct = default);
    Task AddAsync(MediationSession session, CancellationToken ct = default);
    Task UpdateAsync(MediationSession session, CancellationToken ct = default);
    Task<string> GenerateSessionNumberAsync(Guid disputeId, CancellationToken ct = default);
}

public interface IDisputeParticipantRepository
{
    Task<List<DisputeParticipant>> GetByDisputeAsync(Guid disputeId, CancellationToken ct = default);
    Task<DisputeParticipant?> GetByUserAndDisputeAsync(Guid userId, Guid disputeId, CancellationToken ct = default);
    Task AddAsync(DisputeParticipant participant, CancellationToken ct = default);
    Task UpdateAsync(DisputeParticipant participant, CancellationToken ct = default);
}

public interface IResolutionTemplateRepository
{
    Task<ResolutionTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ResolutionTemplate>> GetActiveAsync(CancellationToken ct = default);
    Task<List<ResolutionTemplate>> GetByTypeAsync(DisputeType type, CancellationToken ct = default);
    Task AddAsync(ResolutionTemplate template, CancellationToken ct = default);
}

public interface IDisputeSlaConfigurationRepository
{
    Task<DisputeSlaConfiguration?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DisputeSlaConfiguration?> GetByTypeAndPriorityAsync(DisputeType type, DisputePriority priority, CancellationToken ct = default);
    Task<List<DisputeSlaConfiguration>> GetActiveAsync(CancellationToken ct = default);
    Task AddAsync(DisputeSlaConfiguration config, CancellationToken ct = default);
}
