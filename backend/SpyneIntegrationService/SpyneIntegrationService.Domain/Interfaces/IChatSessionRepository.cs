using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Domain.Interfaces;

/// <summary>
/// Repository interface for chat sessions (Fase 4 - Backend only)
/// </summary>
public interface IChatSessionRepository
{
    Task<ChatSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ChatSession?> GetBySpyneTokenAsync(string spyneSessionToken, CancellationToken cancellationToken = default);
    Task<ChatSession?> GetActiveByVisitorAsync(string visitorFingerprint, CancellationToken cancellationToken = default);
    Task<List<ChatSession>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<List<ChatSession>> GetByVehicleIdAsync(Guid vehicleId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<ChatSession>> GetByDealerIdAsync(Guid dealerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<ChatSession>> GetWithLeadsAsync(Guid dealerId, DateTime since, CancellationToken cancellationToken = default);
    Task<List<ChatSession>> GetExpiredSessionsAsync(TimeSpan inactivityThreshold, CancellationToken cancellationToken = default);
    Task<ChatSession> AddAsync(ChatSession session, CancellationToken cancellationToken = default);
    Task<ChatSession> UpdateAsync(ChatSession session, CancellationToken cancellationToken = default);
    Task AddMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);
    Task<List<ChatMessage>> GetMessagesAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
