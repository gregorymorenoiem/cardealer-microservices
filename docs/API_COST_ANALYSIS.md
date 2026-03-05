# 💰 OKLA Platform — API Integration Cost Analysis

> **Generated:** 2026-03-05  
> **Scope:** All external API integrations identified in the codebase  
> **Currency:** USD  
> **Assumptions:** Early-stage marketplace in Dominican Republic, ~500 listings/month, ~5,000 MAU, ~50 dealers

---

## Cost Summary

| API                           | Purpose                      | Pricing Model              | Est. Monthly Cost | Status            |
| ----------------------------- | ---------------------------- | -------------------------- | ----------------- | ----------------- |
| DigitalOcean Spaces           | Image/media storage + CDN    | $5/mo + transfer           | $7–15             | ✅ Ready (AWS)    |
| Stripe                        | Payment processing           | 2.9% + $0.30/txn           | $15–50            | ✅ Active         |
| Azul (Banco Popular)          | Dominican payment gateway    | ~3.5% per txn              | $10–35            | 🟡 Planned        |
| Resend                        | Transactional email          | Free tier 3k/mo            | $0 (free tier)    | ✅ Active         |
| SendGrid                      | Backup email provider        | Free tier 100/day          | $0 (free tier)    | ⚠️ Backup         |
| Twilio (SMS)                  | SMS verification & alerts    | $0.0079/SMS (DR)           | $5–20             | ❌ Not configured |
| Twilio (WhatsApp)             | WhatsApp notifications       | $0.0042/msg (DR)           | $5–15             | ❌ Not configured |
| Anthropic Claude (Haiku 4.5)  | Search agent, Support agent  | $0.80/MTok in, $4/MTok out | $10–40            | ✅ Active         |
| Anthropic Claude (Sonnet 4.5) | Chatbot, Reco agent          | $3/MTok in, $15/MTok out   | $25–100           | ✅ Active         |
| Firebase (FCM)                | Push notifications           | Free (unlimited)           | $0                | 🟡 Configured     |
| Google Maps                   | Vehicle/dealer location maps | $7/1000 loads              | $0–10             | ❌ Not configured |
| Google OAuth                  | Social login                 | Free                       | $0                | ❌ Not configured |
| Google reCAPTCHA v3           | Bot protection               | Free (1M/mo)               | $0                | ❌ Not configured |
| Google Analytics (GA4)        | User analytics               | Free                       | $0                | ❌ Not configured |
| Sentry                        | Frontend error tracking      | Free tier 5k events        | $0 (free tier)    | ❌ Not configured |
| NHTSA                         | Vehicle recalls/VIN decode   | Free (public API)          | $0                | ✅ Active         |
| DO Managed PostgreSQL         | Database                     | $15/mo (basic)             | $15–60            | ✅ Active         |
| Redis (in-cluster)            | Caching, rate limiting       | Included in K8s            | $0                | ✅ Active         |
| RabbitMQ (in-cluster)         | Message broker               | Included in K8s            | $0                | ✅ Active         |
| DO Kubernetes (DOKS)          | Container orchestration      | $12/mo/node                | $36–72            | ✅ Active         |
| GHCR                          | Container registry           | Free (public repos)        | $0                | ✅ Active         |
| **TOTAL ESTIMATE**            |                              |                            | **$128–417/mo**   |                   |

---

## Detailed API Breakdown

### 1. DigitalOcean Spaces (S3-compatible Storage)

**Service:** MediaService  
**Purpose:** Vehicle images, thumbnails, processed media  
**Credentials:** `k8s/secrets.template.yaml` → `external-services-secrets` (`AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`)  
**Config:** `backend/MediaService/MediaService.Api/appsettings.json` → `Storage.S3`

**Current Setup:** AWS S3 bucket `okla-images-2026` in `us-east-2`. Ready to migrate to DO Spaces.

| Component          | DO Spaces Pricing              | Est. Monthly |
| ------------------ | ------------------------------ | ------------ |
| Storage            | $5/mo for 250 GB included      | $5           |
| Additional storage | $0.02/GB beyond 250 GB         | $0–5         |
| Outbound transfer  | 1 TB included, $0.01/GB beyond | $0–5         |
| CDN                | Included with Spaces CDN       | $0           |
| **Subtotal**       |                                | **$5–15/mo** |

