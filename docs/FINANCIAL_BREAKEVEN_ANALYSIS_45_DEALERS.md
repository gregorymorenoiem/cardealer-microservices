# 💰 OKLA Financial Break-Even Analysis — 45 Dealers

**Date:** March 7, 2026  
**Scope:** Complete financial viability analysis with 45 active dealers  
**Sources:** Codebase audit (`AdminService/DealerService.cs`, `BillingService/context.md`, `k8s/deployments.yaml`, `docs/INFRASTRUCTURE_COST_ANALYSIS.md`, `docs/API_COST_ANALYSIS.md`, `docs/process-matrix/05-PAGOS-FACTURACION/`)

---

## 1. ⚠️ Price Discrepancy Notice

The codebase contains **two different pricing models**. This analysis uses the **production pricing from the billing process matrix**, which is the authoritative source implemented in `BillingService`:

| Plan                   | Prices in Code (AdminService MRR calc) | Prices in BillingService Process Matrix (Authoritative) |
| ---------------------- | -------------------------------------- | ------------------------------------------------------- |
| **Free/None**          | $0/mo                                  | $0/mo (3 vehicles, 30 days)                             |
| **Starter/Basic**      | $49/mo                                 | **$49/mo** (15 vehicles, 3 employees, 1 location)       |
| **Pro/Professional**   | $149/mo                                | **$129/mo** (50 vehicles, 10 employees, 3 locations)    |
| **Enterprise/Premium** | $299/mo                                | **$299/mo** (unlimited everything)                      |

> **Note:** The AdminService MRR calculation uses $49/$149/$299. The billing process matrix documents $49/$129/$299. The E2E tests show `Professional` at $149. **This analysis models both sets** and uses $49/$129/$299 as the primary scenario since it matches the most recent billing documentation.

### Additional Revenue Streams (Per-Listing for Individual Sellers)

| Service         | Price | Duration |
| --------------- | ----- | -------- |
| Publish vehicle | $29   | 30 days  |
| Renew listing   | $19   | 30 days  |
| Feature vehicle | $15   | 7 days   |
| Super Feature   | $39   | 7 days   |
| Bump to Top     | $9    | 24 hours |

### OKLA Coins

Mentioned in codebase (`GET /api/okla-coins/packages`) but package details ($25/$50/$100/$250) not yet documented in the backend process matrix. Excluded from this analysis as they represent supplementary revenue.

---

## 2. 📊 Revenue Modeling — 45 Dealers

### Scenario A: Conservative Mix (Many Free, Few Premium)

**Assumption:** 60% Free, 20% Starter, 15% Pro, 5% Enterprise

| Plan       | Dealers | Price/mo | Monthly Revenue |
| ---------- | ------- | -------- | --------------- |
| Free/None  | 27      | $0       | $0              |
| Starter    | 9       | $49      | $441            |
| Pro        | 7       | $129     | $903            |
| Enterprise | 2       | $299     | $598            |
| **TOTAL**  | **45**  |          | **$1,942/mo**   |

**Annual Revenue:** $23,304

---

### Scenario B: Balanced Mix (Realistic Early-Stage)

**Assumption:** 40% Free, 25% Starter, 25% Pro, 10% Enterprise

| Plan       | Dealers | Price/mo | Monthly Revenue |
| ---------- | ------- | -------- | --------------- |
| Free/None  | 18      | $0       | $0              |
| Starter    | 11      | $49      | $539            |
| Pro        | 11      | $129     | $1,419          |
| Enterprise | 5       | $299     | $1,495          |
| **TOTAL**  | **45**  |          | **$3,453/mo**   |

**Annual Revenue:** $41,436

---

### Scenario C: Optimistic Mix (Good Conversion)

**Assumption:** 25% Free, 25% Starter, 30% Pro, 20% Enterprise

