using FluentValidation;
using AdminService.Application.UseCases.Moderation;

namespace AdminService.Application.Validators.Moderation;

/// <summary>
/// Validator for ProcessModerationActionCommand.
/// Validates Action, ReviewerId, Reason, Notes with NoSqlInjection/NoXss.
/// </summary>
public class ProcessModerationActionCommandValidator : AbstractValidator<ProcessModerationActionCommand>
{
    private static readonly string[] ValidActions = { "approve", "reject", "flag", "escalate", "dismiss" };

    public ProcessModerationActionCommandValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required.");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required.")
            .Must(action => ValidActions.Contains(action, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Action must be one of: {string.Join(", ", ValidActions)}.")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.ReviewerId)
            .NotEmpty().WithMessage("Reviewer ID is required.")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Reason)
            .MaximumLength(2000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Reason));

        RuleFor(x => x.Notes)
            .MaximumLength(4000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for GetModerationQueueQuery.
/// Validates Type, Priority, Status filters with NoSqlInjection/NoXss.
/// </summary>
public class GetModerationQueueQueryValidator : AbstractValidator<GetModerationQueueQuery>
{
    public GetModerationQueueQueryValidator()
    {
        RuleFor(x => x.Type)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Type));

        RuleFor(x => x.Priority)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Priority));

        RuleFor(x => x.Status)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
