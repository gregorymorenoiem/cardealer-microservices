using MediatR;
using ChatbotService.Application.DTOs;

namespace ChatbotService.Application.Features.Conversations.Queries;

/// <summary>
/// Gets chat analytics for a date range
/// </summary>
public record GetChatAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<ChatAnalyticsDto>;

public class GetChatAnalyticsQueryHandler : IRequestHandler<GetChatAnalyticsQuery, ChatAnalyticsDto>
{
    public Task<ChatAnalyticsDto> Handle(GetChatAnalyticsQuery request, CancellationToken cancellationToken)
    {
        // Return mock analytics for now
        // In production, this would query the database for real metrics
        return Task.FromResult(new ChatAnalyticsDto(
            TotalConversations: 150,
            ActiveConversations: 25,
            HandedOffConversations: 8,
            HotLeads: 12,
            WarmLeads: 35,
            ColdLeads: 103,
            AverageLeadScore: 65.5,
            ConversionRate: 0.15,
            AverageConversationDuration: TimeSpan.FromMinutes(8),
            TopIntents: new Dictionary<string, int>
            {
                { "price_inquiry", 45 },
                { "vehicle_availability", 38 },
                { "financing_options", 25 },
                { "test_drive_schedule", 18 },
                { "trade_in_valuation", 12 }
            },
            TopBuyingSignals: new Dictionary<string, int>
            {
                { "budget_mentioned", 35 },
                { "timeline_urgent", 28 },
                { "trade_in_ready", 22 }
            },
            RecentHotLeads: new List<ConversationSummaryDto>()
        ));
    }
}
