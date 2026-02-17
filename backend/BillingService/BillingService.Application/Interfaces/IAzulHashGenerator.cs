namespace BillingService.Application.Interfaces;

/// <summary>
/// Interface for generating and validating AZUL payment hashes using HMAC-SHA512
/// </summary>
public interface IAzulHashGenerator
{
    /// <summary>
    /// Generates authentication hash for AZUL payment request
    /// </summary>
    string GenerateRequestHash(
        string merchantId,
        string merchantName,
        string merchantType,
        string currencyCode,
        string orderNumber,
        string amount,
        string itbis,
        string approvedUrl,
        string declinedUrl,
        string cancelUrl,
        string useCustomField1,
        string customField1Label,
        string customField1Value,
        string useCustomField2,
        string customField2Label,
        string customField2Value,
        string authKey);

    /// <summary>
    /// Generates hash for validating AZUL payment response
    /// </summary>
    string GenerateResponseHash(
        string orderNumber,
        string amount,
        string authorizationCode,
        string dateTime,
        string responseCode,
        string isoCode,
        string responseMessage,
        string rrn,
        string azulOrderId,
        string authKey);

    /// <summary>
    /// Validates that response hash matches calculated hash
    /// </summary>
    bool ValidateResponseHash(
        string orderNumber,
        string amount,
        string authorizationCode,
        string dateTime,
        string responseCode,
        string isoCode,
        string responseMessage,
        string rrn,
        string azulOrderId,
        string receivedHash,
        string authKey);
}
