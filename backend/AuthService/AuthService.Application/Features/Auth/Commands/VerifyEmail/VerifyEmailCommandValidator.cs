using FluentValidation;
using AuthService.Application.Features.Auth.Commands.VerifyEmail;
using AuthService.Application.Validators;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Verification token is required.")
            .NoSqlInjection()
            .NoXss();
    }
}
