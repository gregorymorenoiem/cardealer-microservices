using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Serilog;
using AzulPaymentService.Application.DTOs;

namespace AzulPaymentService.Infrastructure.Services;

/// <summary>
/// Cliente HTTP para integración con API de AZUL
/// </summary>
public class AzulHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzulHttpClient> _logger;

    private readonly string _merchantId;
    private readonly string _authKey;
    private readonly string _baseUrl;

    public AzulHttpClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AzulHttpClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _merchantId = configuration["Azul:MerchantId"] ?? throw new InvalidOperationException("Azul:MerchantId not configured");
        _authKey = configuration["Azul:AuthKey"] ?? throw new InvalidOperationException("Azul:AuthKey not configured");
        _baseUrl = configuration["Azul:BaseUrl"] ?? "https://api.azul.com.do/api/1.0";
    }

    /// <summary>
    /// Procesa un cobro/transacción
    /// </summary>
    public async Task<ChargeResponseDto> ProcessChargeAsync(ChargeRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Procesando cobro en AZUL. Usuario: {UserId}, Monto: {Amount}", request.UserId, request.Amount);

            var payload = BuildChargePayload(request);
            var signature = GenerateAuthHash(payload);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/transaction")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            httpRequest.Headers.Add("X-Signature", signature);
            httpRequest.Headers.Add("X-Merchant-Id", _merchantId);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.Information("Respuesta AZUL: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"AZUL API error: {response.StatusCode} - {responseContent}");
            }

            var azulResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return ParseChargeResponse(azulResponse, request);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al procesar cobro en AZUL");
            throw;
        }
    }

    /// <summary>
    /// Procesa un reembolso
    /// </summary>
    public async Task<ChargeResponseDto> ProcessRefundAsync(string azulTransactionId, decimal amount, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Procesando reembolso en AZUL. Transacción: {AzulTransactionId}, Monto: {Amount}", azulTransactionId, amount);

            var payload = new
            {
                transactionId = azulTransactionId,
                amount = amount.ToString("F2"),
                reason = reason
            };

            var signature = GenerateAuthHash(payload);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/refund")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            httpRequest.Headers.Add("X-Signature", signature);
            httpRequest.Headers.Add("X-Merchant-Id", _merchantId);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"AZUL Refund API error: {response.StatusCode}");
            }

            var azulResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return new ChargeResponseDto
            {
                AzulTransactionId = azulResponse.GetProperty("refundId").GetString() ?? string.Empty,
                Status = "Refunded",
                Amount = amount,
                IsSuccessful = true
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al procesar reembolso en AZUL");
            throw;
        }
    }

    /// <summary>
    /// Crea una suscripción recurrente
    /// </summary>
    public async Task<SubscriptionResponseDto> CreateSubscriptionAsync(SubscriptionRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Creando suscripción en AZUL. Usuario: {UserId}, Plan: {Plan}", request.UserId, request.PlanName);

            var payload = BuildSubscriptionPayload(request);
            var signature = GenerateAuthHash(payload);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/subscription")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            httpRequest.Headers.Add("X-Signature", signature);
            httpRequest.Headers.Add("X-Merchant-Id", _merchantId);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"AZUL Subscription API error: {response.StatusCode}");
            }

            var azulResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return ParseSubscriptionResponse(azulResponse, request);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al crear suscripción en AZUL");
            throw;
        }
    }

    /// <summary>
    /// Cancela una suscripción
    /// </summary>
    public async Task<bool> CancelSubscriptionAsync(string azulSubscriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Cancelando suscripción en AZUL. ID: {AzulSubscriptionId}", azulSubscriptionId);

            var payload = new { subscriptionId = azulSubscriptionId };
            var signature = GenerateAuthHash(payload);

            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/subscription/{azulSubscriptionId}");
            httpRequest.Headers.Add("X-Signature", signature);
            httpRequest.Headers.Add("X-Merchant-Id", _merchantId);

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error al cancelar suscripción en AZUL");
            throw;
        }
    }

    // ============= Métodos Privados =============

    private object BuildChargePayload(ChargeRequestDto request)
    {
        return new
        {
            transactionType = request.TransactionType,
            amount = request.Amount.ToString("F2"),
            currency = request.Currency,
            description = request.Description,
            cardNumber = request.CardNumber ?? string.Empty,
            cardExpiryMonth = request.CardExpiryMonth ?? string.Empty,
            cardExpiryYear = request.CardExpiryYear ?? string.Empty,
            cardCVV = request.CardCVV ?? string.Empty,
            cardholderName = request.CardholderName ?? string.Empty,
            cardToken = request.CardToken ?? string.Empty,
            customerEmail = request.CustomerEmail ?? string.Empty,
            customerPhone = request.CustomerPhone ?? string.Empty,
            customerIpAddress = request.CustomerIpAddress ?? string.Empty,
            invoiceReference = request.InvoiceReference ?? string.Empty
        };
    }

    private object BuildSubscriptionPayload(SubscriptionRequestDto request)
    {
        return new
        {
            amount = request.Amount.ToString("F2"),
            currency = request.Currency,
            frequency = request.Frequency,
            startDate = request.StartDate.ToString("yyyy-MM-dd"),
            endDate = request.EndDate?.ToString("yyyy-MM-dd"),
            cardNumber = request.CardNumber ?? string.Empty,
            cardExpiryMonth = request.CardExpiryMonth ?? string.Empty,
            cardExpiryYear = request.CardExpiryYear ?? string.Empty,
            cardCVV = request.CardCVV ?? string.Empty,
            cardholderName = request.CardholderName ?? string.Empty,
            cardToken = request.CardToken ?? string.Empty,
            customerEmail = request.CustomerEmail ?? string.Empty,
            customerPhone = request.CustomerPhone ?? string.Empty,
            planName = request.PlanName ?? string.Empty
        };
    }

    private string GenerateAuthHash(object payload)
    {
        var jsonString = JsonSerializer.Serialize(payload);
        var message = $"{_merchantId}{jsonString}{_authKey}";

        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToHexString(hash).ToLower();
        }
    }

    private ChargeResponseDto ParseChargeResponse(JsonElement response, ChargeRequestDto request)
    {
        return new ChargeResponseDto
        {
            TransactionId = Guid.NewGuid(),
            AzulTransactionId = response.GetProperty("transactionId").GetString() ?? string.Empty,
            Status = response.GetProperty("status").GetString() ?? "Pending",
            ResponseCode = response.GetProperty("responseCode").GetString(),
            ResponseMessage = response.GetProperty("responseMessage").GetString(),
            AuthorizationCode = response.GetProperty("authorizationCode").GetString(),
            Amount = request.Amount,
            Currency = request.Currency,
            TransactionDate = DateTime.UtcNow,
            IsSuccessful = response.GetProperty("status").GetString() == "Approved"
        };
    }

    private SubscriptionResponseDto ParseSubscriptionResponse(JsonElement response, SubscriptionRequestDto request)
    {
        return new SubscriptionResponseDto
        {
            SubscriptionId = Guid.NewGuid(),
            AzulSubscriptionId = response.GetProperty("subscriptionId").GetString() ?? string.Empty,
            UserId = request.UserId,
            Amount = request.Amount,
            Currency = request.Currency,
            Frequency = request.Frequency,
            Status = "Active",
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            NextChargeDate = request.StartDate,
            IsSuccessful = true
        };
    }
}
