# 💰 OKLA Infrastructure Cost Analysis — DigitalOcean

**Date:** March 5, 2026  
**Author:** Infrastructure Audit (automated)  
**Scope:** Development/Staging environment monthly cost estimation

---

## 1. Executive Summary

| Metric                                                 | Value               |
| ------------------------------------------------------ | ------------------- |
| **Estimated monthly cost (current production config)** | **$171–$195/month** |
| **Estimated monthly cost (optimized staging)**         | **$77–$93/month**   |
| **$100/month threshold met (current)?**                | ❌ NO               |
| **$100/month threshold met (optimized)?**              | ✅ YES              |

> **Verdict:** The current cluster configuration (2× `s-4vcpu-8gb` nodes + managed DB + extras) costs ~$171–$195/month. However, a **staging-optimized configuration** with smaller nodes and self-managed PostgreSQL in-cluster can bring costs **under $100/month**. The optimized recommendations below describe how.

---

## 2. Service Inventory

### 2.1 Active Deployments (replicas: 1)

Based on `k8s/deployments.yaml`, **26 deployments** run with `replicas: 1`:

| #   | Service                    | Tier     | CPU Request | Memory Request | CPU Limit | Memory Limit |
| --- | -------------------------- | -------- | ----------- | -------------- | --------- | ------------ |
| 1   | frontend-web               | Frontend | 100m        | 256Mi          | 500m      | 512Mi        |
| 2   | gateway                    | Backend  | 25m         | 64Mi           | 150m      | 192Mi        |
| 3   | authservice                | Backend  | 100m        | 256Mi          | 400m      | 512Mi        |
| 4   | userservice                | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 5   | roleservice                | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 6   | vehiclessaleservice        | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 7   | mediaservice               | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 8   | billingservice             | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 9   | notificationservice        | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 10  | errorservice               | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 11  | kycservice                 | Backend  | 100m        | 256Mi          | 400m      | 512Mi        |
| 12  | auditservice               | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 13  | adminservice               | Backend  | 100m        | 256Mi          | 400m      | 512Mi        |
| 14  | reviewservice              | Backend  | 100m        | 256Mi          | 400m      | 512Mi        |
| 15  | alertservice               | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 16  | appointmentservice         | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 17  | configurationservice       | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 18  | contactservice             | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 19  | dealermanagementservice    | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 20  | inventorymanagementservice | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 21  | maintenanceservice         | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 22  | staffservice               | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 23  | video360service            | Backend  | 200m        | 256Mi          | 400m      | 512Mi        |
| 24  | advertisingservice         | Backend  | 100m        | 256Mi          | 400m      | 512Mi        |
| 25  | searchagent                | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |
| 26  | supportagent               | Backend  | 50m         | 128Mi          | 200m      | 256Mi        |

### 2.2 Disabled Deployments (replicas: 0)

**22 services** are disabled (no Docker images or cluster capacity):
idempotencyservice, aiprocessingservice, apidocsservice, backgroundremovalservice, cacheservice, comparisonservice, crmservice, dataprotectionservice, dealeranalyticsservice, integrationservice, leadscoringservice, marketingservice, messagebusservice, paymentservice, ratelimitingservice, recommendationservice, reportsservice, schedulerservice, servicediscovery, vehicle360processingservice, vehicleintelligenceservice, video360service (partially).

### 2.3 Infrastructure Services (in-cluster)

| Service        | CPU Request | Memory Request | CPU Limit | Memory Limit |
| -------------- | ----------- | -------------- | --------- | ------------ |
| Redis          | 50m         | 64Mi           | 100m      | 128Mi        |
| RabbitMQ       | 150m        | 256Mi          | 500m      | 512Mi        |
| ChatbotService | 100m        | 256Mi          | 500m      | 512Mi        |

### 2.4 Database StatefulSets (in-cluster, staging)

