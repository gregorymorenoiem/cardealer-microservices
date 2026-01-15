# 🤖 APIs de IA de Terceros - OKLA

**Versión:** 1.0  
**Última actualización:** Enero 15, 2026  
**Estado:** 📋 Documentación en progreso

---

## 🎯 Propósito

Este directorio documenta todas las **APIs de IA de terceros** (externas) que OKLA consumirá para potenciar sus servicios de Machine Learning y AI.

---

## 📦 APIs de Terceros Documentadas (9 APIs)

### 1️⃣ **OpenAI** (ChatGPT, Embeddings, Moderation)

- **Propósito:** LLM para chatbot, embeddings para similitud, moderation para reviews
- **Costo:** $0.50-$15/1M tokens (según modelo)
- **Documentación:** [OPENAI_API.md](OPENAI_API.md)
- **Servicios que usan:**
  - ChatbotService (ChatGPT para soporte)
  - ReviewService (Moderation de reviews)
  - RecommendationService (Embeddings para similitud)

### 2️⃣ **Google Vertex AI** (Embeddings, Tabular, Forecasting)

- **Propósito:** Modelos pre-trained para recomendaciones, predicción de demanda, pricing
- **Costo:** Gratis tier ($6/mes después)
- **Documentación:** [GOOGLE_VERTEX_AI.md](GOOGLE_VERTEX_AI.md)
- **Servicios que usan:**
  - VehicleIntelligenceService (Pricing predictions)
  - RecommendationService (Recomendaciones de vehículos)
  - DataPipelineService (Forecasting de demanda)

### 3️⃣ **Hugging Face** (Transformers, Sentiment Analysis)

- **Propósito:** Modelos de NLP open-source, análisis de sentimiento, clasificación
- **Costo:** Gratis (open-source)
- **Documentación:** [HUGGING_FACE_API.md](HUGGING_FACE_API.md)
- **Servicios que usan:**
  - ReviewService (Sentiment analysis de reviews)
  - UserBehaviorService (Clasificación de intención)
  - ChatbotService (Procesamiento de NLP)

### 4️⃣ **Cohere** (LLM alternative, Text Generation)

- **Propósito:** Generación de descripciones de vehículos, mejora de listing
- **Costo:** Gratis tier, $0.50-$1/1M tokens
- **Documentación:** [COHERE_API.md](COHERE_API.md)
- **Servicios que usan:**
  - ListingAnalyticsService (Mejorar descripciones)
  - RecommendationService (Personalized recommendations copy)

### 5️⃣ **Anthropic Claude** (Alternative LLM)

- **Propósito:** Análisis profundo, resúmenes inteligentes
- **Costo:** $0.80-$24/1M tokens
- **Documentación:** [ANTHROPIC_CLAUDE_API.md](ANTHROPIC_CLAUDE_API.md)
- **Servicios que usan:**
  - ChatbotService (Conversaciones más sofisticadas)
  - RecommendationService (Análisis de preferencias)

### 6️⃣ **AWS SageMaker** (ML Platform)

- **Propósito:** Entrenamiento de modelos custom, inference endpoints
- **Costo:** $0.25-$5/hora según instancia
- **Documentación:** [AWS_SAGEMAKER.md](AWS_SAGEMAKER.md)
- **Servicios que usan:**
  - MLTrainingService (Entrenar modelos custom)
  - VehicleIntelligenceService (Inference endpoints)
  - LeadScoringService (Models en producción)

### 7️⃣ **Replicate** (Simple ML API)

- **Propósito:** Modelos de visión (OCR para documentos), generación de imágenes
- **Costo:** $0.00075-$0.01 por predicción
- **Documentación:** [REPLICATE_API.md](REPLICATE_API.md)
- **Servicios que usan:**
  - MediaService (OCR en documentos)
  - ListingAnalyticsService (Análisis de fotos)

### 8️⃣ **Stripe** (Payment ML - Fraud Detection)

