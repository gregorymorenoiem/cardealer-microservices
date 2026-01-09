using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Interfaces;

namespace ChatbotService.Infrastructure.Services;

/// <summary>
/// OpenAI-powered chatbot service using GPT-4o-mini
/// </summary>
public class OpenAIChatbotService : IChatbotService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAIChatbotService> _logger;
    private readonly string _apiKey;
    private readonly string _model;

    public OpenAIChatbotService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenAIChatbotService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");
        _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";

        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<ChatResponse> GenerateResponseAsync(
        string userMessage,
        ChatConversation conversation,
        VehicleContext? vehicleContext = null,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var messages = BuildMessages(userMessage, conversation, vehicleContext);
            var requestBody = new
            {
                model = _model,
                messages = messages,
                max_tokens = 500,
                temperature = 0.7
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("chat/completions", jsonContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var openAiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var assistantMessage = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "Lo siento, no pude procesar tu solicitud.";
            var tokensUsed = openAiResponse?.Usage?.TotalTokens ?? 0;

            var responseTime = DateTime.UtcNow - startTime;

            // Analyze intent from the conversation
            var intentAnalysis = AnalyzeMessageIntent(userMessage);

            return new ChatResponse
            {
                Content = assistantMessage,
                TokensUsed = tokensUsed,
                ResponseTime = responseTime,
                Intent = intentAnalysis.PrimaryIntent,
                SentimentScore = 0.5, // Neutral by default
                SuggestedReplies = GetDefaultQuickReplies(vehicleContext),
                ShouldTransferToAgent = intentAnalysis.IsBuyingIntent || intentAnalysis.NeedsHumanAgent,
                TransferReason = intentAnalysis.IsBuyingIntent ? "Usuario muestra intención de compra" : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI response for conversation {ConversationId}", conversation.Id);

            return new ChatResponse
            {
                Content = "Lo siento, estoy teniendo problemas técnicos. Por favor, intenta de nuevo en unos momentos o contacta directamente al vendedor.",
                TokensUsed = 0,
                ResponseTime = DateTime.UtcNow - startTime,
                SuggestedReplies = GetDefaultQuickReplies(vehicleContext)
            };
        }
    }

    public Task<IntentAnalysis> AnalyzeIntentAsync(string message, CancellationToken cancellationToken = default)
    {
        var analysis = AnalyzeMessageIntent(message);
        return Task.FromResult(analysis);
    }

    public Task<List<QuickReply>> GetSuggestedRepliesAsync(ChatConversation conversation, CancellationToken cancellationToken = default)
    {
        VehicleContext? vehicleContext = null;
        if (!string.IsNullOrEmpty(conversation.VehicleContext))
        {
            try
            {
                vehicleContext = JsonSerializer.Deserialize<VehicleContext>(conversation.VehicleContext);
            }
            catch { }
        }

        return Task.FromResult(GetDefaultQuickReplies(vehicleContext));
    }

    private List<object> BuildMessages(string userMessage, ChatConversation conversation, VehicleContext? vehicleContext)
    {
        var messages = new List<object>();

        // System prompt
        var systemPrompt = GetSystemPrompt(vehicleContext);
        messages.Add(new { role = "system", content = systemPrompt });

        // Previous conversation context (last 10 messages)
        var previousMessages = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .TakeLast(10)
            .ToList();

        foreach (var msg in previousMessages)
        {
            var role = msg.Role switch
            {
                MessageRole.User => "user",
                MessageRole.Assistant => "assistant",
                _ => "system"
            };
            messages.Add(new { role, content = msg.Content });
        }

        // Current user message
        messages.Add(new { role = "user", content = userMessage });

        return messages;
    }

    private string GetSystemPrompt(VehicleContext? vehicleContext)
    {
        var basePrompt = """
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

        if (vehicleContext != null)
        {
            basePrompt += $"\n\n{vehicleContext.ToContextString()}";
        }

        return basePrompt;
    }

    private IntentAnalysis AnalyzeMessageIntent(string message)
    {
        var lowerMessage = message.ToLowerInvariant();

        // Check for buying intent
        var buyingKeywords = new[] { "comprar", "precio", "cuanto", "cuánto", "disponible", "financiamiento", "negociar", "oferta", "efectivo", "test drive", "ver el carro", "cuándo puedo", "dónde está" };
        var isBuyingIntent = buyingKeywords.Any(k => lowerMessage.Contains(k));

        // Check if needs human agent
        var humanKeywords = new[] { "hablar con", "vendedor", "persona real", "agente", "llamar", "número", "whatsapp" };
        var needsHuman = humanKeywords.Any(k => lowerMessage.Contains(k));

        // Extract vehicle type if mentioned
        string? vehicleType = null;
        var vehicleTypes = new[] { "suv", "sedan", "camioneta", "pickup", "deportivo", "híbrido", "eléctrico" };
        vehicleType = vehicleTypes.FirstOrDefault(t => lowerMessage.Contains(t));

        // Try to extract budget
        decimal? budget = null;
        var budgetMatch = System.Text.RegularExpressions.Regex.Match(lowerMessage, @"\$?(\d{1,3}(?:,?\d{3})*(?:\.\d{2})?)\s*(?:pesos|rd|dop|millones)?");
        if (budgetMatch.Success && decimal.TryParse(budgetMatch.Groups[1].Value.Replace(",", ""), out var parsedBudget))
        {
            budget = parsedBudget;
        }

        return new IntentAnalysis
        {
            PrimaryIntent = isBuyingIntent ? "buying_intent" : (needsHuman ? "agent_request" : "general_inquiry"),
            Confidence = isBuyingIntent || needsHuman ? 0.8 : 0.5,
            IsBuyingIntent = isBuyingIntent,
            NeedsHumanAgent = needsHuman,
            ExtractedVehicleType = vehicleType,
            ExtractedBudget = budget
        };
    }

    private List<QuickReply> GetDefaultQuickReplies(VehicleContext? vehicleContext)
    {
        var replies = new List<QuickReply>();

        if (vehicleContext != null)
        {
            replies.Add(new QuickReply { Id = "1", Label = "📋 Ver más detalles", Value = "¿Puedes darme más detalles sobre este vehículo?" });
            replies.Add(new QuickReply { Id = "2", Label = "💰 Precio negociable?", Value = "¿El precio es negociable?" });
            replies.Add(new QuickReply { Id = "3", Label = "🚗 Test drive", Value = "¿Puedo agendar un test drive?" });
            replies.Add(new QuickReply { Id = "4", Label = "📞 Contactar vendedor", Value = "Quiero hablar con el vendedor" });
        }
        else
        {
            replies.Add(new QuickReply { Id = "1", Label = "🔍 Buscar vehículo", Value = "Ayúdame a encontrar un vehículo" });
            replies.Add(new QuickReply { Id = "2", Label = "💵 Vender mi carro", Value = "Quiero vender mi vehículo" });
            replies.Add(new QuickReply { Id = "3", Label = "❓ Cómo funciona", Value = "¿Cómo funciona OKLA?" });
        }

        return replies;
    }
}

// OpenAI API Response Models
public class OpenAIResponse
{
    public string? Id { get; set; }
    public List<OpenAIChoice>? Choices { get; set; }
    public OpenAIUsage? Usage { get; set; }
}

public class OpenAIChoice
{
    public OpenAIMessage? Message { get; set; }
    public string? FinishReason { get; set; }
}

public class OpenAIMessage
{
    public string? Role { get; set; }
    public string? Content { get; set; }
}

public class OpenAIUsage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}
