using System;
using System.Collections.Generic;

namespace UserService.Application.DTOs
{
    public class UserRoleDto
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int Priority { get; set; }
        public DateTime AssignedAt { get; set; }
        public string AssignedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class AssignRoleRequest
    {
        public Guid RoleId { get; set; }
    }

    public class UserRolesResponse
    {
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public List<UserRoleDto> Roles { get; set; } = new();
    }

    public class CheckPermissionResponse
    {
        public bool HasPermission { get; set; }
        public List<string> GrantedByRoles { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    // DTOs para comunicación con RoleService
    public class RoleDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public bool IsSystemRole { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new();
    }

    public class PermissionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
    }
}
