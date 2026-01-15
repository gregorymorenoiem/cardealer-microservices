# 🔌 Cohere API - Documentación Completa

**Versión:** Cohere Command v4  
**Costo:** Gratis tier + $0.50-$1/1M tokens  
**Latencia:** 1-3 segundos  
**Modelos:** Generación, embeddings, clasificación

---

## 📖 Introducción

**Cohere** proporciona LLMs especializados para:

- Generación de texto (descriptions, emails, etc.)
- Embeddings alternativos (más rápido que OpenAI)
- Clasificación (sin ejemplos)
- Resúmenes

### Uso en OKLA:

1. **ListingAnalyticsService**: Mejorar descripciones de vehículos
2. **RecommendationService**: Generación de copy personalizado
3. **ChatbotService**: Alternativa a OpenAI (fallback)

---

## 🔗 Endpoints Principales

### 1. Generate (Text Generation)

**Endpoint:** `POST https://api.cohere.ai/v1/generate`

```bash
curl -X POST https://api.cohere.ai/v1/generate \
  -H "Authorization: Bearer $COHERE_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "command-xlarge-nightly",
    "prompt": "Mejora esta descripción de vehículo: \"Toyota Corolla 2023, automático\"",
    "max_tokens": 200,
    "temperature": 0.8,
    "num_generations": 1
  }'
```

**Response:**

```json
{
  "generations": [
    {
      "id": "gen-...",
      "text": "Toyota Corolla 2023 Automático: Sedán confiable perfecto para familias. Excelente consumo de combustible, aire acondicionado, sistema de sonido premium, mantenimiento bajo. Precio competitivo RD$1.6M-1.8M. Estado impecable."
    }
  ],
  "finish_reason": "COMPLETE"
}
```

---

### 2. Embed (Embeddings)

**Endpoint:** `POST https://api.cohere.ai/v1/embed`

```bash
curl -X POST https://api.cohere.ai/v1/embed \
  -H "Authorization: Bearer $COHERE_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "embed-english-v3.0",
    "input_type": "search_document",
    "texts": ["Toyota Corolla 2023 automático aire acondicionado"]
  }'
```

**Response:**

```json
{
  "embeddings": [[0.0123, -0.0456, 0.0789, ...]],
  "model": "embed-english-v3.0"
}
```

---

### 3. Classify (Zero-shot Classification)

**Endpoint:** `POST https://api.cohere.ai/v1/classify`

```bash
curl -X POST https://api.cohere.ai/v1/classify \
  -H "Authorization: Bearer $COHERE_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "embed-english-light-v3.0",
    "inputs": ["Necesito un vehículo deportivo rápido"],
    "examples": [
      {"text": "Quiero un sedán familiar", "label": "family_car"},
      {"text": "Busco máxima velocidad", "label": "sports_car"},
      {"text": "Necesito bajo mantenimiento", "label": "economical"}
    ]
  }'
```

---

## 💻 Implementación C#

### NuGet Package

```bash
dotnet add package CohereSDK
```

### Program.cs

```csharp
using Cohere;

var cohereClient = new CohereClient(apiKey: builder.Configuration["Cohere:ApiKey"]);
builder.Services.AddSingleton(cohereClient);
builder.Services.AddScoped<ICohereService, CohereService>();
```

### ICohereService.cs

```csharp
public interface ICohereService
{
    Task<string> GenerateDescriptionAsync(string baseDescription, CancellationToken ct);
    Task<float[]> GetEmbeddingAsync(string text, CancellationToken ct);
    Task<string> ClassifyTextAsync(string text, string[] categories, CancellationToken ct);
}
```

### CohereService.cs

```csharp
using Cohere;

public class CohereService : ICohereService
{
    private readonly CohereClient _client;
    private readonly ILogger<CohereService> _logger;

    public CohereService(CohereClient client, ILogger<CohereService> logger)
    {
        _client = client;
        _logger = logger;
    }

    /// <summary>
    /// Mejora descripción de vehículo usando Cohere
    /// </summary>
    public async Task<string> GenerateDescriptionAsync(
        string baseDescription,
        CancellationToken ct)
    {
        var prompt = $@"Eres un experto en marketing de vehículos.
Tu tarea es mejorar esta descripción para hacerla más atractiva:

Descripción original: {baseDescription}

Proporcionas una descripción mejorada que:
- Destaca puntos positivos
- Usa lenguaje profesional
- Es concisa (2-3 oraciones)
- Incluye información útil para compradores

Descripción mejorada:";

        try
        {
            var response = await _client.GenerateAsync(
                new GenerateRequest
                {
                    Model = "command-xlarge-nightly",
                    Prompt = prompt,
                    MaxTokens = 200,
                    Temperature = 0.8m,
                    NumGenerations = 1
                },
                cancellationToken: ct
            );

            var improvedDescription = response.Generations
                .First()
                .Text
                .Trim();

            _logger.LogInformation($"Description improved: {improvedDescription}");

            return improvedDescription;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Cohere generation error: {ex.Message}");
            throw new ServiceException("Error generando descripción mejorada", ex);
        }
    }

    /// <summary>
    /// Obtiene embedding de un texto
    /// </summary>
    public async Task<float[]> GetEmbeddingAsync(
        string text,
        CancellationToken ct)
    {
        try
        {
            var response = await _client.EmbedAsync(
                new EmbedRequest
                {
                    Model = "embed-english-v3.0",
                    InputType = "search_document",
                    Texts = new[] { text }
                },
                cancellationToken: ct
            );

            return response.Embeddings
                .First()
                .Select(x => (float)x)
                .ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Cohere embedding error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Clasifica texto sin entrenar modelo
    /// </summary>
    public async Task<string> ClassifyTextAsync(
        string text,
        string[] categories,
        CancellationToken ct)
    {
        try
        {
            var examples = new[]
            {
                new ClassifyExample { Text = "Quiero máxima velocidad", Label = categories[0] },
                new ClassifyExample { Text = "Bajo mantenimiento", Label = categories[1] },
                new ClassifyExample { Text = "Familia grande", Label = categories[2] }
            };

            var response = await _client.ClassifyAsync(
                new ClassifyRequest
                {
                    Model = "embed-english-light-v3.0",
                    Inputs = new[] { text },
                    Examples = examples.ToList()
                },
                cancellationToken: ct
            );

            return response.Classifications
                .First()
                .Prediction
                .First();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Cohere classification error: {ex.Message}");
            throw;
        }
    }
}
```

