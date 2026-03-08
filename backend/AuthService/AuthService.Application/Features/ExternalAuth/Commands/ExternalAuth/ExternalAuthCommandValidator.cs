using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.ExternalAuth.Commands.ExternalAuth;

public class ExternalAuthCommandValidator : AbstractValidator<ExternalAuthCommand>
{
    public ExternalAuthCommandValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required")
            .Must(BeValidProvider).WithMessage("Provider must be either 'Google' or 'Microsoft'")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("ID token is required")
            .MinimumLength(10).WithMessage("Invalid ID token")
            .NoSqlInjection()
            .NoXss();
    }

    private bool BeValidProvider(string provider)
    {
        return provider.Equals("Google", StringComparison.OrdinalIgnoreCase) ||
               provider.Equals("Microsoft", StringComparison.OrdinalIgnoreCase);
    }
}
