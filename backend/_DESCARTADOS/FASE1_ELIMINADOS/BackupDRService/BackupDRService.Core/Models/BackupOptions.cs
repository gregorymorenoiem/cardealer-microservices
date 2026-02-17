namespace BackupDRService.Core.Models;

/// <summary>
/// Configuration options for the Backup & DR Service
/// </summary>
public class BackupOptions
{
    public const string SectionName = "BackupOptions";

    /// <summary>
    /// Default retention period in days for backups
    /// </summary>
    public int DefaultRetentionDays { get; set; } = 30;

    /// <summary>
    /// Maximum number of concurrent backup jobs
    /// </summary>
    public int MaxConcurrentJobs { get; set; } = 3;

    /// <summary>
    /// Default storage type for new backup jobs
    /// </summary>
    public StorageType DefaultStorageType { get; set; } = StorageType.Local;

    /// <summary>
    /// Local storage base path for backups
    /// </summary>
    public string LocalStoragePath { get; set; } = "/var/backups/cardealer";

    /// <summary>
    /// Azure Blob Storage connection string
    /// </summary>
    public string? AzureBlobConnectionString { get; set; }

    /// <summary>
    /// Azure Blob Storage container name
    /// </summary>
    public string AzureBlobContainerName { get; set; } = "backups";

    /// <summary>
    /// Enable compression for backups by default
    /// </summary>
    public bool EnableCompressionByDefault { get; set; } = true;

    /// <summary>
    /// Enable encryption for backups by default
    /// </summary>
    public bool EnableEncryptionByDefault { get; set; } = false;

    /// <summary>
    /// Default encryption key (should be stored securely in production)
    /// </summary>
    public string? DefaultEncryptionKey { get; set; }

    /// <summary>
    /// Verify backup integrity after creation
    /// </summary>
    public bool VerifyBackupAfterCreation { get; set; } = true;

    /// <summary>
    /// Timeout in minutes for backup operations
    /// </summary>
    public int BackupTimeoutMinutes { get; set; } = 60;

    /// <summary>
    /// Timeout in minutes for restore operations
    /// </summary>
    public int RestoreTimeoutMinutes { get; set; } = 120;

    /// <summary>
    /// Enable automatic cleanup of expired backups
    /// </summary>
    public bool EnableAutomaticCleanup { get; set; } = true;

    /// <summary>
    /// Cleanup schedule (cron expression)
    /// </summary>
    public string CleanupSchedule { get; set; } = "0 2 * * *"; // Daily at 2 AM

    /// <summary>
    /// Maximum backup file size in MB (0 = unlimited)
    /// </summary>
    public int MaxBackupFileSizeMB { get; set; } = 0;

    /// <summary>
    /// PostgreSQL pg_dump path
    /// </summary>
    public string PgDumpPath { get; set; } = "pg_dump";

    /// <summary>
    /// PostgreSQL pg_restore path
    /// </summary>
    public string PgRestorePath { get; set; } = "pg_restore";

    /// <summary>
    /// Send notifications on backup failures
    /// </summary>
    public bool NotifyOnFailure { get; set; } = true;

    /// <summary>
    /// Send notifications on backup success
    /// </summary>
    public bool NotifyOnSuccess { get; set; } = false;

    /// <summary>
    /// Notification webhook URL
    /// </summary>
    public string? NotificationWebhookUrl { get; set; }

    /// <summary>
    /// Default connection strings for various database types
    /// </summary>
    public Dictionary<string, string> DefaultConnectionStrings { get; set; } = new();
}
