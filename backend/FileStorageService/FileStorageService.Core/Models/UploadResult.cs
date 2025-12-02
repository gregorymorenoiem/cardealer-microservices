namespace FileStorageService.Core.Models;

/// <summary>
/// Result of a file upload operation
/// </summary>
public class UploadResult
{
    /// <summary>
    /// Whether upload was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Stored file information
    /// </summary>
    public StoredFile? File { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Error code if failed
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Upload duration in milliseconds
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Bytes uploaded
    /// </summary>
    public long BytesUploaded { get; set; }

    /// <summary>
    /// Virus scan result
    /// </summary>
    public ScanResult? ScanResult { get; set; }

    /// <summary>
    /// Extracted metadata
    /// </summary>
    public FileMetadata? Metadata { get; set; }

    /// <summary>
    /// Generated variants
    /// </summary>
    public List<FileVariant> Variants { get; set; } = new();

    /// <summary>
    /// Processing steps completed
    /// </summary>
    public List<ProcessingStep> ProcessingSteps { get; set; } = new();

    /// <summary>
    /// Creates a successful upload result
    /// </summary>
    public static UploadResult Successful(StoredFile file) => new()
    {
        Success = true,
        File = file,
        BytesUploaded = file.SizeBytes
    };

    /// <summary>
    /// Creates a failed upload result
    /// </summary>
    public static UploadResult Failed(string errorMessage, string? errorCode = null) => new()
    {
        Success = false,
        ErrorMessage = errorMessage,
        ErrorCode = errorCode
    };

    /// <summary>
    /// Creates a virus detected upload result
    /// </summary>
    public static UploadResult VirusDetected(string threatName, ScanResult scanResult) => new()
    {
        Success = false,
        ErrorMessage = $"Virus detected: {threatName}",
        ErrorCode = "VIRUS_DETECTED",
        ScanResult = scanResult
    };
}

/// <summary>
/// Processing step in file upload pipeline
/// </summary>
public class ProcessingStep
{
    /// <summary>
    /// Step name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Step status
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Duration in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional details
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();

    /// <summary>
    /// Creates a completed step
    /// </summary>
    public static ProcessingStep Completed(string name, long durationMs) => new()
    {
        Name = name,
        Status = "Completed",
        DurationMs = durationMs
    };

    /// <summary>
    /// Creates a failed step
    /// </summary>
    public static ProcessingStep Failed(string name, string error) => new()
    {
        Name = name,
        Status = "Failed",
        ErrorMessage = error
    };

    /// <summary>
    /// Creates a skipped step
    /// </summary>
    public static ProcessingStep Skipped(string name, string reason) => new()
    {
        Name = name,
        Status = "Skipped",
        Details = { ["reason"] = reason }
    };
}

/// <summary>
/// Batch upload result
/// </summary>
public class BatchUploadResult
{
    /// <summary>
    /// Total files processed
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// Successful uploads
    /// </summary>
    public int SuccessfulUploads { get; set; }

    /// <summary>
    /// Failed uploads
    /// </summary>
    public int FailedUploads { get; set; }

    /// <summary>
    /// Individual results
    /// </summary>
    public List<UploadResult> Results { get; set; } = new();

    /// <summary>
    /// Total bytes uploaded
    /// </summary>
    public long TotalBytesUploaded { get; set; }

    /// <summary>
    /// Total duration in milliseconds
    /// </summary>
    public long TotalDurationMs { get; set; }

    /// <summary>
    /// Batch started at
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Batch completed at
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Add a result
    /// </summary>
    public void AddResult(UploadResult result)
    {
        Results.Add(result);
        TotalFiles++;
        if (result.Success)
        {
            SuccessfulUploads++;
            TotalBytesUploaded += result.BytesUploaded;
        }
        else
        {
            FailedUploads++;
        }
    }

    /// <summary>
    /// Complete the batch
    /// </summary>
    public void Complete()
    {
        CompletedAt = DateTime.UtcNow;
        TotalDurationMs = (long)(CompletedAt.Value - StartedAt).TotalMilliseconds;
    }
}

/// <summary>
/// Download result
/// </summary>
public class DownloadResult
{
    /// <summary>
    /// Whether download was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// File stream
    /// </summary>
    public Stream? Stream { get; set; }

    /// <summary>
    /// Content type
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// File name
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// File size
    /// </summary>
    public long? SizeBytes { get; set; }

    /// <summary>
    /// Content hash
    /// </summary>
    public string? ContentHash { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Creates a successful download result
    /// </summary>
    public static DownloadResult Successful(Stream stream, string contentType, string fileName, long sizeBytes) => new()
    {
        Success = true,
        Stream = stream,
        ContentType = contentType,
        FileName = fileName,
        SizeBytes = sizeBytes
    };

    /// <summary>
    /// Creates a failed download result
    /// </summary>
    public static DownloadResult Failed(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}

/// <summary>
/// Delete result
/// </summary>
public class DeleteResult
{
    /// <summary>
    /// Whether delete was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// File ID deleted
    /// </summary>
    public string? FileId { get; set; }

    /// <summary>
    /// Deleted storage keys
    /// </summary>
    public List<string> DeletedKeys { get; set; } = new();

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Creates a successful delete result
    /// </summary>
    public static DeleteResult Successful(string fileId, IEnumerable<string> deletedKeys) => new()
    {
        Success = true,
        FileId = fileId,
        DeletedKeys = deletedKeys.ToList()
    };

    /// <summary>
    /// Creates a failed delete result
    /// </summary>
    public static DeleteResult Failed(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}
