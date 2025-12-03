using BackupDRService.Core.Entities;
using BackupDRService.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace BackupDRService.Core.Services;

public class RetentionService
{
    private readonly IBackupHistoryRepository _historyRepository;
    private readonly IRetentionPolicyRepository _policyRepository;
    private readonly IAuditLogRepository _auditRepository;
    private readonly ILogger<RetentionService> _logger;

    public RetentionService(
        IBackupHistoryRepository historyRepository,
        IRetentionPolicyRepository policyRepository,
        IAuditLogRepository auditRepository,
        ILogger<RetentionService> logger)
    {
        _historyRepository = historyRepository;
        _policyRepository = policyRepository;
        _auditRepository = auditRepository;
        _logger = logger;
    }

    public async Task<RetentionPolicy> CreatePolicyAsync(RetentionPolicy policy, string userId = "system")
    {
        var created = await _policyRepository.CreateAsync(policy);

        await _auditRepository.CreateAsync(new AuditLog
        {
            Action = "RetentionPolicyCreated",
            EntityType = "RetentionPolicy",
            EntityId = created.Id.ToString(),
            EntityName = created.Name,
            UserId = userId,
            NewValues = System.Text.Json.JsonSerializer.Serialize(created),
            Details = $"Retention policy created: Daily={created.DailyRetentionDays}, Weekly={created.WeeklyRetentionWeeks}, Monthly={created.MonthlyRetentionMonths}"
        });

        _logger.LogInformation("Retention policy {PolicyName} created", created.Name);

        return created;
    }

    public async Task<RetentionPolicy> UpdatePolicyAsync(RetentionPolicy policy, string userId = "system")
    {
        var existing = await _policyRepository.GetByIdAsync(policy.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"Retention policy with ID {policy.Id} not found");
        }

        policy.UpdatedBy = userId;
        var updated = await _policyRepository.UpdateAsync(policy);

        await _auditRepository.CreateAsync(new AuditLog
        {
            Action = "RetentionPolicyUpdated",
            EntityType = "RetentionPolicy",
            EntityId = updated.Id.ToString(),
            EntityName = updated.Name,
            UserId = userId,
            OldValues = System.Text.Json.JsonSerializer.Serialize(existing),
            NewValues = System.Text.Json.JsonSerializer.Serialize(updated),
            Details = "Retention policy updated"
        });

        _logger.LogInformation("Retention policy {PolicyName} updated", updated.Name);

