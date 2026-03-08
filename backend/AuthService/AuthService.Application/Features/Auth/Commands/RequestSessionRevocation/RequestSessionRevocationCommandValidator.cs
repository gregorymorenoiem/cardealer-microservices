using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.Auth.Commands.RequestSessionRevocation;

public class RequestSessionRevocationCommandValidator : AbstractValidator<RequestSessionRevocationCommand>
{
    public RequestSessionRevocationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID de usuario es requerido")
            .MaximumLength(128).WithMessage("El ID de usuario no puede exceder 128 caracteres")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("El ID de sesión es requerido")
            .MaximumLength(256).WithMessage("El ID de sesión no puede exceder 256 caracteres")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.CurrentSessionId)
            .MaximumLength(256).WithMessage("El ID de sesión actual no puede exceder 256 caracteres")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.CurrentSessionId));

        RuleFor(x => x.IpAddress)
            .MaximumLength(45).WithMessage("La dirección IP no puede exceder 45 caracteres")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.IpAddress));

        RuleFor(x => x.UserAgent)
            .MaximumLength(512).WithMessage("El User-Agent no puede exceder 512 caracteres")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.UserAgent));
    }
}
