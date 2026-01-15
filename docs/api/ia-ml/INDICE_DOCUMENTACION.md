# 📑 Índice de Documentación - IA & ML OKLA

**Fecha:** Enero 15, 2026  
**Versión:** 1.0

---

## 📚 Documentos Creados

### 🎯 Planning & Strategy

| Documento                                              | Descripción                         | Líneas | Propósito        |
| ------------------------------------------------------ | ----------------------------------- | ------ | ---------------- |
| [README.md](README.md)                                 | Índice general + quick start        | 250    | Punto de entrada |
| [PLAN_DOCUMENTACION_IA.md](PLAN_DOCUMENTACION_IA.md)   | Plan detallado de documentación     | 450    | Roadmap          |
| [RESUMEN_EJECUTIVO.md](RESUMEN_EJECUTIVO.md)           | Para C-level / leadership           | 350    | Aprobación       |
| [ARQUITECTURA_GENERAL.md](ARQUITECTURA_GENERAL.md)     | Cómo funcionan los servicios juntos | 400    | Visión técnica   |
| [INTEGRACIONES_EXTERNAS.md](INTEGRACIONES_EXTERNAS.md) | APIs externas a consumir            | 500    | Dependencias     |
| [MATRIZ_APIS_COMPLETA.md](MATRIZ_APIS_COMPLETA.md)     | 360° de todos los APIs              | 700    | Referencia       |
| [INDICE_DOCUMENTACION.md](INDICE_DOCUMENTACION.md)     | Este archivo                        | 300    | Navegación       |

**Total docs: 2,950 líneas de planificación**

---

## 🗺️ Cómo Navegar

### Si eres...

#### 👨‍💼 Ejecutivo (CEO/Product Manager)

1. Lee [RESUMEN_EJECUTIVO.md](RESUMEN_EJECUTIVO.md) (5 min)
2. Revisa impacto esperado
3. Aprueba o sugiere cambios

#### 👨‍💻 Tech Lead

1. Lee [ARQUITECTURA_GENERAL.md](ARQUITECTURA_GENERAL.md) (15 min)
2. Revisa [PLAN_DOCUMENTACION_IA.md](PLAN_DOCUMENTACION_IA.md) (20 min)
3. Comienza con EventTrackingService (próximo)

#### 🧠 ML Engineer

