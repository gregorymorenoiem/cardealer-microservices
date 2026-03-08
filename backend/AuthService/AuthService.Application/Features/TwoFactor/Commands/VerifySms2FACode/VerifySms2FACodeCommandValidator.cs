using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.TwoFactor.Commands.VerifySms2FACode;

public class VerifySms2FACodeCommandValidator : AbstractValidator<VerifySms2FACodeCommand>
{
    public VerifySms2FACodeCommandValidator()
    {
        RuleFor(x => x.TempToken)
            .NotEmpty().WithMessage("El token temporal es requerido")
            .MaximumLength(512).WithMessage("El token temporal no puede exceder 512 caracteres")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código de verificación es requerido")
            .Length(6).WithMessage("El código debe tener 6 dígitos")
            .Matches(@"^\d{6}$").WithMessage("El código debe contener solo 6 dígitos")
            .NoXss()
            .NoSqlInjection();
    }
}
