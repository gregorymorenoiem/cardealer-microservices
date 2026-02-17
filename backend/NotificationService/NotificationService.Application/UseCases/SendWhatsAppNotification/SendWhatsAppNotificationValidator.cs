using FluentValidation;
using NotificationService.Application.Validators;

namespace NotificationService.Application.UseCases.SendWhatsAppNotification;

public class SendWhatsAppNotificationValidator : AbstractValidator<SendWhatsAppNotificationCommand>
{
    public SendWhatsAppNotificationValidator()
    {
        RuleFor(x => x.To)
            .NotEmpty().WithMessage("Recipient phone number is required")
            .MaximumLength(30).WithMessage("Phone number must not exceed 30 characters")
            .NoSqlInjection().NoXss();

        // Either Message or TemplateName must be provided
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Message) || !string.IsNullOrWhiteSpace(x.TemplateName))
            .WithMessage("Either Message or TemplateName must be provided");

        When(x => !string.IsNullOrWhiteSpace(x.Message), () =>
        {
            RuleFor(x => x.Message)
                .MaximumLength(4096).WithMessage("WhatsApp message must not exceed 4096 characters")
                .NoSqlInjection().NoXss();
        });

        When(x => !string.IsNullOrWhiteSpace(x.TemplateName), () =>
        {
            RuleFor(x => x.TemplateName)
                .MaximumLength(100).WithMessage("Template name must not exceed 100 characters")
                .NoSqlInjection().NoXss();
        });
    }
}
