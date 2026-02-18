using CarDealer.Shared.ErrorHandling.Interfaces;
using CarDealer.Shared.ErrorHandling.Models;

namespace ErrorService.Api.Services;

/// <summary>
/// No-op implementation of IErrorPublisher for ErrorService itself.
/// ErrorService should NOT publish errors to itself (circular dependency).
/// Errors are logged locally instead.
/// </summary>
public class NoOpErrorPublisher : IErrorPublisher
{
    private readonly ILogger _logger;

    public NoOpErrorPublisher(ILogger logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(ErrorEvent errorEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "[ErrorService-NoOp] Error event logged locally (not published to self): {Message}",
            errorEvent.Message);
        return Task.CompletedTask;
    }

    public Task PublishExceptionAsync(
        Exception exception,
        ErrorContext? context = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            exception,
            "[ErrorService-NoOp] Exception logged locally (not published to self): {Message}",
            exception.Message);
        return Task.CompletedTask;
    }
}
