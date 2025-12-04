using DotNet.Testcontainers.Builders;
using RabbitMQ.Client;

namespace IntegrationTests.Fixtures;

/// <summary>
/// RabbitMQ test container fixture for integration testing
/// </summary>
public class RabbitMQFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _container;

    public RabbitMQFixture()
    {
        _container = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management-alpine")
            .WithUsername("guest")
            .WithPassword("guest")
            .WithPortBinding(5673, 5672)
            .WithPortBinding(15673, 15672)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .Build();
    }

    public string ConnectionString => _container.GetConnectionString();

    public string Host => _container.Hostname;

    public int Port => _container.GetMappedPublicPort(5672);

    public int ManagementPort => _container.GetMappedPublicPort(15672);

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Wait a bit for RabbitMQ to fully initialize
        await Task.Delay(2000);

        // Configure exchanges and queues
        await ConfigureExchangesAndQueuesAsync();
    }

    private async Task ConfigureExchangesAndQueuesAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = Host,
            Port = Port,
            UserName = "guest",
            Password = "guest"
        };

        await Task.Run(() =>
        {
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Declare exchanges
            var exchanges = new[]
            {
                "error.events",
                "auth.events",
                "notification.events",
                "audit.events",
                "backup.events"
            };

            foreach (var exchange in exchanges)
            {
                channel.ExchangeDeclare(
                    exchange: exchange,
                    type: "topic",
                    durable: true,
                    autoDelete: false);
            }

            // Declare queues
            var queues = new[]
            {
                ("error.critical.queue", "error.events", "error.critical"),
                ("error.warning.queue", "error.events", "error.warning"),
                ("auth.user.registered.queue", "auth.events", "auth.user.registered"),
                ("notification.email.queue", "notification.events", "notification.email.*"),
                ("notification.teams.queue", "notification.events", "notification.teams.*"),
                ("audit.all.queue", "audit.events", "#")
            };

            foreach (var (queueName, exchange, routingKey) in queues)
            {
                channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                channel.QueueBind(
                    queue: queueName,
                    exchange: exchange,
                    routingKey: routingKey);
            }

            // Declare Dead Letter Queue
            channel.ExchangeDeclare("dlx.events", "topic", durable: true);
            channel.QueueDeclare("dead.letter.queue", durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind("dead.letter.queue", "dlx.events", "#");
        });
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = Host,
            Port = Port,
            UserName = "guest",
            Password = "guest"
        };

        return factory.CreateConnection();
    }

    public async Task PurgeQueuesAsync()
    {
        await Task.Run(() =>
        {
            using var connection = CreateConnection();
            using var channel = connection.CreateModel();

            var queues = new[]
            {
                "error.critical.queue",
                "error.warning.queue",
                "auth.user.registered.queue",
                "notification.email.queue",
                "notification.teams.queue",
                "audit.all.queue",
                "dead.letter.queue"
            };

            foreach (var queue in queues)
            {
                try
                {
                    channel.QueuePurge(queue);
                }
                catch
                {
                    // Queue might not exist, ignore
                }
            }
        });
    }
}

/// <summary>
/// Collection definition for sharing RabbitMQ container across tests
/// </summary>
[CollectionDefinition("RabbitMQ")]
public class RabbitMQCollection : ICollectionFixture<RabbitMQFixture>
{
}
