using FluentValidation;
using AdminService.Application.UseCases.Messages;

namespace AdminService.Application.Validators.Messages;

/// <summary>
/// Validator for MarkMessageReadCommand.
/// Validates MessageId with NoSqlInjection/NoXss.
/// </summary>
public class MarkMessageReadCommandValidator : AbstractValidator<MarkMessageReadCommand>
{
    public MarkMessageReadCommandValidator()
    {
        RuleFor(x => x.MessageId)
            .NotEmpty().WithMessage("Message ID is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();
    }
}

/// <summary>
/// Validator for ReplyToMessageCommand.
/// Validates MessageId and Message with NoSqlInjection/NoXss.
/// </summary>
public class ReplyToMessageCommandValidator : AbstractValidator<ReplyToMessageCommand>
{
    public ReplyToMessageCommandValidator()
    {
        RuleFor(x => x.MessageId)
            .NotEmpty().WithMessage("Message ID is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MaximumLength(5000)
            .NoSqlInjection()
            .NoXss();
    }
}
