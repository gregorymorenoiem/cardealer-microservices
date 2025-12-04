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

    // =========== GetEnabledJobs Tests ===========

    [Fact]
    public async Task GetEnabledJobs_ShouldReturnOkWithEnabledJobs()
    {
        // Arrange
        var jobs = new List<BackupJob>
        {
            new BackupJob { Id = "1", Name = "Job 1", IsEnabled = true },
            new BackupJob { Id = "2", Name = "Job 2", IsEnabled = true }
        };
        _backupServiceMock.Setup(s => s.GetEnabledJobsAsync()).ReturnsAsync(jobs);

        // Act
        var result = await _controller.GetEnabledJobs();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedJobs = okResult.Value.Should().BeAssignableTo<IEnumerable<BackupJob>>().Subject;
        returnedJobs.Should().HaveCount(2);
        returnedJobs.Should().OnlyContain(j => j.IsEnabled);
    }

    [Fact]
    public async Task GetEnabledJobs_ShouldReturnEmptyList_WhenNoEnabledJobs()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.GetEnabledJobsAsync()).ReturnsAsync(new List<BackupJob>());

        // Act
        var result = await _controller.GetEnabledJobs();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedJobs = okResult.Value.Should().BeAssignableTo<IEnumerable<BackupJob>>().Subject;
        returnedJobs.Should().BeEmpty();
    }

    // =========== GetJobByName Tests ===========

    [Fact]
    public async Task GetJobByName_ShouldReturnOk_WhenJobExists()
    {
        // Arrange
        var job = new BackupJob { Id = "test-id", Name = "Daily Backup" };
        _backupServiceMock.Setup(s => s.GetJobByNameAsync("Daily Backup")).ReturnsAsync(job);

        // Act
        var result = await _controller.GetJobByName("Daily Backup");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedJob = okResult.Value.Should().BeOfType<BackupJob>().Subject;
        returnedJob.Name.Should().Be("Daily Backup");
    }

    [Fact]
    public async Task GetJobByName_ShouldReturnNotFound_WhenJobDoesNotExist()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.GetJobByNameAsync("NonExistent")).ReturnsAsync((BackupJob?)null);

        // Act
        var result = await _controller.GetJobByName("NonExistent");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // =========== CancelBackup Tests ===========

    [Fact]
    public async Task CancelBackup_ShouldReturnOk_WhenCancelled()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.CancelBackupAsync("result-id")).ReturnsAsync(true);

        // Act
        var result = await _controller.CancelBackup("result-id");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CancelBackup_ShouldReturnBadRequest_WhenCannotCancel()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.CancelBackupAsync("result-id")).ReturnsAsync(false);

        // Act
        var result = await _controller.CancelBackup("result-id");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // =========== GetRecentResults Tests ===========

    [Fact]
    public async Task GetRecentResults_ShouldReturnOkWithResults()
    {
        // Arrange
        var results = new List<BackupResult>
        {
            BackupResult.Success("job1", "Job 1", "/backup/1.backup", 1024, "checksum1"),
            BackupResult.Success("job2", "Job 2", "/backup/2.backup", 2048, "checksum2")
        };
        _backupServiceMock.Setup(s => s.GetRecentBackupResultsAsync(10)).ReturnsAsync(results);

        // Act
        var result = await _controller.GetRecentResults();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResults = okResult.Value.Should().BeAssignableTo<IEnumerable<BackupResult>>().Subject;
        returnedResults.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentResults_ShouldUseCustomCount()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.GetRecentBackupResultsAsync(5)).ReturnsAsync(new List<BackupResult>());

        // Act
        await _controller.GetRecentResults(5);

        // Assert
        _backupServiceMock.Verify(s => s.GetRecentBackupResultsAsync(5), Times.Once);
    }

    // =========== GetJobResults Tests ===========

    [Fact]
    public async Task GetJobResults_ShouldReturnOkWithResults()
    {
        // Arrange
        var results = new List<BackupResult>
        {
            BackupResult.Success("job-id", "Test Job", "/backup/1.backup", 1024, "checksum1")
        };
        _backupServiceMock.Setup(s => s.GetBackupResultsAsync("job-id")).ReturnsAsync(results);

        // Act
        var result = await _controller.GetJobResults("job-id");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResults = okResult.Value.Should().BeAssignableTo<IEnumerable<BackupResult>>().Subject;
        returnedResults.Should().HaveCount(1);
    }

    // =========== GetResult Tests ===========

    [Fact]
    public async Task GetResult_ShouldReturnOk_WhenResultExists()
    {
        // Arrange
        var backupResult = BackupResult.Success("job-id", "Test Job", "/backup/test.backup", 1024, "checksum");
        _backupServiceMock.Setup(s => s.GetBackupResultAsync("result-id")).ReturnsAsync(backupResult);

        // Act
        var result = await _controller.GetResult("result-id");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<BackupResult>();
    }

    [Fact]
    public async Task GetResult_ShouldReturnNotFound_WhenResultDoesNotExist()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.GetBackupResultAsync("non-existent")).ReturnsAsync((BackupResult?)null);

        // Act
        var result = await _controller.GetResult("non-existent");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // =========== GetResultsByDateRange Tests ===========

    [Fact]
    public async Task GetResultsByDateRange_ShouldReturnOkWithResults()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;
        var results = new List<BackupResult>
        {
            BackupResult.Success("job-id", "Test Job", "/backup/test.backup", 1024, "checksum")
        };
        _backupServiceMock.Setup(s => s.GetBackupResultsByDateRangeAsync(from, to)).ReturnsAsync(results);

        // Act
        var result = await _controller.GetResultsByDateRange(from, to);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResults = okResult.Value.Should().BeAssignableTo<IEnumerable<BackupResult>>().Subject;
        returnedResults.Should().HaveCount(1);
    }

    // =========== VerifyBackup Tests ===========

    [Fact]
    public async Task VerifyBackup_ShouldReturnOk_WithVerificationResult()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.VerifyBackupAsync("result-id")).ReturnsAsync(true);

        // Act
        var result = await _controller.VerifyBackup("result-id");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task VerifyBackup_ShouldReturnOk_WhenVerificationFails()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.VerifyBackupAsync("result-id")).ReturnsAsync(false);

        // Act
        var result = await _controller.VerifyBackup("result-id");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // =========== CleanupExpiredBackups Tests ===========

    [Fact]
    public async Task CleanupExpiredBackups_ShouldReturnOk_WithDeletedCount()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.CleanupExpiredBackupsAsync()).ReturnsAsync(5);

        // Act
        var result = await _controller.CleanupExpiredBackups();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CleanupExpiredBackups_ShouldReturnOk_WhenNoExpiredBackups()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.CleanupExpiredBackupsAsync()).ReturnsAsync(0);

        // Act
        var result = await _controller.CleanupExpiredBackups();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // =========== EnableJob & DisableJob Tests ===========

    [Fact]
    public async Task EnableJob_ShouldReturnNotFound_WhenJobDoesNotExist()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.EnableJobAsync("non-existent")).ReturnsAsync(false);

        // Act
        var result = await _controller.EnableJob("non-existent");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DisableJob_ShouldReturnNotFound_WhenJobDoesNotExist()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.DisableJobAsync("non-existent")).ReturnsAsync(false);

        // Act
        var result = await _controller.DisableJob("non-existent");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // =========== UpdateJob with all fields Tests ===========

    [Fact]
    public async Task UpdateJob_ShouldUpdateAllFields_WhenProvided()
    {
        // Arrange
        var existingJob = new BackupJob
        {
            Id = "test-id",
            Name = "Original",
            Description = "Original Desc",
            Type = BackupType.Full,
            Schedule = "0 0 * * *",
            StoragePath = "/old/path",
            RetentionDays = 30,
            IsEnabled = true,
            CompressBackup = true,
            EncryptBackup = false
        };
        _backupServiceMock.Setup(s => s.GetJobAsync("test-id")).ReturnsAsync(existingJob);
        _backupServiceMock.Setup(s => s.UpdateJobAsync(It.IsAny<BackupJob>()))
            .ReturnsAsync((BackupJob job) => job);

        var request = new UpdateBackupJobRequest
        {
            Name = "Updated",
            Description = "Updated Desc",
            Type = BackupType.Incremental,
            Schedule = "0 12 * * *",
            StoragePath = "/new/path",
            RetentionDays = 60,
            IsEnabled = false,
            CompressBackup = false,
            EncryptBackup = true
        };

        // Act
        var result = await _controller.UpdateJob("test-id", request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedJob = okResult.Value.Should().BeOfType<BackupJob>().Subject;
        returnedJob.Name.Should().Be("Updated");
        returnedJob.Description.Should().Be("Updated Desc");
        returnedJob.Type.Should().Be(BackupType.Incremental);
        returnedJob.Schedule.Should().Be("0 12 * * *");
        returnedJob.StoragePath.Should().Be("/new/path");
        returnedJob.RetentionDays.Should().Be(60);
        returnedJob.IsEnabled.Should().BeFalse();
        returnedJob.CompressBackup.Should().BeFalse();
        returnedJob.EncryptBackup.Should().BeTrue();
    }

    // =========== CreateJob with all fields Tests ===========

    [Fact]
    public async Task CreateJob_ShouldSetAllFields()
    {
        // Arrange
        var request = new CreateBackupJobRequest
        {
            Name = "New Job",
            Description = "Job Description",
            Type = BackupType.Differential,
            Target = BackupTarget.SqlServer,
            ConnectionString = "Host=localhost",
            DatabaseName = "testdb",
            Schedule = "0 0 * * *",
            StorageType = StorageType.S3,
            StoragePath = "s3://bucket/backup",
            RetentionDays = 90,
            IsEnabled = true,
            CompressBackup = true,
            EncryptBackup = true
        };

        _backupServiceMock.Setup(s => s.CreateJobAsync(It.IsAny<BackupJob>()))
            .ReturnsAsync((BackupJob job) => { job.Id = "new-id"; return job; });

        // Act
        var result = await _controller.CreateJob(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedJob = createdResult.Value.Should().BeOfType<BackupJob>().Subject;
        returnedJob.Name.Should().Be("New Job");
        returnedJob.Description.Should().Be("Job Description");
        returnedJob.Type.Should().Be(BackupType.Differential);
        returnedJob.Target.Should().Be(BackupTarget.SqlServer);
        returnedJob.StorageType.Should().Be(StorageType.S3);
        returnedJob.RetentionDays.Should().Be(90);
        returnedJob.EncryptBackup.Should().BeTrue();
    }
}
