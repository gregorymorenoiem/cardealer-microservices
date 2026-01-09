using MediatR;
using ChatbotService.Application.DTOs;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Interfaces;

namespace ChatbotService.Application.Features.Commands;

// ============================================
// Create Conversation Command
// ============================================

public record CreateConversationCommand(
    Guid? UserId,
    string? SessionId,
    Guid? VehicleId,
    string? UserEmail,
    string? UserName,
    string? UserPhone,
    VehicleContextDto? VehicleContext
) : IRequest<ConversationDto>;

public class CreateConversationHandler : IRequestHandler<CreateConversationCommand, ConversationDto>
{
    private readonly IChatConversationRepository _conversationRepository;

    public CreateConversationHandler(IChatConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<ConversationDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        // Check for existing active conversation
        var existingConversation = await _conversationRepository.GetActiveConversationAsync(
            request.UserId,
            request.SessionId,
            request.VehicleId,
            cancellationToken);

        if (existingConversation != null)
        {
            return existingConversation.ToDto();
        }

        // Create new conversation
        var conversation = new ChatConversation
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
            VehicleId = request.VehicleId,
            UserEmail = request.UserEmail,
            UserName = request.UserName,
            UserPhone = request.UserPhone,
            Status = ConversationStatus.Active,
            VehicleContext = request.VehicleContext != null
                ? System.Text.Json.JsonSerializer.Serialize(request.VehicleContext)
                : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _conversationRepository.CreateAsync(conversation, cancellationToken);
        return created.ToDto();
    }
}
