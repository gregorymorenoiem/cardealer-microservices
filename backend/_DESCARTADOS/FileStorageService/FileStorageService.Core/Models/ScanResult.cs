namespace FileStorageService.Core.Models;

/// <summary>
/// Virus scan status
/// </summary>
public enum ScanStatus
{
    /// <summary>
    /// Scan not started
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Scan in progress
    /// </summary>
    Scanning = 1,

    /// <summary>
    /// Scan completed successfully
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Scan failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Scan skipped (disabled or file type excluded)
    /// </summary>
    Skipped = 4
}

/// <summary>
/// Threat level classification
/// </summary>
public enum ThreatLevel
{
    /// <summary>
    /// No threat detected
    /// </summary>
    None = 0,

    /// <summary>
    /// Low risk (potentially unwanted program)
    /// </summary>
    Low = 1,

    /// <summary>
    /// Medium risk (adware, spyware)
    /// </summary>
    Medium = 2,

    /// <summary>
    /// High risk (malware, virus)
    /// </summary>
    High = 3,

    /// <summary>
    /// Critical (ransomware, rootkit)
    /// </summary>
    Critical = 4
}

/// <summary>
/// Result of a virus scan operation
/// </summary>
public class ScanResult
{
    /// <summary>
    /// Unique scan identifier
    /// </summary>
    public string ScanId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// File identifier that was scanned
    /// </summary>
    public string FileId { get; set; } = string.Empty;

    /// <summary>
    /// Current scan status
    /// </summary>
    public ScanStatus Status { get; set; } = ScanStatus.Pending;

    /// <summary>
    /// Whether the file is clean (no threats detected)
    /// </summary>
    public bool IsClean { get; set; } = true;

    /// <summary>
    /// Detected threat level
    /// </summary>
    public ThreatLevel ThreatLevel { get; set; } = ThreatLevel.None;

    /// <summary>
    /// Name of detected threat (if any)
    /// </summary>
    public string? ThreatName { get; set; }

    /// <summary>
    /// Detailed threat description
    /// </summary>
    public string? ThreatDescription { get; set; }

    /// <summary>
    /// Scanner engine name
    /// </summary>
    public string ScannerEngine { get; set; } = "ClamAV";

    /// <summary>
    /// Scanner engine version
    /// </summary>
    public string? ScannerVersion { get; set; }

    /// <summary>
    /// Virus definition version/date
    /// </summary>
    public string? DefinitionVersion { get; set; }

    /// <summary>
    /// Scan start timestamp
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Scan completion timestamp
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Scan duration in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// File size that was scanned
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Error message if scan failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional scan details
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();

    /// <summary>
    /// Creates a clean scan result
    /// </summary>
    public static ScanResult Clean(string fileId, long fileSizeBytes) => new()
    {
        FileId = fileId,
        FileSizeBytes = fileSizeBytes,
        Status = ScanStatus.Completed,
        IsClean = true,
        ThreatLevel = ThreatLevel.None,
        CompletedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Creates a threat detected scan result
    /// </summary>
    public static ScanResult ThreatDetected(string fileId, string threatName, ThreatLevel level) => new()
    {
        FileId = fileId,
        Status = ScanStatus.Completed,
        IsClean = false,
        ThreatLevel = level,
        ThreatName = threatName,
        CompletedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Creates a failed scan result
    /// </summary>
    public static ScanResult Error(string fileId, string errorMessage) => new()
    {
        FileId = fileId,
        Status = ScanStatus.Failed,
        IsClean = false,
        ErrorMessage = errorMessage,
        CompletedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Creates a skipped scan result
    /// </summary>
    public static ScanResult Skipped(string fileId, string reason) => new()
    {
        FileId = fileId,
        Status = ScanStatus.Skipped,
        IsClean = true,
        Details = { ["skipReason"] = reason },
        CompletedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Complete the scan
    /// </summary>
    public void Complete()
    {
        CompletedAt = DateTime.UtcNow;
        DurationMs = (long)(CompletedAt.Value - StartedAt).TotalMilliseconds;
        Status = ScanStatus.Completed;
    }

    /// <summary>
    /// Get scan summary
    /// </summary>
    public string GetSummary()
    {
        if (Status == ScanStatus.Skipped)
            return "Scan skipped";
        if (Status == ScanStatus.Failed)
            return $"Scan failed: {ErrorMessage}";
        if (!IsClean)
            return $"Threat detected: {ThreatName} ({ThreatLevel})";
        return "Clean - No threats detected";
    }
}

/// <summary>
/// Batch scan result for multiple files
/// </summary>
public class BatchScanResult
{
    /// <summary>
    /// Total files scanned
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// Clean files count
    /// </summary>
    public int CleanFiles { get; set; }

    /// <summary>
    /// Infected files count
    /// </summary>
    public int InfectedFiles { get; set; }

    /// <summary>
    /// Failed scans count
    /// </summary>
    public int FailedScans { get; set; }

    /// <summary>
    /// Skipped files count
    /// </summary>
    public int SkippedFiles { get; set; }

    /// <summary>
    /// Individual scan results
    /// </summary>
    public List<ScanResult> Results { get; set; } = new();

    /// <summary>
    /// Batch scan start time
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Batch scan completion time
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Total duration in milliseconds
    /// </summary>
    public long? TotalDurationMs { get; set; }

    /// <summary>
    /// Add a scan result
    /// </summary>
    public void AddResult(ScanResult result)
    {
        Results.Add(result);
        TotalFiles++;

        switch (result.Status)
        {
            case ScanStatus.Completed when result.IsClean:
                CleanFiles++;
                break;
            case ScanStatus.Completed when !result.IsClean:
                InfectedFiles++;
                break;
            case ScanStatus.Failed:
                FailedScans++;
                break;
            case ScanStatus.Skipped:
                SkippedFiles++;
                break;
        }
    }

    /// <summary>
    /// Complete the batch scan
    /// </summary>
    public void Complete()
    {
        CompletedAt = DateTime.UtcNow;
        TotalDurationMs = (long)(CompletedAt.Value - StartedAt).TotalMilliseconds;
    }
}
