using MessageBusService.Domain.Entities;

namespace MessageBusService.Application.Interfaces;

public interface IDeadLetterManager
{
    Task<List<DeadLetterMessage>> GetDeadLettersAsync(int pageNumber = 1, int pageSize = 50);
    Task<bool> RetryAsync(Guid deadLetterMessageId);
    Task<bool> DiscardAsync(Guid deadLetterMessageId);
    Task<DeadLetterMessage?> GetByIdAsync(Guid deadLetterMessageId);
}
