using MediatR;
using ChatbotService.Application.DTOs;

namespace ChatbotService.Application.Features.Conversations.Commands;

/// <summary>
/// Inicia una nueva conversación con el chatbot
/// </summary>
public record StartConversationCommand(
    Guid UserId,
    string? UserName,
    string? UserEmail,
    string? UserPhone,
    Guid? VehicleId,
    string? VehicleTitle,
    decimal? VehiclePrice,
    Guid? DealerId,
    string? DealerName,
    string? DealerWhatsApp
) : IRequest<ConversationDto>;
