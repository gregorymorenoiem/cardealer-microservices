# 🟦 Google Vertex AI - Documentación Completa

**Versión:** Vertex AI + BigQuery ML  
**Costo:** $6-100/mes según uso  
**Latencia:** <1 segundo  
**Cobertura:** Modelos pre-trained, AutoML, custom models

---

## 📖 Introducción

**Google Vertex AI** es la plataforma de ML de Google Cloud que incluye:

- Pre-trained models (Embeddings, Text, Image, Video)
- AutoML (entrenar modelos sin código)
- Custom training (código Python/TensorFlow)
- Model Registry y deployment

### Uso en OKLA:

1. **VehicleIntelligenceService**: Predicción de precios (precio óptimo)
2. **RecommendationService**: Embeddings para similitud
3. **DataPipelineService**: Forecasting de demanda

---

## 🔗 Principales Endpoints

### 1. Generative AI (Gemini Models)

**Endpoint:** `https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}/publishers/google/models/gemini-pro:generateContent`

```bash
curl -X POST \
  "https://us-central1-aiplatform.googleapis.com/v1/projects/okla-ai/locations/us-central1/publishers/google/models/gemini-pro:generateContent" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "contents": {
      "role": "user",
      "parts": {
        "text": "¿Cuál es el precio recomendado para un Toyota Corolla 2023?"
      }
    }
  }'
```

---

### 2. Embeddings (Text)

**Endpoint:** `https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}/publishers/google/models/textembedding-gecko:predict`

```bash
curl -X POST \
  "https://us-central1-aiplatform.googleapis.com/v1/projects/okla-ai/locations/us-central1/publishers/google/models/textembedding-gecko:predict" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "instances": [
      {
        "content": "Toyota Corolla 2023 automático aire"
      }
    ]
  }'
```

Response:

```json
{
  "predictions": [
    {
      "embeddings": {
        "values": [
          0.0123,
          -0.0456,
          0.0789,
          ...
        ]
      }
    }
  ]
}
```

---

### 3. TabularRegression (Pricing)

**Endpoint:** Custom model endpoint

```bash
curl -X POST \
  "https://us-central1-aiplatform.googleapis.com/v1/projects/okla-ai/locations/us-central1/endpoints/{ENDPOINT_ID}:predict" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "instances": [
      {
        "year": 2023,
        "mileage": 45000,
        "make": "Toyota",
        "model": "Corolla",
        "fuel_type": "Gasolina",
        "transmission": "Automático"
      }
    ]
  }'
```

Response:

```json
{
  "predictions": [
    {
      "value": 1650000
    }
  ]
}
```

---

## 💻 Implementación C#

### Nuget Package

```bash
dotnet add package Google.Cloud.AIPlatform.V1
dotnet add package Google.Api.Gax.GrpcAdapter
```

### Program.cs

```csharp
using Google.Cloud.AIPlatform.V1;
using Google.Api.Gax.Grpc;

var builder = WebApplication.CreateBuilder(args);

// Setup Google Cloud authentication
Environment.SetEnvironmentVariable(
    "GOOGLE_APPLICATION_CREDENTIALS",
    builder.Configuration["Google:CredentialsPath"]
);

var projectId = builder.Configuration["Google:ProjectId"];
var location = "us-central1";

// Registrar servicios de Vertex AI
builder.Services.AddSingleton(_ =>
    new PredictionServiceClientBuilder
    {
        ChannelCredentials = Google.Api.Gax.Grpc.ChannelCredentials.Create(
            Google.Apis.Auth.OAuth2.GoogleCredential
                .GetApplicationDefault()
                .CreateScoped("https://www.googleapis.com/auth/cloud-platform")
                .UnderlyingCredential as Google.Apis.Auth.OAuth2.ServiceAccountCredential
        )
    }.Build()
);

builder.Services.AddScoped<IVertexAIService>(sp =>
    new VertexAIService(
        sp.GetRequiredService<PredictionServiceClient>(),
        projectId,
        location,
        sp.GetRequiredService<ILogger<VertexAIService>>()
    )
);

var app = builder.Build();
```

### IVertexAIService.cs

