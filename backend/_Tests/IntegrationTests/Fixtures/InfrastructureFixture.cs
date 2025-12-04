namespace IntegrationTests.Fixtures;

/// <summary>
/// Combined fixture for tests requiring multiple infrastructure services
/// </summary>
public class InfrastructureFixture : IAsyncLifetime
{
    public PostgresFixture Postgres { get; } = new();
    public RabbitMQFixture RabbitMQ { get; } = new();
    public RedisFixture Redis { get; } = new();

    public async Task InitializeAsync()
    {
        // Start all containers in parallel
        await Task.WhenAll(
            Postgres.InitializeAsync(),
            RabbitMQ.InitializeAsync(),
            Redis.InitializeAsync()
        );
    }

    public async Task DisposeAsync()
    {
        // Dispose all containers sequentially to avoid issues
        await Postgres.DisposeAsync();
        await RabbitMQ.DisposeAsync();
        await Redis.DisposeAsync();
    }

    public async Task ResetAllAsync()
    {
        await Task.WhenAll(
            Postgres.ResetDatabaseAsync(),
            RabbitMQ.PurgeQueuesAsync(),
            Redis.FlushAllAsync()
        );
    }
}

/// <summary>
/// Collection definition for sharing all infrastructure containers across tests
/// </summary>
[CollectionDefinition("Infrastructure")]
public class InfrastructureCollection : ICollectionFixture<InfrastructureFixture>
{
}