1. Lee [INTEGRACIONES_EXTERNAS.md](INTEGRACIONES_EXTERNAS.md) (20 min)
2. Ve a [MLTrainingService](#8-ml-training-service) en MATRIZ_APIS
3. Diseña pipeline de entrenamiento

#### 🎨 Frontend Developer

1. Lee [ARQUITECTURA_GENERAL.md](ARQUITECTURA_GENERAL.md) - sección "Casos de Uso"
2. Mira [MATRIZ_APIS_COMPLETA.md](MATRIZ_APIS_COMPLETA.md) - busca tu servicio
3. Espera documentación de [RecommendationService](#5-recommendation-service)

#### 🔧 DevOps Engineer

1. Lee [INTEGRACIONES_EXTERNAS.md](INTEGRACIONES_EXTERNAS.md) - sección "Stack Tecnológico"
2. Prepara Kubernetes para Kafka, MLflow, Elasticsearch
3. Setup CI/CD para Python + .NET

---

## 📋 Documentos Por Servicio (Próximos)

### Cuando se creen (Semana 3+), tendrán esta estructura:

```
{número}-{servicio}/
├── README.md                    (500-800 líneas)
│   ├─ ¿Por qué es necesario?
│   ├─ De un vistazo
│   ├─ Flujo de datos
│   └─ Tabla de contenidos
│
├── ENDPOINTS.md                 (300-500 líneas)
│   ├─ GET /api/...
│   ├─ POST /api/...
│   ├─ PUT /api/...
│   ├─ DELETE /api/...
│   └─ Ejemplos de request/response
│
├── DOMAIN_MODELS.md o similar   (200-400 líneas)
│   ├─ Entidades principales
│   ├─ Value Objects
│   ├─ Enumeraciones
│   └─ Relaciones
│
├── IMPLEMENTATION.md            (1,500-2,000 líneas)
│   ├─ Domain Layer (entities, interfaces)
│   ├─ Application Layer (commands, queries, validators)
│   ├─ Infrastructure Layer (context, repositories, services)
│   ├─ API Layer (controllers, program.cs)
│   └─ Código completo copy/paste ready
│
├── FRONTEND_INTEGRATION.md      (1,000-1,500 líneas)
│   ├─ React Components
│   ├─ Custom Hooks (React Query)
│   ├─ TypeScript Types
│   └─ Ejemplos de uso
│
├── TESTING.md                   (800-1,200 líneas)
│   ├─ Unit Tests
│   ├─ Integration Tests
│   ├─ E2E Tests
│   └─ Código de tests
│
└── DEPLOYMENT.md                (300-400 líneas)
    ├─ Dockerfile
    ├─ Kubernetes manifests
    ├─ Environment variables
    └─ Health checks
```

---

## 🎯 Timeline de Documentación

```
SEMANA 1-2: SETUP
└─ Crear infraestructura
└─ Crear estructura base

SEMANA 3-5: CORE SERVICES (20,000 líneas)
├─ 1-event-tracking/ (COMPLETO)
├─ 2-data-pipeline/ (COMPLETO)
├─ 3-user-behavior/ (COMPLETO)
└─ 4-feature-store/ (COMPLETO)

SEMANA 6-10: SMART SERVICES (27,000 líneas)
├─ 5-recommendation/ (COMPLETO)
├─ 6-lead-scoring/ (COMPLETO)
├─ 7-vehicle-intelligence/ (COMPLETO)
└─ 8-ml-training/ (COMPLETO)

SEMANA 11-12: ANALYTICS + POLISH (9,000 líneas)
├─ 9-listing-analytics/ (COMPLETO)
├─ 10-review-service/ (COMPLETO)
└─ Testing, bugfixes

TOTAL: ~56,000 líneas
```

---

## 📈 Estado Actual

### ✅ COMPLETADO

- [x] Plan de documentación general
- [x] Arquitectura de sistemas
- [x] Mapa de dependencias externas
- [x] Matriz de APIs
- [x] Estructura de carpetas

**5 documentos = 2,950 líneas**

### ⏳ PRÓXIMO

- [ ] EventTrackingService (semana 3)
- [ ] DataPipelineService (semana 4)
- [ ] UserBehaviorService (semana 4)
- [ ] FeatureStoreService (semana 5)

### 📅 PLANIFICADO

- [ ] RecommendationService (semana 6)
- [ ] LeadScoringService (semana 7)
- [ ] VehicleIntelligenceService (semana 8)
- [ ] MLTrainingService (semana 8)
- [ ] ListingAnalyticsService (semana 11)
- [ ] ReviewService (semana 12)

---

## 🔍 Búsqueda Rápida

### Por Concepto

#### 🔴 Eventos

- [ARQUITECTURA_GENERAL.md - Flujo de Eventos](#)
- [MATRIZ_APIS_COMPLETA.md - EventTrackingService](#1-eventrackingservice-puerto-5050)
- Próximo: `1-event-tracking/README.md`

#### 🟠 Datos

- [PLAN_DOCUMENTACION_IA.md - Data Pipeline](#)
- [MATRIZ_APIS_COMPLETA.md - DataPipelineService](#-datapipelineservice-puerto-5051)
- Próximo: `2-data-pipeline/README.md`

#### 🟡 Análisis

- [ARQUITECTURA_GENERAL.md - 14 Modelos ML](#14-modelos-de-ml-a-entrenar)
- [INTEGRACIONES_EXTERNAS.md - Stack Tecnológico](#-stack-de-alternativas-recomendadas)
- Próximo: `3-user-behavior/README.md`, `9-listing-analytics/README.md`

#### 🟢 ML/Predicción

- [PLAN_DOCUMENTACION_IA.md - Implementación](#🚀-plan-de-implementación-12-semanas)
- [MATRIZ_APIS_COMPLETA.md - Recommendation/LeadScoring/VehicleIntel](#4---vehicleintelligenceservice-puerto-5056)
- Próximo: `5-recommendation/README.md`, `6-lead-scoring/README.md`, `7-vehicle-intelligence/README.md`

#### 🟣 Entrenamientos

- [INTEGRACIONES_EXTERNAS.md - MLflow](#3--mlflow---model-registry--tracking)
- [MATRIZ_APIS_COMPLETA.md - MLTrainingService](#-mltrainingservice-puerto-5057)
- Próximo: `8-ml-training/README.md`

#### 🔵 Features

- [PLAN_DOCUMENTACION_IA.md - FeatureStore](#4-feature-store-service-puerto-5053)
- [MATRIZ_APIS_COMPLETA.md - FeatureStoreService](#-featurestoreservice-puerto-5053)
- Próximo: `4-feature-store/README.md`

#### 🟤 Reviews

- [MATRIZ_APIS_COMPLETA.md - ReviewService](#-reviewservice-puerto-5059)
- Próximo: `10-review-service/README.md`

### Por Tecnología

#### PostgreSQL

- [INTEGRACIONES_EXTERNAS.md - PostgreSQL](#10--postgresql---primary-database)
- [ARQUITECTURA_GENERAL.md - Stack Tecnológico](#-stack-tecnológico-recomendado)

#### Kafka

- [INTEGRACIONES_EXTERNAS.md - Apache Kafka](#2--apache-kafka---event-streaming)
- [ARQUITECTURA_GENERAL.md - Flujo de Datos](#-flujo-de-datos-completo)

#### MLflow

- [INTEGRACIONES_EXTERNAS.md - MLflow](#3--mlflow---model-registry--tracking)
- [PLAN_DOCUMENTACION_IA.md - ML Training](#-ml-training-service-puerto-5057)

#### TensorFlow / XGBoost

- [INTEGRACIONES_EXTERNAS.md - TensorFlow Serving](#7--tensorflow-serving---model-serving)
- [INTEGRACIONES_EXTERNAS.md - XGBoost](#8-9--scikit-learn--xgboost---ml-libraries)

#### Kubernetes/Docker

- [INTEGRACIONES_EXTERNAS.md - Stack Tecnológico](#-stack-de-alternativas-recomendadas)
- Próximo: `{servicio}/DEPLOYMENT.md` (en cada servicio)

#### React/TypeScript

- [PLAN_DOCUMENTACION_IA.md - Frontend Integration](#-integración-frontend-semanas-11-12)
- Próximo: `{servicio}/FRONTEND_INTEGRATION.md` (en cada servicio)

---

## 🎯 Cómo Usar Esta Documentación

### Para Comenzar Desarrollo

1. [ ] Lee [README.md](README.md) (5 min)
2. [ ] Lee [ARQUITECTURA_GENERAL.md](ARQUITECTURA_GENERAL.md) (15 min)
3. [ ] Revisa [PLAN_DOCUMENTACION_IA.md](PLAN_DOCUMENTACION_IA.md) (20 min)
4. [ ] Abre carpeta `1-event-tracking/` y comienza (cuando esté lista)

### Para Entender un Servicio

1. [ ] Ve a [MATRIZ_APIS_COMPLETA.md](MATRIZ_APIS_COMPLETA.md)
2. [ ] Busca el servicio en el índice (Ctrl+F)
3. [ ] Lee sección de API
4. [ ] Cuando esté documentado, abre carpeta correspondiente

### Para Agregar Documentación

1. [ ] Copia estructura de [PLAN_DOCUMENTACION_IA.md](PLAN_DOCUMENTACION_IA.md)
2. [ ] Crea carpeta `{número}-{nombre}/`
3. [ ] Completa archivos: README, ENDPOINTS, IMPLEMENTATION, etc.
4. [ ] Haz PR a development

### Para Monitorear Progreso

1. [ ] Revisa [Estado Actual](#-estado-actual) en este documento
2. [ ] Compara con [Timeline](#-timeline-de-documentación)
3. [ ] Actualiza cuando completes un servicio

---

## 🔗 Links Externos

### Documento Original (Fuente)

- [DATA_ML_MICROSERVICES_STRATEGY.md](../../DATA_ML_MICROSERVICES_STRATEGY.md) - Documento base con detalles específicos

### Documentación General del Proyecto

- [SPRINT_PLAN_MARKETPLACE.md](../../SPRINT_PLAN_MARKETPLACE.md) - Plan de sprints general
- [ESTRATEGIA_TIPOS_USUARIO_DEALERS.md](../../ESTRATEGIA_TIPOS_USUARIO_DEALERS.md) - Estrategia de dealers

### Otros APIs en OKLA

- [docs/api/pricing/](../pricing/) - APIs de pricing externas
- [docs/api/financing/](../financing/) - APIs de financiamiento
- [docs/api/vehicle-history/](../vehicle-history/) - APIs de historial vehicular

---

## 📞 Contacto & Soporte

- **Preguntas generales:** Revisa [README.md](README.md)
- **Preguntas técnicas:** Revisa [ARQUITECTURA_GENERAL.md](ARQUITECTURA_GENERAL.md)
- **Preguntas sobre un API:** Busca en [MATRIZ_APIS_COMPLETA.md](MATRIZ_APIS_COMPLETA.md)
- **Issues/Bugs:** Abre GitHub issue con tag `docs/ia-ml`

---

## ✅ Checklist de Documentación

### Completado

- [x] README principal
- [x] Plan de documentación (12 semanas)
- [x] Resumen ejecutivo
- [x] Arquitectura general
- [x] Integraciones externas
- [x] Matriz completa de APIs
- [x] Índice (este archivo)

### En Progreso

- [ ] EventTrackingService
- [ ] DataPipelineService
- [ ] UserBehaviorService
- [ ] FeatureStoreService

### Planificado

- [ ] RecommendationService
- [ ] LeadScoringService
- [ ] VehicleIntelligenceService
- [ ] MLTrainingService
- [ ] ListingAnalyticsService
- [ ] ReviewService

---

## 📊 Estadísticas

| Métrica                         | Valor  |
| ------------------------------- | ------ |
| **Documentos de Planning**      | 7      |
| **Líneas Planning**             | 2,950  |
| **Servicios a documentar**      | 10     |
| **APIs totales**                | 50+    |
| **Líneas esperadas (completo)** | 56,000 |
| **Semanas de documentación**    | 12     |
| **% Completado**                | 5%     |

---

**Índice de Documentación - IA & ML OKLA**  
_Enero 15, 2026_  
_v1.0 - Planning Phase Complete_
