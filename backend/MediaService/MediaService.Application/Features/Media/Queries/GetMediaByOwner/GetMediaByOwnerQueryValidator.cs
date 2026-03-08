using FluentValidation;
using MediaService.Application.Validators;

namespace MediaService.Application.Features.Media.Queries.GetMediaByOwner;

public class GetMediaByOwnerQueryValidator : AbstractValidator<GetMediaByOwnerQuery>
{
    public GetMediaByOwnerQueryValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.")
            .NoSqlInjection()
            .NoXss();

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

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200).WithMessage("PageSize must be between 1 and 200.");
    }
}
