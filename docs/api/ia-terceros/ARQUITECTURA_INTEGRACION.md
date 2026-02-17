# 🏗️ Arquitectura de Integración - Todos los APIs de IA

**Objetivo:** Cómo todos los 8 APIs de IA trabajan juntos en OKLA  
**Actualización:** Enero 15, 2026  
**Status:** Completo (12 servicios + 8 APIs)

---

## 📊 Vista General del Sistema

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                                 OKLA FRONTEND                                │
│  React 19 + TypeScript (okla.com.do)                                         │
└─────────────────────────────────┬──────────────────────────────────────────────┘
                                  │
                    ┌─────────────────────────────┐
                    │  API GATEWAY (Ocelot)       │
                    │  api.okla.com.do:8080       │
                    └────────────┬────────────────┘
                                 │
        ┌────────────┬────────────┼────────────┬──────────────┐
        │            │            │            │              │
        ▼            ▼            ▼            ▼              ▼
   ┌────────┐  ┌────────┐  ┌───────────┐  ┌──────────┐  ┌─────────┐
   │ Auth   │  │ User   │  │ Vehicles  │  │Billing   │  │Media    │
   │Service │  │Service │  │Service    │  │Service   │  │Service  │
   └────────┘  └────────┘  └───┬───────┘  └────┬─────┘  └────┬────┘
                                │              │             │
        ┌───────────────────────┼──────────────┼─────────────┘
        │                       │              │
        ▼                       ▼              ▼
   ┌──────────────────────────────────────────────┐
   │          AI/ML MICROSERVICES                 │
   │                                              │
   │  1. ChatbotService          (OpenAI + Claude)
   │  2. ReviewService           (OpenAI Moderation + HF)
   │  3. RecommendationService   (OpenAI Embeddings + Vertex AI)
   │  4. VehicleIntelligenceService (Vertex AI + SageMaker)
   │  5. LeadScoringService      (SageMaker XGBoost)
   │  6. ListingAnalyticsService (Replicate OCR + Google Vision)
   │  7. DataPipelineService     (Vertex AI AutoML)
   │  8. UserBehaviorService     (HF NLP + Cohere)
   │  9. NotificationService     (Cohere generation)
   │ 10. ErrorService            (Claude analysis)
   │ 11. DataAggregationService  (Vertex AI BigQuery)
   │ 12. MonitoringService       (CloudWatch + Stripe Radar)
   │                                              │
   └──────────────────┬───────────────────────────┘
                      │
        ┌─────────────┴──────────────┬─────────────┐
        │                            │             │
        ▼                            ▼             ▼
┌──────────────────┐     ┌──────────────────┐  ┌──────────────┐
│  External APIs   │     │   Databases      │  │ Message      │
│                  │     │                  │  │ Brokers      │
│ • OpenAI         │     │ • PostgreSQL     │  │              │
│ • Google Vertex  │     │ • BigQuery       │  │ • RabbitMQ   │
│ • Hugging Face   │     │ • Redis (cache)  │  │ • Kafka      │
│ • Cohere         │     │                  │  │              │
│ • Anthropic      │     └──────────────────┘  └──────────────┘
│ • AWS SageMaker  │
│ • Replicate      │
│ • Stripe         │
│ • Google Vision  │
└──────────────────┘
```

---

## 🔄 Flujos de Datos Principales

### Flujo 1: Usuario Busca Vehículo → Recomendación

```
USUARIO FRONTEND
    │
    ├─ Escribe búsqueda: "SUV 2023 bajo presupuesto"
    │
    ▼
GATEWAY
    │
    ├─ Route: /api/search → VehiclesSaleService
    │
    ▼
VEHICLESSALESERVICE
    │
    ├─ Query: SELECT * FROM vehicles WHERE category='SUV' AND year=2023
    │
    ▼
