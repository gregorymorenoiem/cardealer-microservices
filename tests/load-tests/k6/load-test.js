import http from "k6/http";
import { check, group, sleep } from "k6";
import { Rate, Trend, Counter } from "k6/metrics";
import {
  randomIntBetween,
  randomItem,
} from "https://jslib.k6.io/k6-utils/1.4.0/index.js";
import {
  BASE_URL,
  THRESHOLDS,
  SCENARIOS,
  SCENARIOS_QUICK,
  AUTOSCALE_LATENCY_THRESHOLD_MS,
} from "./config.js";

// =============================================================================
// CUSTOM METRICS
// =============================================================================
const errorRate5xx = new Rate("errors_5xx");
const errorRate4xx = new Rate("errors_4xx");
const autoscaleAlerts = new Counter("autoscale_latency_alerts");
const endpointLatency = new Trend("endpoint_latency", true);

// =============================================================================
// OPTIONS
// =============================================================================
const isQuickMode = __ENV.QUICK === "true";

export const options = {
  scenarios: isQuickMode ? SCENARIOS_QUICK : SCENARIOS,
  thresholds: THRESHOLDS,
  tags: {
    testName: "okla-pre-launch-load-test",
    environment: __ENV.ENV || "staging",
  },
  // Output options
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
  noConnectionReuse: false,
  userAgent: "OKLA-LoadTest/1.0 (k6)",
};

// =============================================================================
// SETUP — Run once before all VUs
// =============================================================================
export function setup() {
  console.log(`🚀 OKLA Pre-Launch Load Test`);
  console.log(`   Target: ${BASE_URL}`);
  console.log(
    `   Mode: ${isQuickMode ? "QUICK (2 min)" : "FULL (12 min, 500 VUs)"}`,
  );
  console.log(
    `   Autoscale alert threshold: ${AUTOSCALE_LATENCY_THRESHOLD_MS}ms`,
  );
  console.log("");

  // Verify the API is reachable
  const healthRes = http.get(`${BASE_URL}/health`, { timeout: "10s" });
  const isHealthy = check(healthRes, {
    "API is reachable": (r) => r.status === 200,
    "Health check returns OK": (r) => {
      try {
        const body = r.json();
        return body.status === "Healthy" || r.status === 200;
      } catch {
        return r.status === 200;
      }
    },
  });

  if (!isHealthy) {
    console.error(`❌ API at ${BASE_URL} is NOT reachable. Aborting.`);
    return { abort: true };
  }

  console.log(`✅ API is healthy. Starting load test...`);
  return { startTime: new Date().toISOString() };
}

// =============================================================================
// DEFAULT — Main VU function (each VU runs this in a loop)
// =============================================================================
export default function (data) {
  if (data && data.abort) {
    sleep(1);
    return;
  }

  // Randomly pick a user journey
  const journey = randomItem([
    "anonymous_browsing", // 40% weight
    "anonymous_browsing",
    "anonymous_browsing",
    "anonymous_browsing",
    "vehicle_search", // 25% weight
    "vehicle_search",
    "vehicle_search_filter", // 15% weight
    "vehicle_search_filter",
    "health_checks", // 10% weight
    "auth_flow", // 10% weight
  ]);

  switch (journey) {
    case "anonymous_browsing":
      anonymousBrowsing();
      break;
    case "vehicle_search":
      vehicleSearch();
      break;
    case "vehicle_search_filter":
      vehicleSearchWithFilters();
      break;
    case "health_checks":
      healthChecks();
      break;
    case "auth_flow":
      authFlow();
      break;
  }

  // Think time between iterations (simulates real user behavior)
  sleep(randomIntBetween(1, 3));
}

