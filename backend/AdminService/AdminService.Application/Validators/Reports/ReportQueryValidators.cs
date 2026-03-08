using FluentValidation;
using AdminService.Application.UseCases.Reports;

namespace AdminService.Application.Validators.Reports;

/// <summary>
/// Validator for UpdateReportStatusCommand.
/// Validates Status, Resolution with NoSqlInjection/NoXss.
/// </summary>
public class UpdateReportStatusCommandValidator : AbstractValidator<UpdateReportStatusCommand>
{
    private static readonly string[] ValidStatuses = { "open", "in-review", "resolved", "dismissed", "escalated" };

    public UpdateReportStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Report ID is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => ValidStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Resolution)
            .MaximumLength(4000)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Resolution));
    }
}

/// <summary>
/// Validator for GetReportsQuery.
/// Validates Type, Status, Priority, Search filters with NoSqlInjection/NoXss.
/// </summary>
public class GetReportsQueryValidator : AbstractValidator<GetReportsQuery>
{
    public GetReportsQueryValidator()
    {
        RuleFor(x => x.Type)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Type));

        RuleFor(x => x.Status)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Priority)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Priority));

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Search));

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
