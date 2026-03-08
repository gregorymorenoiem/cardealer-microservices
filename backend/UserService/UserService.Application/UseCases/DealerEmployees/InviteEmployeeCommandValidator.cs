using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.DealerEmployees;

public class InviteEmployeeCommandValidator : AbstractValidator<InviteEmployeeCommand>
{
    public InviteEmployeeCommandValidator()
    {
        RuleFor(x => x.DealerId)
            .NotEmpty().WithMessage("DealerId is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss();
    }
}
