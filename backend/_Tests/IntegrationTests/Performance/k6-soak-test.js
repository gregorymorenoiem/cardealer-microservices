import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';

// Custom metrics for soak testing
const errorRate = new Rate('errors');
const memoryLeakIndicator = new Trend('response_time_degradation');
const requestsProcessed = new Counter('total_requests');

// Soak test configuration - extended duration at moderate load
export const options = {
    scenarios: {
        soak: {
            executor: 'constant-vus',
            vus: 10,
            duration: '30m',  // 30 minute soak test
            gracefulStop: '1m',
        },
    },
    thresholds: {
        http_req_duration: ['p(95)<500'],     // Performance should stay consistent
        errors: ['rate<0.01'],                 // Very low error tolerance for soak
        http_req_failed: ['rate<0.01'],
    },
};

const BASE_URL = __ENV.GATEWAY_URL || 'http://localhost:5000';

// Track response times over windows to detect degradation
const responseWindows = [];
const WINDOW_SIZE = 100;

export default function () {
    const response = http.get(`${BASE_URL}/health`, {
        headers: {
            'X-Correlation-Id': `k6-soak-${__VU}-${__ITER}`,
        },
        timeout: '30s',
    });
    
    requestsProcessed.add(1);
    
    // Track response time for degradation detection
    responseWindows.push(response.timings.duration);
    if (responseWindows.length > WINDOW_SIZE * 2) {
        responseWindows.shift();
    }
    
    // Calculate degradation (compare recent vs earlier)
    if (responseWindows.length >= WINDOW_SIZE * 2) {
        const earlyAvg = responseWindows.slice(0, WINDOW_SIZE).reduce((a, b) => a + b, 0) / WINDOW_SIZE;
        const recentAvg = responseWindows.slice(-WINDOW_SIZE).reduce((a, b) => a + b, 0) / WINDOW_SIZE;
        const degradation = ((recentAvg - earlyAvg) / earlyAvg) * 100;
        memoryLeakIndicator.add(degradation);
    }
    
    const isSuccess = check(response, {
        'status is 200': (r) => r.status === 200,
        'response time < 500ms': (r) => r.timings.duration < 500,
        'response has body': (r) => r.body && r.body.length > 0,
    });
    
    errorRate.add(!isSuccess);
    
    // Consistent pacing
    sleep(1);
}

export function setup() {
    console.log(`Soak test targeting: ${BASE_URL}`);
    console.log('This test will run for 30 minutes at moderate load.');
    console.log('Monitoring for memory leaks and performance degradation...');
    
    const response = http.get(`${BASE_URL}/health`);
    if (response.status !== 200) {
        throw new Error(`Gateway not accessible: ${response.status}`);
    }
    
    return { 
        startTime: Date.now(),
        initialLatency: response.timings.duration 
    };
}

export function teardown(data) {
    const duration = (Date.now() - data.startTime) / 1000 / 60;
    console.log(`Soak test completed after ${duration.toFixed(2)} minutes`);
    console.log(`Initial latency was: ${data.initialLatency.toFixed(2)}ms`);
}
