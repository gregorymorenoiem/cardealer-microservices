using RabbitMQ.Client;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;
using MessageBusService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MessageBusService.Infrastructure.Services;

public class RabbitMQSubscriber : IMessageSubscriber
{
    private readonly IConnection _connection;
    private readonly MessageBusDbContext _dbContext;
    private readonly ILogger<RabbitMQSubscriber> _logger;

    public RabbitMQSubscriber(
        IConnection connection,
        MessageBusDbContext dbContext,
        ILogger<RabbitMQSubscriber> logger)
    {
        _connection = connection;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Subscription> SubscribeAsync(string topic, string consumerName)
    {
        try
        {
            using var channel = _connection.CreateModel();
            
            // Declarar exchange
            channel.ExchangeDeclare(exchange: topic, type: ExchangeType.Fanout, durable: true);
            
            // Crear queue única para el consumidor
            var queueName = $"{topic}.{consumerName}";
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            
            // Bind queue al exchange
            channel.QueueBind(queue: queueName, exchange: topic, routingKey: "");

            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                Topic = topic,
                ConsumerName = consumerName,
                QueueName = queueName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow
            };

            _dbContext.Subscriptions.Add(subscription);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created subscription {SubscriptionId} for topic {Topic}", subscription.Id, topic);
            return subscription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription for topic {Topic}", topic);
            throw;
        }
    }

    public async Task<bool> UnsubscribeAsync(Guid subscriptionId)
    {
        try
        {
            var subscription = await _dbContext.Subscriptions.FindAsync(subscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("Subscription {SubscriptionId} not found", subscriptionId);
                return false;
            }

            using var channel = _connection.CreateModel();
            
            // Eliminar queue de RabbitMQ
            channel.QueueDelete(subscription.QueueName);

            subscription.IsActive = false;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Unsubscribed {SubscriptionId}", subscriptionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<List<Subscription>> GetSubscriptionsAsync(string? topic = null)
    {
        var query = _dbContext.Subscriptions.AsQueryable();

        if (!string.IsNullOrEmpty(topic))
        {
            query = query.Where(s => s.Topic == topic);
        }

        return await query.Where(s => s.IsActive).ToListAsync();
    }
}
