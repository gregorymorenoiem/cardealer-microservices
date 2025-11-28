using AuditService.Domain.Entities;
using AuditService.Domain.Interfaces.Repositories;
using AuditService.Shared;
using AuditService.Shared.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuditService.Application.Features.Audit.Commands.CreateAudit;

public class CreateAuditCommandHandler : IRequestHandler<CreateAuditCommand, ApiResponse<string>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<CreateAuditCommandHandler> _logger;

    public CreateAuditCommandHandler(
        IAuditLogRepository auditLogRepository,
        ILogger<CreateAuditCommandHandler> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<string>> Handle(CreateAuditCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate the request
            if (string.IsNullOrWhiteSpace(request.UserId) ||
                string.IsNullOrWhiteSpace(request.Action) ||
                string.IsNullOrWhiteSpace(request.Resource))
            {
                return ApiResponse<string>.Fail("UserId, Action, and Resource are required");
            }

            // Create audit log entity
            AuditLog auditLog;

            if (request.Success)
            {
                auditLog = AuditLog.CreateSuccess(
                    request.UserId,
                    request.Action,
                    request.Resource,
                    request.UserIp,
                    request.UserAgent,
                    request.AdditionalData,
                    request.DurationMs,
                    request.CorrelationId,
                    request.ServiceName
                );
            }
            else
            {
                auditLog = AuditLog.CreateFailure(
                    request.UserId,
                    request.Action,
                    request.Resource,
                    request.UserIp,
                    request.UserAgent,
                    request.ErrorMessage ?? "Unknown error",
                    request.AdditionalData,
                    request.DurationMs,
                    request.CorrelationId,
                    request.ServiceName,
                    request.Severity
                );
            }

            // Set severity if provided
            if (request.Severity != AuditSeverity.Information)
            {
                auditLog.SetSeverity(request.Severity);
            }

            // Add to repository
            await _auditLogRepository.AddAsync(auditLog, cancellationToken);

            _logger.LogDebug("Audit log created with ID: {AuditLogId} for user {UserId}", auditLog.Id, request.UserId);

            return ApiResponse<string>.Ok(auditLog.Id, new Dictionary<string, object>
            {
                ["auditLogId"] = auditLog.Id,
                ["timestamp"] = auditLog.CreatedAt,
                ["userDisplayName"] = auditLog.GetUserDisplayName()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for user {UserId}", request.UserId);
            return ApiResponse<string>.Fail("Error creating audit log");
        }
    }
}