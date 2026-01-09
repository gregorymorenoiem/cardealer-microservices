using Microsoft.EntityFrameworkCore;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Interfaces;

namespace ChatbotService.Infrastructure.Persistence.Repositories;

public class ChatConversationRepository : IChatConversationRepository
{
    private readonly ChatbotDbContext _context;

    public ChatConversationRepository(ChatbotDbContext context)
    {
        _context = context;
    }

    public async Task<ChatConversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<ChatConversation?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<ChatConversation?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.Status == ConversationStatus.Active, cancellationToken);
    }

    public async Task<ChatConversation?> GetActiveConversationAsync(Guid? userId, string? sessionId, Guid? vehicleId, CancellationToken cancellationToken = default)
    {
        var query = _context.Conversations
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .Where(c => c.Status == ConversationStatus.Active);

        if (userId.HasValue)
        {
            query = query.Where(c => c.UserId == userId.Value);
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            query = query.Where(c => c.SessionId == sessionId);
        }

        if (vehicleId.HasValue)
        {
            query = query.Where(c => c.VehicleId == vehicleId.Value);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<ChatConversation>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ChatConversation>> GetByVehicleIdAsync(Guid vehicleId, int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Where(c => c.VehicleId == vehicleId)
            .OrderByDescending(c => c.UpdatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ChatConversation>> GetRecentConversationsAsync(int hours = 24, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddHours(-hours);
        return await _context.Conversations
            .Where(c => c.CreatedAt >= cutoff)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetConversationCountAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Conversations.AsQueryable();

        if (from.HasValue)
            query = query.Where(c => c.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(c => c.CreatedAt <= to.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<ChatConversation> CreateAsync(ChatConversation conversation, CancellationToken cancellationToken = default)
    {
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(cancellationToken);
        return conversation;
    }

    public async Task<ChatConversation> UpdateAsync(ChatConversation conversation, CancellationToken cancellationToken = default)
    {
        conversation.UpdatedAt = DateTime.UtcNow;
        _context.Conversations.Update(conversation);
        await _context.SaveChangesAsync(cancellationToken);
        return conversation;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var conversation = await GetByIdAsync(id, cancellationToken);
        if (conversation != null)
        {
            _context.Conversations.Remove(conversation);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
