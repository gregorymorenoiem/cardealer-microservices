using MediatR;
using Microsoft.Extensions.Logging;

namespace MediaService.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestType = request.GetType().Name;

        _logger.LogInformation("Handling {RequestName} ({RequestType})", requestName, requestType);

        try
        {
            var startTime = DateTime.UtcNow;
            var response = await next();
            var duration = DateTime.UtcNow - startTime;

            _logger.LogInformation("Handled {RequestName} successfully in {Duration}ms",
                requestName, duration.TotalMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {RequestName}", requestName);
            throw;
        }
    }
}