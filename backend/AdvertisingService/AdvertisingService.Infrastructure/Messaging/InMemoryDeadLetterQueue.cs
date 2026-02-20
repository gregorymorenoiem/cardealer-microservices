using System.Collections.Concurrent;

namespace AdvertisingService.Infrastructure.Messaging;

public interface IDeadLetterQueue
{
    Task EnqueueAsync(string queueName, string message, string? errorReason = null);
    Task<IEnumerable<DeadLetterMessage>> GetMessagesAsync(string queueName, int count = 10);
}

public class InMemoryDeadLetterQueue : IDeadLetterQueue
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DeadLetterMessage>> _queues = new();

    public Task EnqueueAsync(string queueName, string message, string? errorReason = null)
    {
        var queue = _queues.GetOrAdd(queueName, _ => new ConcurrentQueue<DeadLetterMessage>());
        queue.Enqueue(new DeadLetterMessage
        {
            Id = Guid.NewGuid(),
            QueueName = queueName,
            Message = message,
            ErrorReason = errorReason,
            EnqueuedAt = DateTime.UtcNow
        });
        return Task.CompletedTask;
    }

    public Task<IEnumerable<DeadLetterMessage>> GetMessagesAsync(string queueName, int count = 10)
    {
        if (_queues.TryGetValue(queueName, out var queue))
        {
            var messages = new List<DeadLetterMessage>();
            while (messages.Count < count && queue.TryDequeue(out var msg))
            {
                messages.Add(msg);
            }
            return Task.FromResult<IEnumerable<DeadLetterMessage>>(messages);
        }
        return Task.FromResult<IEnumerable<DeadLetterMessage>>(Array.Empty<DeadLetterMessage>());
    }
}

public class DeadLetterMessage
{
    public Guid Id { get; set; }
    public string QueueName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ErrorReason { get; set; }
    public DateTime EnqueuedAt { get; set; }
}
