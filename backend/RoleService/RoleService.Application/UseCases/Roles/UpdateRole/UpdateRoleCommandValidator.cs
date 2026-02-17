using FluentValidation;

namespace RoleService.Application.UseCases.Roles.UpdateRole;

/// <summary>
/// Validador para UpdateRoleCommand.
/// Valida que los campos opcionales tengan formato correcto cuando se proporcionan.
/// </summary>
public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        // DisplayName: Opcional, pero si se proporciona debe ser válido
        RuleFor(x => x.Request.DisplayName)
            .MinimumLength(3)
                .WithMessage("Display name must be at least 3 characters")
                .WithErrorCode("INVALID_DISPLAY_NAME")
            .MaximumLength(100)
                .WithMessage("Display name cannot exceed 100 characters")
                .WithErrorCode("INVALID_DISPLAY_NAME")
            .When(x => !string.IsNullOrEmpty(x.Request.DisplayName));

        // Description: Opcional, pero con límite de longitud
        RuleFor(x => x.Request.Description)
            .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters")
                .WithErrorCode("INVALID_DESCRIPTION")
            .When(x => !string.IsNullOrEmpty(x.Request.Description));

        // PermissionIds: Validar límites y duplicados
        RuleFor(x => x.Request.PermissionIds)
            .Must(ids => ids == null || ids.Count <= 100)
                .WithMessage("Cannot assign more than 100 permissions to a role")
                .WithErrorCode("TOO_MANY_PERMISSIONS")
            .Must(ids => ids == null || ids.Distinct().Count() == ids.Count)
                .WithMessage("Duplicate permission IDs are not allowed")
                .WithErrorCode("DUPLICATE_PERMISSION_IDS");

        // RoleId: Debe ser un GUID válido (no vacío)
        RuleFor(x => x.RoleId)
            .NotEmpty()
                .WithMessage("Role ID is required")
                .WithErrorCode("INVALID_ROLE_ID");
    }
}
