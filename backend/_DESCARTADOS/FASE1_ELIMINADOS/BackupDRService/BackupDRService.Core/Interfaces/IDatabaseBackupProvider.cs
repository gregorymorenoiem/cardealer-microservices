using BackupDRService.Core.Models;

namespace BackupDRService.Core.Interfaces;

/// <summary>
/// Interface for database-specific backup operations
/// </summary>
public interface IDatabaseBackupProvider
{
    /// <summary>
    /// Database target type this provider handles
    /// </summary>
    BackupTarget TargetType { get; }

    /// <summary>
    /// Create a backup of the database
    /// </summary>
    Task<DatabaseBackupResult> BackupAsync(DatabaseBackupRequest request);

    /// <summary>
    /// Restore a database from a backup file
    /// </summary>
    Task<DatabaseRestoreResult> RestoreAsync(DatabaseRestoreRequest request);

    /// <summary>
    /// Test connection to the database
    /// </summary>
    Task<bool> TestConnectionAsync(string connectionString);

    /// <summary>
    /// Get database information (size, tables, etc.)
    /// </summary>
    Task<DatabaseInfo> GetDatabaseInfoAsync(string connectionString, string databaseName);
}

/// <summary>
/// Request for database backup
/// </summary>
public class DatabaseBackupRequest
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public BackupType BackupType { get; set; } = BackupType.Full;
    public bool Compress { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 3600;
    public Dictionary<string, string> Options { get; set; } = new();
}

/// <summary>
/// Result of database backup
/// </summary>
public class DatabaseBackupResult
{
    public bool Success { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorDetails { get; set; }
    public int TablesBackedUp { get; set; }
    public long RowsBackedUp { get; set; }

    public static DatabaseBackupResult Succeeded(string filePath, long size, string checksum, TimeSpan duration)
    {
        return new DatabaseBackupResult
        {
            Success = true,
            FilePath = filePath,
            FileSizeBytes = size,
            Checksum = checksum,
            Duration = duration
        };
    }

    public static DatabaseBackupResult Failed(string error, string? details = null)
    {
        return new DatabaseBackupResult
        {
            Success = false,
            ErrorMessage = error,
            ErrorDetails = details
        };
    }
}

/// <summary>
/// Request for database restore
/// </summary>
public class DatabaseRestoreRequest
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string BackupFilePath { get; set; } = string.Empty;
    public bool DropExistingDatabase { get; set; } = false;
    public bool CreateIfNotExists { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 7200;
    public Dictionary<string, string> Options { get; set; } = new();
}

/// <summary>
/// Result of database restore
/// </summary>
public class DatabaseRestoreResult
{
    public bool Success { get; set; }
    public TimeSpan Duration { get; set; }
    public long BytesRestored { get; set; }
    public int TablesRestored { get; set; }
    public long RowsRestored { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorDetails { get; set; }

    public static DatabaseRestoreResult Succeeded(TimeSpan duration, long bytes, int tables, long rows)
    {
        return new DatabaseRestoreResult
        {
            Success = true,
            Duration = duration,
            BytesRestored = bytes,
            TablesRestored = tables,
            RowsRestored = rows
        };
    }

    public static DatabaseRestoreResult Failed(string error, string? details = null)
    {
        return new DatabaseRestoreResult
        {
            Success = false,
            ErrorMessage = error,
            ErrorDetails = details
        };
    }
}

/// <summary>
/// Database information
/// </summary>
public class DatabaseInfo
{
    public string Name { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public int TableCount { get; set; }
    public long TotalRows { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string ServerVersion { get; set; } = string.Empty;
    public List<string> TableNames { get; set; } = new();
}
