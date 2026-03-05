# 🏗️ OKLA Infrastructure Audit — DigitalOcean

> **Date:** 2026-03-05  
> **Purpose:** Audit current infrastructure on DigitalOcean, cost analysis, and optimization recommendations.

---

## 1. Current Infrastructure Overview

### 1.1 Kubernetes Cluster (DOKS)

| Parameter          | Value                           |
| ------------------ | ------------------------------- |
| **Cluster name**   | okla                            |
| **Region**         | (DO region — verify in console) |
| **K8s version**    | v1.34.1                         |
| **Nodes**          | 2                               |
| **Node pool**      | `okla-pool-upgraded`            |
| **Node size**      | 4 vCPU / ~8 GB RAM each         |
| **Total capacity** | 8 vCPU / ~16 GB RAM             |
| **OS**             | Debian trixie/sid               |

### 1.2 Active Services (18/51 deployments running)

| #   | Service             | Replicas | Purpose                        |
| --- | ------------------- | -------- | ------------------------------ |
| 1   | frontend-web        | 1/1      | Next.js frontend               |
| 2   | gateway             | 1/1      | Ocelot API Gateway             |
| 3   | authservice         | 1/1      | Authentication & authorization |
| 4   | userservice         | 1/1      | User profiles & accounts       |
| 5   | roleservice         | 1/1      | RBAC role management           |
| 6   | vehiclessaleservice | 1/1      | Vehicle listings & search      |
| 7   | mediaservice        | 1/1      | Image upload & processing      |
| 8   | billingservice      | 1/1      | Stripe payments                |
| 9   | contactservice      | 1/1      | Contact forms & inquiries      |
| 10  | notificationservice | 1/1      | Email/SMS notifications        |
| 11  | chatbotservice      | 1/1      | AI support chatbot             |
| 12  | kycservice          | 1/1      | Identity verification          |
| 13  | reviewservice       | 1/1      | Vehicle/dealer reviews         |
| 14  | adminservice        | 1/1      | Admin panel API                |
| 15  | auditservice        | 1/1      | Audit logging                  |
| 16  | errorservice        | 1/1      | Error tracking & DLQ           |
| 17  | rabbitmq            | 1/1      | Message broker                 |
| 18  | redis               | 1/1      | Cache layer                    |

### 1.3 Inactive Services (33 deployments at 0/0)

These services are deployed but scaled to 0 replicas (not consuming resources):

- advertisingservice, aiprocessingservice, alertservice, appointmentservice
- backgroundremovalservice, cacheservice, comparisonservice, configurationservice
- crmservice, dataprotectionservice, dealeranalyticsservice, dealermanagementservice
- idempotencyservice, integrationservice, inventorymanagementservice
- leadscoringservice, llm-server, maintenanceservice, marketingservice
- messagebusservice, paymentservice, ratelimitingservice, recommendationservice
- reportsservice, schedulerservice, searchagent, servicediscovery
- staffservice, supportagent, vehicle360processingservice
- vehicleintelligenceservice, video360service, apidocsservice

### 1.4 Managed Database (PostgreSQL)

| Parameter     | Value                                                            |
| ------------- | ---------------------------------------------------------------- |
| **Host**      | `okla-db-do-user-31493168-0.g.db.ondigitalocean.com`             |
| **Port**      | 25060                                                            |
| **SSL**       | Required                                                         |
| **Databases** | ~18 (one per active service)                                     |
| **Size**      | (Verify in DO Console — likely db-s-1vcpu-1gb or db-s-1vcpu-2gb) |

---

## 2. Estimated Monthly Cost

| Resource                 | DO Plan (est.)            | Monthly Cost (USD) |
| ------------------------ | ------------------------- | ------------------ |
| **DOKS Cluster**         | Free (control plane)      | $0                 |
| **2× Worker Nodes**      | s-4vcpu-8gb ($48/mo each) | $96                |
| **Managed PostgreSQL**   | db-s-1vcpu-2gb (est.)     | $15                |
| **Container Registry**   | Starter (500 MB)          | $0 (free)          |
| **Load Balancer**        | Standard                  | $12                |
| **DNS (okla.com.do)**    | DO DNS                    | $0 (free)          |
| **Bandwidth**            | Included with droplets    | $0                 |
| **Total Infrastructure** |                           | **~$123/mo**       |

### Additional External Services

| Service              | Provider  | Monthly Cost (est.)          |
| -------------------- | --------- | ---------------------------- |
| AWS S3 (images)      | AWS       | ~$5-10                       |
| Resend (email)       | Resend    | $0 (free tier: 3K emails/mo) |
| Stripe (payments)    | Stripe    | 2.9% + $0.30 per transaction |
| Anthropic Claude     | Anthropic | ~$5-20 (usage-based)         |
| Domain (okla.com.do) | Registrar | ~$30/year (~$2.50/mo)        |

