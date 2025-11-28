
using System.Text.Json.Serialization;

namespace AuthService.Shared.ErrorMessages;

public class RabbitMQErrorEvent
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; } = "AuthService";

    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; } = string.Empty;

    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; } = string.Empty;

    [JsonPropertyName("stackTrace")]
    public string? StackTrace { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }

    [JsonPropertyName("httpMethod")]
    public string? HttpMethod { get; set; }

    [JsonPropertyName("statusCode")]
    public int? StatusCode { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [JsonPropertyName("environment")]
    public string Environment { get; set; } = "Development";

    public RabbitMQErrorEvent() { }

    public RabbitMQErrorEvent(string errorCode, string errorMessage, string? stackTrace = null, string? userId = null)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        StackTrace = stackTrace;
        UserId = userId;
    }
}