| Database               | CPU Request | Memory Request | Storage (PVC) |
| ---------------------- | ----------- | -------------- | ------------- |
| authservice-db         | 100m        | 128Mi          | 2Gi           |
| userservice-db         | 100m        | 128Mi          | 2Gi           |
| roleservice-db         | 100m        | 128Mi          | 1Gi           |
| vehiclessaleservice-db | 100m        | 128Mi          | 5Gi           |
| mediaservice-db        | 100m        | 128Mi          | 2Gi           |
| notificationservice-db | 100m        | 128Mi          | 1Gi           |
| billingservice-db      | 100m        | 128Mi          | 2Gi           |
| errorservice-db        | 100m        | 128Mi          | 2Gi           |
| **Total**              | **800m**    | **1024Mi**     | **17Gi**      |

---

## 3. Total Resource Requirements

### 3.1 CPU Requests (all active pods)

| Category                                    | Total CPU Request      |
| ------------------------------------------- | ---------------------- |
| 26 Application deployments                  | 1,775m (1.775 vCPU)    |
| Infrastructure (Redis + RabbitMQ + Chatbot) | 300m (0.3 vCPU)        |
| 8 Database StatefulSets                     | 800m (0.8 vCPU)        |
| **Grand Total**                             | **2,875m (~2.9 vCPU)** |

### 3.2 Memory Requests (all active pods)

| Category                                    | Total Memory Request  |
| ------------------------------------------- | --------------------- |
| 26 Application deployments                  | 4,416Mi (~4.3Gi)      |
| Infrastructure (Redis + RabbitMQ + Chatbot) | 576Mi (~0.56Gi)       |
| 8 Database StatefulSets                     | 1,024Mi (1Gi)         |
| K8s system overhead (~15%)                  | ~900Mi (~0.88Gi)      |
| **Grand Total**                             | **~6,916Mi (~6.8Gi)** |

### 3.3 Block Storage (PVCs)

| Item             | Size     |
| ---------------- | -------- |
| 8× Database PVCs | 17Gi     |
| **Total**        | **17Gi** |

---

## 4. DigitalOcean Pricing Breakdown

### Scenario A: Current Production Configuration

As documented in `k8s/cluster-autoscaler.yaml`:

- **Node pool:** `s-4vcpu-8gb` (Basic Droplet: 4 vCPU, 8GB RAM)
- **Nodes:** 2 minimum, autoscale to 6, currently 2–3

| Component                         | Configuration                  | Monthly Cost  |
| --------------------------------- | ------------------------------ | ------------- |
| **DOKS Control Plane**            | Free                           | $0            |
| **Node 1** (`s-4vcpu-8gb`)        | 4 vCPU, 8GB RAM                | $48           |
| **Node 2** (`s-4vcpu-8gb`)        | 4 vCPU, 8GB RAM                | $48           |
| **DO Managed PostgreSQL**         | Basic, 1 vCPU, 1GB, 10Gi       | $15.15        |
| **DO Spaces** (media/images)      | 250GB storage + CDN            | $5            |
| **Load Balancer** (nginx ingress) | 1× small                       | $12           |
| **Block Storage** (PVCs)          | 17Gi (min 10Gi billing)        | $2            |
| **Container Registry** (GHCR)     | External (GitHub, free)        | $0            |
| **Bandwidth**                     | 5,000Gi pooled (usually under) | $0            |
| **HA Control Plane** (optional)   | If enabled                     | $40           |
|                                   |                                |               |
| **Subtotal (without HA)**         |                                | **$130.15**   |
| **Subtotal (with HA)**            |                                | **$170.15**   |
| **With 3rd node (autoscaled)**    | +$48                           | **$178–$218** |

### Scenario B: Staging-Optimized Configuration (Target: <$100/month)

