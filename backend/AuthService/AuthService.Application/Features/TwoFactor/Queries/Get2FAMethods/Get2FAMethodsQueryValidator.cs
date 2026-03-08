using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.TwoFactor.Queries.Get2FAMethods;

public class Get2FAMethodsQueryValidator : AbstractValidator<Get2FAMethodsQuery>
{
    public Get2FAMethodsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID de usuario es requerido")
            .MaximumLength(128).WithMessage("El ID de usuario no puede exceder 128 caracteres")
            .NoXss()
            .NoSqlInjection();
    }
}
