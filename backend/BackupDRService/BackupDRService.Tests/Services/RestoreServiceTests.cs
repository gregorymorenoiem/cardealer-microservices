using BackupDRService.Core.Interfaces;
using BackupDRService.Core.Models;
using BackupDRService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BackupDRService.Tests.Services;

public class RestoreServiceTests
{
    private readonly Mock<ILogger<RestoreService>> _loggerMock;
    private readonly Mock<IDatabaseBackupProvider> _databaseProviderMock;
    private readonly Mock<IStorageProvider> _storageProviderMock;
    private readonly Mock<IBackupService> _backupServiceMock;
    private readonly BackupOptions _options;
    private readonly RestoreService _service;

    public RestoreServiceTests()
    {
        _loggerMock = new Mock<ILogger<RestoreService>>();
        _databaseProviderMock = new Mock<IDatabaseBackupProvider>();
        _storageProviderMock = new Mock<IStorageProvider>();
        _backupServiceMock = new Mock<IBackupService>();
        
        _options = new BackupOptions
        {
            LocalStoragePath = "C:\\Backups",
            DefaultRetentionDays = 30,
            CleanupSchedule = "0 2 * * *"
        };

        _service = new RestoreService(
            _loggerMock.Object,
            Options.Create(_options),
            _databaseProviderMock.Object,
            _storageProviderMock.Object,
            _backupServiceMock.Object
        );
    }

    #region CreateRestorePointAsync Tests

    [Fact]
    public async Task CreateRestorePointAsync_ShouldCreateRestorePoint()
    {
        // Arrange
        var backupResult = new BackupResult
        {
            Id = Guid.NewGuid().ToString(),
            JobId = "job1",
            JobName = "TestJob",
            Status = BackupExecutionStatus.Completed,
            FilePath = "/path/to/backup.bak",
            FileSizeBytes = 1024
        };

        // Act
        var result = await _service.CreateRestorePointAsync(backupResult, "TestPoint", "Test restore point");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Name.Should().Be("TestPoint");
        result.Description.Should().Be("Test restore point");
        result.JobId.Should().Be("job1");
        result.Status.Should().Be(RestorePointStatus.Available);
    }

