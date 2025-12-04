import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');
const healthCheckDuration = new Trend('health_check_duration');
const requestCounter = new Counter('requests');

// Test configuration
export const options = {
    scenarios: {
        // Smoke test - minimal load
        smoke: {
            executor: 'constant-vus',
            vus: 1,
            duration: '30s',
            startTime: '0s',
            gracefulStop: '5s',
        },
        // Load test - normal expected load
        load: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '30s', target: 10 },  // Ramp up
                { duration: '1m', target: 10 },   // Stay at 10 VUs
                { duration: '30s', target: 20 },  // Ramp up more
                { duration: '1m', target: 20 },   // Stay at 20 VUs
                { duration: '30s', target: 0 },   // Ramp down
            ],
            startTime: '35s',
            gracefulStop: '10s',
        },
        // Stress test - beyond normal capacity
        stress: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '30s', target: 50 },  // Ramp up to 50
                { duration: '1m', target: 50 },   // Stay at 50
                { duration: '30s', target: 100 }, // Ramp to 100
                { duration: '30s', target: 100 }, // Stay at 100
                { duration: '1m', target: 0 },    // Ramp down
            ],
            startTime: '4m45s',
            gracefulStop: '30s',
        },
    },
    thresholds: {
        http_req_duration: ['p(95)<500', 'p(99)<1000'],  // 95% under 500ms, 99% under 1s
        errors: ['rate<0.1'],                            // Error rate under 10%
        http_req_failed: ['rate<0.05'],                  // HTTP failures under 5%
        health_check_duration: ['p(90)<200'],            // Health checks under 200ms
    },
};

// Gateway base URL (configurable via environment)
const BASE_URL = __ENV.GATEWAY_URL || 'http://localhost:5000';

// Test data
const endpoints = [
    { path: '/health', method: 'GET', weight: 40 },
    { path: '/health/ready', method: 'GET', weight: 20 },
    { path: '/health/live', method: 'GET', weight: 20 },
    { path: '/api/vehicles', method: 'GET', weight: 10 },
    { path: '/api/auth/health', method: 'GET', weight: 10 },
];

// Weighted random endpoint selection
function selectEndpoint() {
    const totalWeight = endpoints.reduce((sum, e) => sum + e.weight, 0);
    let random = Math.random() * totalWeight;
    
    for (const endpoint of endpoints) {
        random -= endpoint.weight;
        if (random <= 0) {
            return endpoint;
        }
    }
    return endpoints[0];
}

export default function () {
    const endpoint = selectEndpoint();
    const url = `${BASE_URL}${endpoint.path}`;
    
    const params = {
        headers: {
            'Content-Type': 'application/json',
            'X-Correlation-Id': `k6-${Date.now()}-${__VU}-${__ITER}`,
        },
        timeout: '30s',
    };
    
    const start = Date.now();
    let response;
    
    if (endpoint.method === 'GET') {
        response = http.get(url, params);
    } else if (endpoint.method === 'POST') {
        response = http.post(url, JSON.stringify({}), params);
    }
    
    const duration = Date.now() - start;
    
    // Track health check duration specifically
    if (endpoint.path.includes('/health')) {
        healthCheckDuration.add(duration);
    }
    
    // Increment request counter
    requestCounter.add(1);
    
    // Validate response
    const isSuccess = check(response, {
        'status is 2xx or 3xx': (r) => r.status >= 200 && r.status < 400,
        'response time < 500ms': (r) => r.timings.duration < 500,
        'has response body': (r) => r.body && r.body.length > 0,
    });
    
    // Track errors
    errorRate.add(!isSuccess);
    
    // Random sleep between requests (50-150ms)
    sleep(0.05 + Math.random() * 0.1);
}

// Lifecycle hooks
export function setup() {
    console.log(`Testing Gateway at: ${BASE_URL}`);
    
    // Verify gateway is accessible
    const response = http.get(`${BASE_URL}/health`);
    if (response.status !== 200) {
        throw new Error(`Gateway health check failed with status ${response.status}`);
    }
    
    console.log('Gateway is healthy, starting load test...');
    return { startTime: Date.now() };
}

export function teardown(data) {
    const duration = (Date.now() - data.startTime) / 1000;
    console.log(`Load test completed in ${duration.toFixed(2)} seconds`);
}

// Summary handler
export function handleSummary(data) {
    return {
        'stdout': textSummary(data, { indent: ' ', enableColors: true }),
        'summary.json': JSON.stringify(data, null, 2),
    };
}

function textSummary(data, opts) {
    const summary = [];
    
    summary.push('================================');
    summary.push('K6 Gateway Load Test Summary');
    summary.push('================================');
    summary.push('');
    
    if (data.metrics.http_reqs) {
        summary.push(`Total Requests: ${data.metrics.http_reqs.values.count}`);
        summary.push(`Requests/sec: ${data.metrics.http_reqs.values.rate.toFixed(2)}`);
    }
    
    if (data.metrics.http_req_duration) {
        const dur = data.metrics.http_req_duration.values;
        summary.push('');
        summary.push('Response Times:');
        summary.push(`  Avg: ${dur.avg.toFixed(2)}ms`);
        summary.push(`  Min: ${dur.min.toFixed(2)}ms`);
        summary.push(`  Max: ${dur.max.toFixed(2)}ms`);
        summary.push(`  P90: ${dur['p(90)'].toFixed(2)}ms`);
        summary.push(`  P95: ${dur['p(95)'].toFixed(2)}ms`);
        summary.push(`  P99: ${dur['p(99)'].toFixed(2)}ms`);
    }
    
    if (data.metrics.errors) {
        summary.push('');
        summary.push(`Error Rate: ${(data.metrics.errors.values.rate * 100).toFixed(2)}%`);
    }
    
    summary.push('');
    summary.push('================================');
    
    return summary.join('\n');
}