- **Propósito:** Detección de fraude en transacciones
- **Costo:** Incluido en comisión de pagos
- **Documentación:** [STRIPE_ML_API.md](STRIPE_ML_API.md)
- **Servicios que usan:**
  - BillingService (Fraud detection)

---

## 📊 Tabla Comparativa

| Provider             | Caso de Uso          | Costo             | Latencia | Alternativa       |
| -------------------- | -------------------- | ----------------- | -------- | ----------------- |
| **OpenAI**           | LLM + Embeddings     | $0.5-15/1M tokens | 1-5s     | Claude, Cohere    |
| **Google Vertex AI** | Tabular + Embeddings | $6/mes            | <1s      | Azure ML          |
| **Hugging Face**     | NLP Models           | Gratis            | <1s      | AWS SageMaker     |
| **Cohere**           | Text Generation      | Gratis tier       | 1-3s     | OpenAI            |
| **Claude**           | Advanced LLM         | $0.8-24/1M tokens | 2-5s     | OpenAI            |
| **AWS SageMaker**    | Custom Models        | $0.25-5/h         | <1s      | Google Vertex     |
| **Replicate**        | Vision + Generative  | $0.0001-0.01      | 5-30s    | API Vision Google |
| **Stripe**           | Fraud Detection      | Incluido          | <100ms   | Chargify          |
| **Resend**           | Email Transaccional  | $0.20/1K emails   | <100ms   | SendGrid          |

---

## 🔄 Flujo de Integración