**Notes:**

- Currently using AWS S3 (~$0.023/GB storage + $0.09/GB transfer). DO Spaces would be cheaper.
- 500 listings × ~10 images × ~500KB = ~2.5 GB/month new storage.
- CDN is free with DO Spaces, reducing latency for DR users.

---

### 2. Stripe (Payment Processing)

**Service:** BillingService  
**Purpose:** Seller listing fees ($29/listing), dealer subscriptions ($49–$299/mo)  
**Credentials:** `k8s/secrets.template.yaml` → `external-services-secrets` (`STRIPE_SECRET_KEY`, `STRIPE_WEBHOOK_SECRET`)  
**Config:** `k8s/deployments.yaml` → `billingservice` env vars

| Component            | Pricing                            | Est. Monthly  |
| -------------------- | ---------------------------------- | ------------- |
| Transaction fee      | 2.9% + $0.30 per successful charge | $10–40        |
| Subscription billing | Included                           | $0            |
| Stripe Tax           | 0.5% (if enabled)                  | $0–5          |
| Radar (fraud)        | Free (basic), $0.05/txn (advanced) | $0–5          |
| **Subtotal**         |                                    | **$10–50/mo** |

**Estimated volume:** ~50 individual listings ($29 each) + ~20 dealer subscriptions (~$100 avg) = ~$3,450 GMV → ~$115 in Stripe fees.

**Notes:** Stripe is the primary international payment gateway. For Dominican cards, Azul may be more cost-effective.

---

### 3. Azul (Banco Popular Dominican Republic)

**Service:** BillingService  
**Purpose:** Local Dominican payment processing (debit/credit cards)  
**Credentials:** Not yet in K8s secrets (planned)  
**Config:** `backend/ConfigurationService/ConfigurationService.Api/Program.cs` (lines 328-333)  
**Domain entities:** `backend/BillingService/BillingService.Domain/Interfaces/IAzulTransactionRepository.cs`

| Component       | Pricing                      | Est. Monthly  |
| --------------- | ---------------------------- | ------------- |
| Transaction fee | ~3.5% per transaction        | $10–35        |
| Monthly fee     | Varies by merchant agreement | $0–25         |
| Setup fee       | One-time ~$200-500           | N/A           |
| **Subtotal**    |                              | **$10–60/mo** |

**Status:** 🟡 Planned — Configuration flags exist, domain entities created, but not yet deployed. No K8s secrets configured.

---

### 4. Resend (Email Delivery)

**Service:** NotificationService  
**Purpose:** Transactional emails (welcome, password reset, listing notifications)  
**Credentials:** `k8s/secrets.template.yaml` → `external-services-secrets` (`RESEND_API_KEY`)  
**Config:** `k8s/deployments.yaml` → `notificationservice` env vars

| Tier     | Emails/month | Price  |
| -------- | ------------ | ------ |
| Free     | 3,000        | $0     |
| Pro      | 50,000       | $20/mo |
| Business | 100,000      | $50/mo |

**Est. Monthly Cost:** **$0** (free tier sufficient for early stage)

**Notes:** At 5,000 MAU with ~2 emails/user/month, free tier (3,000/mo) may be tight. Monitor usage.

---

### 5. SendGrid (Backup Email)

**Service:** NotificationService  
**Purpose:** Fallback email provider  
**Credentials:** `k8s/secrets.template.yaml` → `external-services-secrets` (`SENDGRID_API_KEY`)

| Tier       | Emails/month | Price     |
| ---------- | ------------ | --------- |
| Free       | 100/day      | $0        |
| Essentials | 50,000       | $19.95/mo |

**Est. Monthly Cost:** **$0** (backup only, free tier)

---

### 6. Twilio (SMS & WhatsApp)

**Service:** NotificationService  
**Purpose:** SMS verification codes, appointment reminders, WhatsApp notifications  
**Credentials:** `k8s/secrets.template.yaml` → `external-services-secrets` (`TWILIO_ACCOUNT_SID`, `TWILIO_AUTH_TOKEN`)  
**Config:** `backend/NotificationService/NotificationService.Api/appsettings.Development.json`

**SMS Pricing (Dominican Republic):**

