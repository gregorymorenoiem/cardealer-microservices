using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.TwoFactor.Commands.TwoFactorLogin;

public class TwoFactorLoginCommandValidator : AbstractValidator<TwoFactorLoginCommand>
{
    public TwoFactorLoginCommandValidator()
    {
        RuleFor(x => x.TempToken)
            .NotEmpty().WithMessage("El token temporal es requerido")
            .MaximumLength(512).WithMessage("El token temporal no puede exceder 512 caracteres")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.TwoFactorCode)
            .NotEmpty().WithMessage("El código 2FA es requerido")
            .MaximumLength(10).WithMessage("El código 2FA no puede exceder 10 caracteres")
            .Matches(@"^[\d]+$").WithMessage("El código 2FA debe contener solo dígitos")
            .NoXss()
            .NoSqlInjection();
    }
}
