using BackgroundRemovalService.Application.DTOs;
using BackgroundRemovalService.Application.Features.Commands;
using BackgroundRemovalService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackgroundRemovalService.Application.Features.Handlers;

public class CreateRemovalJobCommandHandler : IRequestHandler<CreateRemovalJobCommand, RemovalJobResponse>
{
    private readonly IBackgroundRemovalOrchestrator _orchestrator;
    private readonly ILogger<CreateRemovalJobCommandHandler> _logger;

    public CreateRemovalJobCommandHandler(
        IBackgroundRemovalOrchestrator orchestrator,
        ILogger<CreateRemovalJobCommandHandler> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    public async Task<RemovalJobResponse> Handle(CreateRemovalJobCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing background removal job for user {UserId}, CorrelationId: {CorrelationId}",
            command.UserId, command.Request.CorrelationId);
        
        try
        {
            var result = await _orchestrator.ProcessRemovalAsync(
                command.Request,
                command.UserId,
                command.TenantId,
                cancellationToken);
            
            _logger.LogInformation(
                "Background removal job {JobId} completed with status {Status}",
                result.JobId, result.Status);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing background removal job");
            throw;
        }
    }
}

public class RetryRemovalJobCommandHandler : IRequestHandler<RetryRemovalJobCommand, RemovalJobResponse>
{
    private readonly IBackgroundRemovalOrchestrator _orchestrator;
    private readonly ILogger<RetryRemovalJobCommandHandler> _logger;

    public RetryRemovalJobCommandHandler(
        IBackgroundRemovalOrchestrator orchestrator,
        ILogger<RetryRemovalJobCommandHandler> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    public async Task<RemovalJobResponse> Handle(RetryRemovalJobCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrying job {JobId} with provider {Provider}", 
            command.JobId, command.AlternateProvider);
        
        return await _orchestrator.RetryJobAsync(
            command.JobId, 
            command.AlternateProvider, 
            cancellationToken);
    }
}

public class CancelRemovalJobCommandHandler : IRequestHandler<CancelRemovalJobCommand, bool>
{
    private readonly IBackgroundRemovalOrchestrator _orchestrator;
    private readonly ILogger<CancelRemovalJobCommandHandler> _logger;

    public CancelRemovalJobCommandHandler(
        IBackgroundRemovalOrchestrator orchestrator,
        ILogger<CancelRemovalJobCommandHandler> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    public async Task<bool> Handle(CancelRemovalJobCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling job {JobId}", command.JobId);
        return await _orchestrator.CancelJobAsync(command.JobId, cancellationToken);
    }
}
