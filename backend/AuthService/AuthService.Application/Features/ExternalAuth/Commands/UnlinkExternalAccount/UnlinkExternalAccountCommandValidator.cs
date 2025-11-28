using FluentValidation;

namespace AuthService.Application.Features.ExternalAuth.Commands.UnlinkExternalAccount;

public class UnlinkExternalAccountCommandValidator : AbstractValidator<UnlinkExternalAccountCommand>
{
    public UnlinkExternalAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required")
            .Must(BeValidProvider).WithMessage("Provider must be either 'Google' or 'Microsoft'");
    }

    private bool BeValidProvider(string provider)
    {
        return provider.Equals("Google", StringComparison.OrdinalIgnoreCase) ||
               provider.Equals("Microsoft", StringComparison.OrdinalIgnoreCase);
    }
}