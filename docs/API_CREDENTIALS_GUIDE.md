# 🔑 OKLA Platform — API & Credentials Guide

> **Purpose:** Lists every external API and service integration used by the OKLA platform, where to configure credentials, and their current status.
>
> **Generated:** 2026-03-05  
> **Last Audited:** 2026-03-05

---

## 📋 Quick Reference Table

| #   | Service                 | Used By               | Status            | K8s Secret                  | Config File Path                                                                    |
| --- | ----------------------- | --------------------- | ----------------- | --------------------------- | ----------------------------------------------------------------------------------- |
| 1   | AWS S3 / DO Spaces      | MediaService          | ✅ Active         | `external-services-secrets` | `backend/MediaService/MediaService.Api/appsettings.json` → `Storage.S3`             |
| 2   | Stripe (Payments)       | BillingService        | ✅ Active         | `external-services-secrets` | `k8s/deployments.yaml` → billingservice env vars                                    |
| 3   | Resend (Email)          | NotificationService   | ✅ Active         | `external-services-secrets` | `backend/NotificationService/NotificationService.Api/appsettings.Production.json`   |
| 4   | SendGrid (Email)        | NotificationService   | ⚠️ Backup         | `external-services-secrets` | `backend/NotificationService/NotificationService.Api/appsettings.Development.json`  |
| 5   | Twilio (SMS)            | NotificationService   | ❌ Not configured | —                           | `backend/NotificationService/NotificationService.Api/appsettings.Development.json`  |
| 6   | Anthropic Claude (AI)   | ChatbotService        | ✅ Active         | `chatbotservice-anthropic`  | `k8s/deployments.yaml` → chatbotservice env                                         |
| 7   | Google OAuth            | AuthService           | ❌ Not configured | —                           | `backend/AuthService/AuthService.Api/appsettings.json` → `Authentication.Google`    |
| 8   | Facebook OAuth          | AuthService           | ❌ Not configured | —                           | `backend/AuthService/AuthService.Api/appsettings.json` → `Authentication.Facebook`  |
| 9   | Microsoft OAuth         | AuthService           | ❌ Not configured | —                           | `backend/AuthService/AuthService.Api/appsettings.json` → `Authentication.Microsoft` |
| 10  | Google reCAPTCHA v3     | AuthService           | ❌ Not configured | —                           | `backend/AuthService/AuthService.Api/appsettings.json` → `ReCaptcha`                |
| 11  | Google Maps             | Frontend              | ❌ Not configured | —                           | `frontend/web-next/.env.local` → `NEXT_PUBLIC_GOOGLE_MAPS_KEY`                      |
| 12  | Google Analytics        | Frontend              | ❌ Not configured | —                           | `frontend/web-next/.env.local` → `NEXT_PUBLIC_GA_TRACKING_ID`                       |
| 13  | Sentry (Error tracking) | Frontend              | ❌ Not configured | —                           | `frontend/web-next/.env.local` → `NEXT_PUBLIC_SENTRY_DSN`                           |
| 14  | VinAudit (VIN decode)   | Frontend/VehicleScore | ❌ Not configured | —                           | `frontend/web-next/.env.local` → `VINAUDIT_API_KEY`                                 |
| 15  | Marketcheck (Pricing)   | Frontend/VehicleScore | ❌ Not configured | —                           | `frontend/web-next/.env.local` → `MARKETCHECK_API_KEY`                              |
| 16  | KBB (Blue Book)         | Frontend/VehicleScore | ❌ Not configured | —                           | `frontend/web-next/.env.local` → `KBB_API_KEY`                                      |
| 17  | CarFax (History)        | Frontend/VehicleScore | ❌ Not configured | —                           | `frontend/web-next/.env.local` → `CARFAX_API_KEY`                                   |
| 18  | NHTSA (Recalls/Safety)  | VehiclesSaleService   | ✅ Free API       | —                           | No key needed                                                                       |
| 19  | PostgreSQL (DO Managed) | All Services          | ✅ Active         | Per-service `*-db-secret`   | `k8s/deployments.yaml`                                                              |
| 20  | RabbitMQ                | All Services          | ✅ Active         | `rabbitmq-secrets`          | `k8s/deployments.yaml`                                                              |
| 21  | Redis                   | Gateway, AuthService  | ✅ Active         | `redis-secrets`             | `k8s/deployments.yaml`                                                              |
| 22  | JWT Signing             | AuthService/Gateway   | ✅ Active         | `jwt-secrets`               | `k8s/deployments.yaml`                                                              |

---

## 1. AWS S3 / DigitalOcean Spaces (Image Storage)

