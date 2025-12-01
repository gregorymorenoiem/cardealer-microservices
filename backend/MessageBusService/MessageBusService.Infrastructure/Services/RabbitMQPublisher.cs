using RabbitMQ.Client;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;
using MessageBusService.Domain.Enums;
using MessageBusService.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace MessageBusService.Infrastructure.Services;

public class RabbitMQPublisher : IMessagePublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly MessageBusDbContext _dbContext;
    private readonly ILogger<RabbitMQPublisher> _logger;

    public RabbitMQPublisher(
        IConnection connection,
        MessageBusDbContext dbContext,
        ILogger<RabbitMQPublisher> logger)
    {
        _connection = connection;
        _channel = _connection.CreateModel();
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> PublishAsync(string topic, string payload, MessagePriority priority = MessagePriority.Normal, Dictionary<string, string>? headers = null)
    {
        try
        {
            // Declarar exchange y queue
            _channel.ExchangeDeclare(exchange: topic, type: ExchangeType.Fanout, durable: true);
            
            var message = new Message
            {
                Id = Guid.NewGuid(),
                Topic = topic,
                Payload = payload,
                Status = MessageStatus.Pending,
                Priority = priority,
                CreatedAt = DateTime.UtcNow,
                Headers = headers,
                CorrelationId = Guid.NewGuid().ToString()
            };

            // Guardar en base de datos
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            // Publicar a RabbitMQ
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.Priority = (byte)priority;
            properties.MessageId = message.Id.ToString();
            properties.CorrelationId = message.CorrelationId;

            _channel.BasicPublish(
                exchange: topic,
                routingKey: "",
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Published message {MessageId} to topic {Topic}", message.Id, topic);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to topic {Topic}", topic);
            return false;
        }
    }

    public async Task<bool> PublishBatchAsync(string topic, List<string> payloads, MessagePriority priority = MessagePriority.Normal)
    {
        try
        {
            var batch = new MessageBatch
            {
                Id = Guid.NewGuid(),
                BatchName = $"{topic}_batch_{DateTime.UtcNow:yyyyMMddHHmmss}",
                CreatedAt = DateTime.UtcNow,
                Status = MessageStatus.Pending,
                TotalMessages = payloads.Count
            };

            _dbContext.MessageBatches.Add(batch);

            foreach (var payload in payloads)
            {
                await PublishAsync(topic, payload, priority);
            }

            batch.Status = MessageStatus.Completed;
            batch.CompletedAt = DateTime.UtcNow;
            batch.ProcessedMessages = payloads.Count;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Published batch {BatchId} with {Count} messages", batch.Id, payloads.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing batch to topic {Topic}", topic);
            return false;
        }
    }

    public async Task<Message?> GetMessageStatusAsync(Guid messageId)
    {
        return await _dbContext.Messages.FindAsync(messageId);
    }
}
