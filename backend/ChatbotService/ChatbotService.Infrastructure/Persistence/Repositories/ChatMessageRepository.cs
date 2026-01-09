using Microsoft.EntityFrameworkCore;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Interfaces;

namespace ChatbotService.Infrastructure.Persistence.Repositories;

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly ChatbotDbContext _context;

    public ChatMessageRepository(ChatbotDbContext context)
    {
        _context = context;
    }

    public async Task<ChatMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ChatMessage>> GetByConversationIdAsync(Guid conversationId, int? limit = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt);

        if (limit.HasValue)
        {
            return await query.TakeLast(limit.Value).ToListAsync(cancellationToken);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<ChatMessage> CreateAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);
        return message;
    }

    public async Task<IEnumerable<ChatMessage>> CreateManyAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        var messageList = messages.ToList();
        _context.Messages.AddRange(messageList);
        await _context.SaveChangesAsync(cancellationToken);
        return messageList;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await GetByIdAsync(id, cancellationToken);
        if (message != null)
        {
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteByConversationIdAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var messages = await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .ToListAsync(cancellationToken);

        _context.Messages.RemoveRange(messages);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetMessageCountAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _context.Messages.CountAsync(m => m.ConversationId == conversationId, cancellationToken);
    }
}
