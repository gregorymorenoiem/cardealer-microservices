using BackupDRService.Core.Entities;
using BackupDRService.Core.Repositories;
using BackupDRService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackupDRService.Tests.Services;

public class SchedulerMonitoringServiceTests
{
    private readonly Mock<IBackupScheduleRepository> _scheduleRepoMock;
    private readonly Mock<IBackupHistoryRepository> _historyRepoMock;
    private readonly Mock<ILogger<SchedulerMonitoringService>> _loggerMock;
    private readonly SchedulerMonitoringService _service;

    public SchedulerMonitoringServiceTests()
    {
        _scheduleRepoMock = new Mock<IBackupScheduleRepository>();
        _historyRepoMock = new Mock<IBackupHistoryRepository>();
        _loggerMock = new Mock<ILogger<SchedulerMonitoringService>>();

        _service = new SchedulerMonitoringService(
            _scheduleRepoMock.Object,
            _historyRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetHealthMetricsAsync_ShouldReturnHealthyStatus_WhenNoIssues()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var schedules = new List<BackupSchedule>
        {
            new()
            {
                Id = 1,
                Name = "Daily Backup",
                DatabaseName = "testdb",
                CronExpression = "0 0 0 * * *",
                IsEnabled = true,
                NextRunAt = now.AddHours(1)
            }
        };
        var histories = new List<BackupHistory>
        {
            new()
            {
                Id = 1,
                BackupId = "b1",
                ScheduleId = 1,
                Status = "Success",
                CreatedAt = now.AddHours(-1),
                StartedAt = now.AddHours(-1)
            }
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);
        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(histories);

        // Act
        var result = await _service.GetHealthMetricsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Stats.Should().NotBeNull();
        result.Stats.TotalSchedules.Should().Be(1);
        result.Stats.EnabledSchedules.Should().Be(1);
    }

    [Fact]
    public async Task GetHealthMetricsAsync_ShouldIdentifyDisabledSchedules()
    {
        // Arrange
        var schedules = new List<BackupSchedule>
        {
            new() { Id = 1, Name = "Enabled", IsEnabled = true, CronExpression = "0 0 0 * * *" },
            new() { Id = 2, Name = "Disabled", IsEnabled = false, CronExpression = "0 0 0 * * *" }
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);
        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<BackupHistory>());

        // Act
        var result = await _service.GetHealthMetricsAsync();

        // Assert
        result.Stats.TotalSchedules.Should().Be(2);
        result.Stats.EnabledSchedules.Should().Be(1);
        result.Stats.DisabledSchedules.Should().Be(1);
    }

    [Fact]
    public async Task GetHealthMetricsAsync_ShouldCountBackupsExecutedToday()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var today = now.Date;
        var schedules = new List<BackupSchedule>
        {
            new() { Id = 1, Name = "Backup", IsEnabled = true, CronExpression = "0 0 0 * * *" }
        };
        var histories = new List<BackupHistory>
        {
            new() { Id = 1, ScheduleId = 1, Status = "Success", CreatedAt = today.AddHours(2) },
            new() { Id = 2, ScheduleId = 1, Status = "Success", CreatedAt = today.AddHours(4) },
            new() { Id = 3, ScheduleId = 1, Status = "Failed", CreatedAt = today.AddHours(6) },
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);
        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(histories);

        // Act
        var result = await _service.GetHealthMetricsAsync();