RECOMMENDATIONSERVICE (Async via RabbitMQ)
    │
    ├─ 1. OpenAI Embeddings
    │      - Convertir búsqueda a vector (1536 dims)
    │      - input: "SUV 2023 bajo presupuesto"
    │      - output: [0.123, -0.456, ...]
    │
    ├─ 2. PostgreSQL Vector Search
    │      - Buscar vehículos similares (cosine similarity)
    │      - SELECT * FROM vehicles WHERE embedding <-> query_embedding < 0.3
    │
    ├─ 3. Google Vertex AI Ranking
    │      - Re-rankear resultados por relevancia
    │      - Modelo: ranking-v2
    │
    ├─ 4. Cohere Classification (opcional)
    │      - Clasificar: ¿Es búsqueda premium o budget?
    │      - Ajustar recomendaciones según clase
    │
    ▼
FRONTEND
    │
    └─ Mostrar resultados ordenados: [Toyota RAV4 2023, Honda CR-V 2023, ...]
```

### Flujo 2: Dealer Publica Vehículo → Analytics

```
DEALER FRONTEND
    │
    ├─ Completa formulario de nuevo vehículo
    │ - Título: "Toyota Corolla 2023 Automático"
    │ - Descripción: "Vehículo en excelente estado..."
    │ - Fotos: 8 imágenes
    │
    ▼
VEHICLESSALESERVICE
    │
    ├─ POST /api/vehicles
    │ ├─ Guardar en PostgreSQL
    │ └─ Publicar evento: VehicleCreated
    │
    ▼
RabbitMQ (Event Bus)
    │
    ├─ VehicleCreated event
    │
    ├─────────────────────────────────────────┐
    │                                         │
    ▼                                         ▼
LISTINGANALYTICSSERVICE          NOTIFICATIONSERVICE
    │                                 │
    ├─ 1. Replicate OCR              ├─ Generar email
    │      - Extraer VIN de fotos    │
    │      - Verificar documentos    │ - Cohere (generar copy)
    │                                 │ - SendGrid API
    ├─ 2. Google Vision              │
    │      - Analizar calidad de     └─ Enviar a cliente+dealer
    │        fotos
    │      - Detectar daños
    │
    ├─ 3. Claude Analysis
    │      - Analizar descripción
    │      - Sugerir mejoras
    │
    ├─ Guardar: listing_quality_score (1-100)
    │
    ▼
DATAWAREHOUSESERVICE
    │
    └─ Enviar a BigQuery para analytics
```

### Flujo 3: Usuario Lee Reviews → Análisis IA

```
REVIEWSERVICE
    │
    ├─ Nuevo review: "Muy buen coche, excelente atención del vendedor"
    │
    ├─ 1. OpenAI Moderation
    │      - Verificar si es spam/ofensivo
    │      - Resultado: {"flagged": false}
    │
    ├─ 2. Hugging Face Sentiment
    │      - Análisis: Positivo (0.95 confidence)
    │      - Label: 5 estrellas
    │
    ├─ 3. Claude Deep Analysis (opcional)
    │      - Extraer aspectos (vendedor, proceso, vehículo)
    │      - Generar summary
    │
    ├─ Guardar: {
    │   sentiment: "positive",
    │   score: 5.0,
    │   flagged: false,
    │   aspects: ["excellent_service", "good_condition"]
    │ }
    │
    ▼
FRONTEND
    │
    └─ Mostrar review con badge ✅ (verificado por IA)
```

### Flujo 4: Lead Scoring para Dealers

```
USERSERVICE
    │
    ├─ Usuario nuevo registrado
    │ ├─ Email: john@example.com
    │ ├─ Profile: {"interests": "SUV", "budget": 1500000}
    │ └─ Activity: viewed 5 vehicles
    │
    ▼
