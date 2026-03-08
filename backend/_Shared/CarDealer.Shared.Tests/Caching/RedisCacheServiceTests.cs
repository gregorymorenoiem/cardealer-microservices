using CarDealer.Shared.Caching.Models;
using CarDealer.Shared.Caching.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CarDealer.Shared.Tests.Caching;

/// <summary>
/// Tests for RedisCacheService using the in-memory distributed cache backend.
/// No real Redis needed — validates the serialization, TTL, and fail-safe logic.
/// </summary>
public class RedisCacheServiceTests
{
    private readonly RedisCacheService _sut;
    private readonly IDistributedCache _cache;

    public RedisCacheServiceTests()
    {
        // Use in-memory distributed cache as backend (same interface as Redis)
        _cache = new MemoryDistributedCache(
            Options.Create(new MemoryDistributedCacheOptions()));

        var options = Options.Create(new CacheOptions
        {
            DefaultTtlSeconds = 60,
            MaxTtlSeconds = 3600,
            EnableMetrics = true,
            InstanceName = "test:"
        });

        _sut = new RedisCacheService(
            _cache,
            options,
            NullLogger<RedisCacheService>.Instance,
            redis: null); // no IConnectionMultiplexer for unit tests
    }

    // -- Test DTO --
    private record TestVehicle(Guid Id, string Title, decimal Price, int Year);

