using ChatbotService.Domain.Entities;

namespace ChatbotService.Domain.Interfaces;

public interface IChatConversationRepository
{
    Task<ChatConversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ChatConversation?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ChatConversation?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<ChatConversation?> GetActiveConversationAsync(Guid? userId, string? sessionId, Guid? vehicleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatConversation>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatConversation>> GetByVehicleIdAsync(Guid vehicleId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatConversation>> GetRecentConversationsAsync(int hours = 24, CancellationToken cancellationToken = default);
    Task<int> GetConversationCountAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<ChatConversation> CreateAsync(ChatConversation conversation, CancellationToken cancellationToken = default);
    Task<ChatConversation> UpdateAsync(ChatConversation conversation, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
