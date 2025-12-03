using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;
using RabbitMQ.Client;

namespace MessageBusService.Infrastructure.Services;

/// <summary>
/// Saga step executor that publishes messages to RabbitMQ
/// </summary>
public class RabbitMQSagaStepExecutor : ISagaStepExecutor
{
    private readonly ILogger<RabbitMQSagaStepExecutor> _logger;
    private readonly IConnection _connection;

    public RabbitMQSagaStepExecutor(
        IConnection connection,
        ILogger<RabbitMQSagaStepExecutor> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public bool CanHandle(string actionType)
    {
        return actionType.StartsWith("rabbitmq.", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string?> ExecuteAsync(SagaStep step, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing RabbitMQ action {ActionType} for step {StepId}",
            step.ActionType, step.Id);

        try
        {
            using var channel = _connection.CreateModel();

            // Parse action type: rabbitmq.publish.{exchange}.{routingKey}
            var parts = step.ActionType.Split('.');
            if (parts.Length < 4 || parts[0] != "rabbitmq" || parts[1] != "publish")
            {
                throw new InvalidOperationException($"Invalid RabbitMQ action type: {step.ActionType}");
            }

            var exchange = parts[2];
            var routingKey = parts[3];

            // Declare exchange if needed
            if (exchange != "")
            {
                channel.ExchangeDeclare(
                    exchange: exchange,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false);
            }

            // Prepare message
            var messageBytes = Encoding.UTF8.GetBytes(step.ActionPayload);
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.CorrelationId = step.Saga?.CorrelationId ?? Guid.NewGuid().ToString();
            properties.MessageId = step.Id.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            // Add saga metadata to headers
            properties.Headers = new Dictionary<string, object>
            {
                ["saga-id"] = step.SagaId.ToString(),
                ["saga-step-id"] = step.Id.ToString(),
                ["saga-step-name"] = step.Name,
                ["saga-step-order"] = step.Order
            };

            // Publish message
            channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: messageBytes);

            _logger.LogInformation("Published message to exchange {Exchange} with routing key {RoutingKey}",
                exchange, routingKey);

            // Return confirmation
            var response = new
            {
                Published = true,
                Exchange = exchange,
                RoutingKey = routingKey,
                MessageId = properties.MessageId,
                Timestamp = DateTime.UtcNow
            };

            return await Task.FromResult(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing RabbitMQ step {StepId}", step.Id);
            throw;
        }
    }

    public async Task CompensateAsync(SagaStep step, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Compensating RabbitMQ action {CompensationType} for step {StepId}",
            step.CompensationActionType, step.Id);

        try
        {
            if (string.IsNullOrEmpty(step.CompensationActionType) ||
                string.IsNullOrEmpty(step.CompensationPayload))
            {
                _logger.LogWarning("No compensation action or payload defined for step {StepId}", step.Id);
                return;
            }

            using var channel = _connection.CreateModel();

            // Parse compensation type: rabbitmq.publish.{exchange}.{routingKey}
            var parts = step.CompensationActionType.Split('.');
            if (parts.Length < 4 || parts[0] != "rabbitmq" || parts[1] != "publish")
            {
                throw new InvalidOperationException($"Invalid RabbitMQ compensation type: {step.CompensationActionType}");
            }

            var exchange = parts[2];
            var routingKey = parts[3];

            // Declare exchange if needed
            if (exchange != "")
            {
                channel.ExchangeDeclare(
                    exchange: exchange,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false);
            }

            // Prepare compensation message
            var messageBytes = Encoding.UTF8.GetBytes(step.CompensationPayload);
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.CorrelationId = step.Saga?.CorrelationId ?? Guid.NewGuid().ToString();
            properties.MessageId = $"{step.Id}-compensation";
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            // Add saga metadata to headers
            properties.Headers = new Dictionary<string, object>
            {
                ["saga-id"] = step.SagaId.ToString(),
                ["saga-step-id"] = step.Id.ToString(),
                ["saga-step-name"] = step.Name,
                ["saga-step-order"] = step.Order,
                ["saga-compensation"] = true
            };

            // Publish compensation message
            channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: messageBytes);

            _logger.LogInformation("Published compensation message to exchange {Exchange} with routing key {RoutingKey}",
                exchange, routingKey);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compensating RabbitMQ step {StepId}", step.Id);
            throw;
        }
    }
}
