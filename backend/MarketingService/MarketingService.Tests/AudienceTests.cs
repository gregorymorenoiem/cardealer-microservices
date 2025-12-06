using FluentAssertions;
using MarketingService.Domain.Entities;
using Xunit;

namespace MarketingService.Tests;

public class AudienceTests
{
    private readonly Guid _dealerId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Audience_ShouldBeCreated_WithValidParameters()
    {
        // Arrange & Act
        var audience = new Audience(_dealerId, "VIP Customers", AudienceType.Dynamic, _userId, "High-value customers");

        // Assert
        audience.Name.Should().Be("VIP Customers");
        audience.Type.Should().Be(AudienceType.Dynamic);
        audience.Description.Should().Be("High-value customers");
        audience.IsActive.Should().BeTrue();
        audience.MemberCount.Should().Be(0);
    }

    [Fact]
    public void Audience_ShouldThrow_WhenNameIsEmpty()
    {
        // Arrange & Act
        var act = () => new Audience(_dealerId, "", AudienceType.Static, _userId, null);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetFilterCriteria_ShouldSetFilterCriteria()
    {
        // Arrange
        var audience = new Audience(_dealerId, "Test", AudienceType.Dynamic, _userId, null);

        // Act
        audience.SetFilterCriteria("{\"purchaseCount\": { \"gte\": 5 }}");

        // Assert
        audience.FilterCriteria.Should().Be("{\"purchaseCount\": { \"gte\": 5 }}");
    }

    [Fact]
    public void UpdateMemberCount_ShouldUpdateCount()
    {
        // Arrange
        var audience = new Audience(_dealerId, "Test", AudienceType.Static, _userId, null);

        // Act
        audience.UpdateMemberCount(500);

        // Assert
        audience.MemberCount.Should().Be(500);
        audience.LastSyncedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        // Arrange
        var audience = new Audience(_dealerId, "Test", AudienceType.Static, _userId, null);

        // Act
        audience.Deactivate();

        // Assert
        audience.IsActive.Should().BeFalse();
    }
}
