# 🔴 OpenAI API - Documentación Completa

**Versión:** GPT-4o Mini (actual), GPT-4o (próximo)  
**Costo:** $0.15-$15 por 1M tokens  
**Latencia:** 1-5 segundos  
**Rate Limit:** 3,500 req/min (tier 1)

---

## 📋 Tabla de Contenidos

1. [Introducción](#introducción)
2. [Endpoints Principales](#endpoints-principales)
3. [Modelos Disponibles](#modelos-disponibles)
4. [Casos de Uso en OKLA](#casos-de-uso-en-okla)
5. [Implementación C#](#implementación-c)
6. [Ejemplo React](#ejemplo-react)
7. [Pricing](#pricing)
8. [Troubleshooting](#troubleshooting)

---

## 📖 Introducción

**OpenAI** proporciona acceso a modelos de lenguaje de última generación (LLM):

- **GPT-4o**: Último modelo (multimodal, texto+imagen)
- **GPT-4 Turbo**: Modelo intermedio
- **GPT-3.5 Turbo**: Modelo económico (recomendado para producción)

OKLA usará OpenAI para:

1. 🤖 **ChatbotService**: Responder preguntas sobre vehículos
2. 🚫 **ReviewService**: Moderation de reviews (spam/abusivo)
3. 🔍 **RecommendationService**: Embeddings para similitud de vehículos

---

## 🔗 Endpoints Principales

### 1. Chat Completions (LLM)

**Endpoint:** `POST https://api.openai.com/v1/chat/completions`

```json
{
  "model": "gpt-4o-mini",
  "messages": [
    {
      "role": "system",
      "content": "Eres un asistente de OKLA especializado en vehículos. Responde en español."
    },
    {
      "role": "user",
      "content": "¿Cuál es el precio promedio de un Toyota Corolla 2023?"
    }
  ],
  "temperature": 0.7,
  "max_tokens": 500
}
```

**Response:**

```json
{
  "id": "chatcmpl-8u3b...",
  "object": "chat.completion",
  "created": 1705340000,
  "model": "gpt-4o-mini",
  "choices": [
    {
      "index": 0,
      "message": {
        "role": "assistant",
        "content": "El precio promedio de un Toyota Corolla 2023 en República Dominicana está entre RD$1.5M - RD$1.8M..."
      },
      "finish_reason": "stop"
    }
  ],
  "usage": {
    "prompt_tokens": 45,
    "completion_tokens": 120,
    "total_tokens": 165
  }
}
```

---

### 2. Embeddings (Similitud)

**Endpoint:** `POST https://api.openai.com/v1/embeddings`

```json
{
  "input": "Toyota Corolla 2023 automático aire acondicionado",
  "model": "text-embedding-3-small"
}
```

**Response:**

```json
{
  "object": "list",
  "data": [
    {
      "object": "embedding",
      "embedding": [
        0.0123,
        -0.0456,
        0.0789,
        ...
        0.0234
      ],
      "index": 0
    }
  ],
  "model": "text-embedding-3-small",
  "usage": {
    "prompt_tokens": 12,
    "total_tokens": 12
  }
}
```

---

### 3. Moderation (Content Policy)

**Endpoint:** `POST https://api.openai.com/v1/moderations`

```json
{
  "input": "Este vendedor es un estafador. No confíen.",
  "model": "text-moderation-latest"
}
```

**Response:**

```json
{
  "id": "modr-8u3b...",
  "model": "text-moderation-7",
  "results": [
    {
      "flagged": true,
      "categories": {
        "hate": false,
        "sexual": false,
        "harassment": true,
        "self-harm": false,
        "violence": false,
        "illegal": false
      },
      "category_scores": {
        "harassment": 0.92
      }
    }
  ]
}
```

---

## 🤖 Modelos Disponibles

| Modelo                     | Tokens | Precio (input) | Uso en OKLA                                  |
| -------------------------- | ------ | -------------- | -------------------------------------------- |
| **gpt-4o**                 | 128K   | $5/1M          | Análisis profundo (no recomendado por costo) |
| **gpt-4o-mini**            | 128K   | $0.15/1M       | ✅ ChatBot (recomendado)                     |
| **gpt-4 turbo**            | 128K   | $10/1M         | Análisis complejos                           |
| **gpt-3.5-turbo**          | 16K    | $0.50/1M       | ✅ Embeddings + Moderation                   |
| **text-embedding-3-small** | -      | $0.02/1M       | ✅ Embeddings rápidos                        |
| **text-embedding-3-large** | -      | $0.13/1M       | Embeddings más precisos                      |
| **text-moderation-latest** | -      | Gratis         | ✅ Moderation                                |

---

## 💡 Casos de Uso en OKLA

### 1. ChatbotService (5060)

**Descripción:** Chatbot que responde preguntas sobre vehículos

**Prompts:**

```
SISTEMA:
"Eres un asistente de OKLA, plataforma de compra-venta de vehículos en República Dominicana.
Tienes acceso a una base de datos de 500+ vehículos.

Tu objetivo:
1. Responder preguntas sobre vehículos específicos
2. Ayudar a los usuarios a encontrar el vehículo perfecto
3. Proporcionar información de precios y especificaciones
4. Direccionar a los vendedores

Responde siempre en español, de forma amable y profesional."

USUARIO:
"¿Cuál es la diferencia entre un Toyota Corolla y un Honda Civic?"
```

**Response esperada:**

```
Ambos son sedanes económicos populares:

**Toyota Corolla:**
- Confiabilidad: Muy alta
- Mantenimiento: Bajo
- Precio RD: $1.5M - $1.8M
- Consumo: 15-18 km/litro
- Mejor para: Compradores que priorizan durabilidad

**Honda Civic:**
- Potencia: Superior
- Diseño: Más moderno
- Precio RD: $1.6M - $1.9M
- Consumo: 14-17 km/litro
- Mejor para: Compradores que quieren rendimiento

¿Buscan alguno de estos modelos?
```

---

### 2. ReviewService (5059)

**Descripción:** Moderation automática de reviews

**Casos:**

```
REVIEW A: "Excelente vehículo, muy buen trato del vendedor!"
→ ✅ APROBADO (sin issues)

REVIEW B: "Estafador, no confíen!!! Eso no funciona!!!"
→ ❌ RECHAZADO (harassment + potentially false)

REVIEW C: "Se ve que fue chocado, pintura mala"
→ ✅ APROBADO (crítica legítima)

REVIEW D: "[Spam] Compra viagra barata aquí..."
→ ❌ RECHAZADO (spam + illegal)
```

---

### 3. RecommendationService (5054)

**Descripción:** Embeddings para encontrar vehículos similares

**Flujo:**

```
Usuario mira: "Toyota Corolla 2023, automático, aire, $1.6M"
↓
Se calcula embedding del vehículo
↓
Se buscan 10 vehículos más cercanos en vector space
↓
Se retorna: "Similares a lo que viste"
```

**Ejemplo:**

```python
# Vector de un vehículo
vehicle_1 = "Toyota Corolla 2023 automático aire acondicionado"
embedding_1 = [0.012, -0.045, 0.078, ...]  # 1,536 dimensiones

# Vector de otro vehículo
vehicle_2 = "Toyota Corolla 2024 automático aire"
embedding_2 = [0.011, -0.046, 0.079, ...]

# Similitud coseno (0-1)
similarity = cosine_similarity(embedding_1, embedding_2)  # 0.98 (muy similar!)
```

---

## 💻 Implementación C#

### Nuget Package

```bash
dotnet add package OpenAI
```

### Program.cs

```csharp
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

// Registrar OpenAI
var openaiApiKey = builder.Configuration["OpenAI:ApiKey"];
builder.Services.AddSingleton(new OpenAIClient(openaiApiKey));

// Registrar servicio
builder.Services.AddScoped<IOpenAIService, OpenAIService>();

var app = builder.Build();
```

### IOpenAIService.cs

```csharp
namespace ChatbotService.Application.Services;

public interface IOpenAIService
{
    Task<string> GetChatResponseAsync(string message, CancellationToken ct);
    Task<List<double>> GetEmbeddingAsync(string text, CancellationToken ct);
    Task<ModerationResult> ModerateContentAsync(string text, CancellationToken ct);
}
```

### OpenAIService.cs

```csharp
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using OpenAI.Moderations;

namespace ChatbotService.Infrastructure.Services;

public class OpenAIService : IOpenAIService
{
    private readonly OpenAIClient _client;
    private readonly ILogger<OpenAIService> _logger;
    private const string SystemPrompt = @"Eres un asistente de OKLA especializado en vehículos.
Tu objetivo es ayudar a usuarios a encontrar el vehículo perfecto.
Responde siempre en español, de forma clara y amable.
Si no sabes algo, se honesto y sugiere contactar al vendedor.";

    public OpenAIService(OpenAIClient client, ILogger<OpenAIService> logger)
    {
        _client = client;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene respuesta del chatbot
    /// </summary>
    public async Task<string> GetChatResponseAsync(string message, CancellationToken ct)
    {
        try
        {
            var chatRequest = new ChatRequest(
                messages: new[]
                {
                    new Message(Role.System, SystemPrompt),
                    new Message(Role.User, message)
                },
                model: "gpt-4o-mini"
            )
            {
                Temperature = 0.7m,
                MaxTokens = 500
            };

            var response = await _client.ChatEndpoint.GetCompletionAsync(chatRequest, ct);

            _logger.LogInformation($"Chat response: {response.FirstChoice.Message.Content}");

            return response.FirstChoice.Message.Content;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"OpenAI API error: {ex.Message}");
            throw new ServiceException("Error al comunicarse con ChatGPT", ex);
        }
    }

    /// <summary>
    /// Obtiene embedding de un texto para similitud
    /// </summary>
    public async Task<List<double>> GetEmbeddingAsync(string text, CancellationToken ct)
    {
        try
        {
            var embeddingRequest = new EmbeddingRequest(
                model: "text-embedding-3-small",
                input: text
            );

            var response = await _client.EmbeddingsEndpoint.CreateEmbeddingAsync(embeddingRequest, ct);

            return response.Data.First().Embedding.ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Embedding API error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Modera contenido (detecta spam, abuso, etc)
    /// </summary>
    public async Task<ModerationResult> ModerateContentAsync(string text, CancellationToken ct)
    {
        try
        {
            var moderationRequest = new ModerationRequest(input: text);
            var response = await _client.ModerationsEndpoint.CreateModerationAsync(moderationRequest, ct);

            var result = response.Results.First();

            return new ModerationResult
            {
                IsFlagged = result.Flagged,
                Categories = new ModerationCategories
                {
                    Harassment = result.CategoryScores.Harassment,
                    Hate = result.CategoryScores.Hate,
                    SelfHarm = result.CategoryScores.SelfHarm,
                    Sexual = result.CategoryScores.Sexual,
                    Violence = result.CategoryScores.Violence
                }
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Moderation API error: {ex.Message}");
            throw;
        }
    }
}
```

### Usar en Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly IOpenAIService _openaiService;

    public ChatbotController(IOpenAIService openaiService)
    {
        _openaiService = openaiService;
    }

    [HttpPost("message")]
    public async Task<IActionResult> SendMessage(
        [FromBody] ChatMessageRequest request,
        CancellationToken ct)
    {
        var response = await _openaiService.GetChatResponseAsync(request.Message, ct);
        return Ok(new { message = response });
    }

    [HttpPost("moderate")]
    public async Task<IActionResult> ModerateReview(
        [FromBody] ReviewModerationRequest request,
        CancellationToken ct)
    {
        var result = await _openaiService.ModerateContentAsync(request.ReviewText, ct);

        return Ok(new
        {
            isFlagged = result.IsFlagged,
            reason = result.IsFlagged ? "Potencialmente abusivo o spam" : "Aprobado",
            scores = result.Categories
        });
    }
}
```

---

## ⚛️ Ejemplo React

### useOpenAIChat.ts

```typescript
import { useQuery, useMutation } from "@tanstack/react-query";
import axios from "axios";

interface ChatMessage {
  role: "user" | "assistant";
  content: string;
}

export const useOpenAIChat = () => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);

  const sendMessage = useMutation({
    mutationFn: async (message: string) => {
      const { data } = await axios.post("/api/chatbot/message", { message });
      return data.message;
    },
    onSuccess: (assistantMessage) => {
      setMessages((prev) => [
        ...prev,
        { role: "assistant", content: assistantMessage },
      ]);
    },
  });

  const handleSendMessage = (text: string) => {
    setMessages((prev) => [...prev, { role: "user", content: text }]);
    sendMessage.mutate(text);
  };

  return { messages, sendMessage, handleSendMessage };
};
```

### ChatbotWidget.tsx

```tsx
import React from "react";
import { useOpenAIChat } from "@/hooks/useOpenAIChat";

export const ChatbotWidget: React.FC = () => {
  const { messages, handleSendMessage, sendMessage } = useOpenAIChat();
  const [input, setInput] = React.useState("");

  const onSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (input.trim()) {
      handleSendMessage(input);
      setInput("");
    }
  };

  return (
    <div className="chatbot-widget">
      <div className="messages">
        {messages.map((msg, i) => (
          <div key={i} className={`message ${msg.role}`}>
            {msg.content}
          </div>
        ))}
        {sendMessage.isPending && <div className="loading">Escribiendo...</div>}
      </div>

      <form onSubmit={onSubmit}>
        <input
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="Pregunta sobre vehículos..."
          disabled={sendMessage.isPending}
        />
        <button type="submit" disabled={sendMessage.isPending}>
          Enviar
        </button>
      </form>
    </div>
  );
};
```

---

## 💵 Pricing

### Modelos (por 1M tokens)

| Modelo                 | Input  | Output |
| ---------------------- | ------ | ------ |
| gpt-4o-mini            | $0.15  | $0.60  |
| gpt-3.5-turbo          | $0.50  | $1.50  |
| text-embedding-3-small | $0.02  | -      |
| text-moderation        | Gratis | -      |

### Estimación Mensual (100K usuarios activos)

```
ChatGPT (gpt-4o-mini):
- 10M tokens/mes (estimado)
- Costo: (10M * $0.15) / 1M = $1,500

Embeddings (texto-embedding-3-small):
- 5M tokens/mes
- Costo: (5M * $0.02) / 1M = $100

Moderation (gratis):
- Costo: $0

────────────────────────
TOTAL: ~$1,600/mes
```

---

## 🔍 Troubleshooting

### Error: 401 Unauthorized

```
Causa: API key inválida o expirada
Solución:
1. Verificar que la API key está correcta
2. Regenerar key en https://platform.openai.com/account/api-keys
3. Actualizar secret en Kubernetes
```

### Error: 429 Rate Limit

```
Causa: Demasiadas requests
Solución:
1. Implementar exponential backoff
2. Usar queue (RabbitMQ) para procesar requests
3. Aumentar tier de OpenAI ($100/mes = 500K req/min)
```

### Error: 500 Server Error

```
Causa: Error interno de OpenAI
Solución:
1. Reintentar después de 5 segundos
2. Usar fallback (cached responses)
3. Notificar al usuario
```

### Latencia Alta (>5s)

```
Causa: Sobrecarga de OpenAI
Solución:
1. Usar modelo más rápido (gpt-4o-mini en lugar de gpt-4o)
2. Reducir max_tokens
3. Implementar caching con Redis
4. Usar batch processing para no-critical tasks
```

---

## ✅ Checklist de Implementación

- [ ] Crear cuenta OpenAI
- [ ] Generar API key
- [ ] Guardar en Kubernetes secrets
- [ ] Instalar NuGet package
- [ ] Implementar IOpenAIService
- [ ] Crear ChatbotController
- [ ] Testing (unit tests)
- [ ] Deploy a desarrollo
- [ ] Monitor costos y latencia
- [ ] Deploy a producción

---

_Documentación OpenAI API para OKLA_  
_Última actualización: Enero 15, 2026_
