using System;

namespace InventoryManagementService.Domain.Entities;

/// <summary>
/// Bulk Import Job - Tracks CSV/Excel import operations
/// </summary>
public class BulkImportJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid DealerId { get; set; }
    public Guid UserId { get; set; }
    
    // File Information
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty; // S3 or storage URL
    public long FileSizeBytes { get; set; }
    public ImportFileType FileType { get; set; }
    
    // Job Status
    public ImportJobStatus Status { get; set; } = ImportJobStatus.Pending;
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int SuccessfulRows { get; set; }
    public int FailedRows { get; set; }
    public int SkippedRows { get; set; }
    
    // Progress
    public decimal ProgressPercentage => TotalRows > 0 
        ? (decimal)ProcessedRows / TotalRows * 100 
        : 0;
    
    // Error Tracking
    public List<ImportError> Errors { get; set; } = new();
    public string? FailureReason { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Duration
    public TimeSpan? Duration => CompletedAt.HasValue && StartedAt.HasValue 
        ? CompletedAt - StartedAt 
        : null;
    
    // Methods
    public void Start()
    {
        Status = ImportJobStatus.Processing;
        StartedAt = DateTime.UtcNow;
    }
    
    public void Complete()
    {
        Status = ImportJobStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }
    
    public void Fail(string reason)
    {
        Status = ImportJobStatus.Failed;
        FailureReason = reason;
        CompletedAt = DateTime.UtcNow;
    }
    
    public void AddError(int rowNumber, string field, string errorMessage)
    {
        Errors.Add(new ImportError
        {
            RowNumber = rowNumber,
            Field = field,
            ErrorMessage = errorMessage
        });
        FailedRows++;
    }
    
    public void RecordSuccess()
    {
        SuccessfulRows++;
        ProcessedRows++;
    }
    
    public void RecordSkip()
    {
        SkippedRows++;
        ProcessedRows++;
    }
}

/// <summary>
/// Import Error Details
/// </summary>
public class ImportError
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Import File Type
/// </summary>
public enum ImportFileType
{
    CSV,
    Excel,
    JSON
}

/// <summary>
/// Import Job Status
/// </summary>
public enum ImportJobStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}