| Component         | Price       | Est. Monthly |
| ----------------- | ----------- | ------------ |
| Outbound SMS (DR) | $0.0760/msg | $5–20        |
| Phone number      | $1.15/mo    | $1.15        |
| Inbound SMS       | $0.0075/msg | $0–2         |

**WhatsApp Pricing (DR):**

| Component               | Price       | Est. Monthly |
| ----------------------- | ----------- | ------------ |
| Utility conversations   | $0.0042/msg | $2–10        |
| Marketing conversations | $0.0153/msg | $3–10        |

**Est. Monthly Cost:** **$10–35/mo** (when activated)

**Status:** ❌ Not configured — credentials not in K8s secrets.

---

### 7. Anthropic Claude AI

**Services:** SearchAgent, SupportAgent, RecoAgent, ChatbotService  
**Credentials:** `k8s/secrets.template.yaml` → `claude-api-secret` (`CLAUDE_API_KEY`)

**Models in use:**

| Service        | Model                      | Input Price | Output Price | Use Case                |
| -------------- | -------------------------- | ----------- | ------------ | ----------------------- |
| SearchAgent    | claude-haiku-4-5-20251001  | $0.80/MTok  | $4.00/MTok   | NL vehicle search       |
| SupportAgent   | claude-haiku-4-5-20251001  | $0.80/MTok  | $4.00/MTok   | Customer support chat   |
| RecoAgent      | claude-sonnet-4-5-20251022 | $3.00/MTok  | $15.00/MTok  | Vehicle recommendations |
| ChatbotService | claude-sonnet-4-5          | $3.00/MTok  | $15.00/MTok  | Dealer AI chatbot       |

**Cost estimation (early stage):**

| Service        | Requests/mo | Avg tokens/req   | Est. Cost     |
| -------------- | ----------- | ---------------- | ------------- |
| SearchAgent    | 2,000       | ~800 in/200 out  | $2–5          |
| SupportAgent   | 500         | ~1000 in/300 out | $1–3          |
| RecoAgent      | 300         | ~1500 in/500 out | $3–10         |
| ChatbotService | 1,000       | ~2000 in/500 out | $10–25        |
| **Subtotal**   |             |                  | **$16–43/mo** |

**Notes:**

- Prompt caching is enabled (`CacheControl` in SearchAgent), which reduces costs by ~90% for cached system prompts.
- Rate limiting is configured on AI endpoints to control costs.
- ChatbotService also supports a local LLM option (`okla-llama3-8b-q4_k_m.gguf`) for cost-free fallback.

---

### 8. Firebase (Push Notifications)

**Service:** NotificationService  
**Purpose:** Push notifications to mobile/web apps  
**Credentials:** `k8s/secrets.template.yaml` → `external-services-secrets` (via `FirebaseCredentialProvider`)  
**Config:** `backend/NotificationService/NotificationService.Infrastructure/Configuration/FirebaseCredentialProvider.cs`

| Component             | Pricing          |
| --------------------- | ---------------- |
| FCM (Cloud Messaging) | Free (unlimited) |
| Analytics             | Free (standard)  |
| Crashlytics           | Free             |

**Est. Monthly Cost:** **$0**

**Status:** 🟡 Configured in code with graceful degradation — works if credentials are present, silently degrades if not.

---

### 9. Google Maps

**Service:** Frontend (web-next)  
**Purpose:** Vehicle location maps, dealer maps  
**Credentials:** `frontend/web-next/.env.local` → `NEXT_PUBLIC_GOOGLE_MAPS_KEY`

| API                 | Free tier      | Beyond free tier     |
| ------------------- | -------------- | -------------------- |
| Maps JavaScript API | $200/mo credit | $7.00/1000 loads     |
| Geocoding           | $200/mo credit | $5.00/1000 requests  |
| Places              | $200/mo credit | $17.00/1000 requests |

