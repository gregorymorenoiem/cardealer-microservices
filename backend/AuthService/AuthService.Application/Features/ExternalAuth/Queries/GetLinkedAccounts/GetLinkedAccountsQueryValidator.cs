using FluentValidation;
using AuthService.Application.Validators;

namespace AuthService.Application.Features.ExternalAuth.Queries.GetLinkedAccounts;

public class GetLinkedAccountsQueryValidator : AbstractValidator<GetLinkedAccountsQuery>
{
    public GetLinkedAccountsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID de usuario es requerido")
            .MaximumLength(128).WithMessage("El ID de usuario no puede exceder 128 caracteres")
            .NoXss()
            .NoSqlInjection();
    }
}
