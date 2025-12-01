using MediatR;
using MessageBusService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MessageBusService.Application.Commands;

public class RetryDeadLetterCommandHandler : IRequestHandler<RetryDeadLetterCommand, bool>
{
    private readonly IDeadLetterManager _deadLetterManager;
    private readonly ILogger<RetryDeadLetterCommandHandler> _logger;

    public RetryDeadLetterCommandHandler(
        IDeadLetterManager deadLetterManager,
        ILogger<RetryDeadLetterCommandHandler> logger)
    {
        _deadLetterManager = deadLetterManager;
        _logger = logger;
    }

    public async Task<bool> Handle(RetryDeadLetterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrying dead letter message: {MessageId}", request.DeadLetterMessageId);

            var result = await _deadLetterManager.RetryAsync(request.DeadLetterMessageId);

            if (result)
            {
                _logger.LogInformation("Dead letter message retried successfully: {MessageId}", request.DeadLetterMessageId);
            }
            else
            {
                _logger.LogWarning("Failed to retry dead letter message: {MessageId}", request.DeadLetterMessageId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying dead letter message: {MessageId}", request.DeadLetterMessageId);
            throw;
        }
    }
}