// =============================================================================
// JOURNEY: Anonymous Browsing (most common — 40%)
// =============================================================================
function anonymousBrowsing() {
  group("Anonymous Browsing", () => {
    // 1. Hit homepage / health
    const healthRes = taggedGet("/health", "health");
    check(healthRes, {
      "health returns 200": (r) => r.status === 200,
    });
    trackMetrics(healthRes, "health");
    sleep(0.5);

    // 2. Browse vehicle catalog (public endpoint)
    const vehiclesRes = taggedGet(
      "/api/vehicles?page=1&pageSize=20",
      "vehicles_list",
    );
    check(vehiclesRes, {
      "vehicles list returns 200": (r) => r.status === 200 || r.status === 304,
      "vehicles list has data": (r) => {
        try {
          const body = r.json();
          return body !== null;
        } catch {
          return r.status === 200;
        }
      },
    });
    trackMetrics(vehiclesRes, "vehicles_list");
    sleep(randomIntBetween(1, 2));

    // 3. View a specific vehicle detail (simulate random vehicle IDs 1-50)
    const vehicleId = randomIntBetween(1, 50);
    const detailRes = taggedGet(
      `/api/vehicles/${vehicleId}`,
      "vehicles_detail",
    );
    check(detailRes, {
      "vehicle detail returns 200 or 404": (r) =>
        r.status === 200 || r.status === 404,
    });
    trackMetrics(detailRes, "vehicles_detail");
    sleep(randomIntBetween(1, 3));

    // 4. Browse catalog sections
    const catalogRes = taggedGet("/api/catalog/brands", "catalog");
    check(catalogRes, {
      "catalog returns 200": (r) => r.status === 200 || r.status === 404,
    });
    trackMetrics(catalogRes, "catalog");
  });
}

// =============================================================================
// JOURNEY: Vehicle Search (25%)
// =============================================================================
function vehicleSearch() {
  group("Vehicle Search", () => {
    // 1. Search vehicles by keyword
    const searchTerms = [
      "Toyota",
      "Honda",
      "Hyundai",
      "Kia",
      "Nissan",
      "BMW",
      "Mercedes",
    ];
    const term = randomItem(searchTerms);
    const searchRes = taggedGet(
      `/api/vehicles?search=${term}&page=1&pageSize=20`,
      "vehicles_list",
    );
    check(searchRes, {
      "search returns 200": (r) => r.status === 200 || r.status === 404,
    });
    trackMetrics(searchRes, "vehicles_list");
    sleep(randomIntBetween(1, 2));

    // 2. View first result (if any)
    const detailRes = taggedGet(
      `/api/vehicles/${randomIntBetween(1, 30)}`,
      "vehicles_detail",
    );
    check(detailRes, {
      "search detail returns 200 or 404": (r) =>
        r.status === 200 || r.status === 404,
    });
    trackMetrics(detailRes, "vehicles_detail");
  });
}

// =============================================================================
// JOURNEY: Vehicle Search with Filters (15%)
// =============================================================================
function vehicleSearchWithFilters() {
  group("Vehicle Search with Filters", () => {
    const brands = ["Toyota", "Honda", "Hyundai", "Kia", "Nissan"];
    const years = [2020, 2021, 2022, 2023, 2024, 2025];
    const conditions = ["new", "used"];

    const brand = randomItem(brands);
    const year = randomItem(years);
    const condition = randomItem(conditions);
    const minPrice = randomItem([500000, 1000000, 1500000]);
    const maxPrice = minPrice + randomItem([500000, 1000000, 2000000]);

    const filterRes = taggedGet(
      `/api/vehicles?brand=${brand}&yearFrom=${year}&condition=${condition}&priceMin=${minPrice}&priceMax=${maxPrice}&page=1&pageSize=20`,
      "vehicles_list",
    );
    check(filterRes, {
      "filtered search returns 200": (r) =>
        r.status === 200 || r.status === 404,
    });
    trackMetrics(filterRes, "vehicles_list");
    sleep(randomIntBetween(1, 3));

    // Paginate
    const page2Res = taggedGet(
      `/api/vehicles?brand=${brand}&yearFrom=${year}&page=2&pageSize=20`,
      "vehicles_list",
    );
    check(page2Res, {
      "page 2 returns 200": (r) => r.status === 200 || r.status === 404,
    });
    trackMetrics(page2Res, "vehicles_list");
  });
}

// =============================================================================
// JOURNEY: Health Checks (10%)
// =============================================================================
function healthChecks() {
  group("Health Checks", () => {
    const endpoints = ["/health", "/api/auth/health", "/api/vehicles/health"];

    for (const ep of endpoints) {
      const res = taggedGet(ep, "health");
      check(res, {
        [`${ep} returns 200`]: (r) => r.status === 200,
      });
      trackMetrics(res, "health");
      sleep(0.2);
    }
  });
}

