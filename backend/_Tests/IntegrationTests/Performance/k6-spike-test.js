import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');
const apiLatency = new Trend('api_latency');

// Spike test configuration - sudden traffic surge
export const options = {
    scenarios: {
        spike: {
            executor: 'ramping-vus',
            startVUs: 1,
            stages: [
                { duration: '10s', target: 1 },    // Baseline
                { duration: '5s', target: 100 },   // Spike to 100 VUs
                { duration: '30s', target: 100 },  // Stay at spike
                { duration: '10s', target: 1 },    // Scale down
                { duration: '30s', target: 1 },    // Recovery period
            ],
            gracefulStop: '30s',
        },
    },
    thresholds: {
        http_req_duration: ['p(95)<2000'],    // Allow higher latency during spike
        errors: ['rate<0.25'],                 // Allow up to 25% errors during spike
        http_req_failed: ['rate<0.25'],
    },
};

const BASE_URL = __ENV.GATEWAY_URL || 'http://localhost:5000';

export default function () {
    const response = http.get(`${BASE_URL}/health`, {
        headers: {
            'X-Correlation-Id': `k6-spike-${__VU}-${__ITER}`,
        },
        timeout: '10s',
    });
    
    apiLatency.add(response.timings.duration);
    
    const isSuccess = check(response, {
        'status is 2xx': (r) => r.status >= 200 && r.status < 300,
        'response time < 2000ms': (r) => r.timings.duration < 2000,
    });
    
    errorRate.add(!isSuccess);
    
    // Minimal sleep to maximize load
    sleep(0.01);
}

export function setup() {
    console.log(`Spike test targeting: ${BASE_URL}`);
    
    const response = http.get(`${BASE_URL}/health`);
    if (response.status !== 200) {
        throw new Error(`Gateway not accessible: ${response.status}`);
    }
    
    console.log('Starting spike test...');
    return { startTime: Date.now() };
}

export function teardown(data) {
    console.log(`Spike test duration: ${(Date.now() - data.startTime) / 1000}s`);
}
