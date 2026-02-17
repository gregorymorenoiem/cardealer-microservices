using BackupDRService.Core.Entities;
using BackupDRService.Core.Repositories;
using BackupDRService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackupDRService.Tests.Services;

public class RetentionServiceTests
{
    private readonly Mock<IBackupHistoryRepository> _historyRepoMock;
    private readonly Mock<IRetentionPolicyRepository> _policyRepoMock;
    private readonly Mock<IAuditLogRepository> _auditRepoMock;
    private readonly Mock<ILogger<RetentionService>> _loggerMock;
    private readonly RetentionService _service;

    public RetentionServiceTests()
    {
        _historyRepoMock = new Mock<IBackupHistoryRepository>();
        _policyRepoMock = new Mock<IRetentionPolicyRepository>();
        _auditRepoMock = new Mock<IAuditLogRepository>();
        _loggerMock = new Mock<ILogger<RetentionService>>();

        _service = new RetentionService(
            _historyRepoMock.Object,
            _policyRepoMock.Object,
            _auditRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreatePolicyAsync_ShouldCreateAndAuditPolicy()
    {
        // Arrange
        var policy = new RetentionPolicy
        {
            Name = "Default Policy",
            DailyRetentionDays = 7,
            WeeklyRetentionWeeks = 4,
            MonthlyRetentionMonths = 12,
            YearlyRetentionYears = 3
        };

        _policyRepoMock.Setup(r => r.CreateAsync(It.IsAny<RetentionPolicy>()))
            .ReturnsAsync((RetentionPolicy p) =>
            {
                p.Id = 1;
                return p;
            });

        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog { Id = 1 });

        // Act
        var result = await _service.CreatePolicyAsync(policy, "testuser");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Default Policy");
        _auditRepoMock.Verify(r => r.CreateAsync(It.Is<AuditLog>(a =>
            a.Action == "RetentionPolicyCreated" && a.UserId == "testuser")), Times.Once);
    }

    [Fact]
    public async Task CreatePolicyAsync_WithDefaultUser_ShouldUseSystemUser()
    {
        // Arrange
        var policy = new RetentionPolicy { Name = "Test Policy" };

        _policyRepoMock.Setup(r => r.CreateAsync(It.IsAny<RetentionPolicy>()))
            .ReturnsAsync((RetentionPolicy p) => { p.Id = 1; return p; });

        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        // Act
        await _service.CreatePolicyAsync(policy);

        // Assert
        _auditRepoMock.Verify(r => r.CreateAsync(It.Is<AuditLog>(a =>
            a.UserId == "system")), Times.Once);
    }

    [Fact]
    public async Task UpdatePolicyAsync_WhenPolicyExists_ShouldUpdateAndAudit()
    {
        // Arrange
        var existingPolicy = new RetentionPolicy
        {
            Id = 1,
            Name = "Existing",
            DailyRetentionDays = 7
        };
        var updatedPolicy = new RetentionPolicy
        {
            Id = 1,
            Name = "Updated",
            DailyRetentionDays = 14
        };

        _policyRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingPolicy);
        _policyRepoMock.Setup(r => r.UpdateAsync(It.IsAny<RetentionPolicy>()))
            .ReturnsAsync(updatedPolicy);
        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        // Act
        var result = await _service.UpdatePolicyAsync(updatedPolicy, "admin");

        // Assert
        result.Name.Should().Be("Updated");
        _auditRepoMock.Verify(r => r.CreateAsync(It.Is<AuditLog>(a =>
            a.Action == "RetentionPolicyUpdated")), Times.Once);
    }

    [Fact]
    public async Task UpdatePolicyAsync_WhenPolicyNotExists_ShouldThrowException()
    {
        // Arrange
        var policy = new RetentionPolicy { Id = 999, Name = "NonExistent" };

        _policyRepoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((RetentionPolicy?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdatePolicyAsync(policy));
        exception.Message.Should().Contain("999");
        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task GetBackupsToRetainAsync_ShouldRetainDailyBackups()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var backups = new List<BackupHistory>
        {
            new() { Id = 1, BackupId = "b1", StartedAt = now.AddDays(-1), Status = "Success" },
            new() { Id = 2, BackupId = "b2", StartedAt = now.AddDays(-3), Status = "Success" },
            new() { Id = 3, BackupId = "b3", StartedAt = now.AddDays(-5), Status = "Success" },
        };
        var policy = new RetentionPolicy
        {
            DailyRetentionDays = 7,
            WeeklyRetentionWeeks = 4,
            MonthlyRetentionMonths = 3,
            YearlyRetentionYears = 1
        };

        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync("testdb"))
            .ReturnsAsync(backups);

        // Act
        var result = await _service.GetBackupsToRetainAsync("testdb", policy);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(b => b.BackupId == "b1");
        result.Should().Contain(b => b.BackupId == "b2");
        result.Should().Contain(b => b.BackupId == "b3");
    }

