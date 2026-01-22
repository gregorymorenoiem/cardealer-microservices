using MediatR;
using Microsoft.Extensions.Logging;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Application.Features.Chat.Commands;

/// <summary>
/// Handler para terminar sesión de chat
/// ⚠️ FASE 4: Este handler está IMPLEMENTADO pero el endpoint NO debe ser 
/// consumido por el frontend en esta versión.
/// </summary>
public class EndChatSessionCommandHandler : IRequestHandler<EndChatSessionCommand, bool>
{
    private readonly IChatSessionRepository _repository;
    private readonly ILogger<EndChatSessionCommandHandler> _logger;

    public EndChatSessionCommandHandler(
        IChatSessionRepository repository,
        ILogger<EndChatSessionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(EndChatSessionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[FASE 4 - NOT IN USE] Ending chat session {SessionId}", request.SessionId);

        var session = await _repository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            _logger.LogWarning("Chat session {SessionId} not found", request.SessionId);
            return false;
        }

        if (!session.IsActive())
        {
            _logger.LogWarning("Chat session {SessionId} is not active", request.SessionId);
            return false;
        }

        session.End();
        
        if (request.UserRating.HasValue)
        {
            session.UserRating = request.UserRating.Value;
        }

        await _repository.UpdateAsync(session, cancellationToken);

        _logger.LogInformation("[FASE 4] Chat session {SessionId} ended", request.SessionId);
        return true;
    }
}
