# 🧠 Anthropic Claude API - Advanced LLM

**Versión:** Claude 3.5 Sonnet  
**Contexto:** 200K tokens  
**Latencia:** 1-5s  
**Costo:** $3-$15 per 1M tokens

---

## 📖 Introducción

**Claude** de Anthropic es un LLM avanzado para:

- Análisis complejos y razonamiento
- Procesamiento de contextos largos (200K tokens)
- Tareas que requieren múltiples pasos
- Generación de contenido de alta calidad

### Vs ChatGPT:

| Aspecto       | Claude      | ChatGPT     |
| ------------- | ----------- | ----------- |
| Razonamiento  | ⭐⭐⭐⭐⭐  | ⭐⭐⭐⭐    |
| Contexto      | 200K tokens | 128K tokens |
| Precio        | $3-15/1M    | $0.15-15/1M |
| Velocidad     | 1-5s        | <1s         |
| API Stability | ⭐⭐⭐⭐⭐  | ⭐⭐⭐⭐    |

### Uso en OKLA:

1. **ChatbotService**: Análisis profundo de consultas complejas
2. **ReviewService**: Análisis detallado de reviews (sentimiento + contexto)
3. **DataPipelineService**: Procesamiento de datos para ML
4. **RecommendationService**: Explicaciones personalizadas

---

## 🎯 Caso Principal: ChatBot Avanzado para Dealers

### Ejemplo de Conversación Multi-Turno

```
User: "¿Cuál es el mejor SUV de 2023 para una familia con 3 hijos en RD?"

Claude Response (using long context):
1. Analiza preferencias (familia, 3 hijos = seguridad importante)
2. Filtra SUVs 2023 disponibles en OKLA
3. Explica por qué recomendaciones específicas
4. Menciona financiamiento disponible
5. Ofrece comparación con alternativas
6. Propone siguiente paso (test drive)

Complejidad: 50K+ tokens para análisis completo
```

---

## 💻 Implementación C#

### NuGet Packages

```bash
dotnet add package Anthropic.SDK
```

### IClaudeService.cs

```csharp
public interface IClaudeService
{
    Task<string> AnalyzeUserQueryAsync(
        string userQuery,
        string context, // Información de vehículos, usuario, etc.
        CancellationToken ct
    );

    Task<string> AnalyzeReviewAsync(
        string reviewText,
        string vehicleInfo,
        CancellationToken ct
    );

    Task<List<string>> GenerateRecommendationsAsync(
        string userPreferences,
        int maxRecommendations,
        CancellationToken ct
    );

    Task<string> ProcessLongContextAsync(
        string document,
        string question,
        CancellationToken ct
    );
}

public record ClaudeAnalysisResult(
    string Analysis,
    int TokensUsed,
    double Cost
);
```

### ClaudeService.cs

