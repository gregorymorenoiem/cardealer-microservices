using MediatR;
using RoleService.Application.DTOs.RolePermissions;
using RoleService.Domain.Enums;
using RoleService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace RoleService.Application.UseCases.RolePermissions.CheckPermission;

public class CheckPermissionQueryHandler : IRequestHandler<CheckPermissionQuery, CheckPermissionResponse>
{
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly ILogger<CheckPermissionQueryHandler> _logger;

    public CheckPermissionQueryHandler(
        IRolePermissionRepository rolePermissionRepository,
        ILogger<CheckPermissionQueryHandler> logger)
    {
        _rolePermissionRepository = rolePermissionRepository;
        _logger = logger;
    }

    public async Task<CheckPermissionResponse> Handle(CheckPermissionQuery request, CancellationToken cancellationToken)
    {
        // Validate action
        if (!Enum.TryParse<PermissionAction>(request.Action, true, out var action))
        {
            _logger.LogWarning("Invalid permission action requested: {Action}", request.Action);
            return new CheckPermissionResponse(false, $"Invalid action: {request.Action}");
        }

        // Validate roleIds
        if (request.RoleIds == null || !request.RoleIds.Any())
        {
            _logger.LogWarning("No role IDs provided for permission check");
            return new CheckPermissionResponse(false, "No roles provided");
        }

        // Check each role for the required permission
        foreach (var roleId in request.RoleIds)
        {
            try
            {
                var hasPermission = await _rolePermissionRepository.RoleHasPermissionAsync(
                    roleId,
                    request.Resource,
                    action,
                    cancellationToken);

                if (hasPermission)
                {
                    _logger.LogDebug(
                        "Permission granted for role {RoleId} on resource {Resource} with action {Action}",
                        roleId, request.Resource, request.Action);

                    return new CheckPermissionResponse(true, $"Permission granted via role {roleId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking permission for role {RoleId} on resource {Resource}",
                    roleId, request.Resource);
                // Continue checking other roles
            }
        }

        _logger.LogDebug(
            "Permission denied for roles [{RoleIds}] on resource {Resource} with action {Action}",
            string.Join(", ", request.RoleIds), request.Resource, request.Action);

        return new CheckPermissionResponse(false, "User does not have permission");
    }
}
