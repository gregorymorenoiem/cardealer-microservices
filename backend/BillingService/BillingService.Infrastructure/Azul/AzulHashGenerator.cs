using System.Security.Cryptography;
using System.Text;
using BillingService.Application.Interfaces;

namespace BillingService.Infrastructure.Azul;

public class AzulHashGenerator : IAzulHashGenerator
{
    public string GenerateRequestHash(
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
        string authKey)
    {
        var toHash = merchantId + merchantName + merchantType + currencyCode
                   + orderNumber + amount + itbis + approvedUrl + declinedUrl
                   + cancelUrl + useCustomField1 + customField1Label
                   + customField1Value + useCustomField2 + customField2Label
                   + customField2Value + authKey;

        return ComputeHmacSha512(toHash, authKey);
    }

    public string GenerateResponseHash(
        string orderNumber,
        string amount,
        string authorizationCode,
        string dateTime,
        string responseCode,
        string isoCode,
        string responseMessage,
        string rrn,
        string azulOrderId,
        string authKey)
    {
        var toHash = orderNumber + amount + authorizationCode + dateTime
                   + responseCode + isoCode + responseMessage + rrn + azulOrderId + authKey;

        return ComputeHmacSha512(toHash, authKey);
    }

    public bool ValidateResponseHash(
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
        string authKey)
    {
        var expectedHash = GenerateResponseHash(
            orderNumber,
            amount,
            authorizationCode,
            dateTime,
            responseCode,
            isoCode,
            responseMessage,
            rrn,
            azulOrderId,
            authKey);

        return string.Equals(expectedHash, receivedHash, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeHmacSha512(string data, string key)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
