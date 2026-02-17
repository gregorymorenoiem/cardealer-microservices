# 🏗️ Arquitectura General de Data & ML - OKLA

**Fecha:** Enero 15, 2026  
**Versión:** 1.0  
**Objetivo:** Entender cómo funcionan todos los servicios de Data & ML juntos

---

## 📊 Visión General del Sistema

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              FRONTEND (Web/Mobile)                           │
│                                                                              │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │  Usuario interaccionando:                                            │   │
│  │  • Viendo vehículos (page views)                                     │   │
│  │  • Buscando (search queries)                                         │   │
│  │  • Guardando favoritos (favorites)                                   │   │
│  │  • Contactando vendedor (leads)                                      │   │
│  │  • Escribiendo reviews                                               │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────┬────────────────────────────────────────┘
                                     │ (eventos, acciones, datos)
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                    🔴 EVENT TRACKING SERVICE (5050)                          │
│                                                                              │
│  Captura TODOS los eventos:                                                 │
│  • Page views: usuario vio vehículo X                                        │
│  • Clicks: usuario clickeó botón Y                                           │
│  • Searches: usuario buscó [filtros]                                         │
│  • Engagement: tiempo en página, scroll depth                                │
│  • Conversions: contacto enviado, test drive solicitado                      │
│                                                                              │
│  Tecnologías: Kafka, TimescaleDB, JSON events                              │
└────────────────────────────────────┬────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                    🔴 DATA PIPELINE SERVICE (5051)                           │
│                                                                              │
│  Procesa datos raw → datos limpios:                                          │
│  • Extracción (Extract): Lee eventos de Kafka                                │
│  • Transformación (Transform): Limpia, normaliza, agrega datos              │
│  • Carga (Load): Almacena en PostgreSQL, BigQuery, Data Warehouse           │
│                                                                              │
│  Ejecuta pipelines:                                                          │
│  • Vehicle Performance Score: ¿Qué tan bien se está vendiendo?             │
│  • User Interest Profile: ¿Qué tipo de vehículos le interesan?             │
│  • Market Trends: ¿Cuáles son las tendencias del mercado?                  │
│                                                                              │
│  Tecnologías: Airflow/Dagster, Spark, dbt, PostgreSQL                      │
└────────────────────────────────────┬────────────────────────────────────────┘
                                     │
                    ┌────────────────┼────────────────┐
                    │                │                │
                    ▼                ▼                ▼
        ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
        │ USER BEHAVIOR    │  │ FEATURE STORE    │  │ LISTING ANALYTICS│
        │ (5052)           │  │ (5053)           │  │ (5058)           │
        └──────────────────┘  └──────────────────┘  └──────────────────┘
                    │                │                │
                    ▼                ▼                ▼
        Profiles:          Features:           Stats por publicación:
        • Preferencias     • User features     • Vistas por día
        • Comportamiento   • Vehicle features  • Contactos
        • Segmentos        • Dealer features   • Conversión
        • Churn risk       • Market features   • Comparación mercado
                                               • Tips para mejorar
                    │                │
                    ▼                ▼
        ┌──────────────────────────────────────┐
        │    ML TRAINING SERVICE (5057)         │
        │                                      │
        │  Entrena modelos cada semana:        │
        │  • RecommendationModel               │
        │  • LeadScoringModel                  │
        │  • PricingModel                      │
        │  • DemandPredictionModel             │
        │  • etc. (14 modelos total)           │
        │                                      │
        │  Versionado + A/B testing            │
        └──────────┬───────────┬───────────────┘
                   │           │
        ┌──────────▼┐  ┌──────┴─────────────────┐
        │ Deploy v1 │  │ Test v2 on 10% traffic │
        └──────────┬┘  └──────┬─────────────────┘
                   │           │
                   ▼           ▼
        ┌──────────────────────────────────┐
        │   INFERENCE SERVICES             │
        │                                  │
        │ • RecommendationService (5054)   │ → Vehículos para ti
        │ • LeadScoringService (5055)      │ → Hot/Warm/Cold leads
        │ • VehicleIntelligence (5056)     │ → Pricing, demanda, anomalías
        │                                  │
        │ Modelos cargados en memoria      │
        │ (rápido: <100ms latencia)        │
        └────────────┬─────────────────────┘
                     │
                     ▼
        ┌──────────────────────────────────┐
        │    FRONTEND (Mostrando AL)       │
        │                                  │
        │ • "Vehículos para ti" (5054)     │
        │ • "Compradores interesados"      │
        │ • "Precio sugerido" (5056)       │
        │ • "Score del lead" (5055)        │
        │ • "Stats de tu publicación"      │
        │ • "Reviews de vendedor"          │
        └──────────────────────────────────┘
