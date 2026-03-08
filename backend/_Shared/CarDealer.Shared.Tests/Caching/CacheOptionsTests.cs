using CarDealer.Shared.Caching.Models;
using FluentAssertions;

namespace CarDealer.Shared.Tests.Caching;

public class CacheOptionsTests
{
    [Fact]
    public void SectionName_ShouldBe_Caching()
    {
        CacheOptions.SectionName.Should().Be("Caching");
    }

    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var options = new CacheOptions();

        options.RedisConnectionString.Should().BeEmpty();
        options.InstanceName.Should().Be("okla:");
        options.DefaultTtlSeconds.Should().Be(300);
        options.MaxTtlSeconds.Should().Be(86400);
        options.UseSlidingExpiration.Should().BeFalse();
        options.EnableMetrics.Should().BeTrue();
        options.FallbackToMemory.Should().BeTrue();
    }

    [Fact]
    public void DefaultTtlSeconds_ShouldBe5Minutes()
    {
        var options = new CacheOptions();
        options.DefaultTtlSeconds.Should().Be(300); // 5 minutes
    }

    [Fact]
    public void MaxTtlSeconds_ShouldBe24Hours()
    {
        var options = new CacheOptions();
        options.MaxTtlSeconds.Should().Be(86400); // 24 hours
    }

    [Fact]
    public void CanOverride_AllProperties()
    {
        var options = new CacheOptions
        {
            RedisConnectionString = "localhost:6379",
            InstanceName = "test:",
            DefaultTtlSeconds = 600,
            MaxTtlSeconds = 3600,
            UseSlidingExpiration = true,
            EnableMetrics = false,
            FallbackToMemory = false
        };

        options.RedisConnectionString.Should().Be("localhost:6379");
        options.InstanceName.Should().Be("test:");
        options.DefaultTtlSeconds.Should().Be(600);
        options.MaxTtlSeconds.Should().Be(3600);
        options.UseSlidingExpiration.Should().BeTrue();
        options.EnableMetrics.Should().BeFalse();
        options.FallbackToMemory.Should().BeFalse();
    }
}
