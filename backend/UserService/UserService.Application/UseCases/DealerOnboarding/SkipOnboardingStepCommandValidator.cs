using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.DealerOnboarding;

public class SkipOnboardingStepCommandValidator : AbstractValidator<SkipOnboardingStepCommand>
{
    public SkipOnboardingStepCommandValidator()
    {
        RuleFor(x => x.DealerId)
            .NotEmpty().WithMessage("DealerId is required");

        RuleFor(x => x.Step)
            .NotEmpty().WithMessage("Step is required")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();
    }
}
