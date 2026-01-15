# 📋 Plan de Documentación de APIs de IA & Machine Learning - OKLA

**Fecha:** Enero 15, 2026  
**Estado:** Planificación  
**Objetivo:** Documentación completa de todos los servicios de Data & ML  
**Prioridad:** 🔴 CRÍTICO - Base de diferenciación de OKLA

---

## 🎯 RESUMEN EJECUTIVO

OKLA necesita **9 microservicios de Data & ML** para convertirse en el mejor marketplace de vehículos de República Dominicana. Esta documentación cubrirá:

1. **Arquitectura de datos** - Cómo fluyen los datos
2. **APIs internas** - Endpoints entre servicios
3. **APIs para Frontend** - Lo que consume la UI
4. **Modelos de IA** - Qué se entrena y cuándo
5. **Implementación** - Código C# + React
6. **Testing** - Tests unitarios e integración
7. **Deployment** - Kubernetes, CI/CD

---

## 📊 SERVICIOS DE DATA & ML

### 🟡 CRÍTICOS - Iniciar Sprint 1 (6-8 semanas)

| #   | Servicio                       | Puerto | Prioridad  | Descripción                         |
| --- | ------------------------------ | ------ | ---------- | ----------------------------------- |
| 1   | **EventTrackingService**       | 5050   | 🔴 CRÍTICO | Captura eventos en tiempo real      |
| 2   | **DataPipelineService**        | 5051   | 🔴 CRÍTICO | ETL, transformación de datos        |
| 3   | **UserBehaviorService**        | 5052   | 🔴 CRÍTICO | Perfiles y segmentación de usuarios |
| 4   | **FeatureStoreService**        | 5053   | 🔴 CRÍTICO | Almacén centralizado de features    |
| 5   | **RecommendationService**      | 5054   | 🟠 ALTO    | Recomendaciones personalizadas      |
| 6   | **LeadScoringService**         | 5055   | 🟠 ALTO    | Scoring de leads para dealers       |
| 7   | **VehicleIntelligenceService** | 5056   | 🟠 ALTO    | Pricing e IA de vehículos           |
| 8   | **MLTrainingService**          | 5057   | 🟠 ALTO    | Pipeline de entrenamiento           |
| 9   | **ListingAnalyticsService**    | 5058   | 🔴 CRÍTICO | Estadísticas de publicaciones       |
| 10  | **ReviewService**              | 5059   | 🟠 ALTO    | Reviews estilo Amazon               |

---

## 📁 ESTRUCTURA DE CARPETAS