**Service:** MediaService  
**Purpose:** Store vehicle images, thumbnails, and processed media

### Where to Configure

| Environment    | File                                                     | Keys                                                                                                                           |
| -------------- | -------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| Development    | `backend/MediaService/MediaService.Api/appsettings.json` | `Storage.S3.AccessKey`, `Storage.S3.SecretKey`                                                                                 |
| Production     | K8s Secret `external-services-secrets`                   | `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`                                                                                   |
| K8s Deployment | `k8s/deployments.yaml` → `mediaservice`                  | `Storage__Provider=s3`, `Storage__S3__BucketName`, `Storage__S3__Region`, `Storage__S3__ServiceUrl`, `Storage__S3__CdnBaseUrl` |

### Current Values

- **Provider:** `s3` (set via K8s env var)
- **Bucket:** `okla-images-2026`
- **Region:** `us-east-2` (AWS Ohio)
- **ServiceUrl:** Not set (defaults to AWS S3)
- **CdnBaseUrl:** Not set

### To Migrate to DO Spaces

1. Create a Space in DO Console (e.g., `nyc3` region)
2. Generate Spaces access keys in DO Console → API → Spaces Keys
3. Update K8s secret:

```bash
kubectl create secret generic external-services-secrets \
  --from-literal=AWS_ACCESS_KEY_ID=<DO_SPACES_ACCESS_KEY> \
  --from-literal=AWS_SECRET_ACCESS_KEY=<DO_SPACES_SECRET_KEY> \
  ... (keep other keys) \
  -n okla --dry-run=client -o yaml | kubectl apply -f -
```

4. Update deployment env vars:

```yaml
Storage__S3__ServiceUrl: "https://nyc3.digitaloceanspaces.com"
Storage__S3__CdnBaseUrl: "https://okla-images.nyc3.cdn.digitaloceanspaces.com"
Storage__S3__BucketName: "okla-images"
Storage__S3__Region: "nyc3"
```

---

## 2. Stripe (Payments)

**Service:** BillingService  
**Purpose:** Process seller listing payments and dealer subscriptions

### Where to Configure

| Environment    | File                                      | Keys                                                                   |
| -------------- | ----------------------------------------- | ---------------------------------------------------------------------- |
| Production     | K8s Secret `external-services-secrets`    | `STRIPE_SECRET_KEY`, `STRIPE_PUBLISHABLE_KEY`, `STRIPE_WEBHOOK_SECRET` |
| K8s Deployment | `k8s/deployments.yaml` → `billingservice` | `Stripe__SecretKey`, `Stripe__PublishableKey`, `Stripe__WebhookSecret` |
| Frontend       | `frontend/web-next/.env.local`            | Currently not exposed (uses BFF pattern through BillingService API)    |

### Notes

- Stripe keys are present in K8s secrets ✅
- Webhook endpoint: `https://okla.com.do/api/billing/webhook`
- Test mode vs Live mode is determined by the key prefix (`sk_test_` vs `sk_live_`)

---

## 3. Resend (Email Delivery)

**Service:** NotificationService  
**Purpose:** Transactional emails (welcome, password reset, listing notifications)

### Where to Configure

| Environment    | File                                                                               | Keys                                                                           |
| -------------- | ---------------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| Production     | K8s Secret `external-services-secrets`                                             | `RESEND_API_KEY`, `RESEND_FROM_EMAIL`, `RESEND_FROM_NAME`                      |
| K8s Deployment | `k8s/deployments.yaml` → `notificationservice`                                     | `Email__Resend__ApiKey`, `Email__Resend__FromEmail`, `Email__Resend__FromName` |
| Development    | `backend/NotificationService/NotificationService.Api/appsettings.Development.json` | `Resend.ApiKey`, `Resend.FromEmail`, `Resend.FromName`                         |

### Notes

- Resend API key is present in K8s secrets ✅
- From email: configured in secret
- Dashboard: https://resend.com/domains

---

## 4. SendGrid (Email — Backup)

**Service:** NotificationService  
**Purpose:** Backup email provider

### Where to Configure

| Environment | File                                                                               | Keys                                                         |
| ----------- | ---------------------------------------------------------------------------------- | ------------------------------------------------------------ |
| Production  | K8s Secret `external-services-secrets`                                             | `SENDGRID_API_KEY`                                           |
| Development | `backend/NotificationService/NotificationService.Api/appsettings.Development.json` | `SendGrid.ApiKey`, `SendGrid.FromEmail`, `SendGrid.FromName` |

---

## 5. Twilio (SMS & WhatsApp)

