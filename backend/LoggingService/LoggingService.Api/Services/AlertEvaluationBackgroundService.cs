using LoggingService.Application.Interfaces;

namespace LoggingService.Api.Services;

/// <summary>
/// Background service that periodically evaluates alert rules
/// </summary>
public class AlertEvaluationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AlertEvaluationBackgroundService> _logger;
    private readonly TimeSpan _evaluationInterval;

    public AlertEvaluationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<AlertEvaluationBackgroundService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var intervalMinutes = configuration.GetValue<int>("Alerting:EvaluationIntervalMinutes", 5);
        _evaluationInterval = TimeSpan.FromMinutes(intervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Alert evaluation background service starting. Interval: {Interval}", _evaluationInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EvaluateRulesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during alert rule evaluation");
            }

            await Task.Delay(_evaluationInterval, stoppingToken);
        }

        _logger.LogInformation("Alert evaluation background service stopping");
    }

    private async Task EvaluateRulesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var alertingService = scope.ServiceProvider.GetRequiredService<IAlertingService>();

        _logger.LogDebug("Evaluating alert rules...");

        await alertingService.EvaluateRulesAsync(cancellationToken);

        _logger.LogDebug("Alert rule evaluation completed");
    }
}
