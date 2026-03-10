# 🏗️ Runbook: Infrastructure Cost Reduction — DigitalOcean $210 Budget

**Trigger:** Alert `InfraCostWarning`, `InfraCostCritical`, or `InfraCostEmergency`  
**Owner:** Platform Team / CTO  
**Last Updated:** 2026-03-08  
**SLA:** Warning → review within 4h | Critical → acknowledge within 1h | Emergency → act immediately

---

## 📊 Budget Overview

| Metric                     | Value             |
| -------------------------- | ----------------- |
| Monthly Budget             | **$210**          |
| Current Baseline           | ~$139/month       |
| Headroom                   | ~$71 (51% buffer) |
| Warning Threshold (80%)    | $168              |
| Critical Threshold (90%)   | $189              |
| Emergency Threshold (100%) | $210              |

### Cost Baseline Breakdown

| Component                   | Monthly Cost | Notes              |
| --------------------------- | ------------ | ------------------ |
| 2× DOKS nodes (s-4vcpu-8gb) | $96          | 8 vCPU, 16 GiB RAM |
| PostgreSQL Managed DB       | ~$30         | Single-node basic  |
| Load Balancer (Ingress)     | $12          | 1× DigitalOcean LB |
| Block Storage (10Gi PVC)    | $1           | LLM model PVC      |
| **Total Baseline**          | **~$139**    |                    |

---

## 🔴 Step-by-Step: Cost Reduction Procedures

### Phase 1: Immediate Savings (No Service Impact) — Saves ~$5–15/mo

Execute these steps as soon as **any** infra cost alert fires:

```bash
# 1. Scale non-critical services to 0 replicas
# These are already scaled to 0 in production but may be accidentally enabled
kubectl scale deployment llm-server --replicas=0 -n okla
kubectl scale deployment video360processingservice --replicas=0 -n okla
kubectl scale deployment backgroundremovalservice --replicas=0 -n okla
kubectl scale deployment apidocsservice --replicas=0 -n okla

# 2. Delete unused PVCs (llm-model-pvc if llm-server is at 0 replicas)
kubectl get pvc -n okla
# Verify llm-model-pvc is unused, then:
# kubectl delete pvc llm-model-pvc -n okla  # Saves $1/mo

# 3. Verify no orphaned DigitalOcean volumes
doctl compute volume list --format ID,Name,Size,DropletIDs
# Delete any volumes not attached to active droplets

# 4. Check for abandoned snapshots
doctl compute snapshot list --format ID,Name,Size,CreatedAt
# Delete snapshots older than 30 days
```

### Phase 2: Service Consolidation (Low-Risk) — Saves ~$10–20/mo

Execute if alert persists at **Warning** for >24h or reaches **Critical**:

```bash
# 1. Reduce replica counts of non-peak services
kubectl scale deployment cacheservice --replicas=0 -n okla        # Redis direct is sufficient
kubectl scale deployment messagebusservice --replicas=0 -n okla   # RabbitMQ direct
kubectl scale deployment servicediscovery --replicas=0 -n okla    # K8s DNS sufficient
kubectl scale deployment configurationservice --replicas=0 -n okla # Merge into AdminService
kubectl scale deployment idempotencyservice --replicas=0 -n okla  # Gateway middleware
kubectl scale deployment ratelimitingservice --replicas=0 -n okla # Gateway middleware

# 2. Reduce active services to single replica (if currently multi-replica)
kubectl get deployments -n okla -o custom-columns=NAME:.metadata.name,REPLICAS:.spec.replicas | grep -v "0$"
# Scale any service with replicas > 1 down to 1 (except gateway):
# kubectl scale deployment <service> --replicas=1 -n okla
```

### Phase 3: Node Reduction (High Impact) — Saves ~$48/mo

Execute **ONLY** if alert reaches **Emergency** ($210+) and Phases 1-2 are insufficient:

> ⚠️ **WARNING:** Reducing from 2 nodes to 1 eliminates high-availability.
> Only do this if workloads fit within 4 vCPU / 8 GiB after Phase 2 consolidation.

