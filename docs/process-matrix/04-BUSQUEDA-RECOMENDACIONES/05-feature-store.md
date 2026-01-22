# 🧠 Feature Store Service - Almacén de Features para ML - Matriz de Procesos

> **Servicio:** FeatureStoreService  
> **Puerto:** 5053  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 1. Información General

### 1.1 Descripción

Sistema centralizado de almacenamiento y servicio de features (características) para modelos de Machine Learning. Proporciona una capa de abstracción entre los datos raw y los modelos ML, asegurando consistencia entre entrenamiento e inferencia.

### 1.2 Propósito

- **Feature Engineering:** Transformaciones reutilizables de datos
- **Feature Serving:** Baja latencia para inferencia en tiempo real
- **Feature Discovery:** Catálogo de features disponibles
- **Feature Monitoring:** Drift detection y calidad de datos
- **Feature Versioning:** Versionado de transformaciones

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                       Feature Store Architecture                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Data Sources                                                          │
│   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                     │
│   │ PostgreSQL  │  │ Event       │  │ External    │                     │
│   │ (Entities)  │  │ Tracking    │  │ APIs        │                     │
│   └──────┬──────┘  └──────┬──────┘  └──────┬──────┘                     │
│          │                │                │                            │
│          └────────────────┼────────────────┘                            │
│                           │                                              │
│                           ▼                                              │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                    Feature Engineering                           │   │
│   │                                                                   │   │
│   │   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │   │
│   │   │ Batch        │  │ Stream       │  │ On-Demand    │          │   │
│   │   │ Pipelines    │  │ Processing   │  │ Computation  │          │   │
│   │   │ (Daily)      │  │ (Real-time)  │  │ (Request)    │          │   │
│   │   └──────┬───────┘  └──────┬───────┘  └──────┬───────┘          │   │
│   │          │                 │                 │                   │   │
│   └──────────┼─────────────────┼─────────────────┼───────────────────┘   │
│              │                 │                 │                       │
│              ▼                 ▼                 ▼                       │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                       Feature Store                              │   │
│   │                                                                   │   │
│   │   ┌────────────────────────────────────────────────────────┐    │   │
│   │   │              Offline Store (PostgreSQL)                 │    │   │
│   │   │   - Historical features                                 │    │   │
│   │   │   - Point-in-time correct joins                        │    │   │
│   │   │   - Training data generation                           │    │   │
│   │   └────────────────────────────────────────────────────────┘    │   │
│   │                                                                   │   │
│   │   ┌────────────────────────────────────────────────────────┐    │   │
│   │   │              Online Store (Redis)                       │    │   │
│   │   │   - Latest feature values                               │    │   │
│   │   │   - Low-latency serving (<10ms)                        │    │   │
│   │   │   - Real-time inference                                │    │   │
│   │   └────────────────────────────────────────────────────────┘    │   │
│   │                                                                   │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│              │                                                          │
│              ▼                                                          │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                        ML Models                                 │   │
│   │                                                                   │   │
│   │   ┌────────────────┐  ┌────────────────┐  ┌────────────────┐    │   │
│   │   │ Lead Scoring   │  │ Price          │  │ Recommendation │    │   │
│   │   │ Model          │  │ Prediction     │  │ Engine         │    │   │
│   │   └────────────────┘  └────────────────┘  └────────────────┘    │   │
│   │                                                                   │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

### 2.1 Feature Registry

| Método   | Endpoint               | Descripción                 | Auth    |
| -------- | ---------------------- | --------------------------- | ------- |
| `GET`    | `/api/features`        | Listar features registrados | Service |
| `GET`    | `/api/features/{name}` | Obtener feature por nombre  | Service |
| `POST`   | `/api/features`        | Registrar nueva feature     | Admin   |
| `PUT`    | `/api/features/{name}` | Actualizar feature          | Admin   |
| `DELETE` | `/api/features/{name}` | Eliminar feature            | Admin   |

### 2.2 Feature Serving (Online)

| Método | Endpoint                                                  | Descripción                       | Auth    |
| ------ | --------------------------------------------------------- | --------------------------------- | ------- |
| `GET`  | `/api/features/online/{entity}/{entityId}`                | Features para entidad             | Service |
| `POST` | `/api/features/online/batch`                              | Features para múltiples entidades | Service |
| `GET`  | `/api/features/online/vector/{featureSetName}/{entityId}` | Feature vector completo           | Service |

### 2.3 Feature Serving (Offline/Training)

