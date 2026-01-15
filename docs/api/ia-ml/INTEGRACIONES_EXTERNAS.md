# 🔌 APIs Externas a Consumir - OKLA IA/ML

**Fecha:** Enero 15, 2026  
**Objetivo:** Documentar todas las dependencias externas del sistema de Data & ML

---

## 📊 Índice de APIs Externas

| #   | Servicio           | Tipo            | Costo    | Estado         | Prioridad  |
| --- | ------------------ | --------------- | -------- | -------------- | ---------- |
| 1   | Google BigQuery    | Data Warehouse  | $6.25/TB | ⚠️ Recomendado | 🔴 CRÍTICO |
| 2   | Apache Kafka       | Event Streaming | $0 (OSS) | ✅ Existe      | 🔴 CRÍTICO |
| 3   | MLflow             | Model Registry  | $0 (OSS) | ⚠️ A instalar  | 🟠 ALTO    |
| 4   | TimescaleDB        | Time-Series DB  | $0 (OSS) | ✅ Existe      | 🔴 CRÍTICO |
| 5   | Redis              | Cache           | $0 (OSS) | ✅ Existe      | 🟠 ALTO    |
| 6   | Elasticsearch      | Search          | $0 (OSS) | ⚠️ A instalar  | 🟡 MEDIO   |
| 7   | TensorFlow Serving | Model Serving   | $0 (OSS) | ⚠️ A instalar  | 🟠 ALTO    |
| 8   | scikit-learn       | ML Library      | $0 (OSS) | ✅ Python      | 🔴 CRÍTICO |
| 9   | XGBoost            | ML Library      | $0 (OSS) | ✅ Python      | 🔴 CRÍTICO |
| 10  | PostgreSQL         | Primary DB      | $0 (OSS) | ✅ Existe      | 🔴 CRÍTICO |
| 11  | RabbitMQ           | Message Queue   | $0 (OSS) | ✅ Existe      | 🟠 ALTO    |
| 12  | Prometheus         | Monitoring      | $0 (OSS) | ✅ Existe      | 🟡 MEDIO   |
| 13  | Grafana            | Visualization   | $0 (OSS) | ✅ Existe      | 🟡 MEDIO   |
| 14  | Apache Spark       | Big Data        | $0 (OSS) | ⚠️ A instalar  | 🟡 MEDIO   |
| 15  | Airflow            | Orchestration   | $0 (OSS) | ⚠️ A instalar  | 🟡 MEDIO   |
| 16  | Feast              | Feature Store   | $0 (OSS) | ⚠️ A instalar  | 🟡 MEDIO   |

---

## 1. 🗄️ Google BigQuery - Data Warehouse

### ¿Por qué?

- Almacenar datos históricos de vehículos, eventos, transacciones
- Ejecutar queries SQL analíticas en petabytes de datos
- Integración con ML (BigQuery ML)
- Serverless (no hay que gestionar infraestructura)

### Costo

- **$6.25 por TB** de datos consultados
- **$50/mes** de almacenamiento (después de 10GB gratis)
- **1TB gratis/mes** para nuevas cuentas (primer año)

**Estimado para OKLA:**

- Almacenamiento: ~500GB → $25/mes
- Queries: ~100TB/mes (después de crecer) → $625/mes
- **Total: ~$650/mes** (cuando hayamos escalado mucho)

### Endpoints a consumir

```csharp
// C# SDK
using Google.Cloud.BigQuery.V2;

// Insertar eventos
var client = BigQueryClient.Create(projectId);
var table = client.GetTable("okla_dataset.events");
table.InsertRow(new { timestamp, userId, eventType, data });

// Queries
var result = client.ExecuteQuery(
    @"SELECT user_id, COUNT(*) as events
      FROM okla_dataset.events
      WHERE date BETWEEN @date1 AND @date2
      GROUP BY user_id"
);
```

### Integración en

- **DataPipelineService** (5051) - Carga datos transformados
- **FeatureStoreService** (5053) - Consulta features históricas
- **MLTrainingService** (5057) - Obtiene datos de entrenamiento

### Documentación requerida

- Setup de proyecto GCP
- IAM roles requeridos
- Esquema de tablas
- Queries útiles
- Cost optimization

---

## 2. 📡 Apache Kafka - Event Streaming

### ¿Por qué?

- Ingesta en **tiempo real** de eventos
- Desacoplamiento entre productores (frontend) y consumidores (servicios)
- Durabilidad: eventos no se pierden si un servicio está down
- Escalabilidad: miles de eventos por segundo

### Costo

