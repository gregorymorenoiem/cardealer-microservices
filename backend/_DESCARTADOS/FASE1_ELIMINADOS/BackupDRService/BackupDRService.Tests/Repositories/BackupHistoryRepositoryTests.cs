using BackupDRService.Core.Data;
using BackupDRService.Core.Entities;
using BackupDRService.Core.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BackupDRService.Tests.Repositories;

public class BackupHistoryRepositoryTests : IDisposable
{
    private readonly BackupDbContext _context;
    private readonly BackupHistoryRepository _repository;

    public BackupHistoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BackupDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BackupDbContext(options);
        _repository = new BackupHistoryRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateHistoryRecord()
    {
        // Arrange
        var history = new BackupHistory
        {
            BackupId = "test-backup-001",
            JobId = "job-001",
            JobName = "Daily Backup",
            DatabaseName = "testdb",
            BackupType = "Full",
            StorageType = "Local",
            Status = "InProgress",
            StartedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(history);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.BackupId.Should().Be("test-backup-001");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnHistory_WhenExists()
    {
        // Arrange
        var history = new BackupHistory
        {
            BackupId = "test-backup-002",
            JobId = "job-002",
            JobName = "Test",
            DatabaseName = "testdb",
            Status = "Success"
        };
        await _repository.CreateAsync(history);

        // Act
        var result = await _repository.GetByIdAsync(history.Id);

        // Assert
        result.Should().NotBeNull();
        result!.BackupId.Should().Be("test-backup-002");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByBackupIdAsync_ShouldReturnHistory_WhenExists()
    {
        // Arrange
        var history = new BackupHistory
        {
            BackupId = "unique-backup-id",
            JobId = "job-003",
            JobName = "Test",
            DatabaseName = "testdb",
            Status = "Success"
        };
        await _repository.CreateAsync(history);

        // Act
        var result = await _repository.GetByBackupIdAsync("unique-backup-id");

        // Assert
        result.Should().NotBeNull();
        result!.JobId.Should().Be("job-003");
    }

    [Fact]
    public async Task GetByBackupIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByBackupIdAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllHistories()
    {
        // Arrange
        await _repository.CreateAsync(new BackupHistory { BackupId = "b1", JobId = "j1", JobName = "Job1", DatabaseName = "db1", Status = "Success" });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b2", JobId = "j2", JobName = "Job2", DatabaseName = "db2", Status = "Success" });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b3", JobId = "j3", JobName = "Job3", DatabaseName = "db3", Status = "Failed" });

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByJobIdAsync_ShouldReturnHistoriesForJob()
    {
        // Arrange
        await _repository.CreateAsync(new BackupHistory { BackupId = "b1", JobId = "target-job", JobName = "Job", DatabaseName = "db", Status = "Success" });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b2", JobId = "target-job", JobName = "Job", DatabaseName = "db", Status = "Success" });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b3", JobId = "other-job", JobName = "Other", DatabaseName = "db", Status = "Success" });

        // Act
        var results = await _repository.GetByJobIdAsync("target-job");

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(h => h.JobId.Should().Be("target-job"));
    }