```csharp
namespace VehicleIntelligenceService.Application.Services;

public interface IVertexAIService
{
    /// <summary>
    /// Predice el precio óptimo de un vehículo
    /// </summary>
    Task<decimal> PredictVehiclePriceAsync(
        VehiclePricingInput input,
        CancellationToken ct
    );

    /// <summary>
    /// Obtiene embedding de un texto para búsqueda por similitud
    /// </summary>
    Task<float[]> GetEmbeddingAsync(string text, CancellationToken ct);

    /// <summary>
    /// Predice demanda futura de un vehículo
    /// </summary>
    Task<DemandForecastOutput> ForecastDemandAsync(
        DemandForecastInput input,
        CancellationToken ct
    );
}

public record VehiclePricingInput(
    int Year,
    long Mileage,
    string Make,
    string Model,
    string FuelType,
    string Transmission,
    string Condition,
    int DaysOnMarket
);

public record DemandForecastInput(
    string Category,
    int Month,
    int Year,
    decimal AveragePrice
);

public record DemandForecastOutput(
    int ExpectedListings,
    decimal TrendDirection, // -1 to 1
    double Confidence
);
```

### VertexAIService.cs

```csharp
using Google.Cloud.AIPlatform.V1;
using Google.Protobuf.WellKnownTypes;

namespace VehicleIntelligenceService.Infrastructure.Services;

public class VertexAIService : IVertexAIService
{
    private readonly PredictionServiceClient _client;
    private readonly string _projectId;
    private readonly string _location;
    private readonly ILogger<VertexAIService> _logger;

    private const string PricingEndpointId = "4567890123456789";
    private const string EmbeddingModel = "textembedding-gecko@001";
    private const string ForecastingEndpointId = "9876543210987654";

    public VertexAIService(
        PredictionServiceClient client,
        string projectId,
        string location,
        ILogger<VertexAIService> logger)
    {
        _client = client;
        _projectId = projectId;
        _location = location;
        _logger = logger;
    }

    /// <summary>
    /// Predice precio usando modelo entrenado en Vertex AI
    /// </summary>
    public async Task<decimal> PredictVehiclePriceAsync(
        VehiclePricingInput input,
        CancellationToken ct)
    {
        try
        {
            var endpoint = EndpointName.FromProjectLocationEndpoint(
                _projectId,
                _location,
                PricingEndpointId
            );

            var instances = new Value
            {
                StructValue = new Struct
                {
                    Fields =
                    {
                        { "year", Value.ForNumber(input.Year) },
                        { "mileage", Value.ForNumber(input.Mileage) },
                        { "make", Value.ForString(input.Make) },
                        { "model", Value.ForString(input.Model) },
                        { "fuel_type", Value.ForString(input.FuelType) },
                        { "transmission", Value.ForString(input.Transmission) },
                        { "condition", Value.ForString(input.Condition) },
                        { "days_on_market", Value.ForNumber(input.DaysOnMarket) }
                    }
                }
            };

            var request = new PredictRequest
            {
                EndpointAsEndpointName = endpoint,
                Instances = { instances }
            };

            var response = await _client.PredictAsync(request, cancellationToken: ct);

            var predictedPrice = response.Predictions[0]
                .StructValue
                .Fields["value"]
                .NumberValue;

            _logger.LogInformation(
                $"Price prediction for {input.Year} {input.Make} {input.Model}: RD${predictedPrice}"
            );

            return (decimal)predictedPrice;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Pricing prediction error: {ex.Message}");
            throw new ServiceException("Error en predicción de precio", ex);
        }
    }

    /// <summary>
    /// Obtiene embedding de un texto
    /// </summary>
    public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken ct)
    {
        try
        {
            var publisher = PublisherName.FromProjectPublisher(_projectId, "google");
            var model = PublisherModelName.FromPublisherModel(publisher, EmbeddingModel);

            var instances = new Value
            {
                StructValue = new Struct
                {
                    Fields =
                    {
                        { "content", Value.ForString(text) }
                    }
                }
            };

            var request = new PredictRequest
            {
                EndpointAsPublisherModelName = model,
                Instances = { instances }
            };

            var response = await _client.PredictAsync(request, cancellationToken: ct);

            var embedding = response.Predictions[0]
                .StructValue
                .Fields["embeddings"]
                .StructValue
                .Fields["values"]
                .ListValue
                .Values
                .Select(v => (float)v.NumberValue)
                .ToArray();

            return embedding;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Embedding API error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Predice demanda futura
    /// </summary>
    public async Task<DemandForecastOutput> ForecastDemandAsync(
        DemandForecastInput input,
        CancellationToken ct)
    {
        try
        {
            var endpoint = EndpointName.FromProjectLocationEndpoint(
                _projectId,
                _location,
                ForecastingEndpointId
            );

            var instances = new Value
            {
                StructValue = new Struct
                {
                    Fields =
                    {
                        { "category", Value.ForString(input.Category) },
                        { "month", Value.ForNumber(input.Month) },
                        { "year", Value.ForNumber(input.Year) },
                        { "avg_price", Value.ForNumber((double)input.AveragePrice) }
                    }
                }
            };

            var request = new PredictRequest
            {
                EndpointAsEndpointName = endpoint,
                Instances = { instances }
            };

            var response = await _client.PredictAsync(request, cancellationToken: ct);

            var predictions = response.Predictions[0].StructValue.Fields;

            return new DemandForecastOutput(
                ExpectedListings: (int)predictions["expected_listings"].NumberValue,
                TrendDirection: (decimal)predictions["trend_direction"].NumberValue,
                Confidence: predictions["confidence"].NumberValue
            );
        }
        catch (Exception ex)
        {
            _logger.LogError($"Forecasting error: {ex.Message}");
            throw;
        }
    }
}
```

