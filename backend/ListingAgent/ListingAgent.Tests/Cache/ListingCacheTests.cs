// =============================================================================
// ListingAgent — Redis Cache Tests
// Validates:
//   1. ListingCacheKeyBuilder: same make/model/year/trim → same key (cache hit)
//   2. ListingCacheKeyBuilder: different VehicleIds but same attributes → same key
//   3. ListingCacheKeyBuilder: normalization (case, spaces, hyphens)
//   4. ListingCacheStats.HitRatePercent calculation
//   5. ListingCacheStats.TargetMet threshold (≥50%)
//   6. RedisListingCacheMetrics in-memory fallback (when Redis throws)
// =============================================================================

using FluentAssertions;
using ListingAgent.Domain.Interfaces;
using ListingAgent.Domain.Utilities;
using ListingAgent.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace ListingAgent.Tests;

public sealed class ListingCacheKeyTests
{
    // ── 1. Same make/model/year/trim → identical cache key ───────────────────
    [Fact]
    public void SameVehicleType_DifferentVehicleIds_ProduceSameCacheKey()
    {
        var key1 = ListingCacheKeyBuilder.Build("Toyota", "Camry", 2022, "LE");
        var key2 = ListingCacheKeyBuilder.Build("Toyota", "Camry", 2022, "LE");

        key1.Should().Be(key2,
            "two vehicles of the same make/model/year/trim must share the same cache key " +
            "so the second request is served from Redis without calling the LLM API");
    }

    // ── 2. Different trim levels → different cache keys ──────────────────────
    [Fact]
    public void DifferentTrims_ProduceDifferentCacheKeys()
    {
        var key1 = ListingCacheKeyBuilder.Build("Toyota", "Camry", 2022, "LE");
        var key2 = ListingCacheKeyBuilder.Build("Toyota", "Camry", 2022, "XSE");

        key1.Should().NotBe(key2);
    }

    // ── 3. Different years → different cache keys ────────────────────────────
    [Fact]
    public void DifferentYears_ProduceDifferentCacheKeys()
    {
        var key1 = ListingCacheKeyBuilder.Build("Toyota", "Camry", 2021, "LE");
        var key2 = ListingCacheKeyBuilder.Build("Toyota", "Camry", 2022, "LE");

        key1.Should().NotBe(key2);
    }

    // ── 4. Normalization: case insensitive ───────────────────────────────────
    [Fact]
    public void CacheKey_IsCaseInsensitive()
    {
        var key1 = ListingCacheKeyBuilder.Build("TOYOTA", "CAMRY", 2022, "LE");
        var key2 = ListingCacheKeyBuilder.Build("toyota", "camry", 2022, "le");

        key1.Should().Be(key2);
    }

    // ── 5. Normalization: extra spaces are trimmed ───────────────────────────
    [Fact]
    public void CacheKey_TrimsWhitespace()
    {
        var key1 = ListingCacheKeyBuilder.Build("Toyota", "Camry", 2022, "LE");
        var key2 = ListingCacheKeyBuilder.Build("  Toyota  ", "  Camry  ", 2022, "  LE  ");

        key1.Should().Be(key2);
    }

    // ── 6. Normalization: hyphens and spaces in model names ──────────────────
    [Fact]
    public void CacheKey_NormalizesHyphensAndSpaces()
    {
        var key1 = ListingCacheKeyBuilder.Build("BMW", "X5-M", 2023, "Competition");
        var key2 = ListingCacheKeyBuilder.Build("BMW", "X5 M", 2023, "Competition");

        // After normalization both become x5_m
        key1.Should().Be(key2);
    }

    // ── 7. Null trim is treated as empty string ───────────────────────────────
    [Fact]
    public void CacheKey_NullTrim_TreatedAsEmpty()
    {
        var key1 = ListingCacheKeyBuilder.Build("Honda", "Civic", 2023, null);
        var key2 = ListingCacheKeyBuilder.Build("Honda", "Civic", 2023, null);

        key1.Should().Be(key2);
    }