| Método | Endpoint                                            | Descripción                 | Auth    |
| ------ | --------------------------------------------------- | --------------------------- | ------- |
| `POST` | `/api/features/offline/training-data`               | Generar dataset de training | Service |
| `POST` | `/api/features/offline/point-in-time`               | Point-in-time join          | Service |
| `GET`  | `/api/features/offline/history/{entity}/{entityId}` | Historial de features       | Service |

### 2.4 Feature Sets

| Método | Endpoint                   | Descripción            | Auth    |
| ------ | -------------------------- | ---------------------- | ------- |
| `GET`  | `/api/feature-sets`        | Listar feature sets    | Service |
| `GET`  | `/api/feature-sets/{name}` | Obtener feature set    | Service |
| `POST` | `/api/feature-sets`        | Crear feature set      | Admin   |
| `PUT`  | `/api/feature-sets/{name}` | Actualizar feature set | Admin   |

### 2.5 Monitoring

| Método | Endpoint                             | Descripción             | Auth  |
| ------ | ------------------------------------ | ----------------------- | ----- |
| `GET`  | `/api/features/monitoring/drift`     | Feature drift detection | Admin |
| `GET`  | `/api/features/monitoring/freshness` | Feature freshness       | Admin |
| `GET`  | `/api/features/monitoring/quality`   | Data quality metrics    | Admin |

---

## 3. Entidades

### 3.1 FeatureDefinition

```csharp
public class FeatureDefinition
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Data type
    public FeatureDataType DataType { get; set; }
    public string? DataTypeSchema { get; set; } // JSON schema for complex types

    // Entity
    public string EntityType { get; set; } = string.Empty; // Vehicle, User, Dealer
    public string EntityIdField { get; set; } = "Id";

    // Computation
    public FeatureComputationType ComputationType { get; set; }
    public string? TransformationSql { get; set; }
    public string? TransformationCode { get; set; }

    // Source
    public string SourceTable { get; set; } = string.Empty;
    public string? SourceService { get; set; }

    // Versioning
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // TTL for online store
    public int OnlineTtlMinutes { get; set; } = 60;

    // Metadata
    public string Owner { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public enum FeatureDataType
{
    Integer,
    Float,
    String,
    Boolean,
    DateTime,
    Array,
    Embedding
}

public enum FeatureComputationType
{
    Direct,        // Columna directa de la fuente
    Aggregation,   // Agregación (count, sum, avg)
    Transformation,// Transformación SQL
    Derived,       // Calculada de otras features
    External       // De servicio externo
}
```

### 3.2 FeatureSet

```csharp
public class FeatureSet
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Entity
    public string EntityType { get; set; } = string.Empty;

    // Features
    public List<string> FeatureNames { get; set; } = new();

    // Use case
    public string UseCase { get; set; } = string.Empty; // lead_scoring, pricing, recommendations

    // Version
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### 3.3 FeatureValue (Online Store)

```csharp
public class FeatureValue
{
    public string FeatureName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;

