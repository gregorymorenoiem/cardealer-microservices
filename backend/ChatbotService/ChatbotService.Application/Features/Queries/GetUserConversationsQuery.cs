using MediatR;
using ChatbotService.Application.DTOs;
using ChatbotService.Domain.Interfaces;

namespace ChatbotService.Application.Features.Queries;

// ============================================
// Get User Conversations Query
// ============================================

public record GetUserConversationsQuery(
    Guid UserId,
    int Skip = 0,
    int Take = 20
) : IRequest<List<ConversationSummaryDto>>;

public class GetUserConversationsHandler : IRequestHandler<GetUserConversationsQuery, List<ConversationSummaryDto>>
{
    private readonly IChatConversationRepository _conversationRepository;

    public GetUserConversationsHandler(IChatConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<List<ConversationSummaryDto>> Handle(GetUserConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await _conversationRepository.GetByUserIdAsync(
            request.UserId,
            request.Skip,
            request.Take,
            cancellationToken);

        return conversations.Select(c => c.ToSummaryDto()).ToList();
    }
}
