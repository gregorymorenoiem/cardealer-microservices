using MediatR;
using MessageBusService.Domain.Entities;

namespace MessageBusService.Application.Queries;

public class GetDeadLettersQuery : IRequest<List<DeadLetterMessage>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