### **Total Estimated Monthly: ~$130-150/mo**

---

## 3. Optimization Recommendations

### 3.1 ✅ IMPLEMENT (Cost < $100 impact)

#### A. Migrate AWS S3 → DO Spaces ($5/mo)

**Current:** AWS S3 in Ohio (us-east-2) — high latency for DR users  
**Proposed:** DO Spaces in NYC3 with CDN — ~65ms latency to DR  
**Cost:** $5/mo (250 GB storage + 1 TB transfer)  
**Savings:** ~$5-10/mo (eliminating S3 + CloudFront fees)  
**Status:** Code ready, K8s env vars prepared. See `docs/MEDIASERVICE_INFRASTRUCTURE_AUDIT.md`

#### B. Enable Node Auto-scaling

**Current:** Fixed 2 nodes  
**Proposed:** Auto-scale 2-4 nodes based on CPU/memory  
**Cost:** $0 additional (only pay for nodes when needed)  
**Benefit:** Handle traffic spikes (e.g., promotions) without manual intervention

#### C. Add Metrics Server

**Current:** No resource metrics available (`kubectl top` doesn't work)  
**Action:** Install metrics-server for resource monitoring

```bash
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
```

### 3.2 🔶 EVALUATE (May increase cost)

#### D. Scale to 3 Nodes for HA

**Current:** 2 nodes — single node failure impacts 50% of services  
**Proposed:** 3 nodes with pod anti-affinity for critical services  
**Cost:** +$48/mo  
**Benefit:** True high availability — any single node can fail without downtime

#### E. Upgrade Database to HA

**Current:** Single-node managed PostgreSQL  
**Proposed:** 2-node HA with automatic failover  
**Cost:** +$15-30/mo  
**Benefit:** Zero-downtime database maintenance, automatic failover

#### F. Add Monitoring Stack

**Proposed:** Prometheus + Grafana on dedicated small node  
**Cost:** +$24/mo (s-2vcpu-4gb node)  
**Benefit:** Full observability, alerting, dashboards

---

## 4. Resource Utilization Analysis

### Pod Count per Node

- **Total active pods:** 18 services + system pods
- **Per node (avg):** ~9-10 pods
- **Estimated resource usage per service:** ~200-500 MB RAM, 100-250m CPU

### Memory Pressure Check

- 18 services × ~350 MB avg = ~6.3 GB
- Total available: ~16 GB (2 nodes × 8 GB)
- System overhead: ~2 GB
- **Estimated usage: ~45-50% of total capacity** ✅ Healthy

### Scaling Assessment

- Can safely add 5-8 more services before needing a 3rd node
- 33 services at 0/0 replicas — scaling them all up would require 4-5 nodes

---

## 5. Security Assessment

### ✅ Good Practices

- TLS termination at ingress (cert-manager with Let's Encrypt)
- Database SSL required
- K8s secrets for all credentials
- Internal-only gateway (not exposed to internet)
- CORS, CSRF, rate limiting configured

### ⚠️ Improvements Needed

- **No network policies:** All pods can communicate with each other. Should restrict to only necessary service-to-service communication
- **No pod security standards:** Consider enabling PSA (Pod Security Admission) for restricted mode
- **Registry credentials:** Use DO Container Registry instead of personal GHCR PAT
- **Secret rotation:** No automated secret rotation — consider using external-secrets-operator
- **Backup strategy:** Verify DO managed DB has automated backups enabled

---

## 6. Recommended Next Steps

| Priority | Action                        | Est. Time | Cost Impact                  |
| -------- | ----------------------------- | --------- | ---------------------------- |
| 1        | Migrate S3 → DO Spaces        | 2 hours   | -$5/mo                       |
| 2        | Install metrics-server        | 5 min     | $0                           |
| 3        | Configure Google reCAPTCHA    | 30 min    | $0                           |
| 4        | Configure Google OAuth        | 1 hour    | $0                           |
| 5        | Enable node auto-scaling      | 15 min    | $0 (variable)                |
| 6        | Add network policies          | 2 hours   | $0                           |
| 7        | Scale AdvertisingService to 1 | 5 min     | ~$0 (fits on existing nodes) |
| 8        | Configure Sentry              | 30 min    | $0 (free tier)               |
| 9        | Configure Google Analytics    | 15 min    | $0                           |

---

_Report generated on 2026-03-05 during platform infrastructure audit._
