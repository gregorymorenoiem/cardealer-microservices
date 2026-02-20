using AdvertisingService.Application.Validators;
using FluentValidation;

namespace AdvertisingService.Application.Features.HomepageConfig.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryKey)
            .NotEmpty()
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();

        When(x => x.DisplayName != null, () =>
        {
            RuleFor(x => x.DisplayName!).MaximumLength(100).NoSqlInjection().NoXss();
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description!).MaximumLength(300).NoSqlInjection().NoXss();
        });

        When(x => x.ImageUrl != null, () =>
        {
            RuleFor(x => x.ImageUrl!).MaximumLength(500).NoXss();
        });

        When(x => x.Route != null, () =>
        {
            RuleFor(x => x.Route!).MaximumLength(200).NoSqlInjection().NoXss();
        });
    }
}
