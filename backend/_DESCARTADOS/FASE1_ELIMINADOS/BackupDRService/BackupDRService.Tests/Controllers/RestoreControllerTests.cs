using BackupDRService.Api.Controllers;
using BackupDRService.Core.Interfaces;
using BackupDRService.Core.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackupDRService.Tests.Controllers;

public class RestoreControllerTests
{
    private readonly Mock<IRestoreService> _restoreServiceMock;
    private readonly Mock<IBackupService> _backupServiceMock;
    private readonly Mock<ILogger<RestoreController>> _loggerMock;
    private readonly RestoreController _controller;

    public RestoreControllerTests()
    {
        _restoreServiceMock = new Mock<IRestoreService>();
        _backupServiceMock = new Mock<IBackupService>();
        _loggerMock = new Mock<ILogger<RestoreController>>();
        _controller = new RestoreController(
            _restoreServiceMock.Object,
            _backupServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllRestorePoints_ShouldReturnOkWithPoints()
    {
        // Arrange
        var points = new List<RestorePoint>
        {
            new RestorePoint { Id = "1", Name = "Point 1" },
            new RestorePoint { Id = "2", Name = "Point 2" }
        };
        _restoreServiceMock.Setup(s => s.GetAllRestorePointsAsync()).ReturnsAsync(points);

        // Act
        var result = await _controller.GetAllRestorePoints();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPoints = okResult.Value.Should().BeAssignableTo<IEnumerable<RestorePoint>>().Subject;
        returnedPoints.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRestorePoint_ShouldReturnOk_WhenExists()
    {
        // Arrange
        var point = new RestorePoint { Id = "test-id", Name = "Test Point" };
        _restoreServiceMock.Setup(s => s.GetRestorePointAsync("test-id")).ReturnsAsync(point);

        // Act
        var result = await _controller.GetRestorePoint("test-id");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPoint = okResult.Value.Should().BeOfType<RestorePoint>().Subject;
        returnedPoint.Name.Should().Be("Test Point");
    }

    [Fact]
    public async Task GetRestorePoint_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        _restoreServiceMock.Setup(s => s.GetRestorePointAsync("non-existent"))
            .ReturnsAsync((RestorePoint?)null);

        // Act
        var result = await _controller.GetRestorePoint("non-existent");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateRestorePoint_ShouldReturnCreated_WhenBackupExists()
    {
        // Arrange
        var backupResult = BackupResult.Success("job-id", "Test", "/backup/test.backup", 1024, "checksum");
        var restorePoint = new RestorePoint { Id = "new-id", Name = "New Point" };

        _backupServiceMock.Setup(s => s.GetBackupResultAsync("backup-id")).ReturnsAsync(backupResult);
        _restoreServiceMock.Setup(s => s.CreateRestorePointAsync(backupResult, "New Point", null))
            .ReturnsAsync(restorePoint);

        var request = new CreateRestorePointRequest
        {
            BackupResultId = "backup-id",
            Name = "New Point"
        };

        // Act
        var result = await _controller.CreateRestorePoint(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedPoint = createdResult.Value.Should().BeOfType<RestorePoint>().Subject;
        returnedPoint.Name.Should().Be("New Point");
    }

    [Fact]
    public async Task CreateRestorePoint_ShouldReturnNotFound_WhenBackupDoesNotExist()
    {
        // Arrange
        _backupServiceMock.Setup(s => s.GetBackupResultAsync("non-existent"))
            .ReturnsAsync((BackupResult?)null);

        var request = new CreateRestorePointRequest
        {
            BackupResultId = "non-existent",
            Name = "New Point"
        };

        // Act
        var result = await _controller.CreateRestorePoint(request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteRestorePoint_ShouldReturnNoContent_WhenDeleted()
    {
        // Arrange
        _restoreServiceMock.Setup(s => s.DeleteRestorePointAsync("test-id")).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteRestorePoint("test-id");

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RestoreFromPoint_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var restoreResult = RestoreResult.Success("point-id", "Test Point", 1024);
        _restoreServiceMock.Setup(s => s.RestoreFromPointAsync("point-id", It.IsAny<RestoreOptions?>()))
            .ReturnsAsync(restoreResult);

        // Act
        var result = await _controller.RestoreFromPoint("point-id", null);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RestoreFromPoint_ShouldReturnBadRequest_WhenFailed()
    {
        // Arrange
        var restoreResult = RestoreResult.Failure("point-id", "Test Point", "Restore failed");
        _restoreServiceMock.Setup(s => s.RestoreFromPointAsync("point-id", It.IsAny<RestoreOptions?>()))
            .ReturnsAsync(restoreResult);

        // Act
        var result = await _controller.RestoreFromPoint("point-id", null);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task VerifyRestorePoint_ShouldReturnOk()
    {
        // Arrange
        _restoreServiceMock.Setup(s => s.VerifyRestorePointAsync("test-id")).ReturnsAsync(true);

        // Act
        var result = await _controller.VerifyRestorePoint("test-id");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task TestRestorePoint_ShouldReturnOk()
    {
        // Arrange
        _restoreServiceMock.Setup(s => s.TestRestoreAsync("test-id")).ReturnsAsync(true);

        // Act
        var result = await _controller.TestRestorePoint("test-id");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRestoreResults_ShouldReturnOk()
    {
        // Arrange
        var results = new List<RestoreResult>
        {
            RestoreResult.Success("p1", "Point 1", 1024),
            RestoreResult.Success("p2", "Point 2", 2048)
        };
        _restoreServiceMock.Setup(s => s.GetRestoreResultsAsync()).ReturnsAsync(results);

        // Act
        var result = await _controller.GetRestoreResults();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResults = okResult.Value.Should().BeAssignableTo<IEnumerable<RestoreResult>>().Subject;
        returnedResults.Should().HaveCount(2);
    }

    // =========== GetAvailableRestorePoints Tests ===========

    [Fact]
    public async Task GetAvailableRestorePoints_ShouldReturnOkWithPoints()
    {
        // Arrange
        var points = new List<RestorePoint>
        {
            new RestorePoint { Id = "1", Name = "Point 1", IsAvailable = true },
            new RestorePoint { Id = "2", Name = "Point 2", IsAvailable = true }
        };
        _restoreServiceMock.Setup(s => s.GetAvailableRestorePointsAsync()).ReturnsAsync(points);

        // Act
        var result = await _controller.GetAvailableRestorePoints();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPoints = okResult.Value.Should().BeAssignableTo<IEnumerable<RestorePoint>>().Subject;
        returnedPoints.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAvailableRestorePoints_ShouldReturnEmptyList_WhenNoAvailablePoints()
    {
        // Arrange
        _restoreServiceMock.Setup(s => s.GetAvailableRestorePointsAsync()).ReturnsAsync(new List<RestorePoint>());

        // Act
        var result = await _controller.GetAvailableRestorePoints();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPoints = okResult.Value.Should().BeAssignableTo<IEnumerable<RestorePoint>>().Subject;
        returnedPoints.Should().BeEmpty();
    }

    // =========== GetRestorePointsByJob Tests ===========

    [Fact]
    public async Task GetRestorePointsByJob_ShouldReturnOkWithPoints()
    {
        // Arrange
        var points = new List<RestorePoint>
        {
            new RestorePoint { Id = "1", Name = "Point 1", JobId = "job-id" }
        };
        _restoreServiceMock.Setup(s => s.GetRestorePointsByJobAsync("job-id")).ReturnsAsync(points);

        // Act
        var result = await _controller.GetRestorePointsByJob("job-id");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPoints = okResult.Value.Should().BeAssignableTo<IEnumerable<RestorePoint>>().Subject;
        returnedPoints.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetRestorePointsByJob_ShouldReturnEmptyList_WhenNoPointsForJob()
    {
        // Arrange
        _restoreServiceMock.Setup(s => s.GetRestorePointsByJobAsync("job-id")).ReturnsAsync(new List<RestorePoint>());

        // Act
        var result = await _controller.GetRestorePointsByJob("job-id");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPoints = okResult.Value.Should().BeAssignableTo<IEnumerable<RestorePoint>>().Subject;
        returnedPoints.Should().BeEmpty();
    }

    // =========== CancelRestore Tests ===========

    [Fact]
    public async Task CancelRestore_ShouldReturnOk_WhenCancelled()
    {
        // Arrange
        _restoreServiceMock.Setup(s => s.CancelRestoreAsync("result-id")).ReturnsAsync(true);

        // Act
        var result = await _controller.CancelRestore("result-id");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CancelRestore_ShouldReturnBadRequest_WhenCannotCancel()
    {
        // Arrange
        _restoreServiceMock.Setup(s => s.CancelRestoreAsync("result-id")).ReturnsAsync(false);

        // Act
        var result = await _controller.CancelRestore("result-id");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // =========== GetRecentRestoreResults Tests ===========

    [Fact]
    public async Task GetRecentRestoreResults_ShouldReturnOkWithResults()
    {
        // Arrange
        var results = new List<RestoreResult>
        {
            RestoreResult.Success("p1", "Point 1", 1024),
            RestoreResult.Success("p2", "Point 2", 2048)
        };
        _restoreServiceMock.Setup(s => s.GetRecentRestoreResultsAsync(10)).ReturnsAsync(results);

        // Act
        var result = await _controller.GetRecentRestoreResults();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResults = okResult.Value.Should().BeAssignableTo<IEnumerable<RestoreResult>>().Subject;
        returnedResults.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentRestoreResults_ShouldUseCustomCount()
    {
        // Arrange
        _restoreServiceMock.Setup(s => s.GetRecentRestoreResultsAsync(5)).ReturnsAsync(new List<RestoreResult>());

        // Act
        await _controller.GetRecentRestoreResults(5);

        // Assert
        _restoreServiceMock.Verify(s => s.GetRecentRestoreResultsAsync(5), Times.Once);
    }

    // =========== GetRestoreResult Tests ===========

    [Fact]
    public async Task GetRestoreResult_ShouldReturnOk_WhenResultExists()
    {
        // Arrange
        var restoreResult = RestoreResult.Success("point-id", "Test Point", 1024);
        _restoreServiceMock.Setup(s => s.GetRestoreResultAsync("result-id")).ReturnsAsync(restoreResult);

        // Act
        var result = await _controller.GetRestoreResult("result-id");

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRestoreResult_ShouldReturnNotFound_WhenResultDoesNotExist()
    {
        // Arrange
        _restoreServiceMock.Setup(s => s.GetRestoreResultAsync("non-existent")).ReturnsAsync((RestoreResult?)null);

        // Act
        var result = await _controller.GetRestoreResult("non-existent");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // =========== RestoreFromBackup Tests ===========

    [Fact]
    public async Task RestoreFromBackup_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var restoreResult = RestoreResult.Success("backup-id", "Test Backup", 1024);
        _restoreServiceMock.Setup(s => s.RestoreFromBackupAsync("backup-id", It.IsAny<RestoreOptions?>()))
            .ReturnsAsync(restoreResult);

        // Act
        var result = await _controller.RestoreFromBackup("backup-id", null);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RestoreFromBackup_ShouldReturnBadRequest_WhenFailed()
    {
        // Arrange
        var restoreResult = RestoreResult.Failure("backup-id", "Test Backup", "Restore failed");
        _restoreServiceMock.Setup(s => s.RestoreFromBackupAsync("backup-id", It.IsAny<RestoreOptions?>()))
            .ReturnsAsync(restoreResult);

        // Act
        var result = await _controller.RestoreFromBackup("backup-id", null);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RestoreFromBackup_ShouldPassOptions_WhenProvided()
    {
        // Arrange
        var restoreResult = RestoreResult.Success("backup-id", "Test Backup", 1024);
        RestoreOptions? capturedOptions = null;
        _restoreServiceMock.Setup(s => s.RestoreFromBackupAsync("backup-id", It.IsAny<RestoreOptions?>()))
            .Callback<string, RestoreOptions?>((_, opts) => capturedOptions = opts)
            .ReturnsAsync(restoreResult);

        var request = new RestoreRequest
        {
            TargetConnectionString = "Host=newhost",
            TargetDatabaseName = "newdb",
            Mode = RestoreMode.NewDatabase,
            DropExistingDatabase = true,
            CreateIfNotExists = false,
            InitiatedBy = "admin"
        };

        // Act
        await _controller.RestoreFromBackup("backup-id", request);

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.TargetConnectionString.Should().Be("Host=newhost");
        capturedOptions.TargetDatabaseName.Should().Be("newdb");
        capturedOptions.Mode.Should().Be(RestoreMode.NewDatabase);
        capturedOptions.DropExistingDatabase.Should().BeTrue();
        capturedOptions.CreateIfNotExists.Should().BeFalse();
        capturedOptions.InitiatedBy.Should().Be("admin");
    }

    // =========== CleanupExpiredRestorePoints Tests ===========

    [Fact]
    public async Task CleanupExpiredRestorePoints_ShouldReturnOk_WithDeletedCount()
    {
        // Arrange
        _restoreServiceMock.Setup(s => s.CleanupExpiredRestorePointsAsync()).ReturnsAsync(5);

        // Act
        var result = await _controller.CleanupExpiredRestorePoints();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CleanupExpiredRestorePoints_ShouldReturnOk_WhenNoExpiredPoints()
    {
        // Arrange
        _restoreServiceMock.Setup(s => s.CleanupExpiredRestorePointsAsync()).ReturnsAsync(0);

        // Act
        var result = await _controller.CleanupExpiredRestorePoints();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // =========== DeleteRestorePoint Tests ===========

    [Fact]
    public async Task DeleteRestorePoint_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        _restoreServiceMock.Setup(s => s.DeleteRestorePointAsync("non-existent")).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteRestorePoint("non-existent");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // =========== RestoreFromPoint with Options Tests ===========

    [Fact]
    public async Task RestoreFromPoint_ShouldPassOptions_WhenProvided()
    {
        // Arrange
        var restoreResult = RestoreResult.Success("point-id", "Test Point", 1024);
        RestoreOptions? capturedOptions = null;
        _restoreServiceMock.Setup(s => s.RestoreFromPointAsync("point-id", It.IsAny<RestoreOptions?>()))
            .Callback<string, RestoreOptions?>((_, opts) => capturedOptions = opts)
            .ReturnsAsync(restoreResult);

        var request = new RestoreRequest
        {
            TargetConnectionString = "Host=testhost",
            TargetDatabaseName = "testdb",
            Mode = RestoreMode.InPlace,
            DropExistingDatabase = false,
            CreateIfNotExists = true,
            InitiatedBy = "user"
        };

        // Act
        await _controller.RestoreFromPoint("point-id", request);

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.TargetConnectionString.Should().Be("Host=testhost");
        capturedOptions.Mode.Should().Be(RestoreMode.InPlace);
    }

    // =========== TestRestorePoint Tests ===========

    [Fact]
    public async Task TestRestorePoint_ShouldReturnOk_WhenTestFails()
    {
        // Arrange
        _restoreServiceMock.Setup(s => s.TestRestoreAsync("test-id")).ReturnsAsync(false);

        // Act
        var result = await _controller.TestRestorePoint("test-id");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // =========== VerifyRestorePoint Tests ===========

    [Fact]
    public async Task VerifyRestorePoint_ShouldReturnOk_WhenVerificationFails()
    {
        // Arrange
        _restoreServiceMock.Setup(s => s.VerifyRestorePointAsync("test-id")).ReturnsAsync(false);

        // Act
        var result = await _controller.VerifyRestorePoint("test-id");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // =========== CreateRestorePoint with Description Tests ===========

    [Fact]
    public async Task CreateRestorePoint_ShouldPassDescription_WhenProvided()
    {
        // Arrange
        var backupResult = BackupResult.Success("job-id", "Test", "/backup/test.backup", 1024, "checksum");
        var restorePoint = new RestorePoint { Id = "new-id", Name = "New Point", Description = "Test description" };

        _backupServiceMock.Setup(s => s.GetBackupResultAsync("backup-id")).ReturnsAsync(backupResult);
        _restoreServiceMock.Setup(s => s.CreateRestorePointAsync(backupResult, "New Point", "Test description"))
            .ReturnsAsync(restorePoint);

        var request = new CreateRestorePointRequest
        {
            BackupResultId = "backup-id",
            Name = "New Point",
            Description = "Test description"
        };

        // Act
        var result = await _controller.CreateRestorePoint(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        _restoreServiceMock.Verify(s => s.CreateRestorePointAsync(backupResult, "New Point", "Test description"), Times.Once);
    }
}