**Est. Monthly Cost:** **$0–10** (Google's $200/mo free credit covers ~28,000 map loads)

**Status:** ❌ Not configured in active frontend. Key exists hardcoded in discarded frontend code.

---

### 10. Google OAuth

**Service:** AuthService  
**Purpose:** "Sign in with Google"  
**Credentials:** `backend/AuthService/AuthService.Api/appsettings.json`

**Pricing:** Free (no per-request charges)

**Est. Monthly Cost:** **$0**

**Status:** ❌ Not configured

---

### 11. Google reCAPTCHA v3

**Service:** AuthService  
**Purpose:** Bot protection on auth forms  
**Credentials:** `backend/AuthService/AuthService.Api/appsettings.json`

| Tier       | Assessments/mo | Price   |
| ---------- | -------------- | ------- |
| Free       | 1,000,000      | $0      |
| Enterprise | Beyond 1M      | $1/1000 |

**Est. Monthly Cost:** **$0**

**Status:** ❌ Not configured (enabled flag present but keys are placeholders)

---

### 12. Infrastructure Services

| Service              | Provider     | Pricing                      | Est. Monthly Cost |
| -------------------- | ------------ | ---------------------------- | ----------------- |
| PostgreSQL (Managed) | DigitalOcean | $15/mo (1GB, 1vCPU) basic    | $15–60            |
| Redis                | In-cluster   | Part of K8s node cost        | $0                |
| RabbitMQ             | In-cluster   | Part of K8s node cost        | $0                |
| DOKS (Kubernetes)    | DigitalOcean | $12/mo control plane + nodes | $36–72            |
| GHCR (Registry)      | GitHub       | Free for public repos        | $0                |
| Seq (Logging)        | In-cluster   | Open-source                  | $0                |
| Jaeger (Tracing)     | In-cluster   | Open-source                  | $0                |

**DOKS Node Pricing:**

| Node Type                  | vCPU | RAM  | Price/mo | Min Nodes |
| -------------------------- | ---- | ---- | -------- | --------- |
| Basic (s-1vcpu-2gb)        | 1    | 2 GB | $12      | 3         |
| Professional (s-2vcpu-4gb) | 2    | 4 GB | $24      | 3         |

---

## Total Monthly Cost Projection

### Startup Phase (Current — <1,000 MAU)

| Category            | Cost Range     |
| ------------------- | -------------- |
| Infrastructure (DO) | $51–132        |
| Payments (Stripe)   | $10–50         |
| AI (Claude)         | $16–43         |
| Email (Resend)      | $0             |
| Storage (S3/Spaces) | $5–15          |
| Other APIs          | $0             |
| **TOTAL**           | **$82–240/mo** |

### Growth Phase (1,000–10,000 MAU)

| Category                 | Cost Range      |
| ------------------------ | --------------- |
| Infrastructure (DO)      | $132–300        |
| Payments (Stripe + Azul) | $50–200         |
| AI (Claude)              | $40–150         |
| Email (Resend Pro)       | $20–50          |
| SMS/WhatsApp             | $20–80          |
| Storage (DO Spaces)      | $10–30          |
| Maps/Analytics           | $0–20           |
| **TOTAL**                | **$272–830/mo** |

### Scale Phase (10,000+ MAU)

| Category            | Cost Range        |
| ------------------- | ----------------- |
| Infrastructure (DO) | $300–800          |
| Payments            | $200–600          |
| AI (Claude)         | $150–500          |
| Email               | $50–100           |
| SMS/WhatsApp        | $80–300           |
| Storage             | $30–100           |
| Maps/Analytics      | $20–50            |
| **TOTAL**           | **$830–2,450/mo** |

---

## Cost Optimization Recommendations

1. **Migrate to DO Spaces** — Save ~40% on storage costs vs AWS S3, plus free CDN.
2. **Use Claude Haiku over Sonnet** where possible — Haiku is ~4x cheaper. Consider using Haiku for ChatbotService routine queries and Sonnet only for complex recommendations.
3. **Implement prompt caching aggressively** — System prompts are ~80% of input tokens; caching saves ~90%.
4. **Use Azul for Dominican cards** — Lower processing fees for local transactions vs international Stripe charges.
5. **Monitor email volume** — Upgrade Resend to Pro ($20/mo) only when approaching 3,000 emails/month.
6. **Leverage Google's $200/mo credit** — Maps, Geocoding, and reCAPTCHA all share this credit; sufficient for early stage.
7. **Use local LLM fallback** — ChatbotService supports a local Llama 3 model for zero-cost AI when Claude is unavailable or for non-critical queries.

---

_Analysis performed on 2026-03-05 based on codebase audit and published API pricing._
