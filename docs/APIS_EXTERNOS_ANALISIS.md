# 📡 Análisis Exhaustivo de APIs Externos - OKLA Marketplace

**Análisis realizado:** Marzo 7, 2026  
**Basado en:** Análisis del código fuente del proyecto

---

## 📋 Tabla de Contenidos

1. [APIs de IA/LLM](#ias--llm)
2. [APIs de Pagos](#apis-de-pagos)
3. [APIs de Notificaciones](#apis-de-notificaciones)
4. [APIs de Datos Automotrices](#apis-de-datos-automotrices)
5. [APIs de Terceros Integrados](#apis-de-terceros-integrados)
6. [APIs Internas de Microservicios](#apis-internas-de-microservicios)
7. [Matriz de Uso por Servicio](#matriz-de-uso-por-servicio)

---

## 🤖 IAs & LLM

### 1. **Anthropic Claude API** ✅ **PRINCIPAL**

- **URL:** `https://api.anthropic.com`
- **Versión:** `2023-06-01`
- **Modelos utilizados:**
  - `claude-sonnet-4-5` (RecoAgent, DealerChatAgent, SupportAgent)
  - `claude-haiku-4.5` (SearchAgent - búsquedas rápidas)
- **Endpoints:**
  - `POST /v1/messages` - Generación de respuestas
  - `GET /v1/models` - Listado de modelos (optional)
- **Configuración:**
  ```json
  {
    "LlmService": {
      "ServerUrl": "https://api.anthropic.com",
      "ApiKey": "${LLM_API_KEY}",
      "ModelId": "claude-sonnet-4-5",
      "MaxTokens": 1024,
      "Temperature": 0.3,
      "TopP": 0.9,
      "TimeoutSeconds": 120
    }
  }
  ```
- **Servicios que lo usan:**
  - ✅ ChatbotService (DealerChatAgent)
  - ✅ RecoAgent (Recomendaciones personalizadas)
  - ✅ SearchAgent (Búsqueda de vehículos)
  - ✅ SupportAgent (Soporte 24/7)
- **Costo:** `$3 USD por millón de input tokens` / `$15 por millón de output tokens`
- **Rate Limits:** Requiere manejo de throttling en ConfigurationServiceClient
- **Código Fuente:**
  - [ChatbotService/ClaudeLlmService.cs](backend/ChatbotService/ChatbotService.Infrastructure/Services/ClaudeLlmService.cs)
  - [RecoAgent/ClaudeRecoService.cs](backend/RecoAgent/RecoAgent.Infrastructure/Services/ClaudeRecoService.cs)
  - [SearchAgent/ClaudeSearchService.cs](backend/SearchAgent/SearchAgent.Infrastructure/Services/ClaudeSearchService.cs)

### 2. **Google Vertex AI** ⚠️ **OPTIONAL**

- **URL:** `https://us-central1-aiplatform.googleapis.com`
- **Uso:** Alternativa a Claude para ciertos escenarios (fallback)
- **Modelos:** Gemini Pro, Gemini Vision
- **Estado:** Documentado pero no está en configuración principal
- **Nota:** Mencionado en documentación de ML

### 3. **OpenAI API** ⚠️ **FALLBACK**

- **URL:** `https://api.openai.com`
- **Uso potencial:** Embeddings, completions
- **Modelos:**
  - `gpt-4-turbo` (si se requiere)
  - `text-embedding-3-small` (para embeddings de vectores)
- **Estado:** Mencionado en `VectorSearchService.cs` pero Anthropic es preferido

### 4. **Hugging Face API**

- **URL:** `https://api-inference.huggingface.co`
- **Uso:** Modelos de ML alternativos
- **Estado:** Documentado, no implementado en código principal

### 5. **Cohere API**

- **URL:** `https://api.cohere.ai`
- **Uso:** Generación de texto alternativa
- **Estado:** Documentado como opción futura

---

## 💳 APIs de Pagos

### 1. **Stripe API** ✅ **PRINCIPAL BILLING**

- **URL:** `https://api.stripe.com`
- **Versión API:** Latest (2024-x-x)
- **Endpoints principales:**
  - `POST /v1/customers` - Crear cliente
  - `POST /v1/subscriptions` - Crear suscripción
  - `POST /v1/payment_intents` - Crear intención de pago
  - `POST /v1/invoices` - Crear factura
  - `POST /v1/webhook_endpoints` - Webhooks
- **Configuración:**
  ```json
  {
    "Stripe": {
      "SecretKey": "${STRIPE_SECRET_KEY}",
      "PublishableKey": "${STRIPE_PUBLISHABLE_KEY}",
      "WebhookSecret": "${STRIPE_WEBHOOK_SECRET}",
      "DefaultTrialDays": 14,
      "PriceIds": {
        "basic_monthly": "price_basic_monthly",
        "professional_monthly": "price_professional_monthly",
        "enterprise_monthly": "price_enterprise_monthly"
      }
    }
  }
  ```
- **Planes soportados:**
  - Libre: $0
  - Visible: $29/mes
  - Pro: $79/mes
  - Élite: $299/mes
- **Servicio:** BillingService
- **Webhook:** `POST /api/payment/stripe/webhook`
- **Código:** [BillingService/StripeService.cs](backend/BillingService/BillingService.Infrastructure/Services/StripeService.cs)
- **Documentación:** [Stripe API Reference](https://stripe.com/docs/api)

### 2. **AZUL Payments (Dominicana)** ✅ **PROCESADOR LOCAL RD**

- **URL:** `https://api.azul.com.do` (Test) / Producción TBD
- **Tipo:** Gateway de pago local para República Dominicana
- **Parámetros requeridos:**
  ```json
  {
    "Azul": {
      "MerchantId": "${AZUL_MERCHANT_ID}",
      "MerchantName": "OKLA Marketplace",
      "AuthKey": "${AZUL_AUTH_KEY}",
      "Auth1": "${AZUL_AUTH1}",
      "Auth2": "${AZUL_AUTH2}",
      "CurrencyCode": "214",
      "Environment": "Test|Production",
      "ApprovedUrl": "https://okla.com.do/api/payment/azul/callback/approved",
      "DeclinedUrl": "https://okla.com.do/api/payment/azul/callback/declined"
    }
  }
  ```
- **Métodos de pago:** Tarjetas de crédito (Visa, Mastercard, JCB, Amex)
- **Servicio:** BillingService
- **Implementación:** [AzulPaymentService.cs](backend/BillingService/BillingService.Infrastructure/Services/AzulPaymentService.cs)
- **Hash Generator:** MD5/HMAC-SHA256 para seguridad

---

## 📬 APIs de Notificaciones

### 1. **Resend Email API** ✅ **EMAIL PROVIDER**

- **URL:** `https://api.resend.com/emails`
- **Método:** `POST`
- **Configuración:**
  ```json
  {
    "NotificationSettings": {
      "Resend": {
        "ApiKey": "${RESEND_API_KEY}",
        "FromEmail": "noreply@okla.com.do",
        "FromName": "OKLA Marketplace"
      }
    }
  }
  ```
- **Uso:**
  - Confirmación de email
  - Password reset
  - Notificaciones de transacciones
  - Alertas de precio
- **Servicio:** NotificationService
- **Headers:**
  - `Authorization: Bearer {RESEND_API_KEY}`
  - `Content-Type: application/json`
- **Código:** [ResendEmailService.cs](backend/NotificationService/NotificationService.Infrastructure/External/ResendEmailService.cs)
- **Status:** ✅ Implementado, fallback a mock si API key no está configurada

### 2. **Meta WhatsApp Cloud API** ⚠️ **WHATSAPP BUSINESS**

- **URL:** `https://graph.facebook.com/v18.0`
- **Tipo:** Mensajería WhatsApp para Business
- **Endpoints:**
  - `POST /{phone_number_id}/messages` - Enviar mensaje
  - `POST /webhooks` - Webhook para mensajes entrantes
- **Configuración:**
  ```json
  {
    "WhatsApp": {
      "VerifyToken": "${WA_VERIFY_TOKEN}",
      "AccessToken": "${WA_ACCESS_TOKEN}",
      "PhoneNumberId": "${WA_PHONE_NUMBER_ID}",
      "ApiBaseUrl": "https://graph.facebook.com/v18.0",
      "AllowedCountryCodes": ["+1809", "+1829", "+1849"]
    }
  }
  ```
- **Servicio:** ChatbotService, NotificationService
- **Uso:**
  - Notificaciones de cambios de precio
  - Respuestas del DealerChatAgent en WhatsApp
  - Confirmación de listings
- **Código:** [WhatsAppService.cs](backend/ChatbotService/ChatbotService.Infrastructure/Services/WhatsAppService.cs)
- **Rate Limit:** Configurable (default: 10 msg/min)

### 3. **Slack API** ⚠️ **INTERNAL ALERTS**

- **URL:** `https://hooks.slack.com/services/{webhook_id}`
- **Tipo:** Webhooks para notificaciones de errores críticos
- **Servicio:** NotificationService (SlackProvider)
- **Uso:** Alertas de fallos en sistema, eventos críticos

### 4. **Microsoft Teams API** ⚠️ **INTERNAL ALERTS**

- **URL:** `https://outlook.webhook.office.com/webhookb2/...`
- **Tipo:** Webhooks para notificaciones de equipo
- **Servicio:** NotificationService (TeamsProvider)
- **Código:** [TeamsProvider.cs](backend/NotificationService/NotificationService.Infrastructure/Providers/TeamsProvider.cs)

---

## 🚗 APIs de Datos Automotrices

### 1. **NHTSA vPIC (Vehicle Product Information Catalog)** ✅ **HISTORIAL VIN**

- **URL:** `https://vpic.nhtsa.dot.gov`
- **Endpoints:**
  - `GET /api/vehicles/DecodeVin/{vin}` - Decodificar VIN
  - `GET /api/vehicles/DecodeVin/{vin}?year={year}` - Con año
  - `GET /api/vehicles/DecodeVinValues/{vin}` - Detalles completos
- **Uso:** Parte del **D1 Scoring (25%)** del OKLA Score
  - Marca, modelo, año del vehículo
  - Motor, transmisión, equipamiento
- **Rate Limit:** Sin límite, libre
- **Servicio:** VehiclesSaleService
- **Configuración:**
  ```csharp
  builder.Services.AddHttpClient("NHTSA", client =>
  {
      client.BaseAddress = new Uri("https://vpic.nhtsa.dot.gov");
  });
  ```
- **Código:** [VehiclesSaleService/Program.cs](backend/VehiclesSaleService/VehiclesSaleService.Api/Program.cs)
- **Precio:** Gratuito (API pública)

### 2. **CARFAX / AutoCheck** ⚠️ **HISTORIAL VEHICULAR**

- **URL:** `https://api.carfax.com` (requiere API key)
- **Uso:** Historial de accidentes, cambios de dueño (D1 Scoring)
- **Estado:** ⚠️ Integración pendiente (mencionada en documentación)
- **Costo:** ~$2-5 USD por reporte

### 3. **VinAudit API** ⚠️ **ALTERNATIVE VIN HISTORY**

- **URL:** `https://vinaudit.com/api`
- **Uso:** Alternativa a CARFAX para historial de VIN
- **Estado:** ⚠️ Integración pendiente
- **Endpoints:** `/report/{vin}`, `/check/{vin}`

### 4. **MarketCheck / AutoTrader API** ⚠️ **PRECIOS DE MERCADO**

- **URL:** `https://marketcheck-prod.apigee.net` o `https://api.autotrader.com`
- **Uso:** **D4 Scoring (17%)** - Comparar precio con mercado
- **Endpoints:** Búsqueda de listados, precios promedio por marca/modelo
- **Estado:** ⚠️ Integración pendiente
- **Autenticación:** API Key

### 5. **BCRD (Banco Central República Dominicana)** ✅ **TIPO DE CAMBIO**

- **URL:** `https://api.bcrd.do` o `https://bcrd.finanzas.gob.do`
- **Uso:** Obtener tipo de cambio USD/DOP para cálculos de precio
- **Endpoints:** `GET /tipo_cambio/{date}`
- **Frecuencia:** Diaria (actualización cada 9:00 AM RD)
- **Costo:** Gratuito (API pública)
- **Servicio:** VehiclesSaleService, BillingService
- **Nota:** Crítico para OKLA Score (D4 utiliza BCRD)

### 6. **Edmunds API** ⚠️ **ESPECIFICACIONES TÉCNICAS**

- **URL:** `https://api.edmunds.com`
- **Uso:** Especificaciones técnicas completas (motor, transmisión, consumo)
- **Estado:** Documentado, integración pendiente

---

## 🔗 APIs de Terceros Integrados

### 1. **Google reCAPTCHA v3** ✅ **ANTI-SPAM**

- **URL:** `https://www.google.com/recaptcha/api/siteverify`
- **Método:** `POST`
- **Uso:** Prevenir bots en registro, login
- **Configuración:**
  ```json
  {
    "ReCaptcha": {
      "SecretKey": "${RECAPTCHA_SECRET_KEY}",
      "SiteKey": "${RECAPTCHA_SITE_KEY}",
      "Threshold": 0.5
    }
  }
  ```
- **Servicio:** AuthService, Frontend (Next.js)
- **Interfaz:** [ICaptchaService.cs](backend/AuthService/AuthService.Domain/Interfaces/Services/ICaptchaService.cs)

### 2. **IP-API / GeoIP Geolocation** ✅ **GEOLOCALIZACIÓN**

- **URL:** `https://ip-api.com` o `https://ipapi.co`
- **Uso:** Detectar país/región del usuario, IP geolocation
- **Servicio:** AuthService (IpApiGeoLocationService)
- **Configuración:**
  ```csharp
  builder.Services.AddHttpClient<IGeoLocationService, IpApiGeoLocationService>();
  ```
- **Código:** [IpApiGeoLocationService.cs](backend/AuthService/AuthService.Infrastructure/Services/IpApiGeoLocationService.cs)
- **Precio:** Gratuito (hasta 45 req/min)

### 3. **Google OAuth 2.0** ✅ **EXTERNAL AUTH**

- **URL:** `https://accounts.google.com/o/oauth2/v2/auth`
- **Token Endpoint:** `https://oauth2.googleapis.com/token`
- **Uso:** Login con Google para buyers/dealers
- **Scopes:** `openid email profile`
- **Servicio:** AuthService (ExternalAuthController)
- **Configuración:**
  ```json
  {
    "GoogleAuth": {
      "ClientId": "${GOOGLE_CLIENT_ID}",
      "ClientSecret": "${GOOGLE_CLIENT_SECRET}",
      "RedirectUri": "https://okla.com.do/api/auth/external/callback"
    }
  }
  ```

### 4. **Facebook OAuth 2.0** ✅ **EXTERNAL AUTH**

- **URL:** `https://www.facebook.com/v18.0/dialog/oauth`
- **Token Endpoint:** `https://graph.facebook.com/v18.0/oauth/access_token`
- **Uso:** Login con Facebook
- **Scopes:** `email public_profile`
- **Servicio:** AuthService

### 5. **Microsoft/Azure OAuth 2.0** ✅ **EXTERNAL AUTH**

- **URL:** `https://login.microsoftonline.com/common/oauth2/v2.0/authorize`
- **Uso:** Login con Microsoft / Office 365
- **Servicio:** AuthService

### 6. **Apple Sign In** ✅ **EXTERNAL AUTH**

- **URL:** `https://appleid.apple.com/auth/authorize`
- **Uso:** Login con Apple ID
- **Servicio:** AuthService

---

## 🔄 APIs Internas de Microservicios

### Mapa de Clientes Inter-Servicios

| Servicio Llamador   | Servicio Destino        | Interfaz                   | URL Puerto |
| ------------------- | ----------------------- | -------------------------- | ---------- |
| ChatbotService      | VehiclesSaleService     | IVehicleServiceClient      | :8080      |
| ChatbotService      | NotificationService     | INotificationServiceClient | :8080      |
| VehiclesSaleService | KYCService              | IDealerVerificationClient  | :8080      |
| VehiclesSaleService | ErrorService            | IErrorServiceClient        | :8080      |
| VehiclesSaleService | AlertService            | AlertService (named)       | :8080      |
| AdminService        | AuthService             | IAuthServiceClient         | :8080      |
| AdminService        | VehiclesSaleService     | IVehicleServiceClient      | :8080      |
| AdminService        | DealerManagementService | IDealerService             | :8080      |
| AdminService        | AuditService            | IAuditServiceClient        | :8080      |
| AdminService        | NotificationService     | INotificationServiceClient | :8080      |
| UserService         | RoleService             | IRoleServiceClient         | :8080      |
| UserService         | NotificationService     | INotificationServiceClient | :8080      |
| UserService         | ErrorService            | IErrorServiceClient        | :8080      |
| UserService         | VehiclesSaleService     | IVehiclesSaleServiceClient | :8080      |
| BillingService      | UserService             | IUserServiceClient         | :8080      |
| BillingService      | AuditService            | IAuditServiceClient        | :8080      |
| BillingService      | ErrorService            | IErrorServiceClient        | :8080      |
| NotificationService | AuditService            | IAuditServiceClient        | :8080      |
| KYCService          | MediaService            | IMediaServiceClient        | :8080      |
| KYCService          | IdempotencyService      | IIdempotencyServiceClient  | :8080      |

### Configuración Estándar

Todos los clientes inter-servicios utilizan:

- **Base de resilencia:** `AddStandardResilience()` (Polly)
  - Reintentos: 3 con backoff exponencial
  - Circuit Breaker: 5 fallos → abre por 30s
  - Timeout: 30 segundos (variable por servicio)
- **Autenticación:** X-Service-Name header
- **Formato:** Content-Type: application/json
- **Serialización:** System.Text.Json con camelCase

---

## 📊 Matriz de Uso por Servicio

### ChatbotService

```
✅ Anthropic Claude API (DealerChatAgent)
✅ Meta WhatsApp API (notificaciones/chat)
✅ VehiclesSaleService (inter-service)
✅ NotificationService (inter-service)
```

### BillingService

```
✅ Stripe API (pagos, suscripciones)
✅ AZUL Payments (procesador local RD)
✅ UserService (inter-service)
✅ AuditService (inter-service)
✅ ErrorService (inter-service)
```

### AuthService

```
✅ Google reCAPTCHA (anti-spam)
✅ IP-API Geolocation (geo-detection)
✅ Google OAuth 2.0 (external auth)
✅ Facebook OAuth 2.0 (external auth)
✅ Microsoft OAuth 2.0 (external auth)
✅ Apple Sign In (external auth)
```

### VehiclesSaleService

```
✅ NHTSA vPIC (historial VIN - D1 Scoring)
✅ BCRD Tipo de Cambio (precios - D4 Scoring)
⚠️ CARFAX / VinAudit (historial - pendiente)
⚠️ MarketCheck (precios mercado - pendiente)
✅ KYCService (inter-service)
✅ AlertService (inter-service)
✅ ErrorService (inter-service)
```

### NotificationService

```
✅ Resend Email API (emails)
✅ Meta WhatsApp API (WhatsApp)
⚠️ Slack API (alerts internas)
⚠️ Teams API (alerts internas)
✅ AuditService (inter-service)
```

### KYCService

```
✅ MediaService (inter-service - URLs pre-firmadas)
✅ IdempotencyService (inter-service)
⚠️ Stripe (posible integración para KYC verificado)
```

### AdminService

```
✅ AuthService (inter-service)
✅ VehiclesSaleService (inter-service)
✅ DealerManagementService (inter-service)
✅ AuditService (inter-service)
✅ NotificationService (inter-service)
```

---

## 🔐 Seguridad & Secrets Management

### Ubicaciones de API Keys

| API          | Almacenamiento   | Variable de Entorno       | Kubernetes Secret          |
| ------------ | ---------------- | ------------------------- | -------------------------- |
| Anthropic    | appsettings.json | `LLM_API_KEY`             | chatbotservice-secret      |
| Stripe       | appsettings.json | `STRIPE_SECRET_KEY`       | billingservice-secret      |
| AZUL         | appsettings.json | `AZUL_AUTH_KEY`           | billingservice-secret      |
| Resend       | appsettings.json | `RESEND_API_KEY`          | notificationservice-secret |
| WhatsApp     | appsettings.json | `WA_ACCESS_TOKEN`         | chatbotservice-secret      |
| Google OAuth | appsettings.json | `GOOGLE_CLIENT_ID/SECRET` | authservice-secret         |
| reCAPTCHA    | appsettings.json | `RECAPTCHA_SECRET_KEY`    | authservice-secret         |

### Mejores Prácticas Implementadas

✅ **Nunca hardcodear secrets** - Todos están vacíos en appsettings.json
✅ **Docker Secrets** - `/run/secrets/` en K8s
✅ **Environment Variables** - Inyección en deployments
✅ **Configuration Service** - Configuración dinámica desde admin panel
✅ **Audit Logging** - Todas las llamadas a APIs externas logged en AuditService
✅ **Rate Limiting** - Por API key y endpoint

---

## 📈 Resumen de Costos Mensuales Estimados

| API                | Costo Estimado        | Base                   |
| ------------------ | --------------------- | ---------------------- |
| Anthropic Claude   | $2,000-5,000          | ~1-2B input tokens/mes |
| Stripe             | $300-800              | 2-5% of GMV            |
| Resend Email       | $20-100               | 1,000-5,000 emails/mes |
| NHTSA vPIC         | $0                    | Gratuito               |
| reCAPTCHA v3       | $0                    | Gratuito (10k gratis)  |
| IP-API             | $0                    | Gratuito               |
| WhatsApp           | Variable              | Envío de mensajes      |
| **TOTAL ESTIMADO** | **~$2,500-6,000/mes** |                        |

---

## 🚀 Próximos Pasos Recomendados

1. **Integrar CARFAX/VinAudit** → Mejorar D1 Scoring
2. **Integrar MarketCheck** → Mejorar D4 Scoring (precios)
3. **Implementar caching distribuido** → Redis para resultados de APIs
4. **Agregar fallbacks** → Si API externa cae, usar datos cached
5. **Monitoreo de quota** → Alertas cuando se aproxima limite de APIs
6. **Rate limiting mejorado** → Protección contra abuse de APIs costosos

---

**Documento generado:** 2026-03-07  
**Última actualización de APIs:** Detectado en Program.cs y appsettings.json  
**Confidencial:** Contiene referencias a endpoints reales
