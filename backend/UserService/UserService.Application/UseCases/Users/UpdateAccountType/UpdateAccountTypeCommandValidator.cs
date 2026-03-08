using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.Users.UpdateAccountType;

public class UpdateAccountTypeCommandValidator : AbstractValidator<UpdateAccountTypeCommand>
{
    public UpdateAccountTypeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.AccountType)
            .NotEmpty().WithMessage("Account type is required")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.PerformedBy)
            .NotEmpty().WithMessage("PerformedBy is required")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();

        RuleFor(x => x.Reason!)
            .MaximumLength(500)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));

        RuleFor(x => x.IpAddress!)
            .MaximumLength(45)
            .NoSqlInjection()
            .When(x => !string.IsNullOrWhiteSpace(x.IpAddress));
    }
}
