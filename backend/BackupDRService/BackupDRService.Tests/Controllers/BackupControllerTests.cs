using BackupDRService.Api.Controllers;
using BackupDRService.Core.Interfaces;
using BackupDRService.Core.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackupDRService.Tests.Controllers;

public class BackupControllerTests
{
    private readonly Mock<IBackupService> _backupServiceMock;
    private readonly Mock<ILogger<BackupController>> _loggerMock;
    private readonly BackupController _controller;

    public BackupControllerTests()
    {
        _backupServiceMock = new Mock<IBackupService>();
        _loggerMock = new Mock<ILogger<BackupController>>();
        _controller = new BackupController(_backupServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllJobs_ShouldReturnOkWithJobs()
    {
        // Arrange
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = "1", Name = "Job 1" },
            new BackupJob { Id = "2", Name = "Job 2" }
        };
        _backupServiceMock.Setup(s => s.GetAllJobsAsync()).ReturnsAsync(jobs);

        // Act
        var result = await _controller.GetAllJobs();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedJobs = okResult.Value.Should().BeAssignableTo<IEnumerable<BackupJob>>().Subject;
        returnedJobs.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetJob_ShouldReturnOk_WhenJobExists()
    {
        // Arrange
        var job = new BackupJob { Id = "test-id", Name = "Test Job" };
        _backupServiceMock.Setup(s => s.GetJobAsync("test-id")).ReturnsAsync(job);

        // Act
        var result = await _controller.GetJob("test-id");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedJob = okResult.Value.Should().BeOfType<BackupJob>().Subject;
        returnedJob.Name.Should().Be("Test Job");
    }

    [Fact]
    public async Task GetJob_ShouldReturnNotFound_WhenJobDoesNotExist()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.GetJobAsync("non-existent")).ReturnsAsync((BackupJob?)null);

        // Act
        var result = await _controller.GetJob("non-existent");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateJob_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var request = new CreateBackupJobRequest
        {
            Name = "New Job",
            DatabaseName = "testdb",
            ConnectionString = "Host=localhost"
        };

        _backupServiceMock.Setup(s => s.CreateJobAsync(It.IsAny<BackupJob>()))
            .ReturnsAsync((BackupJob job) => { job.Id = "new-id"; return job; });

        // Act
        var result = await _controller.CreateJob(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(BackupController.GetJob));
        var returnedJob = createdResult.Value.Should().BeOfType<BackupJob>().Subject;
        returnedJob.Name.Should().Be("New Job");
    }

    [Fact]
    public async Task UpdateJob_ShouldReturnOk_WhenJobExists()
    {
        // Arrange
        var existingJob = new BackupJob { Id = "test-id", Name = "Original" };
        _backupServiceMock.Setup(s => s.GetJobAsync("test-id")).ReturnsAsync(existingJob);
        _backupServiceMock.Setup(s => s.UpdateJobAsync(It.IsAny<BackupJob>()))
            .ReturnsAsync((BackupJob job) => job);

        var request = new UpdateBackupJobRequest { Name = "Updated" };

        // Act
        var result = await _controller.UpdateJob("test-id", request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedJob = okResult.Value.Should().BeOfType<BackupJob>().Subject;
        returnedJob.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateJob_ShouldReturnNotFound_WhenJobDoesNotExist()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.GetJobAsync("non-existent")).ReturnsAsync((BackupJob?)null);

        // Act
        var result = await _controller.UpdateJob("non-existent", new UpdateBackupJobRequest());

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteJob_ShouldReturnNoContent_WhenDeleted()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.DeleteJobAsync("test-id")).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteJob("test-id");

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteJob_ShouldReturnNotFound_WhenJobDoesNotExist()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.DeleteJobAsync("non-existent")).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteJob("non-existent");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task EnableJob_ShouldReturnOk_WhenEnabled()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.EnableJobAsync("test-id")).ReturnsAsync(true);

        // Act
        var result = await _controller.EnableJob("test-id");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DisableJob_ShouldReturnOk_WhenDisabled()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.DisableJobAsync("test-id")).ReturnsAsync(true);

        // Act
        var result = await _controller.DisableJob("test-id");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ExecuteBackup_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var backupResult = BackupResult.Success("job-id", "Test Job", "/backup/test.backup", 1024, "checksum");
        _backupServiceMock.Setup(s => s.ExecuteBackupAsync("job-id")).ReturnsAsync(backupResult);

        // Act
        var result = await _controller.ExecuteBackup("job-id");

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ExecuteBackup_ShouldReturnBadRequest_WhenFailed()
    {
        // Arrange
        var backupResult = BackupResult.Failure("job-id", "Test Job", "Backup failed");
        _backupServiceMock.Setup(s => s.ExecuteBackupAsync("job-id")).ReturnsAsync(backupResult);

        // Act
        var result = await _controller.ExecuteBackup("job-id");

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetStatistics_ShouldReturnOk()
    {
        // Arrange
        var stats = new BackupStatistics
        {
            TotalJobs = 5,
            SuccessfulBackups = 100
        };
        _backupServiceMock.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStats = okResult.Value.Should().BeOfType<BackupStatistics>().Subject;
        returnedStats.TotalJobs.Should().Be(5);
    }
}
