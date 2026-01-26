using MediatR;
using InventoryManagementService.Application.DTOs;
using InventoryManagementService.Domain.Interfaces;

namespace InventoryManagementService.Application.Features.Inventory.Queries;

/// <summary>
/// Query to get bulk import job status
/// </summary>
public record GetBulkImportJobQuery : IRequest<BulkImportJobDto?>
{
    public Guid JobId { get; init; }
}

public class GetBulkImportJobQueryHandler : IRequestHandler<GetBulkImportJobQuery, BulkImportJobDto?>
{
    private readonly IBulkImportJobRepository _repository;

    public GetBulkImportJobQueryHandler(IBulkImportJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<BulkImportJobDto?> Handle(GetBulkImportJobQuery request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.JobId);
        
        if (job == null)
            return null;

        return new BulkImportJobDto
        {
            Id = job.Id,
            DealerId = job.DealerId,
            UserId = job.UserId,
            FileName = job.FileName,
            FileUrl = job.FileUrl,
            FileSizeBytes = job.FileSizeBytes,
            FileType = job.FileType.ToString(),
            Status = job.Status.ToString(),
            TotalRows = job.TotalRows,
            ProcessedRows = job.ProcessedRows,
            SuccessfulRows = job.SuccessfulRows,
            FailedRows = job.FailedRows,
            SkippedRows = job.SkippedRows,
            ProgressPercentage = job.ProgressPercentage,
            Errors = job.Errors.Select(e => new ImportErrorDto
            {
                RowNumber = e.RowNumber,
                Field = e.Field,
                ErrorMessage = e.ErrorMessage
            }).ToList(),
            FailureReason = job.FailureReason,
            CreatedAt = job.CreatedAt,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            Duration = job.Duration
        };
    }
}

/// <summary>
/// Query to get all bulk import jobs for a dealer
/// </summary>
public record GetBulkImportJobsQuery : IRequest<List<BulkImportJobDto>>
{
    public Guid DealerId { get; init; }
    public int Limit { get; init; } = 20;
}

public class GetBulkImportJobsQueryHandler : IRequestHandler<GetBulkImportJobsQuery, List<BulkImportJobDto>>
{
    private readonly IBulkImportJobRepository _repository;

    public GetBulkImportJobsQueryHandler(IBulkImportJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<BulkImportJobDto>> Handle(GetBulkImportJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _repository.GetByDealerIdAsync(request.DealerId);
        
        return jobs
            .OrderByDescending(j => j.CreatedAt)
            .Take(request.Limit)
            .Select(job => new BulkImportJobDto
            {
                Id = job.Id,
                DealerId = job.DealerId,
                UserId = job.UserId,
                FileName = job.FileName,
                FileUrl = job.FileUrl,
                FileSizeBytes = job.FileSizeBytes,
                FileType = job.FileType.ToString(),
                Status = job.Status.ToString(),
                TotalRows = job.TotalRows,
                ProcessedRows = job.ProcessedRows,
                SuccessfulRows = job.SuccessfulRows,
                FailedRows = job.FailedRows,
                SkippedRows = job.SkippedRows,
                ProgressPercentage = job.ProgressPercentage,
                Errors = job.Errors.Select(e => new ImportErrorDto
                {
                    RowNumber = e.RowNumber,
                    Field = e.Field,
                    ErrorMessage = e.ErrorMessage
                }).ToList(),
                FailureReason = job.FailureReason,
                CreatedAt = job.CreatedAt,
                StartedAt = job.StartedAt,
                CompletedAt = job.CompletedAt,
                Duration = job.Duration
            })
            .ToList();
    }
}
