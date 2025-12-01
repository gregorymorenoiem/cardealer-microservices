using MediatR;

namespace MessageBusService.Application.Commands;

public class SubscribeToTopicCommand : IRequest<Guid>
{
    public string Topic { get; set; } = string.Empty;
    public string ConsumerName { get; set; } = string.Empty;
}
