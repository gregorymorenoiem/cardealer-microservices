using AuditService.Application.DTOs;
using AuditService.Domain.Interfaces.Repositories;
using AuditService.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuditService.Application.Features.Audit.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, ApiResponse<PaginatedResult<AuditLogDto>>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<GetAuditLogsQueryHandler> _logger;

    public GetAuditLogsQueryHandler(
        IAuditLogRepository auditLogRepository,
        ILogger<GetAuditLogsQueryHandler> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<PaginatedResult<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate pagination parameters
            if (request.Page < 1) request.Page = 1;
            if (request.PageSize < 1 || request.PageSize > Constants.Pagination.MaxPageSize)
                request.PageSize = Constants.Pagination.DefaultPageSize;

            // Get paginated results from repository
            var (items, totalCount) = await _auditLogRepository.GetPaginatedAsync(
                userId: request.UserId,
                action: request.Action,
                resource: request.Resource,
                fromDate: request.FromDate,
                toDate: request.ToDate,
                page: request.Page,
                pageSize: request.PageSize,
                sortBy: request.SortBy,
                sortDescending: request.SortDescending,
                searchText: request.SearchText,
                serviceName: request.ServiceName,
                severity: request.Severity,
                success: request.Success);

            // Convert entities to DTOs
            var auditLogDtos = items.Select(AuditLogDto.FromEntity).ToList();

            // Create paginated result using constructor instead of Create method
            var result = new PaginatedResult<AuditLogDto>
            {
                Items = auditLogDtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                SortBy = request.SortBy,
                SortDescending = request.SortDescending
            };

            // Prepare metadata
            var metadata = new Dictionary<string, object>
            {
                ["totalCount"] = totalCount,
                ["filteredCount"] = auditLogDtos.Count,
                ["currentPage"] = request.Page,
                ["totalPages"] = result.TotalPages,
                ["hasPreviousPage"] = result.HasPreviousPage,
                ["hasNextPage"] = result.HasNextPage
            };

            if (!string.IsNullOrEmpty(request.UserId))
                metadata["filteredByUserId"] = request.UserId;
            if (!string.IsNullOrEmpty(request.Action))
                metadata["filteredByAction"] = request.Action;
            if (!string.IsNullOrEmpty(request.Resource))
                metadata["filteredByResource"] = request.Resource;
            if (request.FromDate.HasValue)
                metadata["filteredFromDate"] = request.FromDate.Value;
            if (request.ToDate.HasValue)
                metadata["filteredToDate"] = request.ToDate.Value;

            return ApiResponse<PaginatedResult<AuditLogDto>>.Ok(result, metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs with filters: {@Filters}", request);
            return ApiResponse<PaginatedResult<AuditLogDto>>.Fail("Error retrieving audit logs");
        }
    }
}