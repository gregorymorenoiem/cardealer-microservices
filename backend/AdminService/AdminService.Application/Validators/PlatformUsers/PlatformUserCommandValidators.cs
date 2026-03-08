using FluentValidation;
using AdminService.Application.UseCases.PlatformUsers;

namespace AdminService.Application.Validators.PlatformUsers;

/// <summary>
/// Validator for UpdatePlatformUserStatusCommand.
/// Validates Status and Reason with NoSqlInjection/NoXss.
/// </summary>
public class UpdatePlatformUserStatusCommandValidator : AbstractValidator<UpdatePlatformUserStatusCommand>
{
    private static readonly string[] ValidStatuses = { "Active", "Suspended", "Banned", "PendingVerification" };

    public UpdatePlatformUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => ValidStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Reason)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}
