using ChatbotService.Domain.Entities;

namespace ChatbotService.Domain.Interfaces;

public interface IChatMessageRepository
{
    Task<ChatMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatMessage>> GetByConversationIdAsync(Guid conversationId, int? limit = null, CancellationToken cancellationToken = default);
    Task<ChatMessage> CreateAsync(ChatMessage message, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatMessage>> CreateManyAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteByConversationIdAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task<int> GetMessageCountAsync(Guid conversationId, CancellationToken cancellationToken = default);
}