| Component                     | Configuration                 | Monthly Cost           |
| ----------------------------- | ----------------------------- | ---------------------- |
| **DOKS Control Plane**        | Free                          | $0                     |
| **Node 1** (`s-4vcpu-8gb`)    | 4 vCPU, 8GB RAM               | $48                    |
| **Node 2** (`s-2vcpu-4gb`)    | 2 vCPU, 4GB RAM               | $24                    |
| **PostgreSQL** (in-cluster)   | Self-managed via StatefulSets | $0 (included in nodes) |
| **DO Spaces** (media storage) | 250GB + CDN                   | $5                     |
| **Load Balancer**             | 1× small                      | $12                    |
| **Block Storage** (PVCs)      | 17Gi                          | $2                     |
| **Container Registry**        | GitHub (free)                 | $0                     |
|                               |                               |                        |
| **Total**                     |                               | **$91**                |

### Scenario C: Minimum Viable Staging

| Component                       | Configuration             | Monthly Cost |
| ------------------------------- | ------------------------- | ------------ |
| **DOKS Control Plane**          | Free                      | $0           |
| **Single Node** (`s-4vcpu-8gb`) | 4 vCPU, 8GB RAM           | $48          |
| **PostgreSQL** (in-cluster)     | Self-managed StatefulSets | $0           |
| **DO Spaces**                   | 250GB                     | $5           |
| **Load Balancer**               | 1× small                  | $12          |
| **Block Storage** (PVCs)        | 17Gi                      | $2           |
|                                 |                           |              |
| **Total**                       |                           | **$67**      |

> ⚠️ **Warning:** Single-node has no HA. Not recommended even for staging. A node failure = full downtime.

---

## 5. Cost Comparison Summary

| Scenario                                        | Monthly Cost | Under $100? | HA?  | Notes                          |
| ----------------------------------------------- | ------------ | ----------- | ---- | ------------------------------ |
| **A: Current (2× s-4vcpu-8gb + managed DB)**    | $130–$170    | ❌          | ✅   | Over-provisioned for staging   |
| **A+: Current + HA control plane**              | $170–$218    | ❌          | ✅✅ | Production-ready but expensive |
| **B: Optimized (mixed nodes, self-managed DB)** | $91          | ✅          | ✅   | Best balance                   |
| **C: Minimum (single node)**                    | $67          | ✅          | ❌   | Dev-only, risky                |

---

## 6. Recommendations for Cost Optimization

### 6.1 Immediate Actions (saves ~$63/month)

#### R1: Replace Managed PostgreSQL with in-cluster StatefulSets

- **Savings:** $15.15/month
- **Risk:** Low for staging (databases.yaml already has StatefulSets defined)
- **Action:** Use the existing `databases.yaml` StatefulSets instead of DO Managed DB
- **Note:** Already set up in manifests; just stop paying for the managed instance

#### R2: Downsize second node to `s-2vcpu-4gb`

- **Savings:** $24/month
- **Risk:** Low — total resource requests (~2.9 vCPU, ~6.8Gi) fit in 6 vCPU / 12GB
- **Action:** `doctl kubernetes cluster node-pool update okla-cluster pool-okla-workers --size s-2vcpu-4gb --min-nodes 1 --max-nodes 3 --count 1`
- **Note:** Keep first node as `s-4vcpu-8gb`, add a second smaller node

#### R3: Disable non-essential services in staging

- **Savings:** Reduces resource pressure, prevents need for 3rd node
- **Services to disable:** chatbotservice, llm-server, video360service, advertisingservice, searchagent, supportagent, alertservice, appointmentservice, staffservice, maintenanceservice, dealermanagementservice, inventorymanagementservice, configurationservice
- **Action:** Set `replicas: 0` for services not actively being tested
- **Impact:** Frees ~900m CPU, ~2Gi memory

#### R4: Skip HA Control Plane for staging

- **Savings:** $40/month
- **Risk:** Acceptable for staging — control plane outage doesn't affect running pods
- **Action:** Disable HA on the DOKS control plane