| Plan       | Dealers | Price/mo | Monthly Revenue |
| ---------- | ------- | -------- | --------------- |
| Free/None  | 11      | $0       | $0              |
| Starter    | 11      | $49      | $539            |
| Pro        | 14      | $129     | $1,806          |
| Enterprise | 9       | $299     | $2,691          |
| **TOTAL**  | **45**  |          | **$5,036/mo**   |

**Annual Revenue:** $60,432

---

### Scenario D: All 45 Are Paying Dealers (No Free)

**Assumption:** 0% Free, 40% Starter, 35% Pro, 25% Enterprise

| Plan       | Dealers | Price/mo | Monthly Revenue |
| ---------- | ------- | -------- | --------------- |
| Free/None  | 0       | $0       | $0              |
| Starter    | 18      | $49      | $882            |
| Pro        | 16      | $129     | $2,064          |
| Enterprise | 11      | $299     | $3,289          |
| **TOTAL**  | **45**  |          | **$6,235/mo**   |

**Annual Revenue:** $74,820

---

### Supplementary Revenue: Individual Sellers (Per-Listing)

Assuming 50–200 individual listing purchases/month at $29/listing:

| Volume          | Monthly Revenue | Annual Revenue |
| --------------- | --------------- | -------------- |
| 50 listings/mo  | $1,450          | $17,400        |
| 100 listings/mo | $2,900          | $34,800        |
| 200 listings/mo | $5,800          | $69,600        |

**Plus renewals, features, and bumps (est. 30% addon rate):**

| Volume          | Base   | Addons (~30%) | Total/mo |
| --------------- | ------ | ------------- | -------- |
| 50 listings/mo  | $1,450 | $435          | $1,885   |
| 100 listings/mo | $2,900 | $870          | $3,770   |
| 200 listings/mo | $5,800 | $1,740        | $7,540   |

---

## 3. 🏗️ Monthly OPEX Breakdown

### 3.1 Infrastructure (DigitalOcean)

Source: `docs/INFRASTRUCTURE_COST_ANALYSIS.md`, `k8s/deployments.yaml`

#### Production Configuration (Recommended for 45 Dealers)

| Component                         | Configuration            | Monthly Cost |
| --------------------------------- | ------------------------ | ------------ |
| **DOKS Control Plane**            | Free (no HA)             | $0           |
| **Node 1** (`s-4vcpu-8gb`)        | 4 vCPU, 8GB RAM          | $48          |
| **Node 2** (`s-4vcpu-8gb`)        | 4 vCPU, 8GB RAM          | $48          |
| **DO Managed PostgreSQL**         | Basic, 1 vCPU, 1GB, 10Gi | $15          |
| **DO Spaces** (images/CDN)        | 250GB + CDN              | $5           |
| **Load Balancer** (nginx ingress) | 1× small                 | $12          |
| **Block Storage** (PVCs)          | ~17Gi                    | $2           |
| **Container Registry** (GHCR)     | GitHub Free              | $0           |
| **Subtotal Infrastructure**       |                          | **$130/mo**  |

#### Staging-Optimized Configuration (Cost-Cutting)

| Component                                               | Configuration | Monthly Cost |
| ------------------------------------------------------- | ------------- | ------------ |
| DOKS (mixed nodes: 1× `s-4vcpu-8gb` + 1× `s-2vcpu-4gb`) |               | $72          |
| PostgreSQL (in-cluster)                                 | Self-managed  | $0           |
| DO Spaces                                               | 250GB + CDN   | $5           |
| Load Balancer                                           | 1× small      | $12          |
| Block Storage                                           | ~17Gi         | $2           |
| **Subtotal Infrastructure (Optimized)**                 |               | **$91/mo**   |

### 3.2 External API Costs

Source: `docs/API_COST_ANALYSIS.md`

