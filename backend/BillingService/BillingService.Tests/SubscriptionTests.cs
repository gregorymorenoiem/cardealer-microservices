using BillingService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace BillingService.Tests;

public class SubscriptionTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesSubscription()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var plan = SubscriptionPlan.Professional;
        var cycle = BillingCycle.Monthly;
        var price = 99.99m;
        var maxUsers = 10;
        var maxVehicles = 100;

        // Act
        var subscription = new Subscription(dealerId, plan, cycle, price, maxUsers, maxVehicles);

        // Assert
        subscription.Id.Should().NotBeEmpty();
        subscription.DealerId.Should().Be(dealerId);
        subscription.Plan.Should().Be(plan);
        subscription.Cycle.Should().Be(cycle);
        subscription.PricePerCycle.Should().Be(price);
        subscription.MaxUsers.Should().Be(maxUsers);
        subscription.MaxVehicles.Should().Be(maxVehicles);
        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_WithTrialDays_CreatesTrialSubscription()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var trialDays = 14;

        // Act
        var subscription = new Subscription(dealerId, SubscriptionPlan.Basic, BillingCycle.Monthly, 49.99m, 5, 50, trialDays);

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.Trial);
        subscription.TrialEndDate.Should().NotBeNull();
        subscription.TrialEndDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(trialDays), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithNegativePrice_ThrowsArgumentException()
    {
        // Arrange
        var dealerId = Guid.NewGuid();

        // Act & Assert
        var act = () => new Subscription(dealerId, SubscriptionPlan.Basic, BillingCycle.Monthly, -10m, 5, 50);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Activate_ChangesStatusToActive()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Basic, BillingCycle.Monthly, 49.99m, 5, 50, 14);

        // Act
        subscription.Activate();

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public void Suspend_ChangesStatusToSuspended()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Basic, BillingCycle.Monthly, 49.99m, 5, 50);

        // Act
        subscription.Suspend();

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.Suspended);
    }

    [Fact]
    public void Cancel_ChangesStatusToCancelled()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Basic, BillingCycle.Monthly, 49.99m, 5, 50);
        var reason = "Customer requested cancellation";

        // Act
        subscription.Cancel(reason);

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
        subscription.CancellationReason.Should().Be(reason);
        subscription.CancelledAt.Should().NotBeNull();
        subscription.EndDate.Should().NotBeNull();
    }

    [Fact]
    public void Upgrade_UpdatesPlanAndLimits()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Basic, BillingCycle.Monthly, 49.99m, 5, 50);
        var newPlan = SubscriptionPlan.Enterprise;
        var newPrice = 199.99m;
        var newMaxUsers = 100;
        var newMaxVehicles = 1000;

        // Act
        subscription.Upgrade(newPlan, newPrice, newMaxUsers, newMaxVehicles);

        // Assert
        subscription.Plan.Should().Be(newPlan);
        subscription.PricePerCycle.Should().Be(newPrice);
        subscription.MaxUsers.Should().Be(newMaxUsers);
        subscription.MaxVehicles.Should().Be(newMaxVehicles);
    }

    [Fact]
    public void ChangeBillingCycle_UpdatesCycleAndPrice()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Professional, BillingCycle.Monthly, 99.99m, 10, 100);
        var newCycle = BillingCycle.Yearly;
        var newPrice = 999.99m;

        // Act
        subscription.ChangeBillingCycle(newCycle, newPrice);

        // Assert
        subscription.Cycle.Should().Be(newCycle);
        subscription.PricePerCycle.Should().Be(newPrice);
    }

    [Fact]
    public void RenewBilling_UpdatesNextBillingDate()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), SubscriptionPlan.Basic, BillingCycle.Monthly, 49.99m, 5, 50);
        var originalBillingDate = subscription.NextBillingDate;

        // Act
        subscription.RenewBilling();

        // Assert
        subscription.NextBillingDate.Should().BeAfter(originalBillingDate!.Value);
        subscription.Status.Should().Be(SubscriptionStatus.Active);
    }
}
