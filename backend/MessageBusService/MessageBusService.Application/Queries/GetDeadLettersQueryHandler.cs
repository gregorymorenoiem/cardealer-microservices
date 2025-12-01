using MediatR;
using MessageBusService.Application.Interfaces;
using MessageBusService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MessageBusService.Application.Queries;

public class GetDeadLettersQueryHandler : IRequestHandler<GetDeadLettersQuery, List<DeadLetterMessage>>
{
    private readonly IDeadLetterManager _deadLetterManager;
    private readonly ILogger<GetDeadLettersQueryHandler> _logger;

    public GetDeadLettersQueryHandler(
        IDeadLetterManager deadLetterManager,
        ILogger<GetDeadLettersQueryHandler> logger)
    {
        _deadLetterManager = deadLetterManager;
        _logger = logger;
    }

    public async Task<List<DeadLetterMessage>> Handle(GetDeadLettersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching dead letters - Page: {Page}, Size: {Size}", 
                request.PageNumber, request.PageSize);

            return await _deadLetterManager.GetDeadLettersAsync(request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dead letters");
            throw;
        }
    }
}
