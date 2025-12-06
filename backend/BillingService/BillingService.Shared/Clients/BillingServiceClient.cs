using System.Net.Http.Json;
using System.Text.Json;
using BillingService.Shared.DTOs;
using Microsoft.Extensions.DependencyInjection;

namespace BillingService.Shared.Clients;

/// <summary>
/// Cliente HTTP para comunicarse con BillingService
/// </summary>
public interface IBillingServiceClient
{
    // Customer Operations
    Task<StripeCustomerResponse?> CreateCustomerAsync(CreateStripeCustomerRequest request, CancellationToken ct = default);
    Task<StripeCustomerResponse?> GetCustomerByDealerIdAsync(Guid dealerId, CancellationToken ct = default);

    // Subscription Operations
    Task<SubscriptionResponse?> CreateSubscriptionAsync(Guid dealerId, CreateSubscriptionRequest request, CancellationToken ct = default);
    Task<SubscriptionResponse?> GetSubscriptionAsync(Guid dealerId, CancellationToken ct = default);
    Task<SubscriptionResponse?> UpdateSubscriptionAsync(Guid subscriptionId, UpdateSubscriptionRequest request, CancellationToken ct = default);
    Task<bool> CancelSubscriptionAsync(Guid subscriptionId, CancelSubscriptionRequest request, CancellationToken ct = default);

    // Checkout & Portal
    Task<CheckoutSessionResponse?> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request, CancellationToken ct = default);
    Task<BillingPortalResponse?> CreateBillingPortalSessionAsync(CreateBillingPortalRequest request, CancellationToken ct = default);

    // Pricing
    Task<PricingResponse?> GetPricingAsync(CancellationToken ct = default);
}

/// <summary>
/// Implementación del cliente HTTP para BillingService
/// </summary>
public class BillingServiceClient : IBillingServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public BillingServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    // ========================================
    // CUSTOMER OPERATIONS
    // ========================================

    public async Task<StripeCustomerResponse?> CreateCustomerAsync(
        CreateStripeCustomerRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/billing/customers", request, _jsonOptions, ct);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<StripeCustomerResponse>(_jsonOptions, ct);
        }
        catch
        {
            return null;
        }
    }

    public async Task<StripeCustomerResponse?> GetCustomerByDealerIdAsync(
        Guid dealerId,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/billing/customers/by-dealer/{dealerId}", ct);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<StripeCustomerResponse>(_jsonOptions, ct);
        }
        catch
        {
            return null;
        }
    }

    // ========================================
    // SUBSCRIPTION OPERATIONS
    // ========================================

    public async Task<SubscriptionResponse?> CreateSubscriptionAsync(
        Guid dealerId,
        CreateSubscriptionRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/billing/subscriptions", request, _jsonOptions, ct);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<SubscriptionResponse>(_jsonOptions, ct);
        }
        catch
        {
            return null;
        }
    }

    public async Task<SubscriptionResponse?> GetSubscriptionAsync(
        Guid dealerId,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/billing/subscriptions/by-dealer/{dealerId}", ct);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<SubscriptionResponse>(_jsonOptions, ct);
        }
        catch
        {
            return null;
        }
    }

    public async Task<SubscriptionResponse?> UpdateSubscriptionAsync(
        Guid subscriptionId,
        UpdateSubscriptionRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"/api/billing/subscriptions/{subscriptionId}",
                request,
                _jsonOptions,
                ct);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<SubscriptionResponse>(_jsonOptions, ct);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> CancelSubscriptionAsync(
        Guid subscriptionId,
        CancelSubscriptionRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/billing/subscriptions/{subscriptionId}")
            {
                Content = JsonContent.Create(request, options: _jsonOptions)
            };

            var response = await _httpClient.SendAsync(httpRequest, ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // ========================================
    // CHECKOUT & PORTAL
    // ========================================

    public async Task<CheckoutSessionResponse?> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/billing/checkout", request, _jsonOptions, ct);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CheckoutSessionResponse>(_jsonOptions, ct);
        }
        catch
        {
            return null;
        }
    }

    public async Task<BillingPortalResponse?> CreateBillingPortalSessionAsync(
        CreateBillingPortalRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/billing/billing-portal", request, _jsonOptions, ct);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<BillingPortalResponse>(_jsonOptions, ct);
        }
        catch
        {
            return null;
        }
    }

    // ========================================
    // PRICING
    // ========================================

    public async Task<PricingResponse?> GetPricingAsync(CancellationToken ct = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PricingResponse>("/api/billing/pricing", _jsonOptions, ct);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Extensiones para registrar el cliente de BillingService en DI
/// </summary>
public static class BillingServiceClientExtensions
{
    /// <summary>
    /// Agrega el cliente de BillingService al contenedor de DI
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="baseAddress">URL base del BillingService (ej: "http://localhost:5070")</param>
    public static IServiceCollection AddBillingServiceClient(
        this IServiceCollection services,
        string baseAddress)
    {
        services.AddHttpClient<IBillingServiceClient, BillingServiceClient>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }

    /// <summary>
    /// Agrega el cliente de BillingService usando configuración
    /// </summary>
    public static IServiceCollection AddBillingServiceClient(
        this IServiceCollection services,
        Action<HttpClient> configureClient)
    {
        services.AddHttpClient<IBillingServiceClient, BillingServiceClient>(configureClient);
        return services;
    }
}
