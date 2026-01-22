using MediatR;
using SpyneIntegrationService.Application.DTOs;

namespace SpyneIntegrationService.Application.Features.Chat.Queries;

/// <summary>
/// Query to get a chat session
/// 
/// ⚠️ FASE 4: Este endpoint está IMPLEMENTADO pero NO debe ser consumido 
/// por el frontend en esta versión.
/// </summary>
public record GetChatSessionQuery(Guid SessionId) : IRequest<ChatSessionDto?>;
