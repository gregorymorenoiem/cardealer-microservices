using MediatR;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Application.DTOs;

namespace ChatbotService.Application.Features.Conversations.Commands;

public class StartConversationCommandHandler : IRequestHandler<StartConversationCommand, ConversationDto>
{
    private readonly IConversationRepository _conversationRepository;

    public StartConversationCommandHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<ConversationDto> Handle(StartConversationCommand request, CancellationToken cancellationToken)
    {
        // Check if there's an active conversation for this user
        var existingConversation = await _conversationRepository.GetActiveByUserIdAsync(request.UserId, cancellationToken);
        
        if (existingConversation != null)
        {
            // Return existing conversation
            return MapToDto(existingConversation);
        }

        // Create new conversation
        var conversation = new Conversation
        {
            UserId = request.UserId,
            UserName = request.UserName,
            UserEmail = request.UserEmail,
            UserPhone = request.UserPhone,
            VehicleId = request.VehicleId,
            VehicleTitle = request.VehicleTitle,
            VehiclePrice = request.VehiclePrice,
            DealerId = request.DealerId,
            DealerName = request.DealerName,
            DealerWhatsApp = request.DealerWhatsApp,
            Status = ConversationStatus.Active
        };

        // Add welcome message
        var welcomeMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = MessageRole.Assistant,
            Content = GenerateWelcomeMessage(request.VehicleTitle),
            Type = MessageType.Text
        };
        
        conversation.Messages.Add(welcomeMessage);

        var created = await _conversationRepository.CreateAsync(conversation, cancellationToken);
        return MapToDto(created);
    }

    private string GenerateWelcomeMessage(string? vehicleTitle)
    {
        if (!string.IsNullOrEmpty(vehicleTitle))
        {
            return $"¡Hola! 👋 Veo que estás interesado en el {vehicleTitle}. ¿En qué puedo ayudarte?";
        }
        
        return "¡Hola! 👋 Soy OKLA Bot, tu asistente virtual. ¿En qué puedo ayudarte hoy?";
    }

    private ConversationDto MapToDto(Conversation conversation)
    {
        return new ConversationDto(
            conversation.Id,
            conversation.UserId,
            conversation.UserName,
            conversation.UserEmail,
            conversation.UserPhone,
            conversation.VehicleId,
            conversation.VehicleTitle,
            conversation.VehiclePrice,
            conversation.DealerId,
            conversation.DealerName,
            conversation.Status,
            conversation.LeadScore,
            conversation.LeadTemperature,
            conversation.BuyingSignals,
            conversation.IsHandedOff,
            conversation.StartedAt,
            conversation.EndedAt,
            conversation.MessageCount,
            conversation.Duration
        );
    }
}
