using FluentValidation;
using MediaService.Application.Validators;

namespace MediaService.Application.Features.Media.Queries.ListMedia;

public class ListMediaQueryValidator : AbstractValidator<ListMediaQuery>
{
    private static readonly string[] ValidSortFields = { "createdAt", "updatedAt", "fileName", "fileSize", "mediaType" };
    private static readonly string[] ValidStatuses = { "pending", "processing", "ready", "failed", "deleted" };

    public ListMediaQueryValidator()
    {
        RuleFor(x => x.OwnerId)
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.OwnerId));

        RuleFor(x => x.Context)
            .MaximumLength(200)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Context));

        RuleFor(x => x.MediaType)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.MediaType));

        RuleFor(x => x.Status)
            .Must(s => string.IsNullOrEmpty(s) || ValidStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.")
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.SortBy)
            .Must(s => string.IsNullOrEmpty(s) || ValidSortFields.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"SortBy must be one of: {string.Join(", ", ValidSortFields)}.")
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.SortBy));

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200).WithMessage("PageSize must be between 1 and 200.");
    }
}
