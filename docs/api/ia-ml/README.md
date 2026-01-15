# 🤖 Documentación de IA & Machine Learning - OKLA

**Fecha:** Enero 15, 2026  
**Versión:** 1.0  
**Estado:** 📋 Planificación (Implementación inicia semana 1)

---

## 🎯 ¿Qué es esta carpeta?

Documentación completa de los **9 microservicios de Data & Machine Learning** que hacen a OKLA diferente: recomendaciones personalizadas, lead scoring inteligente, pricing dinámico, y análisis predictivos.

**Objetivo:** Convertir a OKLA en el mejor marketplace de vehículos de República Dominicana usando IA.

---

## 📂 Estructura de Carpetas

```
docs/api/ia-ml/
│
├── 📘 README.md (este archivo)
│   └─ Índice general y quick start
│
├── 📘 PLAN_DOCUMENTACION_IA.md
│   └─ Plan detallado de documentación
│   └─ Timeline de 12 semanas
│   └─ Estructura esperada por servicio
│
├── 📘 ARQUITECTURA_GENERAL.md
│   └─ Visión general del sistema
│   └─ Flujo de datos
│   └─ Stack tecnológico
│   └─ 14 modelos a entrenar
│
├── 📘 INTEGRACIONES_EXTERNAS.md
│   └─ APIs externas a consumir
│   └─ Kafka, BigQuery, MLflow, etc.
│   └─ Alternativas recomendadas
│
├── 📁 1-event-tracking/
│   └─ EventTrackingService (Puerto 5050)
│   └─ Captura eventos en tiempo real
│
├── 📁 2-data-pipeline/
│   └─ DataPipelineService (Puerto 5051)
│   └─ ETL, transformación, agregación
│
├── 📁 3-user-behavior/
│   └─ UserBehaviorService (Puerto 5052)
│   └─ Perfiles y segmentación
│
├── 📁 4-feature-store/
│   └─ FeatureStoreService (Puerto 5053)
│   └─ Almacén centralizado de features
│
├── 📁 5-recommendation/
│   └─ RecommendationService (Puerto 5054)
│   └─ Vehículos para ti, similar vehicles
│
├── 📁 6-lead-scoring/
│   └─ LeadScoringService (Puerto 5055)
│   └─ Hot/Warm/Cold leads
│
├── 📁 7-vehicle-intelligence/
│   └─ VehicleIntelligenceService (Puerto 5056)
│   └─ Pricing, demanda, anomalías
│
├── 📁 8-ml-training/
│   └─ MLTrainingService (Puerto 5057)
│   └─ Entrenamiento de modelos
│
├── 📁 9-listing-analytics/
│   └─ ListingAnalyticsService (Puerto 5058)
│   └─ Estadísticas de publicaciones
│
└── 📁 10-review-service/
    └─ ReviewService (Puerto 5059)
    └─ Reviews estilo Amazon
```

---

## 🚀 Quick Start

### 1. Entender la Visión General (5 minutos)

Lee [ARQUITECTURA_GENERAL.md](ARQUITECTURA_GENERAL.md) para entender:

- Cómo fluyen los datos entre servicios
- Los 14 modelos que se van a entrenar
- El timeline de implementación

### 2. Ver el Plan de Documentación (10 minutos)

Lee [PLAN_DOCUMENTACION_IA.md](PLAN_DOCUMENTACION_IA.md) para saber:

- Qué se va a documentar en cada servicio
- Estructura esperada de archivos
- Métricas de documentación

### 3. Entender las Dependencias Externas (15 minutos)

Lee [INTEGRACIONES_EXTERNAS.md](INTEGRACIONES_EXTERNAS.md) para saber:

- Qué APIs externas consumirá el sistema
- Cuáles están instaladas, cuáles a instalar
- Costo de cada dependencia

### 4. Iniciar un Servicio Específico

Ve a la carpeta del servicio que quieras explorar (ej: `1-event-tracking/`) y lee su README.md

