using MediatR;
using ChatbotService.Application.DTOs;

namespace ChatbotService.Application.Features.Conversations.Commands;

/// <summary>
/// Procesa mensaje del usuario y genera respuesta del chatbot
/// </summary>
public record SendMessageCommand(
    Guid ConversationId,
    string UserMessage,
    VehicleContextDto? VehicleContext = null
) : IRequest<ChatbotResponseDto>;