    [Fact]
    public async Task GetBackupsToRetainAsync_ShouldExcludeFailedBackups()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var backups = new List<BackupHistory>
        {
            new() { Id = 1, BackupId = "b1", StartedAt = now.AddDays(-1), Status = "Success" },
            new() { Id = 2, BackupId = "b2", StartedAt = now.AddDays(-2), Status = "Failed" },
        };
        var policy = new RetentionPolicy
        {
            DailyRetentionDays = 7,
            WeeklyRetentionWeeks = 4,
            MonthlyRetentionMonths = 3,
            YearlyRetentionYears = 1
        };

        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync("testdb"))
            .ReturnsAsync(backups);

        // Act
        var result = await _service.GetBackupsToRetainAsync("testdb", policy);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(b => b.BackupId == "b1");
        result.Should().NotContain(b => b.Status == "Failed");
    }

    [Fact]
    public async Task GetBackupsToDeleteAsync_ShouldIdentifyExpiredBackups()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var backups = new List<BackupHistory>
        {
            new() { Id = 1, BackupId = "b1", StartedAt = now.AddDays(-1), Status = "Success" },
            new() { Id = 2, BackupId = "b2", StartedAt = now.AddYears(-5), Status = "Success" }, // Way outside retention
        };
        var policy = new RetentionPolicy
        {
            DailyRetentionDays = 7,
            WeeklyRetentionWeeks = 4,
            MonthlyRetentionMonths = 2,
            YearlyRetentionYears = 1
        };

        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync("testdb"))
            .ReturnsAsync(backups);

        // Act
        var result = await _service.GetBackupsToDeleteAsync("testdb", policy);

        // Assert
        result.Should().Contain(b => b.BackupId == "b2");
    }

    [Fact]
    public async Task GetBackupsToDeleteAsync_WithMaxStorageLimit_ShouldEnforceLimit()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var backups = new List<BackupHistory>
        {
            new() { Id = 1, BackupId = "b1", StartedAt = now.AddDays(-1), Status = "Success", FileSizeBytes = 500 },
            new() { Id = 2, BackupId = "b2", StartedAt = now.AddDays(-2), Status = "Success", FileSizeBytes = 500 },
            new() { Id = 3, BackupId = "b3", StartedAt = now.AddDays(-3), Status = "Success", FileSizeBytes = 500 },
        };
        var policy = new RetentionPolicy
        {
            DailyRetentionDays = 7,
            WeeklyRetentionWeeks = 4,
            MonthlyRetentionMonths = 3,
            YearlyRetentionYears = 1,
            MaxStorageSizeBytes = 800 // Only allow 800 bytes total
        };

        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync("testdb"))
            .ReturnsAsync(backups);

        // Act
        var result = await _service.GetBackupsToDeleteAsync("testdb", policy);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetBackupsToDeleteAsync_WithMaxBackupCount_ShouldEnforceLimit()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var backups = Enumerable.Range(1, 10).Select(i => new BackupHistory
        {
            Id = i,
            BackupId = $"b{i}",
            StartedAt = now.AddDays(-i),
            Status = "Success"
        }).ToList();

        var policy = new RetentionPolicy
        {
            DailyRetentionDays = 30, // Keep all daily
            WeeklyRetentionWeeks = 4,
            MonthlyRetentionMonths = 3,
            YearlyRetentionYears = 1,
            MaxBackupCount = 5 // But limit to 5 total
        };

        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync("testdb"))
            .ReturnsAsync(backups);

        // Act
        var result = await _service.GetBackupsToDeleteAsync("testdb", policy);

        // Assert
        result.Count().Should().BeGreaterOrEqualTo(5);
    }

    [Fact]
    public async Task ApplyRetentionPolicyAsync_ShouldDeleteExpiredBackups()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var backups = new List<BackupHistory>
        {
            new() { Id = 1, BackupId = "b1", JobName = "Job1", StartedAt = now.AddDays(-1), Status = "Success" },
            new() { Id = 2, BackupId = "b2", JobName = "Job1", StartedAt = now.AddYears(-5), Status = "Success" }, // Way outside retention
        };
        var policy = new RetentionPolicy
        {
            Name = "Test Policy",
            DailyRetentionDays = 7,
            WeeklyRetentionWeeks = 4,
            MonthlyRetentionMonths = 2,
            YearlyRetentionYears = 1
        };

        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync("testdb"))
            .ReturnsAsync(backups);
        _historyRepoMock.Setup(r => r.DeleteAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        var deletedFiles = new List<string>();
        Func<BackupHistory, Task> deleteFunc = b =>
        {
            deletedFiles.Add(b.BackupId);
            return Task.CompletedTask;
        };

        // Act
        await _service.ApplyRetentionPolicyAsync("testdb", policy, deleteFunc);

        // Assert
        deletedFiles.Should().Contain("b2");
        _historyRepoMock.Verify(r => r.DeleteAsync(2), Times.Once);
    }

    [Fact]
    public async Task ApplyRetentionPolicyAsync_WhenDeleteFails_ShouldLogError()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var backups = new List<BackupHistory>
        {
            new() { Id = 1, BackupId = "b1", JobName = "Job1", StartedAt = now.AddYears(-5), Status = "Success" }, // Way outside all retention
        };
        var policy = new RetentionPolicy
        {
            Name = "Test",
            DailyRetentionDays = 1,
            WeeklyRetentionWeeks = 1,
            MonthlyRetentionMonths = 1,
            YearlyRetentionYears = 1
        };

        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync("testdb"))
            .ReturnsAsync(backups);
        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        Func<BackupHistory, Task> deleteFunc = _ => throw new IOException("Disk error");

        // Act
        await _service.ApplyRetentionPolicyAsync("testdb", policy, deleteFunc);

        // Assert
        _auditRepoMock.Verify(r => r.CreateAsync(It.Is<AuditLog>(a =>
            a.Action == "BackupDeletionFailed" && a.Status == "Failed")), Times.Once);
    }

    [Fact]
    public async Task CleanupExpiredBackupsAsync_ShouldReturnCleanupResult()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var policies = new List<RetentionPolicy>
        {
            new() { Id = 1, Name = "Default", DailyRetentionDays = 1, WeeklyRetentionWeeks = 1, MonthlyRetentionMonths = 1, YearlyRetentionYears = 1 }
        };
        var backups = new List<BackupHistory>
        {
            new() { Id = 1, BackupId = "b1", DatabaseName = "db1", StartedAt = now.AddDays(-100), Status = "Success", FileSizeBytes = 1024 },
        };

        _policyRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(policies);
        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(backups);
        _historyRepoMock.Setup(r => r.DeleteAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        // Act
        var result = await _service.CleanupExpiredBackupsAsync();

        // Assert
        result.Should().NotBeNull();
        result.DeletedCount.Should().BeGreaterOrEqualTo(0);
        result.FreedSpaceBytes.Should().BeGreaterOrEqualTo(0);
        result.Errors.Should().NotBeNull();
    }

    [Fact]
    public async Task CleanupExpiredBackupsAsync_WhenPolicyError_ShouldLogAndContinue()
    {
        // Arrange
        _policyRepoMock.Setup(r => r.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.CleanupExpiredBackupsAsync();

        // Assert
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(e => e.Contains("Fatal error"));
    }

    [Fact]
    public async Task CleanupExpiredBackupsAsync_WhenHistoryRepoFails_ShouldAddError()
    {
        // Arrange
        var policies = new List<RetentionPolicy>
        {
            new() { Id = 1, Name = "Default", DailyRetentionDays = 1 }
        };

        _policyRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(policies);
        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("History error"));

        // Act
        var result = await _service.CleanupExpiredBackupsAsync();

        // Assert
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetBackupsToRetainAsync_ShouldRetainWeeklyBackups()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var backups = new List<BackupHistory>
        {
            new() { Id = 1, BackupId = "b1", StartedAt = now.AddDays(-8), Status = "Success" }, // Last week
            new() { Id = 2, BackupId = "b2", StartedAt = now.AddDays(-9), Status = "Success" }, // Also last week
            new() { Id = 3, BackupId = "b3", StartedAt = now.AddDays(-15), Status = "Success" }, // Two weeks ago
        };
        var policy = new RetentionPolicy
        {
            DailyRetentionDays = 7, // Daily retention ends at 7 days
            WeeklyRetentionWeeks = 4,
            MonthlyRetentionMonths = 3,
            YearlyRetentionYears = 1
        };

        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync("testdb"))
            .ReturnsAsync(backups);

        // Act
        var result = await _service.GetBackupsToRetainAsync("testdb", policy);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetBackupsToRetainAsync_ShouldRetainMonthlyBackups()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var backups = new List<BackupHistory>
        {
            new() { Id = 1, BackupId = "b1", StartedAt = now.AddMonths(-2), Status = "Success" },
            new() { Id = 2, BackupId = "b2", StartedAt = now.AddMonths(-2).AddDays(-5), Status = "Success" },
        };
        var policy = new RetentionPolicy
        {
            DailyRetentionDays = 7,
            WeeklyRetentionWeeks = 2, // Only 2 weeks weekly
            MonthlyRetentionMonths = 6,
            YearlyRetentionYears = 1
        };

        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync("testdb"))
            .ReturnsAsync(backups);

        // Act
        var result = await _service.GetBackupsToRetainAsync("testdb", policy);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetBackupsToRetainAsync_ShouldRetainYearlyBackups()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var backups = new List<BackupHistory>
        {
            new() { Id = 1, BackupId = "b1", StartedAt = now.AddYears(-1).AddDays(10), Status = "Success" },
            new() { Id = 2, BackupId = "b2", StartedAt = now.AddYears(-1).AddDays(15), Status = "Success" },
        };
        var policy = new RetentionPolicy
        {
            DailyRetentionDays = 7,
            WeeklyRetentionWeeks = 4,
            MonthlyRetentionMonths = 3, // Only 3 months
            YearlyRetentionYears = 5
        };

        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync("testdb"))
            .ReturnsAsync(backups);

        // Act
        var result = await _service.GetBackupsToRetainAsync("testdb", policy);

        // Assert
        result.Should().NotBeEmpty();
    }
}
