using MediatR;
using InventoryManagementService.Application.DTOs;
using InventoryManagementService.Domain.Entities;
using InventoryManagementService.Domain.Interfaces;

namespace InventoryManagementService.Application.Features.Inventory.Commands;

/// <summary>
/// Command to start a bulk import job
/// </summary>
public record StartBulkImportCommand : IRequest<BulkImportJobDto>
{
    public Guid DealerId { get; init; }
    public Guid UserId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string FileUrl { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public ImportFileType FileType { get; init; }
}

public class StartBulkImportCommandHandler : IRequestHandler<StartBulkImportCommand, BulkImportJobDto>
{
    private readonly IBulkImportJobRepository _repository;

    public StartBulkImportCommandHandler(IBulkImportJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<BulkImportJobDto> Handle(StartBulkImportCommand request, CancellationToken cancellationToken)
    {
        var job = new BulkImportJob
        {
            Id = Guid.NewGuid(),
            DealerId = request.DealerId,
            UserId = request.UserId,
            FileName = request.FileName,
            FileUrl = request.FileUrl,
            FileSizeBytes = request.FileSizeBytes,
            FileType = request.FileType,
            Status = ImportJobStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(job);

        return new BulkImportJobDto
        {
            Id = created.Id,
            DealerId = created.DealerId,
            UserId = created.UserId,
            FileName = created.FileName,
            FileUrl = created.FileUrl,
            FileSizeBytes = created.FileSizeBytes,
            FileType = created.FileType.ToString(),
            Status = created.Status.ToString(),
            TotalRows = created.TotalRows,
            ProcessedRows = created.ProcessedRows,
            SuccessfulRows = created.SuccessfulRows,
            FailedRows = created.FailedRows,
            SkippedRows = created.SkippedRows,
            ProgressPercentage = created.ProgressPercentage,
            Errors = created.Errors.Select(e => new ImportErrorDto
            {
                RowNumber = e.RowNumber,
                Field = e.Field,
                ErrorMessage = e.ErrorMessage
            }).ToList(),
            FailureReason = created.FailureReason,
            CreatedAt = created.CreatedAt,
            StartedAt = created.StartedAt,
            CompletedAt = created.CompletedAt,
            Duration = created.Duration
        };
    }
}