        return updated;
    }

    public async Task<IEnumerable<BackupHistory>> GetBackupsToRetainAsync(
        string databaseName,
        RetentionPolicy policy)
    {
        var allBackups = await _historyRepository.GetByDatabaseNameAsync(databaseName);
        var successfulBackups = allBackups
            .Where(b => b.Status == "Success")
            .OrderByDescending(b => b.StartedAt)
            .ToList();

        var backupsToRetain = new HashSet<BackupHistory>();
        var now = DateTime.UtcNow;

        // Keep daily backups
        var dailyCutoff = now.AddDays(-policy.DailyRetentionDays);
        var dailyBackups = successfulBackups
            .Where(b => b.StartedAt >= dailyCutoff)
            .ToList();
        foreach (var backup in dailyBackups)
        {
            backupsToRetain.Add(backup);
        }

        // Keep weekly backups (first backup of each week)
        var weeklyCutoff = now.AddDays(-policy.WeeklyRetentionWeeks * 7);
        var weeklyBackups = successfulBackups
            .Where(b => b.StartedAt >= weeklyCutoff && b.StartedAt < dailyCutoff)
            .GroupBy(b => GetWeekOfYear(b.StartedAt))
            .Select(g => g.First())
            .ToList();
        foreach (var backup in weeklyBackups)
        {
            backupsToRetain.Add(backup);
        }

        // Keep monthly backups (first backup of each month)
        var monthlyCutoff = now.AddMonths(-policy.MonthlyRetentionMonths);
        var monthlyBackups = successfulBackups
            .Where(b => b.StartedAt >= monthlyCutoff && b.StartedAt < weeklyCutoff)
            .GroupBy(b => new { b.StartedAt.Year, b.StartedAt.Month })
            .Select(g => g.First())
            .ToList();
        foreach (var backup in monthlyBackups)
        {
            backupsToRetain.Add(backup);
        }

        // Keep yearly backups (first backup of each year)
        var yearlyCutoff = now.AddYears(-policy.YearlyRetentionYears);
        var yearlyBackups = successfulBackups
            .Where(b => b.StartedAt >= yearlyCutoff && b.StartedAt < monthlyCutoff)
            .GroupBy(b => b.StartedAt.Year)
            .Select(g => g.First())
            .ToList();
        foreach (var backup in yearlyBackups)
        {
            backupsToRetain.Add(backup);
        }

        _logger.LogInformation(
            "Retention analysis for {DatabaseName}: Total={Total}, Daily={Daily}, Weekly={Weekly}, Monthly={Monthly}, Yearly={Yearly}, ToRetain={ToRetain}",
            databaseName,
            successfulBackups.Count,
            dailyBackups.Count,
            weeklyBackups.Count,
            monthlyBackups.Count,
            yearlyBackups.Count,
            backupsToRetain.Count);

        return backupsToRetain;
    }

    public async Task<IEnumerable<BackupHistory>> GetBackupsToDeleteAsync(
        string databaseName,
        RetentionPolicy policy)
    {
        var allBackups = await _historyRepository.GetByDatabaseNameAsync(databaseName);
        var successfulBackups = allBackups
            .Where(b => b.Status == "Success")
            .ToList();

        var backupsToRetain = await GetBackupsToRetainAsync(databaseName, policy);
        var retainIds = backupsToRetain.Select(b => b.Id).ToHashSet();

        var backupsToDelete = successfulBackups
            .Where(b => !retainIds.Contains(b.Id))
            .ToList();

        // Apply max storage limit if configured
        if (policy.MaxStorageSizeBytes.HasValue)
        {
            var totalStorage = backupsToRetain.Sum(b => b.FileSizeBytes);
            if (totalStorage > policy.MaxStorageSizeBytes.Value)
            {
                _logger.LogWarning(
                    "Storage limit exceeded for {DatabaseName}: {TotalStorage} > {MaxStorage}",
                    databaseName,
                    totalStorage,
                    policy.MaxStorageSizeBytes.Value);

                // Add oldest backups to delete list until under limit
                var sortedRetained = backupsToRetain.OrderBy(b => b.StartedAt).ToList();
                foreach (var backup in sortedRetained)
                {
                    if (totalStorage <= policy.MaxStorageSizeBytes.Value)
                        break;

                    backupsToDelete.Add(backup);
                    totalStorage -= backup.FileSizeBytes;
                }
            }
        }

        // Apply max backup count if configured
        if (policy.MaxBackupCount.HasValue)
        {
            var retainedCount = backupsToRetain.Count() - backupsToDelete.Count();
            if (retainedCount > policy.MaxBackupCount.Value)
            {
                var excess = retainedCount - policy.MaxBackupCount.Value;
                var oldestRetained = backupsToRetain
                    .Where(b => !backupsToDelete.Contains(b))
                    .OrderBy(b => b.StartedAt)
                    .Take(excess)
                    .ToList();

                backupsToDelete.AddRange(oldestRetained);
            }
        }

        _logger.LogInformation(
            "Retention cleanup for {DatabaseName}: {ToDelete} backups marked for deletion",
            databaseName,
            backupsToDelete.Count);

        return backupsToDelete;
    }

    public async Task ApplyRetentionPolicyAsync(
        string databaseName,
        RetentionPolicy policy,
        Func<BackupHistory, Task> deleteBackupFileFunc,
        string userId = "system")
    {
        var backupsToDelete = await GetBackupsToDeleteAsync(databaseName, policy);

        foreach (var backup in backupsToDelete)
        {
            try
            {
                // Delete physical file
                await deleteBackupFileFunc(backup);

                // Delete from database
                await _historyRepository.DeleteAsync(backup.Id);

                await _auditRepository.CreateAsync(new AuditLog
                {
                    Action = "BackupDeletedByRetention",
                    EntityType = "BackupHistory",
                    EntityId = backup.BackupId,
                    EntityName = backup.JobName,
                    UserId = userId,
                    Details = $"Backup deleted by retention policy {policy.Name}. File: {backup.FileName}, Size: {backup.FileSizeBytes} bytes"
                });

                _logger.LogInformation(
                    "Backup {BackupId} deleted by retention policy {PolicyName}",
                    backup.BackupId,
                    policy.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error deleting backup {BackupId} during retention policy application",
                    backup.BackupId);

                await _auditRepository.CreateAsync(new AuditLog
                {
                    Action = "BackupDeletionFailed",
                    EntityType = "BackupHistory",
                    EntityId = backup.BackupId,
                    EntityName = backup.JobName,
                    UserId = userId,
                    Status = "Failed",
                    ErrorMessage = ex.Message,
                    Details = $"Failed to delete backup during retention policy application"
                });
            }
        }
    }

    private static int GetWeekOfYear(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        var calendar = culture.Calendar;
        var weekRule = culture.DateTimeFormat.CalendarWeekRule;
        var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;

        return calendar.GetWeekOfYear(date, weekRule, firstDayOfWeek);
    }

    /// <summary>
    /// Cleanup expired backups based on retention policies
    /// </summary>
    public async Task<RetentionCleanupResult> CleanupExpiredBackupsAsync()
    {
        var result = new RetentionCleanupResult
        {
            DeletedCount = 0,
            FreedSpaceBytes = 0,
            Errors = new List<string>()
        };

        try
        {
            var policies = await _policyRepository.GetAllAsync();

            foreach (var policy in policies)
            {
                try
                {
                    // Get all databases that use this policy
                    var histories = await _historyRepository.GetByDatabaseNameAsync("");
                    var databases = histories
                        .Where(h => h.Status == "Success")
                        .Select(h => h.DatabaseName)
                        .Distinct()
                        .ToList();

                    foreach (var database in databases)
                    {
                        var backupsToDelete = await GetBackupsToDeleteAsync(database, policy);

                        foreach (var backup in backupsToDelete)
                        {
                            try
                            {
                                result.FreedSpaceBytes += backup.FileSizeBytes;
                                await _historyRepository.DeleteAsync(backup.Id);
                                result.DeletedCount++;

                                await _auditRepository.CreateAsync(new AuditLog
                                {
                                    Action = "AutoCleanupBackup",
                                    EntityType = "BackupHistory",
                                    EntityId = backup.BackupId,
                                    EntityName = backup.JobName,
                                    UserId = "system",
                                    Details = $"Auto-cleaned by retention policy: {policy.Name}"
                                });

                                _logger.LogInformation(
                                    "Auto-cleaned backup {BackupId} for {DatabaseName}, freed {SizeBytes} bytes",
                                    backup.BackupId,
                                    database,
                                    backup.FileSizeBytes);
                            }
                            catch (Exception ex)
                            {
                                var error = $"Failed to delete backup {backup.BackupId}: {ex.Message}";
                                result.Errors.Add(error);
                                _logger.LogError(ex, "Error deleting backup {BackupId}", backup.BackupId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var error = $"Error processing policy {policy.Name}: {ex.Message}";
                    result.Errors.Add(error);
                    _logger.LogError(ex, "Error in retention cleanup for policy {PolicyName}", policy.Name);
                }
            }
        }
        catch (Exception ex)
        {
            var error = $"Fatal error in cleanup: {ex.Message}";
            result.Errors.Add(error);
            _logger.LogError(ex, "Fatal error in retention cleanup");
        }

        return result;
    }
}

public class RetentionCleanupResult
{
    public int DeletedCount { get; set; }
    public long FreedSpaceBytes { get; set; }
    public List<string> Errors { get; set; } = new();
}