| Service                 | Purpose                                   | Est. Monthly Cost (45 Dealers) |
| ----------------------- | ----------------------------------------- | ------------------------------ |
| **Stripe**              | Payment processing (2.9% + $0.30/txn)     | $50–$150                       |
| **Anthropic Claude AI** | SearchAgent, SupportAgent, ChatbotService | $20–$60                        |
| **Resend** (Email)      | Transactional emails                      | $0–$20                         |
| **DO Spaces** (media)   | Already counted above                     | $0                             |
| **Firebase FCM**        | Push notifications                        | $0                             |
| **NHTSA**               | VIN decode                                | $0                             |
| **Subtotal API**        |                                           | **$70–$230/mo**                |

#### Stripe Fee Detail Calculation

| Revenue Source                           | GMV/mo     | Stripe Fee (2.9% + $0.30) | Est. Fee |
| ---------------------------------------- | ---------- | ------------------------- | -------- |
| Dealer subs (Scenario B: $3,453)         | $3,453     | 2.9% + $0.30 × ~27 txns   | $108     |
| Dealer subs (Scenario A: $1,942)         | $1,942     | 2.9% + $0.30 × ~18 txns   | $62      |
| Individual listings (100/mo)             | $2,900     | 2.9% + $0.30 × 100        | $114     |
| **Combined (Scenario B + 100 listings)** | **$6,353** |                           | **$222** |

### 3.3 Operational Costs (Lean Team)