```
docs/api/ia-ml/
├── PLAN_DOCUMENTACION_IA.md                 ← Este archivo
├── ARQUITECTURA_GENERAL.md                  ← Visión general
│
├── 1-event-tracking/
│   ├── README.md                            ← Overview
│   ├── ENDPOINTS.md                         ← Especificación de API
│   ├── DOMAIN_MODELS.md                     ← Entidades y enums
│   ├── IMPLEMENTATION.md                    ← C# completo
│   ├── FRONTEND_INTEGRATION.md              ← React + TypeScript
│   ├── TESTING.md                           ← Tests unitarios
│   └── DEPLOYMENT.md                        ← Kubernetes
│
├── 2-data-pipeline/
│   ├── README.md
│   ├── ENDPOINTS.md
│   ├── PIPELINES.md                         ← Tipos de pipelines
│   ├── IMPLEMENTATION.md
│   ├── FRONTEND_INTEGRATION.md
│   ├── TESTING.md
│   └── DEPLOYMENT.md
│
├── 3-user-behavior/
│   ├── README.md
│   ├── ENDPOINTS.md
│   ├── PROFILES.md                          ← Perfiles y segmentación
│   ├── IMPLEMENTATION.md
│   ├── FRONTEND_INTEGRATION.md
│   ├── TESTING.md
│   └── DEPLOYMENT.md
│
├── 4-feature-store/
│   ├── README.md
│   ├── ENDPOINTS.md
│   ├── FEATURES.md                          ← Catálogo de features
│   ├── IMPLEMENTATION.md
│   ├── FRONTEND_INTEGRATION.md
│   ├── TESTING.md
│   └── DEPLOYMENT.md
│
├── 5-recommendation/
│   ├── README.md
│   ├── ENDPOINTS.md
│   ├── ALGORITHMS.md                        ← Collaborative, Content-Based, Hybrid
│   ├── IMPLEMENTATION.md
│   ├── FRONTEND_INTEGRATION.md              ← Mostrar recomendaciones
│   ├── TESTING.md
│   └── DEPLOYMENT.md
│
├── 6-lead-scoring/
│   ├── README.md
│   ├── ENDPOINTS.md
│   ├── SCORING_MODEL.md                     ← Cómo funciona el scoring
│   ├── IMPLEMENTATION.md
│   ├── FRONTEND_INTEGRATION.md              ← Dashboard de dealers
│   ├── TESTING.md
│   └── DEPLOYMENT.md
│
├── 7-vehicle-intelligence/
│   ├── README.md
│   ├── ENDPOINTS.md
│   ├── PRICING_MODEL.md                     ← Análisis de precio
│   ├── DEMAND_PREDICTION.md                 ← Predicción de demanda
│   ├── ANOMALY_DETECTION.md                 ← Detección de fraude
│   ├── IMPLEMENTATION.md
│   ├── FRONTEND_INTEGRATION.md
│   ├── TESTING.md
│   └── DEPLOYMENT.md
│
├── 8-ml-training/
│   ├── README.md
│   ├── ENDPOINTS.md
│   ├── MODELS.md                            ← 14 modelos a entrenar
│   ├── TRAINING_PIPELINE.md                 ← Cómo se entrena
│   ├── IMPLEMENTATION.md
│   ├── TESTING.md
│   └── DEPLOYMENT.md
│
├── 9-listing-analytics/
│   ├── README.md
│   ├── ENDPOINTS.md
│   ├── DASHBOARD_VIEWS.md                   ← Vistas del dashboard
│   ├── IMPLEMENTATION.md
│   ├── FRONTEND_INTEGRATION.md              ← React components
│   ├── TESTING.md
│   └── DEPLOYMENT.md
│
├── 10-review-service/
│   ├── README.md
│   ├── ENDPOINTS.md
│   ├── REVIEW_SYSTEM.md                     ← Sistema de reviews
│   ├── IMPLEMENTATION.md
│   ├── FRONTEND_INTEGRATION.md              ← UI de reviews
│   ├── TESTING.md
│   └── DEPLOYMENT.md
│
└── INTEGRACIONES_EXTERNAS.md                ← APIs de terceros a consumir
```

---

## 🔌 APIs EXTERNAS A CONSUMIR

### 1. **Google BigQuery** - Data Warehouse

- **Para:** Almacenar datos históricos de vehículos, eventos, transacciones
- **Costo:** ~$6.25/TB consultado (después de 1TB gratis/mes)
- **En documento:** Data Storage en DataPipelineService
- **Integración en:** DataPipelineService (5051)
- **Documentación requerida:** INTEGRACIONES_EXTERNAS.md

### 2. **Apache Kafka** - Event Streaming

- **Para:** Ingesta en tiempo real de eventos
- **Auto-hospedado:** Recomendado en DOKS (Kubernetes)
- **Costo:** $0 (open source)
- **En documento:** Tecnologías de EventTrackingService
- **Integración en:** EventTrackingService (5050) y DataPipelineService (5051)

### 3. **MLflow** - Model Registry

- **Para:** Versionado, tracking y despliegue de modelos
- **Auto-hospedado:** Recomendado en DOKS
- **Costo:** $0 (open source)
- **En documento:** MLTrainingService (5057)
- **Integración en:** MLTrainingService

### 4. **TensorFlow Serving** / **MLflow Models**

- **Para:** Servir modelos entrenados en producción
- **Auto-hospedado:** En DOKS
- **Costo:** $0 (open source)
- **Integración en:** Todos los servicios que hacen predicciones

### 5. **Scikit-learn / XGBoost** - ML Libraries (Python)

- **Para:** Entrenar modelos de scoring, pricing, demanda
- **Costo:** $0 (open source)
- **En documento:** MLTrainingService (5057)
- **Tecnología:** Python con .NET Integration (IronPython o REST)

### 6. **PostgreSQL TimescaleDB** - Time-Series DB

- **Para:** Almacenar eventos y métricas de tiempo real
- **Auto-hospedado:** Ya existe en DOKS
- **Costo:** $0 (open source)
- **Integración en:** EventTrackingService, ListingAnalyticsService

### 7. **Redis** - Cache & Sessions

