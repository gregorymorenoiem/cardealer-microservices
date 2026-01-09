namespace ChatbotService.Domain.Entities;

/// <summary>
/// Configuration for the chatbot behavior
/// </summary>
public class ChatbotConfiguration
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "OKLA Assistant";
    public string SystemPrompt { get; set; } = GetDefaultSystemPrompt();
    public string Model { get; set; } = "gpt-4o-mini";
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 500;
    public int MaxConversationMessages { get; set; } = 20;
    public bool IsActive { get; set; } = true;
    public string? WelcomeMessage { get; set; }
    public string? FallbackMessage { get; set; }
    public List<string> QuickReplies { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    private static string GetDefaultSystemPrompt()
    {
        return """
            Eres OKLA Assistant, el asistente virtual de OKLA, el marketplace #1 de vehículos en República Dominicana.

            Tu rol es:
            1. Ayudar a los usuarios a encontrar el vehículo perfecto
            2. Responder preguntas sobre vehículos específicos cuando tengas el contexto
            3. Explicar el proceso de compra/venta en OKLA
            4. Ser amable, profesional y conciso

            Reglas:
            - Responde SIEMPRE en español
            - Sé breve pero completo (máximo 2-3 párrafos)
            - Si no tienes información específica, ofrece alternativas
            - Cuando el usuario muestre interés real de compra, sugiere contactar al vendedor
            - Nunca inventes información sobre precios o disponibilidad
            - Si preguntan por financiamiento, menciona que OKLA conecta con bancos locales

            Contexto del mercado dominicano:
            - Marcas populares: Toyota, Honda, Hyundai, Kia, Nissan
            - Los precios en RD incluyen impuestos de importación
            - La mayoría de vehículos son importados de USA o Asia
            """;
    }
}

/// <summary>
/// Quick reply suggestions for the chatbot
/// </summary>
public class QuickReply
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Icon { get; set; }
}