| Item                     | Monthly Cost | Notes                       |
| ------------------------ | ------------ | --------------------------- |
| Domain (okla.com.do)     | ~$3          | Annual cost amortized       |
| SSL (Let's Encrypt)      | $0           | Free via cert-manager       |
| GitHub (Pro)             | $4           | CI/CD, Actions, GHCR        |
| Monitoring (in-cluster)  | $0           | Prometheus + Grafana in K8s |
| Logging (Seq in-cluster) | $0           | Self-hosted                 |
| **Subtotal Operational** |              | **~$7/mo**                  |

### 3.4 Total Monthly OPEX Summary

| Configuration                                  | Infrastructure | APIs     | Operations | **Total OPEX**   |
| ---------------------------------------------- | -------------- | -------- | ---------- | ---------------- |
| **Production** (2× `s-4vcpu-8gb` + managed DB) | $130           | $70–$230 | $7         | **$207–$367/mo** |
| **Optimized** (mixed nodes, self-managed DB)   | $91            | $70–$230 | $7         | **$168–$328/mo** |
| **Midpoint estimate (Production)**             | $130           | $150     | $7         | **$287/mo**      |
| **Midpoint estimate (Optimized)**              | $91            | $150     | $7         | **$248/mo**      |

> **Note:** Stripe fees scale with revenue — they are only incurred when revenue comes in. The fixed OPEX floor (infrastructure + operations) is **$91–$137/mo**.

---

## 4. 📈 Break-Even Analysis

### 4.1 Fixed vs. Variable Costs

| Cost Type         | Components                                                                 | Monthly Amount                  |
| ----------------- | -------------------------------------------------------------------------- | ------------------------------- |
| **Fixed OPEX**    | Infrastructure + Operations                                                | $97–$137/mo                     |
| **Variable OPEX** | Stripe (~3.2% effective), AI ($0.50–$1.50/dealer/mo), Email ($0.002/email) | ~3.5% of revenue + $30–$70 base |

**Simplified cost formula:**
$$\text{Total OPEX} = \text{Fixed}_{infra} + \text{AI}_{base} + 0.035 \times \text{Revenue}$$

### 4.2 Break-Even Calculation

**For Production Infrastructure ($137/mo fixed + $50/mo AI base):**

$$\text{Break-even Revenue} = \frac{\text{Fixed Costs}}{1 - \text{Variable Rate}} = \frac{187}{1 - 0.035} = \$194/\text{mo}$$

**For Optimized Infrastructure ($98/mo fixed + $30/mo AI base):**

$$\text{Break-even Revenue} = \frac{128}{0.965} = \$133/\text{mo}$$

### 4.3 Minimum Dealers to Break Even (Subscription Revenue Only)

#### Production Infrastructure ($137/mo fixed)

| Scenario                     | Min. Paying Dealers | Plan Mix                         |
| ---------------------------- | ------------------- | -------------------------------- |
| All Starter ($49)            | **4 dealers**       | 4 × $49 = $196 > $194            |
| All Pro ($129)               | **2 dealers**       | 2 × $129 = $258 > $194           |
| All Enterprise ($299)        | **1 dealer**        | 1 × $299 > $194                  |
| Mixed (50% Starter, 50% Pro) | **3 dealers**       | 1.5×$49 + 1.5×$129 = $267 > $194 |

#### Optimized Infrastructure ($98/mo fixed)

| Scenario              | Min. Paying Dealers | Plan Mix               |
| --------------------- | ------------------- | ---------------------- |
| All Starter ($49)     | **3 dealers**       | 3 × $49 = $147 > $133  |
| All Pro ($129)        | **2 dealers**       | 2 × $129 = $258 > $133 |
| All Enterprise ($299) | **1 dealer**        | 1 × $299 > $133        |

> **🎯 Key Finding: OKLA breaks even with as few as 3–4 paying dealers on Starter plans.** The infrastructure cost is remarkably low relative to subscription prices.

---

## 5. 💰 Profitability Scenarios with 45 Dealers

### 5.1 Net Revenue After All Costs

| Scenario                       | Gross Revenue/mo | Stripe Fee (~3.2%) | AI Costs | Fixed OPEX (Prod) | **Net Profit/mo** | **Margin** |
| ------------------------------ | ---------------- | ------------------ | -------- | ----------------- | ----------------- | ---------- |
| **A: Conservative** (60% Free) | $1,942           | -$62               | -$50     | -$137             | **$1,693**        | 87%        |
| **B: Balanced** (40% Free)     | $3,453           | -$111              | -$50     | -$137             | **$3,155**        | 91%        |
| **C: Optimistic** (25% Free)   | $5,036           | -$161              | -$60     | -$137             | **$4,678**        | 93%        |
| **D: All Paying** (0% Free)    | $6,235           | -$200              | -$70     | -$137             | **$5,828**        | 93%        |

### 5.2 With Individual Seller Revenue Added (100 listings/mo)

| Scenario        | Dealer Rev | Seller Rev | Total Gross | Total OPEX | **Net Profit/mo** | **Annual Profit** |
| --------------- | ---------- | ---------- | ----------- | ---------- | ----------------- | ----------------- |
| **A + Sellers** | $1,942     | $3,770     | $5,712      | -$432      | **$5,280**        | **$63,360**       |
| **B + Sellers** | $3,453     | $3,770     | $7,223      | -$480      | **$6,743**        | **$80,916**       |
| **C + Sellers** | $5,036     | $3,770     | $8,806      | -$530      | **$8,276**        | **$99,312**       |
| **D + Sellers** | $6,235     | $3,770     | $10,005     | -$565      | **$9,440**        | **$113,280**      |

---

## 6. 📊 What Plan Mix Do We Need for Break-Even with 45 Dealers?

### 6.1 Against Production OPEX ($287/mo midpoint)

| #   | Mix                                               | Paying Dealers | Revenue/mo            | Covers OPEX? | Surplus     |
| --- | ------------------------------------------------- | -------------- | --------------------- | ------------ | ----------- |
| 1   | 95% Free, 5% Starter                              | 2 paying       | $98                   | ❌           | -$189       |
| 2   | 90% Free, 10% Starter                             | 5 paying       | $245                  | ❌           | -$42        |
| 3   | 85% Free, 15% Starter                             | 7 paying       | $343                  | ✅           | +$56        |
| 4   | 80% Free, 10% Starter, 10% Pro                    | 9 paying       | 5×$49 + 4×$129 = $761 | ✅           | +$474       |
| 5   | **60% Free, 20% Starter, 15% Pro, 5% Enterprise** | **18 paying**  | **$1,942**            | **✅**       | **+$1,655** |

> **🎯 Minimum for break-even: ~7 dealers on Starter ($343/mo) OR ~4 dealers on Pro ($516/mo) OR any 6 paying dealers on a Starter/Pro mix.** All 45-dealer scenarios with 15%+ paying dealers are profitable.

### 6.2 Sensitivity: What If Only 10 Out of 45 Pay?

| Mix of 10 Paying Dealers         | Revenue/mo | Net After OPEX | Profitable? |
| -------------------------------- | ---------- | -------------- | ----------- |
| 10 × Starter ($49)               | $490       | +$203          | ✅          |
| 7 Starter + 3 Pro                | $730       | +$443          | ✅          |
| 5 Starter + 3 Pro + 2 Enterprise | $1,230     | +$943          | ✅          |
| 3 Starter + 4 Pro + 3 Enterprise | $1,560     | +$1,273        | ✅          |

**Even with only 10 out of 45 dealers paying (22% conversion), OKLA is profitable.**

---

## 7. 🏗️ Infrastructure Scaling Thresholds

| Dealers | Users (est.)     | Current Infra OK? | Action Needed                | New Monthly OPEX |
| ------- | ---------------- | ----------------- | ---------------------------- | ---------------- |
| 1–50    | <1,000 MAU       | ✅ Yes            | None                         | $130/mo          |
| 50–200  | 1,000–5,000 MAU  | ✅ Yes            | Maybe 3rd node at peak       | $130–$178/mo     |
| 200–500 | 5,000–20,000 MAU | ⚠️ Scale up       | 3 nodes + managed DB + HA    | $283/mo          |
| 500+    | 20,000+ MAU      | ❌ Must scale     | 4+ nodes, managed Redis, CDN | $400–$600/mo     |

**At 45 dealers, the current 2-node production cluster is more than sufficient.** No scaling actions needed.

---

## 8. 🎯 Revenue Growth Projections (12-Month)

### Projection: Starting with 45 dealers, adding 10/month, improving conversion

| Month | Total Dealers | Paying (%) | MRR (Dealer Subs) | Seller Listings | Total MRR | Cumulative |
| ----- | ------------- | ---------- | ----------------- | --------------- | --------- | ---------- |
| 1     | 45            | 40% (18)   | $1,942            | $1,450          | $3,392    | $3,392     |
| 2     | 55            | 42% (23)   | $2,507            | $1,740          | $4,247    | $7,639     |
| 3     | 65            | 44% (29)   | $3,160            | $2,030          | $5,190    | $12,829    |
| 4     | 75            | 46% (35)   | $3,815            | $2,320          | $6,135    | $18,964    |
| 5     | 85            | 48% (41)   | $4,470            | $2,610          | $7,080    | $26,044    |
| 6     | 95            | 50% (48)   | $5,232            | $2,900          | $8,132    | $34,176    |
| 7     | 105           | 50% (53)   | $5,777            | $3,190          | $8,967    | $43,143    |
| 8     | 115           | 52% (60)   | $6,540            | $3,480          | $10,020   | $53,163    |
| 9     | 125           | 52% (65)   | $7,085            | $3,770          | $10,855   | $64,018    |
| 10    | 135           | 54% (73)   | $7,957            | $4,060          | $12,017   | $76,035    |
| 11    | 145           | 54% (78)   | $8,502            | $4,350          | $12,852   | $88,887    |
| 12    | 155           | 55% (85)   | $9,265            | $4,640          | $13,905   | $102,792   |

**Year 1 Gross Revenue: ~$102,792**  
**Year 1 Total OPEX: ~$4,200 (growing with scale)**  
**Year 1 Net Revenue: ~$98,500** (before team salaries, marketing)

> **Assumptions:** Average paying dealer revenue = $109/mo (weighted mix: 45% Starter, 35% Pro, 20% Enterprise). Individual seller listings grow proportionally with platform traffic.

---

## 9. 💡 Key Financial Metrics

| Metric                                      | Value                                      |
| ------------------------------------------- | ------------------------------------------ |
| **Fixed OPEX floor**                        | $91–$137/mo                                |
| **Break-even (dealer subs only)**           | **3–4 paying dealers**                     |
| **Break-even with 45 dealers**              | Achieved at **~15% conversion** (7 paying) |
| **Gross margin at Scenario B**              | **91%**                                    |
| **ARPU (paying dealers)**                   | $109–$192/mo (depending on mix)            |
| **LTV (12-mo, 5% churn)**                   | $1,308–$2,304/dealer                       |
| **CAC target**                              | <$130 (10:1 LTV:CAC ratio)                 |
| **Stripe effective rate**                   | ~3.2% of revenue                           |
| **Infrastructure cost per dealer**          | $2.89–$3.04/dealer/mo                      |
| **Payback period (45 dealers, Scenario B)** | **Month 1** ✅                             |

---

## 10. ✅ Conclusions

### Is 45 dealers enough to cover OPEX?

**YES, overwhelmingly.** Even in the most conservative scenario (60% free, 40% paying), 45 dealers generate **$1,942/mo** against a total OPEX of **$207–$287/mo**. That's a **6.8x–9.4x OPEX coverage ratio**.

### What's the absolute minimum for break-even?

- **Infrastructure-only break-even:** 3 Starter dealers ($147/mo vs $91–$137/mo OPEX)
- **Full OPEX break-even:** 4 Starter dealers or 2 Pro dealers
- **With 45 dealers:** Profitable from Day 1 if **even 7 dealers (15%) convert to any paid plan**

### The real challenge isn't OPEX — it's growth

OKLA's infrastructure costs are extremely lean (~$130–$287/mo total). The financial model is healthy even at very low conversion rates. The key business challenges are:

1. **Dealer acquisition** — Getting to 45 dealers and beyond
2. **Conversion optimization** — Moving free dealers to Starter → Pro → Enterprise
3. **Retention** — Reducing churn to maximize LTV
4. **Marketing spend** — The budget that isn't in OPEX but is needed for growth

### What if we use the alternate pricing ($49/$149/$299)?

The AdminService code uses $149 for Pro instead of $129. If we use those prices, Scenario B becomes **$3,663/mo** (+6% higher), making all scenarios even more favorable.

---

## Appendix: Comparison with Dominican Competitors

| Platform                    | Individual Listing | Dealer Monthly | OKLA Advantage                                      |
| --------------------------- | ------------------ | -------------- | --------------------------------------------------- |
| **SuperCarros** (Estándar)  | ~$35 (RD$2,100)    | Contact sales  | OKLA is **17% cheaper** ($29 vs $35)                |
| **SuperCarros** (Destacado) | ~$54 (RD$3,250)    | Contact sales  | OKLA Feature ($15+$29=$44) is **19% cheaper**       |
| **SuperCarros** (Premium)   | ~$117 (RD$7,000)   | Contact sales  | OKLA Super Feature ($39+$29=$68) is **42% cheaper** |
| **Corotos** (Plus)          | Varies             | Varies         | OKLA offers KYC, AI, CRM, Analytics for $49-$299    |

OKLA's pricing is competitive for the Dominican market while offering significantly more features (AI, KYC, CRM, Analytics, Comparisons) than any local competitor.

---

_Analysis generated March 7, 2026 from codebase audit of `AdminService/DealerService.cs`, `BillingService/context.md`, `k8s/deployments.yaml`, `docs/INFRASTRUCTURE_COST_ANALYSIS.md`, `docs/API_COST_ANALYSIS.md`, and `docs/process-matrix/05-PAGOS-FACTURACION/`._
