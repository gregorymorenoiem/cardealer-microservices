# 🛠️ AWS SageMaker - Documentación Completa

**Versión:** SageMaker Studio  
**Costo:** $0.25-$5/hora (training), $0.01-$1/hora (inference)  
**Latencia:** <500ms (inference)  
**Modelos:** XGBoost, LightGBM, DeepAR, etc.

---

## 📖 Introducción

**AWS SageMaker** es la plataforma de ML fully-managed de Amazon que permite:

- Entrenar modelos custom (XGBoost, TensorFlow, PyTorch)
- Hosting de modelos en producción
- Batch predictions
- AutoML

### Uso en OKLA:

1. **LeadScoringService**: Clasificar leads (Hot/Warm/Cold)
2. **VehicleIntelligenceService**: Alternativa para pricing
3. **DataPipelineService**: Time series forecasting

---

## 🎯 Caso Principal: Lead Scoring con XGBoost

### Dataset

```csv
user_id,profile_score,listings_viewed,days_since_last_activity,
category_interested,budget_range,contact_attempts,is_hot_lead
1,85,25,2,SUV,1500000-2000000,3,1
2,45,5,30,Sedan,1000000-1500000,0,0
3,92,45,1,SUV,2000000+,5,1
...
```

### Proceso en SageMaker

```bash
# 1. Preparar datos en S3
aws s3 cp lead_data.csv s3://okla-sagemaker/training/lead_data.csv

# 2. Crear training job
aws sagemaker create-training-job \
  --training-job-name lead-scoring-v1 \
  --role-arn arn:aws:iam::ACCOUNT:role/SageMakerRole \
  --algorithm-specification TrainingImage=246618743249.dkr.ecr.us-east-1.amazonaws.com/sagemaker-xgboost:latest \
  --input-data-config ChannelName=training,DataSource={S3DataSource={S3Uri=s3://okla-sagemaker/training/,S3DataType=S3Prefix}} \
  --output-data-config S3OutputPath=s3://okla-sagemaker/output/ \
  --resource-config InstanceType=ml.m5.xlarge,InstanceCount=1,VolumeSizeInGB=30

# 3. Esperar a que termine (5-10 minutos)

# 4. Deploy modelo
aws sagemaker create-endpoint-config \
  --endpoint-config-name lead-scoring-config \
  --production-variants VariantName=Primary,ModelName=lead-scoring-v1,InstanceType=ml.t2.medium,InitialInstanceCount=1

aws sagemaker create-endpoint \
  --endpoint-name lead-scoring-endpoint \
  --endpoint-config-name lead-scoring-config
```

---

## 💻 Implementación C#

### NuGet Packages

```bash
dotnet add package AWSSDK.SageMakerRuntime
dotnet add package AWSSDK.SageMaker
```

### ISageMakerService.cs

```csharp
public interface ISageMakerService
{
    Task<LeadScoreResult> ScoreLeadAsync(
        LeadScoringInput input,
        CancellationToken ct
    );

    Task<string> TrainModelAsync(
        string trainingJobName,
        string s3DataPath,
        CancellationToken ct
    );
}

public record LeadScoringInput(
    int ProfileScore,
    int ListingsViewed,
    int DaysSinceLastActivity,
    string CategoryInterested,
    decimal BudgetMin,
    decimal BudgetMax,
    int ContactAttempts
);

public record LeadScoreResult(
    double HotProbability,
    double WarmProbability,
    double ColdProbability,
    string Classification
);
```

### SageMakerService.cs

