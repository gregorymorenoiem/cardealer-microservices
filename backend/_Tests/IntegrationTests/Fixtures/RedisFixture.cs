using DotNet.Testcontainers.Builders;
using StackExchange.Redis;

namespace IntegrationTests.Fixtures;

/// <summary>
/// Redis test container fixture for integration testing
/// </summary>
public class RedisFixture : IAsyncLifetime
{
    private readonly RedisContainer _container;

    public RedisFixture()
    {
        _container = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
            .Build();
    }

    public string ConnectionString => $"{_container.Hostname}:{_container.GetMappedPublicPort(6379)}";

    public string Host => _container.Hostname;

    public int Port => _container.GetMappedPublicPort(6379);

    private IConnectionMultiplexer? _connection;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Create connection
        _connection = await ConnectionMultiplexer.ConnectAsync(ConnectionString);

        // Verify connection
        var db = _connection.GetDatabase();
        await db.PingAsync();
    }

    public async Task DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        await _container.DisposeAsync();
    }

    public IDatabase GetDatabase()
    {
        return _connection?.GetDatabase()
            ?? throw new InvalidOperationException("Redis connection not initialized");
    }

    public async Task FlushAllAsync()
    {
        if (_connection != null)
        {
            var endpoints = _connection.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _connection.GetServer(endpoint);
                await server.FlushAllDatabasesAsync();
            }
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var db = GetDatabase();
        var value = await db.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return default;

        return System.Text.Json.JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var db = GetDatabase();
        var json = System.Text.Json.JsonSerializer.Serialize(value);
        await db.StringSetAsync(key, json, expiry);
    }
}

/// <summary>
/// Collection definition for sharing Redis container across tests
/// </summary>
[CollectionDefinition("Redis")]
public class RedisCollection : ICollectionFixture<RedisFixture>
{
}
