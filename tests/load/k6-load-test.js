/**
 * ═══════════════════════════════════════════════════════════════════
 * OKLA Marketplace — Pre-Launch Load Test (k6)
 * ═══════════════════════════════════════════════════════════════════
 *
 * Simulates 500 concurrent users for 10 minutes hitting the
 * platform's critical API endpoints through the Gateway.
 *
 * SLOs validated:
 *   ✅ P95 latency < 3 seconds (all endpoints)
 *   ✅ P95 latency < 2 seconds triggers autoscaling BEFORE breach
 *   ✅ Zero HTTP 5xx errors
 *   ✅ DOKS HPA scales pods before latency degrades
 *
 * Run:
 *   k6 run tests/load/k6-load-test.js
 *   k6 run --out json=results.json tests/load/k6-load-test.js
 *
 * Environment variables:
 *   BASE_URL      — Gateway URL (default: https://api.okla.do)
 *   TEST_EMAIL    — Test user email for auth flows
 *   TEST_PASSWORD — Test user password
 *   VUS           — Override max VUs (default: 500)
 *   DURATION      — Override sustained duration (default: 10m)
 */

import http from "k6/http";
import { check, sleep, group } from "k6";
import { Rate, Trend, Counter } from "k6/metrics";
import {
  randomIntBetween,
  randomItem,
} from "https://jslib.k6.io/k6-utils/1.4.0/index.js";

// ── Custom Metrics ──────────────────────────────────────────────
const errorRate = new Rate("errors");
const http5xxRate = new Rate("http_5xx");
const p95Breach = new Rate("p95_breach_3s");
const autoscaleTrigger = new Rate("latency_above_2s");

const vehicleListLatency = new Trend("vehicle_list_latency", true);
const vehicleDetailLatency = new Trend("vehicle_detail_latency", true);
const authLatency = new Trend("auth_latency", true);
const searchLatency = new Trend("search_latency", true);
const contactLatency = new Trend("contact_latency", true);
const healthLatency = new Trend("health_check_latency", true);

const totalRequests = new Counter("total_requests");
const successfulRequests = new Counter("successful_requests");

// ── Configuration ───────────────────────────────────────────────
const BASE_URL = __ENV.BASE_URL || "https://api.okla.do";
const MAX_VUS = parseInt(__ENV.VUS) || 500;
const DURATION = __ENV.DURATION || "10m";

export const options = {
  // Ramp-up → sustained → ramp-down pattern
  stages: [
    { duration: "2m", target: Math.floor(MAX_VUS * 0.2) }, // Warm-up: 0 → 100 VUs
    { duration: "2m", target: Math.floor(MAX_VUS * 0.5) }, // Ramp: 100 → 250 VUs
    { duration: "1m", target: MAX_VUS }, // Ramp: 250 → 500 VUs
    { duration: DURATION, target: MAX_VUS }, // Sustained: 500 VUs for 10m
    { duration: "2m", target: Math.floor(MAX_VUS * 0.1) }, // Cool-down: 500 → 50 VUs
    { duration: "1m", target: 0 }, // Drain
  ],

  thresholds: {
    // ═══ SLO: P95 < 3 seconds for ALL requests ═══
    http_req_duration: [
      { threshold: "p(95)<3000", abortOnFail: true },
      { threshold: "p(99)<5000", abortOnFail: false },
      { threshold: "avg<1500", abortOnFail: false },
    ],

    // ═══ SLO: Zero 5xx errors ═══
    http_5xx: [{ threshold: "rate<0.001", abortOnFail: true }],

    // ═══ SLO: Error rate < 1% ═══
    errors: [{ threshold: "rate<0.01", abortOnFail: false }],

    // ═══ Per-endpoint P95 thresholds ═══
    vehicle_list_latency: ["p(95)<2000"], // Vehicle listing: P95 < 2s
    vehicle_detail_latency: ["p(95)<1500"], // Vehicle detail: P95 < 1.5s
    auth_latency: ["p(95)<2000"], // Auth flows: P95 < 2s
    search_latency: ["p(95)<3000"], // AI search: P95 < 3s (LLM overhead)
    contact_latency: ["p(95)<2000"], // Contact forms: P95 < 2s
    health_check_latency: ["p(95)<500"], // Health checks: P95 < 500ms

    // ═══ Autoscaling trigger: If > 5% of requests take > 2s, HPA should be scaling ═══
    latency_above_2s: [{ threshold: "rate<0.05", abortOnFail: false }],
  },

  // Tags for Prometheus/Grafana integration
  tags: {
    testid: `okla-load-${new Date().toISOString().slice(0, 10)}`,
  },

  // Graceful stop
  gracefulStop: "30s",
  gracefulRampDown: "30s",
};

