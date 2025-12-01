using MediatR;

namespace MessageBusService.Application.Commands;

public class RetryDeadLetterCommand : IRequest<bool>
{
    public Guid DeadLetterMessageId { get; set; }
}
