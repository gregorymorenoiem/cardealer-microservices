// DisputeService - Repository Implementations
// Aligned with Domain Interfaces

namespace DisputeService.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using DisputeService.Domain.Entities;
using DisputeService.Domain.Interfaces;

#region Dispute Repository

public class DisputeRepository : IDisputeRepository
{
    private readonly DisputeDbContext _context;

    public DisputeRepository(DisputeDbContext context) => _context = context;

    public async Task<Dispute?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Disputes.FindAsync(new object[] { id }, ct);

    public async Task<Dispute?> GetByCaseNumberAsync(string caseNumber, CancellationToken ct = default)
        => await _context.Disputes.FirstOrDefaultAsync(d => d.CaseNumber == caseNumber, ct);

    public async Task<List<Dispute>> GetByComplainantAsync(Guid complainantId, CancellationToken ct = default)
        => await _context.Disputes.Where(d => d.ComplainantId == complainantId).OrderByDescending(d => d.FiledAt).ToListAsync(ct);

    public async Task<List<Dispute>> GetByRespondentAsync(Guid respondentId, CancellationToken ct = default)
        => await _context.Disputes.Where(d => d.RespondentId == respondentId).OrderByDescending(d => d.FiledAt).ToListAsync(ct);

    public async Task<List<Dispute>> GetByStatusAsync(DisputeStatus status, CancellationToken ct = default)
        => await _context.Disputes.Where(d => d.Status == status).OrderByDescending(d => d.FiledAt).ToListAsync(ct);

    public async Task<List<Dispute>> GetByMediatorAsync(string mediatorId, CancellationToken ct = default)
        => await _context.Disputes.Where(d => d.AssignedMediatorId == mediatorId).OrderByDescending(d => d.FiledAt).ToListAsync(ct);

    public async Task<List<Dispute>> GetPendingAsync(CancellationToken ct = default)
        => await _context.Disputes
            .Where(d => d.Status == DisputeStatus.Filed || d.Status == DisputeStatus.Acknowledged || d.Status == DisputeStatus.InReview)
            .OrderBy(d => d.FiledAt)
            .ToListAsync(ct);

    public async Task<List<Dispute>> GetOverdueAsync(CancellationToken ct = default)
        => await _context.Disputes
            .Where(d => d.ResolutionDueDate < DateTime.UtcNow && 
                       d.Status != DisputeStatus.Resolved && 
                       d.Status != DisputeStatus.Closed)
            .OrderBy(d => d.ResolutionDueDate)
            .ToListAsync(ct);

    public async Task<List<Dispute>> GetEscalatedAsync(CancellationToken ct = default)
        => await _context.Disputes
            .Where(d => d.IsEscalated)
            .OrderByDescending(d => d.EscalatedAt)
            .ToListAsync(ct);

    public async Task<List<Dispute>> GetProConsumidorReferralsAsync(CancellationToken ct = default)
        => await _context.Disputes
            .Where(d => d.ReferredToProConsumidor)
            .OrderByDescending(d => d.ProConsumidorReferralDate)
            .ToListAsync(ct);

    public async Task<(List<Dispute> Items, int Total)> GetPagedAsync(
        int page, int pageSize, DisputeStatus? status = null, DisputeType? type = null, CancellationToken ct = default)
    {
        var query = _context.Disputes.AsQueryable();

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        if (type.HasValue)
            query = query.Where(d => d.Type == type.Value);

        var total = await query.CountAsync(ct);
        var disputes = await query
            .OrderByDescending(d => d.FiledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (disputes, total);
    }

    public async Task<string> GenerateCaseNumberAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Disputes.CountAsync(d => d.CaseNumber.StartsWith($"DISP-{year}"), ct);
        return $"DISP-{year}-{(count + 1):D6}";
    }

