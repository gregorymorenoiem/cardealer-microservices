using FluentValidation;
using NotificationService.Application.DTOs;
using NotificationService.Application.Validators;

namespace NotificationService.Application.UseCases.SendSmsNotification;

public class SendSmsNotificationCommandValidator : AbstractValidator<SendSmsNotificationCommand>
{
    public SendSmsNotificationCommandValidator()
    {
        RuleFor(x => x.Request.To)
            .NotEmpty().WithMessage("Recipient phone number is required")
            .NoSqlInjection().NoXss();

        RuleFor(x => x.Request.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(160).WithMessage("Message cannot exceed 160 characters")
            .NoSqlInjection().NoXss();
    }
}