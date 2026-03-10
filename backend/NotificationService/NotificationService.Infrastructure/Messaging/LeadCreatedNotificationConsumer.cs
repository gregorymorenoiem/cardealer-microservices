using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CarDealer.Contracts.Events.Vehicle;
using MediatR;
using NotificationService.Application.Interfaces;
using NotificationService.Application.UseCases.SendWhatsAppNotification;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Infrastructure.Messaging;

/// <summary>
/// Consumes LeadCreatedEvent from RabbitMQ and sends notifications to dealers:
/// 1. Email notification with lead details
/// 2. WhatsApp notification (if enabled and dealer phone is available)
/// 3. In-app notification for real-time dashboard
/// 4. Admin alert for monitoring
/// 
/// SLA: Notification delivered in under 60 seconds from event publish.
/// </summary>
public class LeadCreatedNotificationConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LeadCreatedNotificationConsumer> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    private const string ExchangeName = "cardealer.events";
    private const string QueueName = "notificationservice.lead.created";
    private const string RoutingKey = "vehicles.lead.created";

    public LeadCreatedNotificationConsumer(
        IServiceProvider serviceProvider,
        ILogger<LeadCreatedNotificationConsumer> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbitMQEnabled = _configuration.GetValue<bool>("RabbitMQ:Enabled");
        if (!rabbitMQEnabled)
        {
            _logger.LogInformation("RabbitMQ is disabled. LeadCreatedNotificationConsumer will not start.");
            return;
        }

        try
        {
            InitializeRabbitMQ();

            if (_channel == null)
            {
                _logger.LogWarning("RabbitMQ channel is null after initialization for LeadCreatedNotificationConsumer. Consumer will not start.");
                return;
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var leadEvent = JsonSerializer.Deserialize<LeadCreatedEvent>(message, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (leadEvent != null)
                    {
                        _logger.LogInformation(
                            "Received LeadCreatedEvent: LeadId={LeadId}, VehicleId={VehicleId}, BuyerName={BuyerName}, SellerId={SellerId}",
                            leadEvent.LeadId, leadEvent.VehicleId, leadEvent.BuyerName, leadEvent.SellerId);

                        await HandleLeadCreatedEventAsync(leadEvent, stoppingToken);

                        _channel.BasicAck(ea.DeliveryTag, multiple: false);
                        _logger.LogDebug("LeadCreatedEvent message acknowledged: {MessageId}", ea.BasicProperties?.MessageId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize LeadCreatedEvent. Sending to DLQ.");
                        _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing LeadCreatedEvent. Requeuing for retry.");
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("LeadCreatedNotificationConsumer started listening on queue: {Queue}", QueueName);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in LeadCreatedNotificationConsumer");
        }
    }

    private void InitializeRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? _configuration["RabbitMQ:HostName"] ?? "localhost",
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:Username"] ?? _configuration["RabbitMQ:UserName"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest",
                VirtualHost = _configuration["RabbitMQ:VirtualHost"] ?? "/",
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection($"LeadCreatedConsumer-{Environment.MachineName}");
            _channel = _connection.CreateModel();

            // Declare exchange
            _channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            // Declare queue with DLQ support
            var queueArgs = new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = $"{ExchangeName}.dlq",
                ["x-dead-letter-routing-key"] = $"{RoutingKey}.dlq"
            };

            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArgs);

            // Bind queue to exchange
            _channel.QueueBind(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: RoutingKey);

            // Prefetch 1 for ordered processing
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _logger.LogInformation("RabbitMQ initialized successfully for LeadCreatedNotificationConsumer");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection for LeadCreatedNotificationConsumer");
            throw;
        }
    }

    private async Task HandleLeadCreatedEventAsync(LeadCreatedEvent eventData, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();

        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var userNotifService = scope.ServiceProvider.GetService<IUserNotificationService>();
        var adminAlertService = scope.ServiceProvider.GetService<IAdminAlertService>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // ── 1. Resolve dealer email & phone via UserService ────────────────────
        string? dealerEmail = null;
        string? dealerPhone = null;
        string? dealerName = null;

        try
        {
            var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();
            if (httpClientFactory != null)
            {
                var client = httpClientFactory.CreateClient("UserService");

                // Get dealer contact info
                var response = await client.GetAsync($"/api/users/{eventData.SellerId}/profile", ct);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(ct);
                    var profile = JsonSerializer.Deserialize<JsonElement>(json);

                    dealerEmail = profile.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
                    dealerPhone = profile.TryGetProperty("phoneNumber", out var phoneProp) ? phoneProp.GetString() : null;
                    dealerName = profile.TryGetProperty("fullName", out var nameProp) ? nameProp.GetString() : null;

                    if (string.IsNullOrWhiteSpace(dealerEmail))
                    {
                        // Fallback: try email endpoint
                        var emailResp = await client.GetAsync($"/api/users/{eventData.SellerId}/email", ct);
                        if (emailResp.IsSuccessStatusCode)
                        {
                            dealerEmail = await emailResp.Content.ReadAsStringAsync(ct);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve dealer info from UserService for SellerId={SellerId}", eventData.SellerId);
        }

        if (string.IsNullOrWhiteSpace(dealerEmail))
        {
            _logger.LogWarning(
                "Could not resolve dealer email for LeadId={LeadId}, SellerId={SellerId}. Skipping email notification.",
                eventData.LeadId, eventData.SellerId);
        }

        // ── 2. Determine if this is a CELEBRATION (first ever inquiry) ────────
        var isCelebration = eventData.IsFirstInquiry;
        if (isCelebration)
        {
            _logger.LogInformation(
                "🎉 FIRST INQUIRY EVER for SellerId={SellerId}! Sending celebration notifications for LeadId={LeadId}",
                eventData.SellerId, eventData.LeadId);
        }

        // ── 3. Send Email to Dealer ───────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(dealerEmail))
        {
            try
            {
                string subject;
                string body;

                if (isCelebration)
                {
                    subject = $"🎉🚗 ¡Felicidades! Tu primera consulta en OKLA — {eventData.BuyerName} quiere saber de tu vehículo";
                    body = BuildFirstInquiryCelebrationEmailHtml(eventData, dealerName);
                }
                else
                {
                    subject = $"🔔 Nuevo Lead: {eventData.BuyerName} interesado en {eventData.VehicleTitle}";
                    body = BuildLeadEmailHtml(eventData, dealerName);
                }

                await emailService.SendEmailAsync(
                    to: dealerEmail,
                    subject: subject,
                    body: body,
                    isHtml: true);

                _logger.LogInformation(
                    "✅ Lead email notification sent to dealer {DealerEmail} for LeadId={LeadId} (celebration={IsCelebration})",
                    dealerEmail, eventData.LeadId, isCelebration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send lead email to dealer for LeadId={LeadId}", eventData.LeadId);
            }
        }

        // ── 4. Send WhatsApp to Dealer ────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(dealerPhone))
        {
            try
            {
                string whatsappMessage;

                if (isCelebration)
                {
                    whatsappMessage = $"🎉🚗 *¡FELICIDADES! Tu primera consulta en OKLA*\n\n" +
                                      $"Un comprador real está interesado en tu vehículo. " +
                                      $"¡Este es el inicio de algo grande!\n\n" +
                                      $"*Comprador:* {eventData.BuyerName}\n" +
                                      $"*Email:* {eventData.BuyerEmail}\n" +
                                      $"*Teléfono:* {eventData.BuyerPhone ?? "No proporcionado"}\n" +
                                      $"*Vehículo:* {eventData.VehicleTitle}\n\n" +
                                      $"💬 *Su mensaje:*\n\"{TruncateMessage(eventData.Message, 400)}\"\n\n" +
                                      $"⚡ *Tip de OKLA:* Responde en menos de 5 min y tendrás 10x más probabilidad de cerrar la venta.\n\n" +
                                      $"🔗 Ver lead: https://okla.com.do/dashboard/leads/{eventData.LeadId}";
                }
                else
                {
                    whatsappMessage = $"🚗 *Nuevo Lead en OKLA*\n\n" +
                                      $"*Comprador:* {eventData.BuyerName}\n" +
                                      $"*Email:* {eventData.BuyerEmail}\n" +
                                      $"*Teléfono:* {eventData.BuyerPhone ?? "No proporcionado"}\n" +
                                      $"*Vehículo:* {eventData.VehicleTitle}\n" +
                                      $"*Mensaje:* {TruncateMessage(eventData.Message, 500)}\n\n" +
                                      $"Responde rápido para cerrar la venta 💪";
                }

                var whatsappResult = await mediator.Send(new SendWhatsAppNotificationCommand(
                    To: NormalizePhoneNumber(dealerPhone),
                    Message: whatsappMessage,
                    Metadata: new Dictionary<string, object>
                    {
                        ["leadId"] = eventData.LeadId.ToString(),
                        ["vehicleId"] = eventData.VehicleId.ToString(),
                        ["buyerName"] = eventData.BuyerName,
                        ["notificationType"] = isCelebration ? "first_inquiry_celebration" : "lead_created"
                    }
                ), ct);

                if (whatsappResult.Success)
                {
                    _logger.LogInformation(
                        "✅ WhatsApp notification sent to dealer {DealerPhone} for LeadId={LeadId}. MessageId={MessageId}",
                        dealerPhone, eventData.LeadId, whatsappResult.MessageId);
                }
                else
                {
                    _logger.LogWarning(
                        "⚠️ WhatsApp notification failed for dealer {DealerPhone}: {Error}",
                        dealerPhone, whatsappResult.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "⚠️ WhatsApp notification error for LeadId={LeadId}. Non-critical, email was sent.",
                    eventData.LeadId);
            }
        }
        else
        {
            _logger.LogInformation(
                "Dealer phone not available for SellerId={SellerId}. WhatsApp notification skipped for LeadId={LeadId}",
                eventData.SellerId, eventData.LeadId);
        }

        // ── 4. In-App Notification for Dealer Dashboard ────────────────────────
        if (userNotifService != null)
        {
            try
            {
                await userNotifService.CreateAsync(
                    userId: eventData.SellerId,
                    type: "new_lead",
                    title: "🔔 Nuevo Lead Recibido",
                    message: $"{eventData.BuyerName} está interesado en {eventData.VehicleTitle}",
                    icon: "🔔",
                    link: $"/dashboard/leads/{eventData.LeadId}",
                    cancellationToken: ct);

                _logger.LogDebug("In-app notification created for seller {SellerId}", eventData.SellerId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create in-app notification for lead. Non-critical.");
            }
        }

        // ── 5. Admin Alert ─────────────────────────────────────────────────────
        if (adminAlertService != null)
        {
            try
            {
                await adminAlertService.SendAlertAsync(
                    alertType: "new_lead_received",
                    title: "Nuevo Lead en OKLA",
                    message: $"Lead de {eventData.BuyerName} ({eventData.BuyerEmail}) para vehículo {eventData.VehicleTitle}",
                    severity: "Info",
                    metadata: new Dictionary<string, string>
                    {
                        ["LeadId"] = eventData.LeadId.ToString(),
                        ["VehicleId"] = eventData.VehicleId.ToString(),
                        ["BuyerName"] = eventData.BuyerName,
                        ["BuyerEmail"] = eventData.BuyerEmail,
                        ["SellerId"] = eventData.SellerId.ToString()
                    },
                    ct: ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send admin alert for new lead. Non-critical.");
            }
        }

        _logger.LogInformation(
            "✅ Lead notification pipeline completed for LeadId={LeadId}, SellerId={SellerId}",
            eventData.LeadId, eventData.SellerId);
    }

    private static string BuildLeadEmailHtml(LeadCreatedEvent lead, string? dealerName)
    {
        var greeting = !string.IsNullOrWhiteSpace(dealerName) ? $"Hola {dealerName}" : "Hola";

        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
                    
                    <div style='background: linear-gradient(135deg, #007bff, #0056b3); padding: 24px; text-align: center;'>
                        <h1 style='color: white; margin: 0; font-size: 24px;'>🔔 Nuevo Lead Recibido</h1>
                    </div>
                    
                    <div style='padding: 24px;'>
                        <p style='font-size: 16px; color: #333;'>{greeting},</p>
                        <p style='font-size: 16px; color: #333;'>
                            Tienes un nuevo comprador interesado en tu vehículo publicado en <strong>OKLA</strong>.
                        </p>
                        
                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #007bff;'>
                            <h3 style='margin-top: 0; color: #007bff;'>📋 Detalles del Lead</h3>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='padding: 8px 0; color: #666; width: 140px;'><strong>Comprador:</strong></td>
                                    <td style='padding: 8px 0; color: #333;'>{lead.BuyerName}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Email:</strong></td>
                                    <td style='padding: 8px 0; color: #333;'>
                                        <a href='mailto:{lead.BuyerEmail}' style='color: #007bff;'>{lead.BuyerEmail}</a>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Teléfono:</strong></td>
                                    <td style='padding: 8px 0; color: #333;'>{(string.IsNullOrWhiteSpace(lead.BuyerPhone) ? "No proporcionado" : lead.BuyerPhone)}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Vehículo:</strong></td>
                                    <td style='padding: 8px 0; color: #333; font-weight: bold;'>{lead.VehicleTitle}</td>
                                </tr>
                            </table>
                        </div>
                        
                        <div style='background-color: #fff3cd; padding: 16px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ffc107;'>
                            <h4 style='margin-top: 0; color: #856404;'>💬 Mensaje del Comprador</h4>
                            <p style='color: #856404; font-style: italic; margin-bottom: 0;'>""{lead.Message}""</p>
                        </div>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='https://okla.com.do/dashboard/leads/{lead.LeadId}' 
                               style='background-color: #007bff; color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; font-size: 16px; font-weight: bold; display: inline-block;'>
                                Ver Lead y Responder
                            </a>
                        </div>

                        <p style='color: #dc3545; font-size: 14px; text-align: center; font-weight: bold;'>
                            ⏱️ Los compradores que reciben respuesta en menos de 5 minutos tienen 10x más probabilidad de cerrar la compra.
                        </p>
                    </div>
                    
                    <div style='background-color: #f8f9fa; padding: 16px; text-align: center; border-top: 1px solid #dee2e6;'>
                        <p style='color: #6c757d; font-size: 12px; margin: 0;'>
                            Este es un mensaje automático de OKLA — El marketplace automotriz #1 de RD.
                        </p>
                    </div>
                </div>
            </body>
            </html>
        ";
    }

    /// <summary>
    /// Normalizes Dominican phone numbers to E.164 format for WhatsApp.
    /// </summary>
    private static string NormalizePhoneNumber(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        // Dominican Republic: 809, 829, 849 area codes
        if (digits.Length == 10 && (digits.StartsWith("809") || digits.StartsWith("829") || digits.StartsWith("849")))
        {
            return $"+1{digits}";
        }

        // Already has country code
        if (digits.Length == 11 && digits.StartsWith("1"))
        {
            return $"+{digits}";
        }

        // Already E.164
        if (phone.StartsWith("+"))
        {
            return phone;
        }

        return $"+{digits}";
    }

    private static string TruncateMessage(string message, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(message)) return "(Sin mensaje)";
        return message.Length <= maxLength ? message : message[..maxLength] + "...";
    }

    /// <summary>
    /// Builds celebration HTML email for the dealer's very first inquiry on OKLA.
    /// High-impact, positive reinforcement design.
    /// </summary>
    private static string BuildFirstInquiryCelebrationEmailHtml(LeadCreatedEvent lead, string? dealerName)
    {
        var greeting = !string.IsNullOrWhiteSpace(dealerName) ? $"¡Felicidades, {dealerName}!" : "¡Felicidades!";

        return $@"
            <html>
            <body style='font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px;'>
                <div style='max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 16px rgba(0,0,0,0.15);'>
                    
                    <div style='background: linear-gradient(135deg, #ff6b35, #f7931e, #ffd700); padding: 32px; text-align: center;'>
                        <h1 style='color: white; margin: 0; font-size: 32px;'>🎉🚗 ¡Tu Primera Consulta!</h1>
                        <p style='color: white; font-size: 18px; margin-top: 8px; opacity: 0.95;'>Tu vehículo ya está generando interés en OKLA</p>
                    </div>
                    
                    <div style='padding: 28px;'>
                        <p style='font-size: 18px; color: #333; font-weight: bold;'>{greeting}</p>
                        <p style='font-size: 16px; color: #333; line-height: 1.6;'>
                            Un comprador real acaba de contactarte a través de <strong>OKLA</strong>. 
                            ¡Esto significa que tu publicación está funcionando y tu vehículo está captando la atención del mercado!
                        </p>

                        <div style='background: linear-gradient(135deg, #e8f5e9, #c8e6c9); padding: 20px; border-radius: 10px; margin: 24px 0; border-left: 5px solid #4caf50; text-align: center;'>
                            <p style='font-size: 48px; margin: 0;'>🏆</p>
                            <p style='font-size: 20px; color: #2e7d32; font-weight: bold; margin: 8px 0 4px 0;'>¡Hito Desbloqueado!</p>
                            <p style='color: #388e3c; font-size: 14px; margin: 0;'>Primera consulta recibida — El camino hacia tu primera venta ha comenzado.</p>
                        </div>
                        
                        <div style='background-color: #f0f4ff; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #007bff;'>
                            <h3 style='margin-top: 0; color: #007bff;'>📋 Tu Primer Lead</h3>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='padding: 8px 0; color: #666; width: 140px;'><strong>Comprador:</strong></td>
                                    <td style='padding: 8px 0; color: #333; font-weight: bold;'>{lead.BuyerName}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Email:</strong></td>
                                    <td style='padding: 8px 0; color: #333;'>
                                        <a href='mailto:{lead.BuyerEmail}' style='color: #007bff;'>{lead.BuyerEmail}</a>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Teléfono:</strong></td>
                                    <td style='padding: 8px 0; color: #333;'>{(string.IsNullOrWhiteSpace(lead.BuyerPhone) ? "No proporcionado" : lead.BuyerPhone)}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Vehículo:</strong></td>
                                    <td style='padding: 8px 0; color: #333; font-weight: bold;'>{lead.VehicleTitle}</td>
                                </tr>
                            </table>
                        </div>
                        
                        <div style='background-color: #fff8e1; padding: 16px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ff9800;'>
                            <h4 style='margin-top: 0; color: #e65100;'>💬 Mensaje del Comprador</h4>
                            <p style='color: #bf360c; font-style: italic; font-size: 15px; margin-bottom: 0;'>""{lead.Message}""</p>
                        </div>
                        
                        <div style='text-align: center; margin: 32px 0;'>
                            <a href='https://okla.com.do/dashboard/leads/{lead.LeadId}' 
                               style='background: linear-gradient(135deg, #ff6b35, #f7931e); color: white; padding: 16px 40px; text-decoration: none; border-radius: 8px; font-size: 18px; font-weight: bold; display: inline-block; box-shadow: 0 4px 12px rgba(255,107,53,0.4);'>
                                🚀 Responder Ahora
                            </a>
                        </div>

                        <div style='background-color: #fce4ec; padding: 14px; border-radius: 8px; text-align: center; margin: 20px 0;'>
                            <p style='color: #c62828; font-size: 15px; font-weight: bold; margin: 0;'>
                                ⏱️ Los dealers que responden en menos de 5 minutos tienen <span style='font-size: 18px;'>10x</span> más probabilidad de cerrar la venta.
                            </p>
                        </div>

                        <div style='background-color: #f3e5f5; padding: 16px; border-radius: 8px; margin: 20px 0;'>
                            <h4 style='margin-top: 0; color: #6a1b9a;'>💡 Tips para tu primera venta:</h4>
                            <ul style='color: #4a148c; line-height: 1.8; padding-left: 20px;'>
                                <li>Responde rápido — la velocidad cierra ventas</li>
                                <li>Sé amigable y profesional — primera impresión importa</li>
                                <li>Ofrece agendar una cita de prueba del vehículo</li>
                                <li>Incluye fotos adicionales si el comprador lo pide</li>
                            </ul>
                        </div>
                    </div>
                    
                    <div style='background: linear-gradient(135deg, #1a237e, #283593); padding: 20px; text-align: center;'>
                        <p style='color: #ffffff; font-size: 14px; margin: 0 0 4px 0; font-weight: bold;'>
                            OKLA — El marketplace automotriz #1 de la República Dominicana
                        </p>
                        <p style='color: #bbdefb; font-size: 12px; margin: 0;'>
                            Estamos contigo en cada paso del camino. ¡Éxito con tu primera venta! 🏁
                        </p>
                    </div>
                </div>
            </body>
            </html>
        ";
    }

    public override void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ connection for LeadCreatedNotificationConsumer");
        }

        base.Dispose();
    }
}
