using System.Diagnostics.Metrics;

namespace NotificationService.Infrastructure.Metrics;

public class NotificationServiceMetrics
{
    private readonly Meter _meter;

    // Notification Sending Metrics
    private readonly Counter<long> _notificationsSent;
    private readonly Counter<long> _notificationsFailed;
    private readonly Histogram<double> _notificationDeliveryDuration;

    // Channel-Specific Metrics
    private readonly Counter<long> _emailsSent;
    private readonly Counter<long> _smsSent;
    private readonly Counter<long> _pushNotificationsSent;

    // Failure Tracking
    private readonly Counter<long> _emailDeliveryFailures;
    private readonly Counter<long> _smsDeliveryFailures;
    private readonly Counter<long> _pushDeliveryFailures;

    // Queue Metrics
    private readonly ObservableGauge<long> _queuedNotifications;
    private readonly Histogram<double> _queueProcessingDuration;

    public NotificationServiceMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create("NotificationService");

        // Overall Notifications
        _notificationsSent = _meter.CreateCounter<long>(
            "notifications_sent_total",
            description: "Total number of notifications sent");

        _notificationsFailed = _meter.CreateCounter<long>(
            "notifications_failed_total",
            description: "Total number of failed notification attempts");

        _notificationDeliveryDuration = _meter.CreateHistogram<double>(
            "notification_delivery_duration_seconds",
            unit: "s",
            description: "Time taken to deliver notifications");

        // Channel-Specific
        _emailsSent = _meter.CreateCounter<long>(
            "emails_sent_total",
            description: "Total number of emails sent");

        _smsSent = _meter.CreateCounter<long>(
            "sms_sent_total",
            description: "Total number of SMS messages sent");

        _pushNotificationsSent = _meter.CreateCounter<long>(
            "push_notifications_sent_total",
            description: "Total number of push notifications sent");

        // Failures
        _emailDeliveryFailures = _meter.CreateCounter<long>(
            "email_delivery_failures_total",
            description: "Total email delivery failures");

        _smsDeliveryFailures = _meter.CreateCounter<long>(
            "sms_delivery_failures_total",
            description: "Total SMS delivery failures");

        _pushDeliveryFailures = _meter.CreateCounter<long>(
            "push_delivery_failures_total",
            description: "Total push notification delivery failures");

        // Queue
        _queuedNotifications = _meter.CreateObservableGauge<long>(
            "queued_notifications",
            () => GetQueuedNotificationsCount(),
            description: "Number of notifications currently queued");

        _queueProcessingDuration = _meter.CreateHistogram<double>(
            "queue_processing_duration_seconds",
            unit: "s",
            description: "Time taken to process notification queue");
    }

    // Notification Sending Methods
    public void RecordNotificationSent(string channel, string status, double durationSeconds)
    {
        _notificationsSent.Add(1,
            new KeyValuePair<string, object?>("channel", channel),
            new KeyValuePair<string, object?>("status", status));

        _notificationDeliveryDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("channel", channel));

        switch (channel.ToLower())
        {
            case "email":
                _emailsSent.Add(1, new KeyValuePair<string, object?>("status", status));
                break;
            case "sms":
                _smsSent.Add(1, new KeyValuePair<string, object?>("status", status));
                break;
            case "push":
                _pushNotificationsSent.Add(1, new KeyValuePair<string, object?>("status", status));
                break;
        }
    }

    public void RecordNotificationFailed(string channel, string reason)
    {
        _notificationsFailed.Add(1,
            new KeyValuePair<string, object?>("channel", channel),
            new KeyValuePair<string, object?>("reason", reason));

        switch (channel.ToLower())
        {
            case "email":
                _emailDeliveryFailures.Add(1, new KeyValuePair<string, object?>("reason", reason));
                break;
            case "sms":
                _smsDeliveryFailures.Add(1, new KeyValuePair<string, object?>("reason", reason));
                break;
            case "push":
                _pushDeliveryFailures.Add(1, new KeyValuePair<string, object?>("reason", reason));
                break;
        }
    }

    public void RecordQueueProcessing(double durationSeconds, int processedCount)
    {
        _queueProcessingDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("processed_count", processedCount));
    }

    private long GetQueuedNotificationsCount()
    {
        // Implementación placeholder - debe conectarse con servicio de cola real
        return 0;
    }
}
