import http from "k6/http";
import { check, sleep } from "k6";
import { Rate, Trend, Counter } from "k6/metrics";
import { BASE_URL, AUTOSCALE_LATENCY_THRESHOLD_MS } from "./config.js";

// =============================================================================
// K6 Stress Test — OKLA Marketplace
// =============================================================================
// Purpose: Find the breaking point of the system
// Strategy: Gradually increase load beyond 500 VUs until errors appear
// =============================================================================

const errorRate5xx = new Rate("errors_5xx");
const autoscaleAlerts = new Counter("autoscale_latency_alerts");

export const options = {
  scenarios: {
    stress_test: {
      executor: "ramping-vus",
      startVUs: 0,
      stages: [
        { duration: "1m", target: 100 }, // Warm-up
        { duration: "2m", target: 300 }, // Normal load
        { duration: "2m", target: 500 }, // Target load
        { duration: "2m", target: 750 }, // Above target
        { duration: "2m", target: 1000 }, // Stress
        { duration: "1m", target: 1250 }, // Breaking point search
        { duration: "2m", target: 0 }, // Recovery
      ],
    },
  },
  thresholds: {
    http_req_duration: ["p(95)<5000"], // More lenient — we want to find the limit
    http_req_failed: ["rate<0.10"], // Allow up to 10% failure
    errors_5xx: ["rate<0.05"], // Alert at 5% 5xx
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
  console.log(`🔥 OKLA Stress Test — Finding breaking point`);
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

  // High-frequency endpoint tests for stress
  const endpoints = [
    "/health",
    "/api/vehicles?page=1&pageSize=20",
    `/api/vehicles/${Math.floor(Math.random() * 50) + 1}`,
    "/api/catalog/brands",
  ];

  const ep = endpoints[Math.floor(Math.random() * endpoints.length)];
  const res = http.get(`${BASE_URL}${ep}`, {
    timeout: "15s",
    tags: { endpoint: ep.split("?")[0].split("/").slice(0, 4).join("/") },
  });

  check(res, {
    "status is not 5xx": (r) => r.status < 500,
    "response time < 5s": (r) => r.timings.duration < 5000,
  });

  if (res.status >= 500) {
    errorRate5xx.add(1);
  } else {
    errorRate5xx.add(0);
  }

  if (res.timings.duration > AUTOSCALE_LATENCY_THRESHOLD_MS) {
    autoscaleAlerts.add(1);
  }

  sleep(Math.random() * 2);
}

export function handleSummary(data) {
  const now = new Date().toISOString().replace(/[:.]/g, "-");
  const metrics = data.metrics;
  const duration = metrics.http_req_duration
    ? metrics.http_req_duration.values
    : {};

  const report = `
╔══════════════════════════════════════════════════════════════════════════╗
║                    🔥 OKLA Stress Test Report                          ║
╠══════════════════════════════════════════════════════════════════════════╣
║  Date:        ${new Date().toISOString()}
║  Base URL:    ${BASE_URL}
║  Max VUs:     1250 (ramping)
╠══════════════════════════════════════════════════════════════════════════╣

📊 RESULTS
──────────────────
  Total Requests:     ${metrics.http_reqs ? metrics.http_reqs.values.count : "N/A"}
  Request Rate:       ${metrics.http_reqs ? metrics.http_reqs.values.rate.toFixed(2) + " req/s" : "N/A"}
  Failed Rate:        ${metrics.http_req_failed ? (metrics.http_req_failed.values.rate * 100).toFixed(2) + "%" : "N/A"}

⏱️ LATENCY
──────────────────
  P50:  ${duration.med ? duration.med.toFixed(0) + "ms" : "N/A"}
  P95:  ${duration["p(95)"] ? duration["p(95)"].toFixed(0) + "ms" : "N/A"}
  P99:  ${duration["p(99)"] ? duration["p(99)"].toFixed(0) + "ms" : "N/A"}
  Max:  ${duration.max ? duration.max.toFixed(0) + "ms" : "N/A"}

🔴 5xx Rate: ${metrics.errors_5xx ? (metrics.errors_5xx.values.rate * 100).toFixed(2) + "%" : "0%"}
📈 Autoscale Alerts: ${metrics.autoscale_latency_alerts ? metrics.autoscale_latency_alerts.values.count : 0}

╚══════════════════════════════════════════════════════════════════════════╝
`;

  return {
    stdout: report,
    [`./tests/load-tests/k6/reports/stress-test-${now}.json`]: JSON.stringify(
      { metrics: data.metrics, timestamp: new Date().toISOString() },
      null,
      2,
    ),
    [`./tests/load-tests/k6/reports/stress-test-${now}.txt`]: report,
  };
}