- **Para:** Cache de features, modelos, dashboards en tiempo real
- **Auto-hospedado:** Ya existe en DOKS
- **Costo:** $0 (open source)
- **Integración en:** Todos los servicios

### 8. **Elasticsearch** - Búsqueda & Analytics

- **Para:** Búsqueda de vehículos con filtros, faceted search
- **Auto-hospedado:** Recomendado en DOKS
- **Costo:** $0 (open source, con alternativa $0 opensearch)
- **Integración en:** DataPipelineService para indexación

### 9. **RabbitMQ** - Message Queue

- **Para:** Comunicación entre servicios, eventos asincronos
- **Auto-hospedado:** Ya existe en DOKS
- **Costo:** $0 (open source)
- **Integración en:** Todos los servicios

### 10. **Prometheus + Grafana** - Monitoring

- **Para:** Monitorear modelos, pipelines, performance
- **Auto-hospedado:** Ya existe en DOKS
- **Costo:** $0 (open source)
- **Integración en:** Todos los servicios

---

## 📚 CONTENIDO POR DOCUMENTO

### README.md (Cada servicio)

**Estructura estándar:**

```markdown
# [N]. [Nombre] Service (Puerto XXXX)

## ¿Por qué es necesario?

- 3 razones principales

## De un vistazo

- Features principales
- Endpoints clave
- Tecnologías

## Flujo de datos

- Diagrama ASCII de cómo fluyen los datos

## Tabla de contenidos

- Link a ENDPOINTS.md
- Link a IMPLEMENTATION.md
- Link a FRONTEND_INTEGRATION.md
- etc.
```

### ENDPOINTS.md (Cada servicio)

**Estructura estándar:**

```markdown
# Endpoints de [Servicio]

## GET /api/[service]/...

- Descripción
- Parámetros
- Ejemplo de request/response

## POST /api/[service]/...

- Descripción
- Body
- Respuestas posibles

[Completo para todos los endpoints]
```

### IMPLEMENTATION.md (Cada servicio)

**Estructura estándar:**

```markdown
# Implementación en C# 8 - [Servicio]

## 1. Domain Layer

- Entities
- Value Objects
- Interfaces

## 2. Application Layer

- Commands/Queries (CQRS con MediatR)
- DTOs
- Validators (FluentValidation)

## 3. Infrastructure Layer

- DbContext
- Repositories
- Services (integración con APIs externas)

## 4. API Layer

- Controllers
- Program.cs (DI, CORS, Swagger)

## Código Completo

- [Copia/paste ready]
```

### FRONTEND_INTEGRATION.md (Cada servicio)

**Estructura estándar:**

```markdown
# Integración Frontend - [Servicio]

## React Components

- [Componente 1]
- [Componente 2]
- etc.

## React Query / TanStack Query

- Hooks custom para consumir API

## TypeScript Types

- DTOs mapeados a TypeScript

## Ejemplos de Uso

- Cómo usar en páginas

## Testing

- Tests de componentes
```

### TESTING.md (Cada servicio)

**Estructura estándar:**

```markdown
# Testing - [Servicio]

## Unit Tests

- [5-10 tests por servicio]
- xUnit + FluentAssertions

## Integration Tests

- Tests con base de datos

## E2E Tests

- Workflow completo

## Código Completo

- Tests listos para copiar/pegar
```

---

## 🚀 PLAN DE IMPLEMENTACIÓN (12 semanas)

### Fase 1: Infrastructure (Semanas 1-2)

- [ ] Configurar Kafka en DOKS
- [ ] Configurar TimescaleDB
- [ ] Configurar MLflow
- [ ] Setup de todos los servicios base

### Fase 2: Core Services (Semanas 3-5)

- [ ] EventTrackingService (5050)
- [ ] DataPipelineService (5051)
- [ ] UserBehaviorService (5052)
- [ ] FeatureStoreService (5053)

**Documentación esperada:** 4 READMEs completos (~8,000 líneas)

### Fase 3: Smart Services (Semanas 6-8)

- [ ] RecommendationService (5054)
- [ ] LeadScoringService (5055)
- [ ] VehicleIntelligenceService (5056)
- [ ] MLTrainingService (5057)

**Documentación esperada:** 4 READMEs completos (~10,000 líneas)

### Fase 4: Analytics & Reviews (Semanas 9-10)

- [ ] ListingAnalyticsService (5058)
- [ ] ReviewService (5059)

**Documentación esperada:** 2 READMEs completos (~6,000 líneas)

