using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.DealerEmployees;

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.DealerId)
            .NotEmpty().WithMessage("DealerId is required");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("EmployeeId is required");

        RuleFor(x => x.Role!)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.Role));

        RuleFor(x => x.Status!)
            .MaximumLength(50)
            .NoSqlInjection()
            .NoXss()
            .When(x => !string.IsNullOrWhiteSpace(x.Status));
    }
}
