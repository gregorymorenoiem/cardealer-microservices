using BackupDRService.Core.Interfaces;
using BackupDRService.Core.Models;
using FluentAssertions;
using Xunit;

namespace BackupDRService.Tests.Interfaces;

public class DatabaseModelsTests
{
    #region DatabaseBackupRequest Tests

    [Fact]
    public void DatabaseBackupRequest_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var request = new DatabaseBackupRequest();

        // Assert
        request.ConnectionString.Should().BeEmpty();
        request.DatabaseName.Should().BeEmpty();
        request.OutputPath.Should().BeEmpty();
        request.BackupType.Should().Be(BackupType.Full);
        request.Compress.Should().BeTrue();
        request.TimeoutSeconds.Should().Be(3600);
        request.Options.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void DatabaseBackupRequest_ShouldAllowSettingAllProperties()
    {
        // Arrange & Act
        var request = new DatabaseBackupRequest
        {
            ConnectionString = "Host=localhost;Database=test",
            DatabaseName = "testdb",
            OutputPath = "/backup/test.bak",
            BackupType = BackupType.Incremental,
            Compress = false,
            TimeoutSeconds = 7200,
            Options = new Dictionary<string, string> { ["key1"] = "value1" }
        };

        // Assert
        request.ConnectionString.Should().Be("Host=localhost;Database=test");
        request.DatabaseName.Should().Be("testdb");
        request.OutputPath.Should().Be("/backup/test.bak");
        request.BackupType.Should().Be(BackupType.Incremental);
        request.Compress.Should().BeFalse();
        request.TimeoutSeconds.Should().Be(7200);
        request.Options.Should().ContainKey("key1");
    }

    #endregion

    #region DatabaseBackupResult Tests

    [Fact]
    public void DatabaseBackupResult_Succeeded_ShouldCreateSuccessResult()
    {
        // Arrange
        var duration = TimeSpan.FromMinutes(5);

        // Act
        var result = DatabaseBackupResult.Succeeded("/path/file.bak", 1024, "checksum123", duration);

        // Assert
        result.Success.Should().BeTrue();
        result.FilePath.Should().Be("/path/file.bak");
        result.FileSizeBytes.Should().Be(1024);
        result.Checksum.Should().Be("checksum123");
        result.Duration.Should().Be(duration);
        result.ErrorMessage.Should().BeNull();
        result.ErrorDetails.Should().BeNull();
    }

    [Fact]
    public void DatabaseBackupResult_Failed_ShouldCreateFailedResult()
    {
        // Act
        var result = DatabaseBackupResult.Failed("Error occurred", "Stack trace here");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Error occurred");
        result.ErrorDetails.Should().Be("Stack trace here");
    }

    [Fact]
    public void DatabaseBackupResult_Failed_WithoutDetails_ShouldCreateFailedResult()
    {
        // Act
        var result = DatabaseBackupResult.Failed("Error occurred");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Error occurred");
        result.ErrorDetails.Should().BeNull();
    }

    [Fact]
    public void DatabaseBackupResult_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var result = new DatabaseBackupResult();

