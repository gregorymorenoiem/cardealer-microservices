# 📊 Matriz Completa de APIs - OKLA IA & ML

**Fecha:** Enero 15, 2026  
**Versión:** 1.0  
**Propósito:** Visión de 360° de todos los APIs que se documentarán

---

## 🗂️ Clasificación de APIs

### Por Tipo

- 🔴 **Eventos** - EventTrackingService
- 🟠 **Datos** - DataPipelineService
- 🟡 **Análisis** - UserBehaviorService, ListingAnalyticsService
- 🟢 **ML/Predicción** - Recommendation, LeadScoring, VehicleIntel
- 🟣 **Entrenamiento** - MLTrainingService
- 🔵 **Features** - FeatureStoreService
- 🟤 **Sociales** - ReviewService

### Por Audiencia

- 👨‍💻 **Backend-to-Backend** - Todos los servicios
- 🌐 **Frontend-facing** - Recommendation, LeadScoring (para dealer), Analytics
- 📊 **Admin** - MLTraining, VehicleIntel
- 👤 **Usuario** - Recommendation, Reviews, Analytics

---

## 📡 EventTrackingService (Puerto 5050)

### Descripción

Captura TODOS los eventos de usuario en tiempo real.

### APIs

#### Event Tracking Endpoints

```
POST  /api/events/track                          - Single event
POST  /api/events/batch                          - Batch events
GET   /api/events/user/{userId}                  - User events
GET   /api/events/vehicle/{vehicleId}            - Vehicle events
GET   /api/events/session/{sessionId}            - Session events
```

#### SDK Frontend

```javascript
// JavaScript SDK
oklaTracker.track("page_view", {
  page: "vehicle-detail",
  vehicleId: "xyz",
  timestamp: Date.now(),
});
```

#### SDK Mobile

```dart
// Dart/Flutter SDK
OklaTracker.track('page_view', {
  'page': 'vehicle-detail',
  'vehicleId': 'xyz'
})
```

### Eventos Capturados

- 20+ tipos de eventos de usuario
- 15+ tipos de eventos de dealer
- Metadata automática (IP, device, location)

### Tecnologías

- Kafka (producer)
- TimescaleDB (storage)
- Redis (deduplication)

---

## 🔄 DataPipelineService (Puerto 5051)

### Descripción

Transforma datos raw en datos limpios, listos para ML.

### APIs

#### Pipeline Management

```
POST  /api/pipelines/run/{pipelineName}          - Ejecutar pipeline
GET   /api/pipelines                             - Listar pipelines
GET   /api/pipelines/{id}/status                 - Status de pipeline
GET   /api/pipelines/{id}/results                - Resultados
GET   /api/pipelines/{id}/logs                   - Logs ejecución
```

#### Data Endpoints

```
GET   /api/data/aggregated-metrics               - Métricas agregadas
GET   /api/data/vehicle-performance              - Performance por vehículo
GET   /api/data/user-interests                   - Intereses de usuario
GET   /api/data/market-trends                    - Tendencias del mercado
```

### Pipelines

- User Interest Profile
- Vehicle Performance Score
- Market Trends
- User Segmentation
- Feature Engineering

### Tecnologías

- Airflow / Dagster (orchestration)
- Spark (processing)
- dbt (transformations)
- PostgreSQL + BigQuery (storage)

---

## 👤 UserBehaviorService (Puerto 5052)

### Descripción

Construye perfiles de comportamiento y segmentos de usuarios.

### APIs

#### User Profile

```
GET   /api/behavior/user/{userId}                - Perfil completo
GET   /api/behavior/user/{userId}/preferences    - Preferencias
GET   /api/behavior/user/{userId}/segments       - Segmentos
GET   /api/behavior/user/{userId}/funnel         - Posición en funnel
GET   /api/behavior/user/{userId}/churn-risk     - Riesgo de churn
```

#### Segmentation

```
POST  /api/behavior/segment/users                - Usuarios por segmento
GET   /api/behavior/segment/{segmentId}          - Detalle de segmento
GET   /api/behavior/similar-users/{userId}       - Usuarios similares
```

#### Trends

```
GET   /api/behavior/trending/makes               - Marcas trending
GET   /api/behavior/trending/searches             - Búsquedas trending
GET   /api/behavior/emerging-segments             - Nuevos segmentos
```

### Perfiles Generados

- 30+ atributos por usuario
- 5+ segmentos automáticos
- Probabilidades de conversión/churn
- Preferencias de vehículos

### Tecnologías

- PostgreSQL (profiles)
- Redis (cache)
- Python (clustering)

---

## 📦 FeatureStoreService (Puerto 5053)

### Descripción

Almacén centralizado de features para ML. Una sola fuente de verdad.

### APIs

#### User Features

```
GET   /api/features/user/{userId}                - Todas las features
GET   /api/features/user/{userId}/historical     - Histórico de features
```

