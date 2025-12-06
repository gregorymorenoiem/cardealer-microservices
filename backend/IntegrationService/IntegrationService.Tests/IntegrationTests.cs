using FluentAssertions;
using IntegrationService.Domain.Entities;
using Xunit;

namespace IntegrationService.Tests;

public class IntegrationTests
{
    private readonly Guid _dealerId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Integration_ShouldBeCreated_WithValidParameters()
    {
        // Arrange & Act
        var integration = new Integration(_dealerId, "WhatsApp Business", IntegrationType.WhatsApp, _userId, "WhatsApp integration");

        // Assert
        integration.Name.Should().Be("WhatsApp Business");
        integration.Type.Should().Be(IntegrationType.WhatsApp);
        integration.Status.Should().Be(IntegrationStatus.Pending);
        integration.DealerId.Should().Be(_dealerId);
    }

    [Fact]
    public void Integration_ShouldThrow_WhenNameIsEmpty()
    {
        // Arrange & Act
        var act = () => new Integration(_dealerId, "", IntegrationType.WhatsApp, _userId);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Activate_ShouldSetStatusToActive()
    {
        // Arrange
        var integration = new Integration(_dealerId, "Test", IntegrationType.Api, _userId);

        // Act
        integration.Activate();

        // Assert
        integration.Status.Should().Be(IntegrationStatus.Active);
    }

    [Fact]
    public void SetCredentials_ShouldStoreCredentials()
    {
        // Arrange
        var integration = new Integration(_dealerId, "Test", IntegrationType.Api, _userId);

        // Act
        integration.SetCredentials("api-key-123", "secret-456");

        // Assert
        integration.ApiKey.Should().Be("api-key-123");
        integration.ApiSecret.Should().Be("secret-456");
    }

    [Fact]
    public void MarkAsError_ShouldSetStatusAndError()
    {
        // Arrange
        var integration = new Integration(_dealerId, "Test", IntegrationType.Api, _userId);

        // Act
        integration.MarkAsError("Connection failed");

        // Assert
        integration.Status.Should().Be(IntegrationStatus.Error);
        integration.LastError.Should().Be("Connection failed");
    }
}
