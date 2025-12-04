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
            // Validate input
            if (request.ReportId == Guid.Empty)
            {
                _logger.LogWarning("Invalid ReportId provided for resolution");
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.Resolution))
            {
                _logger.LogWarning("Empty resolution provided for report {ReportId}", request.ReportId);
                return false;
            }

            _logger.LogInformation("Resolving report {ReportId} by {ResolvedBy}. Resolution: {Resolution}",
                request.ReportId, request.ResolvedBy, request.Resolution);

            try
            {
                // Record audit log asynchronously (fire-and-forget with error handling)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _auditClient.LogReportResolvedAsync(request.ReportId, request.ResolvedBy, request.Resolution);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to log audit for report resolution {ReportId}", request.ReportId);
                    }
                }, cancellationToken);

                // Send notification to reporter (fire-and-forget with error handling)
                if (!string.IsNullOrWhiteSpace(request.ReporterEmail))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _notificationClient.SendReportResolvedNotificationAsync(
                                request.ReporterEmail, request.ReportSubject, request.Resolution);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send notification for report resolution {ReportId}", request.ReportId);
                        }
                    }, cancellationToken);
                }

                _logger.LogInformation("Report {ReportId} resolved successfully", request.ReportId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving report {ReportId}", request.ReportId);
                return false;
            }
        }
    }
}
