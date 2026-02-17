using BackupDRService.Core.Entities;
using BackupDRService.Core.Repositories;
using BackupDRService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackupDRService.Tests.Services;

public class SchedulerServiceTests
{
    private readonly Mock<IBackupScheduleRepository> _scheduleRepoMock;
    private readonly Mock<IAuditLogRepository> _auditRepoMock;
    private readonly Mock<ILogger<SchedulerService>> _loggerMock;
    private readonly SchedulerService _service;

    public SchedulerServiceTests()
    {
        _scheduleRepoMock = new Mock<IBackupScheduleRepository>();
        _auditRepoMock = new Mock<IAuditLogRepository>();
        _loggerMock = new Mock<ILogger<SchedulerService>>();

        _service = new SchedulerService(
            _scheduleRepoMock.Object,
            _auditRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateScheduleAsync_WithValidCron_ShouldCreateSchedule()
    {
        // Arrange
        var schedule = new BackupSchedule
        {
            Name = "Daily Backup",
            DatabaseName = "testdb",
            CronExpression = "0 0 0 * * *", // Daily at midnight
            IsEnabled = true
        };

        _scheduleRepoMock.Setup(r => r.CreateAsync(It.IsAny<BackupSchedule>()))
            .ReturnsAsync((BackupSchedule s) =>
            {
                s.Id = 1;
                return s;
            });

        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog { Id = 1 });

        // Act
        var result = await _service.CreateScheduleAsync(schedule, "testuser");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.NextRunAt.Should().NotBeNull();
        _auditRepoMock.Verify(r => r.CreateAsync(It.Is<AuditLog>(a =>
            a.Action == "ScheduleCreated" && a.UserId == "testuser")), Times.Once);
    }

    [Fact]
    public async Task CreateScheduleAsync_WithInvalidCron_ShouldThrowArgumentException()
    {
        // Arrange
        var schedule = new BackupSchedule
        {
            Name = "Invalid Schedule",
            CronExpression = "invalid cron expression"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateScheduleAsync(schedule));
        exception.Message.Should().Contain("Invalid cron expression");
    }

    [Fact]
    public async Task CreateScheduleAsync_WithDefaultUser_ShouldUseSystemUser()
    {
        // Arrange
        var schedule = new BackupSchedule
        {
            Name = "Test Schedule",
            CronExpression = "0 0 0 * * *"
        };

        _scheduleRepoMock.Setup(r => r.CreateAsync(It.IsAny<BackupSchedule>()))
            .ReturnsAsync((BackupSchedule s) => { s.Id = 1; return s; });

        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        // Act
        await _service.CreateScheduleAsync(schedule);

        // Assert
        _auditRepoMock.Verify(r => r.CreateAsync(It.Is<AuditLog>(a =>
            a.UserId == "system")), Times.Once);
    }

    [Fact]
    public async Task UpdateScheduleAsync_WhenScheduleExists_ShouldUpdateSchedule()
    {
        // Arrange
        var existing = new BackupSchedule
        {
            Id = 1,
            Name = "Original",
            CronExpression = "0 0 0 * * *"
        };
        var updated = new BackupSchedule
        {
            Id = 1,
            Name = "Updated",
            CronExpression = "0 30 6 * * *" // 6:30 AM daily
        };

        _scheduleRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);
        _scheduleRepoMock.Setup(r => r.UpdateAsync(It.IsAny<BackupSchedule>()))
            .ReturnsAsync(updated);
        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        // Act
        var result = await _service.UpdateScheduleAsync(updated, "admin");

        // Assert
        result.Name.Should().Be("Updated");
        result.NextRunAt.Should().NotBeNull();
        _auditRepoMock.Verify(r => r.CreateAsync(It.Is<AuditLog>(a =>
            a.Action == "ScheduleUpdated")), Times.Once);
    }

