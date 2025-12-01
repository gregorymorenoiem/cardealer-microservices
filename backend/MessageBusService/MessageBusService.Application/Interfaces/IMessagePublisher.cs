using MessageBusService.Domain.Entities;
using MessageBusService.Domain.Enums;

namespace MessageBusService.Application.Interfaces;

public interface IMessagePublisher
{
    Task<bool> PublishAsync(string topic, string payload, MessagePriority priority = MessagePriority.Normal, Dictionary<string, string>? headers = null);
    Task<bool> PublishBatchAsync(string topic, List<string> payloads, MessagePriority priority = MessagePriority.Normal);
    Task<Message?> GetMessageStatusAsync(Guid messageId);
}