---

## 📊 Servicios Principales

### 🔴 Críticos (Sprint 1-2)

| Servicio                    | Puerto | Descripción                    |
| --------------------------- | ------ | ------------------------------ |
| **EventTrackingService**    | 5050   | Captura eventos en tiempo real |
| **DataPipelineService**     | 5051   | ETL, transformación de datos   |
| **UserBehaviorService**     | 5052   | Perfiles y segmentación        |
| **FeatureStoreService**     | 5053   | Almacén de features para ML    |
| **ListingAnalyticsService** | 5058   | Estadísticas de publicaciones  |

### 🟠 Altos (Sprint 3-4)

| Servicio                       | Puerto | Descripción                    |
| ------------------------------ | ------ | ------------------------------ |
| **RecommendationService**      | 5054   | Recomendaciones personalizadas |
| **LeadScoringService**         | 5055   | Scoring Hot/Warm/Cold          |
| **VehicleIntelligenceService** | 5056   | Pricing, demanda, anomalías    |
| **MLTrainingService**          | 5057   | Entrenamiento de modelos       |
| **ReviewService**              | 5059   | Reviews estilo Amazon          |

---

## 🎯 Casos de Uso Principales

### Para Compradores

```
"Vehículos para ti"
  ↓ (RecommendationService)
  ↓
"Te recomendamos estos 5 vehículos basados en tu historial"
```

### Para Dealers

```
Lead prioritization
  ↓ (LeadScoringService)
  ↓
"🔥 HOT Lead - J.P. desde Santo Domingo
 Vio tu Toyota 7 veces en 3 días - Contacta AHORA"
```

### Para Vendedores

```
Estadísticas del vehículo
  ↓ (ListingAnalyticsService)
  ↓
"Tu Honda tuvo 156 vistas este mes
 Performance: Top 20% en su categoría"
```

---

## 📈 Beneficios Esperados

| Métrica                  | Hoy   | En 6 meses | En 12 meses |
| ------------------------ | ----- | ---------- | ----------- |
| Tiempo promedio en sitio | 4 min | 7 min      | 10 min      |
| Conversión view→contact  | 8%    | 12%        | 15%         |
| Dealer NPS               | 45    | 65         | 75          |
| MRR                      | $50k  | $120k      | $250k       |

---

## 🔧 Tecnologías Principales

### Backend

- .NET 8 LTS
- Clean Architecture
- CQRS + MediatR
- Entity Framework Core

### Machine Learning

- Python (scikit-learn, XGBoost)
- MLflow (model registry)
- Jupyter Notebooks

### Data Processing

- Apache Kafka (event streaming)
- Apache Spark (big data)
- Airflow (orchestration)
- dbt (transformations)

### Infrastructure

- PostgreSQL + TimescaleDB
- Redis
- RabbitMQ
- Kubernetes (DOKS)

### Frontend

- React 19
- TypeScript
- TanStack Query
- Recharts

---

## 📋 Checklist de Implementación

### Antes de empezar

- [ ] Aprobar plan de documentación
- [ ] Setup de infraestructura (Kafka, TimescaleDB, etc.)
- [ ] Crear estructura de carpetas

### Durante implementación

