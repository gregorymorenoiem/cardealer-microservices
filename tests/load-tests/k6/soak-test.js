import http from "k6/http";
import { check, sleep } from "k6";
import { Rate, Counter } from "k6/metrics";
import { BASE_URL, AUTOSCALE_LATENCY_THRESHOLD_MS } from "./config.js";

// =============================================================================
// K6 Soak Test — OKLA Marketplace
// =============================================================================
// Purpose: Verify system stability under sustained load for extended periods
// Detects memory leaks, connection pool exhaustion, DB connection saturation
// =============================================================================

const errorRate5xx = new Rate("errors_5xx");
const autoscaleAlerts = new Counter("autoscale_latency_alerts");

export const options = {
  scenarios: {
    soak_test: {
      executor: "ramping-vus",
      startVUs: 0,
      stages: [
        { duration: "2m", target: 200 }, // Ramp up
        { duration: "30m", target: 200 }, // Sustained load for 30 minutes
        { duration: "2m", target: 0 }, // Ramp down
      ],
    },
  },
  thresholds: {
    http_req_duration: ["p(95)<3000", "avg<1500"],
    http_req_failed: ["rate<0.01"],
    errors_5xx: ["rate<0.001"],
  },
  summaryTrendStats: [
    "avg",
    "min",
    "med",
    "max",
    "p(90)",
    "p(95)",
    "p(99)",
    "count",
  ],
};

export function setup() {
  console.log(`🧪 OKLA Soak Test — 30 minute sustained load at 200 VUs`);
  console.log(`   Target: ${BASE_URL}`);

  const healthRes = http.get(`${BASE_URL}/health`, { timeout: "10s" });
  if (healthRes.status !== 200) {
    console.error(`❌ API unreachable. Aborting.`);
    return { abort: true };
  }
  return {};
}

export default function (data) {
  if (data && data.abort) {
    sleep(1);
    return;
  }

  // Simulate typical browsing patterns
  const endpoints = [
    { path: "/health", weight: 1 },
    { path: "/api/vehicles?page=1&pageSize=20", weight: 4 },
    { path: `/api/vehicles/${Math.floor(Math.random() * 50) + 1}`, weight: 3 },
    { path: "/api/catalog/brands", weight: 2 },
  ];

  // Weighted random selection
  const totalWeight = endpoints.reduce((s, e) => s + e.weight, 0);
  let rand = Math.random() * totalWeight;
  let selected = endpoints[0];
  for (const ep of endpoints) {
    rand -= ep.weight;
    if (rand <= 0) {
      selected = ep;
      break;
    }
  }

  const res = http.get(`${BASE_URL}${selected.path}`, { timeout: "15s" });

  check(res, {
    "status is not 5xx": (r) => r.status < 500,
    "response time < 3s": (r) => r.timings.duration < 3000,
  });

  if (res.status >= 500) errorRate5xx.add(1);
  else errorRate5xx.add(0);

  if (res.timings.duration > AUTOSCALE_LATENCY_THRESHOLD_MS) {
    autoscaleAlerts.add(1);
  }

  sleep(Math.random() * 3 + 1);
}

export function handleSummary(data) {
  const now = new Date().toISOString().replace(/[:.]/g, "-");
  return {
    stdout: JSON.stringify(
      data.metrics.http_req_duration?.values || {},
      null,
      2,
    ),
    [`./tests/load-tests/k6/reports/soak-test-${now}.json`]: JSON.stringify(
      data,
      null,
      2,
    ),
  };
}
