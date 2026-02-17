using BackupDRService.Core.Models;
using FluentAssertions;
using Xunit;

namespace BackupDRService.Tests.Models;

public class BackupOptionsTests
{
    [Fact]
    public void BackupOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var options = new BackupOptions();

        // Assert
        options.DefaultRetentionDays.Should().Be(30);
        options.MaxConcurrentJobs.Should().Be(3);
        options.DefaultStorageType.Should().Be(StorageType.Local);
        options.LocalStoragePath.Should().Be("/var/backups/cardealer");
        options.EnableCompressionByDefault.Should().BeTrue();
        options.EnableEncryptionByDefault.Should().BeFalse();
        options.VerifyBackupAfterCreation.Should().BeTrue();
        options.BackupTimeoutMinutes.Should().Be(60);
        options.RestoreTimeoutMinutes.Should().Be(120);
        options.EnableAutomaticCleanup.Should().BeTrue();
        options.CleanupSchedule.Should().Be("0 2 * * *");
        options.PgDumpPath.Should().Be("pg_dump");
        options.PgRestorePath.Should().Be("pg_restore");
        options.NotifyOnFailure.Should().BeTrue();
        options.NotifyOnSuccess.Should().BeFalse();
    }

    [Fact]
    public void BackupOptions_SectionName_ShouldBeCorrect()
    {
        // Assert
        BackupOptions.SectionName.Should().Be("BackupOptions");
    }

    [Fact]
    public void BackupOptions_AzureSettings_ShouldBeConfigurable()
    {
        // Arrange
        var options = new BackupOptions
        {
            AzureBlobConnectionString = "DefaultEndpointsProtocol=https;...",
            AzureBlobContainerName = "my-backups"
        };

        // Assert
        options.AzureBlobConnectionString.Should().NotBeNullOrEmpty();
        options.AzureBlobContainerName.Should().Be("my-backups");
    }

    [Fact]
    public void BackupOptions_DefaultConnectionStrings_ShouldBeEmptyByDefault()
    {
        // Arrange
        var options = new BackupOptions();

        // Assert
        options.DefaultConnectionStrings.Should().BeEmpty();
    }
}
