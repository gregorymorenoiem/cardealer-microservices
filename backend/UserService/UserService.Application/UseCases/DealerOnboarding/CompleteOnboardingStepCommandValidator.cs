using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.DealerOnboarding;

public class CompleteOnboardingStepCommandValidator : AbstractValidator<CompleteOnboardingStepCommand>
{
    public CompleteOnboardingStepCommandValidator()
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
