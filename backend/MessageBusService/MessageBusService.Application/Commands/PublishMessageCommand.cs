using MediatR;
using MessageBusService.Domain.Enums;

namespace MessageBusService.Application.Commands;

public class PublishMessageCommand : IRequest<bool>
{
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    public Dictionary<string, string>? Headers { get; set; }
}
