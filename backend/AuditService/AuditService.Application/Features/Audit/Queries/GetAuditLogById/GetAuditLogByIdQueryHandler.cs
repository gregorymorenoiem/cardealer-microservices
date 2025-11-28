using AuditService.Application.DTOs;
using AuditService.Domain.Interfaces.Repositories;
using AuditService.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuditService.Application.Features.Audit.Queries.GetAuditLogById;

public class GetAuditLogByIdQueryHandler : IRequestHandler<GetAuditLogByIdQuery, ApiResponse<AuditLogDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<GetAuditLogByIdQueryHandler> _logger;

    public GetAuditLogByIdQueryHandler(
        IAuditLogRepository auditLogRepository,
        ILogger<GetAuditLogByIdQueryHandler> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<AuditLogDto>> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                return ApiResponse<AuditLogDto>.Fail("Audit log ID is required");
            }

            if (!ValidationPatterns.IsValidGuid(request.Id))
            {
                return ApiResponse<AuditLogDto>.Fail("Invalid audit log ID format");
            }

            var auditLog = await _auditLogRepository.GetByIdAsync(request.Id, cancellationToken);

            if (auditLog == null)
            {
                return ApiResponse<AuditLogDto>.Fail("Audit log not found");
            }

            var auditLogDto = AuditLogDto.FromEntity(auditLog);

            return ApiResponse<AuditLogDto>.Ok(auditLogDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log with ID: {AuditLogId}", request.Id);
            return ApiResponse<AuditLogDto>.Fail("Error retrieving audit log");
        }
    }
}