**Service:** NotificationService  
**Purpose:** SMS verification codes, appointment reminders, WhatsApp notifications

### Where to Configure

| Environment | File                                                                               | Keys                                                            |
| ----------- | ---------------------------------------------------------------------------------- | --------------------------------------------------------------- |
| Development | `backend/NotificationService/NotificationService.Api/appsettings.Development.json` | `Twilio.AccountSid`, `Twilio.AuthToken`, `Twilio.FromNumber`    |
| Production  | K8s Secret (needs creation)                                                        | `TWILIO_ACCOUNT_SID`, `TWILIO_AUTH_TOKEN`, `TWILIO_FROM_NUMBER` |

### ⚠️ Status: NOT CONFIGURED

Twilio credentials are not present in K8s secrets. SMS/WhatsApp features will not work until configured.

### Setup Steps

1. Create Twilio account at https://www.twilio.com
2. Get Account SID and Auth Token from Console
3. Buy a phone number with SMS capability
4. Create K8s secret or add to `external-services-secrets`

---

## 6. Anthropic Claude (AI Chatbot)

**Service:** ChatbotService  
**Purpose:** AI-powered customer support chatbot in Spanish

### Where to Configure

| Environment    | File                                      | Keys                              |
| -------------- | ----------------------------------------- | --------------------------------- |
| Production     | K8s Secret `chatbotservice-anthropic`     | `api-key`                         |
| K8s Deployment | `k8s/deployments.yaml` → `chatbotservice` | `Anthropic__ApiKey` (from secret) |

### Notes

- API key is present in K8s secrets ✅
- Model: Claude 3.5 Sonnet (configured in chatbotservice code)

---

## 7-9. Social OAuth (Google, Facebook, Microsoft)

**Service:** AuthService  
**Purpose:** Social login / "Sign in with Google/Facebook/Microsoft"

### Where to Configure

| Provider  | File                                                   | Keys                                                                                                              |
| --------- | ------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------- |
| Google    | `backend/AuthService/AuthService.Api/appsettings.json` | `Authentication.Google.ClientId`, `Authentication.Google.ClientSecret`                                            |
| Facebook  | `backend/AuthService/AuthService.Api/appsettings.json` | `Authentication.Facebook.ClientId`, `Authentication.Facebook.ClientSecret`                                        |
| Microsoft | `backend/AuthService/AuthService.Api/appsettings.json` | `Authentication.Microsoft.ClientId`, `Authentication.Microsoft.ClientSecret`, `Authentication.Microsoft.TenantId` |

### ⚠️ Status: NOT CONFIGURED

All social OAuth providers have empty credentials. Social login buttons exist in the UI but will not work.

### Setup Steps (Google example)

1. Go to https://console.cloud.google.com → APIs & Services → Credentials
2. Create OAuth 2.0 Client ID
3. Set redirect URI: `https://okla.com.do/api/auth/callback/google`
4. Add credentials to K8s secret or appsettings

---

## 10. Google reCAPTCHA v3

**Service:** AuthService  
**Purpose:** Bot protection on registration, login, and password reset

### Where to Configure

| Environment | File                                                   | Keys                                       |
| ----------- | ------------------------------------------------------ | ------------------------------------------ |
| Backend     | `backend/AuthService/AuthService.Api/appsettings.json` | `ReCaptcha.SecretKey`, `ReCaptcha.SiteKey` |
| Frontend    | `frontend/web-next/.env.local`                         | `NEXT_PUBLIC_RECAPTCHA_SITE_KEY`           |

### ⚠️ Status: NOT CONFIGURED

reCAPTCHA is enabled in config (`ReCaptcha.Enabled: true`) but keys are placeholders (`${RECAPTCHA_SECRET_KEY}`).

### Setup Steps

1. Go to https://www.google.com/recaptcha/admin
2. Register site for reCAPTCHA v3
3. Add domain: `okla.com.do`
4. Set Site Key in frontend `.env.local` and Secret Key in backend appsettings

---

## 11-13. Frontend Analytics & Monitoring

### Google Maps

- **File:** `frontend/web-next/.env.local` → `NEXT_PUBLIC_GOOGLE_MAPS_KEY`
- **Purpose:** Vehicle location map, dealer map
- **Setup:** https://console.cloud.google.com → Maps JavaScript API

### Google Analytics (GA4)

- **File:** `frontend/web-next/.env.local` → `NEXT_PUBLIC_GA_TRACKING_ID`
- **Purpose:** User analytics, conversion tracking
- **Setup:** https://analytics.google.com → Create property → Get Measurement ID (`G-XXXXXXX`)

### Sentry (Error Tracking)