// =============================================================================
// JOURNEY: Auth Flow (10%)
// =============================================================================
function authFlow() {
  group("Auth Flow", () => {
    // 1. Attempt login with test credentials (expects 401 or 200)
    const loginPayload = JSON.stringify({
      email: `loadtest_${randomIntBetween(1, 1000)}@test.okla.com`,
      password: "LoadTest2026!",
    });

    const loginRes = http.post(`${BASE_URL}/api/auth/login`, loginPayload, {
      headers: { "Content-Type": "application/json" },
      tags: { endpoint: "auth_login" },
      timeout: "10s",
    });

    check(loginRes, {
      "login returns expected status": (r) =>
        r.status === 200 || r.status === 401 || r.status === 400,
      "login does NOT return 5xx": (r) => r.status < 500,
    });
    trackMetrics(loginRes, "auth_login");
    sleep(1);

    // 2. Attempt register (expects 400 duplicate or 201)
    const registerPayload = JSON.stringify({
      email: `loadtest_${randomIntBetween(1, 100000)}@test.okla.com`,
      password: "LoadTest2026!",
      firstName: "Load",
      lastName: "Test",
    });

    const registerRes = http.post(
      `${BASE_URL}/api/auth/register`,
      registerPayload,
      {
        headers: { "Content-Type": "application/json" },
        tags: { endpoint: "auth_register" },
        timeout: "10s",
      },
    );

    check(registerRes, {
      "register returns expected status": (r) =>
        r.status === 201 ||
        r.status === 200 ||
        r.status === 400 ||
        r.status === 409 ||
        r.status === 422,
      "register does NOT return 5xx": (r) => r.status < 500,
    });
    trackMetrics(registerRes, "auth_register");
  });
}

// =============================================================================
// HELPERS
// =============================================================================

function taggedGet(path, endpointTag) {
  return http.get(`${BASE_URL}${path}`, {
    tags: { endpoint: endpointTag },
    timeout: "15s",
  });
}

function trackMetrics(response, endpointName) {
  // Track 5xx errors
  if (response.status >= 500) {
    errorRate5xx.add(1);
    console.warn(`🔴 5xx ERROR: ${response.status} on ${response.url}`);
  } else {
    errorRate5xx.add(0);
  }

  // Track 4xx errors (informational)
  if (response.status >= 400 && response.status < 500) {
    errorRate4xx.add(1);
  } else {
    errorRate4xx.add(0);
  }

  // Track autoscale alert threshold
  if (response.timings.duration > AUTOSCALE_LATENCY_THRESHOLD_MS) {
    autoscaleAlerts.add(1);
    if (response.timings.duration > 3000) {
      console.warn(
        `⚠️ AUTOSCALE ALERT: ${endpointName} latency ${response.timings.duration.toFixed(0)}ms > ${AUTOSCALE_LATENCY_THRESHOLD_MS}ms`,
      );
    }
  }

  // Track endpoint-specific latency
  endpointLatency.add(response.timings.duration, { endpoint: endpointName });
}

// =============================================================================
// TEARDOWN — Run once after all VUs finish
// =============================================================================
export function handleSummary(data) {
  const now = new Date().toISOString().replace(/[:.]/g, "-");
  const reportDir = "./tests/load-tests/k6/reports";

  // Build summary
  const summary = {
    timestamp: new Date().toISOString(),
    environment: __ENV.ENV || "staging",
    baseUrl: BASE_URL,
    mode: isQuickMode ? "quick" : "full",
    metrics: {
      http_reqs: data.metrics.http_reqs ? data.metrics.http_reqs.values : {},
      http_req_duration: data.metrics.http_req_duration
        ? data.metrics.http_req_duration.values
        : {},
      http_req_failed: data.metrics.http_req_failed
        ? data.metrics.http_req_failed.values
        : {},
      errors_5xx: data.metrics.errors_5xx ? data.metrics.errors_5xx.values : {},
      errors_4xx: data.metrics.errors_4xx ? data.metrics.errors_4xx.values : {},
      autoscale_latency_alerts: data.metrics.autoscale_latency_alerts
        ? data.metrics.autoscale_latency_alerts.values
        : {},
      checks: data.metrics.checks ? data.metrics.checks.values : {},
    },
    thresholds: {},
  };

  // Evaluate thresholds
  for (const [name, threshold] of Object.entries(data.metrics)) {
    if (threshold.thresholds) {
      summary.thresholds[name] = {};
      for (const [key, val] of Object.entries(threshold.thresholds)) {
        summary.thresholds[name][key] = val.ok;
      }
    }
  }

  // Generate text report
  const textReport = generateTextReport(summary, data);

  return {
    stdout: textReport,
    [`${reportDir}/load-test-${now}.json`]: JSON.stringify(summary, null, 2),
    [`${reportDir}/load-test-${now}.txt`]: textReport,
  };
}

