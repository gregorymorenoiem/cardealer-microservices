using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AdminService.Application.Interfaces;

namespace AdminService.Application.UseCases.Vehicles.RejectVehicle
{
    public class RejectVehicleCommandHandler : IRequestHandler<RejectVehicleCommand, bool>
    {
        private readonly IAuditServiceClient _auditClient;
        private readonly INotificationServiceClient _notificationClient;
        private readonly ILogger<RejectVehicleCommandHandler> _logger;

        public RejectVehicleCommandHandler(
            IAuditServiceClient auditClient,
            INotificationServiceClient notificationClient,
            ILogger<RejectVehicleCommandHandler> logger)
        {
            _auditClient = auditClient;
            _notificationClient = notificationClient;
            _logger = logger;
        }

        public async Task<bool> Handle(RejectVehicleCommand request, CancellationToken cancellationToken)
        {
            // Validate input
            if (request.VehicleId == Guid.Empty)
            {
                _logger.LogWarning("Invalid VehicleId provided for rejection");
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                _logger.LogWarning("Empty rejection reason provided for vehicle {VehicleId}", request.VehicleId);
                return false;
            }

            _logger.LogInformation("Rejecting vehicle {VehicleId} by {RejectedBy}. Reason: {Reason}",
                request.VehicleId, request.RejectedBy, request.Reason);

            try
            {
                // Record audit log — awaited safely (non-critical, errors logged but not propagated)
                try
                {
                    await _auditClient.LogVehicleRejectedAsync(request.VehicleId, request.RejectedBy, request.Reason);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to log audit for vehicle rejection {VehicleId}", request.VehicleId);
                }

                // Send notification to owner with rejection reason (non-critical, errors logged but not propagated)
                if (!string.IsNullOrWhiteSpace(request.OwnerEmail))
                {
                    try
                    {
                        await _notificationClient.SendVehicleRejectedNotificationAsync(
                            request.OwnerEmail, request.VehicleTitle, request.Reason);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send notification for vehicle rejection {VehicleId}", request.VehicleId);
                    }
                }

                _logger.LogInformation("Vehicle {VehicleId} rejected successfully", request.VehicleId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting vehicle {VehicleId}", request.VehicleId);
                return false;
            }
        }
    }
}