- **File:** `frontend/web-next/.env.local` → `NEXT_PUBLIC_SENTRY_DSN`
- **Purpose:** Frontend error tracking and monitoring
- **Setup:** https://sentry.io → Create project → Get DSN

---

## 14-17. Vehicle Scoring APIs (Stage 2+)

These APIs are needed for the OKLA Score system (currently Stage 1 — free APIs only).

| API                    | File                                                   | Key                             | Purpose            | Cost |
| ---------------------- | ------------------------------------------------------ | ------------------------------- | ------------------ | ---- |
| VinAudit               | `frontend/web-next/.env.local` → `VINAUDIT_API_KEY`    | VIN decode, specs, market value | ~$0.05/query       |
| Marketcheck            | `frontend/web-next/.env.local` → `MARKETCHECK_API_KEY` | Market pricing data             | ~$0.10/query       |
| KBB (Kelley Blue Book) | `frontend/web-next/.env.local` → `KBB_API_KEY`         | Blue book values                | Enterprise pricing |
| CarFax                 | `frontend/web-next/.env.local` → `CARFAX_API_KEY`      | Vehicle history reports         | Enterprise pricing |

### Notes

- Stage 1 uses only free APIs (NHTSA) — no keys needed
- Stage 2 activates paid VIN decode and market pricing
- Stage 3+ adds CarFax/KBB for premium reports

---

## 18. NHTSA (Free — No Key Required)

**Purpose:** Vehicle recalls, safety ratings, VIN basic decode  
**Endpoint:** `https://vpic.nhtsa.dot.gov/api/`  
**Status:** ✅ Free public API, no key needed

---

## 19-22. Infrastructure Services

### PostgreSQL (DO Managed Database)

- **K8s Secrets:** Individual per service (`authservice-db-secret`, `vehiclessaleservice-db-secret`, etc.)
- **Host:** `okla-db-do-user-31493168-0.g.db.ondigitalocean.com:25060`
- **Config:** Each service reads `Database__ConnectionStrings__PostgreSQL` from its K8s secret
- **Managed in:** DO Console → Databases

### RabbitMQ

- **K8s Secret:** `rabbitmq-secrets`
- **Config:** `k8s/deployments.yaml` → each service's `RabbitMQ__*` env vars
- **Internal host:** `rabbitmq:5672`

### Redis

- **K8s Secret:** `redis-secrets`
- **Config:** `k8s/deployments.yaml` → `Cache__RedisConnectionString`
- **Internal host:** `redis:6379`

### JWT Signing Keys

- **K8s Secret:** `jwt-secrets`
- **Used by:** AuthService (signs tokens), Gateway (validates tokens)
- **Config:** `Jwt__Secret`, `Jwt__Issuer`, `Jwt__Audience`

---

## 🔐 K8s Secrets Management

### View existing secret keys

```bash
kubectl get secret <secret-name> -n okla -o jsonpath='{.data}' | python3 -c "import sys,json; d=json.load(sys.stdin); [print(k) for k in sorted(d.keys())]"
```

### Create/update a secret

```bash
kubectl create secret generic <secret-name> \
  --from-literal=KEY=VALUE \
  -n okla --dry-run=client -o yaml | kubectl apply -f -
```

### Restart service after secret update

```bash
kubectl rollout restart deployment/<service-name> -n okla
```

---

## 📊 Priority Configuration Order

For a fully functional platform, configure credentials in this order:

1. ✅ **PostgreSQL** — Already configured (DO Managed)
2. ✅ **RabbitMQ** — Already configured (in-cluster)
3. ✅ **Redis** — Already configured (in-cluster)
4. ✅ **JWT** — Already configured
5. ✅ **AWS S3** — Already configured (images working)
6. ✅ **Stripe** — Already configured (payments)
7. ✅ **Resend** — Already configured (emails)
8. ✅ **Anthropic Claude** — Already configured (chatbot)
9. 🔶 **Google reCAPTCHA** — HIGH priority (bot protection)
10. 🔶 **Google OAuth** — HIGH priority (social login)
11. 🔶 **Twilio** — MEDIUM priority (SMS verification)
12. 🔶 **Google Analytics** — MEDIUM priority (usage metrics)
13. 🔶 **Sentry** — MEDIUM priority (error monitoring)
14. 🔶 **Google Maps** — LOW priority (vehicle location)
15. ⏳ **VinAudit/Marketcheck** — Stage 2 (paid scoring APIs)
16. ⏳ **KBB/CarFax** — Stage 3+ (premium vehicle reports)

---

_Document generated on 2026-03-05 during platform infrastructure audit._
