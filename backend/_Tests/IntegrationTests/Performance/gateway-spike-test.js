import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');
const successfulRequests = new Counter('successful_requests');
const failedRequests = new Counter('failed_requests');
const responseTimes = new Trend('response_times');

// Spike test configuration
export const options = {
    scenarios: {
        spike: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '30s', target: 5 },    // Normal load
                { duration: '10s', target: 100 },  // Spike to 100 users
                { duration: '1m', target: 100 },   // Stay at spike
                { duration: '10s', target: 5 },    // Back to normal
                { duration: '30s', target: 5 },    // Stay at normal
                { duration: '30s', target: 0 },    // Ramp down
            ],
            tags: { test_type: 'spike' },
        },
    },
    thresholds: {
        http_req_duration: ['p(99)<2000'], // 99% of requests should be below 2s during spike
        http_req_failed: ['rate<0.1'],      // Less than 10% failures during spike is acceptable
        errors: ['rate<0.15'],               // Custom error rate below 15%
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export default function () {
    group('Spike Test - Health Endpoint', () => {
        const startTime = new Date();
        const response = http.get(`${BASE_URL}/health`);
        const duration = new Date() - startTime;
        
        const success = check(response, {
            'status is 200': (r) => r.status === 200,
            'response time acceptable': (r) => r.timings.duration < 2000,
        });
        
        if (success) {
            successfulRequests.add(1);
        } else {
            failedRequests.add(1);
        }
        
        errorRate.add(!success);
        responseTimes.add(duration);
    });
    
    // Random short sleep to simulate realistic traffic
    sleep(Math.random() * 0.5);
}

export function setup() {
    console.log('Starting spike test...');
    console.log(`Target URL: ${BASE_URL}`);
    return {};
}

export function teardown(data) {
    console.log('Spike test completed');
}
