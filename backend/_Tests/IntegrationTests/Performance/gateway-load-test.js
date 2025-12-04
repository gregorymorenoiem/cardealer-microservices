import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');
const healthCheckDuration = new Trend('health_check_duration');
const authEndpointDuration = new Trend('auth_endpoint_duration');

// Test configuration
export const options = {
    scenarios: {
        // Smoke test - quick validation
        smoke: {
            executor: 'constant-vus',
            vus: 1,
            duration: '30s',
            startTime: '0s',
            tags: { test_type: 'smoke' },
        },
        // Load test - normal load
        load: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '1m', target: 10 },  // Ramp up
                { duration: '3m', target: 10 },  // Stay at 10 users
                { duration: '1m', target: 0 },   // Ramp down
            ],
            startTime: '30s',
            tags: { test_type: 'load' },
        },
        // Stress test - heavy load
        stress: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '2m', target: 50 },   // Ramp up
                { duration: '5m', target: 50 },   // Stay at 50 users
                { duration: '2m', target: 100 },  // Increase to 100
                { duration: '5m', target: 100 },  // Stay at 100 users
                { duration: '2m', target: 0 },    // Ramp down
            ],
            startTime: '6m',
            tags: { test_type: 'stress' },
        },
    },
    thresholds: {
        http_req_duration: ['p(95)<500'], // 95% of requests should be below 500ms
        http_req_failed: ['rate<0.01'],   // Less than 1% of requests should fail
        errors: ['rate<0.05'],             // Custom error rate below 5%
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

// Main test function
export default function () {
    // Health check tests
    group('Health Checks', () => {
        const healthResponse = http.get(`${BASE_URL}/health`);
        
        const healthCheck = check(healthResponse, {
            'health status is 200': (r) => r.status === 200,
            'health response time < 200ms': (r) => r.timings.duration < 200,
            'health body contains status': (r) => r.body && r.body.includes('Healthy'),
        });
        
        errorRate.add(!healthCheck);
        healthCheckDuration.add(healthResponse.timings.duration);
    });

    sleep(1);

    // API endpoint tests
    group('API Endpoints', () => {
        // Test CORS preflight
        const corsResponse = http.options(`${BASE_URL}/api/test`, null, {
            headers: {
                'Origin': 'http://localhost:3000',
                'Access-Control-Request-Method': 'GET',
            },
        });
        
        check(corsResponse, {
            'CORS preflight handled': (r) => r.status !== 500,
        });
    });

    sleep(0.5);
}

// Setup function - runs once before the test
export function setup() {
    console.log(`Testing against: ${BASE_URL}`);
    
    // Verify the service is up
    const response = http.get(`${BASE_URL}/health`);
    if (response.status !== 200) {
        throw new Error(`Service not healthy: ${response.status}`);
    }
    
    return { startTime: new Date().toISOString() };
}

// Teardown function - runs once after the test
export function teardown(data) {
    console.log(`Test started at: ${data.startTime}`);
    console.log(`Test ended at: ${new Date().toISOString()}`);
}
