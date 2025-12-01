using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AdminService.Application.Interfaces;

namespace AdminService.Application.UseCases.Reports.ResolveReport
{
    public class ResolveReportCommandHandler : IRequestHandler<ResolveReportCommand, bool>
    {
        private readonly IAuditServiceClient _auditClient;
        private readonly INotificationServiceClient _notificationClient;
        private readonly ILogger<ResolveReportCommandHandler> _logger;

        public ResolveReportCommandHandler(
            IAuditServiceClient auditClient,
            INotificationServiceClient notificationClient,
            ILogger<ResolveReportCommandHandler> logger)
        {
            _auditClient = auditClient;
            _notificationClient = notificationClient;
            _logger = logger;
        }

        public async Task<bool> Handle(ResolveReportCommand request, CancellationToken cancellationToken)
        {
            // TODO: Implementar lógica de resolución del reporte
            // Por ahora, solo simula la operación exitosa

            _logger.LogInformation("Resolving report {ReportId} by {ResolvedBy}. Resolution: {Resolution}",
                request.ReportId, request.ResolvedBy, request.Resolution);

            // Auditoría (fire-and-forget)
            _ = _auditClient.LogReportResolvedAsync(request.ReportId, request.ResolvedBy, request.Resolution);

            // Notificación al reportero (fire-and-forget)
            _ = _notificationClient.SendReportResolvedNotificationAsync(request.ReporterEmail, request.ReportSubject, request.Resolution);

            return true;
        }
    }
}
