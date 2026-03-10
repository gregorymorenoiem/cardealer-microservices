using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CarDealer.Contracts.Events.Vehicle;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Interfaces.Repositories;

namespace NotificationService.Infrastructure.Messaging;

/// <summary>
/// Background service que escucha eventos VehiclePublished y evalúa
/// búsquedas guardadas activas para enviar notificaciones de coincidencia
/// al comprador en tiempo real (< 30 min SLA).
///
/// Routing key: vehicles.vehicle.published
/// Queue: notificationservice.alert.vehicle_published_matcher
/// Exchange: cardealer.events (Topic)
///
/// Published by: VehiclesSaleService (cuando un moderador aprueba un vehículo)
/// </summary>
public class VehiclePublishedAlertMatcherConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VehiclePublishedAlertMatcherConsumer> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;
    private const string ExchangeName = "cardealer.events";
    private const string QueueName = "notificationservice.alert.vehicle_published_matcher";
    private const string RoutingKey = "vehicles.vehicle.published";

    public VehiclePublishedAlertMatcherConsumer(
        IServiceProvider serviceProvider,
        ILogger<VehiclePublishedAlertMatcherConsumer> logger,
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
            _logger.LogInformation("RabbitMQ is disabled. VehiclePublishedAlertMatcherConsumer will not start.");
            return;
        }

        try
        {
            InitializeRabbitMQ();

            if (_channel == null)
            {
                _logger.LogWarning("RabbitMQ channel is null. VehiclePublishedAlertMatcherConsumer will not start.");
                return;
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var vehicleEvent = JsonSerializer.Deserialize<VehiclePublishedEvent>(message,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (vehicleEvent != null)
                    {
                        _logger.LogInformation(
                            "Received VehiclePublishedEvent: VehicleId={VehicleId}, {Make} {Model} {Year}, Price={Price}",
                            vehicleEvent.VehicleId, vehicleEvent.Make, vehicleEvent.Model,
                            vehicleEvent.Year, vehicleEvent.Price);

                        await MatchSavedSearchesAsync(vehicleEvent, stoppingToken);
                        await MatchPriceAlertsAsync(vehicleEvent, stoppingToken);

                        _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize VehiclePublishedEvent");
                        _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing VehiclePublishedEvent for alert matching");
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation(
                "✅ VehiclePublishedAlertMatcherConsumer started — Queue: {Queue}, RoutingKey: {RoutingKey}",
                QueueName, RoutingKey);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in VehiclePublishedAlertMatcherConsumer");
        }
    }

    // ════════════════════════════════════════════════════════════════════
    // SAVED SEARCH MATCHING — "Toyota Corolla 2019-2021 bajo $18,000"
    // ════════════════════════════════════════════════════════════════════

    private async Task MatchSavedSearchesAsync(VehiclePublishedEvent vehicle, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var savedSearchRepo = scope.ServiceProvider.GetRequiredService<ISavedSearchRepository>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var userNotifService = scope.ServiceProvider.GetService<IUserNotificationService>();

        try
        {
            var activeSearches = await savedSearchRepo.GetActiveSearchesAsync();
            var matchCount = 0;

            foreach (var search in activeSearches)
            {
                if (!search.NotifyOnNewResults) continue;

                try
                {
                    var criteria = JsonSerializer.Deserialize<SavedSearchCriteria>(
                        search.CriteriaJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (criteria == null) continue;

                    if (!IsMatch(vehicle, criteria)) continue;

                    matchCount++;

                    // Update match stats
                    search.MatchCount++;
                    search.LastMatchAt = DateTime.UtcNow;
                    await savedSearchRepo.UpdateAsync(search);

                    // Respect notification frequency
                    if (!ShouldNotifyNow(search)) continue;

                    // ── Send notifications ──
                    var userEmail = await ResolveUserEmailAsync(scope, search.UserId);

                    if (!string.IsNullOrWhiteSpace(userEmail) && search.NotifyByEmail)
                    {
                        await SendMatchEmailAsync(emailService, userEmail, search, vehicle);
                    }

                    // In-app notification (always)
                    if (userNotifService != null)
                    {
                        await userNotifService.CreateAsync(
                            userId: search.UserId,
                            type: "saved_search_match",
                            title: "🔔 ¡Nuevo vehículo coincide con tu búsqueda!",
                            message: $"{vehicle.Make} {vehicle.Model} {vehicle.Year} — RD${vehicle.Price:N0} coincide con \"{search.Name}\"",
                            icon: "🚗",
                            link: $"/vehiculos/{vehicle.Slug ?? vehicle.VehicleId.ToString()}",
                            cancellationToken: ct);
                    }

                    // Push notification via FCM topic (user-scoped topic pattern)
                    try
                    {
                        var pushService = scope.ServiceProvider.GetService<IPushNotificationService>();
                        if (pushService != null)
                        {
                            var pushData = new Dictionary<string, string>
                            {
                                ["type"] = "saved_search_match",
                                ["vehicleId"] = vehicle.VehicleId.ToString(),
                                ["slug"] = vehicle.Slug ?? "",
                                ["searchId"] = search.Id.ToString(),
                                ["deepLink"] = $"/vehiculos/{vehicle.Slug ?? vehicle.VehicleId.ToString()}"
                            };
                            await pushService.SendToTopicAsync(
                                topic: $"user_{search.UserId}_alerts",
                                title: "🔔 ¡Nuevo vehículo encontrado!",
                                message: $"{vehicle.Make} {vehicle.Model} {vehicle.Year} — RD${vehicle.Price:N0}",
                                customData: pushData);
                        }
                    }
                    catch (Exception pushEx)
                    {
                        // Push is best-effort — don't fail the entire notification pipeline
                        _logger.LogWarning(pushEx, "Push notification failed for SavedSearch {SearchId}, User {UserId}",
                            search.Id, search.UserId);
                    }

                    search.LastNotifiedAt = DateTime.UtcNow;
                    await savedSearchRepo.UpdateAsync(search);

                    _logger.LogInformation(
                        "✅ Match! SavedSearch {SearchId} (\"{Name}\") matched Vehicle {VehicleId}. User {UserId} notified.",
                        search.Id, search.Name, vehicle.VehicleId, search.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error evaluating SavedSearch {SearchId} against Vehicle {VehicleId}",
                        search.Id, vehicle.VehicleId);
                }
            }

            _logger.LogInformation(
                "SavedSearch matching complete for Vehicle {VehicleId}: {MatchCount} matches out of {TotalSearches} active searches",
                vehicle.VehicleId, matchCount, activeSearches.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to match SavedSearches for Vehicle {VehicleId}", vehicle.VehicleId);
        }
    }

    /// <summary>
    /// Core matching logic: does the published vehicle match the search criteria?
    /// All criteria are AND-ed (every specified field must match).
    /// </summary>
    private static bool IsMatch(VehiclePublishedEvent vehicle, SavedSearchCriteria criteria)
    {
        // Make (case-insensitive)
        if (!string.IsNullOrWhiteSpace(criteria.Make) &&
            !string.Equals(vehicle.Make, criteria.Make, StringComparison.OrdinalIgnoreCase))
            return false;

        // Model (case-insensitive)
        if (!string.IsNullOrWhiteSpace(criteria.Model) &&
            !string.Equals(vehicle.Model, criteria.Model, StringComparison.OrdinalIgnoreCase))
            return false;

        // Year range
        if (criteria.MinYear.HasValue && vehicle.Year < criteria.MinYear.Value)
            return false;
        if (criteria.MaxYear.HasValue && vehicle.Year > criteria.MaxYear.Value)
            return false;

        // Price range
        if (criteria.MinPrice.HasValue && vehicle.Price < criteria.MinPrice.Value)
            return false;
        if (criteria.MaxPrice.HasValue && vehicle.Price > criteria.MaxPrice.Value)
            return false;

        // Body type
        if (!string.IsNullOrWhiteSpace(criteria.BodyType) &&
            !string.Equals(vehicle.BodyType, criteria.BodyType, StringComparison.OrdinalIgnoreCase))
            return false;

        // Fuel type
        if (!string.IsNullOrWhiteSpace(criteria.FuelType) &&
            !string.Equals(vehicle.FuelType, criteria.FuelType, StringComparison.OrdinalIgnoreCase))
            return false;

        // Transmission
        if (!string.IsNullOrWhiteSpace(criteria.Transmission) &&
            !string.Equals(vehicle.Transmission, criteria.Transmission, StringComparison.OrdinalIgnoreCase))
            return false;

        // Location (city or state)
        if (!string.IsNullOrWhiteSpace(criteria.Location))
        {
            var locationMatch =
                string.Equals(vehicle.City, criteria.Location, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(vehicle.State, criteria.Location, StringComparison.OrdinalIgnoreCase);
            if (!locationMatch)
                return false;
        }

        // Max mileage
        if (criteria.MaxMileage.HasValue && vehicle.Mileage > criteria.MaxMileage.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Respect notification frequency: instant → always, daily → max once per day, weekly → max once per week.
    /// </summary>
    private static bool ShouldNotifyNow(SavedSearch search)
    {
        if (search.LastNotifiedAt == null) return true;

        var freq = search.NotificationFrequency?.ToLowerInvariant() ?? "instant";
        var elapsed = DateTime.UtcNow - search.LastNotifiedAt.Value;

        return freq switch
        {
            "instant" => true,
            "daily" => elapsed.TotalHours >= 24,
            "weekly" => elapsed.TotalDays >= 7,
            _ => true
        };
    }

    // ════════════════════════════════════════════════════════════════════
    // PRICE ALERT CHECK — Same vehicle re-listed at a new price
    // ════════════════════════════════════════════════════════════════════

    private async Task MatchPriceAlertsAsync(VehiclePublishedEvent vehicle, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var priceAlertRepo = scope.ServiceProvider.GetRequiredService<IPriceAlertRepository>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var userNotifService = scope.ServiceProvider.GetService<IUserNotificationService>();

        try
        {
            var alerts = await priceAlertRepo.GetActiveAlertsByVehicleIdAsync(vehicle.VehicleId);

            foreach (var alert in alerts)
            {
                try
                {
                    if (vehicle.Price > alert.TargetPrice) continue;

                    // Price meets or beats target!
                    var savings = alert.CurrentPrice - vehicle.Price;
                    var savingsPercent = alert.CurrentPrice > 0
                        ? Math.Round((savings / alert.CurrentPrice) * 100, 1)
                        : 0;

                    alert.MarkTriggered();
                    alert.UpdatePrice(vehicle.Price);
                    await priceAlertRepo.UpdateAsync(alert);

                    // Email notification
                    var userEmail = await ResolveUserEmailAsync(scope, alert.UserId);
                    if (!string.IsNullOrWhiteSpace(userEmail) && alert.NotifyByEmail)
                    {
                        await SendPriceAlertEmailAsync(emailService, userEmail, alert, vehicle, savings, savingsPercent);
                    }

                    // In-app notification
                    if (userNotifService != null)
                    {
                        await userNotifService.CreateAsync(
                            userId: alert.UserId,
                            type: "price_alert",
                            title: "💰 ¡Alerta de precio activada!",
                            message: $"{vehicle.Title} bajó a RD${vehicle.Price:N0} (ahorras RD${savings:N0})",
                            icon: "💰",
                            link: $"/vehiculos/{vehicle.Slug ?? vehicle.VehicleId.ToString()}",
                            cancellationToken: ct);
                    }

                    // Push notification via FCM topic (user-scoped topic pattern)
                    try
                    {
                        var pushService = scope.ServiceProvider.GetService<IPushNotificationService>();
                        if (pushService != null)
                        {
                            var pushData = new Dictionary<string, string>
                            {
                                ["type"] = "price_alert",
                                ["vehicleId"] = vehicle.VehicleId.ToString(),
                                ["slug"] = vehicle.Slug ?? "",
                                ["alertId"] = alert.Id.ToString(),
                                ["deepLink"] = $"/vehiculos/{vehicle.Slug ?? vehicle.VehicleId.ToString()}"
                            };
                            await pushService.SendToTopicAsync(
                                topic: $"user_{alert.UserId}_alerts",
                                title: "💰 ¡Alerta de precio activada!",
                                message: $"{vehicle.Title} bajó a RD${vehicle.Price:N0}",
                                customData: pushData);
                        }
                    }
                    catch (Exception pushEx)
                    {
                        _logger.LogWarning(pushEx, "Push notification failed for PriceAlert {AlertId}, User {UserId}",
                            alert.Id, alert.UserId);
                    }

                    _logger.LogInformation(
                        "✅ PriceAlert {AlertId} triggered! Vehicle {VehicleId} now RD${NewPrice} (target was RD${Target}). User {UserId} notified.",
                        alert.Id, vehicle.VehicleId, vehicle.Price, alert.TargetPrice, alert.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing PriceAlert {AlertId}", alert.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to match PriceAlerts for Vehicle {VehicleId}", vehicle.VehicleId);
        }
    }

    // ════════════════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════════════════

    private async Task<string?> ResolveUserEmailAsync(IServiceScope scope, Guid userId)
    {
        try
        {
            var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();
            if (httpClientFactory != null)
            {
                var client = httpClientFactory.CreateClient("UserService");
                // Add timeout to prevent slow UserService from blocking the matching pipeline
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var response = await client.GetAsync($"/api/users/{userId}/email", cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve email for UserId: {UserId}", userId);
        }
        return null;
    }

    private async Task SendMatchEmailAsync(
        IEmailService emailService,
        string email,
        SavedSearch search,
        VehiclePublishedEvent vehicle)
    {
        var vehicleUrl = $"https://okla.com.do/vehiculos/{vehicle.Slug ?? vehicle.VehicleId.ToString()}";
        var subject = $"🔔 ¡Nuevo vehículo coincide con \"{search.Name}\"!";

        var imageHtml = !string.IsNullOrWhiteSpace(vehicle.ImageUrl)
            ? $"<img src=\"{vehicle.ImageUrl}\" alt=\"{vehicle.Title}\" style=\"width:100%;max-height:300px;object-fit:cover;border-radius:8px;margin-bottom:20px;\" />"
            : "";

        var body = $@"
<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""><title>Alerta de Búsqueda - OKLA</title></head>
<body style=""font-family:Arial,sans-serif;line-height:1.6;color:#333;margin:0;padding:0;background-color:#f5f5f5;"">
<div style=""max-width:600px;margin:0 auto;padding:20px;"">
    <div style=""background:linear-gradient(135deg,#007bff,#0056b3);color:white;padding:30px;text-align:center;border-radius:10px 10px 0 0;"">
        <h1 style=""margin:0;font-size:24px;"">🔔 ¡Nuevo vehículo encontrado!</h1>
        <p style=""margin:10px 0 0;opacity:0.9;"">Coincide con tu búsqueda guardada: &quot;{search.Name}&quot;</p>
    </div>
    <div style=""background:white;padding:30px;border-radius:0 0 10px 10px;box-shadow:0 2px 10px rgba(0,0,0,0.1);"">
        {imageHtml}
        <h2 style=""color:#2c3e50;margin-top:0;"">{vehicle.Make} {vehicle.Model} {vehicle.Year}</h2>
        <div style=""background-color:#e3f2fd;padding:20px;border-radius:8px;margin:20px 0;border-left:4px solid #2196f3;"">
            <p style=""margin:0;color:#1565c0;font-weight:bold;font-size:28px;"">RD${vehicle.Price:N0}</p>
        </div>
        <table style=""width:100%;border-collapse:collapse;margin:20px 0;"">
            <tr><td style=""padding:8px 0;color:#666;"">Marca / Modelo:</td><td style=""padding:8px 0;text-align:right;font-weight:bold;"">{vehicle.Make} {vehicle.Model}</td></tr>
            <tr><td style=""padding:8px 0;color:#666;"">Año:</td><td style=""padding:8px 0;text-align:right;"">{vehicle.Year}</td></tr>
            <tr><td style=""padding:8px 0;color:#666;"">Kilometraje:</td><td style=""padding:8px 0;text-align:right;"">{vehicle.Mileage:N0} km</td></tr>
            {(!string.IsNullOrWhiteSpace(vehicle.FuelType) ? $"<tr><td style=\"padding:8px 0;color:#666;\">Combustible:</td><td style=\"padding:8px 0;text-align:right;\">{vehicle.FuelType}</td></tr>" : "")}
            {(!string.IsNullOrWhiteSpace(vehicle.Transmission) ? $"<tr><td style=\"padding:8px 0;color:#666;\">Transmisión:</td><td style=\"padding:8px 0;text-align:right;\">{vehicle.Transmission}</td></tr>" : "")}
            {(!string.IsNullOrWhiteSpace(vehicle.City) ? $"<tr><td style=\"padding:8px 0;color:#666;\">Ubicación:</td><td style=\"padding:8px 0;text-align:right;\">{vehicle.City}{(!string.IsNullOrWhiteSpace(vehicle.State) ? $", {vehicle.State}" : "")}</td></tr>" : "")}
        </table>
        <div style=""text-align:center;margin:30px 0;"">
            <a href=""{vehicleUrl}"" style=""background-color:#007bff;color:white;padding:14px 32px;text-decoration:none;border-radius:8px;font-size:16px;font-weight:bold;display:inline-block;"">
                Ver Vehículo
            </a>
        </div>
        <p style=""color:#999;font-size:12px;text-align:center;margin-top:30px;border-top:1px solid #eee;padding-top:20px;"">
            Recibiste este correo porque configuraste la búsqueda guardada &quot;{search.Name}&quot; en OKLA.
            <br>Puedes desactivar esta alerta desde tu panel de búsquedas guardadas.
        </p>
    </div>
    <div style=""text-align:center;padding:20px;color:#999;font-size:11px;"">
        &copy; {DateTime.UtcNow.Year} OKLA — Marketplace de Vehículos en República Dominicana
    </div>
</div>
</body>
</html>";

        await emailService.SendEmailAsync(to: email, subject: subject, body: body, isHtml: true);
    }

    private async Task SendPriceAlertEmailAsync(
        IEmailService emailService,
        string email,
        PriceAlert alert,
        VehiclePublishedEvent vehicle,
        decimal savings,
        decimal savingsPercent)
    {
        var vehicleUrl = $"https://okla.com.do/vehiculos/{vehicle.Slug ?? vehicle.VehicleId.ToString()}";
        var subject = $"🔔 ¡Bajó de precio! {vehicle.Title} — RD${vehicle.Price:N0}";

        var body = $@"
<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""><title>Alerta de Precio - OKLA</title></head>
<body style=""font-family:Arial,sans-serif;line-height:1.6;color:#333;margin:0;padding:0;background-color:#f5f5f5;"">
<div style=""max-width:600px;margin:0 auto;padding:20px;"">
    <div style=""background:linear-gradient(135deg,#007bff,#0056b3);color:white;padding:30px;text-align:center;border-radius:10px 10px 0 0;"">
        <h1 style=""margin:0;font-size:24px;"">🔔 ¡Alerta de Precio!</h1>
        <p style=""margin:10px 0 0;opacity:0.9;"">El vehículo que estás siguiendo bajó de precio</p>
    </div>
    <div style=""background:white;padding:30px;border-radius:0 0 10px 10px;box-shadow:0 2px 10px rgba(0,0,0,0.1);"">
        <h2 style=""color:#2c3e50;margin-top:0;"">{vehicle.Title}</h2>
        <div style=""background-color:#e8f5e9;padding:20px;border-radius:8px;margin:20px 0;border-left:4px solid #4caf50;"">
            <p style=""margin:0;color:#999;text-decoration:line-through;font-size:18px;"">RD${alert.CurrentPrice:N0}</p>
            <p style=""margin:5px 0 0;color:#4caf50;font-weight:bold;font-size:28px;"">RD${vehicle.Price:N0}</p>
            <p style=""margin:10px 0 0;color:#4caf50;font-weight:bold;"">¡Ahorras RD${savings:N0} ({savingsPercent}% menos)!</p>
        </div>
        <table style=""width:100%;border-collapse:collapse;margin:20px 0;"">
            <tr><td style=""padding:8px 0;color:#666;"">Tu precio objetivo:</td><td style=""padding:8px 0;font-weight:bold;text-align:right;"">RD${alert.TargetPrice:N0}</td></tr>
        </table>
        <div style=""text-align:center;margin:30px 0;"">
            <a href=""{vehicleUrl}"" style=""background-color:#4caf50;color:white;padding:14px 32px;text-decoration:none;border-radius:8px;font-size:16px;font-weight:bold;display:inline-block;"">
                Ver Vehículo
            </a>
        </div>
        <p style=""color:#999;font-size:12px;text-align:center;margin-top:30px;border-top:1px solid #eee;padding-top:20px;"">
            Recibiste este correo porque configuraste una alerta de precio en OKLA.
            <br>Puedes desactivar esta alerta desde tu panel de alertas.
        </p>
    </div>
    <div style=""text-align:center;padding:20px;color:#999;font-size:11px;"">
        &copy; {DateTime.UtcNow.Year} OKLA — Marketplace de Vehículos en República Dominicana
    </div>
</div>
</body>
</html>";

        await emailService.SendEmailAsync(to: email, subject: subject, body: body, isHtml: true);
    }

    // ════════════════════════════════════════════════════════════════════
    // RABBITMQ SETUP
    // ════════════════════════════════════════════════════════════════════

    private void InitializeRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"]
                    ?? _configuration["RabbitMQ:HostName"]
                    ?? "localhost",
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:Username"]
                    ?? _configuration["RabbitMQ:UserName"]
                    ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest",
                VirtualHost = _configuration["RabbitMQ:VirtualHost"] ?? "/",
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                ClientProvidedName = $"notificationservice-vehicle-alert-matcher-{Environment.MachineName}"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange
            _channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            // Declare queue with DLX
            var queueArgs = new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = $"{ExchangeName}.dlx",
                ["x-dead-letter-routing-key"] = $"dlx.{RoutingKey}",
                ["x-message-ttl"] = 86400000 // 24h TTL
            };

            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArgs);

            _channel.QueueBind(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: RoutingKey);

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _logger.LogInformation("RabbitMQ initialized for VehiclePublishedAlertMatcherConsumer");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ for VehiclePublishedAlertMatcherConsumer");
            throw;
        }
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
            _logger.LogError(ex, "Error disposing RabbitMQ connection for VehiclePublishedAlertMatcherConsumer");
        }

        base.Dispose();
    }
}
