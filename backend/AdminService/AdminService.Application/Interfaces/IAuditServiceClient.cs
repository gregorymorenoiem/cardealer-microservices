using System;
using System.Threading.Tasks;

namespace AdminService.Application.Interfaces
{
    public interface IAuditServiceClient
    {
        Task LogVehicleApprovedAsync(Guid vehicleId, string approvedBy, string reason);
        Task LogVehicleRejectedAsync(Guid vehicleId, string rejectedBy, string reason);
        Task LogReportResolvedAsync(Guid reportId, string resolvedBy, string resolution);
        Task LogUserActionAsync(Guid userId, string action, string performedBy, string details);
        Task LogSystemConfigChangedAsync(string configKey, string oldValue, string newValue, string changedBy);
    }
}
