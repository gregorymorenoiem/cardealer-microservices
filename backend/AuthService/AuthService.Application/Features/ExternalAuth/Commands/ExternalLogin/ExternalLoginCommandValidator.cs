using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.ExternalAuth.Commands.ExternalLogin;

/// <summary>
/// Validator for ExternalLoginCommand.
/// Validates provider and redirect URI with security checks.
/// </summary>
public class ExternalLoginCommandValidator : AbstractValidator<ExternalLoginCommand>
{
    private static readonly string[] ValidProviders = { "Google", "Microsoft", "Facebook", "Apple" };

    public ExternalLoginCommandValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required.")
            .Must(BeValidProvider)
            .WithMessage("Invalid provider. Supported providers: Google, Microsoft, Facebook, Apple.")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.RedirectUri)
            .NotEmpty().WithMessage("Redirect URI is required.")
            .MaximumLength(2048).WithMessage("Redirect URI is too long.")
            .NoSqlInjection()
            .NoXss();
    }

    private static bool BeValidProvider(string provider)
    {
        return !string.IsNullOrEmpty(provider) &&
               ValidProviders.Any(p => p.Equals(provider, StringComparison.OrdinalIgnoreCase));
    }
}
