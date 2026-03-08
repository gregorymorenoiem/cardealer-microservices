using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.UserRoles.AssignRole;

public class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId is required");

        RuleFor(x => x.AssignedBy)
            .NotEmpty().WithMessage("AssignedBy is required")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();
    }
}