- **$0** (open source)
- Hospedaje: En DOKS (Kubernetes)
- Storage: ~500GB/mes de eventos raw

### Arquitectura en DOKS

```yaml
# kafka deployment en Kubernetes
kafka:
  brokers: 3
  replicas: 2
  storage: 500Gi per broker
  partitions per topic: 12
  replication factor: 2
```

### Topics principales

```
# User events (frontend)
user-events
  ├─ page_view
  ├─ search
  ├─ click
  ├─ favorite
  └─ contact_sent

# Dealer events
dealer-events
  ├─ listing_created
  ├─ listing_updated
  ├─ inventory_imported
  └─ message_sent

# ML pipeline events
ml-events
  ├─ model_trained
  ├─ model_deployed
  └─ prediction_logged
```

### SDK a usar

```csharp
// Confluent Kafka .NET SDK
using Confluent.Kafka;

var config = new ProducerConfig
{
    BootstrapServers = "kafka:9092"
};

using (var producer = new ProducerBuilder<string, string>(config).Build())
{
    var result = await producer.ProduceAsync(
        "user-events",
        new Message<string, string>
        {
            Key = userId,
            Value = JsonSerializer.Serialize(eventData)
        }
    );
}
```

### Integración en

- **EventTrackingService** (5050) - Produce eventos
- **DataPipelineService** (5051) - Consume eventos
- Todos los servicios que necesitan pub/sub

### Documentación requerida

- Setup de Kafka en DOKS
- Schema de eventos (Avro)
- Consumer groups
- Monitoreo (lag, throughput)
- Recovery procedures

---

## 3. 🤖 MLflow - Model Registry & Tracking

### ¿Por qué?

- Tracking de parámetros, métricas de modelos
- Versionado de modelos (v1.0, v1.1, v2.0)
- A/B testing (comparar modelos en producción)
- Model registry (producción, staging, archived)

### Costo

- **$0** (open source)
- Hospedaje: En DOKS (Kubernetes)

### Arquitectura

```
MLflow Tracking Server
    │
    ├─ PostgreSQL (backend store)
    ├─ S3 / MinIO (artifact store)
    └─ Web UI (port 5000)
```

### Usar en MLTrainingService

```python
import mlflow
import mlflow.sklearn
from sklearn.ensemble import RandomForestRegressor

# Track training
mlflow.start_run()
mlflow.log_param("n_estimators", 100)
mlflow.log_metric("rmse", 2.5)
mlflow.log_metric("accuracy", 0.92)

model = RandomForestRegressor(n_estimators=100)
model.fit(X_train, y_train)

mlflow.sklearn.log_model(model, "price_predictor")
mlflow.end_run()

# Register model
from mlflow.tracking import MlflowClient
client = MlflowClient()
result = client.create_model_version(
    "PricePredictor",
    "runs:/<run_id>/price_predictor",
    stage="Production"
)
```

### Integración en

- **MLTrainingService** (5057) - Principal
- **RecommendationService** (5054) - Carga modelos
- **LeadScoringService** (5055) - Carga modelos
- **VehicleIntelligenceService** (5056) - Carga modelos

### Documentación requerida

- Setup de MLflow en DOKS
- Workflow de training → registro → deploy
- Versioning strategy
- A/B testing setup
- Monitoring de modelos

---

## 4. ⏱️ TimescaleDB - Time-Series Database

### ¿Por qué?

- Optimizada para datos de series de tiempo
- Compresión automática (~90% menos espacio)
- Queries analíticas rápidas
- Extensión de PostgreSQL (ya lo tenemos)

### Costo

- **$0** (open source)
- Ya existe en DOKS

### Tablas principales

```sql
CREATE TABLE user_events (
    time TIMESTAMPTZ,
    user_id UUID,
    event_type VARCHAR,
    vehicle_id UUID,
    properties JSONB
) PARTITION BY TIME (time INTERVAL '1 day');

CREATE TABLE vehicle_views (
    time TIMESTAMPTZ,
    vehicle_id UUID,
    views INT,
    unique_views INT,
    avg_time_on_page DECIMAL
) PARTITION BY TIME (time INTERVAL '1 day');

CREATE TABLE model_predictions (
    time TIMESTAMPTZ,
    model_name VARCHAR,
    prediction_value DECIMAL,
    actual_value DECIMAL,
    features JSONB
);
```

### Integración en

- **EventTrackingService** (5050) - Almacena eventos
- **ListingAnalyticsService** (5058) - Almacena stats por día
- **DataPipelineService** (5051) - Consulta para agregaciones

### Documentación requerida

- Schema de tablas
- Compression policies
- Retention policies
- Useful queries
- Performance tuning

