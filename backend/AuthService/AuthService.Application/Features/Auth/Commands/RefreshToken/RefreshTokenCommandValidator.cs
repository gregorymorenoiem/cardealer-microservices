using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.")
            .NoSqlInjection()
            .NoXss();
    }
}
