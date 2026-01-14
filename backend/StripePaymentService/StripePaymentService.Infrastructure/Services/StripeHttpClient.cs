using System.Text;
using System.Text.Json;
using Serilog;

namespace StripePaymentService.Infrastructure.Services;

/// <summary>
/// HTTP Client para comunicación con Stripe API
/// </summary>
public class StripeHttpClient
{
    private const string StripeBaseUrl = "https://api.stripe.com/v1";
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<StripeHttpClient> _logger;

    public StripeHttpClient(
        HttpClient httpClient,
        string apiKey,
        ILogger<StripeHttpClient> logger)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new ArgumentNullException(nameof(apiKey));

        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configurar defaults
        _httpClient.BaseAddress = new Uri(StripeBaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    /// <summary>
    /// Crear un Payment Intent
    /// </summary>
    public async Task<PaymentIntentResponse?> CreatePaymentIntentAsync(CreatePaymentIntentRequest request, CancellationToken cancellationToken = default)
    {
        _logger.Information("Creando Payment Intent en Stripe. Amount: {Amount}", request.Amount);

        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "amount", request.Amount.ToString() },
                { "currency", request.Currency },
                { "description", request.Description ?? "" },
                { "customer_email", request.CustomerEmail ?? "" },
                { "metadata[customer_name]", request.CustomerName ?? "" },
                { "metadata[phone]", request.CustomerPhone ?? "" },
                { "off_session", (request.OffSession ?? false).ToString().ToLower() },
                { "automatic_payment_methods[enabled]", "true" }
            });

            var response = await _httpClient.PostAsync("/payment_intents", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("Error creando Payment Intent: {StatusCode} - {Response}", response.StatusCode, responseContent);
                return null;
            }

            var paymentIntent = JsonSerializer.Deserialize<PaymentIntentResponse>(responseContent);
            _logger.Information("Payment Intent creado: {PaymentIntentId}", paymentIntent?.Id);

            return paymentIntent;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Excepción creando Payment Intent");
            throw;
        }
    }

    /// <summary>
    /// Confirmar un Payment Intent
    /// </summary>
    public async Task<PaymentIntentResponse?> ConfirmPaymentIntentAsync(string paymentIntentId, string paymentMethodId, CancellationToken cancellationToken = default)
    {
        _logger.Information("Confirmando Payment Intent: {PaymentIntentId}", paymentIntentId);

        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "payment_method", paymentMethodId },
                { "return_url", "https://api.okla.com.do/api/payments/return" }
            });

            var response = await _httpClient.PostAsync($"/payment_intents/{paymentIntentId}/confirm", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("Error confirmando Payment Intent: {StatusCode}", response.StatusCode);
                return null;
            }

            return JsonSerializer.Deserialize<PaymentIntentResponse>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Excepción confirmando Payment Intent: {PaymentIntentId}", paymentIntentId);
            throw;
        }
    }

    /// <summary>
    /// Crear una subscripción
    /// </summary>
    public async Task<SubscriptionResponse?> CreateSubscriptionAsync(CreateSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.Information("Creando Subscripción para customer: {CustomerId}", request.CustomerId);

        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "customer", request.CustomerId },
                { "items[0][price]", request.PriceId },
                { "trial_period_days", (request.TrialDays ?? 0).ToString() },
                { "payment_behavior", "default_incomplete" },
                { "payment_settings[save_default_payment_method]", "on_subscription" }
            });

            var response = await _httpClient.PostAsync("/subscriptions", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("Error creando subscripción: {StatusCode}", response.StatusCode);
                return null;
            }

            return JsonSerializer.Deserialize<SubscriptionResponse>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Excepción creando subscripción");
            throw;
        }
    }

    /// <summary>
    /// Cancelar una subscripción
    /// </summary>
    public async Task<SubscriptionResponse?> CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        _logger.Information("Cancelando subscripción: {SubscriptionId}", subscriptionId);

        try
        {
            var response = await _httpClient.DeleteAsync($"/subscriptions/{subscriptionId}", cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("Error cancelando subscripción: {StatusCode}", response.StatusCode);
                return null;
            }

            return JsonSerializer.Deserialize<SubscriptionResponse>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Excepción cancelando subscripción: {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    // DTOs para Stripe API responses
    public class PaymentIntentResponse
    {
        public string? Id { get; set; }
        public string? ClientSecret { get; set; }
        public string? Status { get; set; }
        public long Amount { get; set; }
        public string? Currency { get; set; }
        public bool RequiresAction { get; set; }
    }

    public class SubscriptionResponse
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
        public long CurrentPeriodStart { get; set; }
        public long CurrentPeriodEnd { get; set; }
    }

    public class CreatePaymentIntentRequest
    {
        public long Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string? Description { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public bool? OffSession { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class CreateSubscriptionRequest
    {
        public string CustomerId { get; set; } = "";
        public string PriceId { get; set; } = "";
        public int? TrialDays { get; set; }
        public string? Currency { get; set; }
    }
}
