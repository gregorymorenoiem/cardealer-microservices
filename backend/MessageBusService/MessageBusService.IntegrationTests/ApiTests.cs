using MessageBusService.Api;
using MessageBusService.Application.Commands;
using MessageBusService.Domain.Enums;
using MessageBusService.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client;
using System.Net;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace MessageBusService.IntegrationTests;

public class MessagesControllerApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public MessagesControllerApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PublishMessage_WithValidPayload_ReturnsOk()
    {
        // Arrange
        var command = new PublishMessageCommand
        {
            Topic = "api.test.topic",
            Payload = "API test message",
            Priority = MessagePriority.Normal,
            Headers = new Dictionary<string, string> { { "source", "test" } }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/messages", command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<bool>();
        Assert.True(result);
    }

    [Fact]
    public async Task PublishMessage_WithHighPriority_ProcessesSuccessfully()
    {
        // Arrange
        var command = new PublishMessageCommand
        {
            Topic = "priority.api.topic",
            Payload = "High priority API message",
            Priority = MessagePriority.Critical
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/messages", command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<bool>();
        Assert.True(result);
    }

    [Fact]
    public async Task PublishMessage_WithMultipleMessages_AllReturnOk()
    {
        // Arrange & Act - Publish multiple messages
        var response1 = await _client.PostAsJsonAsync("/api/messages", new PublishMessageCommand
        {
            Topic = "batch.api", 
            Payload = "Message 1", 
            Priority = MessagePriority.Normal
        });

        var response2 = await _client.PostAsJsonAsync("/api/messages", new PublishMessageCommand
        {
            Topic = "batch.api", 
            Payload = "Message 2", 
            Priority = MessagePriority.Normal
        });

        var response3 = await _client.PostAsJsonAsync("/api/messages", new PublishMessageCommand
        {
            Topic = "batch.api", 
            Payload = "Message 3", 
            Priority = MessagePriority.Normal
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
    }

    [Fact]
    public async Task GetMessageStatus_WithExistingMessage_ReturnsMessage()
    {
        // Arrange - First publish a message
        var publishCommand = new PublishMessageCommand
        {
            Topic = "status.api.topic",
            Payload = "Status check message",
            Priority = MessagePriority.Normal
        };

        await _client.PostAsJsonAsync("/api/messages", publishCommand);

        // Get the message ID from database (in real scenario, would be returned from publish)
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MessageBusDbContext>();
        var message = await dbContext.Messages.FirstOrDefaultAsync(m => m.Topic == "status.api.topic");
        Assert.NotNull(message);

        // Act
        var response = await _client.GetAsync($"/api/messages/{message.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMessageStatus_WithNonExistentMessage_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/messages/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

public class SubscriptionsControllerApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public SubscriptionsControllerApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Subscribe_WithValidTopic_ReturnsSubscriptionId()
    {
        // Arrange
        var command = new SubscribeToTopicCommand
        {
            Topic = "subscription.api.topic",
            ConsumerName = "api.consumer"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/subscriptions", command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var subscriptionId = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, subscriptionId);
    }

    [Fact]
    public async Task Unsubscribe_WithExistingSubscription_ReturnsNoContent()
    {
        // Arrange - First create a subscription
        var subscribeCommand = new SubscribeToTopicCommand
        {
            Topic = "unsub.api.topic",
            ConsumerName = "unsub.consumer"
        };

        var subscribeResponse = await _client.PostAsJsonAsync("/api/subscriptions", subscribeCommand);
        var subscriptionId = await subscribeResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var response = await _client.DeleteAsync($"/api/subscriptions/{subscriptionId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetSubscriptions_WithTopic_ReturnsFilteredList()
    {
        // Arrange - Create multiple subscriptions
        await _client.PostAsJsonAsync("/api/subscriptions", new SubscribeToTopicCommand
        {
            Topic = "filter.topic",
            ConsumerName = "consumer1"
        });

        await _client.PostAsJsonAsync("/api/subscriptions", new SubscribeToTopicCommand
        {
            Topic = "filter.topic",
            ConsumerName = "consumer2"
        });

        await _client.PostAsJsonAsync("/api/subscriptions", new SubscribeToTopicCommand
        {
            Topic = "other.topic",
            ConsumerName = "consumer3"
        });

        // Act
        var response = await _client.GetAsync("/api/subscriptions?topic=filter.topic");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public class DeadLetterControllerApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public DeadLetterControllerApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDeadLetters_WithPagination_ReturnsPagedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/deadletter?pageNumber=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Retry_WithNonExistentMessage_ReturnsNotFound()
    {
        // Act
        var response = await _client.PostAsync($"/api/deadletter/{Guid.NewGuid()}/retry", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Discard_WithNonExistentMessage_ReturnsNoContent()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/deadletter/{Guid.NewGuid()}");

        // Assert
        // Should still return NoContent even if message doesn't exist (idempotent)
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithNonExistentMessage_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/deadletter/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly RabbitMqContainer _rabbitMqContainer;
    private readonly PostgreSqlContainer _postgresContainer;

    public CustomWebApplicationFactory()
    {
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.12-management")
            .Build();

        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("messagebus_api_test")
            .WithUsername("test")
            .WithPassword("test123")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext
            services.RemoveAll(typeof(DbContextOptions<MessageBusDbContext>));
            services.RemoveAll(typeof(MessageBusDbContext));

            // Add test DbContext with container connection string
            services.AddDbContext<MessageBusDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });

            // Remove existing RabbitMQ connection
            services.RemoveAll(typeof(IConnection));

            // Add test RabbitMQ connection
            services.AddSingleton<IConnection>(sp =>
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_rabbitMqContainer.GetConnectionString())
                };
                return factory.CreateConnection();
            });

            // Ensure database is created
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MessageBusDbContext>();
            dbContext.Database.EnsureCreated();
        });
    }

    public async Task InitializeAsync()
    {
        await _rabbitMqContainer.StartAsync();
        await _postgresContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _rabbitMqContainer.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
}