### Usar en Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class VehicleIntelligenceController : ControllerBase
{
    private readonly IVertexAIService _vertexService;

    public VehicleIntelligenceController(IVertexAIService vertexService)
    {
        _vertexService = vertexService;
    }

    [HttpPost("predict-price")]
    public async Task<IActionResult> PredictPrice(
        [FromBody] PredictPriceRequest request,
        CancellationToken ct)
    {
        var input = new VehiclePricingInput(
            Year: request.Year,
            Mileage: request.Mileage,
            Make: request.Make,
            Model: request.Model,
            FuelType: request.FuelType,
            Transmission: request.Transmission,
            Condition: request.Condition,
            DaysOnMarket: request.DaysOnMarket
        );

        var predictedPrice = await _vertexService.PredictVehiclePriceAsync(input, ct);

        return Ok(new
        {
            vehicle = $"{request.Year} {request.Make} {request.Model}",
            predictedPriceRD = predictedPrice,
            confidence = 0.87,
            recommendation = "Precio competitivo en el mercado"
        });
    }

    [HttpPost("similar-vehicles")]
    public async Task<IActionResult> FindSimilarVehicles(
        [FromBody] FindSimilarRequest request,
        CancellationToken ct)
    {
        var embedding = await _vertexService.GetEmbeddingAsync(request.Description, ct);

        // TODO: Buscar en vector database (pgvector)

        return Ok(new
        {
            embedding = embedding,
            dimension = embedding.Length
        });
    }
}
```

---

## 💰 Pricing

### Google Vertex AI

| Servicio             | Precio                     |
| -------------------- | -------------------------- |
| Prediction API calls | $0.01 per 1000 calls       |
| Training (AutoML)    | $6/hour                    |
| Custom training      | $0.35/hour (n1-standard-4) |
| Text Embeddings      | Gratis (tier)/$0.025/1M    |

### Estimación Mensual

```
Predictions (100K vehiculos):
- 50M API calls = $500

BigQuery (queries):
- 100 GB scanned = $625

Vertex AI Storage:
- 50 GB modelo = $100

────────────────────────
TOTAL: ~$1,225/mes
```

---

## ⚙️ Setup en GCP

```bash
# 1. Crear proyecto
gcloud projects create okla-ai

# 2. Habilitar APIs
gcloud services enable aiplatform.googleapis.com
gcloud services enable bigquery.googleapis.com
gcloud services enable storage-api.googleapis.com

# 3. Crear service account
gcloud iam service-accounts create vertex-ai-okla

# 4. Otorgar permisos
gcloud projects add-iam-policy-binding okla-ai \
  --member="serviceAccount:vertex-ai-okla@okla-ai.iam.gserviceaccount.com" \
  --role="roles/aiplatform.admin"

# 5. Crear key
gcloud iam service-accounts keys create credentials.json \
  --iam-account=vertex-ai-okla@okla-ai.iam.gserviceaccount.com

# 6. Guardar en Kubernetes
kubectl create secret generic google-cloud-credentials \
  --from-file=key.json=credentials.json \
  -n okla
```

---

## ✅ Checklist

- [ ] Crear proyecto GCP
- [ ] Habilitar Vertex AI API
- [ ] Entrenar modelo de pricing
- [ ] Crear embeddings model
- [ ] Deployment de modelos
- [ ] Crear service account
- [ ] Guardar credentials en K8s
- [ ] Implementar IVertexAIService
- [ ] Testing con datos reales
- [ ] Monitor costos en GCP console

---

_Documentación Google Vertex AI para OKLA_  
_Última actualización: Enero 15, 2026_
