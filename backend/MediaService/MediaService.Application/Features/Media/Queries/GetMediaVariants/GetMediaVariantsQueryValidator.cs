using FluentValidation;
using MediaService.Application.Validators;

namespace MediaService.Application.Features.Media.Queries.GetMediaVariants;

public class GetMediaVariantsQueryValidator : AbstractValidator<GetMediaVariantsQuery>
{
    public GetMediaVariantsQueryValidator()
    {
        RuleFor(x => x.MediaId)
            .NotEmpty().WithMessage("Media ID is required.")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.VariantName)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrEmpty(x.VariantName));
    }
}
