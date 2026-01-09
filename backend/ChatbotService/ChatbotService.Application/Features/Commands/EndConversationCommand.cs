using MediatR;
using ChatbotService.Application.DTOs;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Interfaces;

namespace ChatbotService.Application.Features.Commands;

// ============================================
// End Conversation Command
// ============================================

public record EndConversationCommand(
    Guid ConversationId,
    string Reason
) : IRequest<ConversationDto>;

public class EndConversationHandler : IRequestHandler<EndConversationCommand, ConversationDto>
{
    private readonly IChatConversationRepository _conversationRepository;

    public EndConversationHandler(IChatConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<ConversationDto> Handle(EndConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(request.ConversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation {request.ConversationId} not found");

        conversation.EndConversation(request.Reason);
        
        var updated = await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        return updated.ToDto();
    }
}