#### Vehicle Features

```
GET   /api/features/vehicle/{vehicleId}          - Todas las features
GET   /api/features/vehicle/batch                - Batch de features
```

#### Dealer Features

```
GET   /api/features/dealer/{dealerId}            - Todas las features
GET   /api/features/dealer/{dealerId}/trending   - Features trending
```

#### Market Features

```
GET   /api/features/market/{date}                - Features de mercado
GET   /api/features/market/categories             - Por categoría
```

#### ML Batch

```
POST  /api/features/batch                        - Batch para training
GET   /api/features/version/{version}            - Features por versión
POST  /api/features/validate                     - Validar features
```

### Features Por Tipo

- **User:** 20+ features (preferencias, comportamiento, RFM)
- **Vehicle:** 25+ features (popularidad, velocidad venta, precio)
- **Dealer:** 15+ features (ratings, performance, responsiveness)
- **Market:** 10+ features (demanda, tendencias, competencia)

### Tecnologías

- PostgreSQL (storage)
- Redis (cache)
- Feast (optional feature store)
- TimescaleDB (historical)

---

## 🎯 RecommendationService (Puerto 5054)

### Descripción

Recomendaciones personalizadas para compradores y dealers.

### APIs

#### Buyer Recommendations

```
GET   /api/recommendations/user/{userId}         - Vehículos para ti
GET   /api/recommendations/vehicle/{vehicleId}/similar   - Similares
GET   /api/recommendations/vehicle/{vehicleId}/buyers    - Compradores potenciales
```

#### Dealer Recommendations

```
GET   /api/recommendations/dealer/{dealerId}/buyers      - Compradores para dealer
GET   /api/recommendations/dealer/{dealerId}/inventory   - Qué inventario comprar
```

#### Explanation

```
POST  /api/recommendations/explain               - Por qué recomendamos esto
GET   /api/recommendations/debug/{id}            - Debug de recomendación
```

### Algoritmos

1. **Collaborative Filtering** - Usuarios similares
2. **Content-Based** - Vehículos similares
3. **Hybrid** - Combinación ponderada
4. **Context-aware** - Ubicación, demanda local

### Output

- 5-10 recomendaciones por usuario
- Score de relevancia (0-1)
- Razones de recomendación
- Cached en Redis (6 horas)

### Tecnologías

- TensorFlow Serving (model serving)
- Redis (caching)
- Python (training)

---

## 📊 LeadScoringService (Puerto 5055)

### Descripción

Scorer de leads para dealers. Hot/Warm/Cold.

### APIs

#### Lead Score

```
GET   /api/leadscoring/lead/{leadId}             - Score de un lead
GET   /api/leadscoring/dealer/{dealerId}/hot     - Leads HOT
GET   /api/leadscoring/dealer/{dealerId}/pipeline - Pipeline by score
GET   /api/leadscoring/vehicle/{vehicleId}/leads - Leads del vehículo
```

#### Recalculation

```
POST  /api/leadscoring/recalculate               - Recalcular todos
POST  /api/leadscoring/recalculate/{dealerId}    - Por dealer
```

#### Insights

```
GET   /api/leadscoring/insights/{dealerId}       - Insights para dealer
GET   /api/leadscoring/actions/{leadId}          - Acciones sugeridas
```

### Scoring Model

- 20+ factors
- Hot (80-100): Contactar inmediatamente
- Warm (50-79): Nurture
- Cold (0-49): Long-term

### Output

- Score 0-100
- Temperatura (Hot/Warm/Cold)
- Urgency level
- Suggested actions
- Predicted close probability

### Tecnologías

- XGBoost (model)
- MLflow (model registry)
- Redis (caching)

---

## 🚗 VehicleIntelligenceService (Puerto 5056)

### Descripción

Pricing, demanda, y detección de anomalías.

### APIs

#### Pricing

```
GET   /api/vehicleintel/price/{vehicleId}       - Análisis de precio
POST  /api/vehicleintel/evaluate                 - Evaluar vehículo
GET   /api/vehicleintel/price-history            - Historial de precios
```

#### Demand

```
GET   /api/vehicleintel/demand/{make}/{model}    - Demanda
GET   /api/vehicleintel/market/trends            - Tendencias
GET   /api/vehicleintel/market/categories         - Por categoría
```

#### Anomalies

```
GET   /api/vehicleintel/anomalies                - Listings anómalos
GET   /api/vehicleintel/anomalies/{vehicleId}    - Anomalía específica
```

#### Insights

```
GET   /api/vehicleintel/dealer/{dealerId}/insights - Para dealer
```

### Modelos

1. **PricePredictor** - Precio óptimo
2. **DaysToSalePredictor** - Cuánto tardará en vender
3. **DemandPredictor** - Demanda futura
4. **AnomalyDetector** - Fraude, errores

