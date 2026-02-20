using AdvertisingService.Application.Validators;
using FluentValidation;

namespace AdvertisingService.Application.Features.HomepageConfig.Commands.UpdateBrand;

public class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(x => x.BrandKey)
            .NotEmpty()
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        When(x => x.DisplayName != null, () =>
        {
            RuleFor(x => x.DisplayName!).MaximumLength(100).NoSqlInjection().NoXss();
        });

        When(x => x.LogoUrl != null, () =>
        {
            RuleFor(x => x.LogoUrl!).MaximumLength(500).NoXss();
        });

        When(x => x.Route != null, () =>
        {
            RuleFor(x => x.Route!).MaximumLength(200).NoSqlInjection().NoXss();
        });
    }
}
