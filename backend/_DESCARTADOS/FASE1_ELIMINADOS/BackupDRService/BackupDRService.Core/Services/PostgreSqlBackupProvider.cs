using System.Diagnostics;
using System.Security.Cryptography;
using BackupDRService.Core.Interfaces;
using BackupDRService.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BackupDRService.Core.Services;

/// <summary>
/// PostgreSQL database backup provider using pg_dump and pg_restore
/// </summary>
public class PostgreSqlBackupProvider : IDatabaseBackupProvider
{
    private readonly ILogger<PostgreSqlBackupProvider> _logger;
    private readonly BackupOptions _options;

    public PostgreSqlBackupProvider(
        ILogger<PostgreSqlBackupProvider> logger,
        IOptions<BackupOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public BackupTarget TargetType => BackupTarget.PostgreSQL;

    public async Task<DatabaseBackupResult> BackupAsync(DatabaseBackupRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting PostgreSQL backup for database: {Database}", request.DatabaseName);

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(request.OutputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Build pg_dump command
            var connBuilder = new NpgsqlConnectionStringBuilder(request.ConnectionString);
            var arguments = BuildPgDumpArguments(connBuilder, request);

            var startInfo = new ProcessStartInfo
            {
                FileName = _options.PgDumpPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Environment =
                {
                    ["PGPASSWORD"] = connBuilder.Password ?? ""
                }
            };

            using var process = new Process { StartInfo = startInfo };
            var errorOutput = string.Empty;

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorOutput += e.Data + Environment.NewLine;
                    _logger.LogDebug("pg_dump: {Output}", e.Data);
                }
            };

            process.Start();
            process.BeginErrorReadLine();

            var timeoutMs = request.TimeoutSeconds * 1000;
            var completed = await Task.Run(() => process.WaitForExit(timeoutMs));

            if (!completed)
            {
                process.Kill();
                return DatabaseBackupResult.Failed("Backup operation timed out",
                    $"Timeout after {request.TimeoutSeconds} seconds");
            }

            if (process.ExitCode != 0)
            {
                return DatabaseBackupResult.Failed($"pg_dump failed with exit code {process.ExitCode}", errorOutput);
            }

            // Verify backup file was created
            if (!File.Exists(request.OutputPath))
            {
                return DatabaseBackupResult.Failed("Backup file was not created");
            }

            var fileInfo = new FileInfo(request.OutputPath);
            var checksum = await CalculateChecksumAsync(request.OutputPath);

            stopwatch.Stop();

            _logger.LogInformation(
                "PostgreSQL backup completed: {Database}, Size: {Size} bytes, Duration: {Duration}ms",
                request.DatabaseName, fileInfo.Length, stopwatch.ElapsedMilliseconds);

            return DatabaseBackupResult.Succeeded(
                request.OutputPath,
                fileInfo.Length,
                checksum,
                stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "PostgreSQL backup failed for database: {Database}", request.DatabaseName);
            return DatabaseBackupResult.Failed(ex.Message, ex.StackTrace);
        }
    }

    public async Task<DatabaseRestoreResult> RestoreAsync(DatabaseRestoreRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting PostgreSQL restore for database: {Database}", request.DatabaseName);

            if (!File.Exists(request.BackupFilePath))
            {
                return DatabaseRestoreResult.Failed($"Backup file not found: {request.BackupFilePath}");
            }

            var connBuilder = new NpgsqlConnectionStringBuilder(request.ConnectionString);

            // Handle database creation/drop if needed
            if (request.DropExistingDatabase || request.CreateIfNotExists)
            {
                await PrepareTargetDatabaseAsync(connBuilder, request.DatabaseName,
                    request.DropExistingDatabase, request.CreateIfNotExists);
            }

            // Build pg_restore command
            var arguments = BuildPgRestoreArguments(connBuilder, request);

            var startInfo = new ProcessStartInfo
            {
                FileName = _options.PgRestorePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Environment =
                {
                    ["PGPASSWORD"] = connBuilder.Password ?? ""
                }
            };

            using var process = new Process { StartInfo = startInfo };
            var errorOutput = string.Empty;

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorOutput += e.Data + Environment.NewLine;
                    _logger.LogDebug("pg_restore: {Output}", e.Data);
                }
            };

            process.Start();
            process.BeginErrorReadLine();

            var timeoutMs = request.TimeoutSeconds * 1000;
            var completed = await Task.Run(() => process.WaitForExit(timeoutMs));

            if (!completed)
            {
                process.Kill();
                return DatabaseRestoreResult.Failed("Restore operation timed out",
                    $"Timeout after {request.TimeoutSeconds} seconds");
            }

            // pg_restore may return non-zero for warnings, check for actual errors
            if (process.ExitCode != 0 && !string.IsNullOrEmpty(errorOutput) &&
                errorOutput.Contains("ERROR", StringComparison.OrdinalIgnoreCase))
            {
                return DatabaseRestoreResult.Failed($"pg_restore failed with exit code {process.ExitCode}", errorOutput);
            }

            var fileInfo = new FileInfo(request.BackupFilePath);
            stopwatch.Stop();

            _logger.LogInformation(
                "PostgreSQL restore completed: {Database}, Duration: {Duration}ms",
                request.DatabaseName, stopwatch.ElapsedMilliseconds);

            return DatabaseRestoreResult.Succeeded(stopwatch.Elapsed, fileInfo.Length, 0, 0);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "PostgreSQL restore failed for database: {Database}", request.DatabaseName);
            return DatabaseRestoreResult.Failed(ex.Message, ex.StackTrace);
        }
    }

    public async Task<bool> TestConnectionAsync(string connectionString)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PostgreSQL connection test failed");
            return false;
        }
    }

    public async Task<DatabaseInfo> GetDatabaseInfoAsync(string connectionString, string databaseName)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var info = new DatabaseInfo
            {
                Name = databaseName
            };

            // Get database size
            await using (var cmd = new NpgsqlCommand(
                $"SELECT pg_database_size('{databaseName}')", connection))
            {
                var result = await cmd.ExecuteScalarAsync();
                info.SizeBytes = result != null ? Convert.ToInt64(result) : 0;
            }

            // Get table count
            await using (var cmd = new NpgsqlCommand(
                "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public'", connection))
            {
                var result = await cmd.ExecuteScalarAsync();
                info.TableCount = result != null ? Convert.ToInt32(result) : 0;
            }

            // Get table names
            await using (var cmd = new NpgsqlCommand(
                "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name",
                connection))
            {
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    info.TableNames.Add(reader.GetString(0));
                }
            }

            // Get server version
            info.ServerVersion = connection.ServerVersion;

            return info;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get PostgreSQL database info: {Database}", databaseName);
            throw;
        }
    }

    private string BuildPgDumpArguments(NpgsqlConnectionStringBuilder connBuilder, DatabaseBackupRequest request)
    {
        var args = new List<string>
        {
            $"-h {connBuilder.Host}",
            $"-p {connBuilder.Port}",
            $"-U {connBuilder.Username}",
            $"-d {request.DatabaseName}",
            $"-f \"{request.OutputPath}\"",
            "-F c", // Custom format (compressed)
            "-b",   // Include blobs
            "-v"    // Verbose
        };

        if (request.BackupType == BackupType.Full)
        {
            args.Add("--no-owner");
            args.Add("--no-privileges");
        }

        return string.Join(" ", args);
    }

    private string BuildPgRestoreArguments(NpgsqlConnectionStringBuilder connBuilder, DatabaseRestoreRequest request)
    {
        var args = new List<string>
        {
            $"-h {connBuilder.Host}",
            $"-p {connBuilder.Port}",
            $"-U {connBuilder.Username}",
            $"-d {request.DatabaseName}",
            "-v",           // Verbose
            "--no-owner",
            "--no-privileges",
            $"\"{request.BackupFilePath}\""
        };

        return string.Join(" ", args);
    }

    private async Task PrepareTargetDatabaseAsync(NpgsqlConnectionStringBuilder connBuilder,
        string databaseName, bool dropExisting, bool createIfNotExists)
    {
        // Connect to postgres database for administrative tasks
        var adminConnString = new NpgsqlConnectionStringBuilder(connBuilder.ConnectionString)
        {
            Database = "postgres"
        }.ConnectionString;

        await using var connection = new NpgsqlConnection(adminConnString);
        await connection.OpenAsync();

        if (dropExisting)
        {
            // Terminate existing connections
            await using (var cmd = new NpgsqlCommand(
                $@"SELECT pg_terminate_backend(pid) FROM pg_stat_activity 
                   WHERE datname = '{databaseName}' AND pid <> pg_backend_pid()", connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            // Drop database
            await using (var cmd = new NpgsqlCommand(
                $"DROP DATABASE IF EXISTS \"{databaseName}\"", connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }
        }

        if (createIfNotExists || dropExisting)
        {
            // Check if database exists
            await using (var cmd = new NpgsqlCommand(
                $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'", connection))
            {
                var exists = await cmd.ExecuteScalarAsync() != null;
                if (!exists)
                {
                    await using var createCmd = new NpgsqlCommand(
                        $"CREATE DATABASE \"{databaseName}\"", connection);
                    await createCmd.ExecuteNonQueryAsync();
                }
            }
        }
    }

    private async Task<string> CalculateChecksumAsync(string filePath)
    {
        using var sha256 = SHA256.Create();
        await using var stream = File.OpenRead(filePath);
        var hash = await sha256.ComputeHashAsync(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
