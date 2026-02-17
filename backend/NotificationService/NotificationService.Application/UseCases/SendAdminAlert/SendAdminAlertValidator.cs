using FluentValidation;
using NotificationService.Application.Validators;

namespace NotificationService.Application.UseCases.SendAdminAlert;

public class SendAdminAlertValidator : AbstractValidator<SendAdminAlertCommand>
{
    private static readonly string[] ValidAlertTypes =
    {
        "new_user_registered", "new_listing_pending", "new_dealer_registered",
        "user_report", "payment_failed", "daily_summary",
        "kyc_pending_review", "system_errors"
    };

    private static readonly string[] ValidSeverities =
    {
        "Info", "Warning", "Error", "Critical"
    };

    public SendAdminAlertValidator()
    {
        RuleFor(x => x.AlertType)
            .NotEmpty().WithMessage("Alert type is required")
            .Must(x => ValidAlertTypes.Contains(x))
            .WithMessage($"Alert type must be one of: {string.Join(", ", ValidAlertTypes)}")
            .NoSqlInjection().NoXss();

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .NoSqlInjection().NoXss();

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters")
            .NoSqlInjection().NoXss();

        RuleFor(x => x.Severity)
            .Must(x => ValidSeverities.Contains(x))
            .WithMessage($"Severity must be one of: {string.Join(", ", ValidSeverities)}")
            .NoSqlInjection().NoXss();
    }
}
