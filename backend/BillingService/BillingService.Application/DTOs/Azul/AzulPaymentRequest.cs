namespace BillingService.Application.DTOs.Azul;

public record AzulPaymentRequest
{
    public string MerchantId { get; init; } = string.Empty;
    public string MerchantName { get; init; } = string.Empty;
    public string MerchantType { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public string OrderNumber { get; init; } = string.Empty;
    public string Amount { get; init; } = string.Empty; // Sin decimales, ej: 100000 = $1,000.00
    public string ITBIS { get; init; } = string.Empty; // ITBIS sin decimales
    public string ApprovedUrl { get; init; } = string.Empty;
    public string DeclinedUrl { get; init; } = string.Empty;
    public string CancelUrl { get; init; } = string.Empty;
    public string UseCustomField1 { get; init; } = "0";
    public string CustomField1Label { get; init; } = string.Empty;
    public string CustomField1Value { get; init; } = string.Empty;
    public string UseCustomField2 { get; init; } = "0";
    public string CustomField2Label { get; init; } = string.Empty;
    public string CustomField2Value { get; init; } = string.Empty;
    public string SaveToDataVault { get; init; } = "0";
    public string DataVaultToken { get; init; } = string.Empty;
    public string ShowTransactionResult { get; init; } = "1";
    public string Locale { get; init; } = "ES"; // ES o EN
    public string AuthHash { get; init; } = string.Empty;
}
