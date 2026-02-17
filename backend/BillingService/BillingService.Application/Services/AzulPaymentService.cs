using BillingService.Application.DTOs.Azul;
using BillingService.Application.Interfaces;
using BillingService.Application.Configuration;
using Microsoft.Extensions.Options;

namespace BillingService.Application.Services;

public interface IAzulPaymentService
{
    AzulPaymentRequest CreatePaymentRequest(decimal amount, decimal itbis, string orderNumber);
    string GetPaymentPageUrl();
    Dictionary<string, string> GetPaymentFormFields(AzulPaymentRequest request);
}

public class AzulPaymentService : IAzulPaymentService
{
    private readonly AzulConfiguration _config;
    private readonly IAzulHashGenerator _hashGenerator;

    public AzulPaymentService(
        IOptions<AzulConfiguration> config,
        IAzulHashGenerator hashGenerator)
    {
        _config = config.Value;
        _hashGenerator = hashGenerator;
    }

    public AzulPaymentRequest CreatePaymentRequest(decimal amount, decimal itbis, string orderNumber)
    {
        // Convertir montos a formato AZUL (sin decimales, últimos 2 dígitos son centavos)
        var amountStr = FormatAmount(amount);
        var itbisStr = FormatAmount(itbis);

        var authHash = _hashGenerator.GenerateRequestHash(
            _config.MerchantId,
            _config.MerchantName,
            _config.MerchantType,
            _config.CurrencyCode,
            orderNumber,
            amountStr,
            itbisStr,
            _config.ApprovedUrl,
            _config.DeclinedUrl,
            _config.CancelUrl,
            "0", // UseCustomField1
            "",  // CustomField1Label
            "",  // CustomField1Value
            "0", // UseCustomField2
            "",  // CustomField2Label
            "",  // CustomField2Value
            _config.AuthKey);

        return new AzulPaymentRequest
        {
            MerchantId = _config.MerchantId,
            MerchantName = _config.MerchantName,
            MerchantType = _config.MerchantType,
            CurrencyCode = _config.CurrencyCode,
            OrderNumber = orderNumber,
            Amount = amountStr,
            ITBIS = itbisStr,
            ApprovedUrl = _config.ApprovedUrl,
            DeclinedUrl = _config.DeclinedUrl,
            CancelUrl = _config.CancelUrl,
            UseCustomField1 = "0",
            UseCustomField2 = "0",
            SaveToDataVault = "0",
            ShowTransactionResult = "1",
            Locale = "ES",
            AuthHash = authHash
        };
    }

    public string GetPaymentPageUrl()
    {
        return _config.PaymentPageUrl;
    }

    public Dictionary<string, string> GetPaymentFormFields(AzulPaymentRequest request)
    {
        return new Dictionary<string, string>
        {
            { "MerchantId", request.MerchantId },
            { "MerchantName", request.MerchantName },
            { "MerchantType", request.MerchantType },
            { "CurrencyCode", request.CurrencyCode },
            { "OrderNumber", request.OrderNumber },
            { "Amount", request.Amount },
            { "ITBIS", request.ITBIS },
            { "ApprovedUrl", request.ApprovedUrl },
            { "DeclinedUrl", request.DeclinedUrl },
            { "CancelUrl", request.CancelUrl },
            { "UseCustomField1", request.UseCustomField1 },
            { "UseCustomField2", request.UseCustomField2 },
            { "SaveToDataVault", request.SaveToDataVault },
            { "ShowTransactionResult", request.ShowTransactionResult },
            { "Locale", request.Locale },
            { "AuthHash", request.AuthHash }
        };
    }

    private static string FormatAmount(decimal amount)
    {
        // Convertir $1,000.00 a "100000" (sin decimales)
        var amountInCents = (long)(amount * 100);
        return amountInCents.ToString();
    }
}
