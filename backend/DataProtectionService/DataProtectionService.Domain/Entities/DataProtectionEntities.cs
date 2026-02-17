namespace DataProtectionService.Domain.Entities;

public class PrivacyPolicy
{
    public Guid Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public string DocumentType { get; set; } = "PrivacyPolicy"; // PrivacyPolicy, TermsOfService, CookiePolicy
    public string Content { get; set; } = string.Empty;
    public string? ChangesSummary { get; set; }
    public string Language { get; set; } = "es";
    public DateTime EffectiveDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool RequiresReAcceptance { get; set; } = false;
    
    // Audit
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum ExportStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Expired = 5
}

public class DataExport
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ARCORequestId { get; set; }
    
    public ExportStatus Status { get; set; } = ExportStatus.Pending;
    public string Format { get; set; } = "JSON"; // JSON, PDF, CSV
    
    // Data to include
    public bool IncludeTransactions { get; set; } = true;
    public bool IncludeMessages { get; set; } = true;
    public bool IncludeVehicleHistory { get; set; } = true;
    public bool IncludeUserActivity { get; set; } = true;
    
    // Timestamps
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    // Result
    public string? DownloadUrl { get; set; }
    public DateTime? DownloadExpiresAt { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Request metadata
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

public class AnonymizationRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } // Original user ID before anonymization
    public Guid? ARCORequestId { get; set; } // ARCO request that triggered this
    public Guid? RequestedBy { get; set; } // Admin who processed
    
    // Anonymization details
    public DateTime AnonymizedAt { get; set; } = DateTime.UtcNow;
    public string Reason { get; set; } = string.Empty;
    
    // Original data (for compliance record - encrypted/hashed)
    public string OriginalEmail { get; set; } = string.Empty;
    public string OriginalPhone { get; set; } = string.Empty;
    
    // Replacement data
    public string AnonymizedEmail { get; set; } = string.Empty;
    public string AnonymizedPhone { get; set; } = string.Empty;
    
    // Affected records
    public List<string> AffectedTables { get; set; } = new();
    public int AffectedRecordsCount { get; set; }
    
    // Status
    public bool IsComplete { get; set; } = false;
    
    // Retention - keep anonymization record for compliance
    public DateTime RetentionEndDate { get; set; } = DateTime.UtcNow.AddYears(5);
}