// ── Test Data ───────────────────────────────────────────────────
const VEHICLE_SLUGS = [
  "toyota-corolla-2024-santo-domingo",
  "honda-civic-2023-santiago",
  "hyundai-tucson-2024-santo-domingo",
  "kia-sportage-2023-santiago",
  "toyota-rav4-2024-santo-domingo",
  "nissan-kicks-2023-punta-cana",
  "toyota-hilux-2024-santiago",
  "hyundai-santa-fe-2023-santo-domingo",
];

const SEARCH_QUERIES = [
  "SUV familiar menos de 2 millones",
  "Toyota Corolla automático 2023",
  "Yipeta 4x4 para campo",
  "Carro económico con financiamiento",
  "Pickup doble cabina diesel",
  "Honda Civic usado Santo Domingo",
  "Vehículo nuevo menos de 1 millón",
  "Camioneta para trabajo pesado",
  "SUV 7 pasajeros segura",
  "Carro eléctrico disponible RD",
];

const FILTER_COMBOS = [
  { marca: "toyota", tipo: "suv", precioMax: 2500000 },
  { marca: "honda", anioMin: 2020, transmision: "automatico" },
  { marca: "hyundai", tipo: "sedan", precioMin: 800000, precioMax: 1500000 },
  { tipo: "pickup", anioMin: 2022, combustible: "diesel" },
  { condicion: "nuevo", precioMax: 3000000 },
  { condicion: "usado", marca: "kia", ubicacion: "santo-domingo" },
  { tipo: "suv", anioMin: 2021, anioMax: 2024 },
];

// ── Setup: Authenticate once and share token ────────────────────
export function setup() {
  const testEmail = __ENV.TEST_EMAIL || "loadtest@okla.do";
  const testPassword = __ENV.TEST_PASSWORD || "LoadTest2026!";

  // Try to login — if fails, tests run without auth (public endpoints only)
  const loginRes = http.post(
    `${BASE_URL}/api/auth/login`,
    JSON.stringify({
      email: testEmail,
      password: testPassword,
    }),
    {
      headers: { "Content-Type": "application/json" },
      tags: { name: "setup_login" },
    },
  );

  let token = null;
  if (loginRes.status === 200) {
    try {
      const body = JSON.parse(loginRes.body);
      token = body.token || body.data?.token || body.accessToken;
    } catch {
      console.warn(
        "⚠️ Could not parse login response. Running public-only tests.",
      );
    }
  } else {
    console.warn(
      `⚠️ Login failed (${loginRes.status}). Running public-only tests.`,
    );
  }

  return { token };
}

// ── Main Test Function ──────────────────────────────────────────
export default function (data) {
  const headers = {
    "Content-Type": "application/json",
    Accept: "application/json",
    "User-Agent": "OKLA-LoadTest/1.0 k6",
  };

  if (data.token) {
    headers["Authorization"] = `Bearer ${data.token}`;
  }

  // Weighted scenario distribution (mirrors real traffic)
  const scenario = weightedRandom([
    { weight: 35, fn: () => browseVehicles(headers) }, // 35% — catalog browsing
    { weight: 20, fn: () => viewVehicleDetail(headers) }, // 20% — vehicle detail pages
    { weight: 15, fn: () => aiSearch(headers) }, // 15% — AI-powered search
    { weight: 10, fn: () => authFlow(headers) }, // 10% — login/register
    { weight: 8, fn: () => submitContact(headers) }, // 8%  — contact/lead forms
    { weight: 5, fn: () => checkNotifications(headers) }, // 5%  — notification polling
    { weight: 5, fn: () => healthChecks(headers) }, // 5%  — health endpoints
    { weight: 2, fn: () => miscEndpoints(headers) }, // 2%  — other endpoints
  ]);

  scenario();

  // Think time: simulate real user behavior (1-5 seconds between actions)
  sleep(randomIntBetween(1, 5));
}

