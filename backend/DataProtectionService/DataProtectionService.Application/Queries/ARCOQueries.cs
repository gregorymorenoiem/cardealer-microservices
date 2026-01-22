using MediatR;
using DataProtectionService.Application.DTOs;

namespace DataProtectionService.Application.Queries;

public record GetARCORequestByIdQuery(Guid RequestId) : IRequest<ARCORequestDto?>;

public record GetARCORequestByNumberQuery(string RequestNumber) : IRequest<ARCORequestDto?>;

public record GetUserARCORequestsQuery(
    Guid UserId,
    string? Status = null,
    string? Type = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PaginatedResult<ARCORequestDto>>;

public record GetPendingARCORequestsQuery(
    int Page = 1,
    int PageSize = 20,
    bool OverdueOnly = false
) : IRequest<PaginatedResult<ARCORequestDto>>;

public record GetARCOStatisticsQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<ARCOStatisticsDto>;

// ARCOStatisticsDto is defined in DTOs/DataProtectionDtos.cs

public record SearchARCORequestsQuery(
    string? SearchTerm = null,
    string? Status = null,
    string? Type = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PaginatedResult<ARCORequestDto>>;
