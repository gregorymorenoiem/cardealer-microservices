using Microsoft.EntityFrameworkCore;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Infrastructure.Persistence;

namespace ChatbotService.Infrastructure.Persistence.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly ChatbotDbContext _context;

    public ConversationRepository(ChatbotDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Conversation?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Include(c => c.Messages)
            .Where(c => c.UserId == userId && c.Status == ConversationStatus.Active)
            .OrderByDescending(c => c.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Conversation>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Conversation>> GetByDealerIdAsync(Guid dealerId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Where(c => c.DealerId == dealerId)
            .OrderByDescending(c => c.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Conversation>> GetHotLeadsAsync(int minScore = 85, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Where(c => c.LeadScore >= minScore && c.LeadTemperature == LeadTemperature.Hot)
            .OrderByDescending(c => c.LeadScore)
            .ThenByDescending(c => c.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Conversation> CreateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(cancellationToken);
        return conversation;
    }

    public async Task<Conversation> UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default)
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

    public async Task<Message> AddMessageAsync(Guid conversationId, Message message, CancellationToken cancellationToken = default)
    {
        message.ConversationId = conversationId;
        _context.Messages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);
        return message;
    }

    public async Task<List<Message>> GetMessagesAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<string, object>> GetStatisticsAsync(
        Guid? dealerId = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Conversations.AsQueryable();

        if (dealerId.HasValue)
            query = query.Where(c => c.DealerId == dealerId.Value);

        if (startDate.HasValue)
            query = query.Where(c => c.StartedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(c => c.StartedAt <= endDate.Value);

        var conversations = await query.ToListAsync(cancellationToken);

        var totalConversations = conversations.Count;
        var activeConversations = conversations.Count(c => c.Status == ConversationStatus.Active);
        var handedOffConversations = conversations.Count(c => c.IsHandedOff);
        var hotLeads = conversations.Count(c => c.LeadTemperature == LeadTemperature.Hot);
        var warmLeads = conversations.Count(c => c.LeadTemperature == LeadTemperature.Warm || c.LeadTemperature == LeadTemperature.WarmHot);
        var coldLeads = conversations.Count(c => c.LeadTemperature == LeadTemperature.Cold);

        var avgLeadScore = conversations.Any() ? conversations.Average(c => c.LeadScore) : 0;
        var conversionRate = totalConversations > 0 ? (double)handedOffConversations / totalConversations : 0;
        var avgDuration = conversations.Any() 
            ? TimeSpan.FromSeconds(conversations.Average(c => c.Duration.TotalSeconds)) 
            : TimeSpan.Zero;

        var topSignals = conversations
            .SelectMany(c => c.BuyingSignals)
            .GroupBy(s => s)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToDictionary(g => g.Key, g => g.Count());

        return new Dictionary<string, object>
        {
            ["TotalConversations"] = totalConversations,
            ["ActiveConversations"] = activeConversations,
            ["HandedOffConversations"] = handedOffConversations,
            ["HotLeads"] = hotLeads,
            ["WarmLeads"] = warmLeads,
            ["ColdLeads"] = coldLeads,
            ["AverageLeadScore"] = avgLeadScore,
            ["ConversionRate"] = conversionRate,
            ["AverageConversationDuration"] = avgDuration,
            ["TopBuyingSignals"] = topSignals
        };
    }
}