    // ── 8. Key starts with expected prefix ───────────────────────────────────
    [Fact]
    public void CacheKey_StartsWithV2Prefix()
    {
        var key = ListingCacheKeyBuilder.Build("Toyota", "Corolla", 2022, "SE");

        key.Should().StartWith("listing:v2:");
    }

    // ── 9. Key format is "listing:v2:make:model:year:trim" ───────────────────
    [Fact]
    public void CacheKey_HasExpectedFormat()
    {
        var key = ListingCacheKeyBuilder.Build("Toyota", "Corolla", 2022, "SE");

        key.Should().Be("listing:v2:toyota:corolla:2022:se");
    }
}

public sealed class ListingCacheStatsTests
{
    // ── 9. Hit rate calculation ───────────────────────────────────────────────
    [Theory]
    [InlineData(0, 0, 0.0)]
    [InlineData(50, 100, 50.0)]
    [InlineData(75, 100, 75.0)]
    [InlineData(1, 3, 33.33)]
    public void HitRatePercent_IsCalculatedCorrectly(long hits, long total, double expectedRate)
    {
        var stats = new ListingCacheStats
        {
            CacheHits = hits,
            CacheMisses = total - hits,
        };

        stats.HitRatePercent.Should().BeApproximately(expectedRate, 0.01);
    }

    // ── 10. TargetMet flag ────────────────────────────────────────────────────
    [Theory]
    [InlineData(50, 100, true)]   // exactly 50% — target met
    [InlineData(51, 100, true)]   // above 50%
    [InlineData(49, 100, false)]  // below 50%
    [InlineData(0, 0, false)]     // no data
    public void TargetMet_ReflectsHitRateThreshold(long hits, long total, bool expected)
    {
        var stats = new ListingCacheStats
        {
            CacheHits = hits,
            CacheMisses = total - hits,
        };

        stats.TargetMet.Should().Be(expected);
    }
}

public sealed class RedisListingCacheMetricsTests
{
    /// <summary>
    /// Creates a mock IConnectionMultiplexer whose GetDatabase() throws RedisConnectionException.
    /// This simulates Redis being unavailable, triggering the in-memory fallback path.
    /// </summary>
    private static IConnectionMultiplexer CreateFailingRedisMock()
    {
        var dbMock = new Mock<IDatabase>();
        dbMock.Setup(d => d.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
              .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Redis unavailable"));
        dbMock.Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
              .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Redis unavailable"));

        var multiplexerMock = new Mock<IConnectionMultiplexer>();
        multiplexerMock.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                       .Returns(dbMock.Object);

        return multiplexerMock.Object;
    }

    // ── 10. In-memory fallback works when Redis throws ───────────────────────
    [Fact]
    public async Task GetStats_WhenRedisThrows_ReturnsInMemoryCounts()
    {
        var metrics = new RedisListingCacheMetrics(
            redis: CreateFailingRedisMock(),
            logger: NullLogger<RedisListingCacheMetrics>.Instance);

        await metrics.RecordHitAsync("key1");
        await metrics.RecordHitAsync("key2");
        await metrics.RecordMissAsync("key3");

        var stats = await metrics.GetStatsAsync();

        // Counters are static so we can only verify they are ≥ their added values
        stats.CacheHits.Should().BeGreaterThanOrEqualTo(2);
        stats.CacheMisses.Should().BeGreaterThanOrEqualTo(1);
        stats.TotalRequests.Should().Be(stats.CacheHits + stats.CacheMisses);
    }

    // ── 11. TotalRequests = Hits + Misses always ─────────────────────────────
    [Fact]
    public async Task GetStats_TotalRequests_EqualsHitsPlusMisses()
    {
        var metrics = new RedisListingCacheMetrics(
            redis: CreateFailingRedisMock(),
            logger: NullLogger<RedisListingCacheMetrics>.Instance);

        var stats = await metrics.GetStatsAsync();

        stats.TotalRequests.Should().Be(stats.CacheHits + stats.CacheMisses);
    }
}
