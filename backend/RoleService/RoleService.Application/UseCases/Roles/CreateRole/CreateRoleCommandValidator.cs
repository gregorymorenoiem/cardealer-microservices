using FluentValidation;

namespace RoleService.Application.UseCases.Roles.CreateRole;

/// <summary>
/// Validador para el comando de creación de rol.
/// Valida nombre, displayName y formato según estándares de seguridad.
/// </summary>
public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
                .WithMessage("Role name is required")
                .WithErrorCode("INVALID_ROLE_NAME")
            .MinimumLength(3)
                .WithMessage("Role name must be at least 3 characters")
                .WithErrorCode("INVALID_ROLE_NAME")
            .MaximumLength(50)
                .WithMessage("Role name cannot exceed 50 characters")
                .WithErrorCode("INVALID_ROLE_NAME")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9_-]*$")
                .WithMessage("Role name must start with a letter and contain only letters, numbers, underscores and hyphens")
                .WithErrorCode("INVALID_ROLE_NAME");

        RuleFor(x => x.Request.DisplayName)
            .NotEmpty()
                .WithMessage("Display name is required")
                .WithErrorCode("INVALID_DISPLAY_NAME")
            .MinimumLength(3)
                .WithMessage("Display name must be at least 3 characters")
                .WithErrorCode("INVALID_DISPLAY_NAME")
            .MaximumLength(100)
                .WithMessage("Display name cannot exceed 100 characters")
                .WithErrorCode("INVALID_DISPLAY_NAME");

        RuleFor(x => x.Request.Description)
            .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters")
                .WithErrorCode("INVALID_DESCRIPTION")
            .When(x => !string.IsNullOrEmpty(x.Request.Description));

        RuleFor(x => x.Request.PermissionIds)
            .Must(ids => ids == null || ids.Count <= 100)
                .WithMessage("Cannot assign more than 100 permissions to a role")
                .WithErrorCode("TOO_MANY_PERMISSIONS")
            .Must(ids => ids == null || ids.Distinct().Count() == ids.Count)
                .WithMessage("Duplicate permission IDs are not allowed")
                .WithErrorCode("DUPLICATE_PERMISSION_IDS");
    }
}
