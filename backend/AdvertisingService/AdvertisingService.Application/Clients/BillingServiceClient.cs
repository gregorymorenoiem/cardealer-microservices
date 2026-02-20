using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Application.Clients;

public class BillingServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BillingServiceClient> _logger;

    public BillingServiceClient(HttpClient httpClient, ILogger<BillingServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CreatePaymentResult?> CreateAdvertisingPaymentAsync(
        Guid userId,
        Guid campaignId,
        decimal amount,
        string currency,
        string description,
        CancellationToken ct = default)
    {
        try
        {
            var request = new
            {
                UserId = userId,
                Amount = amount,
                Currency = currency,
                Description = description,
                PaymentType = "advertising_campaign",
                Metadata = new Dictionary<string, string>
                {
                    ["campaignId"] = campaignId.ToString()
                }
            };

            var response = await _httpClient.PostAsJsonAsync("/api/billing/payments", request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to create billing payment for campaign {CampaignId}: {StatusCode}",
                    campaignId, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CreatePaymentResult>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling BillingService for campaign {CampaignId}", campaignId);
            return null;
        }
    }
}

public class CreatePaymentResult
{
    public Guid PaymentId { get; set; }
    public string? CheckoutUrl { get; set; }
    public string Status { get; set; } = string.Empty;
}
