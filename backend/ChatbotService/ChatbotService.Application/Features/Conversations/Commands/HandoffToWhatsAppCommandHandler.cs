using MediatR;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Application.DTOs;

namespace ChatbotService.Application.Features.Conversations.Commands;

public class HandoffToWhatsAppCommandHandler : IRequestHandler<HandoffToWhatsAppCommand, WhatsAppHandoffDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IOpenAIService _openAIService;

    public HandoffToWhatsAppCommandHandler(
        IConversationRepository conversationRepository,
        IWhatsAppService whatsAppService,
        IOpenAIService openAIService)
    {
        _conversationRepository = conversationRepository;
        _whatsAppService = whatsAppService;
        _openAIService = openAIService;
    }

    public async Task<WhatsAppHandoffDto> Handle(HandoffToWhatsAppCommand request, CancellationToken cancellationToken)
    {
        // Get conversation
        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation == null)
        {
            throw new InvalidOperationException($"Conversation {request.ConversationId} not found");
        }

        // Validate dealer WhatsApp
        if (string.IsNullOrEmpty(conversation.DealerWhatsApp))
        {
            throw new InvalidOperationException("Dealer WhatsApp number not configured");
        }

        if (!_whatsAppService.IsValidWhatsAppNumber(conversation.DealerWhatsApp))
        {
            throw new InvalidOperationException($"Invalid WhatsApp number: {conversation.DealerWhatsApp}");
        }

        // Generate conversation summary
        var messages = await _conversationRepository.GetMessagesAsync(conversation.Id, cancellationToken);
        var conversationSummary = await _openAIService.GenerateConversationSummaryAsync(messages, cancellationToken);

        // Prepare vehicle details
        var vehicleDetails = !string.IsNullOrEmpty(conversation.VehicleTitle)
            ? $"{conversation.VehicleTitle} - ${conversation.VehiclePrice:N0}"
            : "N/A";

        // Send WhatsApp message
        var handoff = await _whatsAppService.SendHandoffMessageAsync(
            conversation.DealerWhatsApp,
            conversation.DealerName ?? "Dealer",
            conversation.UserName ?? "Prospecto",
            conversation.UserPhone ?? "No phone",
            vehicleDetails,
            conversationSummary,
            conversation.LeadScore,
            cancellationToken);

        // Update conversation status
        conversation.IsHandedOff = true;
        conversation.HandedOffAt = DateTime.UtcNow;
        conversation.HandoffMethod = HandoffMethod.WhatsApp;
        conversation.HandoffNotes = request.CustomMessage;
        conversation.Status = ConversationStatus.HandedOff;

        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        // Add system message to conversation
        var systemMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = MessageRole.System,
            Content = $"✅ Conversación transferida a {conversation.DealerName} por WhatsApp. Te contactarán pronto.",
            Type = MessageType.HandoffNotification
        };
        
        await _conversationRepository.AddMessageAsync(conversation.Id, systemMessage, cancellationToken);

        return new WhatsAppHandoffDto(
            handoff.Id,
            handoff.ConversationId,
            handoff.UserName,
            handoff.UserPhone,
            handoff.LeadScore,
            handoff.LeadTemperature,
            handoff.ConversationSummary,
            handoff.BuyingSignals,
            handoff.DealerWhatsAppNumber,
            handoff.Status,
            handoff.InitiatedAt,
            handoff.SentAt,
            handoff.DeliveredAt
        );
    }
}
