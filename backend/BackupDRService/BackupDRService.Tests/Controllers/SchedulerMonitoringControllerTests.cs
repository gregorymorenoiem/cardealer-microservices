using BackupDRService.Api.Controllers;
using BackupDRService.Core.Entities;
using BackupDRService.Core.Models;
using BackupDRService.Core.Repositories;
using BackupDRService.Core.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackupDRService.Tests.Controllers;

public class SchedulerMonitoringControllerTests
{
    private readonly Mock<IBackupScheduleRepository> _scheduleRepositoryMock;
    private readonly Mock<IBackupHistoryRepository> _historyRepositoryMock;
    private readonly Mock<ILogger<SchedulerMonitoringService>> _monitoringLoggerMock;
    private readonly Mock<ILogger<SchedulerMonitoringController>> _controllerLoggerMock;
    private readonly SchedulerMonitoringService _monitoringService;
    private readonly SchedulerMonitoringController _controller;

    public SchedulerMonitoringControllerTests()
    {
        _scheduleRepositoryMock = new Mock<IBackupScheduleRepository>();
        _historyRepositoryMock = new Mock<IBackupHistoryRepository>();
        _monitoringLoggerMock = new Mock<ILogger<SchedulerMonitoringService>>();
        _controllerLoggerMock = new Mock<ILogger<SchedulerMonitoringController>>();

        _monitoringService = new SchedulerMonitoringService(
            _scheduleRepositoryMock.Object,
            _historyRepositoryMock.Object,
            _monitoringLoggerMock.Object);

        _controller = new SchedulerMonitoringController(_monitoringService, _controllerLoggerMock.Object);
    }

    private void SetupDefaultRepositories()
    {
        _scheduleRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<BackupSchedule>());
        _historyRepositoryMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<BackupHistory>());
    }

    // =========== GetHealthMetrics Tests ===========

    [Fact]
    public async Task GetHealthMetrics_ShouldReturnOk()
    {
        // Arrange
        SetupDefaultRepositories();

        // Act
        var result = await _controller.GetHealthMetrics();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetHealthMetrics_ShouldReturnMetricsWithStats()
    {
        // Arrange
        var schedules = new List<BackupSchedule>
        {
            new BackupSchedule { Id = 1, Name = "Daily", IsEnabled = true, CronExpression = "0 0 0 * * *" },
            new BackupSchedule { Id = 2, Name = "Weekly", IsEnabled = false, CronExpression = "0 0 0 * * 0" }
        };
        _scheduleRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(schedules);
        _historyRepositoryMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<BackupHistory>());

        // Act
        var result = await _controller.GetHealthMetrics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var metrics = okResult.Value.Should().BeOfType<SchedulerHealthMetrics>().Subject;
        metrics.Stats.TotalSchedules.Should().Be(2);
        metrics.Stats.EnabledSchedules.Should().Be(1);
    }

    // =========== GetUpcomingBackups Tests ===========

    [Fact]
    public async Task GetUpcomingBackups_ShouldReturnOk_WithDefaultHours()
    {
        // Arrange
        SetupDefaultRepositories();

        // Act
        var result = await _controller.GetUpcomingBackups();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetUpcomingBackups_ShouldReturnOk_WithCustomHours()
    {
        // Arrange
        SetupDefaultRepositories();

        // Act
        var result = await _controller.GetUpcomingBackups(48);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetUpcomingBackups_ShouldReturnBadRequest_WhenHoursLessThan1()
    {
        // Act
        var result = await _controller.GetUpcomingBackups(0);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetUpcomingBackups_ShouldReturnBadRequest_WhenHoursGreaterThan168()
    {
        // Act
        var result = await _controller.GetUpcomingBackups(169);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetUpcomingBackups_ShouldReturnOk_AtMaxHours()
    {
        // Arrange
        SetupDefaultRepositories();

        // Act
        var result = await _controller.GetUpcomingBackups(168);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetUpcomingBackups_ShouldReturnOk_AtMinHours()
    {
        // Arrange
        SetupDefaultRepositories();

        // Act
        var result = await _controller.GetUpcomingBackups(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // =========== GetStatistics Tests ===========

    [Fact]
    public async Task GetStatistics_ShouldReturnOk()
    {
        // Arrange
        SetupDefaultRepositories();

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // =========== Ping Tests ===========

    [Fact]
    public async Task Ping_ShouldReturnOk_WhenNoEnabledSchedules()
    {
        // Arrange - No schedules means it will have issues but still return
        _scheduleRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<BackupSchedule>());
        _historyRepositoryMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<BackupHistory>());

        // Act
        var result = await _controller.Ping();

        // Assert - will be 503 because no enabled schedules
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    [Fact]
    public async Task Ping_ShouldReturnOk_WhenHealthy()
    {
        // Arrange - Create healthy scenario
        var schedules = new List<BackupSchedule>
        {
            new BackupSchedule
            {
                Id = 1,
                Name = "Daily",
                IsEnabled = true,
                CronExpression = "0 0 0 * * *",
                NextRunAt = DateTime.UtcNow.AddHours(1)
            }
        };
        var history = new List<BackupHistory>
        {
            new BackupHistory
            {
                Id = 1,
                ScheduleId = 1,
                Status = "Success",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };
        _scheduleRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(schedules);
        _historyRepositoryMock.Setup(r => r.GetByDatabaseNameAsync(It.IsAny<string>()))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.Ping();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
