using DotNet.Testcontainers.Builders;
using Npgsql;

namespace IntegrationTests.Fixtures;

/// <summary>
/// PostgreSQL test container fixture for integration testing
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public PostgresFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("cardealer_test")
            .WithUsername("test")
            .WithPassword("test123")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
    }

    public string ConnectionString => _container.GetConnectionString();

    public string Host => _container.Hostname;

    public int Port => _container.GetMappedPublicPort(5432);

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Verify connection
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        // Create test schema if needed
        await using var cmd = new NpgsqlCommand(@"
            CREATE SCHEMA IF NOT EXISTS test;
            CREATE TABLE IF NOT EXISTS test.health_check (
                id SERIAL PRIMARY KEY,
                checked_at TIMESTAMP DEFAULT NOW()
            );
            INSERT INTO test.health_check DEFAULT VALUES;
        ", connection);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public async Task<NpgsqlConnection> CreateConnectionAsync()
    {
        var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        // Truncate all tables for clean state
        await using var cmd = new NpgsqlCommand(@"
            DO $$ 
            DECLARE 
                r RECORD;
            BEGIN
                FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') 
                LOOP
                    EXECUTE 'TRUNCATE TABLE ' || quote_ident(r.tablename) || ' CASCADE';
                END LOOP;
            END $$;
        ", connection);

        await cmd.ExecuteNonQueryAsync();
    }
}

/// <summary>
/// Collection definition for sharing PostgreSQL container across tests
/// </summary>
[CollectionDefinition("PostgreSQL")]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
}