```csharp
using Anthropic;

public class ClaudeService : IClaudeService
{
    private readonly AnthropicClient _client;
    private readonly ILogger<ClaudeService> _logger;

    // Modelos disponibles
    private const string ClaudeModel = "claude-3-5-sonnet-20241022";
    private const double InputTokenCost = 3.0 / 1_000_000; // $3 per 1M
    private const double OutputTokenCost = 15.0 / 1_000_000; // $15 per 1M

    public ClaudeService(
        IConfiguration config,
        ILogger<ClaudeService> logger)
    {
        _logger = logger;
        var apiKey = config["Anthropic:ApiKey"];
        _client = new AnthropicClient(apiKey);
    }

    /// <summary>
    /// Analizar consulta de usuario con contexto completo
    /// </summary>
    public async Task<string> AnalyzeUserQueryAsync(
        string userQuery,
        string context,
        CancellationToken ct)
    {
        try
        {
            var messages = new List<MessageParam>
            {
                new MessageParam
                {
                    Role = "user",
                    Content = $"""
                    CONTEXTO:
                    {context}

                    CONSULTA DEL USUARIO:
                    {userQuery}

                    Por favor, analiza la consulta del usuario considerando todo el contexto.
                    Proporciona una respuesta personalizada y útil. Si es un vehículo,
                    explica por qué es una buena opción.
                    """
                }
            };

            var response = await _client.Messages.CreateAsync(
                new MessageCreateRequest
                {
                    Model = ClaudeModel,
                    MaxTokens = 2000,
                    Messages = messages
                },
                ct
            );

            var result = response.Content[0].Text;

            _logger.LogInformation(
                $"Analysis complete. Tokens: input={response.Usage.InputTokens}, " +
                $"output={response.Usage.OutputTokens}"
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Claude analysis error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Analizar review con contexto largo (200K tokens)
    /// </summary>
    public async Task<string> AnalyzeReviewAsync(
        string reviewText,
        string vehicleInfo,
        CancellationToken ct)
    {
        try
        {
            var messages = new List<MessageParam>
            {
                new MessageParam
                {
                    Role = "user",
                    Content = $"""
                    INFORMACIÓN DEL VEHÍCULO:
                    {vehicleInfo}

                    REVIEW:
                    {reviewText}

                    Por favor, analiza este review y proporciona:
                    1. Sentimiento general (positivo/negativo/neutral)
                    2. Aspectos principales mencionados
                    3. Puntuación recomendada (1-5 estrellas)
                    4. Preocupaciones o puntos negativos
                    5. Fortalezas destacadas

                    Responde en JSON:
                    {{
                        "sentiment": "positive|negative|neutral",
                        "score": 4.5,
                        "highlights": ["..."],
                        "concerns": ["..."],
                        "summary": "..."
                    }}
                    """
                }
            };

            var response = await _client.Messages.CreateAsync(
                new MessageCreateRequest
                {
                    Model = ClaudeModel,
                    MaxTokens = 1500,
                    Messages = messages
                },
                ct
            );

            var result = response.Content[0].Text;

            _logger.LogInformation("Review analysis complete");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Review analysis error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Generar recomendaciones personalizadas
    /// </summary>
    public async Task<List<string>> GenerateRecommendationsAsync(
        string userPreferences,
        int maxRecommendations,
        CancellationToken ct)
    {
        try
        {
            var messages = new List<MessageParam>
            {
                new MessageParam
                {
                    Role = "user",
                    Content = $"""
                    PREFERENCIAS DEL USUARIO:
                    {userPreferences}

                    Basado en estas preferencias, genera {maxRecommendations}
                    recomendaciones de vehículos específicas para OKLA.

                    Para cada recomendación, explica brevemente por qué es
                    adecuada basándose en las preferencias.

                    Responde en JSON:
                    {{
                        "recommendations": [
                            {{
                                "suggestion": "...",
                                "reason": "...",
                                "why_perfect": "..."
                            }}
                        ]
                    }}
                    """
                }
            };

            var response = await _client.Messages.CreateAsync(
                new MessageCreateRequest
                {
                    Model = ClaudeModel,
                    MaxTokens = 2000,
                    Messages = messages
                },
                ct
            );

            var result = response.Content[0].Text;

            // Parsear JSON y extraer recomendaciones
            var recommendations = ExtractRecommendations(result);

            _logger.LogInformation($"Generated {recommendations.Count} recommendations");

            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Recommendation generation error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Procesar documentos largos (200K tokens)
    /// </summary>
    public async Task<string> ProcessLongContextAsync(
        string document,
        string question,
        CancellationToken ct)
    {
        try
        {
            // Útil para:
            // - Analizar 20+ reviews juntos
            // - Procesar historiales de comprador
            // - Analizar múltiples listados

            var messages = new List<MessageParam>
            {
                new MessageParam
                {
                    Role = "user",
                    Content = $"""
                    DOCUMENTO:
                    {document}

                    PREGUNTA:
                    {question}

                    Por favor, analiza el documento completo y responde
                    la pregunta considerando todo el contenido.
                    """
                }
            };

            var response = await _client.Messages.CreateAsync(
                new MessageCreateRequest
                {
                    Model = ClaudeModel,
                    MaxTokens = 3000,
                    Messages = messages
                },
                ct
            );

            var result = response.Content[0].Text;

            _logger.LogInformation(
                $"Long context processing complete. " +
                $"Input tokens: {response.Usage.InputTokens}"
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Long context error: {ex.Message}");
            throw;
        }
    }

    // === Métodos Auxiliares ===

    private List<string> ExtractRecommendations(string jsonResponse)
    {
        // Parsear JSON response
        var recommendations = new List<string>();

        try
        {
            // Usar System.Text.Json para parsear
            var recommendations_text = jsonResponse
                .Split("\"suggestion\"")
                .Skip(1)
                .Select(s => s.Split("\"")[1])
                .ToList();

            return recommendations_text;
        }
        catch
        {
            return new List<string> { jsonResponse };
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
    private readonly IClaudeService _claudeService;
    private readonly IVehicleRepository _vehicleRepository;

    [HttpPost("analyze")]
    [Authorize]
    public async Task<IActionResult> AnalyzeQuery(
        [FromBody] QueryRequest request,
        CancellationToken ct)
    {
        // Obtener contexto (vehículos similares, info del usuario)
        var vehicles = await _vehicleRepository
            .FindByCategoryAsync(request.PreferredCategory);

        var context = FormatVehiclesContext(vehicles);

        // Usar Claude para análisis profundo
        var analysis = await _claudeService.AnalyzeUserQueryAsync(
            request.Query,
            context,
            ct
        );

        return Ok(new
        {
            analysis = analysis,
            nextSteps = GenerateNextSteps(analysis)
        });
    }

    [HttpPost("recommendations")]
    [Authorize]
    public async Task<IActionResult> GetRecommendations(
        [FromBody] PreferencesRequest request,
        CancellationToken ct)
    {
        var recommendations = await _claudeService
            .GenerateRecommendationsAsync(
                request.UserPreferences,
                maxRecommendations: 5,
                ct: ct
            );

        return Ok(new { recommendations });
    }
}
```

