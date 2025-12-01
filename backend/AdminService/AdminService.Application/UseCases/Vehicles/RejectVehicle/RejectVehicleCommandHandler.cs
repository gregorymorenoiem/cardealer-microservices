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
            // TODO: Implementar lógica de rechazo del vehículo
            // Por ahora, solo simula la operación exitosa

            _logger.LogInformation("Rejecting vehicle {VehicleId} by {RejectedBy}. Reason: {Reason}",
                request.VehicleId, request.RejectedBy, request.Reason);

            // Auditoría (fire-and-forget)
            _ = _auditClient.LogVehicleRejectedAsync(request.VehicleId, request.RejectedBy, request.Reason);

            // Notificación al propietario (fire-and-forget)
            _ = _notificationClient.SendVehicleRejectedNotificationAsync(request.OwnerEmail, request.VehicleTitle, request.Reason);

            return true;
        }
    }
}
