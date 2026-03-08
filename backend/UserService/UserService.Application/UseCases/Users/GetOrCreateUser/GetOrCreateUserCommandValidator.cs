using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.Users.GetOrCreateUser;

public class GetOrCreateUserCommandValidator : AbstractValidator<GetOrCreateUserCommand>
{
    public GetOrCreateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(254)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.AvatarUrl!)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl));
    }
}