---

## 🔄 Conversación Multi-Turno (Memory Management)

```csharp
public class ConversationManager
{
    private readonly List<MessageParam> _conversationHistory;
    private readonly IClaudeService _claudeService;

    public ConversationManager(IClaudeService claudeService)
    {
        _claudeService = claudeService;
        _conversationHistory = new List<MessageParam>();
    }

    public async Task<string> SendMessageAsync(
        string userMessage,
        CancellationToken ct)
    {
        // Agregar mensaje del usuario al historial
        _conversationHistory.Add(new MessageParam
        {
            Role = "user",
            Content = userMessage
        });

        // Claude retiene contexto de toda la conversación
        var response = await _claudeService.ProcessLongContextAsync(
            FormatHistory(),
            userMessage,
            ct
        );

        // Agregar respuesta al historial
        _conversationHistory.Add(new MessageParam
        {
            Role = "assistant",
            Content = response
        });

        return response;
    }

    private string FormatHistory()
    {
        return string.Join("\n\n", _conversationHistory
            .Select(m => $"{m.Role.ToUpper()}: {m.Content}"));
    }
}
```

---

## 💰 Pricing

```
Claude 3.5 Sonnet:
- Input: $3.00 per 1M tokens
- Output: $15.00 per 1M tokens

Ejemplo para OKLA (100 consultas/día):
- Promedio 1K tokens input, 500 tokens output por consulta
- Diario: (1K*$3 + 500*$15) / 1M * 100 = $0.96/día
- Mensual: ~$29/mes

Con reviews (50 reviews/día):
- Diario: +$0.50
- Mensual: +$15/mes

TOTAL: ~$45/mes para ChatBot + Reviews
```

---

## ✅ Checklist

- [ ] Crear cuenta Anthropic
- [ ] Generar API key
- [ ] Implementar IClaudeService
- [ ] Testing con queries complejas
- [ ] Integrar en ChatbotService
- [ ] Integrar en ReviewService
- [ ] Multi-turn conversation testing
- [ ] Deployment a Kubernetes
- [ ] Monitoreo de costos

---

_Documentación Anthropic Claude para OKLA_  
_Última actualización: Enero 15, 2026_
