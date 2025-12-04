using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AdminService.Application.Interfaces;

namespace AdminService.Application.UseCases.Vehicles.ApproveVehicle
{
    public class ApproveVehicleCommandHandler : IRequestHandler<ApproveVehicleCommand, bool>
    {
        private readonly IAuditServiceClient _auditClient;
        private readonly INotificationServiceClient _notificationClient;
        private readonly ILogger<ApproveVehicleCommandHandler> _logger;

        public ApproveVehicleCommandHandler(
            IAuditServiceClient auditClient,
            INotificationServiceClient notificationClient,
            ILogger<ApproveVehicleCommandHandler> logger)
        {
            _auditClient = auditClient;
            _notificationClient = notificationClient;
            _logger = logger;
        }

        public async Task<bool> Handle(ApproveVehicleCommand request, CancellationToken cancellationToken)
        {
            // Validate input
            if (request.VehicleId == Guid.Empty)
            {
                _logger.LogWarning("Invalid VehicleId provided for approval");
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.ApprovedBy))
            {
                _logger.LogWarning("Empty ApprovedBy provided for vehicle {VehicleId}", request.VehicleId);
                return false;
            }

            _logger.LogInformation("Approving vehicle {VehicleId} by {ApprovedBy}", request.VehicleId, request.ApprovedBy);

            try
            {
                // Record audit log asynchronously (fire-and-forget with error handling)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _auditClient.LogVehicleApprovedAsync(request.VehicleId, request.ApprovedBy, request.Reason);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to log audit for vehicle approval {VehicleId}", request.VehicleId);
                    }
                }, cancellationToken);

                // Send notification to owner (fire-and-forget with error handling)
                if (!string.IsNullOrWhiteSpace(request.OwnerEmail))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _notificationClient.SendVehicleApprovedNotificationAsync(
                                request.OwnerEmail, request.VehicleTitle);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send notification for vehicle approval {VehicleId}", request.VehicleId);
                        }
                    }, cancellationToken);
                }

                _logger.LogInformation("Vehicle {VehicleId} approved successfully", request.VehicleId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving vehicle {VehicleId}", request.VehicleId);
                return false;
            }
        }
    }
}