```bash
# 1. Verify current resource usage (must fit in 1 node after consolidation)
kubectl top nodes
kubectl top pods -n okla --sort-by=memory

# Total after Phase 2 consolidation should be:
#   CPU: < 3,500m (of 4,000m available on 1 node)
#   RAM: < 6,000 MiB (of 8,192 MiB available on 1 node)

# 2. Cordon the least-loaded node (prevent new pods)
kubectl cordon <node-name>

# 3. Drain the node (evict pods, they reschedule on remaining node)
kubectl drain <node-name> --ignore-daemonsets --delete-emptydir-data

# 4. Scale DOKS node pool to 1
doctl kubernetes cluster node-pool update <cluster-id> <pool-id> --count 1

# 5. Monitor for 30 minutes
kubectl get pods -n okla -w  # Watch for CrashLoopBackOff or Pending
```

### Phase 4: Database Optimization — Saves ~$10–15/mo

Execute if sustained cost pressure continues after Phase 3:

```bash
# 1. Review PostgreSQL managed DB plan
doctl databases list --format ID,Name,Engine,Size,MonthlyPrice
# Consider downgrading from db-s-2vcpu-4gb ($50/mo) to db-s-1vcpu-2gb ($15/mo)
# Only if average connection count < 20 and DB size < 10GB

# 2. Check Redis cache necessity
# If using managed Redis, consider switching to a sidecar container
# This eliminates the managed Redis cost (~$15/mo)
```

---

## 🔄 Rollback Procedures

If cost reduction causes service degradation:

```bash
# Rollback Phase 3: Scale nodes back to 2
doctl kubernetes cluster node-pool update <cluster-id> <pool-id> --count 2

# Rollback Phase 2: Restore replica counts
kubectl scale deployment <service> --replicas=<original-count> -n okla

# Rollback Phase 1: Re-enable services
kubectl scale deployment <service> --replicas=1 -n okla
```

---

## 📋 Decision Matrix

| Alert Level          | Projected Cost | Action                       | Who              | SLA       |
| -------------------- | -------------- | ---------------------------- | ---------------- | --------- |
| **Healthy**          | < $168         | No action                    | —                | —         |
| **Warning** (80%)    | $168–$189      | Phase 1 review               | Platform Team    | 4 hours   |
| **Critical** (90%)   | $189–$210      | Phase 1 + 2 execution        | CTO acknowledges | 1 hour    |
| **Emergency** (100%) | > $210         | Phase 1 + 2 + 3 auto-execute | CTO must approve | Immediate |

---

## 📈 Monitoring & Verification

After executing cost reduction steps:

```bash
# 1. Verify all critical services are running
kubectl get pods -n okla --field-selector=status.phase!=Running

# 2. Check health endpoints
curl -s http://gateway:80/health | jq .status
curl -s http://authservice:80/health | jq .status
curl -s http://contactservice:80/health | jq .status
curl -s http://adminservice:80/health | jq .status

# 3. Monitor Prometheus metric for cost reduction
# okla_infra_cost_usd_monthly should decrease within 24h

# 4. Check DigitalOcean billing directly
doctl balance get
doctl invoice list
```

---

## 🔔 Alert Configuration

| Alert             | Prometheus Rule             | Alertmanager Route       | Notification Channels                     |
| ----------------- | --------------------------- | ------------------------ | ----------------------------------------- |
| Warning ($168)    | `InfraCostWarning`          | `cost-warning` → webhook | Email + Slack                             |
| Critical ($189)   | `InfraCostCritical`         | `cto-alerts` → webhook   | Email + SMS + Teams + Slack               |
| Emergency ($210)  | `InfraCostEmergency`        | `cto-alerts` → webhook   | Email + SMS + Teams + Slack + Auto-action |
| Projected Overrun | `InfraCostProjectedOverrun` | `cost-warning` → webhook | Email + Slack                             |

---

## 📝 Post-Incident Checklist

After resolving a cost alert:

- [ ] Document what caused the cost increase
- [ ] Verify alert has resolved in Alertmanager
- [ ] Update baseline costs if infrastructure changed
- [ ] Schedule architecture review if recurring
- [ ] Update this runbook with lessons learned
- [ ] Log incident in `.github/copilot-audit.log`

---

_Generated: 2026-03-08 — OKLA Platform Team · CONTRA #8 Infrastructure Cost Control_
