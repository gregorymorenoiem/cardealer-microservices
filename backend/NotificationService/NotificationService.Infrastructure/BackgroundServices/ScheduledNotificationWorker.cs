using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Domain.Interfaces.Repositories;
using NotificationService.Infrastructure.Services;

namespace NotificationService.Infrastructure.BackgroundServices;

public class ScheduledNotificationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScheduledNotificationWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public ScheduledNotificationWorker(
        IServiceProvider serviceProvider,
        ILogger<ScheduledNotificationWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scheduled Notification Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled notifications");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Scheduled Notification Worker stopped");
    }

    private async Task ProcessDueNotificationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IScheduledNotificationRepository>();
        var schedulingService = scope.ServiceProvider.GetRequiredService<ISchedulingService>();
        var notificationQueueRepo = scope.ServiceProvider.GetRequiredService<INotificationQueueRepository>();

        var dueNotifications = await repository.GetDueNotificationsAsync();

        foreach (var scheduledNotification in dueNotifications)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                _logger.LogInformation("Processing scheduled notification {Id}", scheduledNotification.Id);

                scheduledNotification.MarkAsProcessing();
                await repository.UpdateAsync(scheduledNotification);

                // Add notification to queue for processing
                if (scheduledNotification.Notification != null)
                {
                    var queue = Domain.Entities.NotificationQueue.Create(scheduledNotification.Notification);
                    await notificationQueueRepo.AddAsync(queue);
                }

                scheduledNotification.MarkAsExecuted();

                // Calculate next execution for recurring notifications
                if (scheduledNotification.IsRecurring)
                {
                    var nextExecution = schedulingService.CalculateNextExecution(scheduledNotification);
                    if (nextExecution.HasValue)
                    {
                        scheduledNotification.SetNextExecution(nextExecution.Value);
                    }
                }

                await repository.UpdateAsync(scheduledNotification);

                _logger.LogInformation("Successfully processed scheduled notification {Id}", scheduledNotification.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled notification {Id}", scheduledNotification.Id);

                scheduledNotification.MarkAsFailed(ex.Message);
                await repository.UpdateAsync(scheduledNotification);
            }
        }
    }
}
