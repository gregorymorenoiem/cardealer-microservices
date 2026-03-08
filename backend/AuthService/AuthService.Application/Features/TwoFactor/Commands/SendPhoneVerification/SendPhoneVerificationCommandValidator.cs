using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.TwoFactor.Commands.SendPhoneVerification;

public class SendPhoneVerificationCommandValidator : AbstractValidator<SendPhoneVerificationCommand>
{
    public SendPhoneVerificationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID de usuario es requerido")
            .MaximumLength(128).WithMessage("El ID de usuario no puede exceder 128 caracteres")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("El número de teléfono es requerido")
            .Matches(@"^[+]?[\d\s\-()]{7,20}$").WithMessage("Formato de teléfono inválido")
            .NoXss()
            .NoSqlInjection();
    }
}
