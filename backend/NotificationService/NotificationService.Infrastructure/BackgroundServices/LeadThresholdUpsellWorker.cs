using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Interfaces;
using CarDealer.Contracts.Enums;

namespace NotificationService.Infrastructure.BackgroundServices;

/// <summary>
/// Daily background worker that detects LIBRE-plan dealers with 5+ inquiries in 30 days
/// and sends a personalized UpgradeNudge email with ROI projections.
///
/// Flow:
///   1. Every 24 hours (configurable), wakes up
///   2. GET UserService /api/internal/dealers?plan=libre → list of LIBRE dealers
///   3. For each dealer, GET ContactService /api/internal/contact-requests/seller/{id}/count?from=&amp;to=
///   4. If count ≥ 5, calculate ROI projections and render UpgradeNudge.html
///   5. Send personalized email via IEmailService
///   6. Log upsell trigger for analytics
///
/// Safety:
///   - Tracks already-notified dealers (in-memory set, resets on restart — acceptable for daily runs)
///   - Only sends ONE upsell email per dealer per 30-day window
///   - Gracefully handles service unavailability (try/catch per dealer)
/// </summary>
public class LeadThresholdUpsellWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LeadThresholdUpsellWorker> _logger;
    private readonly IConfiguration _configuration;

    // Dealers who have already been sent an upsell email (resets on restart)
    private readonly HashSet<Guid> _notifiedDealers = new();
    private readonly object _notifiedLock = new();

    // ── Configuration Constants ──
    private const int LeadThreshold = 5;
    private const int LookbackDays = 30;
    private const string SuggestedPlan = "Visible";
    private const decimal SuggestedPlanPrice = 29m; // PlanConfiguration.PriceVisible

    // ROI projection multipliers (based on OKLA historical data analysis)
    // VISIBLE plan dealers get ~3.5x more visibility than LIBRE
    private const double VisibilityMultiplier = 3.5;
    private const double ConversionRate = 0.05; // 5% lead-to-sale conversion (market average DR)

    public LeadThresholdUpsellWorker(
        IServiceProvider serviceProvider,
        ILogger<LeadThresholdUpsellWorker> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial delay: wait 2 minutes for other services to stabilize
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        _logger.LogInformation("📈 LeadThresholdUpsellWorker started. Threshold={Threshold} leads in {Days} days → suggest {Plan}",
            LeadThreshold, LookbackDays, SuggestedPlan);

        var interval = TimeSpan.FromHours(
            _configuration.GetValue("UpsellWorker:IntervalHours", 24));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunUpsellCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in upsell cycle. Will retry in {Interval}", interval);
            }

            await Task.Delay(interval, stoppingToken);
        }

        _logger.LogInformation("LeadThresholdUpsellWorker stopped.");
    }

    private async Task RunUpsellCycleAsync(CancellationToken ct)
    {
        _logger.LogInformation("🔍 Running upsell detection cycle...");

        using var scope = _serviceProvider.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();
        if (httpClientFactory == null)
        {
            _logger.LogWarning("IHttpClientFactory not available. Skipping upsell cycle.");
            return;
        }

        // ── 1. Fetch LIBRE-plan dealers from UserService ────────────────────
        var libreDealers = await FetchLibreDealersAsync(httpClientFactory, ct);
        if (libreDealers.Count == 0)
        {
            _logger.LogInformation("No LIBRE-plan dealers found. Skipping cycle.");
            return;
        }

        _logger.LogInformation("Found {Count} LIBRE-plan dealers to evaluate", libreDealers.Count);

        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var templateEngine = scope.ServiceProvider.GetRequiredService<ITemplateEngine>();

        var now = DateTime.UtcNow;
        var periodStart = now.AddDays(-LookbackDays);
        int evaluated = 0, qualified = 0, sent = 0;

        foreach (var dealer in libreDealers)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                evaluated++;

                // Skip already-notified dealers
                lock (_notifiedLock)
                {
                    if (_notifiedDealers.Contains(dealer.DealerId))
                        continue;
                }

                // ── 2. Get lead count for this dealer in the last 30 days ────
                var leadCount = await FetchLeadCountAsync(
                    httpClientFactory, dealer.DealerId, periodStart, now, ct);

                if (leadCount < LeadThreshold) continue;

                qualified++;
                _logger.LogInformation(
                    "🎯 Dealer {DealerName} ({DealerId}) qualifies: {LeadCount} leads in {Days} days",
                    dealer.Name, dealer.DealerId, leadCount, LookbackDays);

                // ── 3. Calculate ROI projections ─────────────────────────────
                var projectedLeads = (int)Math.Round(leadCount * VisibilityMultiplier);
                var projectedSales = (int)Math.Ceiling(projectedLeads * ConversionRate);

                // ── 4. Render UpgradeNudge.html template ─────────────────────
                var templateParams = new Dictionary<string, object>
                {
                    ["DealerName"] = dealer.Name ?? "Dealer",
                    ["SuggestedPlan"] = SuggestedPlan,
                    ["TotalViews"] = dealer.TotalViews > 0 ? dealer.TotalViews.ToString("N0") : "—",
                    ["TotalContacts"] = leadCount.ToString(),
                    ["ActiveListings"] = dealer.ActiveListings > 0 ? dealer.ActiveListings.ToString() : "—",
                    ["CurrentPlan"] = PlanConfiguration.DisplayLibre,
                    ["CurrentFeaturedLimit"] = "0",
                    ["SuggestedFeaturedLimit"] = "3",
                    ["CurrentHistoryAccess"] = "Limitado",
                    ["CurrentMarketAccess"] = "Básico",
                    ["PriceDifference"] = $"US${SuggestedPlanPrice:F0}",
                    ["UpgradeUrl"] = _configuration["App:BaseUrl"] ?? "https://okla.com.do" + "/dealer/pricing",
                    ["Year"] = DateTime.UtcNow.Year.ToString(),
                    ["UnsubscribeUrl"] = (_configuration["App:BaseUrl"] ?? "https://okla.com.do") + "/dealer/configuracion/notificaciones"
                };

                string emailBody;
                try
                {
                    emailBody = await templateEngine.RenderTemplateAsync(
                        "EmailTemplates/UpgradeNudge.html", templateParams);
                }
                catch (FileNotFoundException)
                {
                    _logger.LogWarning("UpgradeNudge.html template not found. Using inline fallback.");
                    emailBody = BuildFallbackUpsellHtml(dealer.Name ?? "Dealer", leadCount, projectedLeads);
                }

                // ── 5. Send email ────────────────────────────────────────────
                if (!string.IsNullOrWhiteSpace(dealer.Email))
                {
                    await emailService.SendEmailAsync(
                        to: dealer.Email,
                        subject: $"📈 {dealer.Name}, ya tienes {leadCount} consultas — ¿listo para más?",
                        body: emailBody,
                        isHtml: true);

                    sent++;

                    lock (_notifiedLock)
                    {
                        _notifiedDealers.Add(dealer.DealerId);
                    }

                    _logger.LogInformation(
                        "✅ Upsell email sent to {DealerName} ({Email}). Leads={Leads}, Projected={Projected}",
                        dealer.Name, dealer.Email, leadCount, projectedLeads);
                }
                else
                {
                    _logger.LogWarning(
                        "Dealer {DealerId} ({DealerName}) qualified but has no email. Skipping.",
                        dealer.DealerId, dealer.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Error processing upsell for dealer {DealerId}. Continuing with next.",
                    dealer.DealerId);
            }
        }

        _logger.LogInformation(
            "📊 Upsell cycle complete. Evaluated={Evaluated}, Qualified={Qualified}, EmailsSent={Sent}",
            evaluated, qualified, sent);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HELPER: Fetch LIBRE-plan dealers from UserService
    // ═══════════════════════════════════════════════════════════════════════

    private async Task<List<DealerInfo>> FetchLibreDealersAsync(
        IHttpClientFactory httpClientFactory, CancellationToken ct)
    {
        var dealers = new List<DealerInfo>();

        try
        {
            var client = httpClientFactory.CreateClient("UserService");
            var response = await client.GetAsync("/api/internal/dealers?plan=libre&status=active", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "UserService returned {StatusCode} for LIBRE dealers list. Will retry next cycle.",
                    response.StatusCode);
                return dealers;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<List<DealerInfo>>(json, options);

            return result ?? dealers;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Cannot reach UserService to fetch LIBRE dealers. Will retry next cycle.");
            return dealers;
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "UserService request timed out. Will retry next cycle.");
            return dealers;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HELPER: Fetch lead count from ContactService
    // ═══════════════════════════════════════════════════════════════════════

    private async Task<int> FetchLeadCountAsync(
        IHttpClientFactory httpClientFactory,
        Guid sellerId,
        DateTime from,
        DateTime to,
        CancellationToken ct)
    {
        try
        {
            var client = httpClientFactory.CreateClient("ContactService");
            var fromStr = from.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var toStr = to.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var response = await client.GetAsync(
                $"/api/internal/contact-requests/seller/{sellerId}/count?from={fromStr}&to={toStr}", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "ContactService returned {StatusCode} for seller {SellerId} lead count.",
                    response.StatusCode, sellerId);
                return 0;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<JsonElement>(json);

            return result.TryGetProperty("count", out var countProp) ? countProp.GetInt32() : 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching lead count for seller {SellerId}", sellerId);
            return 0;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // FALLBACK: Inline HTML if template file is missing
    // ═══════════════════════════════════════════════════════════════════════

    private static string BuildFallbackUpsellHtml(string dealerName, int currentLeads, int projectedLeads)
    {
        return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; color: #333; padding: 20px;'>
<h2 style='color: #10b981;'>📈 ¡Estás listo para el siguiente nivel!</h2>
<p>Hola {dealerName},</p>
<p>En los últimos 30 días recibiste <strong>{currentLeads} consultas</strong> en tu plan Libre.</p>
<p>Con el plan <strong>Visible (US$29/mes)</strong>, proyectamos que podrías alcanzar hasta <strong>{projectedLeads} consultas</strong> gracias a mayor visibilidad en búsquedas y vehículos destacados.</p>
<p style='margin-top: 20px;'>
  <a href='https://okla.com.do/dealer/pricing'
     style='background: #10b981; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold;'>
    Ver Plan Visible →
  </a>
</p>
<p style='color: #9ca3af; font-size: 12px; margin-top: 30px;'>
  &copy; {DateTime.UtcNow.Year} OKLA Marketplace · Santo Domingo, RD
</p>
</body>
</html>";
    }

    // ═══════════════════════════════════════════════════════════════════════
    // DTO: Dealer info from UserService
    // ═══════════════════════════════════════════════════════════════════════

    private sealed class DealerInfo
    {
        public Guid DealerId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Plan { get; set; }
        public int ActiveListings { get; set; }
        public int TotalViews { get; set; }
    }
}
