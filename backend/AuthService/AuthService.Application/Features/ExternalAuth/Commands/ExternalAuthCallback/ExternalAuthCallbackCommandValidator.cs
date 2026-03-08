using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.ExternalAuth.Commands.ExternalAuthCallback;

public class ExternalAuthCallbackCommandValidator : AbstractValidator<ExternalAuthCallbackCommand>
{
    public ExternalAuthCallbackCommandValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("El proveedor de autenticación es requerido")
            .MaximumLength(50).WithMessage("El proveedor no puede exceder 50 caracteres")
            .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("El proveedor contiene caracteres no válidos")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.Code)
            .MaximumLength(2048).WithMessage("El código de autorización no puede exceder 2048 caracteres")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.Code));

        RuleFor(x => x.IdToken)
            .MaximumLength(4096).WithMessage("El ID token no puede exceder 4096 caracteres")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.IdToken));

        RuleFor(x => x.RedirectUri)
            .MaximumLength(2048).WithMessage("La URL de redirección no puede exceder 2048 caracteres")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.RedirectUri));

        RuleFor(x => x.State)
            .MaximumLength(1024).WithMessage("El state no puede exceder 1024 caracteres")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.State));
    }
}
