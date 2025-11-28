using Azure.Messaging.ServiceBus;
using System.Text.Json;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Infrastructure.MessageBus;

public class AzureServiceBus : IMessageBus
{
    private readonly ServiceBusClient _client;

    public AzureServiceBus(string connectionString)
    {
        _client = new ServiceBusClient(connectionString);
    }

    public async Task PublishAsync<T>(T message, string queueName)
    {
        var sender = _client.CreateSender(queueName);
        var messageBody = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(messageBody);
        await sender.SendMessageAsync(serviceBusMessage);
    }

    public async Task SubscribeAsync<T>(Func<T, Task> handler, string queueName)
    {
        var processor = _client.CreateProcessor(queueName);
        processor.ProcessMessageAsync += async args =>
        {
            var messageBody = args.Message.Body.ToString();
            var message = JsonSerializer.Deserialize<T>(messageBody);
            if (message != null)
            {
                await handler(message);
            }
            await args.CompleteMessageAsync(args.Message);
        };
        processor.ProcessErrorAsync += args =>
        {
            // Manejar error
            return Task.CompletedTask;
        };
        await processor.StartProcessingAsync();
    }

    public Task<bool> IsConnectedAsync()
    {
        return Task.FromResult(_client != null);
    }
}