# 📊 Resumen Ejecutivo - Plan de IA & ML para OKLA

**Fecha:** Enero 15, 2026  
**Duración:** 12 semanas (3 meses)  
**Equipo:** 2 desarrolladores backend + 1 ML engineer + 1 frontend developer

---

## 🎯 Objetivos

### Primario

Implementar **9 microservicios de Data & ML** que diferencien a OKLA del resto de marketplaces de vehículos en República Dominicana.

### Secundarios

1. Documentar completamente cada servicio (~50,000 líneas)
2. Entrenar 14 modelos de ML
3. Integrar con React frontend
4. Implementar tests completos

---

## 📈 Impacto Esperado

| Métrica                 | Baseline | Target (6m) | Impact |
| ----------------------- | -------- | ----------- | ------ |
| **User Engagement**     |          |             |        |
| Avg time on site        | 4 min    | 7 min       | ↑75%   |
| Pages/session           | 3.2      | 4.5         | ↑40%   |
| **Conversión**          |          |             |        |
| View → Contact          | 8%       | 12%         | ↑50%   |
| Contact → Sale          | 25%      | 35%         | ↑40%   |
| **Dealer Satisfaction** |          |             |        |
| NPS                     | 45       | 65          | ↑44%   |
| Renewal rate            | 70%      | 85%         | ↑21%   |
| **Revenue**             |          |             |        |
| MRR                     | $50k     | $120k       | ↑140%  |

---

## 🏗️ 9 Microservicios

### Sprint 1-2: Core (Crítico)

1. **EventTrackingService** (5050) - Captura eventos
2. **DataPipelineService** (5051) - ETL
3. **UserBehaviorService** (5052) - Perfiles
4. **FeatureStoreService** (5053) - Features

### Sprint 3-4: Intelligence

5. **RecommendationService** (5054) - Recomendaciones
6. **LeadScoringService** (5055) - Lead scoring
7. **VehicleIntelligenceService** (5056) - Pricing/Demanda
8. **MLTrainingService** (5057) - Entrenamientos

### Bonus

9. **ListingAnalyticsService** (5058) - Estadísticas
10. **ReviewService** (5059) - Reviews

---

## 💰 Costos

### Infraestructura (Auto-hospedada en DOKS)

- Kafka: 3 brokers (shared infrastructure)
- TimescaleDB: Extension de PostgreSQL (existe)
- Redis: (existe)
- RabbitMQ: (existe)
- MLflow: (nuevo, minor)
- Elasticsearch: (nuevo, minor)
- **Total nueva infraestructura:** ~$500/mes (compute)

### Servicios Externos

- **Google BigQuery:** $650/mes (cuando crezca mucho)
- **Total:** ~$1,150/mes en producción

### One-time Setup

- Licenses: $0 (todo open source)
- Training/onboarding: 40 horas

### ROI

- Inversión inicial: $30k (3 meses dev)
- Recoup en: 3 meses (si MRR sube $120k)

---

## 📅 Timeline

### Semana 1-2: Setup

- [ ] Setup Kafka, TimescaleDB, MLflow
- [ ] Crear estructura base de servicios
- [ ] Setup de CI/CD

### Semana 3-5: Core Services

- [ ] EventTrackingService
- [ ] DataPipelineService
- [ ] UserBehaviorService
- [ ] FeatureStoreService
- **Documentación:** 4 servicios (~20,000 líneas)

### Semana 6-8: Smart Services

- [ ] RecommendationService
- [ ] LeadScoringService
- [ ] VehicleIntelligenceService
- [ ] MLTrainingService
- **Documentación:** 4 servicios (~25,000 líneas)
- **ML:** Entrenar primeros modelos

### Semana 9-10: Analytics

- [ ] ListingAnalyticsService
- [ ] ReviewService
- **Documentación:** 2 servicios (~5,000 líneas)

### Semana 11-12: Frontend + Polish

- [ ] Dashboards para dealers
- [ ] Recomendaciones en homepage
- [ ] Reviews en listings
- [ ] Testing completo
- [ ] Deploy a producción

---

## 📊 Servicios Prioritarios

### ¿Por qué empezar por eventos?

```
Sin eventos → Sin datos → Sin ML → Sin insights
```

### ¿Por qué es crítico el DataPipeline?

```
Datos raw → Transformación → Features → Modelos
```

### ¿Por qué EventTracking es monetizable?

```
EventTracking → UserBehavior → Recomendaciones
                              → Leads scoring (dealers pagan por esto)
                              → Pricing (dealers quieren)
```

---

## 🔧 Stack Tecnológico

### Backend

- **.NET 8** - C# con Clean Architecture
- **MediatR** - CQRS pattern
- **FluentValidation** - Validaciones
- **PostgreSQL + TimescaleDB** - Almacenamiento

### ML/Data

- **Python 3.11** - Scripting + entrenamiento
- **scikit-learn + XGBoost** - Modelos
- **MLflow** - Model registry
- **Jupyter** - Notebooks

### Infrastructure

- **Kafka** - Event streaming
- **Airflow/Dagster** - Orchestration
- **Docker + Kubernetes** - Deployment
- **Redis** - Cache

### Frontend

- **React 19** - UI
- **TypeScript** - Type safety
- **TanStack Query** - Data fetching
- **Recharts** - Visualizaciones

---

## 🎓 14 Modelos ML a Entrenar