### Output

- Precio sugerido con rango
- Predicción de días para venta
- Nivel de demanda
- Anomalías detectadas

### Tecnologías

- XGBoost (pricing)
- Time-series (demand)
- Isolation Forest (anomalies)
- TensorFlow Serving

---

## 🤖 MLTrainingService (Puerto 5057)

### Descripción

Pipeline de entrenamiento de modelos. Scheduled jobs.

### APIs

#### Training

```
POST  /api/mltraining/train/{model}              - Iniciar entrenamiento
GET   /api/mltraining/status/{trainingId}        - Status
GET   /api/mltraining/history/{model}            - Historial
```

#### Models

```
GET   /api/mltraining/models                     - Listar modelos
GET   /api/mltraining/model/{model}/versions     - Versiones
POST  /api/mltraining/deploy/{model}/{version}   - Deploy
POST  /api/mltraining/rollback/{model}           - Rollback
```

#### Metrics

```
GET   /api/mltraining/metrics/{model}            - Métricas globales
GET   /api/mltraining/metrics/{model}/{version}  - Por versión
GET   /api/mltraining/compare/{v1}/{v2}          - Comparar versiones
```

#### A/B Testing

```
POST  /api/mltraining/abtest                     - Setup A/B test
GET   /api/mltraining/abtest/{id}                - Status
POST  /api/mltraining/abtest/{id}/declare-winner - Declara ganador
```

### Modelos

- 14 modelos entrenables
- Scheduling diario/semanal
- Versionado automático
- A/B testing integrado

### Workflow

1. Extraer datos de training
2. Feature engineering
3. Entrenar modelo
4. Evaluar métricas
5. Versionar
6. Deploy gradual (canary)
7. Monitorear

### Tecnologías

- Python (training)
- MLflow (registry)
- Airflow (scheduling)
- scikit-learn + XGBoost (models)

---

## 📊 ListingAnalyticsService (Puerto 5058)

### Descripción

Estadísticas de publicaciones para vendedores y dealers.

### APIs

#### Individual Seller

```
GET   /api/listinganalytics/vehicle/{vehicleId}  - Stats vehículo
GET   /api/listinganalytics/vehicle/{vehicleId}/views - Vistas por día
GET   /api/listinganalytics/vehicle/{vehicleId}/compare - Comparar mercado
GET   /api/listinganalytics/vehicle/{vehicleId}/tips   - Tips mejorar
```

#### Dealer Dashboard

```
GET   /api/listinganalytics/dealer/{dealerId}/dashboard - Dashboard
GET   /api/listinganalytics/dealer/{dealerId}/vehicles  - Todos vehículos
GET   /api/listinganalytics/dealer/{dealerId}/trends    - Tendencias
GET   /api/listinganalytics/dealer/{dealerId}/top       - Top performers
GET   /api/listinganalytics/dealer/{dealerId}/attention - Necesitan atención
```

#### Reports

```
POST  /api/listinganalytics/report/schedule      - Programar reportes
GET   /api/listinganalytics/report/export/{fmt}  - Exportar (PDF, Excel)
```

### Estadísticas

- Views (total, unique, por día)
- Engagement (favoritos, compartidos, tiempo en página)
- Contactos (formularios, chat, llamadas, WhatsApp)
- Conversión (view→contact, contact→test drive)
- Comparación con mercado

### Tecnologías

- TimescaleDB (time-series)
- PostgreSQL (agregaciones)
- Redis (cache)
- Recharts (visualización)

---

## ⭐ ReviewService (Puerto 5059)

### Descripción

Sistema de reviews estilo Amazon. Confianza y reputación.

### APIs

#### Reading Reviews

```
GET   /api/reviews/seller/{sellerId}             - Reviews del seller
GET   /api/reviews/seller/{sellerId}/summary     - Resumen ratings
GET   /api/reviews/vehicle/{vehicleId}           - Reviews del vehículo
```

#### Writing Reviews

```
POST  /api/reviews                               - Crear review
PUT   /api/reviews/{reviewId}                    - Editar review
DELETE /api/reviews/{reviewId}                   - Eliminar review
```

#### Seller Response

```
POST  /api/reviews/{reviewId}/response           - Responder review
PUT   /api/reviews/{reviewId}/response           - Editar respuesta
DELETE /api/reviews/{reviewId}/response          - Eliminar respuesta
```

#### Moderation

```
POST  /api/reviews/{reviewId}/flag               - Reportar
POST  /api/reviews/{reviewId}/helpful            - Marcar útil
GET   /api/reviews/moderation/queue              - Cola de moderación
POST  /api/reviews/moderation/approve            - Aprobar
POST  /api/reviews/moderation/reject             - Rechazar
```

#### Analytics