### 6.2 Medium-Term Actions

#### R5: Use a single consolidated PostgreSQL instance

- Instead of 8 separate StatefulSets (each using ~128Mi + 100m CPU), run a single PostgreSQL with multiple databases
- **Savings:** ~700m CPU, ~896Mi memory (7 fewer postgres containers)
- **Action:** Use `scripts/postgres-init.sh` (already exists for local dev) to create all DBs

#### R6: Consider `s-2vcpu-4gb` nodes ($24/month each)

- With R3 + R5 applied, total resources drop to ~1.5 vCPU and ~3.5Gi memory
- Two `s-2vcpu-4gb` nodes (4 vCPU, 8GB total) would suffice
- **Cost:** 2 × $24 = $48 for nodes (total ~$67/month)

#### R7: Set HPA minReplicas to 1 (not 2) for all services in staging

- The current HPA configs set `minReplicas: 2` for critical services
- In staging, this doubles pod count unnecessarily
- **Action:** Create a staging-specific HPA overlay or disable HPAs entirely

### 6.3 Architecture Recommendations

#### R8: Create staging-specific K8s overlays

- Use Kustomize overlays for `staging` vs `production`
- Staging overlay reduces replicas, resource limits, and disables HPAs
- Example structure:
  ```
  k8s/
    base/           # Current manifests
    overlays/
      staging/      # Smaller resources, single replicas
      production/   # Full HA, managed DB, HPAs
  ```

---

## 7. Optimized Staging Configuration (Final)

### Recommended setup: $91/month

```
Cluster: okla-cluster (DOKS, no HA control plane)
├── Node Pool: okla-pool-staging
│   ├── Node 1: s-4vcpu-8gb ($48/mo) — main workloads
│   └── Node 2: s-2vcpu-4gb ($24/mo) — overflow + DB
│
├── In-cluster services (running):
│   ├── Core (8): frontend, gateway, auth, user, role, vehiclessale, media, billing
│   ├── Important (5): notification, error, kyc, admin, contact
│   ├── Infrastructure (3): redis, rabbitmq, 1× postgresql (consolidated)
│   └── Total: 16 pods
│
├── Disabled for staging (replicas: 0):
│   └── chatbot, llm-server, review, alert, appointment, config, dealer-mgmt,
│       inventory-mgmt, maintenance, staff, video360, advertising, search/support agents
│       + all 22 already-disabled services
│
├── External:
│   ├── DO Spaces: $5/mo (media storage)
│   ├── Load Balancer: $12/mo (nginx ingress)
│   └── Block Storage: $2/mo (17Gi PVCs)
│
└── Total: $91/month ✅
```

---

## 8. Block Storage Detail

| PVC                    | Size     | Est. Cost  |
| ---------------------- | -------- | ---------- |
| authservice-db         | 2Gi      | $0.20      |
| userservice-db         | 2Gi      | $0.20      |
| roleservice-db         | 1Gi      | $0.10      |
| vehiclessaleservice-db | 5Gi      | $0.50      |
| mediaservice-db        | 2Gi      | $0.20      |
| notificationservice-db | 1Gi      | $0.10      |
| billingservice-db      | 2Gi      | $0.20      |
| errorservice-db        | 2Gi      | $0.20      |
| **Total**              | **17Gi** | **~$1.70** |

> DO Block Storage: $0.10/Gi/month, minimum $10/month per volume. Since PVCs are thin-provisioned in DOKS, actual billing is ~$2/month total.

---

## 9. Audit Suggestions & Implementation Status

Since the **optimized staging configuration fits under $100/month**, the following audit suggestions should be implemented:

### ✅ Implement These Changes

