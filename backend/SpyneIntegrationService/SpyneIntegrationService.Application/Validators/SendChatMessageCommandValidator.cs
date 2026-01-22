using FluentValidation;
using SpyneIntegrationService.Application.Features.Chat.Commands;

namespace SpyneIntegrationService.Application.Validators;

/// <summary>
/// Validator for SendChatMessageCommand
/// 
/// ⚠️ FASE 4: Validador implementado pero el endpoint NO se consume en frontend
/// </summary>
public class SendChatMessageCommandValidator : AbstractValidator<SendChatMessageCommand>
{
    public SendChatMessageCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("SessionId is required");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required")
            .MaximumLength(2000)
            .WithMessage("Message cannot exceed 2000 characters");
    }
}
