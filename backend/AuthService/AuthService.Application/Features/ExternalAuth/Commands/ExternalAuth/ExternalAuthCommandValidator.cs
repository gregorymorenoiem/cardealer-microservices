using FluentValidation;

namespace AuthService.Application.Features.ExternalAuth.Commands.ExternalAuth;

public class ExternalAuthCommandValidator : AbstractValidator<ExternalAuthCommand>
{
    public ExternalAuthCommandValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required")
            .Must(BeValidProvider).WithMessage("Provider must be either 'Google' or 'Microsoft'");

        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("ID token is required")
            .MinimumLength(10).WithMessage("Invalid ID token");
    }

    private bool BeValidProvider(string provider)
    {
        return provider.Equals("Google", StringComparison.OrdinalIgnoreCase) ||
               provider.Equals("Microsoft", StringComparison.OrdinalIgnoreCase);
    }
}
