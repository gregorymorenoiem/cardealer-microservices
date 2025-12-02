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
}
