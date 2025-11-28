namespace NotificationService.Domain.Interfaces;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, string queueName);
    Task SubscribeAsync<T>(Func<T, Task> handler, string queueName);
    Task<bool> IsConnectedAsync();
}