### Fase 5: Integración Frontend (Semanas 11-12)

- [ ] Dashboards para dealers
- [ ] Recomendaciones en homepage
- [ ] Reviews en listing detail
- [ ] Analytics en seller panel

**Documentación esperada:** Frontend integration guides (~4,000 líneas)

---

## 📊 MÉTRICAS DE DOCUMENTACIÓN

### Por Servicio (Esperado)

- **README.md:** 500-800 líneas
- **ENDPOINTS.md:** 300-500 líneas
- **IMPLEMENTATION.md:** 1,500-2,000 líneas de código C#
- **FRONTEND_INTEGRATION.md:** 1,000-1,500 líneas de código React
- **TESTING.md:** 800-1,200 líneas de código de tests
- **DEPLOYMENT.md:** 300-400 líneas

**Total por servicio:** ~5,000-7,000 líneas

**Total proyecto (10 servicios):** ~50,000-70,000 líneas de documentación

### Código Implementado

- **Backend:** ~15,000 líneas de código C# (.NET 8)
- **Frontend:** ~8,000 líneas de código React/TypeScript
- **Tests:** ~4,000 líneas de tests
- **Total:** ~27,000 líneas de código

---

## 🎓 CONTENIDO ESPECIAL POR SERVICIO

### EventTrackingService

- SDK para JavaScript (para capturar eventos del frontend)
- SDK para Dart (para capturar eventos de mobile)

### DataPipelineService

- Configuración de Airflow/Dagster
- Ejemplos de transformaciones con dbt

### UserBehaviorService

- Algoritmo de segmentación automática
- Cálculo de propensión a churn

### RecommendationService

- Algoritmo colaborativo (k-NN)
- Algoritmo content-based (similaridad de features)
- Algoritmo híbrido (combinación ponderada)

### LeadScoringService

- Modelo de scoring detallado (con pesos)
- Simulador de scoring (jugar con parámetros)

### VehicleIntelligenceService

- Modelo de pricing (regresión XGBoost)
- Modelo de demanda (time series)
- Detección de anomalías (Isolation Forest)

### MLTrainingService

- Pipeline de entrenamiento con MLflow
- Configuración de A/B testing de modelos

### ListingAnalyticsService

- Generación de reportes PDF
- Integración con Looker/Power BI

### ReviewService

- Sistema de moderación automática (spam/toxicidad)
- Cálculo de badges

---

## ✅ CHECKLIST DE ENTREGA

Por cada servicio:

- [ ] README.md - Overview completo
- [ ] ENDPOINTS.md - Especificación API REST
- [ ] Domain Models - Entidades, Enums, Interfaces
- [ ] Implementation.md - Código C# copy/paste ready
- [ ] Frontend Integration - React Components + Hooks
- [ ] Testing.md - Tests unitarios e integración
- [ ] Deployment.md - Kubernetes manifests
- [ ] Ejemplo de uso end-to-end (Frontend → API → Backend)

---

## 🔗 INTEGRACIONES ENTRE SERVICIOS

```
EventTrackingService (5050)
        │
        ▼
DataPipelineService (5051)
        │
        ├─→ UserBehaviorService (5052)
        │
        ├─→ FeatureStoreService (5053)
        │
        └─→ (almacena en BigQuery)
                    │
                    ├─→ RecommendationService (5054)
                    │
                    ├─→ LeadScoringService (5055)
                    │
                    ├─→ VehicleIntelligenceService (5056)
                    │
                    └─→ MLTrainingService (5057)
                                │
                                └─→ (nuevas versiones de modelos)
                                        │
                                        ├─→ RecommendationService
                                        ├─→ LeadScoringService
                                        └─→ VehicleIntelligenceService

ListingAnalyticsService (5058)
        ← (consume de EventTrackingService y DataPipelineService)

ReviewService (5059)
        ← (datos de transacciones y eventos de usuario)
```

---

## 📈 PRÓXIMOS PASOS

1. **Aprobar plan** ✅
2. **Crear estructura de carpetas** ✅
3. **Iniciar documentación por servicio** (semana 1)
4. **Comenzar implementación de Sprint 1** (semana 3)
5. **Publicar en GitHub Docs** (mes 2)

---

_Documento: Plan de Documentación de IA & ML_  
_Versión: 1.0_  
_Fecha: Enero 15, 2026_  
_Estado: Aprobado para implementación_