        // Assert
        result.Success.Should().BeFalse();
        result.FilePath.Should().BeEmpty();
        result.FileSizeBytes.Should().Be(0);
        result.Checksum.Should().BeEmpty();
        result.Duration.Should().Be(TimeSpan.Zero);
        result.TablesBackedUp.Should().Be(0);
        result.RowsBackedUp.Should().Be(0);
    }

    #endregion

    #region DatabaseRestoreRequest Tests

    [Fact]
    public void DatabaseRestoreRequest_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var request = new DatabaseRestoreRequest();

        // Assert
        request.ConnectionString.Should().BeEmpty();
        request.DatabaseName.Should().BeEmpty();
        request.BackupFilePath.Should().BeEmpty();
        request.DropExistingDatabase.Should().BeFalse();
        request.CreateIfNotExists.Should().BeTrue();
        request.TimeoutSeconds.Should().Be(7200);
        request.Options.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void DatabaseRestoreRequest_ShouldAllowSettingAllProperties()
    {
        // Arrange & Act
        var request = new DatabaseRestoreRequest
        {
            ConnectionString = "Host=localhost;Database=test",
            DatabaseName = "restoreddb",
            BackupFilePath = "/backup/test.bak",
            DropExistingDatabase = true,
            CreateIfNotExists = false,
            TimeoutSeconds = 14400,
            Options = new Dictionary<string, string> { ["restore_option"] = "value" }
        };

        // Assert
        request.ConnectionString.Should().Be("Host=localhost;Database=test");
        request.DatabaseName.Should().Be("restoreddb");
        request.BackupFilePath.Should().Be("/backup/test.bak");
        request.DropExistingDatabase.Should().BeTrue();
        request.CreateIfNotExists.Should().BeFalse();
        request.TimeoutSeconds.Should().Be(14400);
        request.Options.Should().ContainKey("restore_option");
    }

    #endregion

    #region DatabaseRestoreResult Tests

    [Fact]
    public void DatabaseRestoreResult_Succeeded_ShouldCreateSuccessResult()
    {
        // Arrange
        var duration = TimeSpan.FromMinutes(10);

        // Act
        var result = DatabaseRestoreResult.Succeeded(duration, 2048, 15, 5000);

        // Assert
        result.Success.Should().BeTrue();
        result.Duration.Should().Be(duration);
        result.BytesRestored.Should().Be(2048);
        result.TablesRestored.Should().Be(15);
        result.RowsRestored.Should().Be(5000);
        result.ErrorMessage.Should().BeNull();
        result.ErrorDetails.Should().BeNull();
    }

    [Fact]
    public void DatabaseRestoreResult_Failed_ShouldCreateFailedResult()
    {
        // Act
        var result = DatabaseRestoreResult.Failed("Restore failed", "Connection timeout");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Restore failed");
        result.ErrorDetails.Should().Be("Connection timeout");
    }

    [Fact]
    public void DatabaseRestoreResult_Failed_WithoutDetails_ShouldCreateFailedResult()
    {
        // Act
        var result = DatabaseRestoreResult.Failed("Restore failed");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Restore failed");
        result.ErrorDetails.Should().BeNull();
    }

    [Fact]
    public void DatabaseRestoreResult_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var result = new DatabaseRestoreResult();

        // Assert
        result.Success.Should().BeFalse();
        result.Duration.Should().Be(TimeSpan.Zero);
        result.BytesRestored.Should().Be(0);
        result.TablesRestored.Should().Be(0);
        result.RowsRestored.Should().Be(0);
    }

    #endregion

    #region DatabaseInfo Tests

    [Fact]
    public void DatabaseInfo_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var info = new DatabaseInfo();

        // Assert
        info.Name.Should().BeEmpty();
        info.SizeBytes.Should().Be(0);
        info.TableCount.Should().Be(0);
        info.TotalRows.Should().Be(0);
        info.CreatedAt.Should().BeNull();
        info.ServerVersion.Should().BeEmpty();
        info.TableNames.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void DatabaseInfo_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddYears(-1);

        // Act
        var info = new DatabaseInfo
        {
            Name = "testdb",
            SizeBytes = 1024 * 1024 * 100, // 100 MB
            TableCount = 25,
            TotalRows = 50000,
            CreatedAt = createdAt,
            ServerVersion = "PostgreSQL 15.4",
            TableNames = new List<string> { "users", "orders", "products" }
        };

        // Assert
        info.Name.Should().Be("testdb");
        info.SizeBytes.Should().Be(1024 * 1024 * 100);
        info.TableCount.Should().Be(25);
        info.TotalRows.Should().Be(50000);
        info.CreatedAt.Should().Be(createdAt);
        info.ServerVersion.Should().Be("PostgreSQL 15.4");
        info.TableNames.Should().HaveCount(3);
        info.TableNames.Should().Contain("users");
    }

    [Fact]
    public void DatabaseInfo_TableNames_ShouldBeModifiable()
    {
        // Arrange
        var info = new DatabaseInfo();

        // Act
        info.TableNames.Add("new_table");
        info.TableNames.Add("another_table");

        // Assert
        info.TableNames.Should().HaveCount(2);
        info.TableNames.Should().Contain("new_table");
        info.TableNames.Should().Contain("another_table");
    }

    #endregion
}