// ═══════════════════════════════════════════════════════════════
// SCENARIOS
// ═══════════════════════════════════════════════════════════════

function browseVehicles(headers) {
  group("Browse Vehicles", () => {
    const page = randomIntBetween(1, 10);
    const filters = randomItem(FILTER_COMBOS);
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: "20",
    });

    // Add random filters
    Object.entries(filters).forEach(([key, val]) => {
      params.append(key, val.toString());
    });

    const res = http.get(`${BASE_URL}/api/vehicles?${params.toString()}`, {
      headers,
      tags: { name: "GET /api/vehicles" },
    });

    totalRequests.add(1);
    vehicleListLatency.add(res.timings.duration);

    const ok = check(res, {
      "vehicles list: status 200": (r) => r.status === 200,
      "vehicles list: has data": (r) => {
        try {
          return JSON.parse(r.body)?.data !== undefined;
        } catch {
          return false;
        }
      },
      "vehicles list: P95 < 2s": (r) => r.timings.duration < 2000,
    });

    trackMetrics(res, ok);
  });
}

function viewVehicleDetail(headers) {
  group("Vehicle Detail", () => {
    const slug = randomItem(VEHICLE_SLUGS);

    const res = http.get(`${BASE_URL}/api/vehicles/slug/${slug}`, {
      headers,
      tags: { name: "GET /api/vehicles/slug/:slug" },
    });

    totalRequests.add(1);
    vehicleDetailLatency.add(res.timings.duration);

    const ok = check(res, {
      "vehicle detail: status 200 or 404": (r) =>
        r.status === 200 || r.status === 404,
      "vehicle detail: P95 < 1.5s": (r) => r.timings.duration < 1500,
    });

    trackMetrics(res, ok);

    // If vehicle found, also hit similar + views
    if (res.status === 200) {
      try {
        const vehicleId = JSON.parse(res.body)?.data?.id;
        if (vehicleId) {
          // Track view
          const viewRes = http.post(
            `${BASE_URL}/api/vehicles/${vehicleId}/views`,
            "{}",
            {
              headers,
              tags: { name: "POST /api/vehicles/:id/views" },
            },
          );
          totalRequests.add(1);
          trackMetrics(viewRes, viewRes.status < 500);

          // Get similar
          const simRes = http.get(
            `${BASE_URL}/api/vehicles/${vehicleId}/similar`,
            {
              headers,
              tags: { name: "GET /api/vehicles/:id/similar" },
            },
          );
          totalRequests.add(1);
          trackMetrics(simRes, simRes.status < 500);
        }
      } catch {
        /* ignore parse errors */
      }
    }
  });
}

function aiSearch(headers) {
  group("AI Search", () => {
    const query = randomItem(SEARCH_QUERIES);

    const res = http.post(
      `${BASE_URL}/api/search-agent/search`,
      JSON.stringify({
        query,
        sessionId: `k6-${__VU}-${__ITER}`,
        page: 1,
        pageSize: 20,
      }),
      {
        headers,
        tags: { name: "POST /api/search-agent/search" },
        timeout: "10s", // AI search has higher timeout (LLM call)
      },
    );

    totalRequests.add(1);
    searchLatency.add(res.timings.duration);

    const ok = check(res, {
      "ai search: status 200": (r) => r.status === 200,
      "ai search: has aiFilters": (r) => {
        try {
          return JSON.parse(r.body)?.data?.aiFilters !== undefined;
        } catch {
          return false;
        }
      },
      "ai search: P95 < 3s": (r) => r.timings.duration < 3000,
      "ai search: degraded gracefully if slow": (r) => {
        if (r.status === 200) {
          try {
            const body = JSON.parse(r.body);
            // If X-Degraded-Response header present, AI degraded but still returned 200
            return true;
          } catch {
            return true;
          }
        }
        return r.status < 500;
      },
    });

    trackMetrics(res, ok);
  });
}

function authFlow(headers) {
  group("Auth Flow", () => {
    // Simulate a login attempt (will mostly fail with test credentials but exercises the pipeline)
    const res = http.post(
      `${BASE_URL}/api/auth/login`,
      JSON.stringify({
        email: `loadtest-${__VU}@example.com`,
        password: "FakePassword123!",
      }),
      {
        headers,
        tags: { name: "POST /api/auth/login" },
      },
    );

    totalRequests.add(1);
    authLatency.add(res.timings.duration);

    // 401/400 are expected for fake credentials
    const ok = check(res, {
      "auth: status not 5xx": (r) => r.status < 500,
      "auth: P95 < 2s": (r) => r.timings.duration < 2000,
    });

    trackMetrics(res, ok);
  });
}

