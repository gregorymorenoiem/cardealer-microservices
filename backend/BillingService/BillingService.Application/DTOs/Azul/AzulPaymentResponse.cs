namespace BillingService.Application.DTOs.Azul;

public record AzulPaymentResponse
{
    public string OrderNumber { get; init; } = string.Empty;
    public string Amount { get; init; } = string.Empty;
    public string ITBIS { get; init; } = string.Empty;
    public string AuthorizationCode { get; init; } = string.Empty;
    public string DateTime { get; init; } = string.Empty;
    public string ResponseCode { get; init; } = string.Empty;
    public string IsoCode { get; init; } = string.Empty;
    public string ResponseMessage { get; init; } = string.Empty;
    public string ErrorDescription { get; init; } = string.Empty;
    public string RRN { get; init; } = string.Empty;
    public string AzulOrderId { get; init; } = string.Empty;
    public string DataVaultToken { get; init; } = string.Empty;
    public string DataVaultExpiration { get; init; } = string.Empty;
    public string DataVaultBrand { get; init; } = string.Empty;
    public string AuthHash { get; init; } = string.Empty;

    public bool IsApproved => IsoCode == "00" && ResponseCode == "ISO8583";
    public bool IsDeclined => IsoCode != "00" && ResponseCode == "ISO8583";
    public bool HasError => ResponseCode == "Error";
}