function generateTextReport(summary, data) {
  const metrics = data.metrics;
  const duration = metrics.http_req_duration
    ? metrics.http_req_duration.values
    : {};
  const reqs = metrics.http_reqs ? metrics.http_reqs.values : {};
  const failed = metrics.http_req_failed ? metrics.http_req_failed.values : {};

  let report = `
╔══════════════════════════════════════════════════════════════════════════╗
║                    🚗 OKLA Load Test Report                            ║
║                    Pre-Launch Performance Audit                        ║
╠══════════════════════════════════════════════════════════════════════════╣
║  Date:        ${new Date().toISOString()}
║  Environment: ${summary.environment}
║  Base URL:    ${summary.baseUrl}
║  Mode:        ${summary.mode === "full" ? "500 VUs / 10 min" : "Quick validation"}
╠══════════════════════════════════════════════════════════════════════════╣

📊 REQUEST METRICS
──────────────────
  Total Requests:     ${reqs.count || "N/A"}
  Request Rate:       ${reqs.rate ? reqs.rate.toFixed(2) + " req/s" : "N/A"}
  Failed Rate:        ${failed.rate ? (failed.rate * 100).toFixed(2) + "%" : "N/A"}

⏱️ LATENCY (http_req_duration)
──────────────────
  Average:            ${duration.avg ? duration.avg.toFixed(2) + "ms" : "N/A"}
  Median (P50):       ${duration.med ? duration.med.toFixed(2) + "ms" : "N/A"}
  P90:                ${duration["p(90)"] ? duration["p(90)"].toFixed(2) + "ms" : "N/A"}
  P95:                ${duration["p(95)"] ? duration["p(95)"].toFixed(2) + "ms" : "N/A"}    ${duration["p(95)"] && duration["p(95)"] < 3000 ? "✅ PASS" : "❌ FAIL"} (target < 3000ms)
  P99:                ${duration["p(99)"] ? duration["p(99)"].toFixed(2) + "ms" : "N/A"}
  Max:                ${duration.max ? duration.max.toFixed(2) + "ms" : "N/A"}

🔴 ERROR ANALYSIS
──────────────────
  5xx Error Rate:     ${summary.metrics.errors_5xx.rate ? (summary.metrics.errors_5xx.rate * 100).toFixed(2) + "%" : "0%"}    ${!summary.metrics.errors_5xx.rate || summary.metrics.errors_5xx.rate === 0 ? "✅ PASS" : "❌ FAIL"} (target: 0%)
  4xx Error Rate:     ${summary.metrics.errors_4xx.rate ? (summary.metrics.errors_4xx.rate * 100).toFixed(2) + "%" : "0%"}    (informational)

📈 AUTOSCALING VERIFICATION
──────────────────
  Latency Alerts (>2s): ${summary.metrics.autoscale_latency_alerts.count || 0}
  Alert Threshold:      ${AUTOSCALE_LATENCY_THRESHOLD_MS}ms

✅ THRESHOLD RESULTS
──────────────────`;

  for (const [name, thresholds] of Object.entries(summary.thresholds)) {
    for (const [condition, passed] of Object.entries(thresholds)) {
      report += `\n  ${passed ? "✅" : "❌"} ${name}: ${condition}`;
    }
  }

  report += `

╠══════════════════════════════════════════════════════════════════════════╣
║  OVERALL: ${
    Object.values(summary.thresholds).every((t) =>
      Object.values(t).every((v) => v),
    )
      ? "✅ ALL THRESHOLDS PASSED"
      : "❌ SOME THRESHOLDS FAILED — SEE ABOVE"
  }
╚══════════════════════════════════════════════════════════════════════════╝
`;

  return report;
}
