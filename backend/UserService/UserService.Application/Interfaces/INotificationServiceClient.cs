using System;
using System.Threading.Tasks;

namespace UserService.Application.Interfaces
{
    public interface INotificationServiceClient
    {
        Task SendWelcomeEmailAsync(string email, string firstName, string lastName);
        Task SendRoleAssignedNotificationAsync(string email, string roleName);
        Task SendPasswordResetEmailAsync(string email, string resetToken);
    }
}
