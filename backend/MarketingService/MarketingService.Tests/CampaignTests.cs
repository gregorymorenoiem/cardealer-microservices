using FluentAssertions;
using MarketingService.Domain.Entities;
using Xunit;

namespace MarketingService.Tests;

public class CampaignTests
{
    private readonly Guid _dealerId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Campaign_ShouldBeCreated_WithValidParameters()
    {
        // Arrange & Act
        var campaign = new Campaign(_dealerId, "Test Campaign", CampaignType.Email, _userId, "Test description");

        // Assert
        campaign.Name.Should().Be("Test Campaign");
        campaign.Type.Should().Be(CampaignType.Email);
        campaign.Status.Should().Be(CampaignStatus.Draft);
        campaign.DealerId.Should().Be(_dealerId);
        campaign.CreatedBy.Should().Be(_userId);
    }

    [Fact]
    public void Campaign_ShouldThrow_WhenNameIsEmpty()
    {
        // Arrange & Act
        var act = () => new Campaign(_dealerId, "", CampaignType.Email, _userId);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Schedule_ShouldSetScheduledStatus_WhenDraftCampaign()
    {
        // Arrange
        var campaign = new Campaign(_dealerId, "Test", CampaignType.Email, _userId);
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act
        campaign.Schedule(futureDate);

        // Assert
        campaign.Status.Should().Be(CampaignStatus.Scheduled);
        campaign.ScheduledDate.Should().Be(futureDate);
    }

    [Fact]
    public void Start_ShouldSetRunningStatus_AndUpdateCounters()
    {
        // Arrange
        var campaign = new Campaign(_dealerId, "Test", CampaignType.Email, _userId);

        // Act
        campaign.Start(1000);

        // Assert
        campaign.Status.Should().Be(CampaignStatus.Running);
        campaign.TotalRecipients.Should().Be(1000);
        campaign.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public void Pause_ShouldSetPausedStatus_WhenRunning()
    {
        // Arrange
        var campaign = new Campaign(_dealerId, "Test", CampaignType.Email, _userId);
        campaign.Start(100);

        // Act
        campaign.Pause();

        // Assert
        campaign.Status.Should().Be(CampaignStatus.Paused);
    }

    [Fact]
    public void Complete_ShouldSetCompletedStatus()
    {
        // Arrange
        var campaign = new Campaign(_dealerId, "Test", CampaignType.Email, _userId);
        campaign.Start(100);

        // Act
        campaign.Complete();

        // Assert
        campaign.Status.Should().Be(CampaignStatus.Completed);
        campaign.CompletedAt.Should().NotBeNull();
    }
}
