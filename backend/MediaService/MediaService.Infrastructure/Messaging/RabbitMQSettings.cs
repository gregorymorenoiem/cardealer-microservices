using Microsoft.Extensions.Configuration;

namespace MediaService.Infrastructure.Messaging;

public class RabbitMQSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";

    // Exchanges
    public string MediaEventsExchange { get; set; } = "media.events";
    public string MediaCommandsExchange { get; set; } = "media.commands";

    // Queues
    public string MediaUploadedQueue { get; set; } = "media.uploaded.queue";
    public string MediaProcessedQueue { get; set; } = "media.processed.queue";
    public string MediaDeletedQueue { get; set; } = "media.deleted.queue";
    public string ProcessMediaQueue { get; set; } = "process.media.queue";

    // Routing Keys
    public string MediaUploadedRoutingKey { get; set; } = "media.uploaded";
    public string MediaProcessedRoutingKey { get; set; } = "media.processed";
    public string MediaDeletedRoutingKey { get; set; } = "media.deleted";
    public string ProcessMediaRoutingKey { get; set; } = "media.process";

    public static RabbitMQSettings FromConfiguration(IConfiguration configuration)
    {
        return new RabbitMQSettings
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:UserName"] ?? throw new InvalidOperationException("RabbitMQ:UserName is not configured"),
            Password = configuration["RabbitMQ:Password"] ?? throw new InvalidOperationException("RabbitMQ:Password is not configured"),
            VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/",
            MediaEventsExchange = configuration["RabbitMQ:MediaEventsExchange"] ?? "media.events",
            MediaCommandsExchange = configuration["RabbitMQ:MediaCommandsExchange"] ?? "media.commands",
            MediaUploadedQueue = configuration["RabbitMQ:MediaUploadedQueue"] ?? "media.uploaded.queue",
            MediaProcessedQueue = configuration["RabbitMQ:MediaProcessedQueue"] ?? "media.processed.queue",
            MediaDeletedQueue = configuration["RabbitMQ:MediaDeletedQueue"] ?? "media.deleted.queue",
            ProcessMediaQueue = configuration["RabbitMQ:ProcessMediaQueue"] ?? "process.media.queue"
        };
    }
}