LEADSCORING SERVICE
    │
    ├─ 1. Feature Engineering
    │      - profile_score: 75
    │      - listings_viewed: 5
    │      - days_since_activity: 2
    │      - category_interested: SUV
    │      - budget_range: 1500000-2000000
    │      - contact_attempts: 0
    │
    ├─ 2. AWS SageMaker
    │      - Inference: lead-scoring-endpoint
    │      - Modelo: XGBoost
    │      - Resultado: hot_prob=0.87, warm_prob=0.10, cold_prob=0.03
    │      - Classification: HOT 🔴
    │
    ├─ 3. Guardar en leadscoring_predictions
    │
    ▼
DEALERDASHBOARD
    │
    └─ Dealer ve: "John - HOT lead (87% probability)"
       └─ CTA: "Contactar ahora"
```

### Flujo 5: Fraud Detection en Pagos

```
FRONTEND (Dealer)
    │
    ├─ Intenta pagar suscripción
    │ ├─ Plan: Pro ($129/mes)
    │ ├─ Tarjeta: VISA **** 4242
    │ └─ IP: 200.100.50.1 (RD)
    │
    ▼
BILLINGSERVICE
    │
    ├─ 1. Crear Payment Intent en Stripe
    │      POST /v1/payment_intents
    │      ├─ amount: 12900 (centavos)
    │      ├─ customer: cus_xxx
    │      ├─ payment_method: pm_xxx
    │      └─ radar_options: {session: {ip_address, user_agent}}
    │
    ├─ 2. Stripe Radar ML Analysis
    │      - Verificar: ¿IP consistente con registro?
    │      - Verificar: ¿Monto típico?
    │      - Verificar: ¿Tarjeta nueva?
    │      - Resultado: risk_level = "low"
    │
    ├─ 3. Confirmar Pago
    │      - Charge succeeded ✅
    │      - chargeId: ch_xxxxx
    │
    ├─ 4. Crear Suscripción
    │      POST /v1/subscriptions
    │      ├─ customer: cus_xxx
    │      ├─ items: [{price: price_pro}]
    │      ├─ trial_period_days: 0 (ya pagó)
    │      └─ metadata: {dealer_id, plan}
    │
    ▼
BILLINGWEBHOOK
    │
    ├─ customer.subscription.created
    │ └─ Actualizar dealermanagementservice
    │    └─ dealer.currentPlan = "Pro"
    │    └─ dealer.isSubscriptionActive = true
    │    └─ dealer.maxActiveListings = 50
    │
    ▼
DEALERDASHBOARD
    │
    └─ "Suscripción activada ✅ Pro plan"
```

---

## 📊 Matriz de Servicios vs APIs

| Servicio                   | OpenAI | Google | HF  | Cohere | Claude | SageMaker | Replicate | Stripe |
| -------------------------- | ------ | ------ | --- | ------ | ------ | --------- | --------- | ------ |
| ChatbotService             | ✅     | ❌     | ❌  | ❌     | ✅     | ❌        | ❌        | ❌     |
| ReviewService              | ✅     | ❌     | ✅  | ❌     | ✅     | ❌        | ❌        | ❌     |
| RecommendationService      | ✅     | ✅     | ❌  | ❌     | ❌     | ❌        | ❌        | ❌     |
| VehicleIntelligenceService | ❌     | ✅     | ❌  | ❌     | ❌     | ✅        | ❌        | ❌     |
| LeadScoringService         | ❌     | ❌     | ❌  | ❌     | ❌     | ✅        | ❌        | ❌     |
| ListingAnalyticsService    | ❌     | ✅     | ❌  | ❌     | ❌     | ❌        | ✅        | ❌     |
| DataPipelineService        | ❌     | ✅     | ❌  | ❌     | ❌     | ✅        | ❌        | ❌     |
| UserBehaviorService        | ❌     | ❌     | ✅  | ✅     | ❌     | ❌        | ❌        | ❌     |
| NotificationService        | ❌     | ❌     | ❌  | ✅     | ❌     | ❌        | ❌        | ❌     |
| ErrorService               | ❌     | ❌     | ❌  | ❌     | ✅     | ❌        | ❌        | ❌     |
| DataAggregationService     | ❌     | ✅     | ❌  | ❌     | ❌     | ❌        | ❌        | ❌     |
| BillingService             | ❌     | ❌     | ❌  | ❌     | ❌     | ❌        | ❌        | ✅     |

---

## 🌐 Network Topology (Kubernetes)

```yaml
# Namespace: okla
# Cluster: okla-cluster (DOKS)

