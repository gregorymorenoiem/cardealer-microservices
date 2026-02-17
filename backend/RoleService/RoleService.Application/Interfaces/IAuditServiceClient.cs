using System;
using System.Threading.Tasks;

namespace RoleService.Application.Interfaces
{
    /// <summary>
    /// Cliente para comunicación con AuditService.
    /// Registra todas las operaciones de auditoría del RoleService.
    /// </summary>
    public interface IAuditServiceClient
    {
        /// <summary>
        /// Registra la creación de un rol
        /// </summary>
        Task LogRoleCreatedAsync(Guid roleId, string roleName, string performedBy);
        
        /// <summary>
        /// Registra la actualización de un rol
        /// </summary>
        Task LogRoleUpdatedAsync(Guid roleId, string changes, string performedBy);
        
        /// <summary>
        /// Registra la eliminación de un rol
        /// </summary>
        Task LogRoleDeletedAsync(Guid roleId, string roleName, string performedBy);
        
        /// <summary>
        /// Registra la creación de un permiso
        /// </summary>
        Task LogPermissionCreatedAsync(Guid permissionId, string permissionName, string performedBy);
        
        /// <summary>
        /// Registra la asignación de un permiso a un rol
        /// </summary>
        Task LogPermissionAssignedAsync(Guid roleId, Guid permissionId, string performedBy);
        
        /// <summary>
        /// Registra la remoción de un permiso de un rol
        /// </summary>
        Task LogPermissionRemovedAsync(Guid roleId, Guid permissionId, string performedBy);
    }
}
