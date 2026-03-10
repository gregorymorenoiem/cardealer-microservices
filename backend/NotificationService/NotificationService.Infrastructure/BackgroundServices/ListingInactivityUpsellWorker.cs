using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Infrastructure.BackgroundServices;

/// <summary>
/// Daily background worker that detects vehicles published for 45+ days without selling
/// and sends the dealer a personalized suggestion to activate a Featured Listing ($6/month).
///
/// Flow:
///   1. Every 24 hours, wakes up
///   2. GET VehiclesSaleService /api/internal/vehicles/stale?daysOld=45 → list of stale vehicles
///   3. Group by seller, build personalized email per vehicle
///   4. Render FeaturedListingSuggestion.html with vehicle stats + impact projections
///   5. Send email via IEmailService
///
/// Safety:
///   - Tracks already-notified vehicle IDs (in-memory, resets on restart)
///   - Only sends ONE suggestion per vehicle per cycle
///   - Gracefully handles VehiclesSaleService unavailability
/// </summary>
public class ListingInactivityUpsellWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ListingInactivityUpsellWorker> _logger;
    private readonly IConfiguration _configuration;

    // Vehicles already suggested (prevents duplicates within a pod lifecycle)
    private readonly HashSet<Guid> _notifiedVehicles = new();
    private readonly object _notifiedLock = new();

    // ── Configuration Constants ──
    private const int StaleDaysThreshold = 45;
    private const decimal FeaturedPricePerMonth = 6m;
    private const string FeaturedViewsMultiplier = "4.2"; // Average observed boost
    private const string AvgDaysToSellFeatured = "12"; // Average days to sell when featured

    public ListingInactivityUpsellWorker(
        IServiceProvider serviceProvider,
        ILogger<ListingInactivityUpsellWorker> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial delay: wait 3 minutes for services to stabilize
        await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);

        _logger.LogInformation(
            "⭐ ListingInactivityUpsellWorker started. Threshold={Days} days, Price=US${Price}/mo",
            StaleDaysThreshold, FeaturedPricePerMonth);

        var interval = TimeSpan.FromHours(
            _configuration.GetValue("InactivityWorker:IntervalHours", 24));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunInactivityCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in listing inactivity cycle. Will retry in {Interval}", interval);
            }

            await Task.Delay(interval, stoppingToken);
        }

        _logger.LogInformation("ListingInactivityUpsellWorker stopped.");
    }

    private async Task RunInactivityCycleAsync(CancellationToken ct)
    {
        _logger.LogInformation("🔍 Running listing inactivity detection cycle...");

        using var scope = _serviceProvider.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();
        if (httpClientFactory == null)
        {
            _logger.LogWarning("IHttpClientFactory not available. Skipping inactivity cycle.");
            return;
        }

        // ── 1. Fetch stale listings from VehiclesSaleService ────────────────
        var staleListings = await FetchStaleListingsAsync(httpClientFactory, ct);
        if (staleListings.Count == 0)
        {
            _logger.LogInformation("No stale listings found (>{Days} days). Skipping cycle.", StaleDaysThreshold);
            return;
        }

        _logger.LogInformation("Found {Count} stale listings to process", staleListings.Count);

        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var templateEngine = scope.ServiceProvider.GetRequiredService<ITemplateEngine>();

        int processed = 0, sent = 0, skipped = 0;
        var baseUrl = _configuration["App:BaseUrl"] ?? "https://okla.com.do";

        foreach (var vehicle in staleListings)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                processed++;

                // Skip already-notified vehicles
                lock (_notifiedLock)
                {
                    if (_notifiedVehicles.Contains(vehicle.Id))
                    {
                        skipped++;
                        continue;
                    }
                }

                // Skip if no seller email
                if (string.IsNullOrWhiteSpace(vehicle.SellerEmail))
                {
                    _logger.LogDebug("Vehicle {VehicleId} has no seller email. Skipping.", vehicle.Id);
                    skipped++;
                    continue;
                }

                // ── 2. Build template parameters ─────────────────────────────
                var templateParams = new Dictionary<string, object>
                {
                    ["DealerName"] = vehicle.SellerName ?? "Dealer",
                    ["DaysPublished"] = vehicle.DaysSincePublished.ToString(),
                    ["VehicleTitle"] = vehicle.Title ?? $"{vehicle.Year} {vehicle.Make} {vehicle.Model}",
                    ["VehicleYear"] = vehicle.Year.ToString(),
                    ["VehicleMake"] = vehicle.Make ?? "",
                    ["VehicleModel"] = vehicle.Model ?? "",
                    ["VehiclePrice"] = vehicle.Price > 0 ? $"US${vehicle.Price:N0}" : "—",
                    ["CurrentViews"] = vehicle.ViewCount.ToString("N0"),
                    ["CurrentInquiries"] = vehicle.InquiryCount.ToString(),
                    ["CurrentFavorites"] = vehicle.FavoriteCount.ToString(),
                    ["ProjectedViews"] = FeaturedViewsMultiplier,
                    ["AvgDaysToSellFeatured"] = AvgDaysToSellFeatured,
                    ["FeaturedPrice"] = FeaturedPricePerMonth.ToString("F0"),
                    ["FeatureUrl"] = $"{baseUrl}/dealer/inventario/{vehicle.Id}/destacar",
                    ["Year"] = DateTime.UtcNow.Year.ToString(),
                    ["UnsubscribeUrl"] = $"{baseUrl}/dealer/configuracion/notificaciones"
                };

                // ── 3. Render email template ─────────────────────────────────
                string emailBody;
                try
                {
                    emailBody = await templateEngine.RenderTemplateAsync(
                        "EmailTemplates/FeaturedListingSuggestion.html", templateParams);
                }
                catch (FileNotFoundException)
                {
                    _logger.LogWarning("FeaturedListingSuggestion.html not found. Using inline fallback.");
                    emailBody = BuildFallbackHtml(vehicle);
                }

                // ── 4. Send email ────────────────────────────────────────────
                await emailService.SendEmailAsync(
                    to: vehicle.SellerEmail,
                    subject: $"⭐ Tu {vehicle.Year} {vehicle.Make} {vehicle.Model} lleva {vehicle.DaysSincePublished} días — ¡Destácalo por US${FeaturedPricePerMonth}/mes!",
                    body: emailBody,
                    isHtml: true);

                sent++;

                lock (_notifiedLock)
                {
                    _notifiedVehicles.Add(vehicle.Id);
                }

                _logger.LogInformation(
                    "✅ Featured suggestion sent for Vehicle={VehicleId} ({VehicleTitle}) to {Email}. Days={Days}",
                    vehicle.Id, vehicle.Title, vehicle.SellerEmail, vehicle.DaysSincePublished);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing featured suggestion for vehicle {VehicleId}", vehicle.Id);
            }
        }

        _logger.LogInformation(
            "📊 Inactivity cycle complete. Processed={Processed}, Sent={Sent}, Skipped={Skipped}",
            processed, sent, skipped);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HELPER: Fetch stale listings from VehiclesSaleService
    // ═══════════════════════════════════════════════════════════════════════

    private async Task<List<StaleVehicleInfo>> FetchStaleListingsAsync(
        IHttpClientFactory httpClientFactory, CancellationToken ct)
    {
        var result = new List<StaleVehicleInfo>();

        try
        {
            var client = httpClientFactory.CreateClient("VehiclesSaleService");
            var response = await client.GetAsync(
                $"/api/internal/vehicles/stale?daysOld={StaleDaysThreshold}&take=200", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "VehiclesSaleService returned {StatusCode} for stale listings.", response.StatusCode);
                return result;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var listings = JsonSerializer.Deserialize<List<StaleVehicleInfo>>(json, options);

            return listings ?? result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Cannot reach VehiclesSaleService. Will retry next cycle.");
            return result;
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "VehiclesSaleService request timed out. Will retry next cycle.");
            return result;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // FALLBACK: Inline HTML if template is missing
    // ═══════════════════════════════════════════════════════════════════════

    private static string BuildFallbackHtml(StaleVehicleInfo v)
    {
        return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; color: #333; padding: 20px;'>
<h2 style='color: #f59e0b;'>⭐ Tu vehículo merece más visibilidad</h2>
<p>Hola {v.SellerName ?? "Dealer"},</p>
<p>Tu <strong>{v.Year} {v.Make} {v.Model}</strong> lleva <strong>{v.DaysSincePublished} días</strong> publicado.</p>
<p>Con un <strong>Listing Destacado (US$6/mes)</strong> podrías obtener hasta <strong>4.2x más vistas</strong> y venderlo en un promedio de <strong>12 días</strong>.</p>
<p style='margin-top: 20px;'>
  <a href='https://okla.com.do/dealer/inventario/{v.Id}/destacar'
     style='background: #f59e0b; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold;'>
    ⭐ Destacar por US$6/mes
  </a>
</p>
<p style='color: #9ca3af; font-size: 12px; margin-top: 30px;'>
  &copy; {DateTime.UtcNow.Year} OKLA Marketplace · Santo Domingo, RD
</p>
</body>
</html>";
    }

    // ═══════════════════════════════════════════════════════════════════════
    // DTO: Stale vehicle from VehiclesSaleService internal API
    // ═══════════════════════════════════════════════════════════════════════

    private sealed class StaleVehicleInfo
    {
        public Guid Id { get; set; }
        public Guid SellerId { get; set; }
        public Guid DealerId { get; set; }
        public string? Title { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }
        public string? Currency { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int DaysSincePublished { get; set; }
        public int ViewCount { get; set; }
        public int InquiryCount { get; set; }
        public int FavoriteCount { get; set; }
        public string? SellerName { get; set; }
        public string? SellerEmail { get; set; }
        public string? PrimaryImageUrl { get; set; }
    }
}