---

## 5. 🚀 Redis - Cache

### ¿Por qué?

- Cache de features (evita queries costosas)
- Cache de recomendaciones (pre-generadas)
- Sessions de usuario
- Rate limiting
- Leaderboards (top dealers)

### Costo

- **$0** (open source)
- Ya existe en DOKS

### Esquemas de datos

```
# User features (cached por 1 hora)
features:user:{userId} → JSON

# Recommendations (cached por 6 horas)
recommendations:user:{userId} → JSON array

# Listing stats (cached por 30 minutos)
stats:vehicle:{vehicleId} → JSON

# Model cache (actualizado cada vez que se deploy)
model:recommendations:v1.2 → Serialized model
model:pricing:v2.1 → Serialized model

# Rate limiting
rate:user:{userId}:{endpoint} → INT counter
```

### Integración en

- Todos los servicios que necesitan performance

### Documentación requerida

- Key naming convention
- TTL strategy por tipo de dato
- Invalidation strategy
- Memory limits
- Persistence settings

---

## 6. 🔍 Elasticsearch - Search & Analytics

### ¿Por qué?

- Búsqueda full-text rápida en vehículos
- Faceted search (filtros)
- Autocomplete
- Analytics (agregaciones)

### Costo

- **$0** (open source, alternativa: OpenSearch)
- Hospedaje: En DOKS

### Índices principales

```json
{
  "vehicles": {
    "mappings": {
      "properties": {
        "make": { "type": "keyword" },
        "model": { "type": "keyword" },
        "year": { "type": "integer" },
        "price": { "type": "double" },
        "description": { "type": "text", "analyzer": "spanish" },
        "location": { "type": "geo_point" },
        "features": { "type": "keyword" },
        "created_at": { "type": "date" }
      }
    }
  }
}
```

### Integración en

- **DataPipelineService** (5051) - Indexa vehículos
- Búsqueda de vehículos en frontend

### Documentación requerida

- Índice schema
- Analyzer configuration
- Indexing strategy
- Query examples
- Performance tuning

---

## 7. 🧠 TensorFlow Serving - Model Serving

### ¿Por qué?

- Servir modelos pre-entrenados en gRPC (rápido)
- Auto-reloading de nuevas versiones
- Load balancing entre réplicas
- Canary deployment (gradual rollout)

### Costo

- **$0** (open source)
- Hospedaje: En DOKS

### Arquitectura

```
TensorFlow Serving
    │
    ├─ Recommendation model (v1.2)
    ├─ Pricing model (v2.1)
    ├─ Demand model (v1.0)
    └─ Fraud detection model (v1.5)
```

### Integración en

- **RecommendationService** (5054)
- **LeadScoringService** (5055)
- **VehicleIntelligenceService** (5056)
- **MLTrainingService** (5057)

### Documentación requerida

- Model export format
- gRPC client setup
- Version management
- Canary deployment
- Latency monitoring

---

## 8-9. 🤖 scikit-learn & XGBoost - ML Libraries

### ¿Por qué?

- scikit-learn: Modelos clásicos (regresión, clustering, clasificación)
- XGBoost: Modelos de gradient boosting (mejor performance)

### Costo

- **$0** (open source)
- Instalado en Python environment

### Modelos a usar

```python
from sklearn.ensemble import RandomForestRegressor
from xgboost import XGBRegressor
from sklearn.preprocessing import StandardScaler

# Pricing prediction
model = XGBRegressor(n_estimators=100, learning_rate=0.1)
model.fit(X_train, y_train)
predictions = model.predict(X_test)

# Lead scoring
model = XGBClassifier(objective='binary:logistic')
model.fit(X_train, y_train)
scores = model.predict_proba(X_test)  # [cold, warm, hot]
```

### Integración en

- **MLTrainingService** (5057) - Entrenar modelos

### Documentación requerida

- Feature engineering
- Hyperparameter tuning
- Model evaluation
- Feature importance
- Cross-validation strategy

---

## 10. 🐘 PostgreSQL - Primary Database

### ¿Por qué?

- Base de datos principal para todos los servicios
- Transactions ACID
- JSON support para datos complejos
- Full-text search (con extensiones)

### Costo

- **$0** (open source)
- Ya existe en DOKS

### Schemas por servicio

```sql
-- EventTrackingService
CREATE SCHEMA event_tracking;
CREATE TABLE event_tracking.user_events (...)

-- DataPipelineService
CREATE SCHEMA data_pipeline;
CREATE TABLE data_pipeline.pipelines (...)

-- UserBehaviorService
CREATE SCHEMA user_behavior;
CREATE TABLE user_behavior.user_profiles (...)

-- FeatureStoreService
CREATE SCHEMA feature_store;
CREATE TABLE feature_store.user_features (...)

-- Y así para cada servicio...
```

