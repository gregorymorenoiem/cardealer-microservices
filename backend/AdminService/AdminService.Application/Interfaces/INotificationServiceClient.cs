using System;
using System.Threading.Tasks;

namespace AdminService.Application.Interfaces
{
    public interface INotificationServiceClient
    {
        Task SendVehicleApprovedNotificationAsync(string ownerEmail, string vehicleTitle);
        Task SendVehicleRejectedNotificationAsync(string ownerEmail, string vehicleTitle, string reason);
        Task SendAdminAlertAsync(string adminEmail, string subject, string message);
        Task SendReportResolvedNotificationAsync(string reporterEmail, string reportSubject, string resolution);
    }
}
