using MediatR;
using SpyneIntegrationService.Application.DTOs;

namespace SpyneIntegrationService.Application.Features.Chat.Commands;

/// <summary>
/// Command to send a message in an active chat session
/// 
/// ⚠️ FASE 4: Este endpoint está IMPLEMENTADO pero NO debe ser consumido 
/// por el frontend en esta versión. Está preparado para futuras iteraciones.
/// </summary>
public record SendChatMessageCommand : IRequest<ChatMessageResponseDto>
{
    /// <summary>
    /// ID de la sesión de chat
    /// </summary>
    public Guid SessionId { get; init; }
    
    /// <summary>
    /// Mensaje del usuario
    /// </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Metadatos adicionales (ej: página actual, tiempo en página)
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}
