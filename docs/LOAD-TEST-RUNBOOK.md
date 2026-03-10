# OKLA Marketplace — Load Test Runbook

> **Pre-Launch Performance Validation**  
> Last updated: 2026-03-09

---

## 📋 Table of Contents

1. [Prerequisites](#prerequisites)
2. [Test Scripts Overview](#test-scripts-overview)
3. [Running Tests](#running-tests)
4. [SLOs and Pass/Fail Criteria](#slos-and-passfail-criteria)
5. [Monitoring During Tests](#monitoring-during-tests)
6. [Interpreting Results](#interpreting-results)
7. [Autoscaling Verification](#autoscaling-verification)
8. [Troubleshooting](#troubleshooting)
9. [Post-Test Checklist](#post-test-checklist)

---

## Prerequisites

### 1. Install k6

```bash
# macOS
brew install k6

# Docker (no install needed)
docker run --rm -i grafana/k6 run - < tests/load/k6-load-test.js

# Linux
sudo gpg -k && sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D68
echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update && sudo apt-get install k6
```

### 2. Environment Variables

```bash
export BASE_URL="https://api.okla.do"       # Production gateway
# export BASE_URL="https://staging-api.okla.do"  # Staging
export TEST_EMAIL="loadtest@okla.do"         # Test account (create in admin panel)
export TEST_PASSWORD="LoadTest2026!"         # Test account password
```

### 3. Verify Cluster Access

```bash
kubectl config use-context do-nyc1-okla-cluster
kubectl get nodes -o wide
kubectl get hpa -n okla
kubectl top pods -n okla --sort-by=cpu
```

---

## Test Scripts Overview

| Script                 | Purpose                          | VUs | Duration | When to Use         |
| ---------------------- | -------------------------------- | --- | -------- | ------------------- |
| `k6-smoke-test.js`     | Verify endpoints alive           | 10  | 2 min    | Before every deploy |
| `k6-load-test.js`      | Full load validation (SLO check) | 500 | 10 min   | Pre-launch, weekly  |
| `k6-autoscale-test.js` | Verify HPA + cluster autoscaler  | 800 | 16 min   | Pre-launch, monthly |

---

## Running Tests

### Smoke Test (Quick Verification)

```bash
k6 run tests/load/k6-smoke-test.js
```

Expected: All checks pass, P95 < 5s, error rate < 10%.

### Full Load Test (500 Concurrent Users)

```bash
# Standard run
k6 run tests/load/k6-load-test.js

# With JSON output for analysis
k6 run --out json=results/load-$(date +%Y%m%d-%H%M).json tests/load/k6-load-test.js

# With custom parameters
k6 run -e VUS=300 -e DURATION=5m tests/load/k6-load-test.js
```

### Autoscaling Stress Test

```bash
# Start monitoring in a separate terminal FIRST:
kubectl get hpa -n okla -w &
kubectl get pods -n okla -w &

# Run the test
k6 run --out json=results/autoscale-$(date +%Y%m%d-%H%M).json tests/load/k6-autoscale-test.js
```

### Run from Docker (CI/CD)

```bash
docker run --rm \
  -e BASE_URL=https://api.okla.do \
  -v $(pwd)/tests/load:/scripts \
  -v $(pwd)/results:/results \
  grafana/k6 run /scripts/k6-load-test.js
```

---

## SLOs and Pass/Fail Criteria

### 🟢 PASS Criteria (ALL must be met)

| SLO                | Metric                        | Threshold  | Abort on Fail |
| ------------------ | ----------------------------- | ---------- | ------------- |
| Latency P95        | `http_req_duration{p95}`      | < 3,000 ms | ✅ Yes        |
| Latency P99        | `http_req_duration{p99}`      | < 5,000 ms | ❌ No         |
| Average Latency    | `http_req_duration{avg}`      | < 1,500 ms | ❌ No         |
| HTTP 5xx Rate      | `http_5xx`                    | < 0.1%     | ✅ Yes        |
| Error Rate         | `errors`                      | < 1%       | ❌ No         |
| Vehicle List P95   | `vehicle_list_latency{p95}`   | < 2,000 ms | ❌ No         |
| Vehicle Detail P95 | `vehicle_detail_latency{p95}` | < 1,500 ms | ❌ No         |
| Auth P95           | `auth_latency{p95}`           | < 2,000 ms | ❌ No         |
| AI Search P95      | `search_latency{p95}`         | < 3,000 ms | ❌ No         |
| Health Check P95   | `health_check_latency{p95}`   | < 500 ms   | ❌ No         |

### 🟡 WARNING Criteria

| SLO                   | Metric                   | Threshold        | Action                 |
| --------------------- | ------------------------ | ---------------- | ---------------------- |
| Autoscale Trigger     | `latency_above_2s`       | > 5% of requests | Verify HPA is scaling  |
| P95 Approaching Limit | `http_req_duration{p95}` | > 2,500 ms       | Investigate bottleneck |

### 🔴 FAIL Criteria (test aborts)

| Condition         | Action Required                                                  |
| ----------------- | ---------------------------------------------------------------- |
| P95 > 3,000 ms    | Profile slow endpoints, check DB queries, verify cache hit rates |
| HTTP 5xx > 0.1%   | Check error logs, identify crashing services                     |
| Test aborts early | Check k6 output for which threshold triggered the abort          |

---

## Monitoring During Tests

### Terminal 1: HPA Watch

```bash
kubectl get hpa -n okla -w
```

Expected behavior:

- At 100-250 VUs: HPAs show TARGETS climbing (CPU > 50%)
- At 250-500 VUs: REPLICAS column should increase for critical services
- Gateway, AuthService, VehiclesSaleService should scale first (minReplicas: 2, maxReplicas: 8)

### Terminal 2: Pod Watch

```bash
kubectl get pods -n okla -w --sort-by=.status.startTime
```

Look for:

- New pods appearing (Pending → ContainerCreating → Running)
- No CrashLoopBackOff during scaling
- No OOMKilled events

### Terminal 3: Events

```bash
kubectl get events -n okla --sort-by=.lastTimestamp -w
```

Look for:

- `SuccessfulRescale` events from HPA
- `ScalingUp` events from cluster autoscaler
- NO `FailedScheduling` events (would mean cluster is full)

### Grafana Dashboards

Open during test:

1. **OKLA Service Overview** — Request rate, error rate, P95 latency
2. **OKLA Resource Metrics** — CPU/memory per pod, GC pauses
3. **OKLA Business KPIs** — Vehicle views, search queries

---

## Interpreting Results

### k6 Summary Output

```
     ✓ vehicles list: status 200
     ✓ vehicles list: P95 < 2s
     ✗ ai search: P95 < 3s
      ↳  95% — ✓ 4750 / ✗ 250

     checks.........................: 97.5%  ✓ 48750  ✗ 1250
     http_req_duration..............: avg=850ms  min=45ms  med=650ms  max=8500ms  p(90)=1800ms  p(95)=2400ms  p(99)=4200ms
     http_5xx.......................: 0.02%  ✓ 10      ✗ 49990
     vehicle_list_latency...........: avg=600ms  p(95)=1200ms
     auth_latency...................: avg=400ms  p(95)=800ms
     search_latency.................: avg=1800ms p(95)=2800ms
```

### Reading the Results

| Metric                       | Good  | Needs Investigation | Critical |
| ---------------------------- | ----- | ------------------- | -------- |
| `http_req_duration p(95)`    | < 2s  | 2-3s                | > 3s     |
| `http_5xx rate`              | 0%    | < 0.1%              | > 0.1%   |
| `checks`                     | > 99% | 95-99%              | < 95%    |
| `vehicle_list_latency p(95)` | < 1s  | 1-2s                | > 2s     |
| `search_latency p(95)`       | < 2s  | 2-3s                | > 3s     |

---

## Autoscaling Verification

### How to Verify HPA Worked

After the autoscale test, run:

```bash
# Check HPA history
kubectl describe hpa -n okla | grep -A5 "Events"

# Check scaling timeline
kubectl get events -n okla --field-selector reason=SuccessfulRescale --sort-by=.lastTimestamp

# Verify current replicas
kubectl get hpa -n okla -o custom-columns=\
NAME:.metadata.name,\
MIN:.spec.minReplicas,\
MAX:.spec.maxReplicas,\
CURRENT:.status.currentReplicas,\
TARGET_CPU:.status.currentMetrics[0].resource.current.averageUtilization
```

### Expected Scaling Timeline

| Time (from test start) | VUs           | Expected HPA Action                                              |
| ---------------------- | ------------- | ---------------------------------------------------------------- |
| T+0:00                 | 0→50          | No scaling (within baseline)                                     |
| T+2:30                 | 50→250        | CPU targets climbing, Gateway may start scaling                  |
| T+5:00                 | 250→500       | Gateway, AuthService, VehiclesSaleService scaled to 4-6 replicas |
| T+5:30                 | 500 sustained | All critical HPAs at target, latency stabilizing                 |
| T+10:00                | 500→800       | Cluster autoscaler adds node if pods can't fit                   |
| T+12:00                | 800→50        | Scale-down starts after 5 min stabilization window               |

### ✅ Autoscaling Passes If

1. HPA `SuccessfulRescale` events appear BEFORE P95 > 2 seconds
2. No `FailedScheduling` events at 500 VUs
3. Cluster autoscaler adds nodes at 800 VUs (if needed)
4. Scale-down happens within 10 minutes after load drops
5. No service disruption during scale-up/down (zero 5xx)

---

## Troubleshooting

### P95 > 3s

```bash
# 1. Identify slow services
kubectl top pods -n okla --sort-by=cpu

# 2. Check if DB is the bottleneck
kubectl exec -n okla deploy/gateway -- curl -s http://localhost:80/health/ready

# 3. Check connection pool exhaustion
kubectl logs -n okla deploy/vehiclessaleservice --tail=100 | grep -i "pool\|timeout\|connection"

# 4. Check if cache is working
kubectl logs -n okla deploy/redis --tail=50
```

### HPA Not Scaling

```bash
# 1. Verify metrics-server is running
kubectl get deployment metrics-server -n kube-system

# 2. Check HPA status
kubectl describe hpa gateway-hpa -n okla

# 3. Check if resource requests are set (required for CPU-based HPA)
kubectl get deployment gateway -n okla -o jsonpath='{.spec.template.spec.containers[0].resources}'
```

### Cluster Autoscaler Not Adding Nodes

```bash
# 1. Check autoscaler logs
kubectl logs -n kube-system -l app=cluster-autoscaler --tail=50

# 2. Check node pool limits in DigitalOcean dashboard
doctl kubernetes cluster node-pool list okla-cluster

# 3. Check if nodes have capacity
kubectl describe nodes | grep -A5 "Allocated resources"
```

---

## Post-Test Checklist

- [ ] All k6 thresholds passed (exit code 0)
- [ ] P95 latency < 3s sustained at 500 VUs
- [ ] Zero HTTP 5xx errors (or < 0.1%)
- [ ] HPA scaled critical services before P95 > 2s
- [ ] No CrashLoopBackOff or OOMKilled during test
- [ ] Cluster autoscaler engaged at 800 VUs (if pod limits reached)
- [ ] Scale-down occurred within 10 minutes after load dropped
- [ ] Results saved to `results/` directory
- [ ] Report added to `docs/LOAD-TEST-REPORT.md`
- [ ] Any performance issues documented as GitHub issues

---

## Scheduled Test Cadence

| Frequency         | Test                               | Purpose                   |
| ----------------- | ---------------------------------- | ------------------------- |
| Every deploy      | Smoke test (10 VUs, 2 min)         | Verify no regression      |
| Weekly (Thursday) | Full load (500 VUs, 10 min)        | Baseline tracking         |
| Monthly           | Autoscale stress (800 VUs, 16 min) | Infrastructure validation |
| Pre-launch        | All three tests                    | Go/no-go decision         |

---

_Maintained by OKLA Platform Engineering_
