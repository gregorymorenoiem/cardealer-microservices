using FluentValidation;
using ChatbotService.Application.Features.Sessions.Commands;

namespace ChatbotService.Application.Validators;

/// <summary>
/// Validator for StartSessionCommand — validates all user-supplied inputs
/// against SQL injection and XSS patterns.
/// </summary>
public class StartSessionCommandValidator : AbstractValidator<StartSessionCommand>
{
    public StartSessionCommandValidator()
    {
        RuleFor(x => x.SessionType)
            .NotEmpty().WithMessage("SessionType is required")
            .MaximumLength(50)
            .NoSecurityThreats();

        RuleFor(x => x.Channel)
            .NotEmpty().WithMessage("Channel is required")
            .MaximumLength(50)
            .NoSecurityThreats();

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required")
            .MaximumLength(10)
            .NoSecurityThreats();

        When(x => !string.IsNullOrWhiteSpace(x.UserName), () =>
        {
            RuleFor(x => x.UserName!)
                .MaximumLength(200)
                .NoSecurityThreats();
        });

        When(x => !string.IsNullOrWhiteSpace(x.UserEmail), () =>
        {
            RuleFor(x => x.UserEmail!)
                .MaximumLength(254)
                .EmailAddress()
                .NoSecurityThreats();
        });

        When(x => !string.IsNullOrWhiteSpace(x.UserPhone), () =>
        {
            RuleFor(x => x.UserPhone!)
                .MaximumLength(20)
                .NoSecurityThreats();
        });

        When(x => !string.IsNullOrWhiteSpace(x.ChannelUserId), () =>
        {
            RuleFor(x => x.ChannelUserId!)
                .MaximumLength(200)
                .NoSecurityThreats();
        });

        When(x => !string.IsNullOrWhiteSpace(x.UserAgent), () =>
        {
            RuleFor(x => x.UserAgent!)
                .MaximumLength(500)
                .NoSqlInjection();
        });

        When(x => !string.IsNullOrWhiteSpace(x.IpAddress), () =>
        {
            RuleFor(x => x.IpAddress!)
                .MaximumLength(45)
                .NoSecurityThreats();
        });

        When(x => !string.IsNullOrWhiteSpace(x.DeviceType), () =>
        {
            RuleFor(x => x.DeviceType!)
                .MaximumLength(50)
                .NoSecurityThreats();
        });
    }
}

/// <summary>
/// Validator for SendMessageCommand — critical endpoint, validates user message
/// against XSS and SQL injection. Message content is allowed to be more permissive
/// (users may type special chars) but still blocks dangerous patterns.
/// </summary>
public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.SessionToken)
            .NotEmpty().WithMessage("SessionToken is required")
            .MaximumLength(100)
            .NoSecurityThreats();

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(2000).WithMessage("Message cannot exceed 2000 characters")
            .NoSqlInjection()
            .NoXssAdvanced();

        RuleFor(x => x.MessageType)
            .NotEmpty().WithMessage("MessageType is required")
            .MaximumLength(50)
            .NoSecurityThreats();

        When(x => !string.IsNullOrWhiteSpace(x.MediaUrl), () =>
        {
            RuleFor(x => x.MediaUrl!)
                .MaximumLength(2048)
                .NoSecurityThreats();
        });
    }
}

/// <summary>
/// Validator for EndSessionCommand.
/// </summary>
public class EndSessionCommandValidator : AbstractValidator<EndSessionCommand>
{
    public EndSessionCommandValidator()
    {
        RuleFor(x => x.SessionToken)
            .NotEmpty().WithMessage("SessionToken is required")
            .MaximumLength(100)
            .NoSecurityThreats();

        When(x => !string.IsNullOrWhiteSpace(x.Reason), () =>
        {
            RuleFor(x => x.Reason!)
                .MaximumLength(500)
                .NoSecurityThreats();
        });
    }
}

/// <summary>
/// Validator for TransferToAgentCommand.
/// </summary>
public class TransferToAgentCommandValidator : AbstractValidator<TransferToAgentCommand>
{
    public TransferToAgentCommandValidator()
    {
        RuleFor(x => x.SessionToken)
            .NotEmpty().WithMessage("SessionToken is required")
            .MaximumLength(100)
            .NoSecurityThreats();

        When(x => !string.IsNullOrWhiteSpace(x.Reason), () =>
        {
            RuleFor(x => x.Reason!)
                .MaximumLength(500)
                .NoSecurityThreats();
        });
    }
}
