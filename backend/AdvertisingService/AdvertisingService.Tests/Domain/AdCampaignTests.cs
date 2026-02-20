using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AdvertisingService.Tests.Domain;

public class AdCampaignTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnCampaign()
    {
        var vehicleId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var campaign = AdCampaign.Create(
            vehicleId, ownerId, "Individual",
            AdPlacementType.FeaturedSpot, CampaignPricingModel.PerView,
            500m, 0.50m, null, 1000,
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 0.75m);

        campaign.Should().NotBeNull();
        campaign.OwnerId.Should().Be(ownerId);
        campaign.VehicleId.Should().Be(vehicleId);
        campaign.OwnerType.Should().Be("Individual");
        campaign.Status.Should().Be(CampaignStatus.PendingPayment);
        campaign.PlacementType.Should().Be(AdPlacementType.FeaturedSpot);
        campaign.PricingModel.Should().Be(CampaignPricingModel.PerView);
        campaign.PricePerView.Should().Be(0.50m);
        campaign.TotalBudget.Should().Be(500m);
        campaign.SpentBudget.Should().Be(0m);
        campaign.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Activate_WhenPendingPayment_ShouldChangeStatusToActive()
    {
        var campaign = CreateTestCampaign();
        var billingRef = Guid.NewGuid();

        campaign.Activate(billingRef);

        campaign.Status.Should().Be(CampaignStatus.Active);
        campaign.BillingReferenceId.Should().Be(billingRef);
    }

    [Fact]
    public void Activate_WhenNotPendingPayment_ShouldThrow()
    {
        var campaign = CreateTestCampaign();
        campaign.Activate(Guid.NewGuid());
        campaign.Pause();

        var act = () => campaign.Activate(Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Pause_WhenActive_ShouldChangeStatusToPaused()
    {
        var campaign = CreateTestCampaign();
        campaign.Activate(Guid.NewGuid());

        campaign.Pause();

        campaign.Status.Should().Be(CampaignStatus.Paused);
    }

    [Fact]
    public void Resume_WhenPaused_ShouldChangeStatusToActive()
    {
        var campaign = CreateTestCampaign();
        campaign.Activate(Guid.NewGuid());
        campaign.Pause();

        campaign.Resume();

        campaign.Status.Should().Be(CampaignStatus.Active);
    }

    [Fact]
    public void Cancel_WhenActive_ShouldChangeStatusToCancelled()
    {
        var campaign = CreateTestCampaign();
        campaign.Activate(Guid.NewGuid());

        campaign.Cancel();

        campaign.Status.Should().Be(CampaignStatus.Cancelled);
    }

    [Fact]
    public void RecordView_ShouldIncrementViewsAndDeductBudget()
    {
        var campaign = CreateTestCampaign();
        campaign.Activate(Guid.NewGuid());

        campaign.RecordView();

        campaign.ViewsConsumed.Should().Be(1);
        campaign.SpentBudget.Should().Be(0.50m);
    }

    [Fact]
    public void RecordView_WhenViewCountReached_ShouldComplete()
    {
        var campaign = AdCampaign.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Individual",
            AdPlacementType.FeaturedSpot, CampaignPricingModel.PerView,
            0.50m, 0.50m, null, 1,
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 0.5m);
        campaign.Activate(Guid.NewGuid());

        campaign.RecordView();

        campaign.ViewsConsumed.Should().Be(1);
        campaign.Status.Should().Be(CampaignStatus.Completed);
    }

    [Fact]
    public void RecordClick_ShouldIncrementClicks()
    {
        var campaign = CreateTestCampaign();
        campaign.Activate(Guid.NewGuid());

        campaign.RecordClick();

        campaign.Clicks.Should().Be(1);
    }

    [Fact]
    public void GetCtr_WithZeroViews_ShouldReturnZero()
    {
        var campaign = CreateTestCampaign();
        campaign.GetCtr().Should().Be(0);
    }

    [Fact]
    public void GetCtr_WithViewsAndClicks_ShouldCalculateCorrectly()
    {
        var campaign = CreateTestCampaign();
        campaign.Activate(Guid.NewGuid());
        for (int i = 0; i < 100; i++) campaign.RecordView();
        for (int i = 0; i < 5; i++) campaign.RecordClick();

        var ctr = campaign.GetCtr();
        ctr.Should().Be(0.05m);
    }

    [Fact]
    public void IsExpired_WhenEndDatePassed_ShouldReturnTrue()
    {
        var campaign = AdCampaign.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Individual",
            AdPlacementType.FeaturedSpot, CampaignPricingModel.PerView,
            500m, 0.50m, null, 1000,
            DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-1), 0.5m);

        campaign.IsExpired().Should().BeTrue();
    }

    [Fact]
    public void MarkExpired_ShouldSetStatusToExpired()
    {
        var campaign = CreateTestCampaign();
        campaign.Activate(Guid.NewGuid());

        campaign.MarkExpired();

        campaign.Status.Should().Be(CampaignStatus.Expired);
    }

    [Fact]
    public void GetRemainingBudgetRatio_ShouldCalculateCorrectly()
    {
        var campaign = CreateTestCampaign();
        campaign.Activate(Guid.NewGuid());
        for (int i = 0; i < 500; i++) campaign.RecordView();

        var ratio = campaign.GetRemainingBudgetRatio();
        ratio.Should().Be(0.50m);
    }

    [Fact]
    public void UpdateQualityScore_ShouldClampAndSetScore()
    {
        var campaign = CreateTestCampaign();

        campaign.UpdateQualityScore(0.85m);

        campaign.QualityScore.Should().Be(0.85m);
    }

    private static AdCampaign CreateTestCampaign()
    {
        return AdCampaign.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Individual",
            AdPlacementType.FeaturedSpot, CampaignPricingModel.PerView,
            500m, 0.50m, null, 1000,
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 0.75m);
    }
}
