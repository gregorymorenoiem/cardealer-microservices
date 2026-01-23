using MediatR;
using Microsoft.Extensions.Logging;
using RoleService.Application.DTOs.Permissions;
using RoleService.Application.Interfaces;
using RoleService.Domain.Entities;
using RoleService.Domain.Enums;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;

namespace RoleService.Application.UseCases.Permissions.CreatePermission;

/// <summary>
/// Handler para crear un nuevo permiso en el sistema.
/// Los permisos siguen el formato: resource:action
/// </summary>
public class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, CreatePermissionResponse>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IAuditServiceClient _auditClient;
    private readonly IUserContextService _userContext;
    private readonly ILogger<CreatePermissionCommandHandler> _logger;

    public CreatePermissionCommandHandler(
        IPermissionRepository permissionRepository,
        IAuditServiceClient auditClient,
        IUserContextService userContext,
        ILogger<CreatePermissionCommandHandler> logger)
    {
        _permissionRepository = permissionRepository;
        _auditClient = auditClient;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<CreatePermissionResponse> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        // Paso 1: Validar que el módulo es permitido
        if (!Permission.IsValidModule(req.Module))
        {
            _logger.LogWarning("Invalid module attempted: {Module}", req.Module);
            throw new BadRequestException(
                $"Invalid module '{req.Module}'. Allowed modules: {string.Join(", ", Permission.AllowedModules)}",
                "INVALID_MODULE");
        }

        // Paso 2: Parsear la acción
        if (!Enum.TryParse<PermissionAction>(req.Action, true, out var action))
        {
            _logger.LogWarning("Invalid action attempted: {Action}", req.Action);
            throw new BadRequestException(
                $"Invalid action: {req.Action}. Valid actions: {string.Join(", ", Enum.GetNames<PermissionAction>())}",
                "INVALID_ACTION");
        }

        // Paso 3: Verificar que no existe un permiso con el mismo nombre
        var existingPermissions = await _permissionRepository.GetAllAsync(cancellationToken);
        if (existingPermissions.Any(p => p.Name.Equals(req.Name, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("Attempted to create duplicate permission: {PermissionName}", req.Name);
            throw new ConflictException($"Permission with name '{req.Name}' already exists", "PERMISSION_EXISTS");
        }

        // Paso 4: Crear la entidad Permission
        var currentUserId = _userContext.GetCurrentUserId();
        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = req.Name.ToLowerInvariant(),
            DisplayName = req.DisplayName,
            Description = req.Description ?? string.Empty,
            Resource = req.Resource.ToLowerInvariant(),
            Action = action,
            Module = req.Module.ToLowerInvariant(),
            IsActive = true,
            IsSystemPermission = false, // Los permisos creados manualmente nunca son del sistema
            CreatedAt = DateTime.UtcNow
        };

        // Paso 5: Guardar en la base de datos
        await _permissionRepository.AddAsync(permission, cancellationToken);
        _logger.LogInformation("Permission created: {PermissionId} - {PermissionName}", permission.Id, permission.Name);

        // Paso 6: Auditoría (fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _auditClient.LogPermissionCreatedAsync(permission.Id, permission.Name, currentUserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for permission creation");
            }
        }, cancellationToken);

        // Paso 7: Retornar respuesta
        return new CreatePermissionResponse
        {
            Success = true,
            Data = new PermissionCreatedData
            {
                Id = permission.Id,
                Name = permission.Name,
                DisplayName = permission.DisplayName,
                Description = permission.Description,
                Resource = permission.Resource,
                Action = permission.Action.ToString().ToLowerInvariant(),
                Module = permission.Module,
                CreatedAt = permission.CreatedAt
            }
        };
    }
}
