using MediatR;
using ChatbotService.Application.DTOs;
using ChatbotService.Domain.Interfaces;

namespace ChatbotService.Application.Features.Queries;

// ============================================
// Get Conversation Query
// ============================================

public record GetConversationQuery(Guid Id) : IRequest<ConversationDto?>;

public class GetConversationHandler : IRequestHandler<GetConversationQuery, ConversationDto?>
{
    private readonly IChatConversationRepository _conversationRepository;

    public GetConversationHandler(IChatConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<ConversationDto?> Handle(GetConversationQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(request.Id, cancellationToken);
        return conversation?.ToDto();
    }
}
