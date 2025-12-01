using FluentValidation;

namespace RoleService.Application.UseCases.Roles.CreateRole;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(100).WithMessage("Role name cannot exceed 100 characters")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Role name can only contain letters, numbers, underscores and hyphens");

        RuleFor(x => x.Request.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Request.Priority)
            .GreaterThanOrEqualTo(0).WithMessage("Priority must be greater than or equal to 0")
            .LessThanOrEqualTo(100).WithMessage("Priority cannot exceed 100");
    }
}
