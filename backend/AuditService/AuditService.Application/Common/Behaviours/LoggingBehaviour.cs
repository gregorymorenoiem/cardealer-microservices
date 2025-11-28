using MediatR;
using Microsoft.Extensions.Logging;
using AuditService.Application.Features.Audit.Commands.CreateAudit;
using AuditService.Shared;
using AuditService.Shared.Enums;

namespace AuditService.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;
    private readonly IMediator _mediator;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestType = request.GetType().Name;

        _logger.LogInformation("Handling {RequestName} ({RequestType})", requestName, requestType);

        try
        {
            var startTime = DateTime.UtcNow;
            var response = await next();
            var duration = DateTime.UtcNow - startTime;

            // Log successful operation
            await LogAuditEvent(request, response, duration.TotalMilliseconds, true, null);

            _logger.LogInformation("Handled {RequestName} successfully in {Duration}ms",
                requestName, duration.TotalMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            // Log failed operation
            await LogAuditEvent(request, default, 0, false, ex.Message);

            _logger.LogError(ex, "Error handling {RequestName}", requestName);
            throw;
        }
    }

    private async Task LogAuditEvent(TRequest request, TResponse? response, double durationMs, bool success, string? errorMessage)
    {
        try
        {
            var userId = "system"; // In a real scenario, get from HttpContext or claims
            var userIp = "127.0.0.1"; // In a real scenario, get from HttpContext
            var userAgent = "AuditService"; // In a real scenario, get from HttpContext

            var additionalData = new Dictionary<string, object>
            {
                ["requestType"] = typeof(TRequest).Name,
                ["responseType"] = typeof(TResponse).Name,
                ["durationMs"] = durationMs,
                ["success"] = success,
                ["timestamp"] = DateTime.UtcNow
            };

            if (response is ApiResponse apiResponse)
            {
                additionalData["apiResponseSuccess"] = apiResponse.Success;
                if (!string.IsNullOrEmpty(apiResponse.Error))
                {
                    additionalData["apiResponseError"] = apiResponse.Error;
                }
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                additionalData["error"] = errorMessage;
            }

            // Create audit command
            var auditCommand = new CreateAuditCommand(
                userId: userId,
                action: $"COMMAND_{typeof(TRequest).Name.ToUpper()}",
                resource: typeof(TRequest).FullName ?? "Unknown",
                userIp: userIp,
                userAgent: userAgent,
                additionalData: additionalData,
                success: success,
                errorMessage: errorMessage,
                durationMs: (long)durationMs,
                serviceName: "AuditService",
                severity: success ? AuditSeverity.Information : AuditSeverity.Error
            );

            await _mediator.Send(auditCommand);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit event for {RequestType}", typeof(TRequest).Name);
            // Don't throw - we don't want to break the main flow if auditing fails
        }
    }
}