using FluentValidation;
using RoleService.Domain.Entities;
using RoleService.Domain.Enums;

namespace RoleService.Application.UseCases.Permissions.CreatePermission;

/// <summary>
/// Validador para CreatePermissionCommand.
/// Valida formato de nombre (resource:action), módulo permitido y acción válida.
/// </summary>
public class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        // Validación de Name: formato resource:action
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Permission name is required")
            .MaximumLength(100).WithMessage("Permission name cannot exceed 100 characters")
            .Matches(@"^[a-z0-9]+:[a-z]+(-[a-z]+)*$")
            .WithMessage("Permission name must follow format 'resource:action' (e.g., 'users:create', 'vehicles:manage-featured')");

        // Validación de DisplayName
        RuleFor(x => x.Request.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(150).WithMessage("Display name cannot exceed 150 characters");

        // Validación de Description (opcional pero con límite)
        RuleFor(x => x.Request.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        // Validación de Resource
        RuleFor(x => x.Request.Resource)
            .NotEmpty().WithMessage("Resource is required")
            .MaximumLength(100).WithMessage("Resource cannot exceed 100 characters")
            .Matches(@"^[a-z0-9]+(-[a-z0-9]+)*$")
            .WithMessage("Resource must be lowercase alphanumeric with optional hyphens");

        // Validación de Action: debe ser un valor válido del enum
        RuleFor(x => x.Request.Action)
            .NotEmpty().WithMessage("Action is required")
            .Must(BeValidAction)
            .WithMessage($"Action must be one of: {string.Join(", ", Enum.GetNames<PermissionAction>())}");

        // Validación de Module: debe estar en la lista permitida
        RuleFor(x => x.Request.Module)
            .NotEmpty().WithMessage("Module is required")
            .MaximumLength(100).WithMessage("Module cannot exceed 100 characters")
            .Must(BeValidModule)
            .WithMessage($"Module must be one of: {string.Join(", ", Permission.AllowedModules)}");

        // Validación de consistencia: Name debe coincidir con Resource:Action
        RuleFor(x => x.Request)
            .Must(req => 
            {
                if (string.IsNullOrEmpty(req.Name) || string.IsNullOrEmpty(req.Resource) || string.IsNullOrEmpty(req.Action))
                    return true; // Otras reglas capturarán esto
                
                var expectedName = $"{req.Resource.ToLowerInvariant()}:{req.Action.ToLowerInvariant()}";
                return req.Name.Equals(expectedName, StringComparison.OrdinalIgnoreCase);
            })
            .WithMessage("Permission name must match 'resource:action' format based on provided Resource and Action values");
    }

    private static bool BeValidAction(string action)
    {
        return Enum.TryParse<PermissionAction>(action, true, out _);
    }

    private static bool BeValidModule(string module)
    {
        return Permission.IsValidModule(module);
    }
}