    public object Value { get; set; } = null!;
    public DateTime ComputedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    // For Redis key: feature:{entity_type}:{entity_id}:{feature_name}
    public string CacheKey => $"feature:{EntityType}:{EntityId}:{FeatureName}";
}
```

### 3.4 FeatureHistoryRecord (Offline Store)

```csharp
public class FeatureHistoryRecord
{
    public Guid Id { get; set; }
    public string FeatureName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }

    public string ValueJson { get; set; } = string.Empty;
    public DateTime EventTime { get; set; }
    public DateTime CreatedAt { get; set; }

    // For point-in-time correct joins
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}
```

---

## 4. Features por Entidad

### 4.1 Vehicle Features

| Feature Name                 | Type      | Computation | Description              |
| ---------------------------- | --------- | ----------- | ------------------------ |
| `vehicle_age_days`           | Integer   | Derived     | Días desde publicación   |
| `vehicle_price`              | Float     | Direct      | Precio actual            |
| `vehicle_price_per_km`       | Float     | Derived     | Precio / kilometraje     |
| `vehicle_view_count_7d`      | Integer   | Aggregation | Vistas últimos 7 días    |
| `vehicle_contact_count_7d`   | Integer   | Aggregation | Contactos últimos 7 días |
| `vehicle_favorite_count`     | Integer   | Aggregation | Total favoritos          |
| `vehicle_price_vs_market`    | Float     | Derived     | % sobre/bajo mercado     |
| `vehicle_condition_score`    | Float     | Derived     | Score de condición       |
| `vehicle_photo_count`        | Integer   | Direct      | Cantidad de fotos        |
| `vehicle_has_video`          | Boolean   | Direct      | Tiene video              |
| `vehicle_description_length` | Integer   | Derived     | Largo de descripción     |
| `vehicle_dealer_verified`    | Boolean   | Direct      | Dealer verificado        |
| `vehicle_embedding`          | Embedding | External    | Vector embedding         |

### 4.2 User Features

| Feature Name                 | Type    | Computation | Description              |
| ---------------------------- | ------- | ----------- | ------------------------ |
| `user_search_count_30d`      | Integer | Aggregation | Búsquedas 30 días        |
| `user_view_count_30d`        | Integer | Aggregation | Vistas 30 días           |
| `user_avg_price_viewed`      | Float   | Aggregation | Precio promedio visto    |
| `user_preferred_makes`       | Array   | Aggregation | Marcas más vistas        |
| `user_preferred_types`       | Array   | Aggregation | Tipos más vistos         |
| `user_session_duration_avg`  | Float   | Aggregation | Duración promedio sesión |
| `user_days_since_last_visit` | Integer | Derived     | Días desde última visita |
| `user_is_returning`          | Boolean | Derived     | Usuario recurrente       |
| `user_contact_rate`          | Float   | Derived     | Tasa de contacto         |
| `user_favorite_count`        | Integer | Aggregation | Total favoritos          |

### 4.3 Lead Features

| Feature Name                   | Type    | Computation | Description               |
| ------------------------------ | ------- | ----------- | ------------------------- |
| `lead_response_time_min`       | Float   | Derived     | Tiempo primera respuesta  |
| `lead_message_count`           | Integer | Aggregation | Total mensajes            |
| `lead_user_engagement_score`   | Float   | Derived     | Score de engagement       |
| `lead_vehicle_price_vs_budget` | Float   | Derived     | Precio vs presupuesto     |
| `lead_user_search_match`       | Float   | Derived     | Match con búsquedas       |
| `lead_time_since_contact_hr`   | Float   | Derived     | Horas desde contacto      |
| `lead_financing_interest`      | Boolean | Direct      | Interés en financiamiento |
| `lead_test_drive_requested`    | Boolean | Direct      | Pidió test drive          |

### 4.4 Dealer Features

| Feature Name                | Type    | Computation | Description               |
| --------------------------- | ------- | ----------- | ------------------------- |
| `dealer_active_listings`    | Integer | Aggregation | Vehículos activos         |
| `dealer_avg_response_time`  | Float   | Aggregation | Tiempo respuesta promedio |
| `dealer_rating_avg`         | Float   | Aggregation | Rating promedio           |
| `dealer_review_count`       | Integer | Aggregation | Total reviews             |
| `dealer_sales_30d`          | Integer | Aggregation | Ventas 30 días            |
| `dealer_conversion_rate`    | Float   | Derived     | Tasa de conversión        |
| `dealer_days_on_market_avg` | Float   | Aggregation | Días promedio en mercado  |
| `dealer_is_verified`        | Boolean | Direct      | Dealer verificado         |
| `dealer_years_active`       | Float   | Derived     | Años en la plataforma     |

---

## 5. Procesos Detallados

### 5.1 FS-001: Obtener Features para Inferencia

| Paso | Acción                         | Sistema      | Validación          |
| ---- | ------------------------------ | ------------ | ------------------- |
| 1    | ML Service request features    | FeatureStore | Valid entity ID     |
| 2    | Parse feature set name         | FeatureStore | Feature set exists  |
| 3    | Get feature definitions        | FeatureStore | All features active |
| 4    | Check Redis for each feature   | Redis        | Cache lookup        |
| 5    | For missing: compute on-demand | FeatureStore | Transformation      |
| 6    | Store computed in Redis        | Redis        | TTL applied         |
| 7    | Assemble feature vector        | FeatureStore | Order preserved     |
| 8    | Return feature vector          | API          | < 10ms target       |

```csharp
public class OnlineFeatureService
{
    private readonly IDistributedCache _cache;
    private readonly IFeatureComputer _computer;

