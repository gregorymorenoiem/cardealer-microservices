using Microsoft.EntityFrameworkCore;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for chat sessions (Fase 4 - Backend only)
/// </summary>
public class ChatSessionRepository : IChatSessionRepository
{
    private readonly SpyneDbContext _context;

    public ChatSessionRepository(SpyneDbContext context)
    {
        _context = context;
    }

    public async Task<ChatSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ChatSessions
            .AsNoTracking()
            .Include(x => x.Messages)
            .Include(x => x.LeadInfo)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<ChatSession?> GetBySpyneTokenAsync(string spyneSessionToken, CancellationToken cancellationToken = default)
    {
        return await _context.ChatSessions
            .AsNoTracking()
            .Include(x => x.Messages)
            .Include(x => x.LeadInfo)
            .FirstOrDefaultAsync(x => x.SpyneSessionToken == spyneSessionToken, cancellationToken);
    }

    public async Task<ChatSession?> GetActiveByVisitorAsync(string visitorFingerprint, CancellationToken cancellationToken = default)
    {
        return await _context.ChatSessions
            .AsNoTracking()
            .Include(x => x.Messages)
            .Where(x => x.VisitorFingerprint == visitorFingerprint && x.Status == ChatSessionStatus.Active)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<ChatSession>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _context.ChatSessions
            .AsNoTracking()
            .Include(x => x.LeadInfo)
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ChatSession>> GetByVehicleIdAsync(Guid vehicleId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.ChatSessions
            .AsNoTracking()
            .Include(x => x.LeadInfo)
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ChatSession>> GetByDealerIdAsync(Guid dealerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.ChatSessions
            .AsNoTracking()
            .Include(x => x.LeadInfo)
            .Where(x => x.DealerId == dealerId)
            .OrderByDescending(x => x.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ChatSession>> GetWithLeadsAsync(Guid dealerId, DateTime since, CancellationToken cancellationToken = default)
    {
        return await _context.ChatSessions
            .AsNoTracking()
            .Include(x => x.LeadInfo)
            .Where(x => x.DealerId == dealerId && 
                        x.IsQualifiedLead && 
                        x.StartedAt >= since)
            .OrderByDescending(x => x.LeadInfo!.LeadScore)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ChatSession>> GetExpiredSessionsAsync(TimeSpan inactivityThreshold, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow - inactivityThreshold;
        return await _context.ChatSessions
            .Where(x => x.Status == ChatSessionStatus.Active && x.LastActivityAt < cutoff)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatSession> AddAsync(ChatSession session, CancellationToken cancellationToken = default)
    {
        await _context.ChatSessions.AddAsync(session, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<ChatSession> UpdateAsync(ChatSession session, CancellationToken cancellationToken = default)
    {
        _context.ChatSessions.Update(session);
        await _context.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        await _context.ChatMessages.AddAsync(message, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.ChatMessages
            .AsNoTracking()
            .Where(x => x.ChatSessionId == sessionId)
            .OrderBy(x => x.Timestamp)
            .ToListAsync(cancellationToken);
    }
}
