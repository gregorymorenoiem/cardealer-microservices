using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.TwoFactor.Commands.Add2FAMethod;

public class Add2FAMethodCommandValidator : AbstractValidator<Add2FAMethodCommand>
{
    public Add2FAMethodCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID de usuario es requerido")
            .MaximumLength(128).WithMessage("El ID de usuario no puede exceder 128 caracteres")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
            .MaximumLength(128).WithMessage("La contraseña no puede exceder 128 caracteres")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.Method)
            .IsInEnum().WithMessage("El método 2FA no es válido");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^[+]?[\d\s\-()]{7,20}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Formato de teléfono inválido")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
