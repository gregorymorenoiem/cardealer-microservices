using BackupDRService.Core.Entities;
using BackupDRService.Core.Repositories;
using BackupDRService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackupDRService.Tests.Services;

public class BackupHistoryServiceTests
{
    private readonly Mock<IBackupHistoryRepository> _historyRepoMock;
    private readonly Mock<IAuditLogRepository> _auditRepoMock;
    private readonly Mock<ILogger<BackupHistoryService>> _loggerMock;
    private readonly BackupHistoryService _service;

    public BackupHistoryServiceTests()
    {
        _historyRepoMock = new Mock<IBackupHistoryRepository>();
        _auditRepoMock = new Mock<IAuditLogRepository>();
        _loggerMock = new Mock<ILogger<BackupHistoryService>>();

        _service = new BackupHistoryService(
            _historyRepoMock.Object,
            _auditRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RecordBackupStartAsync_ShouldCreateHistoryRecord()
    {
        // Arrange
        _historyRepoMock.Setup(r => r.CreateAsync(It.IsAny<BackupHistory>()))
            .ReturnsAsync((BackupHistory h) =>
            {
                h.Id = 1;
                return h;
            });

        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog { Id = 1 });

        // Act
        var result = await _service.RecordBackupStartAsync(
            jobId: "job-123",
            jobName: "Daily Backup",
            databaseName: "testdb",
            backupType: "Full",
            storageType: "Local",
            scheduleId: 1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.BackupId.Should().NotBeNullOrEmpty();
        result.JobId.Should().Be("job-123");
        result.JobName.Should().Be("Daily Backup");
        result.DatabaseName.Should().Be("testdb");
        result.BackupType.Should().Be("Full");
        result.StorageType.Should().Be("Local");
        result.Status.Should().Be("InProgress");
        result.ScheduleId.Should().Be(1);
        _auditRepoMock.Verify(r => r.CreateAsync(It.Is<AuditLog>(a =>
            a.Action == "BackupStarted")), Times.Once);
    }

    [Fact]
    public async Task RecordBackupStartAsync_WithoutScheduleId_ShouldCreateHistoryRecord()
    {
        // Arrange
        _historyRepoMock.Setup(r => r.CreateAsync(It.IsAny<BackupHistory>()))
            .ReturnsAsync((BackupHistory h) => { h.Id = 1; return h; });

        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        // Act
        var result = await _service.RecordBackupStartAsync(
            jobId: "job-123",
            jobName: "Manual Backup",
            databaseName: "testdb",
            backupType: "Full",
            storageType: "Local");

        // Assert
        result.ScheduleId.Should().BeNull();
    }

    [Fact]
    public async Task RecordBackupSuccessAsync_ShouldUpdateHistoryWithSuccess()
    {
        // Arrange
        var history = new BackupHistory
        {
            Id = 1,
            BackupId = "backup-123",
            JobName = "Daily Backup",
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            Status = "InProgress"
        };

        _historyRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(history);

        _historyRepoMock.Setup(r => r.UpdateAsync(It.IsAny<BackupHistory>()))
            .ReturnsAsync((BackupHistory h) => h);

        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        // Act
        var result = await _service.RecordBackupSuccessAsync(
            historyId: 1,
            filePath: "/backups/backup-123.gz",
            fileName: "backup-123.gz",
            fileSizeBytes: 1024 * 1024,
            isCompressed: true,
            isEncrypted: false,
            checksum: "abc123");

        // Assert
        result.Status.Should().Be("Success");
        result.FilePath.Should().Be("/backups/backup-123.gz");
        result.FileName.Should().Be("backup-123.gz");
        result.FileSizeBytes.Should().Be(1024 * 1024);
        result.IsCompressed.Should().BeTrue();
        result.IsEncrypted.Should().BeFalse();
        result.Checksum.Should().Be("abc123");
        result.CompletedAt.Should().NotBeNull();
        result.Duration.Should().NotBeNull();
        _auditRepoMock.Verify(r => r.CreateAsync(It.Is<AuditLog>(a =>
            a.Action == "BackupCompleted")), Times.Once);
    }

    [Fact]
    public async Task RecordBackupSuccessAsync_WithMetadata_ShouldStoreMetadata()
    {
        // Arrange
        var history = new BackupHistory
        {
            Id = 1,
            BackupId = "backup-123",
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            Status = "InProgress"
        };

        _historyRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(history);

        _historyRepoMock.Setup(r => r.UpdateAsync(It.IsAny<BackupHistory>()))
            .ReturnsAsync((BackupHistory h) => h);

        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        var metadata = new Dictionary<string, string>
        {
            ["source"] = "manual",
            ["user"] = "admin"
        };

        // Act
        var result = await _service.RecordBackupSuccessAsync(
            historyId: 1,
            filePath: "/backups/test.gz",
            fileName: "test.gz",
            fileSizeBytes: 1024,
            isCompressed: true,
            isEncrypted: false,
            metadata: metadata);

        // Assert
        result.Metadata.Should().ContainKey("source");
        result.Metadata.Should().ContainKey("user");
    }

    [Fact]
    public async Task RecordBackupSuccessAsync_WhenHistoryNotFound_ShouldThrowException()
    {
        // Arrange
        _historyRepoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((BackupHistory?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RecordBackupSuccessAsync(999, "/path", "file.gz", 1024, true, false));
        exception.Message.Should().Contain("999");
        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task RecordBackupFailureAsync_ShouldUpdateHistoryWithFailure()
    {
        // Arrange
        var history = new BackupHistory
        {
            Id = 1,
            BackupId = "backup-123",
            JobName = "Daily Backup",
            DatabaseName = "testdb",
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            Status = "InProgress"
        };

        _historyRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(history);

        _historyRepoMock.Setup(r => r.UpdateAsync(It.IsAny<BackupHistory>()))
            .ReturnsAsync((BackupHistory h) => h);

        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        // Act
        var result = await _service.RecordBackupFailureAsync(1, "Connection timeout");

        // Assert
        result.Status.Should().Be("Failed");
        result.ErrorMessage.Should().Be("Connection timeout");
        result.CompletedAt.Should().NotBeNull();
        result.Duration.Should().NotBeNull();
        _auditRepoMock.Verify(r => r.CreateAsync(It.Is<AuditLog>(a =>
            a.Action == "BackupFailed" && a.Status == "Failed")), Times.Once);
    }

    [Fact]
    public async Task RecordBackupFailureAsync_WhenHistoryNotFound_ShouldThrowException()
    {
        // Arrange
        _historyRepoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((BackupHistory?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RecordBackupFailureAsync(999, "Error"));
        exception.Message.Should().Contain("999");
    }

    [Fact]
    public async Task GetBackupHistoryAsync_WithDatabaseName_ShouldFilterByDatabase()
    {
        // Arrange
        var histories = new List<BackupHistory>
        {
            new() { Id = 1, DatabaseName = "testdb", BackupId = "b1" },
            new() { Id = 2, DatabaseName = "testdb", BackupId = "b2" }
        };

        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync("testdb"))
            .ReturnsAsync(histories);

        // Act
        var result = await _service.GetBackupHistoryAsync(databaseName: "testdb");

        // Assert
        result.Should().HaveCount(2);
        result.All(h => h.DatabaseName == "testdb").Should().BeTrue();
    }

    [Fact]
    public async Task GetBackupHistoryAsync_WithDateRange_ShouldFilterByDate()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var histories = new List<BackupHistory>
        {
            new() { Id = 1, BackupId = "b1", StartedAt = startDate.AddDays(1) },
            new() { Id = 2, BackupId = "b2", StartedAt = startDate.AddDays(2) }
        };

        _historyRepoMock.Setup(r => r.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(histories);

        // Act
        var result = await _service.GetBackupHistoryAsync(startDate: startDate, endDate: endDate);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBackupHistoryAsync_WithStatus_ShouldFilterByStatus()
    {
        // Arrange
        var histories = new List<BackupHistory>
        {
            new() { Id = 1, BackupId = "b1", Status = "Success" },
            new() { Id = 2, BackupId = "b2", Status = "Success" }
        };

        _historyRepoMock.Setup(r => r.GetByStatusAsync("Success"))
            .ReturnsAsync(histories);

        // Act
        var result = await _service.GetBackupHistoryAsync(status: "Success");

        // Assert
        result.Should().HaveCount(2);
        result.All(h => h.Status == "Success").Should().BeTrue();
    }

    [Fact]
    public async Task GetBackupHistoryAsync_WithNoFilters_ShouldReturnAll()
    {
        // Arrange
        var histories = new List<BackupHistory>
        {
            new() { Id = 1, BackupId = "b1" },
            new() { Id = 2, BackupId = "b2" },
            new() { Id = 3, BackupId = "b3" }
        };

        _historyRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(histories);

        // Act
        var result = await _service.GetBackupHistoryAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetBackupByIdAsync_WhenExists_ShouldReturnBackup()
    {
        // Arrange
        var history = new BackupHistory { Id = 1, BackupId = "backup-123", JobName = "Test" };

        _historyRepoMock.Setup(r => r.GetByBackupIdAsync("backup-123"))
            .ReturnsAsync(history);

        // Act
        var result = await _service.GetBackupByIdAsync("backup-123");

        // Assert
        result.Should().NotBeNull();
        result!.BackupId.Should().Be("backup-123");
    }

    [Fact]
    public async Task GetBackupByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        // Arrange
        _historyRepoMock.Setup(r => r.GetByBackupIdAsync("nonexistent"))
            .ReturnsAsync((BackupHistory?)null);

        // Act
        var result = await _service.GetBackupByIdAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTotalStorageUsedAsync_ShouldReturnTotalBytes()
    {
        // Arrange
        _historyRepoMock.Setup(r => r.GetTotalStorageUsedAsync())
            .ReturnsAsync(1024L * 1024L * 1024L); // 1 GB

        // Act
        var result = await _service.GetTotalStorageUsedAsync();

        // Assert
        result.Should().Be(1024L * 1024L * 1024L);
    }

    [Fact]
    public async Task GetBackupStatisticsAsync_ShouldReturnStatistics()
    {
        // Arrange
        _historyRepoMock.Setup(r => r.GetBackupCountByStatusAsync("Success", It.IsAny<DateTime?>()))
            .ReturnsAsync(100);
        _historyRepoMock.Setup(r => r.GetBackupCountByStatusAsync("Failed", It.IsAny<DateTime?>()))
            .ReturnsAsync(5);
        _historyRepoMock.Setup(r => r.GetBackupCountByStatusAsync("InProgress", It.IsAny<DateTime?>()))
            .ReturnsAsync(1);
        _historyRepoMock.Setup(r => r.GetTotalStorageUsedAsync())
            .ReturnsAsync(1024L * 1024L * 500); // 500 MB

        // Act
        var result = await _service.GetBackupStatisticsAsync();

        // Assert
        result.Should().ContainKey("totalBackups");
        result.Should().ContainKey("failedBackups");
        result.Should().ContainKey("inProgressBackups");
        result.Should().ContainKey("successRate");
        result.Should().ContainKey("totalStorageBytes");
        result.Should().ContainKey("totalStorageMB");
        result.Should().ContainKey("totalStorageGB");

        result["totalBackups"].Should().Be(100);
        result["failedBackups"].Should().Be(5);
        result["inProgressBackups"].Should().Be(1);
        ((double)result["successRate"]).Should().BeApproximately(95.0, 0.1);
    }

    [Fact]
    public async Task GetBackupStatisticsAsync_WithSinceDate_ShouldUseFilter()
    {
        // Arrange
        var since = DateTime.UtcNow.AddDays(-30);

        _historyRepoMock.Setup(r => r.GetBackupCountByStatusAsync("Success", since))
            .ReturnsAsync(50);
        _historyRepoMock.Setup(r => r.GetBackupCountByStatusAsync("Failed", since))
            .ReturnsAsync(2);
        _historyRepoMock.Setup(r => r.GetBackupCountByStatusAsync("InProgress", since))
            .ReturnsAsync(0);
        _historyRepoMock.Setup(r => r.GetTotalStorageUsedAsync())
            .ReturnsAsync(1024);

        // Act
        var result = await _service.GetBackupStatisticsAsync(since: since);

        // Assert
        result["totalBackups"].Should().Be(50);
        result["failedBackups"].Should().Be(2);
    }

    [Fact]
    public async Task GetBackupStatisticsAsync_WithZeroBackups_ShouldReturnZeroSuccessRate()
    {
        // Arrange
        _historyRepoMock.Setup(r => r.GetBackupCountByStatusAsync(It.IsAny<string>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0);
        _historyRepoMock.Setup(r => r.GetTotalStorageUsedAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _service.GetBackupStatisticsAsync();

        // Assert
        result["totalBackups"].Should().Be(0);
        result["successRate"].Should().Be(0.0);
    }

    [Fact]
    public async Task RecordBackupStartAsync_ShouldSetStartedAtToCurrentTime()
    {
        // Arrange
        var beforeTest = DateTime.UtcNow;

        _historyRepoMock.Setup(r => r.CreateAsync(It.IsAny<BackupHistory>()))
            .ReturnsAsync((BackupHistory h) => { h.Id = 1; return h; });

        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        // Act
        var result = await _service.RecordBackupStartAsync("job", "name", "db", "Full", "Local");

        var afterTest = DateTime.UtcNow;

        // Assert
        result.StartedAt.Should().BeOnOrAfter(beforeTest);
        result.StartedAt.Should().BeOnOrBefore(afterTest);
    }
}