    public async Task<FeatureVector> GetFeatureVectorAsync(
        string featureSetName,
        string entityType,
        string entityId,
        CancellationToken ct = default)
    {
        // 1. Get feature set definition
        var featureSet = await _repository.GetFeatureSetAsync(featureSetName, ct);
        if (featureSet == null)
            throw new NotFoundException($"Feature set {featureSetName} not found");

        var features = new Dictionary<string, object>();
        var missingFeatures = new List<FeatureDefinition>();

        // 2. Try to get each feature from Redis
        foreach (var featureName in featureSet.FeatureNames)
        {
            var cacheKey = $"feature:{entityType}:{entityId}:{featureName}";
            var cached = await _cache.GetStringAsync(cacheKey, ct);

            if (!string.IsNullOrEmpty(cached))
            {
                features[featureName] = JsonSerializer.Deserialize<object>(cached)!;
            }
            else
            {
                var definition = await _repository.GetFeatureAsync(featureName, ct);
                if (definition != null)
                    missingFeatures.Add(definition);
            }
        }

        // 3. Compute missing features on-demand
        if (missingFeatures.Any())
        {
            var computed = await _computer.ComputeFeaturesAsync(
                entityType, entityId, missingFeatures, ct);

            foreach (var (name, value) in computed)
            {
                features[name] = value;

                // Store in Redis with TTL
                var definition = missingFeatures.First(f => f.Name == name);
                var cacheKey = $"feature:{entityType}:{entityId}:{name}";

                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(value),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow =
                            TimeSpan.FromMinutes(definition.OnlineTtlMinutes)
                    },
                    ct);
            }
        }

        // 4. Assemble ordered feature vector
        return new FeatureVector
        {
            EntityType = entityType,
            EntityId = entityId,
            FeatureSetName = featureSetName,
            Features = features,
            ComputedAt = DateTime.UtcNow
        };
    }
}
```

### 5.2 FS-002: Generar Training Dataset

| Paso | Acción                       | Sistema      | Validación           |
| ---- | ---------------------------- | ------------ | -------------------- |
| 1    | ML Training request dataset  | FeatureStore | Feature set + labels |
| 2    | Get entity IDs with labels   | FeatureStore | Historical data      |
| 3    | For each entity + timestamp  | FeatureStore | Point-in-time        |
| 4    | Join features at timestamp   | PostgreSQL   | Temporal join        |
| 5    | Avoid data leakage           | FeatureStore | Only past data       |
| 6    | Assemble training rows       | FeatureStore | Features + labels    |
| 7    | Return dataset (CSV/Parquet) | API          | File download        |

```csharp
public class OfflineFeatureService
{
    public async Task<TrainingDataset> GenerateTrainingDataAsync(
        TrainingDataRequest request,
        CancellationToken ct = default)
    {
        var featureSet = await _repository.GetFeatureSetAsync(
            request.FeatureSetName, ct);

        var rows = new List<TrainingRow>();

        // For each entity with label
        foreach (var entity in request.Entities)
        {
            // Point-in-time correct feature retrieval
            var features = await GetFeaturesAtTimeAsync(
                featureSet,
                entity.EntityType,
                entity.EntityId,
                entity.EventTime, // The time when the label was recorded
                ct);

            rows.Add(new TrainingRow
            {
                EntityId = entity.EntityId,
                Features = features,
                Label = entity.Label,
                EventTime = entity.EventTime
            });
        }

        return new TrainingDataset
        {
            FeatureSetName = request.FeatureSetName,
            Rows = rows,
            FeatureNames = featureSet.FeatureNames,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private async Task<Dictionary<string, object>> GetFeaturesAtTimeAsync(
        FeatureSet featureSet,
        string entityType,
        Guid entityId,
        DateTime eventTime,
        CancellationToken ct)
    {
        var features = new Dictionary<string, object>();

        foreach (var featureName in featureSet.FeatureNames)
        {
            // Get the feature value that was valid at eventTime
            // This avoids data leakage (using future information)
            var historyRecord = await _historyRepository.GetLatestBeforeAsync(
                featureName,
                entityType,
                entityId,
                eventTime, // Only features computed BEFORE this time
                ct);

            if (historyRecord != null)
            {
                features[featureName] = JsonSerializer.Deserialize<object>(
                    historyRecord.ValueJson)!;
            }
            else
            {
                // Use default value or null
                features[featureName] = GetDefaultValue(featureName);
            }
        }

        return features;
    }
}
```

### 5.3 FS-003: Batch Feature Materialization (Job)

| Paso | Acción                         | Sistema      | Validación       |
| ---- | ------------------------------ | ------------ | ---------------- |
| 1    | Scheduler triggers batch job   | Scheduler    | Cron configured  |
| 2    | Get all active features        | FeatureStore | Batch = true     |
| 3    | For each feature definition    | FeatureStore | Loop             |
| 4    | Execute transformation SQL     | PostgreSQL   | Query execution  |
| 5    | Store results in offline store | PostgreSQL   | History records  |
| 6    | Update online store            | Redis        | Latest values    |
| 7    | Calculate feature statistics   | FeatureStore | Mean, std, etc   |
| 8    | Detect drift if enabled        | FeatureStore | Statistical test |
| 9    | Log completion                 | Monitoring   | Metrics          |

```csharp
public class BatchMaterializationJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var features = await _repository.GetBatchFeaturesAsync();

        foreach (var feature in features)
        {
            try
            {
                // Execute transformation
                var results = await ExecuteTransformationAsync(feature);

                // Store in offline store (history)
                await StoreHistoryAsync(feature, results);

                // Update online store (Redis) with latest values
                await UpdateOnlineStoreAsync(feature, results);

                // Calculate statistics for monitoring
                await CalculateStatisticsAsync(feature, results);

                // Detect drift
                if (feature.DriftDetectionEnabled)
                {
                    await DetectDriftAsync(feature, results);
                }

                _logger.LogInformation(
                    "Materialized {Feature} with {Count} values",
                    feature.Name, results.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to materialize {Feature}", feature.Name);
                await PublishAlertAsync(feature, ex);
            }
        }
    }
}
```

---

## 6. Reglas de Negocio

| Código | Regla                                            | Validación        |
| ------ | ------------------------------------------------ | ----------------- |
| FS-R01 | Features deben tener nombres únicos              | Unique constraint |
| FS-R02 | Feature sets no pueden tener features duplicadas | Validation        |
| FS-R03 | Online store TTL mínimo 1 minuto                 | TTL >= 60s        |
| FS-R04 | Historial máximo 1 año                           | Cleanup job       |
| FS-R05 | Latencia online < 10ms P99                       | SLO               |
| FS-R06 | Training data: solo datos pasados                | No leakage        |

---

## 7. Códigos de Error

| Código   | HTTP | Mensaje                    | Causa                   |
| -------- | ---- | -------------------------- | ----------------------- |
| `FS_001` | 404  | Feature not found          | Feature no existe       |
| `FS_002` | 404  | Feature set not found      | Feature set no existe   |
| `FS_003` | 400  | Entity not found           | Entidad no existe       |
| `FS_004` | 500  | Computation failed         | Error en transformación |
| `FS_005` | 400  | Invalid feature definition | Definición inválida     |
| `FS_006` | 503  | Feature store unavailable  | Store no disponible     |

---

## 8. Eventos RabbitMQ

| Evento                      | Exchange               | Descripción             |
| --------------------------- | ---------------------- | ----------------------- |
| `FeatureMaterializedEvent`  | `feature-store.events` | Feature materializada   |
| `FeatureDriftDetectedEvent` | `feature-store.events` | Drift detectado         |
| `FeatureSetUpdatedEvent`    | `feature-store.events` | Feature set actualizado |

---

## 9. Configuración

```json
{
  "FeatureStore": {
    "OnlineStore": {
      "Redis": "${REDIS_CONNECTION}",
      "DefaultTtlMinutes": 60,
      "MaxLatencyMs": 10
    },
    "OfflineStore": {
      "PostgreSQL": "${POSTGRES_CONNECTION}",
      "RetentionDays": 365
    },
    "Materialization": {
      "BatchCron": "0 * * * *",
      "ParallelDegree": 4
    },
    "DriftDetection": {
      "Enabled": true,
      "ThresholdPct": 10,
      "AlertOnDrift": true
    }
  }
}
```

---

## 10. Métricas Prometheus

```
# Feature serving
feature_store_online_requests_total{feature_set="..."}
feature_store_online_latency_ms{quantile="0.5|0.95|0.99"}
feature_store_cache_hit_rate

# Materialization
feature_store_materialization_duration_seconds{feature="..."}
feature_store_materialization_rows_total{feature="..."}

# Quality
feature_store_null_rate{feature="..."}
feature_store_drift_score{feature="..."}
```

---

## 📚 Referencias

- [03-lead-scoring.md](../06-CRM-LEADS-CONTACTOS/03-lead-scoring.md) - Lead scoring ML
- [04-vehicle-intelligence.md](../03-VEHICULOS-INVENTARIO/04-vehicle-intelligence.md) - Vehicle pricing ML
- [02-recommendation-service.md](02-recommendation-service.md) - Recommendations ML
