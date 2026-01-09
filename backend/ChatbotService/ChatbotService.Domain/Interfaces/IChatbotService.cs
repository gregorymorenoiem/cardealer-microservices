using ChatbotService.Domain.Entities;

namespace ChatbotService.Domain.Interfaces;

/// <summary>
/// Interface for the AI-powered chatbot service
/// </summary>
public interface IChatbotService
{
    /// <summary>
    /// Generate a response from the AI chatbot
    /// </summary>
    Task<ChatResponse> GenerateResponseAsync(
        string userMessage,
        ChatConversation conversation,
        VehicleContext? vehicleContext = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze the intent of a user message
    /// </summary>
    Task<IntentAnalysis> AnalyzeIntentAsync(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get suggested quick replies based on conversation context
    /// </summary>
    Task<List<QuickReply>> GetSuggestedRepliesAsync(ChatConversation conversation, CancellationToken cancellationToken = default);
}

public class ChatResponse
{
    public string Content { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public string? Intent { get; set; }
    public double? SentimentScore { get; set; }
    public List<QuickReply> SuggestedReplies { get; set; } = new();
    public bool ShouldTransferToAgent { get; set; }
    public string? TransferReason { get; set; }
}

public class IntentAnalysis
{
    public string PrimaryIntent { get; set; } = "general_inquiry";
    public double Confidence { get; set; }
    public bool IsBuyingIntent { get; set; }
    public bool NeedsHumanAgent { get; set; }
    public string? ExtractedVehicleType { get; set; }
    public decimal? ExtractedBudget { get; set; }
}

public class VehicleContext
{
    public Guid VehicleId { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public string? Transmission { get; set; }
    public string? FuelType { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }
    public string? SellerName { get; set; }
    public string? Location { get; set; }

    public string ToContextString()
    {
        return $"""
            Vehículo actual:
            - {Year} {Make} {Model}
            - Precio: RD${Price:N0}
            - Kilometraje: {Mileage:N0} km
            - Transmisión: {Transmission ?? "N/A"}
            - Combustible: {FuelType ?? "N/A"}
            - Color: {Color ?? "N/A"}
            - Ubicación: {Location ?? "N/A"}
            - Vendedor: {SellerName ?? "N/A"}
            
            Descripción: {Description ?? "No disponible"}
            """;
    }
}