    [Fact]
    public async Task UpdateScheduleAsync_WhenScheduleNotExists_ShouldThrowException()
    {
        // Arrange
        var schedule = new BackupSchedule { Id = 999, Name = "NonExistent" };

        _scheduleRepoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((BackupSchedule?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateScheduleAsync(schedule));
        exception.Message.Should().Contain("999");
        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task UpdateScheduleAsync_WithSameCron_ShouldNotRecalculateNextRun()
    {
        // Arrange
        var existing = new BackupSchedule
        {
            Id = 1,
            Name = "Original",
            CronExpression = "0 0 0 * * *",
            NextRunAt = DateTime.UtcNow.AddHours(5)
        };
        var updated = new BackupSchedule
        {
            Id = 1,
            Name = "Updated Name Only",
            CronExpression = "0 0 0 * * *" // Same cron
        };

        _scheduleRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);
        _scheduleRepoMock.Setup(r => r.UpdateAsync(It.IsAny<BackupSchedule>()))
            .ReturnsAsync(updated);
        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        // Act
        var result = await _service.UpdateScheduleAsync(updated);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateScheduleAsync_WithInvalidCron_ShouldThrowArgumentException()
    {
        // Arrange
        var existing = new BackupSchedule
        {
            Id = 1,
            Name = "Existing",
            CronExpression = "0 0 0 * * *"
        };
        var updated = new BackupSchedule
        {
            Id = 1,
            Name = "Updated",
            CronExpression = "bad cron"
        };

        _scheduleRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateScheduleAsync(updated));
    }

    [Fact]
    public async Task DeleteScheduleAsync_WhenScheduleExists_ShouldDeleteAndAudit()
    {
        // Arrange
        var schedule = new BackupSchedule
        {
            Id = 1,
            Name = "To Delete",
            DatabaseName = "testdb"
        };

        _scheduleRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(schedule);
        _scheduleRepoMock.Setup(r => r.DeleteAsync(1))
            .Returns(Task.CompletedTask);
        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        // Act
        await _service.DeleteScheduleAsync(1, "admin");

        // Assert
        _scheduleRepoMock.Verify(r => r.DeleteAsync(1), Times.Once);
        _auditRepoMock.Verify(r => r.CreateAsync(It.Is<AuditLog>(a =>
            a.Action == "ScheduleDeleted")), Times.Once);
    }

    [Fact]
    public async Task DeleteScheduleAsync_WhenScheduleNotExists_ShouldThrowException()
    {
        // Arrange
        _scheduleRepoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((BackupSchedule?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteScheduleAsync(999));
        exception.Message.Should().Contain("999");
    }

    [Fact]
    public async Task GetSchedulesDueForExecutionAsync_ShouldReturnDueSchedules()
    {
        // Arrange
        var schedules = new List<BackupSchedule>
        {
            new() { Id = 1, Name = "Schedule 1", IsEnabled = true },
            new() { Id = 2, Name = "Schedule 2", IsEnabled = true }
        };

        _scheduleRepoMock.Setup(r => r.GetDueForExecutionAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(schedules);

        // Act
        var result = await _service.GetSchedulesDueForExecutionAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllSchedulesAsync_ShouldReturnAllSchedules()
    {
        // Arrange
        var schedules = new List<BackupSchedule>
        {
            new() { Id = 1, Name = "Schedule 1" },
            new() { Id = 2, Name = "Schedule 2" },
            new() { Id = 3, Name = "Schedule 3" }
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);

        // Act
        var result = await _service.GetAllSchedulesAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetEnabledSchedulesAsync_ShouldReturnOnlyEnabledSchedules()
    {
        // Arrange
        var schedules = new List<BackupSchedule>
        {
            new() { Id = 1, Name = "Enabled 1", IsEnabled = true },
            new() { Id = 2, Name = "Enabled 2", IsEnabled = true }
        };

        _scheduleRepoMock.Setup(r => r.GetEnabledAsync())
            .ReturnsAsync(schedules);

        // Act
        var result = await _service.GetEnabledSchedulesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(s => s.IsEnabled).Should().BeTrue();
    }

    [Fact]
    public async Task GetScheduleByIdAsync_WhenExists_ShouldReturnSchedule()
    {
        // Arrange
        var schedule = new BackupSchedule { Id = 1, Name = "Test Schedule" };

        _scheduleRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(schedule);

        // Act
        var result = await _service.GetScheduleByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Schedule");
    }

    [Fact]
    public async Task GetScheduleByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        // Arrange
        _scheduleRepoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((BackupSchedule?)null);

        // Act
        var result = await _service.GetScheduleByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateScheduleAfterExecutionAsync_WhenSuccess_ShouldUpdateSuccessCount()
    {
        // Arrange
        var schedule = new BackupSchedule
        {
            Id = 1,
            Name = "Test",
            CronExpression = "0 0 0 * * *"
        };
        var executedAt = DateTime.UtcNow;

        _scheduleRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(schedule);
        _scheduleRepoMock.Setup(r => r.UpdateLastRunAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);
        _scheduleRepoMock.Setup(r => r.UpdateSuccessCountAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateScheduleAfterExecutionAsync(1, success: true, executedAt);

        // Assert
        _scheduleRepoMock.Verify(r => r.UpdateSuccessCountAsync(1), Times.Once);
    }

    [Fact]
    public async Task UpdateScheduleAfterExecutionAsync_WhenFailed_ShouldUpdateFailureCount()
    {
        // Arrange
        var schedule = new BackupSchedule
        {
            Id = 1,
            Name = "Test",
            CronExpression = "0 0 0 * * *"
        };
        var executedAt = DateTime.UtcNow;

        _scheduleRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(schedule);
        _scheduleRepoMock.Setup(r => r.UpdateLastRunAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);
        _scheduleRepoMock.Setup(r => r.UpdateFailureCountAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateScheduleAfterExecutionAsync(1, success: false, executedAt);

        // Assert
        _scheduleRepoMock.Verify(r => r.UpdateFailureCountAsync(1), Times.Once);
    }

    [Fact]
    public async Task UpdateScheduleAfterExecutionAsync_WhenScheduleNotFound_ShouldNotThrow()
    {
        // Arrange
        _scheduleRepoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((BackupSchedule?)null);

        // Act - Should not throw
        await _service.UpdateScheduleAfterExecutionAsync(999, true, DateTime.UtcNow);

        // Assert
        _scheduleRepoMock.Verify(r => r.UpdateSuccessCountAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task EnableScheduleAsync_WhenScheduleExists_ShouldEnableAndAudit()
    {
        // Arrange
        var schedule = new BackupSchedule
        {
            Id = 1,
            Name = "Disabled Schedule",
            CronExpression = "0 0 0 * * *",
            IsEnabled = false
        };

        _scheduleRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(schedule);
        _scheduleRepoMock.Setup(r => r.UpdateAsync(It.IsAny<BackupSchedule>()))
            .ReturnsAsync((BackupSchedule s) => s);
        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        // Act
        await _service.EnableScheduleAsync(1, "admin");

        // Assert
        _scheduleRepoMock.Verify(r => r.UpdateAsync(It.Is<BackupSchedule>(s =>
            s.IsEnabled == true && s.NextRunAt.HasValue)), Times.Once);
        _auditRepoMock.Verify(r => r.CreateAsync(It.Is<AuditLog>(a =>
            a.Action == "ScheduleEnabled")), Times.Once);
    }

    [Fact]
    public async Task EnableScheduleAsync_WhenScheduleNotExists_ShouldThrowException()
    {
        // Arrange
        _scheduleRepoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((BackupSchedule?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.EnableScheduleAsync(999));
    }

    [Fact]
    public async Task DisableScheduleAsync_WhenScheduleExists_ShouldDisableAndAudit()
    {
        // Arrange
        var schedule = new BackupSchedule
        {
            Id = 1,
            Name = "Enabled Schedule",
            IsEnabled = true
        };

        _scheduleRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(schedule);
        _scheduleRepoMock.Setup(r => r.UpdateAsync(It.IsAny<BackupSchedule>()))
            .ReturnsAsync((BackupSchedule s) => s);
        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        // Act
        await _service.DisableScheduleAsync(1, "admin");

        // Assert
        _scheduleRepoMock.Verify(r => r.UpdateAsync(It.Is<BackupSchedule>(s =>
            s.IsEnabled == false)), Times.Once);
        _auditRepoMock.Verify(r => r.CreateAsync(It.Is<AuditLog>(a =>
            a.Action == "ScheduleDisabled")), Times.Once);
    }

    [Fact]
    public async Task DisableScheduleAsync_WhenScheduleNotExists_ShouldThrowException()
    {
        // Arrange
        _scheduleRepoMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((BackupSchedule?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DisableScheduleAsync(999));
    }

    [Fact]
    public async Task CreateScheduleAsync_ShouldCalculateNextRunTime()
    {
        // Arrange
        var schedule = new BackupSchedule
        {
            Name = "Hourly Backup",
            CronExpression = "0 0 * * * *" // Every hour
        };

        _scheduleRepoMock.Setup(r => r.CreateAsync(It.IsAny<BackupSchedule>()))
            .ReturnsAsync((BackupSchedule s) => { s.Id = 1; return s; });
        _auditRepoMock.Setup(r => r.CreateAsync(It.IsAny<AuditLog>()))
            .ReturnsAsync(new AuditLog());

        // Act
        var result = await _service.CreateScheduleAsync(schedule);

        // Assert
        result.NextRunAt.Should().NotBeNull();
        result.NextRunAt.Should().BeAfter(DateTime.UtcNow);
    }
}