```csharp
using Amazon.SageMakerRuntime;
using Amazon.SageMakerRuntime.Model;
using Amazon.SageMaker;
using Amazon.SageMaker.Model;
using System.Text.Json;

public class SageMakerService : ISageMakerService
{
    private readonly AmazonSageMakerRuntimeClient _runtimeClient;
    private readonly AmazonSageMakerClient _sageMakerClient;
    private readonly ILogger<SageMakerService> _logger;

    private const string LeadScoringEndpoint = "lead-scoring-endpoint";

    public SageMakerService(
        AmazonSageMakerRuntimeClient runtimeClient,
        AmazonSageMakerClient sageMakerClient,
        ILogger<SageMakerService> logger)
    {
        _runtimeClient = runtimeClient;
        _sageMakerClient = sageMakerClient;
        _logger = logger;
    }

    /// <summary>
    /// Predice si un lead es Hot/Warm/Cold
    /// </summary>
    public async Task<LeadScoreResult> ScoreLeadAsync(
        LeadScoringInput input,
        CancellationToken ct)
    {
        try
        {
            // Preparar features en orden del training
            var features = new[]
            {
                input.ProfileScore.ToString(),
                input.ListingsViewed.ToString(),
                input.DaysSinceLastActivity.ToString(),
                input.CategoryInterested switch
                {
                    "SUV" => "1",
                    "Sedan" => "2",
                    "Truck" => "3",
                    _ => "0"
                },
                input.BudgetMin.ToString(),
                input.BudgetMax.ToString(),
                input.ContactAttempts.ToString()
            };

            var payload = string.Join(",", features);

            var request = new InvokeEndpointRequest
            {
                EndpointName = LeadScoringEndpoint,
                Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(payload)),
                ContentType = "text/csv"
            };

            var response = await _runtimeClient.InvokeEndpointAsync(request, ct);

            // Parsear respuesta (probabilidades)
            using var reader = new StreamReader(response.Body);
            var predictions = await reader.ReadToEndAsync();

            // XGBoost retorna: hot_prob, warm_prob, cold_prob
            var probs = predictions.Split(',').Select(p => double.Parse(p)).ToArray();

            var classification = (probs[0], probs[1], probs[2]) switch
            {
                var (h, w, c) when h > 0.7 => "Hot",
                var (h, w, c) when w > 0.6 => "Warm",
                _ => "Cold"
            };

            var result = new LeadScoreResult(
                HotProbability: probs[0],
                WarmProbability: probs[1],
                ColdProbability: probs[2],
                Classification: classification
            );

            _logger.LogInformation($"Lead scored: {classification} ({probs[0]:P})");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"SageMaker inference error: {ex.Message}");
            throw new ServiceException("Error scoring lead", ex);
        }
    }

    /// <summary>
    /// Entrena nuevo modelo
    /// </summary>
    public async Task<string> TrainModelAsync(
        string trainingJobName,
        string s3DataPath,
        CancellationToken ct)
    {
        try
        {
            var request = new CreateTrainingJobRequest
            {
                TrainingJobName = trainingJobName,
                RoleArn = "arn:aws:iam::ACCOUNT:role/SageMakerRole",
                AlgorithmSpecification = new AlgorithmSpecification
                {
                    TrainingImage = "246618743249.dkr.ecr.us-east-1.amazonaws.com/sagemaker-xgboost:latest",
                    TrainingInputMode = TrainingInputMode.File
                },
                InputDataConfig = new List<Channel>
                {
                    new Channel
                    {
                        ChannelName = "training",
                        DataSource = new DataSource
                        {
                            S3DataSource = new S3DataSource
                            {
                                S3Uri = s3DataPath,
                                S3DataType = S3DataType.S3Prefix
                            }
                        }
                    }
                },
                OutputDataConfig = new OutputDataConfig
                {
                    S3OutputPath = "s3://okla-sagemaker/output/"
                },
                ResourceConfig = new ResourceConfig
                {
                    InstanceType = TrainingInstanceType.Ml_m5_xlarge,
                    InstanceCount = 1,
                    VolumeSizeInGB = 30
                }
            };

            var response = await _sageMakerClient.CreateTrainingJobAsync(request, ct);

            _logger.LogInformation($"Training job started: {response.TrainingJobArn}");

            return response.TrainingJobArn;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Training job creation error: {ex.Message}");
            throw;
        }
    }
}
```

### Usar en Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class LeadScoringController : ControllerBase
{
    private readonly ISageMakerService _sageMakerService;

    public LeadScoringController(ISageMakerService sageMakerService)
    {
        _sageMakerService = sageMakerService;
    }

    [HttpPost("score")]
    public async Task<IActionResult> ScoreLead(
        [FromBody] LeadScoringRequest request,
        CancellationToken ct)
    {
        var input = new LeadScoringInput(
            ProfileScore: request.ProfileScore,
            ListingsViewed: request.ListingsViewed,
            DaysSinceLastActivity: request.DaysSinceLastActivity,
            CategoryInterested: request.CategoryInterested,
            BudgetMin: request.BudgetMin,
            BudgetMax: request.BudgetMax,
            ContactAttempts: request.ContactAttempts
        );

        var result = await _sageMakerService.ScoreLeadAsync(input, ct);

        return Ok(new
        {
            classification = result.Classification,
            confidence = result.HotProbability,
            probabilities = new
            {
                hot = result.HotProbability,
                warm = result.WarmProbability,
                cold = result.ColdProbability
            }
        });
    }
}
```

---

## 💰 Pricing

### Training

```
XGBoost Training:
- ml.m5.xlarge: $0.25/hora
- Duración typical: 5-10 minutos
- Costo por entrenamiento: ~$0.02-$0.05

Por mes (3 entrenamientos):
- Total: ~$0.15
```

### Inference

```
Endpoint (ml.t2.medium):
- Always-on: $0.07/hora
- Costo mensual: ~$51

Alternativa: Serverless (pay-per-request)
- $0.0000175 per inference
- 100K inferences/mes = $1.75
```

---

## ✅ Checklist

- [ ] Crear IAM Role para SageMaker
- [ ] Preparar datos de entrenamiento
- [ ] Upload datos a S3
- [ ] Entrenar modelo XGBoost
- [ ] Crear endpoint para inference
- [ ] Implementar ISageMakerService
- [ ] Testing con datos reales
- [ ] Deploy a Kubernetes
- [ ] Monitor latencia y costos

---

_Documentación AWS SageMaker para OKLA_  
_Última actualización: Enero 15, 2026_
