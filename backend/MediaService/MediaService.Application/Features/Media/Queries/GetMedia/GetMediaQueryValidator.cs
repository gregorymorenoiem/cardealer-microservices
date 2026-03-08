using FluentValidation;
using MediaService.Application.Validators;

namespace MediaService.Application.Features.Media.Queries.GetMedia;

public class GetMediaQueryValidator : AbstractValidator<GetMediaQuery>
{
    public GetMediaQueryValidator()
    {
        RuleFor(x => x.MediaId)
            .NotEmpty().WithMessage("Media ID is required.")
            .NoSqlInjection()
            .NoXss();
    }
}
