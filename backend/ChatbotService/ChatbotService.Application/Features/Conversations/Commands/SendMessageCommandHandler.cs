using MediatR;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Interfaces;
using ChatbotService.Application.DTOs;

namespace ChatbotService.Application.Features.Conversations.Commands;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, ChatbotResponseDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IOpenAIService _openAIService;
    private readonly ILeadScoringEngine _leadScoringEngine;

    public SendMessageCommandHandler(
        IConversationRepository conversationRepository,
        IOpenAIService openAIService,
        ILeadScoringEngine leadScoringEngine)
    {
        _conversationRepository = conversationRepository;
        _openAIService = openAIService;
        _leadScoringEngine = leadScoringEngine;
    }

    public async Task<ChatbotResponseDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // Get conversation
        var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation == null)
        {
            throw new InvalidOperationException($"Conversation {request.ConversationId} not found");
        }

        // Add user message
        var userMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = MessageRole.User,
            Content = request.UserMessage,
            Type = MessageType.Text
        };
        
        await _conversationRepository.AddMessageAsync(conversation.Id, userMessage, cancellationToken);

        // Analyze intent
        var intentAnalysis = await _openAIService.AnalyzeIntentAsync(
            request.UserMessage, 
            conversation.Messages, 
            cancellationToken);

        // Update user message with intent analysis
        userMessage.DetectedIntent = intentAnalysis.IntentType.ToString();
        userMessage.ExtractedSignals = intentAnalysis.BuyingSignals.Select(s => s.Signal).ToList();

        // Generate vehicle context
        var vehicleContext = GenerateVehicleContext(conversation);

        // Generate AI response
        var aiResponse = await _openAIService.GenerateResponseAsync(
            conversation.Messages, 
            vehicleContext,
            cancellationToken);

        // Add assistant message
        var assistantMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = MessageRole.Assistant,
            Content = aiResponse,
            Type = MessageType.Text
        };
        
        await _conversationRepository.AddMessageAsync(conversation.Id, assistantMessage, cancellationToken);

        // Update conversation with signals
        UpdateConversationWithSignals(conversation, intentAnalysis);

        // Recalculate lead score
        var newScore = await _leadScoringEngine.CalculateLeadScoreAsync(conversation, cancellationToken);
        conversation.LeadScore = newScore;
        conversation.LeadTemperature = _leadScoringEngine.DetermineLeadTemperature(newScore);
        conversation.UpdatedAt = DateTime.UtcNow;

        // Check if handoff should be triggered
        var shouldHandoff = _leadScoringEngine.ShouldTriggerHandoff(conversation);
        var handoffReason = shouldHandoff 
            ? $"Lead score: {newScore} (HOT) - Ready for immediate contact" 
            : null;

        // Update conversation
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        return new ChatbotResponseDto(
            aiResponse,
            MapMessageToDto(assistantMessage),
            MapConversationToDto(conversation),
            shouldHandoff,
            handoffReason
        );
    }

    private string GenerateVehicleContext(Conversation conversation)
    {
        if (conversation.VehicleId == null)
        {
            return "No specific vehicle context";
        }

        return $@"
Vehicle Information:
- Title: {conversation.VehicleTitle}
- Price: ${conversation.VehiclePrice:N0}
- Dealer: {conversation.DealerName}
- Vehicle ID: {conversation.VehicleId}
";
    }

    private void UpdateConversationWithSignals(Conversation conversation, IntentAnalysis analysis)
    {
        // Add new buying signals
        foreach (var signal in analysis.BuyingSignals)
        {
            if (!conversation.BuyingSignals.Contains(signal.Signal))
            {
                conversation.BuyingSignals.Add(signal.Signal);
            }
        }

        // Update conversation flags
        if (analysis.ExtractedUrgency != null)
        {
            conversation.HasUrgency = true;
            conversation.PurchaseTimeframe = analysis.ExtractedUrgency;
        }

        if (analysis.ExtractedBudget != null)
        {
            conversation.HasBudget = true;
        }

        if (analysis.HasTradeIn == true)
        {
            conversation.HasTradeIn = true;
        }

        if (analysis.IntentType == IntentType.TestDriveRequest)
        {
            conversation.WantsTestDrive = true;
        }
    }

    private MessageDto MapMessageToDto(Message message)
    {
        return new MessageDto(
            message.Id,
            message.ConversationId,
            message.Role,
            message.Content,
            message.Type,
            message.Timestamp,
            message.DetectedIntent,
            message.ExtractedSignals,
            message.SentimentScore
        );
    }

    private ConversationDto MapConversationToDto(Conversation conversation)
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
