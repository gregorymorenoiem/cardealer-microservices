# 🤖 OpenAI API - Documentación Técnica

**API Provider:** OpenAI  
**Versión:** v1  
**Tipo:** AI/LLM Service (GPT-4, GPT-3.5-turbo)  
**Status en OKLA:** 🚧 Planificado (Q3 2026)  
**Última actualización:** Enero 15, 2026

---

## 📋 Descripción General

**OpenAI API** se utiliza para:

- Chatbot inteligente en OKLA
- Recomendaciones personalizadas de vehículos
- Análisis de inquietudes de clientes
- Generación de descripciones de vehículos
- Lead scoring automático
- Moderación de contenido

**¿Por qué OpenAI?**

- ✅ **Modelos state-of-the-art** (GPT-4)
- ✅ **Fine-tuning disponible** para casos específicos
- ✅ **API muy confiable** (99.99% uptime)
- ✅ **Pricing transparente** (por tokens)
- ✅ **Excelente documentación**
- ✅ **Community activa**

---

## 🔑 Autenticación

### Crear API Key en OpenAI

1. Ir a [OpenAI Platform](https://platform.openai.com/)
2. Sign up o login
3. Ir a **API Keys**
4. Crear nueva clave

### En appsettings.json

```json
{
  "OpenAI": {
    "ApiKey": "${OPENAI_API_KEY}",
    "Organization": "${OPENAI_ORG_ID}",
    "Model": "gpt-4",
    "MaxTokens": 2048
  }
}
```

---

## 🔌 Endpoints Principales

### Chat Completion (Chatbot)

```
POST https://api.openai.com/v1/chat/completions
```

**Headers:**

```
Authorization: Bearer {API_KEY}
Content-Type: application/json
```

**Body:**

```json
{
  "model": "gpt-4",
  "messages": [
    {
      "role": "system",
      "content": "Eres un asistente de OKLA, un marketplace de vehículos. Ayuda a usuarios a encontrar vehículos y responde preguntas sobre el proceso de compra."
    },
    {
      "role": "user",
      "content": "Busco un auto económico para mi familia"
    }
  ],
  "temperature": 0.7,
  "max_tokens": 500,
  "top_p": 0.9,
  "frequency_penalty": 0.0,
  "presence_penalty": 0.0
}
```

**Response (200 OK):**

```json
{
  "id": "chatcmpl-8nxxxxxxxxxxx",
  "object": "chat.completion",
  "created": 1704067200,
  "model": "gpt-4",
  "usage": {
    "prompt_tokens": 45,
    "completion_tokens": 123,
    "total_tokens": 168
  },
  "choices": [
    {
      "message": {
        "role": "assistant",
        "content": "Te recomendaría buscar Toyota Corolla, Hyundai Elantra o Kia Forte. Están en el rango económico..."
      },
      "finish_reason": "stop",
      "index": 0
    }
  ]
}
```

### Moderation (Revisar Contenido)

```
POST https://api.openai.com/v1/moderations
```

**Body:**

```json
{
  "input": "contenido a revisar por spam/hate speech"
}
```

---

## 💻 Implementación en C#/.NET

### Instalación del paquete

```bash
dotnet add package OpenAI
```

### OpenAIChatbotService.cs

```csharp
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Services;

public class OpenAIChatbotService : IChatbotService
{
    private readonly OpenAIAPI _client;
    private readonly ILogger<OpenAIChatbotService> _logger;
    private readonly string _systemPrompt;

    public OpenAIChatbotService(string apiKey, ILogger<OpenAIChatbotService> logger)
    {
        _client = new OpenAIAPI(apiKey);
        _logger = logger;
        _systemPrompt = @"Eres un asistente experto de OKLA, un marketplace de vehículos en República Dominicana.

Tu objetivo es:
- Ayudar a usuarios a encontrar el vehículo perfecto
- Responder preguntas sobre modelos, características y precios
- Guiar el proceso de compra
- Ofrecer test drives
- Conectar con dealers
- Ser amable, profesional y conciso

Contexto de OKLA:
- 500+ vehículos disponibles
- 50+ dealers verificados
- Tipos de cuenta: Comprador, Vendedor, Dealer
- Planes de dealer: Starter ($49), Pro ($129), Enterprise ($299)
- Métodos de pago: AZUL (tarjetas RD), Stripe (tarjetas internacionales)

Si el usuario pregunta algo fuera de tu contexto, políticamente responde que necesita contactar soporte.";
    }

    // ✅ Chat simple
    public async Task<Result<string>> GetChatResponseAsync(
        string userMessage,
        string conversationHistory = null,
        CancellationToken ct = default)
    {
        try
        {
            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatMessageRole.System, _systemPrompt),
                new ChatMessage(ChatMessageRole.User, userMessage)
            };

            // Si hay historial, agregarlo
            if (!string.IsNullOrEmpty(conversationHistory))
            {
                // Parsear historial (simplificado)
                messages.Insert(1, new ChatMessage(ChatMessageRole.Assistant, conversationHistory));
            }

            var chatRequest = new ChatRequest
            {
                Model = Model.GPT4,
                Messages = messages,
                Temperature = 0.7,
                MaxTokens = 500,
                TopP = 0.9
            };

            var response = await _client.Chat.CreateChatCompletionAsync(chatRequest);

            var assistantMessage = response.Choices[0].Message.Content;

            _logger.LogInformation($"Chatbot response generated. Tokens: {response.Usage.TotalTokens}");
            return Result<string>.Success(assistantMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception generating chat response");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Recomendación de vehículos
    public async Task<Result<string>> GetVehicleRecommendationAsync(
        string userPreferences,
        CancellationToken ct = default)
    {
        try
        {
            var prompt = $@"
Basado en las siguientes preferencias del usuario, recomienda 3-5 vehículos disponibles en OKLA:

Preferencias del usuario: {userPreferences}

Para cada recomendación:
1. Modelo y año
2. Precio
3. Por qué es una buena opción
4. Características principales

Sé específico y conciso.
";

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatMessageRole.System, _systemPrompt),
                new ChatMessage(ChatMessageRole.User, prompt)
            };

            var chatRequest = new ChatRequest
            {
                Model = Model.GPT4,
                Messages = messages,
                Temperature = 0.5,
                MaxTokens = 800,
                TopP = 0.9
            };

            var response = await _client.Chat.CreateChatCompletionAsync(chatRequest);
            var recommendation = response.Choices[0].Message.Content;

            _logger.LogInformation("Vehicle recommendation generated");
            return Result<string>.Success(recommendation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception generating recommendation");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Lead Scoring
    public async Task<Result<LeadScore>> ScoreLeadAsync(
        string inquiryText,
        string buyerProfile,
        CancellationToken ct = default)
    {
        try
        {
            var prompt = $@"
Analiza la siguiente consulta de cliente y asigna un score de lead (0-100) para OKLA.

Consulta: {inquiryText}
Perfil del comprador: {buyerProfile}

Basado en:
- Urgencia (búsqueda inmediata vs exploración)
- Intención (genuino vs curioso)
- Capacidad de compra (presupuesto claro vs vago)
- Engagement (detalles específicos vs pregunta genérica)

Responde SOLO en este formato JSON:
{{
  ""score"": 75,
  ""category"": ""warm"",
  ""reason"": ""Cliente busca vehículo específico con presupuesto definido""
}}

Categorías: hot (80+), warm (60-79), cold (0-59)
";

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatMessageRole.System, "Eres un experto en lead scoring. Analiza consultas y asigna scores."),
                new ChatMessage(ChatMessageRole.User, prompt)
            };

            var chatRequest = new ChatRequest
            {
                Model = Model.GPT4,
                Messages = messages,
                Temperature = 0.3,
                MaxTokens = 200,
                TopP = 0.9
            };

            var response = await _client.Chat.CreateChatCompletionAsync(chatRequest);
            var responseText = response.Choices[0].Message.Content;

            // Parsear JSON
            var leadScore = JsonSerializer.Deserialize<LeadScore>(responseText);

            _logger.LogInformation($"Lead scored: {leadScore.Score} ({leadScore.Category})");
            return Result<LeadScore>.Success(leadScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception scoring lead");
            return Result<LeadScore>.Failure($"Error: {ex.Message}");
        }
    }

    // ✅ Generar descripción de vehículo
    public async Task<Result<string>> GenerateVehicleDescriptionAsync(
        string vehicleDetails,
        CancellationToken ct = default)
    {
        try
        {
            var prompt = $@"
Basado en los siguientes detalles, genera una descripción persuasiva para un anuncio de vehículo:

Detalles: {vehicleDetails}

Requisitos:
- 150-200 palabras
- Enfocarse en características principales
- Incluir condición y estado
- Ser persuasivo pero honesto
- Español dominicano

Descripción:
";

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatMessageRole.System, "Eres un copywriter experto en anuncios de vehículos."),
                new ChatMessage(ChatMessageRole.User, prompt)
            };

            var chatRequest = new ChatRequest
            {
                Model = Model.GPT4,
                Messages = messages,
                Temperature = 0.7,
                MaxTokens = 300,
                TopP = 0.9
            };

            var response = await _client.Chat.CreateChatCompletionAsync(chatRequest);
            var description = response.Choices[0].Message.Content;

            return Result<string>.Success(description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception generating description");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }
}

// DTOs
public class LeadScore
{
    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } // hot, warm, cold

    [JsonPropertyName("reason")]
    public string Reason { get; set; }
}

public class ChatbotConversation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<ChatMessage> Messages { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalTokens { get; set; }
    public decimal Cost => TotalTokens * 0.00003m; // GPT-4 pricing
}
```

---

## 🎯 Casos de Uso en OKLA

### 1. Chatbot en Homepage

```csharp
[HttpPost("chat")]
public async Task<IActionResult> PostMessage(
    [FromBody] ChatRequest request)
{
    var response = await _chatbotService.GetChatResponseAsync(
        request.Message,
        request.ConversationHistory);

    return Ok(new { response = response.Data });
}
```

### 2. Recomendador de Vehículos

```csharp
var recommendation = await _chatbotService.GetVehicleRecommendationAsync(
    "Busco SUV económico para familia, presupuesto $12K-$18K");
```

### 3. Lead Scoring Automático

```csharp
var leadScore = await _chatbotService.ScoreLeadAsync(
    "Necesito un Toyota Corolla 2018-2020 en buen estado para este mes",
    "Primer comprador, presupuesto definido");

if (leadScore.Data.Score >= 80)
{
    await _dealerService.NotifyDealersAsync(leadScore.Data);
}
```

---

## 💰 Costos

| Modelo            | Costo/1K tokens                   |
| ----------------- | --------------------------------- |
| **GPT-4**         | $0.03 (input) / $0.06 (output)    |
| **GPT-3.5-turbo** | $0.005 (input) / $0.0015 (output) |

**Estimación OKLA:**

- 1,000 chat interactions/mes = ~$50
- Recomendaciones: ~$20
- Lead scoring: ~$30
- Total Q1: ~$100/mes

---

**Mantenido por:** AI Team  
**Última revisión:** Enero 15, 2026  
**Próxima implementación:** Q3 2026
