using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.Auth.Queries.GetActiveSessions;

public class GetActiveSessionsQueryValidator : AbstractValidator<GetActiveSessionsQuery>
{
    public GetActiveSessionsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID de usuario es requerido")
            .MaximumLength(128).WithMessage("El ID de usuario no puede exceder 128 caracteres")
            .NoXss()
            .NoSqlInjection();

        RuleFor(x => x.CurrentSessionId)
            .MaximumLength(256).WithMessage("El ID de sesión actual no puede exceder 256 caracteres")
            .NoXss()
            .NoSqlInjection()
            .When(x => !string.IsNullOrEmpty(x.CurrentSessionId));
    }
}