### Usar en Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ListingAnalyticsController : ControllerBase
{
    private readonly ICohereService _cohereService;

    public ListingAnalyticsController(ICohereService cohereService)
    {
        _cohereService = cohereService;
    }

    [HttpPost("improve-description")]
    public async Task<IActionResult> ImproveDescription(
        [FromBody] ImproveDescriptionRequest request,
        CancellationToken ct)
    {
        var improved = await _cohereService
            .GenerateDescriptionAsync(request.OriginalDescription, ct);

        return Ok(new
        {
            original = request.OriginalDescription,
            improved = improved,
            provider = "Cohere"
        });
    }
}
```

---

## ⚛️ Ejemplo React

### useCohere Hook

```typescript
import { useMutation } from "@tanstack/react-query";
import axios from "axios";

export const useCohere = () => {
  const improveDescription = useMutation({
    mutationFn: async (originalDescription: string) => {
      const { data } = await axios.post("/api/listings/improve-description", {
        originalDescription,
      });
      return data.improved;
    },
  });

  return { improveDescription };
};
```

### DescriptionImprover Component

```tsx
import React from "react";
import { useCohere } from "@/hooks/useCohere";

export const DescriptionImprover: React.FC = () => {
  const [original, setOriginal] = React.useState("");
  const [improved, setImproved] = React.useState("");
  const { improveDescription } = useCohere();

  const handleImprove = async () => {
    improveDescription.mutate(original, {
      onSuccess: (result) => setImproved(result),
    });
  };

  return (
    <div className="description-improver">
      <textarea
        value={original}
        onChange={(e) => setOriginal(e.target.value)}
        placeholder="Descripción original..."
      />

      <button onClick={handleImprove} disabled={improveDescription.isPending}>
        {improveDescription.isPending ? "Mejorando..." : "Mejorar Descripción"}
      </button>

      {improved && (
        <div className="improved-description">
          <h3>Versión Mejorada:</h3>
          <p>{improved}</p>
        </div>
      )}
    </div>
  );
};
```

---

## 💰 Pricing

### Modelos (por 1M tokens)

| Modelo                   | Input  | Output | Uso             |
| ------------------------ | ------ | ------ | --------------- |
| command-xlarge-nightly   | $0.50  | $1.50  | Text generation |
| embed-english-v3.0       | $0.10  | -      | Embeddings      |
| embed-english-light-v3.0 | Gratis | -      | Classify        |

### Estimación Mensual

```
Text Generation (improve descriptions):
- 100K descriptions × 100 tokens avg
- 10M tokens/mes = $5

Embeddings (search):
- 1M embeddings × 100 tokens avg
- 100M tokens/mes = $10

Classify (categorizar vehículos):
- 500K classifications
- Costo: Gratis (light model)

────────────────────────────────────
TOTAL: ~$15/mes (muy económico!)
```

---

## 🎯 Casos de Uso en OKLA

### 1. Mejorar Descripciones de Listing

**Before:**

```
"Toyota Corolla 2023, automático"
```

**After (con Cohere):**

```
"Toyota Corolla 2023 Automático: Sedán confiable y económico, perfecto para
familias. Excelente consumo de combustible, aire acondicionado, sistema de
audio moderno. Bajo mantenimiento. Precio competitivo RD$1.6M-1.8M.
Estado impecable."
```

### 2. Generar Títulos de Listing

```csharp
var title = await _cohereService.GenerateAsync(
    prompt: $"Crea un título atractivo para este vehículo: {vehicleSpecs}",
    maxTokens: 50
);
// Output: "Toyota Corolla 2023: Sedán Familiar Económico y Confiable"
```

### 3. Email Marketing Automático

```csharp
var emailBody = await _cohereService.GenerateAsync(
    prompt: $"Escribe email para interesado en {vehicleType}",
    maxTokens: 300
);
// Cuerpo de email personalizado
```

---

## ⚙️ Setup

```bash
# 1. Crear cuenta
# https://dashboard.cohere.ai

# 2. Generar API key
# https://dashboard.cohere.ai/api-keys

# 3. Guardar en Kubernetes
kubectl create secret generic cohere-credentials \
  --from-literal=api_key=$COHERE_API_KEY \
  -n okla

# 4. Usar en código
export COHERE_API_KEY=$COHERE_API_KEY
```

---

## ✅ Checklist

- [ ] Crear cuenta Cohere
- [ ] Generar API key
- [ ] Instalar NuGet package
- [ ] Implementar ICohereService
- [ ] Integrar en ListingAnalyticsService
- [ ] Testing en desarrollo
- [ ] Deploy a Kubernetes
- [ ] Monitor costos

---

_Documentación Cohere API para OKLA_  
_Última actualización: Enero 15, 2026_
