using ChatbotService.Domain.Entities;

namespace ChatbotService.Domain.Interfaces;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Conversation?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Conversation>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<List<Conversation>> GetByDealerIdAsync(Guid dealerId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<List<Conversation>> GetHotLeadsAsync(int minScore = 85, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<Conversation> CreateAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task<Conversation> UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Message operations
    Task<Message> AddMessageAsync(Guid conversationId, Message message, CancellationToken cancellationToken = default);
    Task<List<Message>> GetMessagesAsync(Guid conversationId, CancellationToken cancellationToken = default);
    
    // Analytics
    Task<Dictionary<string, object>> GetStatisticsAsync(Guid? dealerId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}
