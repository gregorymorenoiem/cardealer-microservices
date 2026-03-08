using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.TwoFactor.Commands.SendSms2FACode;

public class SendSms2FACodeCommandValidator : AbstractValidator<SendSms2FACodeCommand>
{
    public SendSms2FACodeCommandValidator()
    {
        RuleFor(x => x.TempToken)
            .NotEmpty().WithMessage("El token temporal es requerido")
            .MaximumLength(512).WithMessage("El token temporal no puede exceder 512 caracteres")
            .NoXss()
            .NoSqlInjection();
    }
}
