using MediatR;
using MessageBusService.Domain.Entities;

namespace MessageBusService.Application.Queries;

public class GetMessageHistoryQuery : IRequest<List<Message>>
{
    public string? Topic { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
