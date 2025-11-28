using FluentValidation;
using AuditService.Shared;

namespace AuditService.Application.Features.Audit.Queries.GetAuditLogs;

public class GetAuditLogsQueryValidator : AbstractValidator<GetAuditLogsQuery>
{
    public GetAuditLogsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, Constants.Pagination.MaxPageSize)
            .WithMessage($"PageSize must be between 1 and {Constants.Pagination.MaxPageSize}");

        RuleFor(x => x.FromDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("FromDate cannot be in the future")
            .When(x => x.FromDate.HasValue);

        RuleFor(x => x.ToDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("ToDate cannot be in the future")
            .When(x => x.ToDate.HasValue);

        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.FromDate <= x.ToDate)
            .WithMessage("FromDate must be less than or equal to ToDate");

        RuleFor(x => x.UserId)
            .MaximumLength(255).WithMessage("UserId must not exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.UserId));

        RuleFor(x => x.Action)
            .MaximumLength(100).WithMessage("Action must not exceed 100 characters")
            .Must(ValidationPatterns.IsValidActionName).WithMessage("Action contains invalid characters")
            .When(x => !string.IsNullOrEmpty(x.Action));

        RuleFor(x => x.Resource)
            .MaximumLength(255).WithMessage("Resource must not exceed 255 characters")
            .Must(ValidationPatterns.IsValidResourceName).WithMessage("Resource contains invalid characters")
            .When(x => !string.IsNullOrEmpty(x.Resource));

        RuleFor(x => x.ServiceName)
            .MaximumLength(50).WithMessage("ServiceName must not exceed 50 characters")
            .Must(ValidationPatterns.IsValidServiceName).WithMessage("ServiceName contains invalid characters")
            .When(x => !string.IsNullOrEmpty(x.ServiceName));

        RuleFor(x => x.SortBy)
            .Must(ValidationPatterns.IsValidSortBy).WithMessage("SortBy contains invalid characters")
            .When(x => !string.IsNullOrEmpty(x.SortBy));

        RuleFor(x => x.SearchText)
            .MaximumLength(100).WithMessage("SearchText must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchText));
    }
}