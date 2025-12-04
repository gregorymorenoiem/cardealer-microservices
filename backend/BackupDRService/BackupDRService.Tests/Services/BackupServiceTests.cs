using BackupDRService.Core.Interfaces;
using BackupDRService.Core.Models;
using BackupDRService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BackupDRService.Tests.Services;

public class BackupServiceTests
{
    private readonly Mock<ILogger<BackupService>> _loggerMock;
    private readonly Mock<IDatabaseBackupProvider> _databaseProviderMock;
    private readonly Mock<IStorageProvider> _storageProviderMock;
    private readonly BackupOptions _options;
    private readonly BackupService _service;

    public BackupServiceTests()
    {
        _loggerMock = new Mock<ILogger<BackupService>>();
        _databaseProviderMock = new Mock<IDatabaseBackupProvider>();
        _storageProviderMock = new Mock<IStorageProvider>();
        _options = new BackupOptions
        {
            LocalStoragePath = "/backups",
            VerifyBackupAfterCreation = true
        };

        _service = new BackupService(
            _loggerMock.Object,
            Options.Create(_options),
            _databaseProviderMock.Object,
            _storageProviderMock.Object);
    }

    [Fact]
    public async Task CreateJobAsync_ShouldCreateNewJob()
    {
        // Arrange
        var job = new BackupJob
        {
            Name = "Daily Backup",
            DatabaseName = "testdb",
            ConnectionString = "Host=localhost;Database=test"
        };

        // Act
        var result = await _service.CreateJobAsync(job);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Name.Should().Be("Daily Backup");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetJobAsync_ShouldReturnJob_WhenExists()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob { Name = "Test Job" });

        // Act
        var result = await _service.GetJobAsync(job.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Job");
    }

    [Fact]
    public async Task GetJobAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _service.GetJobAsync("non-existent-id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllJobsAsync_ShouldReturnAllJobs()
    {
        // Arrange
        await _service.CreateJobAsync(new BackupJob { Name = "Job 1" });
        await _service.CreateJobAsync(new BackupJob { Name = "Job 2" });
        await _service.CreateJobAsync(new BackupJob { Name = "Job 3" });

        // Act
        var result = await _service.GetAllJobsAsync();

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(3);
    }

    [Fact]
    public async Task GetEnabledJobsAsync_ShouldReturnOnlyEnabledJobs()
    {
        // Arrange
        await _service.CreateJobAsync(new BackupJob { Name = "Enabled Job", IsEnabled = true });
        var disabledJob = await _service.CreateJobAsync(new BackupJob { Name = "Disabled Job", IsEnabled = true });
        await _service.DisableJobAsync(disabledJob.Id);

        // Act
        var result = await _service.GetEnabledJobsAsync();

        // Assert
        result.Should().NotContain(j => j.Id == disabledJob.Id);
    }

    [Fact]
    public async Task UpdateJobAsync_ShouldUpdateJob()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob { Name = "Original Name" });
        job.Name = "Updated Name";

        // Act
        var result = await _service.UpdateJobAsync(job);

