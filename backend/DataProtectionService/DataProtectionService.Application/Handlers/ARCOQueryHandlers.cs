using MediatR;
using DataProtectionService.Application.DTOs;
using DataProtectionService.Application.Queries;
using DataProtectionService.Domain.Interfaces;

namespace DataProtectionService.Application.Handlers;

public class GetARCORequestByIdQueryHandler : IRequestHandler<GetARCORequestByIdQuery, ARCORequestDto?>
{
    private readonly IARCORequestRepository _repository;

    public GetARCORequestByIdQueryHandler(IARCORequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<ARCORequestDto?> Handle(GetARCORequestByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await _repository.GetByIdAsync(request.RequestId, cancellationToken);
        if (r == null) return null;

        return new ARCORequestDto(
            r.Id,
            r.UserId,
            r.RequestNumber,
            r.Type.ToString(),
            r.Status.ToString(),
            r.Description,
            r.RequestedAt,
            r.Deadline,
            r.CompletedAt,
            r.ProcessedByName,
            r.Resolution,
            r.RejectionReason,
            r.ExportFileUrl,
            r.IsOverdue,
            r.DaysRemaining,
            r.Attachments?.Select(a => new ARCOAttachmentDto(
                a.Id, a.FileName, a.FileUrl, a.FileType, a.FileSize, a.UploadedAt
            )).ToList() ?? new(),
            r.StatusHistory?.Select(h => new ARCOStatusHistoryDto(
                h.OldStatus.ToString(), h.NewStatus.ToString(), h.Comment, h.ChangedByName, h.ChangedAt
            )).ToList() ?? new()
        );
    }
}

public class GetARCORequestByNumberQueryHandler : IRequestHandler<GetARCORequestByNumberQuery, ARCORequestDto?>
{
    private readonly IARCORequestRepository _repository;

    public GetARCORequestByNumberQueryHandler(IARCORequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<ARCORequestDto?> Handle(GetARCORequestByNumberQuery request, CancellationToken cancellationToken)
    {
        var r = await _repository.GetByRequestNumberAsync(request.RequestNumber, cancellationToken);
        if (r == null) return null;

        return new ARCORequestDto(
            r.Id,
            r.UserId,
            r.RequestNumber,
            r.Type.ToString(),
            r.Status.ToString(),
            r.Description,
            r.RequestedAt,
            r.Deadline,
            r.CompletedAt,
            r.ProcessedByName,
            r.Resolution,
            r.RejectionReason,
            r.ExportFileUrl,
            r.IsOverdue,
            r.DaysRemaining,
            r.Attachments?.Select(a => new ARCOAttachmentDto(
                a.Id, a.FileName, a.FileUrl, a.FileType, a.FileSize, a.UploadedAt
            )).ToList() ?? new(),
            r.StatusHistory?.Select(h => new ARCOStatusHistoryDto(
                h.OldStatus.ToString(), h.NewStatus.ToString(), h.Comment, h.ChangedByName, h.ChangedAt
            )).ToList() ?? new()
        );
    }
}

public class GetPendingARCORequestsQueryHandler : IRequestHandler<GetPendingARCORequestsQuery, PaginatedResult<ARCORequestDto>>
{
    private readonly IARCORequestRepository _repository;

    public GetPendingARCORequestsQueryHandler(IARCORequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResult<ARCORequestDto>> Handle(GetPendingARCORequestsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPendingRequestsAsync(
            request.Page, request.PageSize, request.OverdueOnly, cancellationToken);

        return new PaginatedResult<ARCORequestDto>
        {
            Items = items.Select(r => new ARCORequestDto(
                r.Id,
                r.UserId,
                r.RequestNumber,
                r.Type.ToString(),
                r.Status.ToString(),
                r.Description,
                r.RequestedAt,
                r.Deadline,
                r.CompletedAt,
                r.ProcessedByName,
                r.Resolution,
                r.RejectionReason,
                r.ExportFileUrl,
                r.IsOverdue,
                r.DaysRemaining,
                new List<ARCOAttachmentDto>(),
                new List<ARCOStatusHistoryDto>()
            )).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

public class GetARCOStatisticsQueryHandler : IRequestHandler<GetARCOStatisticsQuery, ARCOStatisticsDto>
{
    private readonly IARCORequestRepository _repository;

    public GetARCOStatisticsQueryHandler(IARCORequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<ARCOStatisticsDto> Handle(GetARCOStatisticsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _repository.GetStatisticsAsync(request.FromDate, request.ToDate, cancellationToken);
        return new ARCOStatisticsDto
        {
            TotalRequests = stats.TotalRequests,
            PendingRequests = stats.PendingRequests,
            CompletedRequests = stats.CompletedRequests,
            RejectedRequests = stats.RejectedRequests,
            OverdueRequests = stats.OverdueRequests,
            AverageProcessingDays = stats.AverageProcessingDays,
            RequestsByType = stats.RequestsByType,
            RequestsByStatus = stats.RequestsByStatus,
            ComplianceRate = stats.ComplianceRate
        };
    }
}
