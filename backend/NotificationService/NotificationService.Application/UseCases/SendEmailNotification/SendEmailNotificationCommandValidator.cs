using FluentValidation;
using NotificationService.Application.DTOs;
using NotificationService.Application.Validators;

namespace NotificationService.Application.UseCases.SendEmailNotification;

public class SendEmailNotificationCommandValidator : AbstractValidator<SendEmailNotificationCommand>
{
    public SendEmailNotificationCommandValidator()
    {
        RuleFor(x => x.Request.To)
            .NotEmpty().WithMessage("Recipient email is required")
            .EmailAddress().WithMessage("Invalid email address format")
            .NoSqlInjection().NoXss();

        RuleFor(x => x.Request.Subject)
            .NotEmpty().WithMessage("Subject is required")
            .MaximumLength(200).WithMessage("Subject cannot exceed 200 characters")
            .NoSqlInjection().NoXss();

        RuleFor(x => x.Request.Body)
            .NotEmpty().WithMessage("Body is required")
            // NOTE: NoSqlInjection() is intentionally NOT applied to Body because email bodies
            // are HTML content that legitimately contain words like "create", "select", "update"
            // as natural language or HTML tags. Applying SQL injection detection to HTML email
            // bodies causes false positives. XSS protection is still applied via NoXss().
            .NoXss();
    }
}