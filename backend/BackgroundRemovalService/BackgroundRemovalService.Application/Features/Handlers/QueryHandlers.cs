using BackgroundRemovalService.Application.DTOs;
using BackgroundRemovalService.Application.Features.Queries;
using BackgroundRemovalService.Application.Interfaces;
using BackgroundRemovalService.Domain.Interfaces;
using BackgroundRemovalService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackgroundRemovalService.Application.Features.Handlers;

public class GetRemovalJobByIdQueryHandler : IRequestHandler<GetRemovalJobByIdQuery, RemovalJobResponse?>
{
    private readonly IBackgroundRemovalJobRepository _repository;

    public GetRemovalJobByIdQueryHandler(IBackgroundRemovalJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<RemovalJobResponse?> Handle(GetRemovalJobByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.JobId, cancellationToken);
        if (job == null) return null;
        
        return new RemovalJobResponse
        {
            JobId = job.Id,
            Status = job.Status,
            Provider = job.Provider,
            SourceImageUrl = job.SourceImageUrl,
            ResultImageUrl = job.ResultImageUrl,
            ProcessingTimeMs = job.ProcessingTimeMs,
            CreditsConsumed = job.CreditsConsumed,
            ErrorMessage = job.ErrorMessage,
            ErrorCode = job.ErrorCode,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.ProcessingCompletedAt
        };
    }
}

public class GetRemovalJobByCorrelationIdQueryHandler : IRequestHandler<GetRemovalJobByCorrelationIdQuery, RemovalJobResponse?>
{
    private readonly IBackgroundRemovalJobRepository _repository;

    public GetRemovalJobByCorrelationIdQueryHandler(IBackgroundRemovalJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<RemovalJobResponse?> Handle(GetRemovalJobByCorrelationIdQuery request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByCorrelationIdAsync(request.CorrelationId, cancellationToken);
        if (job == null) return null;
        
        return new RemovalJobResponse
        {
            JobId = job.Id,
            Status = job.Status,
            Provider = job.Provider,
            SourceImageUrl = job.SourceImageUrl,
            ResultImageUrl = job.ResultImageUrl,
            ProcessingTimeMs = job.ProcessingTimeMs,
            CreditsConsumed = job.CreditsConsumed,
            ErrorMessage = job.ErrorMessage,
            ErrorCode = job.ErrorCode,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.ProcessingCompletedAt
        };
    }
}

public class GetUserRemovalJobsQueryHandler : IRequestHandler<GetUserRemovalJobsQuery, RemovalJobListResponse>
{
    private readonly IBackgroundRemovalJobRepository _repository;

    public GetUserRemovalJobsQueryHandler(IBackgroundRemovalJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<RemovalJobListResponse> Handle(GetUserRemovalJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _repository.GetByUserIdAsync(
            request.UserId, 
            request.Page, 
            request.PageSize, 
            cancellationToken);
        
        var items = jobs.Select(job => new RemovalJobResponse
        {
            JobId = job.Id,
            Status = job.Status,
            Provider = job.Provider,
            SourceImageUrl = job.SourceImageUrl,
            ResultImageUrl = job.ResultImageUrl,
            ProcessingTimeMs = job.ProcessingTimeMs,
            CreditsConsumed = job.CreditsConsumed,
            ErrorMessage = job.ErrorMessage,
            ErrorCode = job.ErrorCode,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.ProcessingCompletedAt
        }).ToList();
        
        // TODO: Obtener conteo total real
        return new RemovalJobListResponse
        {
            Items = items,
            TotalCount = items.Count,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

public class GetUsageStatisticsQueryHandler : IRequestHandler<GetUsageStatisticsQuery, UsageStatisticsResponse>
{
    private readonly IBackgroundRemovalOrchestrator _orchestrator;

    public GetUsageStatisticsQueryHandler(IBackgroundRemovalOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async Task<UsageStatisticsResponse> Handle(GetUsageStatisticsQuery request, CancellationToken cancellationToken)
    {
        return await _orchestrator.GetUsageStatisticsAsync(
            request.UserId, 
            request.BillingPeriod, 
            cancellationToken);
    }
}

public class GetProvidersInfoQueryHandler : IRequestHandler<GetProvidersInfoQuery, IEnumerable<ProviderInfoResponse>>
{
    private readonly IBackgroundRemovalOrchestrator _orchestrator;

    public GetProvidersInfoQueryHandler(IBackgroundRemovalOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async Task<IEnumerable<ProviderInfoResponse>> Handle(GetProvidersInfoQuery request, CancellationToken cancellationToken)
    {
        return await _orchestrator.GetProvidersInfoAsync(cancellationToken);
    }
}

public class GetProvidersHealthQueryHandler : IRequestHandler<GetProvidersHealthQuery, IEnumerable<ProviderHealthResponse>>
{
    private readonly IBackgroundRemovalOrchestrator _orchestrator;

    public GetProvidersHealthQueryHandler(IBackgroundRemovalOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async Task<IEnumerable<ProviderHealthResponse>> Handle(GetProvidersHealthQuery request, CancellationToken cancellationToken)
    {
        return await _orchestrator.CheckProvidersHealthAsync(cancellationToken);
    }
}
