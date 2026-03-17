using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BillingService.Domain.Interfaces;
using System.Collections.Concurrent;

namespace BillingService.Api.Controllers;

/// <summary>
/// Handles inline checkout sessions and Stripe PaymentIntent creation
/// for the frontend Elements-based payment flow.
/// </summary>
[ApiController]
[Route("api/checkout")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly IStripeService _stripeService;
    private readonly ILogger<CheckoutController> _logger;

    // In-memory session store (single replica). For multi-replica, move to Redis.
    private static readonly ConcurrentDictionary<string, CheckoutSessionData> _sessions = new();

    public CheckoutController(
        IStripeService stripeService,
        ILogger<CheckoutController> logger)
    {
        _stripeService = stripeService;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/checkout/sessions — Create a checkout session with server-side pricing.
    /// </summary>
    [HttpPost("sessions")]
    public IActionResult CreateSession([FromBody] CreateSessionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductId))
            return BadRequest(new { error = "productId is required" });

        var product = GetProduct(request.ProductId);
        if (product == null)
            return BadRequest(new { error = $"Unknown product: {request.ProductId}" });

        var subtotal = product.Price;
        var tax = product.Currency == "DOP" ? (int)Math.Round(subtotal * 0.18m) : 0;
        var total = subtotal + tax;

        var sessionId = Guid.NewGuid().ToString("N");
        var session = new CheckoutSessionData
        {
            SessionId = sessionId,
            ProductId = request.ProductId,
            PaymentMethod = request.PaymentMethod ?? "stripe",
            Subtotal = subtotal,
            Tax = tax,
            Total = total,
            Currency = product.Currency,
            Status = "pending",
            UserId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "",
            CreatedAt = DateTime.UtcNow,
        };

        _sessions.TryAdd(sessionId, session);

        // Evict old sessions (>1 hour)
        var cutoff = DateTime.UtcNow.AddHours(-1);
        foreach (var kv in _sessions)
        {
            if (kv.Value.CreatedAt < cutoff)
                _sessions.TryRemove(kv.Key, out _);
        }

        _logger.LogInformation("Checkout session {SessionId} created for product {ProductId}, total {Total} {Currency}",
            sessionId, request.ProductId, total, product.Currency);

        return Ok(new
        {
            sessionId,
            productId = request.ProductId,
            subtotal,
            tax,
            total,
            currency = product.Currency,
            status = "pending",
        });
    }

    /// <summary>
    /// POST /api/checkout/sessions/{sessionId}/payment-intent — Create Stripe PaymentIntent.
    /// </summary>
    [HttpPost("sessions/{sessionId}/payment-intent")]
    public async Task<IActionResult> CreatePaymentIntent(string sessionId, CancellationToken ct)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            return NotFound(new { error = "Session not found or expired" });

        // Security: verify the session belongs to the current user
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        if (session.UserId != userId)
            return StatusCode(403, new { error = "Access denied" });

        if (session.Total <= 0)
            return BadRequest(new { error = "Cannot create payment intent for zero amount" });

        try
        {
            // Convert to cents for Stripe
            var amountCents = (long)(session.Total * 100);
            var currency = session.Currency.ToLowerInvariant();

            var result = await _stripeService.CreatePaymentIntentAsync(
                amountCents,
                currency,
                $"OKLA - {session.ProductId}",
                new Dictionary<string, string>
                {
                    ["session_id"] = sessionId,
                    ["product_id"] = session.ProductId,
                    ["user_id"] = session.UserId,
                },
                cancellationToken: ct);

            session.Status = "processing";
            session.StripePaymentIntentId = result.PaymentIntentId;

            return Ok(new
            {
                id = result.PaymentIntentId,
                clientSecret = result.ClientSecret,
                amount = session.Total,
                currency = session.Currency,
                status = result.Status,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create PaymentIntent for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to create payment intent" });
        }
    }

    /// <summary>
    /// GET /api/checkout/sessions/{sessionId} — Get session status.
    /// </summary>
    [HttpGet("sessions/{sessionId}")]
    public IActionResult GetSession(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            return NotFound(new { error = "Session not found or expired" });

        return Ok(new
        {
            session.SessionId,
            session.ProductId,
            session.Subtotal,
            session.Tax,
            session.Total,
            session.Currency,
            session.Status,
        });
    }

    // ======================================================================
    // Server-side product catalog (source of truth for pricing)
    // ======================================================================
    private static ProductInfo? GetProduct(string productId) => productId switch
    {
        "boost-basic" => new("boost-basic", 499, "DOP"),
        "boost-premium" => new("boost-premium", 1499, "DOP"),
        "dealer-libre" => new("dealer-libre", 0, "DOP"),
        "dealer-visible" => new("dealer-visible", 1699, "DOP"),
        "dealer-pro" => new("dealer-pro", 5199, "DOP"),
        "dealer-elite" => new("dealer-elite", 11599, "DOP"),
        "seller-gratis" => new("seller-gratis", 0, "DOP"),
        "seller-premium" => new("seller-premium", 1699, "DOP"),
        "seller-pro" => new("seller-pro", 3499, "DOP"),
        "listing-single" => new("listing-single", 29, "USD"),
        "okla-score-report" => new("okla-score-report", 420, "DOP"),
        _ => null,
    };

    private record ProductInfo(string Id, decimal Price, string Currency);

    private class CheckoutSessionData
    {
        public string SessionId { get; set; } = "";
        public string ProductId { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string Currency { get; set; } = "DOP";
        public string Status { get; set; } = "pending";
        public string UserId { get; set; } = "";
        public string? StripePaymentIntentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateSessionRequest
    {
        public string ProductId { get; set; } = "";
        public string? PaymentMethod { get; set; }
        public string? VehicleId { get; set; }
        public string? DealerId { get; set; }
        public string? PromoCode { get; set; }
        public string? ReturnUrl { get; set; }
        public string? CancelUrl { get; set; }
    }
}
