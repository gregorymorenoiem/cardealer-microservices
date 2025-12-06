using FluentAssertions;
using IntegrationService.Domain.Entities;
using Xunit;

namespace IntegrationService.Tests;

public class SyncJobTests
{
    private readonly Guid _dealerId = Guid.NewGuid();
    private readonly Guid _integrationId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void SyncJob_ShouldBeCreated_WithValidParameters()
    {
        // Arrange & Act
        var job = new SyncJob(_dealerId, _integrationId, "Product Sync", "Products", SyncDirection.Inbound, _userId);

        // Assert
        job.Name.Should().Be("Product Sync");
        job.EntityType.Should().Be("Products");
        job.Direction.Should().Be(SyncDirection.Inbound);
        job.Status.Should().Be(SyncStatus.Idle);
    }

    [Fact]
    public void Start_ShouldSetStatusToRunning()
    {
        // Arrange
        var job = new SyncJob(_dealerId, _integrationId, "Sync", "Products", SyncDirection.Inbound, _userId);

        // Act
        job.Start(100);

        // Assert
        job.Status.Should().Be(SyncStatus.Running);
        job.TotalRecords.Should().Be(100);
        job.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateProgress_ShouldUpdateCounters()
    {
        // Arrange
        var job = new SyncJob(_dealerId, _integrationId, "Sync", "Products", SyncDirection.Inbound, _userId);
        job.Start(100);

        // Act
        job.UpdateProgress(50, 45, 5);

        // Assert
        job.ProcessedRecords.Should().Be(50);
        job.SuccessCount.Should().Be(45);
        job.ErrorCount.Should().Be(5);
    }

    [Fact]
    public void Complete_ShouldSetStatusToCompleted()
    {
        // Arrange
        var job = new SyncJob(_dealerId, _integrationId, "Sync", "Products", SyncDirection.Inbound, _userId);
        job.Start(100);

        // Act
        job.Complete();

        // Assert
        job.Status.Should().Be(SyncStatus.Completed);
        job.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Schedule_ShouldThrow_WhenDateInPast()
    {
        // Arrange
        var job = new SyncJob(_dealerId, _integrationId, "Sync", "Products", SyncDirection.Inbound, _userId);

        // Act
        var act = () => job.Schedule(DateTime.UtcNow.AddDays(-1));

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
