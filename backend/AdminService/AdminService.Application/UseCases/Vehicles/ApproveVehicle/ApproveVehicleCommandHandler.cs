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
            // TODO: Implementar lógica de aprobación del vehículo
            // Por ahora, solo simula la operación exitosa

            _logger.LogInformation("Approving vehicle {VehicleId} by {ApprovedBy}", request.VehicleId, request.ApprovedBy);

            // Auditoría (fire-and-forget)
            _ = _auditClient.LogVehicleApprovedAsync(request.VehicleId, request.ApprovedBy, request.Reason);

            // Notificación al propietario (fire-and-forget)
            _ = _notificationClient.SendVehicleApprovedNotificationAsync(request.OwnerEmail, request.VehicleTitle);

            return true;
        }
    }
}
