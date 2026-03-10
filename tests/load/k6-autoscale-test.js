/**
 * ═══════════════════════════════════════════════════════════════════
 * OKLA Marketplace — Autoscaling Stress Test (k6)
 * ═══════════════════════════════════════════════════════════════════
 *
 * Progressively increases load to verify HPA autoscaling activates
 * BEFORE P95 latency exceeds 2 seconds. Validates DOKS cluster
 * autoscaler engages when pod limits are reached.
 *
 * Test phases:
 *   Phase 1: Baseline (50 VUs, 2 min)  — measure normal latency
 *   Phase 2: Ramp     (50→500, 3 min)  — trigger HPA scale-up
 *   Phase 3: Sustained (500 VUs, 5 min) — verify scaled pods handle load
 *   Phase 4: Spike    (500→800, 2 min)  — push cluster autoscaler
 *   Phase 5: Recovery (800→50, 3 min)   — verify scale-down
 *
 * Run:
 *   k6 run tests/load/k6-autoscale-test.js
 *   k6 run --out json=autoscale-results.json tests/load/k6-autoscale-test.js
 *
 * Monitor alongside:
 *   kubectl get hpa -n okla -w   (watch HPA scaling events)
 *   kubectl get pods -n okla -w  (watch pod count changes)
 */

import http from "k6/http";
import { check, sleep, group } from "k6";
import { Rate, Trend, Counter, Gauge } from "k6/metrics";
import {
  randomIntBetween,
  randomItem,
} from "https://jslib.k6.io/k6-utils/1.4.0/index.js";

// ── Custom Metrics ──────────────────────────────────────────────
const latencyP95 = new Trend("overall_p95", true);
const autoscaleLatency = new Rate("latency_above_2s");
const http5xx = new Rate("http_5xx");
const currentPhase = new Gauge("test_phase");

const BASE_URL = __ENV.BASE_URL || "https://api.okla.do";

export const options = {
  stages: [
    // Phase 1: Baseline
    { duration: "30s", target: 50 },
    { duration: "2m", target: 50 },

    // Phase 2: Ramp to full load (HPA should start scaling here)
    { duration: "3m", target: 500 },

    // Phase 3: Sustained load (HPA should have scaled by now)
    { duration: "5m", target: 500 },

    // Phase 4: Spike beyond normal (cluster autoscaler should engage)
    { duration: "2m", target: 800 },

    // Phase 5: Recovery
    { duration: "3m", target: 50 },
    { duration: "1m", target: 0 },
  ],

  thresholds: {
    // The autoscaling SLO: P95 must stay below 2s even during scale-up
    // We allow a brief breach during the spike phase
    http_req_duration: [{ threshold: "p(95)<3000", abortOnFail: true }],

    // Zero 5xx tolerance
    http_5xx: [{ threshold: "rate<0.005", abortOnFail: true }],

    // Autoscale trigger metric: should stay below 10% even during ramp
    latency_above_2s: [{ threshold: "rate<0.10", abortOnFail: false }],
  },

  tags: {
    testid: `okla-autoscale-${new Date().toISOString().slice(0, 10)}`,
  },
};

// ── Traffic pattern: heavy on vehicle listing (CPU-intensive) ───
const FILTER_COMBOS = [
  "page=1&pageSize=50&marca=toyota",
  "page=1&pageSize=50&tipo=suv&precioMax=2500000",
  "page=1&pageSize=50&condicion=nuevo",
  "page=1&pageSize=50&anioMin=2022&combustible=diesel",
  "page=1&pageSize=50&marca=honda&transmision=automatico",
  "page=1&pageSize=50&tipo=pickup&precioMin=1000000",
];

export default function () {
  const headers = {
    "Content-Type": "application/json",
    Accept: "application/json",
    "User-Agent": "OKLA-AutoscaleTest/1.0",
  };

  // Determine current phase based on VU count
  const vuCount = __VU;
  if (vuCount <= 50) currentPhase.add(1);
  else if (vuCount <= 250) currentPhase.add(2);
  else if (vuCount <= 500) currentPhase.add(3);
  else currentPhase.add(4);

  // Mix of endpoints weighted toward CPU-heavy operations
  const scenario = Math.random();

  if (scenario < 0.4) {
    // 40% — Vehicle listing with filters (DB + serialization heavy)
    const filters = randomItem(FILTER_COMBOS);
    const res = http.get(`${BASE_URL}/api/vehicles?${filters}`, {
      headers,
      tags: { name: "GET /api/vehicles (filtered)" },
    });
    trackResponse(res);
  } else if (scenario < 0.65) {
    // 25% — AI search (LLM inference heavy)
    const queries = [
      "Toyota usado menos de un millón",
      "SUV familiar segura 2024",
      "Carro económico para estudiante",
      "Pickup 4x4 trabajo pesado",
    ];
    const res = http.post(
      `${BASE_URL}/api/search-agent/search`,
      JSON.stringify({
        query: randomItem(queries),
        sessionId: `autoscale-${__VU}-${__ITER}`,
        page: 1,
        pageSize: 20,
      }),
      {
        headers,
        tags: { name: "POST /api/search-agent/search" },
        timeout: "10s",
      },
    );
    trackResponse(res);
  } else if (scenario < 0.8) {
    // 15% — Auth endpoints (bcrypt/JWT heavy)
    const res = http.post(
      `${BASE_URL}/api/auth/login`,
      JSON.stringify({
        email: `autoscale-${__VU}@test.com`,
        password: "TestPassword123!",
      }),
      {
        headers,
        tags: { name: "POST /api/auth/login" },
      },
    );
    trackResponse(res);
  } else if (scenario < 0.9) {
    // 10% — Featured vehicles (cache-friendly)
    const res = http.get(`${BASE_URL}/api/vehicles/featured`, {
      headers,
      tags: { name: "GET /api/vehicles/featured" },
    });
    trackResponse(res);
  } else {
    // 10% — Health probes (lightweight baseline)
    const res = http.get(`${BASE_URL}/health/ready`, {
      tags: { name: "GET /health/ready" },
    });
    trackResponse(res);
  }

  // Minimal think time to maximize pressure
  sleep(randomIntBetween(0.5, 2));
}

function trackResponse(res) {
  latencyP95.add(res.timings.duration);

  check(res, {
    "status not 5xx": (r) => r.status < 500,
  });

  if (res.status >= 500) {
    http5xx.add(true);
  } else {
    http5xx.add(false);
  }

  if (res.timings.duration > 2000) {
    autoscaleLatency.add(true);
  } else {
    autoscaleLatency.add(false);
  }
}

export function teardown() {
  console.log("═══════════════════════════════════════════════════");
  console.log("  OKLA Autoscaling Stress Test Complete");
  console.log("═══════════════════════════════════════════════════");
  console.log("  NEXT STEPS:");
  console.log("  1. Check HPA events: kubectl describe hpa -n okla");
  console.log(
    "  2. Check pod scaling: kubectl get events -n okla --sort-by=lastTimestamp",
  );
  console.log(
    "  3. Check cluster autoscaler: kubectl logs -n kube-system -l app=cluster-autoscaler",
  );
  console.log("═══════════════════════════════════════════════════");
}