```

---

## 🔄 Flujo de Datos Completo

### Ejemplo: Usuario busca vehículos

```
1. Usuario llega a OKLA.com.do
   ↓
2. Frontend dispara evento: "page_view" → EventTrackingService
   ├─ userId: "abc123"
   ├─ page: "homepage"
   ├─ timestamp: "2026-01-15T10:30:00Z"
   └─ properties: {...}
   ↓
3. EventTrackingService recibe evento
   ├─ Valida estructura
   ├─ Enriquece con contexto (IP, device, etc.)
   └─ Envía a Kafka topic "user.events"
   ↓
4. DataPipelineService consume eventos de Kafka
   ├─ Corre cada 5 minutos (scheduled job)
   ├─ Agrega eventos: "150 views en última hora"
   ├─ Normaliza datos
   └─ Inserta en PostgreSQL + BigQuery
   ↓
5. UserBehaviorService procesa datos
   ├─ Actualiza perfil del usuario
   ├─ Recalcula preferencias: "Prefiere SUVs, Toyota, $20k-30k"
   ├─ Actualiza segmento: "Active Buyer"
   └─ Guarda en FeatureStoreService
   ↓
6. ListingAnalyticsService genera stats
   ├─ "Este vehículo tuvo 45 views hoy"
   ├─ "Conversión view→contact: 12%"
   ├─ "Top performer en su categoría"
   └─ Guarda en Redis para dashboard rápido
   ↓
7. RecommendationService usa datos para generar recs
   ├─ Lee UserBehaviorService: "Prefiere SUVs"
   ├─ Lee FeatureStoreService: features de SUVs populares
   ├─ Corre algoritmo: Collaborative + Content-based
   ├─ Genera: "Te recomendamos estos 5 SUVs"
   └─ Caché en Redis: 1 hora
   ↓
8. Frontend pide recomendaciones
   GET /api/recommendations/user/{userId}
   ↓
9. RecommendationService retorna:
   [
     {
       "vehicleId": "xyz",
       "make": "Toyota",
       "model": "RAV4",
       "score": 0.92,
       "reasons": ["Matches your preferences", "Popular in your area"]
     },
     ...
   ]
   ↓