    [Fact]
    public async Task GetByDatabaseNameAsync_ShouldReturnHistoriesForDatabase()
    {
        // Arrange
        await _repository.CreateAsync(new BackupHistory { BackupId = "b1", JobId = "j1", JobName = "J1", DatabaseName = "targetdb", Status = "Success" });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b2", JobId = "j2", JobName = "J2", DatabaseName = "targetdb", Status = "Success" });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b3", JobId = "j3", JobName = "J3", DatabaseName = "otherdb", Status = "Success" });

        // Act
        var results = await _repository.GetByDatabaseNameAsync("targetdb");

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(h => h.DatabaseName.Should().Be("targetdb"));
    }

    [Fact]
    public async Task GetByDateRangeAsync_ShouldReturnHistoriesInRange()
    {
        // Arrange
        var now = DateTime.UtcNow;
        await _repository.CreateAsync(new BackupHistory { BackupId = "b1", JobId = "j1", JobName = "J", DatabaseName = "db", Status = "Success", StartedAt = now.AddDays(-5) });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b2", JobId = "j2", JobName = "J", DatabaseName = "db", Status = "Success", StartedAt = now.AddDays(-2) });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b3", JobId = "j3", JobName = "J", DatabaseName = "db", Status = "Success", StartedAt = now.AddDays(-10) });

        // Act
        var results = await _repository.GetByDateRangeAsync(now.AddDays(-7), now);

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnHistoriesWithStatus()
    {
        // Arrange
        await _repository.CreateAsync(new BackupHistory { BackupId = "b1", JobId = "j1", JobName = "J", DatabaseName = "db", Status = "Success" });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b2", JobId = "j2", JobName = "J", DatabaseName = "db", Status = "Success" });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b3", JobId = "j3", JobName = "J", DatabaseName = "db", Status = "Failed" });

        // Act
        var results = await _repository.GetByStatusAsync("Success");

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(h => h.Status.Should().Be("Success"));
    }

    [Fact]
    public async Task GetRecentAsync_ShouldReturnMostRecentHistories()
    {
        // Arrange
        var now = DateTime.UtcNow;
        await _repository.CreateAsync(new BackupHistory { BackupId = "b1", JobId = "j", JobName = "J", DatabaseName = "db", Status = "Success", StartedAt = now.AddHours(-3) });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b2", JobId = "j", JobName = "J", DatabaseName = "db", Status = "Success", StartedAt = now.AddHours(-2) });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b3", JobId = "j", JobName = "J", DatabaseName = "db", Status = "Success", StartedAt = now.AddHours(-1) });

        // Act
        var results = await _repository.GetRecentAsync(2);

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateHistory()
    {
        // Arrange
        var history = new BackupHistory
        {
            BackupId = "update-test",
            JobId = "j",
            JobName = "J",
            DatabaseName = "db",
            Status = "InProgress"
        };
        await _repository.CreateAsync(history);

        // Act
        history.Status = "Success";
        history.CompletedAt = DateTime.UtcNow;
        var result = await _repository.UpdateAsync(history);

        // Assert
        result.Status.Should().Be("Success");
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveHistory()
    {
        // Arrange
        var history = new BackupHistory
        {
            BackupId = "delete-test",
            JobId = "j",
            JobName = "J",
            DatabaseName = "db",
            Status = "Success"
        };
        await _repository.CreateAsync(history);
        var id = history.Id;

        // Act
        await _repository.DeleteAsync(id);
        var result = await _repository.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotThrow_WhenNotExists()
    {
        // Act & Assert - Should not throw
        await _repository.DeleteAsync(999);
    }

    [Fact]
    public async Task GetTotalStorageUsedAsync_ShouldReturnTotalBytes()
    {
        // Arrange
        await _repository.CreateAsync(new BackupHistory { BackupId = "b1", JobId = "j", JobName = "J", DatabaseName = "db", Status = "Success", FileSizeBytes = 1000 });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b2", JobId = "j", JobName = "J", DatabaseName = "db", Status = "Success", FileSizeBytes = 2000 });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b3", JobId = "j", JobName = "J", DatabaseName = "db", Status = "Failed", FileSizeBytes = 500 }); // Failed, not counted

        // Act
        var result = await _repository.GetTotalStorageUsedAsync();

        // Assert
        result.Should().Be(3000);
    }

    [Fact]
    public async Task GetStorageUsedByDatabaseAsync_ShouldReturnBytesForDatabase()
    {
        // Arrange
        await _repository.CreateAsync(new BackupHistory { BackupId = "b1", JobId = "j", JobName = "J", DatabaseName = "targetdb", Status = "Success", FileSizeBytes = 1000 });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b2", JobId = "j", JobName = "J", DatabaseName = "targetdb", Status = "Success", FileSizeBytes = 2000 });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b3", JobId = "j", JobName = "J", DatabaseName = "otherdb", Status = "Success", FileSizeBytes = 5000 });

        // Act
        var result = await _repository.GetStorageUsedByDatabaseAsync("targetdb");

        // Assert
        result.Should().Be(3000);
    }

    [Fact]
    public async Task GetBackupCountByStatusAsync_ShouldReturnCount()
    {
        // Arrange
        await _repository.CreateAsync(new BackupHistory { BackupId = "b1", JobId = "j", JobName = "J", DatabaseName = "db", Status = "Success" });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b2", JobId = "j", JobName = "J", DatabaseName = "db", Status = "Success" });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b3", JobId = "j", JobName = "J", DatabaseName = "db", Status = "Failed" });

        // Act
        var successCount = await _repository.GetBackupCountByStatusAsync("Success");
        var failedCount = await _repository.GetBackupCountByStatusAsync("Failed");

        // Assert
        successCount.Should().Be(2);
        failedCount.Should().Be(1);
    }

    [Fact]
    public async Task GetBackupCountByStatusAsync_WithSince_ShouldFilterByDate()
    {
        // Arrange
        var now = DateTime.UtcNow;
        await _repository.CreateAsync(new BackupHistory { BackupId = "b1", JobId = "j", JobName = "J", DatabaseName = "db", Status = "Success", StartedAt = now.AddDays(-2) });
        await _repository.CreateAsync(new BackupHistory { BackupId = "b2", JobId = "j", JobName = "J", DatabaseName = "db", Status = "Success", StartedAt = now.AddDays(-10) });

        // Act
        var result = await _repository.GetBackupCountByStatusAsync("Success", now.AddDays(-5));

        // Assert
        result.Should().Be(1);
    }
}
