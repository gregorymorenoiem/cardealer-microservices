using MediatR;
using ChatbotService.Application.DTOs;

namespace ChatbotService.Application.Features.Conversations.Commands;

/// <summary>
/// Creates a new conversation
/// </summary>
public record CreateConversationCommand(
    Guid UserId,
    string? SessionId,
    Guid? VehicleId,
    string? UserEmail,
    string? UserName,
    string? UserPhone,
    VehicleContextDto? VehicleContext
) : IRequest<ConversationDto>;

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, ConversationDto>
{
    private readonly IMediator _mediator;

    public CreateConversationCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<ConversationDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        // Delegate to StartConversationCommand
        var startCommand = new StartConversationCommand(
            request.UserId,
            request.UserName,
            request.UserEmail,
            request.UserPhone,
            request.VehicleId,
            request.VehicleContext?.Make != null ? $"{request.VehicleContext.Make} {request.VehicleContext.Model}" : null,
            request.VehicleContext?.Price,
            null, // DealerId
            null, // DealerName
            null  // DealerWhatsApp
        );

        return await _mediator.Send(startCommand, cancellationToken);
    }
}