EXTERNAL: ├── OpenAI API (api.openai.com)
  ├── Google Cloud (cloud.google.com)
  ├── Anthropic Claude (api.anthropic.com)
  ├── Hugging Face (api-inference.huggingface.co)
  ├── Cohere (api.cohere.ai)
  ├── Replicate (api.replicate.com)
  ├── AWS SageMaker (sagemaker.us-east-1.amazonaws.com)
  └── Stripe (api.stripe.com)

INTERNAL (K8s): ├── chatbotservice:8080 ─► OpenAI + Claude
  ├── reviewservice:8080 ─► OpenAI + HuggingFace + Claude
  ├── recommendationservice:8080 ─► OpenAI + Vertex
  ├── vehicleintelligenceservice:8080 ─► Vertex + SageMaker
  ├── leadscoringleservice:8080 ─► SageMaker
  ├── listinganalyticsservice:8080 ─► Replicate + Google Vision
  ├── datapiped ineservice:8080 ─► Vertex + Kafka
  ├── userbehaviorservice:8080 ─► HuggingFace + Cohere
  ├── notificationservice:8080 ─► Cohere
  ├── errorservice:8080 ─► Claude
  ├── gateway:8080 ─► (Ocelot, routea a todos)
  ├── postgres:5432 ─► (Datos principales)
  ├── redis:6379 ─► (Cache)
  ├── rabbitmq:5672 ─► (Message broker)
  └── kafka:9092 ─► (Event streaming)
```

---

## 🔐 API Keys Management (Kubernetes Secrets)

```bash
# Crear secrets para todas las APIs
kubectl create secret generic ai-api-keys \
  --from-literal=OPENAI_API_KEY=sk_... \
  --from-literal=GOOGLE_PROJECT_ID=okla-... \
  --from-literal=GOOGLE_CREDENTIALS=$(base64 gcp-key.json) \
  --from-literal=ANTHROPIC_API_KEY=sk-ant-... \
  --from-literal=REPLICATE_API_TOKEN=... \
  --from-literal=COHERE_API_KEY=... \
  --from-literal=HUGGING_FACE_API_KEY=... \
  --from-literal=AWS_ACCESS_KEY_ID=... \
  --from-literal=AWS_SECRET_ACCESS_KEY=... \
  --from-literal=STRIPE_API_KEY=sk_... \
  -n okla

# Verificar
kubectl get secrets -n okla
kubectl describe secret ai-api-keys -n okla
```

---

## 💰 Costo Total Mensual

```
OpenAI:
  - ChatGPT (ChatbotService): $50/mes
  - Embeddings (RecommendationService): $20/mes
  - Moderation (ReviewService): $10/mes
  → Subtotal: $80/mes

Google Vertex AI:
  - Predictions (VehicleIntelligence): $30/mes
  - Forecasting (DataPipeline): $20/mes
  - Embeddings (Ranking): $15/mes
  → Subtotal: $65/mes

AWS SageMaker:
  - Endpoint (LeadScoring): $51/mes (ml.t2.medium)
  - Training: $20/mes
  → Subtotal: $71/mes

Hugging Face: Gratis (open-source, self-hosted)
Replicate: $5/mes (OCR + Vision)
Cohere: $15/mes (text generation)
Anthropic Claude: $45/mes (advanced analysis)
Stripe Radar: Incluido en comisiones
Anthropic: $45/mes

TOTAL: $296/mes (Scaling tier)

Breakdown by Service:
  - ChatbotService: $80-100/mes
  - ReviewService: $50-70/mes
  - RecommendationService: $35-55/mes
  - VehicleIntelligenceService: $51-100/mes
  - LeadScoringService: $51-100/mes
  - ListingAnalyticsService: $5-20/mes
  - DataPipelineService: $50-100/mes
  - UserBehaviorService: $15-30/mes