```
┌──────────────────────────────────────────────────────────────────────┐
│                     SERVICIOS INTERNOS OKLA                         │
├──────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ChatbotService (5060)                                              │
│  ├─> OpenAI (ChatGPT para respuestas)                              │
│  ├─> OpenAI (Moderation para filtros)                              │
│  └─> Hugging Face (Sentiment analysis)                             │
│                                                                      │
│  ReviewService (5059)                                               │
│  ├─> OpenAI (Moderation de reviews)                                │
│  ├─> Hugging Face (Sentimiento)                                    │
│  └─> Google Vertex (Clasificación)                                 │
│                                                                      │
│  RecommendationService (5054)                                       │
│  ├─> OpenAI (Embeddings de vehículos)                              │
│  ├─> Google Vertex AI (Modelos pre-trained)                        │
│  ├─> Cohere (Generación de descripción)                            │
│  └─> Hugging Face (Clasificación de usuario)                       │
│                                                                      │
│  VehicleIntelligenceService (5056)                                  │
│  ├─> Google Vertex AI (Pricing predictions)                        │
│  └─> AWS SageMaker (Modelos custom)                                │
│                                                                      │
│  LeadScoringService (5055)                                          │
│  ├─> AWS SageMaker (Hot/Warm/Cold scoring)                         │
│  └─> Google Vertex AI (Propensity models)                          │
│                                                                      │
│  ListingAnalyticsService (5058)                                     │
│  ├─> Replicate (OCR en documentos)                                 │
│  ├─> Google Vision (Análisis de fotos)                             │
│  └─> Cohere (Mejora de descripciones)                              │
│                                                                      │
│  BillingService (integrado)                                         │
│  └─> Stripe (Fraud detection ML)                                   │
│                                                                      │
│  MLTrainingService (5057)                                           │
│  ├─> AWS SageMaker (Entrenamientos)                                │
│  ├─> Google Vertex (AutoML)                                        │
│  └─> Hugging Face (Transfer learning)                              │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 💰 Costo Total Estimado (Mensual)

### Tier Free (Desarrollo)

```
OpenAI:          $0 (gratis 3 meses)
Google Vertex:   $0 (gratis tier)
Hugging Face:    $0 (open-source)
Cohere:          $0 (gratis tier)
Claude:          $0 (testing account)
AWS SageMaker:   $0 (free tier training)
Replicate:       $0 (micro predictions)
─────────────────────────────────
TOTAL:           $0
```

### Tier Production (Small Scale)

```
OpenAI:          $500 (100M tokens/mes)
Google Vertex:   $50 (small queries)
Hugging Face:    $0 (self-hosted)
Cohere:          $100 (small usage)
Claude:          $200 (testing)
AWS SageMaker:   $100 (inference)
Replicate:       $50 (OCR usage)
Stripe (incluido):$0
─────────────────────────────────
TOTAL:           ~$1,000/mes
```

### Tier Production (Large Scale - 6 meses después)

```
OpenAI:          $2,000 (500M tokens/mes)
Google Vertex:   $300 (BigQuery queries)
Hugging Face:    $100 (self-hosted infra)
Cohere:          $500 (more usage)
Claude:          $500 (higher volume)
AWS SageMaker:   $1,000 (dedicated endpoints)
Replicate:       $500 (more OCR)
─────────────────────────────────
TOTAL:           ~$5,000/mes
```

---

## 🚀 Uso por Fase

### **Fase 1: MVP (Semanas 1-4)**

- OpenAI (básico ChatGPT)
- Google Vertex AI (embeddings gratuitos)
- Hugging Face (sentiment analysis local)

### **Fase 2: Escalado (Semanas 5-8)**

- Cohere (descripción mejorada)
- AWS SageMaker (entrenamientos custom)
- Claude (testing)

### **Fase 3: Producción (Semanas 9-12)**

- Todas las APIs en producción
- Fallbacks configurados
- Monitoring y alertas activas

---

## 📚 Documentos Disponibles

| Documento                                                  | Servicios                                            | Endpoints | Estado         |
| ---------------------------------------------------------- | ---------------------------------------------------- | --------- | -------------- |
| [OPENAI_API.md](OPENAI_API.md)                             | ChatbotService, ReviewService, RecommendationService | 15+       | 📋 En progreso |
| [GOOGLE_VERTEX_AI.md](GOOGLE_VERTEX_AI.md)                 | VehicleIntelligenceService, RecommendationService    | 8+        | 📋 En progreso |
| [HUGGING_FACE_API.md](HUGGING_FACE_API.md)                 | ReviewService, UserBehaviorService                   | 10+       | 📋 En progreso |
| [COHERE_API.md](COHERE_API.md)                             | ListingAnalyticsService, RecommendationService       | 6+        | 📋 En progreso |
| [ANTHROPIC_CLAUDE_API.md](ANTHROPIC_CLAUDE_API.md)         | ChatbotService, RecommendationService                | 8+        | 📋 En progreso |
| [AWS_SAGEMAKER.md](AWS_SAGEMAKER.md)                       | MLTrainingService, VehicleIntelligenceService        | 12+       | 📋 En progreso |
| [REPLICATE_API.md](REPLICATE_API.md)                       | MediaService, ListingAnalyticsService                | 5+        | 📋 En progreso |
| [STRIPE_ML_API.md](STRIPE_ML_API.md)                       | BillingService                                       | 3+        | 📋 En progreso |
| [ARQUITECTURA_INTEGRACION.md](ARQUITECTURA_INTEGRACION.md) | -                                                    | -         | 📋 En progreso |
| [MATRIZ_COMPARATIVA_LLMS.md](MATRIZ_COMPARATIVA_LLMS.md)   | -                                                    | -         | 📋 En progreso |

---

## 🛠️ Cómo Usar Esta Documentación

### Para Developers

1. Lee el documento de tu API específica
2. Copia los ejemplos de C# en tu servicio
3. Configura las API keys en secrets
4. Testa en desarrollo primero

### Para ML Engineers

1. Lee [ARQUITECTURA_INTEGRACION.md](ARQUITECTURA_INTEGRACION.md)
2. Compara pros/cons en [MATRIZ_COMPARATIVA_LLMS.md](MATRIZ_COMPARATIVA_LLMS.md)
3. Elige qué modelo usar donde

### Para DevOps

1. Crea secrets en Kubernetes para API keys
2. Configura rate limiting
3. Monitorea uso y costos
4. Setea fallbacks

---

## 🔑 Setup Requerido

### 1. Obtener API Keys

```bash
# OpenAI
https://platform.openai.com/account/api-keys

