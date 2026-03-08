using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.Auth.Commands.ResendVerification;

public class ResendVerificationCommandValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("Formato de email inválido")
            .MaximumLength(254).WithMessage("El email no puede exceder 254 caracteres")
            .NoXss()
            .NoSqlInjection();
    }
}