R ECOMMENDACIÓN: Presupuestar $400-500/mes en Q1/Q2
```

---

## 📈 Roadmap de Implementación

### FASE 1: MVP (Semana 1-2) ✅

- [x] OpenAI ChatGPT (ChatbotService)
- [x] OpenAI Embeddings (RecommendationService)
- [x] Google Vertex AI (VehicleIntelligence)

### FASE 2: Optimización (Semana 3-4)

- [ ] Hugging Face NLP (ReviewService)
- [ ] AWS SageMaker (LeadScoring)
- [ ] Replicate OCR (ListingAnalytics)

### FASE 3: Premium Features (Semana 5-6)

- [ ] Anthropic Claude (advanced analysis)
- [ ] Cohere (text generation)
- [ ] Multi-model ensembles

### FASE 4: Production (Semana 7+)

- [ ] Monitoring y alertas
- [ ] Cost optimization
- [ ] A/B testing
- [ ] Custom model fine-tuning

---

## 🚨 Error Handling & Fallback Strategy

```csharp
public class AIServiceFallbackStrategy
{
    /// Recomendación Service Fallback
    public async Task<List<Vehicle>> GetRecommendationsAsync(
        string userPreferences)
    {
        try
        {
            // 1. Intenta OpenAI embeddings
            return await _openAIService.GetRecommendationsAsync(userPreferences);
        }
        catch
        {
            try
            {
                // 2. Fallback a Cohere
                return await _cohereService.GetRecommendationsAsync(userPreferences);
            }
            catch
            {
                // 3. Fallback a Hugging Face
                return await _huggingFaceService.GetRecommendationsAsync(userPreferences);
            }
        }
    }

    /// ChatBot Fallback
    public async Task<string> GetChatResponseAsync(string message)
    {
        try
        {
            // 1. Intenta Claude (mejor calidad)
            return await _claudeService.GetResponseAsync(message);
        }
        catch
        {
            try
            {
                // 2. Fallback a OpenAI
                return await _openAIService.GetResponseAsync(message);
            }
            catch
            {
                // 3. Fallback a Cohere
                return await _cohereService.GetResponseAsync(message);
            }
        }
    }

    /// Lead Scoring Fallback
    public async Task<LeadScore> ScoreLeadAsync(LeadData lead)
    {
        try
        {
            // 1. Intenta SageMaker (mejor precision)
            return await _sageMakerService.ScoreAsync(lead);
        }
        catch
        {
            // 2. Fallback a heurísticas simple
            return CalculateSimpleScore(lead);
        }
    }
}
```

---

## 📊 Monitoreo y Observabilidad

```yaml
# Prometheus metrics
prometheus_config:
  - openai_api_latency_ms
  - openai_tokens_used
  - openai_api_errors
  - vertex_ai_predictions_per_second
  - sagemaker_inference_latency
  - ai_api_costs_usd_daily

# Alertas
alert_rules:
  - IF openai_api_latency > 5000ms → PAGE
  - IF openai_api_errors_rate > 0.01 → ALERT
  - IF daily_ai_costs > $20 → WARN
  - IF any_api_unavailable → FALLBACK
```

---

## ✅ Checklist de Deployment

- [ ] Crear Kubernetes secrets para todos los API keys
- [ ] Configurar endpoints en cada servicio
- [ ] Testing unitario con mocks
- [ ] Testing de integración con APIs reales
- [ ] Load testing (ver cuántas requests pueden manejar)
- [ ] Cost monitoring y alertas
- [ ] Documentación de fallback flows
- [ ] Runbooks para incidentes
- [ ] Training del equipo
- [ ] Moniteo en producción

---

_Arquitectura de Integración OKLA - IA Completa_  
_Todos los 8 APIs integrados en 12 servicios_  
_Actualización: Enero 15, 2026_
