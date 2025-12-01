using System;
using System.Threading.Tasks;

namespace RoleService.Application.Interfaces
{
    public interface IAuditServiceClient
    {
        Task LogRoleCreatedAsync(Guid roleId, string roleName, string performedBy);
        Task LogRoleUpdatedAsync(Guid roleId, string changes, string performedBy);
        Task LogRoleDeletedAsync(Guid roleId, string roleName, string performedBy);
        Task LogPermissionAssignedAsync(Guid roleId, Guid permissionId, string performedBy);
        Task LogPermissionRemovedAsync(Guid roleId, Guid permissionId, string performedBy);
    }
}
