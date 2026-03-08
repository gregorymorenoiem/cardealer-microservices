using CarDealer.Shared.Idempotency.Models;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Idempotency;

public class IdempotencyOptionsTests
{
    [Fact]
    public void SectionName_ShouldBeIdempotency()
    {
        IdempotencyOptions.SectionName.Should().Be("Idempotency");
    }

    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var options = new IdempotencyOptions();

        options.Enabled.Should().BeTrue();
        options.RedisConnection.Should().Be("localhost:6379");
        options.DefaultTtlSeconds.Should().Be(86400); // 24 hours
        options.HeaderName.Should().Be("X-Idempotency-Key");
        options.RequireIdempotencyKey.Should().BeTrue();
        options.KeyPrefix.Should().Be("idempotency");
        options.ValidateRequestHash.Should().BeTrue();
        options.ProcessingTimeoutSeconds.Should().Be(60);
    }

    [Fact]
    public void ExcludedPaths_ShouldContainDefaults()
    {
        var options = new IdempotencyOptions();

        options.ExcludedPaths.Should().Contain("/health");
        options.ExcludedPaths.Should().Contain("/swagger");
        options.ExcludedPaths.Should().Contain("/metrics");
    }

    [Fact]
    public void IdempotentMethods_ShouldContainMutatingMethods()
    {
        var options = new IdempotencyOptions();

        options.IdempotentMethods.Should().Contain("POST");
        options.IdempotentMethods.Should().Contain("PUT");
        options.IdempotentMethods.Should().Contain("PATCH");
        options.IdempotentMethods.Should().NotContain("GET");
        options.IdempotentMethods.Should().NotContain("DELETE");
    }

    [Fact]
    public void AllProperties_ShouldBeSettable()
    {
        var options = new IdempotencyOptions
        {
            Enabled = false,
            RedisConnection = "redis-prod:6380",
            DefaultTtlSeconds = 3600,
            HeaderName = "X-Custom-Key",
            RequireIdempotencyKey = false,
            KeyPrefix = "custom",
            ValidateRequestHash = false,
            ProcessingTimeoutSeconds = 120
        };

        options.Enabled.Should().BeFalse();
        options.RedisConnection.Should().Be("redis-prod:6380");
        options.DefaultTtlSeconds.Should().Be(3600);
        options.HeaderName.Should().Be("X-Custom-Key");
        options.RequireIdempotencyKey.Should().BeFalse();
        options.KeyPrefix.Should().Be("custom");
        options.ValidateRequestHash.Should().BeFalse();
        options.ProcessingTimeoutSeconds.Should().Be(120);
    }
}
