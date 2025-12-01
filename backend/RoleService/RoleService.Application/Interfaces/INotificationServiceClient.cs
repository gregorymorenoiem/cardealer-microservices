using System;
using System.Threading.Tasks;

namespace RoleService.Application.Interfaces
{
    public interface INotificationServiceClient
    {
        Task SendRoleCreatedNotificationAsync(string adminEmail, string roleName);
        Task SendCriticalRoleChangedNotificationAsync(string adminEmail, string roleName, string action);
    }
}