    #region GetAsync / SetAsync

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ReturnsNull()
    {
        var result = await _sut.GetAsync<TestVehicle>("nonexistent");
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ThenGetAsync_ReturnsDeserializedObject()
    {
        var vehicle = new TestVehicle(Guid.NewGuid(), "Toyota Corolla", 25000m, 2023);

        await _sut.SetAsync("vehicle:1", vehicle);
        var result = await _sut.GetAsync<TestVehicle>("vehicle:1");

        result.Should().NotBeNull();
        result!.Title.Should().Be("Toyota Corolla");
        result.Price.Should().Be(25000m);
        result.Year.Should().Be(2023);
    }

    [Fact]
    public async Task SetAsync_WithCustomTtl_StoresValue()
    {
        var vehicle = new TestVehicle(Guid.NewGuid(), "Honda Civic", 22000m, 2022);

        await _sut.SetAsync("vehicle:2", vehicle, ttlSeconds: 120);
        var result = await _sut.GetAsync<TestVehicle>("vehicle:2");

        result.Should().NotBeNull();
        result!.Title.Should().Be("Honda Civic");
    }

    [Fact]
    public async Task SetAsync_Overwrites_ExistingKey()
    {
        var v1 = new TestVehicle(Guid.NewGuid(), "Old", 10000m, 2020);
        var v2 = new TestVehicle(Guid.NewGuid(), "New", 20000m, 2024);

        await _sut.SetAsync("vehicle:overwrite", v1);
        await _sut.SetAsync("vehicle:overwrite", v2);

        var result = await _sut.GetAsync<TestVehicle>("vehicle:overwrite");
        result!.Title.Should().Be("New");
        result.Price.Should().Be(20000m);
    }

    #endregion

    #region RemoveAsync

    [Fact]
    public async Task RemoveAsync_DeletesExistingKey()
    {
        var vehicle = new TestVehicle(Guid.NewGuid(), "To Remove", 15000m, 2021);
        await _sut.SetAsync("vehicle:remove", vehicle);

        await _sut.RemoveAsync("vehicle:remove");

        var result = await _sut.GetAsync<TestVehicle>("vehicle:remove");
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_NonexistentKey_DoesNotThrow()
    {
        var act = () => _sut.RemoveAsync("nonexistent:key");
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region ExistsAsync

    [Fact]
    public async Task ExistsAsync_WhenKeyExists_ReturnsTrue()
    {
        var vehicle = new TestVehicle(Guid.NewGuid(), "Exists", 18000m, 2023);
        await _sut.SetAsync("vehicle:exists", vehicle);

        var exists = await _sut.ExistsAsync("vehicle:exists");
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyDoesNotExist_ReturnsFalse()
    {
        var exists = await _sut.ExistsAsync("vehicle:missing");
        exists.Should().BeFalse();
    }

    #endregion

    #region GetOrSetAsync

    [Fact]
    public async Task GetOrSetAsync_WhenCacheMiss_CallsFactory_AndCachesResult()
    {
        var factoryCallCount = 0;
        var vehicle = new TestVehicle(Guid.NewGuid(), "Factory Created", 30000m, 2024);

        var result = await _sut.GetOrSetAsync("vehicle:factory", async () =>
        {
            factoryCallCount++;
            return vehicle;
        }, ttlSeconds: 300);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Factory Created");
        factoryCallCount.Should().Be(1);

        // Second call should hit cache, NOT call factory
        var result2 = await _sut.GetOrSetAsync("vehicle:factory", async () =>
        {
            factoryCallCount++;
            return new TestVehicle(Guid.NewGuid(), "Should Not Appear", 0m, 0);
        });

        result2!.Title.Should().Be("Factory Created");
        factoryCallCount.Should().Be(1); // factory NOT called again
    }

    [Fact]
    public async Task GetOrSetAsync_WhenCacheHit_ReturnsWithoutCallingFactory()
    {
        var vehicle = new TestVehicle(Guid.NewGuid(), "Cached", 20000m, 2023);
        await _sut.SetAsync("vehicle:hit", vehicle);

        var factoryCalled = false;
        var result = await _sut.GetOrSetAsync("vehicle:hit", async () =>
        {
            factoryCalled = true;
            return new TestVehicle(Guid.NewGuid(), "Not This One", 0m, 0);
        });

        result!.Title.Should().Be("Cached");
        factoryCalled.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrSetAsync_WhenFactoryThrows_ReturnsNull()
    {
        var result = await _sut.GetOrSetAsync<TestVehicle>("vehicle:fail", () =>
        {
            throw new InvalidOperationException("DB down");
        });

        result.Should().BeNull();
    }

    #endregion

    #region TTL enforcement (MaxTtl guard)

    [Fact]
    public async Task SetAsync_WhenTtlExceedsMax_ClampsToMaxTtl()
    {
        // MaxTtlSeconds=3600 in our setup, request 999999
        var vehicle = new TestVehicle(Guid.NewGuid(), "Long TTL", 10000m, 2020);

        // Should not throw — TTL is clamped internally
        var act = () => _sut.SetAsync("vehicle:longttl", vehicle, ttlSeconds: 999999);
        await act.Should().NotThrowAsync();

        var result = await _sut.GetAsync<TestVehicle>("vehicle:longttl");
        result.Should().NotBeNull();
    }

    #endregion

    #region InvalidateByPatternAsync

    [Fact]
    public async Task InvalidateByPatternAsync_WithoutRedis_IsNoop()
    {
        // SUT has redis=null, so pattern invalidation should be a no-op
        var act = () => _sut.InvalidateByPatternAsync("vehicle:*");
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Serialization edge cases

    [Fact]
    public async Task SetAsync_WithComplexObject_SerializesAndDeserializesCorrectly()
    {
        var vehicle = new TestVehicle(
            Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            "Hyundai Tucson GLS 4x4",
            35500.99m,
            2024);

        await _sut.SetAsync("vehicle:complex", vehicle);
        var result = await _sut.GetAsync<TestVehicle>("vehicle:complex");

        result.Should().NotBeNull();
        result!.Id.Should().Be(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));
        result.Title.Should().Be("Hyundai Tucson GLS 4x4");
        result.Price.Should().Be(35500.99m);
    }

    [Fact]
    public async Task SetAsync_ThenGetAsync_PreservesGuidValues()
    {
        var id = Guid.NewGuid();
        var vehicle = new TestVehicle(id, "GUID Test", 5000m, 2019);

        await _sut.SetAsync("vehicle:guid", vehicle);
        var result = await _sut.GetAsync<TestVehicle>("vehicle:guid");

        result!.Id.Should().Be(id);
    }

    #endregion
}
