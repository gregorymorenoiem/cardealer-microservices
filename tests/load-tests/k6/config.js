// =============================================================================
// K6 Load Test Configuration — OKLA Marketplace
// =============================================================================
// Target: 500 concurrent users, 10 minutes, P95 < 3s, 0 errors 5xx
// Autoscaling trigger: latency > 2s
// =============================================================================

export const BASE_URL = __ENV.BASE_URL || "http://localhost:8080";

// ── Thresholds ──────────────────────────────────────────────────────────────
export const THRESHOLDS = {
  // Global thresholds
  http_req_duration: [
    "p(95)<3000", // P95 latency < 3 seconds
    "p(99)<5000", // P99 latency < 5 seconds
    "avg<1500", // Average < 1.5 seconds
  ],
  http_req_failed: ["rate<0.01"], // Less than 1% error rate
  http_reqs: ["rate>50"], // At least 50 req/s sustained

  // Per-endpoint thresholds
  "http_req_duration{endpoint:health}": ["p(95)<500"],
  "http_req_duration{endpoint:vehicles_list}": ["p(95)<2000"],
  "http_req_duration{endpoint:auth_login}": ["p(95)<2000"],
  "http_req_duration{endpoint:vehicles_detail}": ["p(95)<2000"],
  "http_req_duration{endpoint:catalog}": ["p(95)<2500"],
  "http_req_duration{endpoint:media}": ["p(95)<3000"],

  // Checks pass rate
  checks: ["rate>0.95"], // 95% of checks must pass
};

// ── Scenario: Pre-launch Load Test ──────────────────────────────────────────
// Simulates 500 concurrent users over 10 minutes with ramp-up/down
export const SCENARIOS = {
  // Phase 1: Smoke test (warm-up)
  smoke: {
    executor: "constant-vus",
    vus: 5,
    duration: "30s",
    startTime: "0s",
    tags: { phase: "smoke" },
  },

  // Phase 2: Ramp up to 500 users
  ramp_up: {
    executor: "ramping-vus",
    startVUs: 10,
    stages: [
      { duration: "1m", target: 100 }, // Ramp to 100 users in 1 min
      { duration: "1m", target: 250 }, // Ramp to 250 users in 1 min
      { duration: "1m", target: 500 }, // Ramp to 500 users in 1 min
    ],
    startTime: "30s",
    tags: { phase: "ramp_up" },
  },

  // Phase 3: Sustained load at 500 users for ~5 minutes
  sustained_load: {
    executor: "constant-vus",
    vus: 500,
    duration: "5m",
    startTime: "3m30s",
    tags: { phase: "sustained" },
  },

  // Phase 4: Spike test (briefly go to 750)
  spike: {
    executor: "ramping-vus",
    startVUs: 500,
    stages: [
      { duration: "30s", target: 750 }, // Spike to 750
      { duration: "1m", target: 750 }, // Hold spike
      { duration: "30s", target: 500 }, // Back to 500
    ],
    startTime: "8m30s",
    tags: { phase: "spike" },
  },

  // Phase 5: Ramp down
  ramp_down: {
    executor: "ramping-vus",
    startVUs: 500,
    stages: [
      { duration: "30s", target: 100 },
      { duration: "30s", target: 0 },
    ],
    startTime: "11m",
    tags: { phase: "ramp_down" },
  },
};

// ── Quick Scenario: 2 minutes for fast validation ───────────────────────────
export const SCENARIOS_QUICK = {
  quick_load: {
    executor: "ramping-vus",
    startVUs: 1,
    stages: [
      { duration: "30s", target: 50 },
      { duration: "1m", target: 50 },
      { duration: "30s", target: 0 },
    ],
    tags: { phase: "quick" },
  },
};

// ── Autoscaling Latency Alert Threshold ─────────────────────────────────────
export const AUTOSCALE_LATENCY_THRESHOLD_MS = 2000; // 2 seconds
