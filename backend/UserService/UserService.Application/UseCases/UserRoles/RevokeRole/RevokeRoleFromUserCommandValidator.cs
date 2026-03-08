using FluentValidation;
using UserService.Application.Validators;

namespace UserService.Application.UseCases.UserRoles.RevokeRole;

public class RevokeRoleFromUserCommandValidator : AbstractValidator<RevokeRoleFromUserCommand>
{
    public RevokeRoleFromUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId is required");

        RuleFor(x => x.RevokedBy)
            .NotEmpty().WithMessage("RevokedBy is required")
            .MaximumLength(100)
            .NoSqlInjection()
            .NoXss();
    }
}
