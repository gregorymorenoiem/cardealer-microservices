using MediaService.Shared.Messaging;

namespace MediaService.Domain.Interfaces;

public interface IDeadLetterQueue
{
    Task Enqueue(string eventType, string eventJson, string error);
    Task<List<FailedEvent>> GetEventsReadyForRetry();
    Task Remove(Guid eventId);
    Task MarkAsFailed(Guid eventId, string error);
    Task<(int Total, int ReadyForRetry, int Exhausted)> GetStats();
}