| #   | Modelo                 | Tipo          | Performance     |
| --- | ---------------------- | ------------- | --------------- |
| 1   | VehicleRecommender     | Colaborativo  | 0.85 AUC        |
| 2   | BuyerRecommender       | Híbrido       | 0.80 AUC        |
| 3   | SimilarVehicles        | Content-based | 0.88 Similarity |
| 4   | LeadScorer             | Clasificación | 0.92 F1         |
| 5   | ChurnPredictor         | Binaria       | 0.85 AUC        |
| 6   | ConversionPredictor    | Binaria       | 0.80 AUC        |
| 7   | PricePredictor         | Regresión     | 0.90 R²         |
| 8   | DaysToSalePredictor    | Regresión     | 0.85 R²         |
| 9   | UserSegmenter          | Clustering    | 0.75 Silhouette |
| 10  | VehicleClassifier      | Clasificación | 0.95 Accuracy   |
| 11  | FraudDetector          | Anomalía      | 0.92 Precision  |
| 12  | DescriptionAnalyzer    | NLP           | TBD             |
| 13  | SentimentAnalyzer      | NLP           | 0.88 Accuracy   |
| 14  | SearchIntentClassifier | Clasificación | 0.90 F1         |

---

## 📚 Documentación Esperada

### Por Servicio (~5,000-7,000 líneas c/u)

- README.md (500-800 líneas)
- ENDPOINTS.md (300-500 líneas)
- Domain Models (Entidades, DTOs)
- IMPLEMENTATION.md (1,500-2,000 líneas código C#)
- FRONTEND_INTEGRATION.md (1,000-1,500 líneas código React)
- TESTING.md (800-1,200 líneas código tests)
- DEPLOYMENT.md (300-400 líneas)

### Total

- **Documentación:** 50,000+ líneas
- **Código C# backend:** 15,000+ líneas
- **Código React frontend:** 8,000+ líneas
- **Tests:** 4,000+ líneas

---

## ✅ Criterios de Éxito

### Técnico

- [ ] 9 microservicios desplegados en producción
- [ ] 14 modelos entrenados y monitoreados
- [ ] 100% de documentación completada
- [ ] 80%+ test coverage
- [ ] <200ms latencia en predicciones

### Negocio

- [ ] Dealers pueden ver recomendaciones de leads
- [ ] Compradores ven "Vehículos para ti"
- [ ] Vendedores ven estadísticas de publicaciones
- [ ] Pricing inteligente recomendado
- [ ] Reviews con ratings de vendedor

### Métricas

- [ ] Conversión view→contact ↑ 50%
- [ ] Dealer NPS ↑ 20 puntos
- [ ] MRR ↑ $70k+ en 6 meses

---

## 🚨 Riesgos y Mitigación

| Riesgo                   | Probabilidad | Impacto | Mitigación                     |
| ------------------------ | ------------ | ------- | ------------------------------ |
| Datos insuficientes      | Media        | Alto    | Empezar con synthetic data     |
| Latencia en predicciones | Baja         | Medio   | Redis cache + pre-compute      |
| Model drift              | Alta         | Medio   | Reentrenamiento semanal        |
| Equipo insuficiente      | Baja         | Alto    | Priorizar core services        |
| Scope creep              | Alta         | Alto    | Documentación clara de roadmap |

---

## 🎯 Fase 1: Sprints 1-2 (Semanas 1-5)

### Objetivo

Capturar eventos y entrenarlos en features/perfiles.

### Entregables

1. EventTrackingService (5050)

   - Frontend SDK para capturar eventos
   - Kafka integration
   - 1M+ eventos/día

2. DataPipelineService (5051)

   - Pipelines de transformación
   - Agregaciones en PostgreSQL
   - 10+ transformaciones

3. UserBehaviorService (5052)

   - Perfiles de usuario
   - Segmentación automática
   - 5+ segmentos

4. FeatureStoreService (5053)
   - 20+ features por usuario
   - 10+ features por vehículo
   - Redis caching

### Documentación

- 4 READMEs completos
- Endpoints especificados
- Código C# copy/paste ready
- React integration examples

### Success Metric

Tracking 500k+ eventos diarios desde frontend.

---

## 🎯 Fase 2: Sprints 3-4 (Semanas 6-12)

### Objetivo

Entrenar modelos y generar insights inteligentes.

### Entregables

1. RecommendationService (5054)

   - Algoritmo colaborativo
   - Content-based matching
   - 3-5 recs por usuario

2. LeadScoringService (5055)

   - Scoring Hot/Warm/Cold
   - 100+ features por lead
   - Dashboard para dealers

3. VehicleIntelligenceService (5056)

   - Pricing predictor
   - Demand forecasting
   - Anomaly detection

4. MLTrainingService (5057)
   - Pipeline de entrenamiento semanal
   - 14 modelos
   - A/B testing de versiones

### ML Models

- Recommendation (0.85 AUC)
- Lead Scoring (0.92 F1)
- Pricing (0.90 R²)
- +11 modelos más

### Frontend Integration

- "Vehículos para ti" section
- Lead scoring dashboard
- Pricing recommendations
- Analytics dashboard

---

## 📞 Contacto & Aprobaciones

| Rol           | Nombre | Status        |
| ------------- | ------ | ------------- |
| **CTO**       | -      | ⏳ Aprobación |
| **Product**   | -      | ⏳ Aprobación |
| **Tech Lead** | -      | ⏳ Aprobación |

---

## 📋 Próximos Pasos

1. ✅ **Plan creado** (Este documento)
2. ⏳ **Aprobar por leadership**
3. ⏳ **Kickoff meeting**
4. ⏳ **Sprint 1 comienza**

---

**Resumen Ejecutivo - IA & ML para OKLA**  
_Enero 15, 2026_  
_Documento clasificado: Interno_