### Integración en

- Todos los servicios

### Documentación requerida

- Schemas por servicio
- Foreign keys y relaciones
- Índices optimizados
- Backup strategy
- Archival policy

---

## 11. 📬 RabbitMQ - Message Queue

### ¿Por qué?

- Comunicación asincrónica entre servicios
- Dead letter queues para manejo de errores
- Retry logic con exponential backoff
- Durabilidad (si un servicio cae, mensajes se guardan)

### Costo

- **$0** (open source)
- Ya existe en DOKS

### Exchanges y queues

```
# Model updates
okla.ml.models.exchange
  ├─ okla.ml.models.trained
  ├─ okla.ml.models.deployed
  └─ okla.ml.models.dlq (dead letter)

# Scoring
okla.scoring.exchange
  ├─ okla.scoring.leads.queue
  └─ okla.scoring.leads.dlq
```

### Integración en

- **MLTrainingService** - Publica modelo entrenado
- **RecommendationService** - Consume modelo nuevo
- **LeadScoringService** - Consume modelo nuevo

### Documentación requerida

- Exchange topology
- Queue configuration
- Dead letter queue setup
- Consumer configuration
- Monitoring

---

## 12-13. 📊 Prometheus & Grafana - Monitoring

### ¿Por qué?

- Monitorear health de servicios
- Alertar si modelo tiene baja accuracy
- Track latencia de predicciones
- Dashboard ejecutivo para admins

### Costo

- **$0** (open source)
- Ya existen en DOKS

### Métricas principales

```prometheus
# Modelos
model_accuracy{model="price_predictor", version="2.1"}
model_latency_ms{model="recommendations", version="1.2"}
model_predictions_total{model="lead_scoring", status="success"}

# Pipeline
pipeline_execution_time_seconds{pipeline="user_features"}
pipeline_rows_processed{pipeline="vehicle_performance"}
pipeline_failures_total{pipeline="data_cleaning"}

# Sistema
service_requests_total{service="recommendation_service"}
service_errors_total{service="lead_scoring_service"}
cache_hit_ratio{service="feature_store"}
```

### Dashboards

```
Main Dashboard:
├─ Service Health (8 servicios + 4 infra)
├─ Model Performance (14 modelos)
├─ Pipeline Status (pipelines en ejecución)
├─ Business Metrics (conversión, engagement)
└─ System Metrics (CPU, memoria, disco)
```

### Integración en

- Todos los servicios

---

## 🚀 Stack de Alternativas Recomendadas

| Caso                | Opción Recomendada   | Alternativas                        |
| ------------------- | -------------------- | ----------------------------------- |
| **Data Warehouse**  | BigQuery             | Snowflake, Redshift, Databricks     |
| **Event Streaming** | Kafka                | Pulsar, AWS Kinesis                 |
| **Model Registry**  | MLflow               | Kubeflow, BentoML, Hugging Face Hub |
| **Feature Store**   | Feast                | Tecton, Databricks Feature Store    |
| **Time-Series DB**  | TimescaleDB          | InfluxDB, Prometheus                |
| **Cache**           | Redis                | Memcached                           |
| **Search**          | Elasticsearch        | OpenSearch, Algolia, Meilisearch    |
| **ML Libraries**    | scikit-learn/XGBoost | LightGBM, CatBoost, PyTorch         |
| **Orchestration**   | Airflow              | Dagster, Prefect, dbt               |

---

## 📝 Resumen de Dependencias

### Instaladas / Existentes ✅

- PostgreSQL
- Redis
- RabbitMQ
- Prometheus + Grafana
- Kubernetes (DOKS)

### A Instalar / Configurar ⚠️

- Kafka (5 días)
- MLflow (3 días)
- TimescaleDB (2 días)
- Elasticsearch (4 días)
- TensorFlow Serving (3 días)
- Spark (2 días)
- Airflow (3 días)

### Como Servicio Externo (SaaS) 💰

- Google BigQuery (costo ~$650/mes cuando crezca)

---

## 🔗 Documentación por Servicio

Cada servicio de Data/ML tendrá su propia sección:

- Setup de dependencias
- Configuration
- Integration examples
- Troubleshooting

Ver [PLAN_DOCUMENTACION_IA.md](PLAN_DOCUMENTACION_IA.md) para estructura completa.

---

**APIs Externas de OKLA v1.0**  
_Enero 15, 2026_
