using BackupDRService.Core.Interfaces;
using BackupDRService.Core.Models;
using BackupDRService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BackupDRService.Tests.Services;

public class PostgreSqlBackupProviderTests
{
    private readonly Mock<ILogger<PostgreSqlBackupProvider>> _loggerMock;
    private readonly BackupOptions _options;
    private readonly PostgreSqlBackupProvider _provider;

    public PostgreSqlBackupProviderTests()
    {
        _loggerMock = new Mock<ILogger<PostgreSqlBackupProvider>>();

        _options = new BackupOptions
        {
            PgDumpPath = "pg_dump",
            PgRestorePath = "pg_restore",
            LocalStoragePath = Path.GetTempPath()
        };

        _provider = new PostgreSqlBackupProvider(
            _loggerMock.Object,
            Options.Create(_options)
        );
    }

    [Fact]
    public void TargetType_ShouldBePostgreSQL()
    {
        // Assert
        _provider.TargetType.Should().Be(BackupTarget.PostgreSQL);
    }

    [Fact]
    public async Task BackupAsync_WithInvalidConnectionString_ShouldReturnFailure()
    {
        // Arrange
        var request = new DatabaseBackupRequest
        {
            ConnectionString = "invalid",
            DatabaseName = "testdb",
            OutputPath = Path.Combine(Path.GetTempPath(), "test_backup.sql"),
            TimeoutSeconds = 60
        };

        // Act
        var result = await _provider.BackupAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task BackupAsync_WithNonExistentPgDump_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentOptions = new BackupOptions
        {
            PgDumpPath = "nonexistent_pg_dump_executable",
            LocalStoragePath = Path.GetTempPath()
        };

        var provider = new PostgreSqlBackupProvider(
            _loggerMock.Object,
            Options.Create(nonExistentOptions)
        );

        var request = new DatabaseBackupRequest
        {
            ConnectionString = "Host=localhost;Database=test;Username=test;Password=test",
            DatabaseName = "testdb",
            OutputPath = Path.Combine(Path.GetTempPath(), "test_backup.sql"),
            TimeoutSeconds = 60
        };

        // Act
        var result = await provider.BackupAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RestoreAsync_WithInvalidConnectionString_ShouldReturnFailure()
    {
        // Arrange
        var request = new DatabaseRestoreRequest
        {
            ConnectionString = "invalid",
            DatabaseName = "testdb",
            BackupFilePath = Path.Combine(Path.GetTempPath(), "test_backup.sql"),
            TimeoutSeconds = 60
        };

        // Act
        var result = await _provider.RestoreAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RestoreAsync_WithNonExistentBackupFile_ShouldReturnFailure()
    {
        // Arrange
        var request = new DatabaseRestoreRequest
        {
            ConnectionString = "Host=localhost;Database=test;Username=test;Password=test",
            DatabaseName = "testdb",
            BackupFilePath = "nonexistent_backup.sql",
            TimeoutSeconds = 60
        };

        // Act
        var result = await _provider.RestoreAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task RestoreAsync_WithNonExistentPgRestore_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentOptions = new BackupOptions
        {
            PgRestorePath = "nonexistent_pg_restore_executable",
            LocalStoragePath = Path.GetTempPath()
        };

        var provider = new PostgreSqlBackupProvider(
            _loggerMock.Object,
            Options.Create(nonExistentOptions)
        );

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_backup_{Guid.NewGuid()}.sql");
        File.WriteAllText(tempFile, "-- Test backup");

        var request = new DatabaseRestoreRequest
        {
            ConnectionString = "Host=localhost;Database=test;Username=test;Password=test",
            DatabaseName = "testdb",
            BackupFilePath = tempFile,
            TimeoutSeconds = 60
        };

        try
        {
            // Act
            var result = await provider.RestoreAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Assert
        _provider.Should().NotBeNull();
        _provider.TargetType.Should().Be(BackupTarget.PostgreSQL);
    }

    [Fact]
    public async Task BackupAsync_WithEmptyDatabaseName_ShouldReturnFailure()
    {
        // Arrange
        var request = new DatabaseBackupRequest
        {
            ConnectionString = "Host=localhost;Database=test;Username=test;Password=test",
            DatabaseName = "",
            OutputPath = Path.Combine(Path.GetTempPath(), "test_backup.sql"),
            TimeoutSeconds = 60
        };

        // Act
        var result = await _provider.BackupAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BackupAsync_WithEmptyOutputPath_ShouldReturnFailure()
    {
        // Arrange
        var request = new DatabaseBackupRequest
        {
            ConnectionString = "Host=localhost;Database=test;Username=test;Password=test",
            DatabaseName = "testdb",
            OutputPath = "",
            TimeoutSeconds = 60
        };

        // Act
        var result = await _provider.BackupAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreAsync_WithEmptyDatabaseName_ShouldReturnFailure()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_backup_{Guid.NewGuid()}.sql");
        File.WriteAllText(tempFile, "-- Test backup");

        var request = new DatabaseRestoreRequest
        {
            ConnectionString = "Host=localhost;Database=test;Username=test;Password=test",
            DatabaseName = "",
            BackupFilePath = tempFile,
            TimeoutSeconds = 60
        };

        try
        {
            // Act
            var result = await _provider.RestoreAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