    [Fact]
    public async Task GetRestorePointAsync_ShouldReturnPoint_WhenExists()
    {
        // Arrange
        var backupResult = new BackupResult
        {
            Id = Guid.NewGuid().ToString(),
            JobId = "job1",
            JobName = "TestJob",
            FilePath = "/path/to/backup.bak",
            FileSizeBytes = 1024
        };
        var created = await _service.CreateRestorePointAsync(backupResult, "TestPoint");

        // Act
        var result = await _service.GetRestorePointAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetRestorePointAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _service.GetRestorePointAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllRestorePointsAsync Tests

    [Fact]
    public async Task GetAllRestorePointsAsync_ShouldReturnAllPoints()
    {
        // Arrange
        var backupResult1 = CreateBackupResult("job1", "backup1.bak");
        var backupResult2 = CreateBackupResult("job2", "backup2.bak");
        
        await _service.CreateRestorePointAsync(backupResult1, "Point1");
        await _service.CreateRestorePointAsync(backupResult2, "Point2");

        // Act
        var results = await _service.GetAllRestorePointsAsync();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRestorePointsByJobAsync_ShouldReturnPointsForJob()
    {
        // Arrange
        var backupResult1 = CreateBackupResult("job1", "backup1.bak");
        var backupResult2 = CreateBackupResult("job1", "backup2.bak");
        var backupResult3 = CreateBackupResult("job2", "backup3.bak");
        
        await _service.CreateRestorePointAsync(backupResult1, "Point1");
        await _service.CreateRestorePointAsync(backupResult2, "Point2");
        await _service.CreateRestorePointAsync(backupResult3, "Point3");

        // Act
        var results = await _service.GetRestorePointsByJobAsync("job1");

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(p => p.JobId.Should().Be("job1"));
    }

    #endregion

    #region GetAvailableRestorePointsAsync Tests

    [Fact]
    public async Task GetAvailableRestorePointsAsync_ShouldReturnOnlyAvailable()
    {
        // Arrange
        var backupResult1 = CreateBackupResult("job1", "backup1.bak");
        var backupResult2 = CreateBackupResult("job2", "backup2.bak");
        
        await _service.CreateRestorePointAsync(backupResult1, "Point1");
        await _service.CreateRestorePointAsync(backupResult2, "Point2");

        // Act
        var results = await _service.GetAvailableRestorePointsAsync();

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(p => p.Status.Should().Be(RestorePointStatus.Available));
    }

    #endregion

    #region DeleteRestorePointAsync Tests

    [Fact]
    public async Task DeleteRestorePointAsync_ShouldDeletePoint_WhenExists()
    {
        // Arrange
        var backupResult = CreateBackupResult("job1", "backup1.bak");
        var created = await _service.CreateRestorePointAsync(backupResult, "Point1");

        _storageProviderMock.Setup(s => s.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteRestorePointAsync(created.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _service.GetRestorePointAsync(created.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteRestorePointAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Act
        var result = await _service.DeleteRestorePointAsync("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RestoreFromPointAsync Tests

    [Fact]
    public async Task RestoreFromPointAsync_ShouldReturnFailure_WhenPointNotFound()
    {
        // Act
        var result = await _service.RestoreFromPointAsync("nonexistent");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(RestoreExecutionStatus.Failed);
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task RestoreFromPointAsync_ShouldRestoreSuccessfully()
    {
        // Arrange
        var backupResult = CreateBackupResult("job1", "backup1.bak");
        var restorePoint = await _service.CreateRestorePointAsync(backupResult, "Point1");

        var job = new BackupJob
        {
            Id = "job1",
            Name = "TestJob",
            ConnectionString = "Host=localhost;Database=testdb",
            DatabaseName = "testdb"
        };

        _backupServiceMock.Setup(b => b.GetJobAsync("job1"))
            .ReturnsAsync(job);

        _databaseProviderMock.Setup(d => d.RestoreAsync(It.IsAny<DatabaseRestoreRequest>()))
            .ReturnsAsync(new DatabaseRestoreResult
            {
                Success = true,
                BytesRestored = 1024,
                TablesRestored = 5,
                RowsRestored = 100
            });

        // Act
        var result = await _service.RestoreFromPointAsync(restorePoint.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(RestoreExecutionStatus.Completed);
        result.TablesRestored.Should().Be(5);
        result.RecordsRestored.Should().Be(100);
    }

    [Fact]
    public async Task RestoreFromPointAsync_WhenDatabaseRestoreFails_ShouldReturnFailure()
    {
        // Arrange
        var backupResult = CreateBackupResult("job1", "backup1.bak");
        var restorePoint = await _service.CreateRestorePointAsync(backupResult, "Point1");

        var job = new BackupJob
        {
            Id = "job1",
            ConnectionString = "Host=localhost;Database=testdb",
            DatabaseName = "testdb"
        };

        _backupServiceMock.Setup(b => b.GetJobAsync("job1"))
            .ReturnsAsync(job);

        _databaseProviderMock.Setup(d => d.RestoreAsync(It.IsAny<DatabaseRestoreRequest>()))
            .ReturnsAsync(new DatabaseRestoreResult
            {
                Success = false,
                ErrorMessage = "Database restore failed"
            });

        // Act
        var result = await _service.RestoreFromPointAsync(restorePoint.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(RestoreExecutionStatus.Failed);
        result.ErrorMessage.Should().Contain("Database restore failed");
    }

    [Fact]
    public async Task RestoreFromPointAsync_WithCustomOptions_ShouldUseOptions()
    {
        // Arrange
        var backupResult = CreateBackupResult("job1", "backup1.bak");
        var restorePoint = await _service.CreateRestorePointAsync(backupResult, "Point1");

        var job = new BackupJob
        {
            Id = "job1",
            ConnectionString = "Host=localhost;Database=testdb",
            DatabaseName = "testdb"
        };

        var options = new RestoreOptions
        {
            TargetConnectionString = "Host=localhost;Database=target",
            TargetDatabaseName = "targetdb",
            DropExistingDatabase = true,
            InitiatedBy = "admin"
        };

        _backupServiceMock.Setup(b => b.GetJobAsync("job1"))
            .ReturnsAsync(job);

        _databaseProviderMock.Setup(d => d.RestoreAsync(It.IsAny<DatabaseRestoreRequest>()))
            .ReturnsAsync(new DatabaseRestoreResult
            {
                Success = true,
                BytesRestored = 1024
            });

        // Act
        var result = await _service.RestoreFromPointAsync(restorePoint.Id, options);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(RestoreExecutionStatus.Completed);
        result.TargetDatabaseName.Should().Be("targetdb");
        result.InitiatedBy.Should().Be("admin");
    }

    #endregion

    #region Helper Methods

    private BackupResult CreateBackupResult(string jobId, string filePath)
    {
        return new BackupResult
        {
            Id = Guid.NewGuid().ToString(),
            JobId = jobId,
            JobName = "TestJob",
            Status = BackupExecutionStatus.Completed,
            FilePath = filePath,
            FileSizeBytes = 1024
        };
    }

    #endregion
}