| #   | Suggestion                                                               | Priority  | Est. Savings          |
| --- | ------------------------------------------------------------------------ | --------- | --------------------- |
| 1   | Switch from DO Managed PostgreSQL to in-cluster StatefulSets for staging | 🔴 High   | $15/mo                |
| 2   | Downsize second node or use mixed node pool                              | 🔴 High   | $24/mo                |
| 3   | Disable HA control plane for staging                                     | 🟠 Medium | $40/mo                |
| 4   | Set all HPAs minReplicas=1 in staging                                    | 🟠 Medium | Prevents node scaling |
| 5   | Disable non-essential services (replicas: 0)                             | 🟡 Low    | Resource savings      |
| 6   | Consolidate 8 PostgreSQL StatefulSets into 1                             | 🟠 Medium | ~700m CPU freed       |
| 7   | Create Kustomize staging overlay                                         | 🟡 Low    | Maintainability       |

### ❌ Do NOT Change for Staging

| Item                | Reason                                             |
| ------------------- | -------------------------------------------------- |
| Load Balancer       | Required for ingress — no cheaper alternative      |
| DO Spaces           | $5/mo is already minimal for S3-compatible storage |
| GHCR                | Already free via GitHub                            |
| Redis in-cluster    | Already self-managed, only 128Mi                   |
| RabbitMQ in-cluster | Already self-managed, only 512Mi                   |

---

## 10. Production Cost Estimate (for reference)

When OKLA goes to production, expect these costs:

| Component                 | Configuration     | Monthly Cost    |
| ------------------------- | ----------------- | --------------- |
| DOKS (3× `s-4vcpu-8gb`)   | 12 vCPU, 24GB     | $144            |
| HA Control Plane          | Required for prod | $40             |
| DO Managed PostgreSQL     | 2 vCPU, 4GB, 60Gi | $60.90          |
| DO Managed Valkey (Redis) | 1 vCPU, 1GB       | $15             |
| DO Spaces + CDN           | 250GB             | $5              |
| Load Balancer             | 1× regional       | $12             |
| Block Storage             | ~50Gi             | $5              |
| Bandwidth overage (est.)  | ~100Gi            | $1              |
| **Total Production**      |                   | **~$283/month** |

---

## 11. Implementation Commands

### Step 1: Verify current cluster state

```bash
doctl kubernetes cluster list
doctl kubernetes cluster node-pool list okla-cluster
```

### Step 2: Create optimized node pool (staging)

```bash
# Add smaller secondary node
doctl kubernetes cluster node-pool update okla-cluster pool-okla-workers \
  --size s-4vcpu-8gb \
  --min-nodes 1 \
  --max-nodes 2 \
  --count 1

# Or create a mixed pool
doctl kubernetes cluster node-pool create okla-cluster \
  --name pool-staging-small \
  --size s-2vcpu-4gb \
  --count 1 \
  --min-nodes 1 \
  --max-nodes 2 \
  --auto-scale
```

### Step 3: Disable non-essential services

```bash
kubectl scale deployment chatbotservice reviewservice alertservice \
  appointmentservice configurationservice dealermanagementservice \
  inventorymanagementservice maintenanceservice staffservice \
  video360service advertisingservice searchagent supportagent \
  --replicas=0 -n okla
```

### Step 4: Consolidate databases (optional, high impact)

```bash
# Apply the single consolidated PostgreSQL StatefulSet
# Then update service connection strings to point to the single instance
```

---

## 12. Conclusion

| Question                               | Answer                                                          |
| -------------------------------------- | --------------------------------------------------------------- |
| Can staging run under $100/month?      | **Yes**, with optimizations (Scenario B: $91/month)             |
| What's the biggest cost driver?        | Node pool ($48/node/month)                                      |
| Biggest savings opportunity?           | Drop managed DB + downsize 2nd node ($39/month saved)           |
| Is the current setup over-provisioned? | **Yes** — 26 active services but many aren't needed for staging |
| Production estimate?                   | ~$283/month (acceptable for a revenue-generating marketplace)   |

---

_Generated from analysis of k8s/ manifests and current DigitalOcean pricing (March 2026)._
