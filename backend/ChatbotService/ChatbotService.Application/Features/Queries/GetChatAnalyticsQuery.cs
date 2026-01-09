using MediatR;
using ChatbotService.Application.DTOs;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Interfaces;

namespace ChatbotService.Application.Features.Queries;

// ============================================
// Get Chat Analytics Query
// ============================================

public record GetChatAnalyticsQuery(
    DateTime? From = null,
    DateTime? To = null
) : IRequest<ChatAnalyticsDto>;

public class GetChatAnalyticsHandler : IRequestHandler<GetChatAnalyticsQuery, ChatAnalyticsDto>
{
    private readonly IChatConversationRepository _conversationRepository;

    public GetChatAnalyticsHandler(IChatConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<ChatAnalyticsDto> Handle(GetChatAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var from = request.From ?? DateTime.UtcNow.AddDays(-30);
        var to = request.To ?? DateTime.UtcNow;

        var conversations = await _conversationRepository.GetRecentConversationsAsync(
            (int)(to - from).TotalHours,
            cancellationToken);

        var conversationList = conversations.ToList();

        var totalConversations = conversationList.Count;
        var activeConversations = conversationList.Count(c => c.Status == ConversationStatus.Active);
        var totalMessages = conversationList.Sum(c => c.MessageCount);
        var avgMessages = totalConversations > 0 ? (double)totalMessages / totalConversations : 0;
        var totalTokens = conversationList.Sum(c => c.TotalTokensUsed);
        var totalCost = conversationList.Sum(c => c.EstimatedCost);

        var conversationsByStatus = conversationList
            .GroupBy(c => c.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var leadsByQualification = conversationList
            .GroupBy(c => c.LeadQualification.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return new ChatAnalyticsDto(
            totalConversations,
            activeConversations,
            totalMessages,
            avgMessages,
            0, // Average response time would need message-level data
            totalTokens,
            totalCost,
            conversationsByStatus,
            leadsByQualification
        );
    }
}
