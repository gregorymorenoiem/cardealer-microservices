using FluentAssertions;
using IntegrationService.Domain.Entities;
using Xunit;

namespace IntegrationService.Tests;

public class WebhookEventTests
{
    private readonly Guid _dealerId = Guid.NewGuid();
    private readonly Guid _integrationId = Guid.NewGuid();

    [Fact]
    public void WebhookEvent_ShouldBeCreated_WithValidParameters()
    {
        // Arrange & Act
        var webhookEvent = new WebhookEvent(
            _dealerId,
            _integrationId,
            WebhookEventType.Inbound,
            "message.received",
            "{\"message\": \"Hello\"}",
            null,
            3
        );

        // Assert
        webhookEvent.EventName.Should().Be("message.received");
        webhookEvent.EventType.Should().Be(WebhookEventType.Inbound);
        webhookEvent.Status.Should().Be(WebhookStatus.Pending);
        webhookEvent.MaxRetries.Should().Be(3);
    }

    [Fact]
    public void Complete_ShouldSetStatusToCompleted()
    {
        // Arrange
        var webhookEvent = new WebhookEvent(_dealerId, _integrationId, WebhookEventType.Inbound, "test", "{}");
        webhookEvent.StartProcessing();

        // Act
        webhookEvent.Complete("{\"success\": true}");

        // Assert
        webhookEvent.Status.Should().Be(WebhookStatus.Completed);
        webhookEvent.Response.Should().Be("{\"success\": true}");
        webhookEvent.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void Fail_ShouldSetRetryingStatus_WhenRetriesAvailable()
    {
        // Arrange
        var webhookEvent = new WebhookEvent(_dealerId, _integrationId, WebhookEventType.Inbound, "test", "{}", null, 3);

        // Act
        webhookEvent.Fail("Connection timeout");

        // Assert
        webhookEvent.Status.Should().Be(WebhookStatus.Retrying);
        webhookEvent.RetryCount.Should().Be(1);
        webhookEvent.NextRetryAt.Should().NotBeNull();
    }

    [Fact]
    public void Fail_ShouldSetFailedStatus_WhenNoRetriesLeft()
    {
        // Arrange
        var webhookEvent = new WebhookEvent(_dealerId, _integrationId, WebhookEventType.Inbound, "test", "{}", null, 1);
        webhookEvent.Fail("Error 1");

        // Act
        webhookEvent.Fail("Error 2");

        // Assert
        webhookEvent.Status.Should().Be(WebhookStatus.Failed);
        webhookEvent.ProcessedAt.Should().NotBeNull();
    }
}