        // Assert
        result.Stats.BackupsExecutedToday.Should().Be(3);
        result.Stats.FailedBackupsToday.Should().Be(1);
    }

    [Fact]
    public async Task GetHealthMetricsAsync_ShouldCalculateSuccessRate()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var today = now.Date;
        var schedules = new List<BackupSchedule>
        {
            new() { Id = 1, Name = "Backup", IsEnabled = true, CronExpression = "0 0 0 * * *" }
        };
        var histories = new List<BackupHistory>
        {
            new() { Id = 1, ScheduleId = 1, Status = "Success", CreatedAt = today.AddHours(1) },
            new() { Id = 2, ScheduleId = 1, Status = "Success", CreatedAt = today.AddHours(2) },
            new() { Id = 3, ScheduleId = 1, Status = "Success", CreatedAt = today.AddHours(3) },
            new() { Id = 4, ScheduleId = 1, Status = "Failed", CreatedAt = today.AddHours(4) },
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);
        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(histories);

        // Act
        var result = await _service.GetHealthMetricsAsync();

        // Assert
        result.Stats.SuccessRateToday.Should().BeApproximately(75.0, 0.1); // 3 out of 4 = 75%
    }

    [Fact]
    public async Task GetHealthMetricsAsync_ShouldCountSchedulesDueNext24Hours()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var schedules = new List<BackupSchedule>
        {
            new() { Id = 1, Name = "Soon", IsEnabled = true, CronExpression = "0 0 0 * * *", NextRunAt = now.AddHours(12) },
            new() { Id = 2, Name = "Later", IsEnabled = true, CronExpression = "0 0 0 * * *", NextRunAt = now.AddHours(48) },
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);
        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<BackupHistory>());

        // Act
        var result = await _service.GetHealthMetricsAsync();

        // Assert
        result.Stats.SchedulesDueNext24Hours.Should().Be(1);
    }

    [Fact]
    public async Task GetHealthMetricsAsync_ShouldIdentifyNoEnabledSchedulesIssue()
    {
        // Arrange
        var schedules = new List<BackupSchedule>
        {
            new() { Id = 1, Name = "Disabled", IsEnabled = false, CronExpression = "0 0 0 * * *" }
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);
        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<BackupHistory>());

        // Act
        var result = await _service.GetHealthMetricsAsync();

        // Assert
        result.Issues.Should().Contain(i => i.Contains("No enabled schedules"));
        result.IsHealthy.Should().BeFalse();
        result.Status.Should().Be("Degraded");
    }

    [Fact]
    public async Task GetHealthMetricsAsync_ShouldIdentifyOverdueSchedules()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var schedules = new List<BackupSchedule>
        {
            new()
            {
                Id = 1,
                Name = "Overdue",
                IsEnabled = true,
                CronExpression = "0 0 0 * * *",
                NextRunAt = now.AddMinutes(-10) // 10 minutes overdue
            }
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);
        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<BackupHistory>());

        // Act
        var result = await _service.GetHealthMetricsAsync();

        // Assert
        result.Issues.Should().Contain(i => i.Contains("overdue"));
    }

    [Fact]
    public async Task GetHealthMetricsAsync_ShouldIdentifyInvalidCronExpressions()
    {
        // Arrange
        var schedules = new List<BackupSchedule>
        {
            new()
            {
                Id = 1,
                Name = "Invalid Cron",
                IsEnabled = true,
                CronExpression = "invalid cron"
            }
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);
        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<BackupHistory>());

        // Act
        var result = await _service.GetHealthMetricsAsync();

        // Assert
        result.Issues.Should().Contain(i => i.Contains("invalid cron expression"));
    }

    [Fact]
    public async Task GetHealthMetricsAsync_ShouldHandleException()
    {
        // Arrange
        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.GetHealthMetricsAsync();

        // Assert
        result.IsHealthy.Should().BeFalse();
        result.Status.Should().Be("Error");
        result.Issues.Should().Contain(i => i.Contains("Error"));
    }

    [Fact]
    public async Task GetHealthMetricsAsync_ShouldPopulateActiveSchedules()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var schedules = new List<BackupSchedule>
        {
            new()
            {
                Id = 1,
                Name = "Active Schedule",
                DatabaseName = "testdb",
                IsEnabled = true,
                CronExpression = "0 0 0 * * *",
                NextRunAt = now.AddHours(1),
                LastRunAt = now.AddHours(-1),
                SuccessCount = 10
            }
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);
        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<BackupHistory>());

        // Act
        var result = await _service.GetHealthMetricsAsync();

        // Assert
        result.ActiveSchedules.Should().HaveCount(1);
        result.ActiveSchedules[0].Name.Should().Be("Active Schedule");
        result.ActiveSchedules[0].DatabaseName.Should().Be("testdb");
        result.ActiveSchedules[0].LastRunStatus.Should().Be("Success");
    }

    [Fact]
    public async Task GetUpcomingBackupsAsync_ShouldReturnSchedulesDueWithinHours()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var schedules = new List<BackupSchedule>
        {
            new()
            {
                Id = 1,
                Name = "Soon",
                DatabaseName = "db1",
                IsEnabled = true,
                CronExpression = "0 0 0 * * *",
                NextRunAt = now.AddHours(12)
            },
            new()
            {
                Id = 2,
                Name = "Later",
                DatabaseName = "db2",
                IsEnabled = true,
                CronExpression = "0 0 0 * * *",
                NextRunAt = now.AddHours(36) // Beyond 24 hours
            },
            new()
            {
                Id = 3,
                Name = "Disabled",
                DatabaseName = "db3",
                IsEnabled = false,
                CronExpression = "0 0 0 * * *",
                NextRunAt = now.AddHours(1)
            }
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);

        // Act
        var result = await _service.GetUpcomingBackupsAsync(24);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Soon");
    }

    [Fact]
    public async Task GetUpcomingBackupsAsync_WithCustomHours_ShouldRespectParameter()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var schedules = new List<BackupSchedule>
        {
            new()
            {
                Id = 1,
                Name = "In12Hours",
                IsEnabled = true,
                CronExpression = "0 0 0 * * *",
                NextRunAt = now.AddHours(12)
            },
            new()
            {
                Id = 2,
                Name = "In36Hours",
                IsEnabled = true,
                CronExpression = "0 0 0 * * *",
                NextRunAt = now.AddHours(36)
            }
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);

        // Act
        var result = await _service.GetUpcomingBackupsAsync(48);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUpcomingBackupsAsync_ShouldOrderByNextRunAt()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var schedules = new List<BackupSchedule>
        {
            new() { Id = 1, Name = "Third", IsEnabled = true, CronExpression = "0 0 0 * * *", NextRunAt = now.AddHours(10) },
            new() { Id = 2, Name = "First", IsEnabled = true, CronExpression = "0 0 0 * * *", NextRunAt = now.AddHours(2) },
            new() { Id = 3, Name = "Second", IsEnabled = true, CronExpression = "0 0 0 * * *", NextRunAt = now.AddHours(5) },
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);

        // Act
        var result = await _service.GetUpcomingBackupsAsync(24);

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("First");
        result[1].Name.Should().Be("Second");
        result[2].Name.Should().Be("Third");
    }

    [Fact]
    public async Task GetUpcomingBackupsAsync_ShouldCalculateTimeUntilNextRun()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var schedules = new List<BackupSchedule>
        {
            new()
            {
                Id = 1,
                Name = "Test",
                IsEnabled = true,
                CronExpression = "0 0 0 * * *",
                NextRunAt = now.AddHours(5).AddMinutes(30)
            }
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);

        // Act
        var result = await _service.GetUpcomingBackupsAsync(24);

        // Assert
        result.Should().HaveCount(1);
        result[0].TimeUntilNextRun.Should().NotBeNull();
        result[0].TimeUntilNextRun!.Value.TotalHours.Should().BeApproximately(5.5, 0.1);
    }

    [Fact]
    public async Task GetUpcomingBackupsAsync_ShouldExcludeSchedulesWithNoNextRun()
    {
        // Arrange
        var schedules = new List<BackupSchedule>
        {
            new()
            {
                Id = 1,
                Name = "NoNextRun",
                IsEnabled = true,
                CronExpression = "0 0 0 * * *",
                NextRunAt = null
            }
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);

        // Act
        var result = await _service.GetUpcomingBackupsAsync(24);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHealthMetricsAsync_ShouldSetLastBackupTime()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var today = now.Date;
        var schedules = new List<BackupSchedule>
        {
            new() { Id = 1, Name = "Backup", IsEnabled = true, CronExpression = "0 0 0 * * *" }
        };
        var expectedTime = today.AddHours(6);
        var histories = new List<BackupHistory>
        {
            new() { Id = 1, ScheduleId = 1, Status = "Success", CreatedAt = today.AddHours(2) },
            new() { Id = 2, ScheduleId = 1, Status = "Success", CreatedAt = expectedTime },
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);
        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(histories);

        // Act
        var result = await _service.GetHealthMetricsAsync();

        // Assert
        result.Stats.LastBackupTime.Should().Be(expectedTime);
    }

    [Fact]
    public async Task GetHealthMetricsAsync_ShouldSetNextScheduledBackup()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var nextRun = now.AddHours(2);
        var schedules = new List<BackupSchedule>
        {
            new() { Id = 1, Name = "First", IsEnabled = true, CronExpression = "0 0 0 * * *", NextRunAt = nextRun },
            new() { Id = 2, Name = "Second", IsEnabled = true, CronExpression = "0 0 0 * * *", NextRunAt = now.AddHours(5) },
        };

        _scheduleRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(schedules);
        _historyRepoMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<BackupHistory>());

        // Act
        var result = await _service.GetHealthMetricsAsync();

        // Assert
        result.Stats.NextScheduledBackup.Should().Be(nextRun);
    }
}