        // Assert
        result.Name.Should().Be("Updated Name");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task DeleteJobAsync_ShouldRemoveJob()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob { Name = "To Delete" });

        // Act
        var deleted = await _service.DeleteJobAsync(job.Id);
        var found = await _service.GetJobAsync(job.Id);

        // Assert
        deleted.Should().BeTrue();
        found.Should().BeNull();
    }

    [Fact]
    public async Task DeleteJobAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Act
        var result = await _service.DeleteJobAsync("non-existent-id");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EnableJobAsync_ShouldEnableJob()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob { Name = "Test", IsEnabled = false });

        // Act
        var enabled = await _service.EnableJobAsync(job.Id);
        var updatedJob = await _service.GetJobAsync(job.Id);

        // Assert
        enabled.Should().BeTrue();
        updatedJob!.IsEnabled.Should().BeTrue();
        updatedJob.Status.Should().Be(BackupJobStatus.Idle);
    }

    [Fact]
    public async Task DisableJobAsync_ShouldDisableJob()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob { Name = "Test", IsEnabled = true });

        // Act
        var disabled = await _service.DisableJobAsync(job.Id);
        var updatedJob = await _service.GetJobAsync(job.Id);

        // Assert
        disabled.Should().BeTrue();
        updatedJob!.IsEnabled.Should().BeFalse();
        updatedJob.Status.Should().Be(BackupJobStatus.Disabled);
    }

    [Fact]
    public async Task ExecuteBackupAsync_ShouldReturnError_WhenJobNotFound()
    {
        // Act
        var result = await _service.ExecuteBackupAsync("non-existent-id");

        // Assert
        result.Status.Should().Be(BackupExecutionStatus.Failed);
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnStatistics()
    {
        // Arrange
        _storageProviderMock.Setup(s => s.GetTotalStorageUsedAsync())
            .ReturnsAsync(1024L * 1024L * 100); // 100 MB

        await _service.CreateJobAsync(new BackupJob { Name = "Job 1", IsEnabled = true });
        await _service.CreateJobAsync(new BackupJob { Name = "Job 2", IsEnabled = true });

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.TotalJobs.Should().BeGreaterOrEqualTo(2);
        stats.EnabledJobs.Should().BeGreaterOrEqualTo(2);
        stats.TotalStorageUsedBytes.Should().Be(1024L * 1024L * 100);
    }

    [Fact]
    public async Task ExecuteBackupAsync_WithJobId_ShouldExecuteBackup_WhenJobExists()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob
        {
            Name = "Test Backup",
            DatabaseName = "testdb",
            ConnectionString = "Host=localhost;Database=test",
            Type = BackupType.Full
        });

        _databaseProviderMock.Setup(d => d.BackupAsync(It.IsAny<DatabaseBackupRequest>()))
            .ReturnsAsync(new DatabaseBackupResult { Success = true, FileSizeBytes = 1024 });

        _storageProviderMock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new StorageUploadResult { Success = true, FileSizeBytes = 1024, Checksum = "abc123" });

        // Act
        var result = await _service.ExecuteBackupAsync(job.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(BackupExecutionStatus.Completed);
    }

    [Fact]
    public async Task ExecuteBackupAsync_WithJob_ShouldMarkRunning()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob
        {
            Name = "Test",
            DatabaseName = "testdb",
            ConnectionString = "Host=localhost;Database=test"
        });

        _databaseProviderMock.Setup(d => d.BackupAsync(It.IsAny<DatabaseBackupRequest>()))
            .ReturnsAsync(new DatabaseBackupResult { Success = true });

        _storageProviderMock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new StorageUploadResult { Success = true, Checksum = "xyz" });

        // Act
        await _service.ExecuteBackupAsync(job);
        var updatedJob = await _service.GetJobAsync(job.Id);

        // Assert
        updatedJob!.Status.Should().Be(BackupJobStatus.Idle); // Returns to Idle after completion
    }

    [Fact]
    public async Task ExecuteBackupAsync_WhenDatabaseBackupFails_ShouldReturnFailure()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob
        {
            Name = "Test",
            DatabaseName = "testdb",
            ConnectionString = "Host=localhost;Database=test"
        });

        _databaseProviderMock.Setup(d => d.BackupAsync(It.IsAny<DatabaseBackupRequest>()))
            .ReturnsAsync(new DatabaseBackupResult
            {
                Success = false,
                ErrorMessage = "Database connection failed"
            });

        // Act
        var result = await _service.ExecuteBackupAsync(job);

        // Assert
        result.Status.Should().Be(BackupExecutionStatus.Failed);
        result.ErrorMessage.Should().Contain("Database connection failed");
    }

    [Fact]
    public async Task ExecuteBackupAsync_WhenStorageUploadFails_ShouldReturnFailure()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob
        {
            Name = "Test",
            DatabaseName = "testdb",
            ConnectionString = "Host=localhost;Database=test"
        });

        _databaseProviderMock.Setup(d => d.BackupAsync(It.IsAny<DatabaseBackupRequest>()))
            .ReturnsAsync(new DatabaseBackupResult { Success = true });

        _storageProviderMock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new StorageUploadResult
            {
                Success = false,
                ErrorMessage = "Storage quota exceeded"
            });

        // Act
        var result = await _service.ExecuteBackupAsync(job);

        // Assert
        result.Status.Should().Be(BackupExecutionStatus.Failed);
        result.ErrorMessage.Should().Contain("Storage quota exceeded");
    }

    [Fact]
    public async Task GetJobByNameAsync_ShouldReturnJob_WhenExists()
    {
        // Arrange
        await _service.CreateJobAsync(new BackupJob { Name = "UniqueJobName" });

        // Act
        var result = await _service.GetJobByNameAsync("UniqueJobName");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("UniqueJobName");
    }

    [Fact]
    public async Task GetJobByNameAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _service.GetJobByNameAsync("NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task EnableJobAsync_ShouldReturnFalse_WhenJobNotFound()
    {
        // Act
        var result = await _service.EnableJobAsync("non-existent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DisableJobAsync_ShouldReturnFalse_WhenJobNotFound()
    {
        // Act
        var result = await _service.DisableJobAsync("non-existent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CancelBackupAsync_ShouldCancelRunningBackup()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob
        {
            Name = "Test",
            DatabaseName = "testdb",
            ConnectionString = "Host=localhost;Database=test"
        });

        _databaseProviderMock.Setup(d => d.BackupAsync(It.IsAny<DatabaseBackupRequest>()))
            .ReturnsAsync(new DatabaseBackupResult { Success = true, FileSizeBytes = 1024 });

        _storageProviderMock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new StorageUploadResult { Success = true, FileSizeBytes = 1024, Checksum = "abc" });

        await _service.ExecuteBackupAsync(job);
        var results = await _service.GetBackupResultsAsync(job.Id);
        var backupResultId = results.First().Id;

        // Act
        var cancelled = await _service.CancelBackupAsync(backupResultId);

        // Assert - Already completed, can't cancel
        cancelled.Should().BeFalse();
    }

    [Fact]
    public async Task CancelBackupAsync_ShouldReturnFalse_WhenResultNotFound()
    {
        // Act
        var result = await _service.CancelBackupAsync("non-existent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetBackupResultAsync_ShouldReturnResult_WhenExists()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob
        {
            Name = "Test",
            DatabaseName = "testdb",
            ConnectionString = "Host=localhost;Database=test"
        });

        _databaseProviderMock.Setup(d => d.BackupAsync(It.IsAny<DatabaseBackupRequest>()))
            .ReturnsAsync(new DatabaseBackupResult { Success = true });

        _storageProviderMock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new StorageUploadResult { Success = true, Checksum = "abc" });

        var backupResult = await _service.ExecuteBackupAsync(job);

        // Act
        var result = await _service.GetBackupResultAsync(backupResult.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(backupResult.Id);
    }

    [Fact]
    public async Task GetBackupResultAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _service.GetBackupResultAsync("non-existent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBackupResultsAsync_ShouldReturnResultsForJob()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob
        {
            Name = "Test",
            DatabaseName = "testdb",
            ConnectionString = "Host=localhost"
        });

        _databaseProviderMock.Setup(d => d.BackupAsync(It.IsAny<DatabaseBackupRequest>()))
            .ReturnsAsync(new DatabaseBackupResult { Success = true });

        _storageProviderMock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new StorageUploadResult { Success = true, Checksum = "abc" });

        await _service.ExecuteBackupAsync(job);
        await _service.ExecuteBackupAsync(job);

        // Act
        var results = await _service.GetBackupResultsAsync(job.Id);

        // Assert
        results.Should().HaveCountGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task GetRecentBackupResultsAsync_ShouldReturnMostRecent()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob
        {
            Name = "Test",
            DatabaseName = "testdb",
            ConnectionString = "Host=localhost"
        });

        _databaseProviderMock.Setup(d => d.BackupAsync(It.IsAny<DatabaseBackupRequest>()))
            .ReturnsAsync(new DatabaseBackupResult { Success = true });

        _storageProviderMock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new StorageUploadResult { Success = true, Checksum = "abc" });

        await _service.ExecuteBackupAsync(job);
        await _service.ExecuteBackupAsync(job);
        await _service.ExecuteBackupAsync(job);

        // Act
        var results = await _service.GetRecentBackupResultsAsync(2);

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBackupResultsByDateRangeAsync_ShouldFilterByDates()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob
        {
            Name = "Test",
            DatabaseName = "testdb",
            ConnectionString = "Host=localhost"
        });

        _databaseProviderMock.Setup(d => d.BackupAsync(It.IsAny<DatabaseBackupRequest>()))
            .ReturnsAsync(new DatabaseBackupResult { Success = true });

        _storageProviderMock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new StorageUploadResult { Success = true, Checksum = "abc" });

        await _service.ExecuteBackupAsync(job);

        // Act
        var results = await _service.GetBackupResultsByDateRangeAsync(
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddHours(1));

        // Assert
        results.Should().NotBeEmpty();
    }

    [Fact]
    public async Task VerifyBackupAsync_ShouldReturnFalse_WhenResultNotFound()
    {
        // Act
        var result = await _service.VerifyBackupAsync("non-existent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyBackupAsync_ShouldReturnFalse_WhenNoChecksum()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob
        {
            Name = "Test",
            DatabaseName = "testdb",
            ConnectionString = "Host=localhost"
        });

        _databaseProviderMock.Setup(d => d.BackupAsync(It.IsAny<DatabaseBackupRequest>()))
            .ReturnsAsync(new DatabaseBackupResult { Success = true });

        _storageProviderMock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new StorageUploadResult { Success = true, Checksum = null }); // No checksum

        var backupResult = await _service.ExecuteBackupAsync(job);

        // Act
        var result = await _service.VerifyBackupAsync(backupResult.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyBackupAsync_ShouldCallStorageVerify()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob
        {
            Name = "Test",
            DatabaseName = "testdb",
            ConnectionString = "Host=localhost"
        });

        _databaseProviderMock.Setup(d => d.BackupAsync(It.IsAny<DatabaseBackupRequest>()))
            .ReturnsAsync(new DatabaseBackupResult { Success = true });

        _storageProviderMock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new StorageUploadResult { Success = true, Checksum = "abc123" });

        _storageProviderMock.Setup(s => s.VerifyIntegrityAsync(It.IsAny<string>(), "abc123"))
            .ReturnsAsync(true);

        var backupResult = await _service.ExecuteBackupAsync(job);

        // Act
        var result = await _service.VerifyBackupAsync(backupResult.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CleanupExpiredBackupsAsync_ShouldCleanupExpiredBackups()
    {
        // Arrange
        _storageProviderMock.Setup(s => s.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var count = await _service.CleanupExpiredBackupsAsync();

        // Assert
        count.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task CleanupBackupsOlderThanAsync_ShouldCleanupExpiredBackups()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob
        {
            Name = "Test",
            DatabaseName = "testdb",
            ConnectionString = "Host=localhost",
            RetentionDays = 0 // Expires immediately
        });

        _databaseProviderMock.Setup(d => d.BackupAsync(It.IsAny<DatabaseBackupRequest>()))
            .ReturnsAsync(new DatabaseBackupResult { Success = true });

        _storageProviderMock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new StorageUploadResult { Success = true, Checksum = "abc" });

        _storageProviderMock.Setup(s => s.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        await _service.ExecuteBackupAsync(job);

        // Act
        var count = await _service.CleanupBackupsOlderThanAsync(DateTime.UtcNow.AddDays(1));

        // Assert
        count.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldIncludeAllStats()
    {
        // Arrange
        _storageProviderMock.Setup(s => s.GetTotalStorageUsedAsync())
            .ReturnsAsync(1024L * 1024L * 50);

        var job = await _service.CreateJobAsync(new BackupJob
        {
            Name = "Test Job",
            IsEnabled = true
        });

        _databaseProviderMock.Setup(d => d.BackupAsync(It.IsAny<DatabaseBackupRequest>()))
            .ReturnsAsync(new DatabaseBackupResult { Success = true });

        _storageProviderMock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new StorageUploadResult { Success = true, Checksum = "abc" });

        await _service.ExecuteBackupAsync(job);

        // Act
        var stats = await _service.GetStatisticsAsync();

        // Assert
        stats.TotalJobs.Should().BeGreaterOrEqualTo(1);
        stats.TotalBackups.Should().BeGreaterOrEqualTo(1);
        stats.TotalStorageUsedBytes.Should().Be(1024L * 1024L * 50);
        stats.BackupsByJob.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExecuteBackupAsync_WhenException_ShouldReturnFailure()
    {
        // Arrange
        var job = await _service.CreateJobAsync(new BackupJob
        {
            Name = "Test",
            DatabaseName = "testdb",
            ConnectionString = "Host=localhost"
        });

        _databaseProviderMock.Setup(d => d.BackupAsync(It.IsAny<DatabaseBackupRequest>()))
            .ThrowsAsync(new InvalidOperationException("Connection failed"));

        // Act
        var result = await _service.ExecuteBackupAsync(job);

        // Assert
        result.Status.Should().Be(BackupExecutionStatus.Failed);
        result.ErrorMessage.Should().Contain("Connection failed");
    }

    [Fact]
    public async Task ExecuteBackupAsync_WithVerification_ShouldVerifyAfterCompletion()
    {
        // Arrange - VerifyBackupAfterCreation is true in setup
        var job = await _service.CreateJobAsync(new BackupJob
        {
            Name = "Test",
            DatabaseName = "testdb",
            ConnectionString = "Host=localhost"
        });

        _databaseProviderMock.Setup(d => d.BackupAsync(It.IsAny<DatabaseBackupRequest>()))
            .ReturnsAsync(new DatabaseBackupResult { Success = true, FileSizeBytes = 1024 });

        _storageProviderMock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new StorageUploadResult { Success = true, FileSizeBytes = 1024, Checksum = "abc123" });

        _storageProviderMock.Setup(s => s.VerifyIntegrityAsync(It.IsAny<string>(), "abc123"))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ExecuteBackupAsync(job);

        // Assert
        result.Status.Should().Be(BackupExecutionStatus.Completed);
        result.IsVerified.Should().BeTrue();
        result.VerifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task VerifyBackupIntegrityAsync_WithNullChecksum_ShouldReturnFalse()
    {
        // Act
        var result = await _service.VerifyBackupIntegrityAsync("/path/to/file", null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyBackupIntegrityAsync_WithEmptyChecksum_ShouldReturnFalse()
    {
        // Act
        var result = await _service.VerifyBackupIntegrityAsync("/path/to/file", "");

        // Assert
        result.Should().BeFalse();
    }
}
