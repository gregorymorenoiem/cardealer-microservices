using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.Auth.Commands.RequestPasswordSetup;

/// <summary>
/// Validator for RequestPasswordSetupCommand (AUTH-PWD-001)
/// </summary>
public class RequestPasswordSetupCommandValidator : AbstractValidator<RequestPasswordSetupCommand>
{
    public RequestPasswordSetupCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.")
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.")
            .NoSqlInjection()
            .NoXss();
    }
}
