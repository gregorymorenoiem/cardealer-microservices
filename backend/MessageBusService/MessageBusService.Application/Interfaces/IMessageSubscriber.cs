using MessageBusService.Domain.Entities;

namespace MessageBusService.Application.Interfaces;

public interface IMessageSubscriber
{
    Task<Subscription> SubscribeAsync(string topic, string consumerName);
    Task<bool> UnsubscribeAsync(Guid subscriptionId);
    Task<List<Subscription>> GetSubscriptionsAsync(string? topic = null);
}
