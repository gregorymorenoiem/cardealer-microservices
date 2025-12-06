using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace BillingService.Infrastructure.External;

/// <summary>
/// Cliente para comunicarse con UserService y sincronizar suscripciones
/// </summary>
public interface IUserServiceClient
{
    /// <summary>
    /// Actualiza el StripeCustomerId de un dealer en UserService
    /// </summary>
    Task<bool> UpdateStripeCustomerAsync(
        Guid dealerId,
        string stripeCustomerId,
        CancellationToken ct = default);

    /// <summary>
    /// Actualiza la información de suscripción de Stripe en UserService
    /// </summary>
    Task<bool> UpdateStripeSubscriptionAsync(
        Guid dealerId,
        string stripeSubscriptionId,
        string plan,
        string status,
        DateTime? trialEndDate = null,
        DateTime? currentPeriodEnd = null,
        CancellationToken ct = default);
}

/// <summary>
/// Implementación del cliente HTTP para UserService
/// </summary>
public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserServiceClient> _logger;

    public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> UpdateStripeCustomerAsync(
        Guid dealerId,
        string stripeCustomerId,
        CancellationToken ct = default)
    {
        try
        {
            var request = new { StripeCustomerId = stripeCustomerId };
            var response = await _httpClient.PatchAsJsonAsync(
                $"/api/dealers/{dealerId}/stripe-customer",
                request,
                ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Successfully synced Stripe customer {StripeCustomerId} to UserService for dealer {DealerId}",
                    stripeCustomerId, dealerId);
                return true;
            }

            _logger.LogWarning(
                "Failed to sync Stripe customer to UserService. Status: {StatusCode}",
                response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error syncing Stripe customer {StripeCustomerId} to UserService for dealer {DealerId}",
                stripeCustomerId, dealerId);
            return false;
        }
    }

    public async Task<bool> UpdateStripeSubscriptionAsync(
        Guid dealerId,
        string stripeSubscriptionId,
        string plan,
        string status,
        DateTime? trialEndDate = null,
        DateTime? currentPeriodEnd = null,
        CancellationToken ct = default)
    {
        try
        {
            var request = new
            {
                StripeSubscriptionId = stripeSubscriptionId,
                Plan = plan,
                Status = status,
                TrialEndDate = trialEndDate,
                CurrentPeriodEnd = currentPeriodEnd
            };

            var response = await _httpClient.PatchAsJsonAsync(
                $"/api/dealers/{dealerId}/stripe-subscription",
                request,
                ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Successfully synced Stripe subscription {StripeSubscriptionId} to UserService for dealer {DealerId}",
                    stripeSubscriptionId, dealerId);
                return true;
            }

            _logger.LogWarning(
                "Failed to sync Stripe subscription to UserService. Status: {StatusCode}",
                response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error syncing Stripe subscription {StripeSubscriptionId} to UserService for dealer {DealerId}",
                stripeSubscriptionId, dealerId);
            return false;
        }
    }
}
