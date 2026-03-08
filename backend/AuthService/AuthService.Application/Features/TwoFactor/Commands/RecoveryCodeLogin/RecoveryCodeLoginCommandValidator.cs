using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.TwoFactor.Commands.RecoveryCodeLogin;

public class RecoveryCodeLoginCommandValidator : AbstractValidator<RecoveryCodeLoginCommand>
{
    public RecoveryCodeLoginCommandValidator()
    {
        RuleFor(x => x.TempToken)
            .NotEmpty().WithMessage("El token temporal es requerido")
            .MaximumLength(512).WithMessage("El token temporal no puede exceder 512 caracteres")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.RecoveryCode)
            .NotEmpty().WithMessage("El código de recuperación es requerido")
            .MaximumLength(20).WithMessage("El código de recuperación no puede exceder 20 caracteres")
            .Matches(@"^[A-Za-z0-9]+$").WithMessage("El código de recuperación solo puede contener letras y números")
            .NoXss()
            .NoSqlInjection();
    }
}
