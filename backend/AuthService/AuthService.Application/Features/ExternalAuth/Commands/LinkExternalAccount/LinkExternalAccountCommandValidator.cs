using FluentValidation;

namespace AuthService.Application.Features.ExternalAuth.Commands.LinkExternalAccount;

public class LinkExternalAccountCommandValidator : AbstractValidator<LinkExternalAccountCommand>
{
    public LinkExternalAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

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