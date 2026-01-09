using System.Diagnostics;
using MediatR;
using ChatbotService.Application.DTOs;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Interfaces;

namespace ChatbotService.Application.Features.Commands;

// ============================================
// Send Message Command
// ============================================

public record SendMessageCommand(
    Guid ConversationId,
    string Content,
    VehicleContextDto? VehicleContext
) : IRequest<SendMessageResponseDto>;

public class SendMessageHandler : IRequestHandler<SendMessageCommand, SendMessageResponseDto>
{
    private readonly IChatConversationRepository _conversationRepository;
    private readonly IChatMessageRepository _messageRepository;
    private readonly IChatbotService _chatbotService;

    public SendMessageHandler(
        IChatConversationRepository conversationRepository,
        IChatMessageRepository messageRepository,
        IChatbotService chatbotService)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _chatbotService = chatbotService;
    }

    public async Task<SendMessageResponseDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // Get conversation
        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(request.ConversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {request.ConversationId} not found");

        if (conversation.Status != ConversationStatus.Active)
        {
            throw new InvalidOperationException("Cannot send message to inactive conversation");
        }

        // Create user message
        var userMessage = ChatMessage.CreateUserMessage(conversation.Id, request.Content);
        await _messageRepository.CreateAsync(userMessage, cancellationToken);
        conversation.AddMessage(userMessage);

        // Build vehicle context
        VehicleContext? vehicleContext = null;
        if (request.VehicleContext != null)
        {
            vehicleContext = new VehicleContext
            {
                VehicleId = request.VehicleContext.VehicleId,
                Make = request.VehicleContext.Make,
                Model = request.VehicleContext.Model,
                Year = request.VehicleContext.Year,
                Price = request.VehicleContext.Price,
                Mileage = request.VehicleContext.Mileage,
                Transmission = request.VehicleContext.Transmission,
                FuelType = request.VehicleContext.FuelType,
                Color = request.VehicleContext.Color,
                Description = request.VehicleContext.Description,
                SellerName = request.VehicleContext.SellerName,
                Location = request.VehicleContext.Location
            };
        }

        // Generate AI response
        var stopwatch = Stopwatch.StartNew();
        var aiResponse = await _chatbotService.GenerateResponseAsync(
            request.Content,
            conversation,
            vehicleContext,
            cancellationToken);
        stopwatch.Stop();

        // Create assistant message
        var assistantMessage = ChatMessage.CreateAssistantMessage(
            conversation.Id,
            aiResponse.Content,
            aiResponse.TokensUsed,
            stopwatch.Elapsed);
        assistantMessage.IntentDetected = aiResponse.Intent;
        assistantMessage.SentimentScore = aiResponse.SentimentScore;

        await _messageRepository.CreateAsync(assistantMessage, cancellationToken);
        conversation.AddMessage(assistantMessage);

        // Update conversation token usage and cost
        conversation.AddTokenUsage(aiResponse.TokensUsed, CalculateCost(aiResponse.TokensUsed));

        // Update lead qualification based on intent
        if (aiResponse.Intent == "buying_intent" || aiResponse.ShouldTransferToAgent)
        {
            conversation.UpdateLeadScore(0.8, LeadQualification.Hot);
        }

        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        return new SendMessageResponseDto(
            userMessage.ToDto(),
            assistantMessage.ToDto(),
            aiResponse.SuggestedReplies.Select(r => r.ToDto()).ToList(),
            aiResponse.ShouldTransferToAgent,
            aiResponse.TransferReason
        );
    }

    private static decimal CalculateCost(int tokens)
    {
        // GPT-4o-mini pricing: $0.15 per 1M input tokens, $0.60 per 1M output tokens
        // Simplified: average $0.30 per 1M tokens
        return tokens * 0.0000003m;
    }
}