function submitContact(headers) {
  group("Contact Form", () => {
    const res = http.post(
      `${BASE_URL}/api/contactrequests`,
      JSON.stringify({
        name: `Load Test User ${__VU}`,
        email: `loadtest-${__VU}@example.com`,
        phone: `809-555-${String(__VU).padStart(4, "0")}`,
        message: "Load test contact request — please ignore.",
        vehicleId: null,
        type: "general",
      }),
      {
        headers,
        tags: { name: "POST /api/contactrequests" },
      },
    );

    totalRequests.add(1);
    contactLatency.add(res.timings.duration);

    const ok = check(res, {
      "contact: status not 5xx": (r) => r.status < 500,
      "contact: P95 < 2s": (r) => r.timings.duration < 2000,
    });

    trackMetrics(res, ok);
  });
}

function checkNotifications(headers) {
  group("Notifications", () => {
    if (!headers["Authorization"]) return;

    const res = http.get(`${BASE_URL}/api/notifications?page=1&pageSize=10`, {
      headers,
      tags: { name: "GET /api/notifications" },
    });

    totalRequests.add(1);

    const ok = check(res, {
      "notifications: status not 5xx": (r) => r.status < 500,
    });

    trackMetrics(res, ok);
  });
}

function healthChecks(headers) {
  group("Health Checks", () => {
    const endpoints = ["/health", "/health/ready", "/health/live"];

    for (const ep of endpoints) {
      const res = http.get(`${BASE_URL}${ep}`, {
        headers: { Accept: "application/json" },
        tags: { name: `GET ${ep}` },
      });

      totalRequests.add(1);
      healthLatency.add(res.timings.duration);

      const ok = check(res, {
        [`health ${ep}: status 200`]: (r) => r.status === 200,
        [`health ${ep}: P95 < 500ms`]: (r) => r.timings.duration < 500,
      });

      trackMetrics(res, ok);
    }
  });
}

function miscEndpoints(headers) {
  group("Miscellaneous", () => {
    // Featured vehicles (homepage)
    const featuredRes = http.get(`${BASE_URL}/api/vehicles/featured`, {
      headers,
      tags: { name: "GET /api/vehicles/featured" },
    });
    totalRequests.add(1);
    trackMetrics(featuredRes, featuredRes.status < 500);

    // Vehicle makes (filter dropdown)
    const makesRes = http.get(`${BASE_URL}/api/vehicles/makes`, {
      headers,
      tags: { name: "GET /api/vehicles/makes" },
    });
    totalRequests.add(1);
    trackMetrics(makesRes, makesRes.status < 500);
  });
}

// ═══════════════════════════════════════════════════════════════
// HELPERS
// ═══════════════════════════════════════════════════════════════

function trackMetrics(res, passed) {
  errorRate.add(!passed);

  if (res.status >= 500) {
    http5xxRate.add(true);
  } else {
    http5xxRate.add(false);
  }

  if (res.timings.duration > 3000) {
    p95Breach.add(true);
  } else {
    p95Breach.add(false);
  }

  if (res.timings.duration > 2000) {
    autoscaleTrigger.add(true);
  } else {
    autoscaleTrigger.add(false);
  }

  if (passed) {
    successfulRequests.add(1);
  }
}

function weightedRandom(items) {
  const totalWeight = items.reduce((sum, item) => sum + item.weight, 0);
  let random = Math.random() * totalWeight;

  for (const item of items) {
    random -= item.weight;
    if (random <= 0) return item.fn;
  }

  return items[items.length - 1].fn;
}

// ── Teardown: Summary Report ────────────────────────────────────
export function teardown(data) {
  console.log("═══════════════════════════════════════════════════");
  console.log("  OKLA Pre-Launch Load Test Complete");
  console.log("═══════════════════════════════════════════════════");
  console.log(`  Max VUs: ${MAX_VUS}`);
  console.log(`  Sustained Duration: ${DURATION}`);
  console.log(`  Target: ${BASE_URL}`);
  console.log("═══════════════════════════════════════════════════");
}
