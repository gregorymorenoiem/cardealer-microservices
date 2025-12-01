namespace UserService.Application.DTOs.UserRoles
{
    /// <summary>
    /// Request para asignar un rol a un usuario
    /// </summary>
    public class AssignRoleToUserRequest
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public string AssignedBy { get; set; } = "system";
    }

    /// <summary>
    /// Request para revocar un rol de un usuario
    /// </summary>
    public class RevokeRoleFromUserRequest
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public string RevokedBy { get; set; } = "system";
    }

    /// <summary>
    /// Response con los roles de un usuario
    /// </summary>
    public class UserRolesResponse
    {
        public Guid UserId { get; set; }
        public List<UserRoleDto> Roles { get; set; } = new();
    }

    /// <summary>
    /// DTO de un rol asignado al usuario
    /// </summary>
    public class UserRoleDto
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int Priority { get; set; }
        public DateTime AssignedAt { get; set; }
        public string AssignedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
