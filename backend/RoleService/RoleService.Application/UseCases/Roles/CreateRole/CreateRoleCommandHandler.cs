using MediatR;
using Microsoft.Extensions.Logging;
using RoleService.Application.DTOs.Roles;
using RoleService.Domain.Entities;
using RoleService.Domain.Interfaces;
using RoleService.Shared.Exceptions;
using RoleService.Application.Interfaces;

namespace RoleService.Application.UseCases.Roles.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, CreateRoleResponse>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IAuditServiceClient _auditClient;
    private readonly INotificationServiceClient _notificationClient;
    private readonly IUserContextService _userContext;
    private readonly ILogger<CreateRoleCommandHandler> _logger;

    public CreateRoleCommandHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IRolePermissionRepository rolePermissionRepository,
        IAuditServiceClient auditClient,
        INotificationServiceClient notificationClient,
        IUserContextService userContext,
        ILogger<CreateRoleCommandHandler> logger)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _auditClient = auditClient;
        _notificationClient = notificationClient;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<CreateRoleResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        
        // Paso 1: Verificar que no exista un rol con el mismo nombre
        var existingRole = await _roleRepository.GetByNameAsync(req.Name, cancellationToken);
        if (existingRole != null)
        {
            _logger.LogWarning("Attempted to create duplicate role: {RoleName}", req.Name);
            throw new ConflictException($"Role with name '{req.Name}' already exists", "ROLE_EXISTS");
        }

        // Paso 2: Validar que los permisos existen (si se proporcionaron)
        var permissionIds = req.PermissionIds ?? new List<Guid>();
        foreach (var permissionId in permissionIds)
        {
            var permission = await _permissionRepository.GetByIdAsync(permissionId, cancellationToken);
            if (permission == null)
            {
                _logger.LogWarning("Invalid permission ID in role creation: {PermissionId}", permissionId);
                throw new BadRequestException($"Permission with ID '{permissionId}' not found", "INVALID_PERMISSION");
            }
        }

        // Paso 3: Crear la entidad Role
        var currentUserId = _userContext.GetCurrentUserId();
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            DisplayName = req.DisplayName,
            Description = req.Description ?? string.Empty,
            Priority = 50, // Priority por defecto (medio)
            IsActive = req.IsActive,
            IsSystemRole = false, // Los roles creados manualmente nunca son del sistema
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        // Paso 4: Guardar el rol en la base de datos
        await _roleRepository.AddAsync(role, cancellationToken);
        _logger.LogInformation("Role created: {RoleId} - {RoleName}", role.Id, role.Name);

        // Paso 5: Asignar permisos al rol (si se proporcionaron)
        foreach (var permissionId in permissionIds)
        {
            await _rolePermissionRepository.AssignPermissionToRoleAsync(
                role.Id, 
                permissionId, 
                currentUserId, 
                cancellationToken);
        }
        
        if (permissionIds.Any())
        {
            _logger.LogInformation("Assigned {Count} permissions to role {RoleId}", 
                permissionIds.Count, role.Id);
        }

        // Paso 6: Auditoría (fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _auditClient.LogRoleCreatedAsync(role.Id, role.Name, currentUserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send audit log for role creation");
            }
        }, cancellationToken);

        // Paso 7: Notificación a admins (fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _notificationClient.SendRoleCreatedNotificationAsync("admin@okla.com.do", role.Name);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification for role creation");
            }
        }, cancellationToken);

        // Paso 8: Retornar respuesta
        return new CreateRoleResponse
        {
            Success = true,
            Data = new RoleCreatedData
            {
                Id = role.Id,
                Name = role.Name,
                DisplayName = role.DisplayName,
                Description = role.Description,
                IsActive = role.IsActive,
                PermissionCount = permissionIds.Count,
                CreatedAt = role.CreatedAt
            }
        };
    }
}
