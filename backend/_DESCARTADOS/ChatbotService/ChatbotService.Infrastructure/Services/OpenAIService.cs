using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Interfaces;

namespace ChatbotService.Infrastructure.Services;

/// <summary>
/// Servicio de integración con OpenAI GPT-4o-mini
/// </summary>
public class OpenAIService : IOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAIService> _logger;
    private readonly string _apiKey;
    private readonly string _model = "gpt-4o-mini";

    public OpenAIService(
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<OpenAIService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiKey = _configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey not configured");
        
        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GenerateResponseAsync(
        List<Message> conversationHistory, 
        string vehicleContext,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = GetSystemPrompt(vehicleContext);
        var messages = new List<object> { new { role = "system", content = systemPrompt } };

        // Add conversation history (last 10 messages for context)
        foreach (var msg in conversationHistory.TakeLast(10))
        {
            messages.Add(new 
            { 
                role = msg.Role.ToString().ToLower(), 
                content = msg.Content 
            });
        }

        var requestBody = new
        {
            model = _model,
            messages,
            temperature = 0.7,
            max_tokens = 500
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<JsonDocument>(responseContent);
            
            var messageContent = result?.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return messageContent ?? "Lo siento, no pude procesar tu mensaje.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            return "Disculpa, estoy teniendo problemas técnicos. ¿Podrías intentar de nuevo?";
        }
    }

    public async Task<IntentAnalysis> AnalyzeIntentAsync(
        string message, 
        List<Message> conversationHistory,
        CancellationToken cancellationToken = default)
    {
        var analysisPrompt = $@"
Analiza el siguiente mensaje del usuario y extrae:
1. Intención principal (InformationSeeking, PriceInquiry, TestDriveRequest, FinancingInquiry, ReadyToBuy, etc.)
2. Señales de compra detectadas
3. Urgencia (inmediato, esta semana, este mes, etc.)
4. Información sobre presupuesto
5. Si menciona trade-in

Mensaje del usuario: ""{message}""

Responde SOLO en formato JSON:
{{
  ""intent"": ""IntentType"",
  ""confidence"": 0.85,
  ""buyingSignals"": [
    {{""signal"": ""wants_test_drive"", ""type"": ""Commitment"", ""weight"": 25}}
  ],
  ""urgency"": ""esta semana"",
  ""hasBudget"": true,
  ""hasTradeIn"": false,
  ""partialScore"": 30
}}
";

        var messages = new List<object>
        {
            new { role = "system", content = "Eres un analizador de intenciones de compra de vehículos." },
            new { role = "user", content = analysisPrompt }
        };

        var requestBody = new
        {
            model = _model,
            messages,
            temperature = 0.3,
            response_format = new { type = "json_object" }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<JsonDocument>(responseContent);
            
            var analysisJson = result?.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return ParseIntentAnalysisJson(analysisJson ?? "{}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing intent with OpenAI");
            return CreateDefaultIntentAnalysis();
        }
    }

    public async Task<string> GenerateConversationSummaryAsync(
        List<Message> messages,
        CancellationToken cancellationToken = default)
    {
        var conversationText = string.Join("\n", messages
            .Where(m => m.Role != MessageRole.System)
            .Select(m => $"{m.Role}: {m.Content}"));

        var summaryPrompt = $@"
Resume esta conversación entre el chatbot OKLA y un prospecto de compra de vehículo.
Incluye:
- Interés principal del usuario
- Señales de compra detectadas
- Urgencia de compra
- Información sobre presupuesto o financiamiento
- Cualquier objeción o preocupación

Conversación:
{conversationText}

Resume en 2-3 oraciones concisas para el vendedor.
";

        var messages_list = new List<object>
        {
            new { role = "system", content = "Eres un experto en resumir conversaciones de ventas." },
            new { role = "user", content = summaryPrompt }
        };

        var requestBody = new
        {
            model = _model,
            messages = messages_list,
            temperature = 0.5,
            max_tokens = 200
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<JsonDocument>(responseContent);
            
            var summary = result?.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return summary ?? "Usuario interesado en el vehículo.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating conversation summary");
            return "Usuario interesado en el vehículo. Requiere seguimiento.";
        }
    }

    public async Task<List<BuyingSignal>> ExtractBuyingSignalsAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        // Simplified version - In production, use OpenAI or ML model
        var signals = new List<BuyingSignal>();
        var messageLower = message.ToLower();

        if (messageLower.Contains("test drive") || messageLower.Contains("probar"))
            signals.Add(new BuyingSignal { Signal = "wants_test_drive", Type = SignalType.Commitment, Weight = 25 });

        if (messageLower.Contains("financ") || messageLower.Contains("cuotas"))
            signals.Add(new BuyingSignal { Signal = "financing_inquiry", Type = SignalType.Financial, Weight = 15 });

        if (messageLower.Contains("disponible") || messageLower.Contains("available"))
            signals.Add(new BuyingSignal { Signal = "availability_check", Type = SignalType.Commitment, Weight = 15 });

        if (messageLower.Contains("esta semana") || messageLower.Contains("urgente"))
            signals.Add(new BuyingSignal { Signal = "high_urgency", Type = SignalType.Urgency, Weight = 25 });

        if (messageLower.Contains("trade") || messageLower.Contains("entregar"))
            signals.Add(new BuyingSignal { Signal = "has_trade_in", Type = SignalType.TradeIn, Weight = 15 });

        if (messageLower.Contains("solo mirando") || messageLower.Contains("just looking"))
            signals.Add(new BuyingSignal { Signal = "just_browsing", Type = SignalType.Negative, Weight = -15 });

        return signals;
    }

    private string GetSystemPrompt(string vehicleContext)
    {
        return $@"
Eres OKLA Bot, el asistente virtual de OKLA.com.do, el marketplace #1 de vehículos en República Dominicana.

TU PERSONALIDAD:
- Amigable y profesional
- Conocedor de vehículos
- Hablas español dominicano (pero entiendes inglés)
- Usas emojis con moderación 🚗 👍 ✅

TU OBJETIVO PRINCIPAL:
1. Ayudar al usuario con información del vehículo
2. Detectar si tiene intención REAL de comprar
3. Recopilar información del lead naturalmente
4. Si el lead está caliente (HOT), ofrecer conectar con el vendedor

INFORMACIÓN DEL VEHÍCULO ACTUAL:
{vehicleContext}

INFORMACIÓN QUE DEBES RECOPILAR (naturalmente, sin interrogar):
- ¿Es para uso personal o negocio?
- ¿Cuándo planea comprar? (urgencia)
- ¿Ya tiene financiamiento o pagará cash?
- ¿Tiene vehículo para trade-in?
- ¿Ya vio el vehículo en persona?

NUNCA:
- Inventes información del vehículo que no tengas
- Des precios diferentes a los del listing
- Presiones al usuario agresivamente
- Compartas información de otros usuarios

RESPUESTAS CORTAS: Máximo 2-3 oraciones por respuesta.
";
    }

    private IntentAnalysis ParseIntentAnalysisJson(string json)
    {
        try
        {
            var doc = JsonSerializer.Deserialize<JsonDocument>(json);
            if (doc == null) return CreateDefaultIntentAnalysis();

            var root = doc.RootElement;
            var intentStr = root.GetProperty("intent").GetString() ?? "Unknown";
            Enum.TryParse<IntentType>(intentStr, out var intentType);

            var analysis = new IntentAnalysis
            {
                IntentType = intentType,
                Confidence = root.GetProperty("confidence").GetDouble(),
                PartialScore = root.GetProperty("partialScore").GetInt32(),
                ExtractedUrgency = root.TryGetProperty("urgency", out var urgency) ? urgency.GetString() : null,
                HasTradeIn = root.TryGetProperty("hasTradeIn", out var tradeIn) && tradeIn.GetBoolean()
            };

            if (root.TryGetProperty("buyingSignals", out var signals))
            {
                foreach (var signal in signals.EnumerateArray())
                {
                    var signalObj = new BuyingSignal
                    {
                        Signal = signal.GetProperty("signal").GetString() ?? "",
                        Type = Enum.Parse<SignalType>(signal.GetProperty("type").GetString() ?? "Engagement"),
                        Weight = signal.GetProperty("weight").GetInt32()
                    };
                    analysis.BuyingSignals.Add(signalObj);
                }
            }

            return analysis;
        }
        catch
        {
            return CreateDefaultIntentAnalysis();
        }
    }

    private IntentAnalysis CreateDefaultIntentAnalysis()
    {
        return new IntentAnalysis
        {
            IntentType = IntentType.InformationSeeking,
            Confidence = 0.5,
            PartialScore = 5
        };
    }
}