# Google Vertex AI
gcloud auth application-default login

# Hugging Face
https://huggingface.co/settings/tokens

# Cohere
https://dashboard.cohere.ai/api-keys

# Claude (Anthropic)
https://console.anthropic.com/account/keys

# AWS SageMaker
aws configure

# Replicate
https://replicate.com/account/api-tokens

# Stripe
https://dashboard.stripe.com/apikeys
```

### 2. Guardar en Kubernetes Secrets

```bash
kubectl create secret generic ai-apis \
  --from-literal=OPENAI_API_KEY=sk-... \
  --from-literal=GOOGLE_VERTEX_KEY=... \
  --from-literal=HUGGING_FACE_KEY=... \
  --from-literal=COHERE_KEY=... \
  --from-literal=ANTHROPIC_KEY=... \
  --from-literal=AWS_ACCESS_KEY=... \
  --from-literal=REPLICATE_KEY=... \
  -n okla
```

### 3. Usar en Services

```csharp
// En Program.cs
var openaiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
var vertexKey = Environment.GetEnvironmentVariable("GOOGLE_VERTEX_KEY");
// ...
```

---

## 📊 Roadmap de Implementación

```
Semana 1-2: Setup
├─ Crear accounts en todas las plataformas
├─ Obtener API keys
└─ Crear secrets en K8s

Semana 3-4: OpenAI + Hugging Face
├─ Integración ChatGPT en ChatbotService
├─ Embeddings en RecommendationService
└─ Sentiment en ReviewService

Semana 5-6: Google Vertex + Cohere
├─ Pricing predictions
├─ Text generation
└─ Recomendaciones mejoradas

Semana 7-8: AWS SageMaker + Claude
├─ Entrenamientos custom
├─ Advanced LLM features
└─ Lead scoring

Semana 9-10: Replicate + Optimizaciones
├─ OCR para documentos
├─ Cache de resultados
└─ Fallbacks

Semana 11-12: Production + Monitoring
├─ Load testing
├─ Cost optimization
├─ Alertas y monitoring
└─ Documentation final
```

---

## ⚡ APIs Por Prioritarios

### 🔴 CRÍTICOS (Semanas 1-4)

1. **OpenAI** - ChatGPT para chatbot y moderation
2. **Google Vertex AI** - Embeddings para recomendaciones
3. **Hugging Face** - Sentiment analysis

### 🟡 IMPORTANTES (Semanas 5-8)

4. **Cohere** - Text generation mejorado
5. **AWS SageMaker** - Entrenamientos custom
6. **Claude** - LLM alternativo

### 🟢 OPCIONALES (Semanas 9-12)

7. **Replicate** - OCR y vision
8. **Stripe ML** - Fraud detection (ya incluido)

---

## 🎯 Métricas de Éxito

| Métrica              | Meta         |
| -------------------- | ------------ |
| Latencia OpenAI      | <2s (P95)    |
| Latency Vertex AI    | <500ms (P95) |
| Accuracy sentiment   | >92%         |
| Accuracy pricing     | MAPE <8%     |
| Lead scoring AUC     | >0.80        |
| Costo por predicción | <$0.001      |
| Uptime APIs          | 99.5%        |

---

## 📞 Próximos Pasos

1. ✅ Revisar esta documentación
2. 📋 Crear accounts en todas las plataformas
3. 🔑 Guardar API keys en secrets
4. 📖 Leer documentación específica de cada API
5. 🚀 Comenzar integración (semana 3)

---

_Documentación de APIs de IA de Terceros para OKLA_  
_Última actualización: Enero 15, 2026_  
_Próximo: Documentar cada API en detalle_
