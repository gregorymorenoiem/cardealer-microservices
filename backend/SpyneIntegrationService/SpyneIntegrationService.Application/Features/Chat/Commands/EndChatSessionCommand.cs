using MediatR;

namespace SpyneIntegrationService.Application.Features.Chat.Commands;

/// <summary>
/// Command to end a chat session
/// 
/// ⚠️ FASE 4: Este endpoint está IMPLEMENTADO pero NO debe ser consumido 
/// por el frontend en esta versión.
/// </summary>
public record EndChatSessionCommand : IRequest<bool>
{
    public Guid SessionId { get; init; }
    public string? EndReason { get; init; }
    public int? UserRating { get; init; } // 1-5
    public string? UserFeedback { get; init; }
}