    public async Task AddAsync(Dispute dispute, CancellationToken ct = default)
    {
        await _context.Disputes.AddAsync(dispute, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Dispute dispute, CancellationToken ct = default)
    {
        _context.Disputes.Update(dispute);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Evidence Repository

public class DisputeEvidenceRepository : IDisputeEvidenceRepository
{
    private readonly DisputeDbContext _context;

    public DisputeEvidenceRepository(DisputeDbContext context) => _context = context;

    public async Task<DisputeEvidence?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.DisputeEvidences.FindAsync(new object[] { id }, ct);

    public async Task<List<DisputeEvidence>> GetByDisputeAsync(Guid disputeId, CancellationToken ct = default)
        => await _context.DisputeEvidences.Where(e => e.DisputeId == disputeId).OrderBy(e => e.SubmittedAt).ToListAsync(ct);

    public async Task<List<DisputeEvidence>> GetPendingReviewAsync(CancellationToken ct = default)
        => await _context.DisputeEvidences.Where(e => e.Status == EvidenceStatus.Submitted).OrderBy(e => e.SubmittedAt).ToListAsync(ct);

    public async Task AddAsync(DisputeEvidence evidence, CancellationToken ct = default)
    {
        await _context.DisputeEvidences.AddAsync(evidence, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DisputeEvidence evidence, CancellationToken ct = default)
    {
        _context.DisputeEvidences.Update(evidence);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Comment Repository

public class DisputeCommentRepository : IDisputeCommentRepository
{
    private readonly DisputeDbContext _context;

    public DisputeCommentRepository(DisputeDbContext context) => _context = context;

    public async Task<DisputeComment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.DisputeComments.FindAsync(new object[] { id }, ct);

    public async Task<List<DisputeComment>> GetByDisputeAsync(Guid disputeId, CancellationToken ct = default)
        => await _context.DisputeComments
            .Where(c => c.DisputeId == disputeId && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(DisputeComment comment, CancellationToken ct = default)
    {
        await _context.DisputeComments.AddAsync(comment, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DisputeComment comment, CancellationToken ct = default)
    {
        _context.DisputeComments.Update(comment);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Timeline Repository

public class DisputeTimelineRepository : IDisputeTimelineRepository
{
    private readonly DisputeDbContext _context;

    public DisputeTimelineRepository(DisputeDbContext context) => _context = context;

    public async Task<List<DisputeTimelineEvent>> GetByDisputeAsync(Guid disputeId, CancellationToken ct = default)
        => await _context.DisputeTimelineEvents.Where(e => e.DisputeId == disputeId).OrderBy(e => e.OccurredAt).ToListAsync(ct);

    public async Task AddAsync(DisputeTimelineEvent evt, CancellationToken ct = default)
    {
        await _context.DisputeTimelineEvents.AddAsync(evt, ct);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Mediation Session Repository

public class MediationSessionRepository : IMediationSessionRepository
{
    private readonly DisputeDbContext _context;

    public MediationSessionRepository(DisputeDbContext context) => _context = context;

    public async Task<MediationSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.MediationSessions.FindAsync(new object[] { id }, ct);

    public async Task<List<MediationSession>> GetByDisputeAsync(Guid disputeId, CancellationToken ct = default)
        => await _context.MediationSessions.Where(s => s.DisputeId == disputeId).OrderBy(s => s.ScheduledAt).ToListAsync(ct);

    public async Task<List<MediationSession>> GetUpcomingAsync(int daysAhead, CancellationToken ct = default)
    {
        var endDate = DateTime.UtcNow.AddDays(daysAhead);
        return await _context.MediationSessions
            .Where(s => s.ScheduledAt > DateTime.UtcNow && s.ScheduledAt <= endDate && s.Status == "Scheduled")
            .OrderBy(s => s.ScheduledAt)
            .ToListAsync(ct);
    }

    public async Task<string> GenerateSessionNumberAsync(Guid disputeId, CancellationToken ct = default)
    {
        var count = await _context.MediationSessions.CountAsync(s => s.DisputeId == disputeId, ct);
        return $"SES-{count + 1:D3}";
    }

    public async Task AddAsync(MediationSession session, CancellationToken ct = default)
    {
        await _context.MediationSessions.AddAsync(session, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(MediationSession session, CancellationToken ct = default)
    {
        _context.MediationSessions.Update(session);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Participant Repository

public class DisputeParticipantRepository : IDisputeParticipantRepository
{
    private readonly DisputeDbContext _context;

    public DisputeParticipantRepository(DisputeDbContext context) => _context = context;

    public async Task<List<DisputeParticipant>> GetByDisputeAsync(Guid disputeId, CancellationToken ct = default)
        => await _context.DisputeParticipants.Where(p => p.DisputeId == disputeId).OrderBy(p => p.JoinedAt).ToListAsync(ct);

    public async Task<DisputeParticipant?> GetByUserAndDisputeAsync(Guid userId, Guid disputeId, CancellationToken ct = default)
        => await _context.DisputeParticipants.FirstOrDefaultAsync(p => p.UserId == userId && p.DisputeId == disputeId, ct);

    public async Task AddAsync(DisputeParticipant participant, CancellationToken ct = default)
    {
        await _context.DisputeParticipants.AddAsync(participant, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DisputeParticipant participant, CancellationToken ct = default)
    {
        _context.DisputeParticipants.Update(participant);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region Resolution Template Repository

public class ResolutionTemplateRepository : IResolutionTemplateRepository
{
    private readonly DisputeDbContext _context;

    public ResolutionTemplateRepository(DisputeDbContext context) => _context = context;

    public async Task<ResolutionTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.ResolutionTemplates.FindAsync(new object[] { id }, ct);

    public async Task<List<ResolutionTemplate>> GetActiveAsync(CancellationToken ct = default)
        => await _context.ResolutionTemplates.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync(ct);

    public async Task<List<ResolutionTemplate>> GetByTypeAsync(DisputeType type, CancellationToken ct = default)
        => await _context.ResolutionTemplates.Where(t => t.ForDisputeType == type && t.IsActive).OrderBy(t => t.Name).ToListAsync(ct);

    public async Task AddAsync(ResolutionTemplate template, CancellationToken ct = default)
    {
        await _context.ResolutionTemplates.AddAsync(template, ct);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion

#region SLA Configuration Repository

public class DisputeSlaConfigurationRepository : IDisputeSlaConfigurationRepository
{
    private readonly DisputeDbContext _context;

    public DisputeSlaConfigurationRepository(DisputeDbContext context) => _context = context;

    public async Task<DisputeSlaConfiguration?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.DisputeSlaConfigurations.FindAsync(new object[] { id }, ct);

    public async Task<DisputeSlaConfiguration?> GetByTypeAndPriorityAsync(DisputeType type, DisputePriority priority, CancellationToken ct = default)
        => await _context.DisputeSlaConfigurations.FirstOrDefaultAsync(c => c.DisputeType == type && c.Priority == priority && c.IsActive, ct);

    public async Task<List<DisputeSlaConfiguration>> GetActiveAsync(CancellationToken ct = default)
        => await _context.DisputeSlaConfigurations.Where(c => c.IsActive).OrderBy(c => c.DisputeType).ThenBy(c => c.Priority).ToListAsync(ct);

    public async Task AddAsync(DisputeSlaConfiguration config, CancellationToken ct = default)
    {
        await _context.DisputeSlaConfigurations.AddAsync(config, ct);
        await _context.SaveChangesAsync(ct);
    }
}

#endregion
