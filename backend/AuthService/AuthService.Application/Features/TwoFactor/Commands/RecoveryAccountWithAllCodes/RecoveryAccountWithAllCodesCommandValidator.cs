using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.TwoFactor.Commands.RecoveryAccountWithAllCodes;

public class RecoveryAccountWithAllCodesCommandValidator : AbstractValidator<RecoveryAccountWithAllCodesCommand>
{
    public RecoveryAccountWithAllCodesCommandValidator()
    {
        RuleFor(x => x.TempToken)
            .NotEmpty().WithMessage("El token temporal es requerido")
            .MaximumLength(512).WithMessage("El token temporal no puede exceder 512 caracteres")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.RecoveryCodes)
            .NotNull().WithMessage("Los códigos de recuperación son requeridos")
            .Must(codes => codes != null && codes.Count == 10)
                .WithMessage("Se requieren exactamente 10 códigos de recuperación");

        RuleForEach(x => x.RecoveryCodes)
            .NotEmpty().WithMessage("Cada código de recuperación no puede estar vacío")
            .MaximumLength(20).WithMessage("Cada código de recuperación no puede exceder 20 caracteres")
            .Matches(@"^[A-Za-z0-9]+$").WithMessage("Los códigos de recuperación solo pueden contener letras y números")
            .NoXss()
            .NoSqlInjection();
    }
}
