using MediatR;
using ChatbotService.Application.DTOs;

namespace ChatbotService.Application.Features.Conversations.Commands;

/// <summary>
/// Ends a conversation with a reason
/// </summary>
public record EndConversationCommand(
    Guid ConversationId,
    string Reason
) : IRequest<ConversationDto>;

public class EndConversationCommandHandler : IRequestHandler<EndConversationCommand, ConversationDto>
{
    public Task<ConversationDto> Handle(EndConversationCommand request, CancellationToken cancellationToken)
    {
        // Return a mock conversation for now
        // In production, this would update the conversation in the database
        return Task.FromResult(new ConversationDto(
            request.ConversationId,
            Guid.Empty,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            ChatbotService.Domain.Entities.ConversationStatus.Ended,
            0,
            ChatbotService.Domain.Entities.LeadTemperature.Cold,
            new List<string>(),
            false,
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow,
            0,
            TimeSpan.FromHours(1)
        ));
    }
}
