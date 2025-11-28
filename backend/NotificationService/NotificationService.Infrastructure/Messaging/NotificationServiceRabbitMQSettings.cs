using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Infrastructure.Messaging;

public class NotificationServiceRabbitMQSettings
{
    public bool EnableRabbitMQ { get; set; } = true;

    // Configuración principal (debe coincidir EXACTAMENTE con AuthService)
    public string QueueName { get; set; } = "notification-queue";
    public string ExchangeName { get; set; } = "notification-exchange";
    public string RoutingKey { get; set; } = "notification.auth";

    // Colas específicas por tipo de notificación
    public string EmailQueueName { get; set; } = "notification-email-queue";
    public string SmsQueueName { get; set; } = "notification-sms-queue";
    public string PushQueueName { get; set; } = "notification-push-queue";

    // Configuración de reintentos y manejo de errores
    public int RetryDelayMs { get; set; } = 60000; // 1 minuto para retry
    public int MaxRetryAttempts { get; set; } = 3;
    public int PrefetchCount { get; set; } = 10; // Número de mensajes a pre-fetch

    // Configuración de timeouts
    public int ConnectionTimeoutMs { get; set; } = 30000;
    public int RequestedHeartbeatMs { get; set; } = 60;

    // Configuración de calidad de servicio (QoS)
    public bool GlobalQos { get; set; } = false;
}