- [ ] Documentar cada servicio
- [ ] Implementar backend (C# .NET)
- [ ] Implementar frontend (React)
- [ ] Tests unitarios
- [ ] Tests de integración
- [ ] E2E testing

### Después de lanzar

- [ ] Monitoreo
- [ ] Optimización de latencias
- [ ] Ajuste de hiperparámetros
- [ ] Feedback de usuarios

---

## 🔗 Links Importantes

### Documentos Principales

- [DATA_ML_MICROSERVICES_STRATEGY.md](../../DATA_ML_MICROSERVICES_STRATEGY.md) - Documento original detallado
- [PLAN_DOCUMENTACION_IA.md](PLAN_DOCUMENTACION_IA.md) - Plan de documentación
- [ARQUITECTURA_GENERAL.md](ARQUITECTURA_GENERAL.md) - Visión general
- [INTEGRACIONES_EXTERNAS.md](INTEGRACIONES_EXTERNAS.md) - APIs externas

### Servicios (cuando estén documentados)

- [1-event-tracking/README.md](1-event-tracking/README.md)
- [2-data-pipeline/README.md](2-data-pipeline/README.md)
- [3-user-behavior/README.md](3-user-behavior/README.md)
- [4-feature-store/README.md](4-feature-store/README.md)
- [5-recommendation/README.md](5-recommendation/README.md)
- [6-lead-scoring/README.md](6-lead-scoring/README.md)
- [7-vehicle-intelligence/README.md](7-vehicle-intelligence/README.md)
- [8-ml-training/README.md](8-ml-training/README.md)
- [9-listing-analytics/README.md](9-listing-analytics/README.md)
- [10-review-service/README.md](10-review-service/README.md)

---

## 👥 Contribuyendo

### Para agregar documentación de un servicio

1. Crea una carpeta: `docs/api/ia-ml/{número}-{nombre}/`
2. Copia la estructura del [PLAN_DOCUMENTACION_IA.md](PLAN_DOCUMENTACION_IA.md)
3. Completa los archivos: README, ENDPOINTS, IMPLEMENTATION, etc.
4. Haz push a rama `feature/ia-{número}-{nombre}`
5. Crea PR a development

### Estructura esperada por servicio

```
{número}-{nombre}/
├── README.md                    (500-800 líneas)
├── ENDPOINTS.md                 (300-500 líneas)
├── [DOMAIN_MODELS | FEATURES | ALGORITHMS].md
├── IMPLEMENTATION.md            (1,500-2,000 líneas código)
├── FRONTEND_INTEGRATION.md      (1,000-1,500 líneas código)
├── TESTING.md                   (800-1,200 líneas código)
└── DEPLOYMENT.md                (300-400 líneas)
```

---

## 📞 Soporte

- **Preguntas sobre arquitectura:** Ver [ARQUITECTURA_GENERAL.md](ARQUITECTURA_GENERAL.md)
- **Preguntas sobre plan:** Ver [PLAN_DOCUMENTACION_IA.md](PLAN_DOCUMENTACION_IA.md)
- **Preguntas sobre dependencias:** Ver [INTEGRACIONES_EXTERNAS.md](INTEGRACIONES_EXTERNAS.md)
- **Preguntas sobre un servicio específico:** Ver carpeta del servicio

---

## 📊 Estado Actual

| Componente       | Estado        | Progreso |
| ---------------- | ------------- | -------- |
| Plan             | ✅ Completado | 100%     |
| Arquitectura     | ✅ Completado | 100%     |
| Dependencias     | ✅ Mapeadas   | 100%     |
| EventTracking    | ⏳ Próximo    | 0%       |
| DataPipeline     | ⏳ Próximo    | 0%       |
| UserBehavior     | ⏳ Próximo    | 0%       |
| FeatureStore     | ⏳ Próximo    | 0%       |
| Recommendation   | ⏳ Próximo    | 0%       |
| LeadScoring      | ⏳ Próximo    | 0%       |
| VehicleIntel     | ⏳ Próximo    | 0%       |
| MLTraining       | ⏳ Próximo    | 0%       |
| ListingAnalytics | ⏳ Próximo    | 0%       |
| ReviewService    | ⏳ Próximo    | 0%       |

---

## 📝 Changelog

### v1.0 (15 Enero 2026)

- ✅ Plan de documentación creado
- ✅ Arquitectura general documentada
- ✅ Integraciones externas mapeadas
- ✅ Estructura de carpetas creada
- ⏳ Comenzar Sprint 1 (EventTrackingService)

---

**Documentación de IA & ML de OKLA**  
_Enero 15, 2026_  
_Por completar: ~50,000 líneas de documentación en 12 semanas_
