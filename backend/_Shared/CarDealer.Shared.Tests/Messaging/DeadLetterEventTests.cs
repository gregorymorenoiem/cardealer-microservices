using CarDealer.Shared.Messaging;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Messaging;

/// <summary>
/// Tests for DeadLetterEvent: defaults, exponential backoff scheduling.
/// </summary>
public class DeadLetterEventTests
{
    [Fact]
    public void NewEvent_HasDefaults()
    {
        var evt = new DeadLetterEvent();

        evt.Id.Should().NotBeEmpty();
        evt.ServiceName.Should().BeEmpty();
        evt.EventType.Should().BeEmpty();
        evt.EventJson.Should().BeEmpty();
        evt.FailedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        evt.RetryCount.Should().Be(0);
        evt.MaxRetries.Should().Be(5);
        evt.NextRetryAt.Should().BeNull();
        evt.LastError.Should().BeNull();
    }

    [Fact]
    public void ScheduleNextRetry_FirstRetry_1MinuteDelay()
    {
        var evt = new DeadLetterEvent();

        evt.ScheduleNextRetry();

        evt.RetryCount.Should().Be(1);
        evt.NextRetryAt.Should().NotBeNull();
        // 2^(1-1) = 1 minute
        evt.NextRetryAt!.Value.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(1), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void ScheduleNextRetry_SecondRetry_2MinuteDelay()
    {
        var evt = new DeadLetterEvent();

        evt.ScheduleNextRetry(); // RetryCount = 1
        evt.ScheduleNextRetry(); // RetryCount = 2

        evt.RetryCount.Should().Be(2);
        // 2^(2-1) = 2 minutes
        evt.NextRetryAt!.Value.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(2), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void ScheduleNextRetry_ExponentialBackoff_Sequence()
    {
        var evt = new DeadLetterEvent();
        var expectedMinutes = new[] { 1.0, 2.0, 4.0, 8.0, 16.0 };

        for (int i = 0; i < 5; i++)
        {
            evt.ScheduleNextRetry();
            evt.RetryCount.Should().Be(i + 1);
            evt.NextRetryAt!.Value.Should().BeCloseTo(
                DateTime.UtcNow.AddMinutes(expectedMinutes[i]),
                TimeSpan.FromSeconds(10));
        }
    }

    [Fact]
    public void ScheduleNextRetry_CapsAt16Minutes()
    {
        var evt = new DeadLetterEvent();

        // Run 10 retries — delay should cap at 16 minutes
        for (int i = 0; i < 10; i++)
            evt.ScheduleNextRetry();

        evt.RetryCount.Should().Be(10);
        // 2^(10-1) = 512, but capped at 16
        evt.NextRetryAt!.Value.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(16), TimeSpan.FromSeconds(10));
    }
}