10. Frontend muestra "Vehículos para ti" con estos resultados
```

---

## 🤖 14 Modelos de ML a Entrenar

### Recomendaciones (3)

1. **VehicleRecommender** - Vehículos para usuarios
2. **BuyerRecommender** - Compradores para dealers
3. **SimilarVehicles** - Vehículos similares

### Scoring (3)

4. **LeadScorer** - Hot/Warm/Cold leads
5. **ChurnPredictor** - Predecir abandono
6. **ConversionPredictor** - Predecir conversión

### Pricing (2)

7. **PricePredictor** - Precio óptimo
8. **DaysToSalePredictor** - Días para venta

### Clasificación (3)

9. **UserSegmenter** - Clasificar usuarios
10. **VehicleClassifier** - Clasificar vehículos
11. **FraudDetector** - Detectar anomalías

### NLP (2)

12. **DescriptionAnalyzer** - Analizar descripciones
13. **SentimentAnalyzer** - Sentimiento de reviews
14. **SearchIntentClassifier** - Intención de búsqueda

---

## 📈 Stack Tecnológico Recomendado

### Backend (.NET 8)

- Framework: ASP.NET Core
- ORM: Entity Framework Core
- CQRS: MediatR
- Validación: FluentValidation
- Logging: Serilog
- Cache: StackExchange.Redis

### Data Processing (Python + .NET)

- Data Pipeline: Apache Airflow / Dagster
- Transformación: Apache Spark / dbt
- ETL: Python scripts
- Feature Store: Feast (open source)

### Machine Learning (Python)

- Frameworks: scikit-learn, XGBoost, TensorFlow
- Serving: MLflow, TensorFlow Serving
- Notebook: Jupyter
- Version Control: MLflow Tracking

### Infrastructure (Kubernetes/DOKS)

- Container Registry: ghcr.io
- Database: PostgreSQL + TimescaleDB
- Cache: Redis
- Message Queue: RabbitMQ
- Stream Processing: Kafka
- Monitoring: Prometheus + Grafana

### Frontend (React 19)

- Framework: React + TypeScript
- Data Fetching: TanStack Query (React Query)
- Charts: Recharts / Chart.js
- State: Zustand / Context API
- CSS: Tailwind CSS

---

## 🚀 Timeline de Implementación

### Sprint 1 (Semanas 1-3): Infraestructura + Core

- [ ] Setup Kafka en DOKS
- [ ] Setup TimescaleDB
- [ ] Crear EventTrackingService (5050)
- [ ] Crear DataPipelineService (5051)
- [ ] Documentación: 1-2 servicios

### Sprint 2 (Semanas 4-6): Behavior + Features

- [ ] Crear UserBehaviorService (5052)
- [ ] Crear FeatureStoreService (5053)
- [ ] Setup MLflow
- [ ] Documentación: 2 servicios

### Sprint 3 (Semanas 7-9): Smart Services

- [ ] Crear RecommendationService (5054)
- [ ] Crear LeadScoringService (5055)
- [ ] Entrenar primeros modelos
- [ ] Documentación: 2 servicios

### Sprint 4 (Semanas 10-12): Intelligence + Analytics

- [ ] Crear VehicleIntelligenceService (5056)
- [ ] Crear MLTrainingService (5057)
- [ ] Crear ListingAnalyticsService (5058)
- [ ] Crear ReviewService (5059)
- [ ] Frontend integration
- [ ] Documentación: 4 servicios

---

## 💼 Casos de Uso Principales

### Para Compradores

1. **Buscar + Filtrar** - Mejora con ML: Búsqueda semántica
2. **Ver Recomendaciones** - "Vehículos para ti" personalizados
3. **Comparar** - Comparación con "vehículos similares"
4. **Leer Reviews** - Confiar en opiniones de otros

### Para Dealers

1. **Gestionar Inventario** - Con predicción de demanda
2. **Pricing Inteligente** - Precio sugerido con IA
3. **Lead Prioritization** - Leads HOT vs fríos
4. **Ver Estadísticas** - Dashboard con trending

### Para OKLA (Admin)

1. **Detección de Fraude** - Anomalías en listings
2. **Moderation** - Spam/reviews tóxicas
3. **Platform Metrics** - KPIs de performance

---

## 🎯 Beneficios Esperados

| Métrica                 | Baseline | Target (6 meses) | Target (12 meses) |
| ----------------------- | -------- | ---------------- | ----------------- |
| **Engagement**          |          |                  |                   |
| Avg. time on site       | 4 min    | 7 min (+75%)     | 10 min (+150%)    |
| Pages/session           | 3.2      | 4.5              | 6                 |
| Repeat visitors         | 35%      | 50%              | 65%               |
| **Conversión**          |          |                  |                   |
| View → Contact          | 8%       | 12% (+50%)       | 15%               |
| Contact → Test Drive    | 25%      | 35%              | 45%               |
| Test Drive → Sale       | 40%      | 48%              | 55%               |
| **Dealer Satisfaction** |          |                  |                   |
| NPS                     | 45       | 65               | 75                |
| Renewal rate            | 70%      | 85%              | 92%               |
| Upgrade rate            | 15%      | 25%              | 35%               |
| **Platform**            |          |                  |                   |
| Revenue (MRR)           | $50k     | $120k            | $250k             |
| Active dealers          | 150      | 300              | 500               |
| Vehicles listed         | 3,500    | 8,000            | 15,000            |

---

## 🔗 Referencias

- [DATA_ML_MICROSERVICES_STRATEGY.md](../../DATA_ML_MICROSERVICES_STRATEGY.md) - Documento original detallado
- [PLAN_DOCUMENTACION_IA.md](PLAN_DOCUMENTACION_IA.md) - Plan de documentación
- [Servicios de IA/ML](#) - Links a cada servicio

---

**Arquitectura de Data & ML de OKLA v1.0**  
_Enero 15, 2026_
