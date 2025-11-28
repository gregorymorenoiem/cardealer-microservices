using FluentValidation;

namespace AuthService.Application.Features.ExternalAuth.Commands.ExternalLogin;

public class ExternalLoginCommandValidator : AbstractValidator<ExternalLoginCommand>
{
    public ExternalLoginCommandValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required")
            .Must(BeValidProvider).WithMessage("Provider must be either 'Google', 'Microsoft', or 'Facebook'");

        RuleFor(x => x.RedirectUri)
            .NotEmpty().WithMessage("Redirect URI is required")
            .Must(BeValidUri).WithMessage("Redirect URI must be a valid URL");
    }

    private bool BeValidProvider(string provider)
    {
        return provider.Equals("Google", StringComparison.OrdinalIgnoreCase) ||
               provider.Equals("Microsoft", StringComparison.OrdinalIgnoreCase) ||
               provider.Equals("Facebook", StringComparison.OrdinalIgnoreCase);
    }

    private bool BeValidUri(string uri)
    {
        return Uri.TryCreate(uri, UriKind.Absolute, out _);
    }
}