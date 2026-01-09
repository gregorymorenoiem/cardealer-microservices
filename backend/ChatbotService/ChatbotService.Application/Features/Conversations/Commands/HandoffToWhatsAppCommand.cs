using MediatR;
using ChatbotService.Application.DTOs;

namespace ChatbotService.Application.Features.Conversations.Commands;

/// <summary>
/// Transfiere conversación a WhatsApp del dealer
/// </summary>
public record HandoffToWhatsAppCommand(
    Guid ConversationId,
    string? CustomMessage
) : IRequest<WhatsAppHandoffDto>;
