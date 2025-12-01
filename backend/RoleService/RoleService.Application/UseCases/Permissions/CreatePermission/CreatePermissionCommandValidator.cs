using FluentValidation;

namespace RoleService.Application.UseCases.Permissions.CreatePermission;

public class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Permission name is required")
            .MaximumLength(100).WithMessage("Permission name cannot exceed 100 characters");

        RuleFor(x => x.Request.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Request.Resource)
            .NotEmpty().WithMessage("Resource is required")
            .MaximumLength(100).WithMessage("Resource cannot exceed 100 characters");

        RuleFor(x => x.Request.Action)
            .NotEmpty().WithMessage("Action is required")
            .Must(BeValidAction).WithMessage("Action must be one of: Create, Read, Update, Delete, Execute, All");

        RuleFor(x => x.Request.Module)
            .NotEmpty().WithMessage("Module is required")
            .MaximumLength(100).WithMessage("Module cannot exceed 100 characters");
    }

    private bool BeValidAction(string action)
    {
        return Enum.TryParse<Domain.Enums.PermissionAction>(action, out _);
    }
}
