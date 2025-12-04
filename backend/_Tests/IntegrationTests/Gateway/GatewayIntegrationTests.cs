using IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace IntegrationTests.Gateway;

/// <summary>
/// Integration tests for Gateway API routing and health checks
/// These tests require Docker to be running
/// </summary>
[Collection("Infrastructure")]
[Trait("Category", "RequiresDocker")]
public class GatewayInfrastructureTests : IAsyncLifetime
{
    private readonly InfrastructureFixture _fixture;

    public GatewayInfrastructureTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task PostgresFixture_ShouldBeHealthy()
    {
        // Arrange & Act
        await using var connection = await _fixture.Postgres.CreateConnectionAsync();

        // Assert
        connection.State.Should().Be(System.Data.ConnectionState.Open);
    }

    [Fact]
    public async Task RedisFixture_ShouldBeHealthy()
    {
        // Arrange
        var db = _fixture.Redis.GetDatabase();

        // Act
        var pong = await db.PingAsync();

        // Assert
        pong.TotalMilliseconds.Should().BeLessThan(1000);
    }

    [Fact]
    public void RabbitMQFixture_ShouldBeHealthy()
    {
        // Arrange & Act
        using var connection = _fixture.RabbitMQ.CreateConnection();

        // Assert
        connection.IsOpen.Should().BeTrue();
    }

    [Fact]
    public async Task Redis_SetAndGet_ShouldWork()
    {
        // Arrange
        var testKey = "test:gateway:key";
        var testValue = new TestData { Name = "Test", Value = 123 };

        // Act
        await _fixture.Redis.SetAsync(testKey, testValue, TimeSpan.FromMinutes(5));
        var result = await _fixture.Redis.GetAsync<TestData>(testKey);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
        result.Value.Should().Be(123);
    }

    private class TestData
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [Fact]
    public async Task RabbitMQ_PublishAndConsume_ShouldWork()
    {
        // Arrange
        using var connection = _fixture.RabbitMQ.CreateConnection();
        using var channel = connection.CreateModel();

        var testMessage = System.Text.Encoding.UTF8.GetBytes("{\"test\": \"message\"}");

        // Act
        channel.BasicPublish(
            exchange: "error.events",
            routingKey: "error.critical",
            mandatory: false,
            basicProperties: null,
            body: testMessage);

        // Give time for message to be routed
        await Task.Delay(100);

        var result = channel.BasicGet("error.critical.queue", autoAck: true);

        // Assert
        result.Should().NotBeNull();
        System.Text.Encoding.UTF8.GetString(result!.Body.ToArray()).Should().Contain("test");
    }

    [Fact]
    public async Task Postgres_CRUD_ShouldWork()
    {
        // Arrange
        await using var connection = await _fixture.Postgres.CreateConnectionAsync();

        // Act - Create
        await using var insertCmd = new Npgsql.NpgsqlCommand(
            "INSERT INTO test.health_check DEFAULT VALUES RETURNING id", connection);
        var insertedId = await insertCmd.ExecuteScalarAsync();

        // Act - Read
        await using var selectCmd = new Npgsql.NpgsqlCommand(
            $"SELECT COUNT(*) FROM test.health_check WHERE id = {insertedId}", connection);
        var count = await selectCmd.ExecuteScalarAsync();

        // Assert
        insertedId.Should().NotBeNull();
        Convert.ToInt32(count).Should().Be(1);
    }
}