```
GET   /api/reviews/seller/{sellerId}/badges      - Badges del seller
GET   /api/reviews/seller/{sellerId}/trends      - Tendencias de ratings
```

### Rating System

- 1-5 estrellas
- Ratings detallados (comunicación, exactitud, rapidez, valor)
- Fotos adjuntas
- Verificación de compra
- Respuestas del vendedor
- Badges automáticos

### Tecnologías

- PostgreSQL (reviews)
- NLP (detección spam/toxicidad)
- ML (badging automático)

---

## 🔗 Dependencias Entre APIs

```
EventTrackingService (5050)
        │
        └─→ [Frontend SDK captura eventos]
                    │
                    ▼
DataPipelineService (5051)
        │
        ├─→ UserBehaviorService (5052)
        │
        ├─→ FeatureStoreService (5053)
        │
        └─→ [BigQuery, TimescaleDB]
                    │
                    ├─→ RecommendationService (5054)
                    │
                    ├─→ LeadScoringService (5055)
                    │
                    ├─→ VehicleIntelligenceService (5056)
                    │
                    └─→ MLTrainingService (5057)
                            │
                            └─→ [Nuevos modelos]
                                    ▼
                            [Cargados en memoria]
                                    ▼
                    ┌───────────────┼───────────────┐
                    │               │               │
                    ▼               ▼               ▼
            Recom (5054)    LeadScoring    VehicleIntel
                            (5055)         (5056)

ListingAnalyticsService (5058)
        ← [Consume de EventTracking + DataPipeline]

ReviewService (5059)
        ← [Lee de transacciones y usuarios]
```

---

## 📈 Matriz de Documentación

| Servicio         | README     | Endpoints  | Domain     | Impl        | Frontend    | Tests      | Deploy     |
| ---------------- | ---------- | ---------- | ---------- | ----------- | ----------- | ---------- | ---------- |
| EventTracking    | 600l       | 400l       | 300l       | 1800l       | 1200l       | 900l       | 350l       |
| DataPipeline     | 700l       | 350l       | 400l       | 1900l       | 800l        | 800l       | 350l       |
| UserBehavior     | 650l       | 450l       | 350l       | 1800l       | 1000l       | 850l       | 350l       |
| FeatureStore     | 600l       | 500l       | 350l       | 1700l       | 1100l       | 900l       | 350l       |
| Recommendation   | 800l       | 400l       | 200l       | 2000l       | 1500l       | 1000l      | 350l       |
| LeadScoring      | 750l       | 450l       | 250l       | 1900l       | 1300l       | 950l       | 350l       |
| VehicleIntel     | 800l       | 450l       | 300l       | 2000l       | 1400l       | 1000l      | 350l       |
| MLTraining       | 700l       | 450l       | 300l       | 1800l       | 800l        | 900l       | 350l       |
| ListingAnalytics | 700l       | 500l       | 350l       | 1900l       | 1600l       | 900l       | 350l       |
| ReviewService    | 650l       | 500l       | 400l       | 1800l       | 1300l       | 900l       | 350l       |
| **TOTAL**        | **6,850l** | **4,550l** | **3,200l** | **18,300l** | **11,200l** | **8,900l** | **3,500l** |

**Total documentación + código: ~56,000 líneas**

---

## 🎯 Plan de Documentación

### Fase 1 (Semanas 1-5): Core

- [ ] EventTracking - COMPLETO
- [ ] DataPipeline - COMPLETO
- [ ] UserBehavior - COMPLETO
- [ ] FeatureStore - COMPLETO
- **Total: ~20,000 líneas**

### Fase 2 (Semanas 6-10): Intelligence

- [ ] Recommendation - COMPLETO
- [ ] LeadScoring - COMPLETO
- [ ] VehicleIntel - COMPLETO
- [ ] MLTraining - COMPLETO
- **Total: ~27,000 líneas**

### Fase 3 (Semanas 11-12): Analytics

- [ ] ListingAnalytics - COMPLETO
- [ ] ReviewService - COMPLETO
- [ ] Testing + Polish
- **Total: ~9,000 líneas**

---

## ✅ Checklist Completitud

Por cada API (Servicio):

- [ ] README.md - Visión general
- [ ] ENDPOINTS.md - Especificación REST
- [ ] Domain Models - Entidades + DTOs
- [ ] IMPLEMENTATION.md - C# copy/paste
- [ ] FRONTEND_INTEGRATION.md - React examples
- [ ] TESTING.md - Unit + Integration tests
- [ ] DEPLOYMENT.md - K8s manifests
- [ ] Ejemplo End-to-End - Frontend → Backend
- [ ] Changelog - Qué cambió
- [ ] Troubleshooting - Problemas comunes

---

**Matriz Completa de APIs - OKLA**  
_Enero 15, 2026_  
_10 servicios | 50+ APIs | 56,000 líneas de documentación_
