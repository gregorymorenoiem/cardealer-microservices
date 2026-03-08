using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.Users.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.FirstName!)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName!)
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.PhoneNumber!)
            .MaximumLength(20)
            .NoSqlInjection()
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
