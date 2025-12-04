import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate, Trend, Counter, Gauge } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');
const activeVUs = new Gauge('active_vus');
const requestDuration = new Trend('request_duration');
const throughput = new Counter('throughput');

// Soak test configuration - long running test
export const options = {
    scenarios: {
        soak: {
            executor: 'constant-vus',
            vus: 20,                    // Moderate constant load
            duration: '30m',            // 30 minutes soak
            tags: { test_type: 'soak' },
        },
    },
    thresholds: {
        http_req_duration: ['p(95)<500', 'p(99)<1000'], // Performance should stay consistent
        http_req_failed: ['rate<0.01'],                   // Very low failure rate
        errors: ['rate<0.01'],                            // Very low error rate
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

// Test counter to detect memory leaks via increasing response times
let testIteration = 0;

export default function () {
    testIteration++;
    activeVUs.add(__VU);
    
    group('Soak Test - Sustained Load', () => {
        // Health check
        const healthResponse = http.get(`${BASE_URL}/health`);
        
        check(healthResponse, {
            'health is 200': (r) => r.status === 200,
            'health response consistent': (r) => r.timings.duration < 300,
        });
        
        requestDuration.add(healthResponse.timings.duration);
        throughput.add(1);
        
        // Occasional error check
        if (testIteration % 100 === 0) {
            console.log(`Iteration ${testIteration}: Response time = ${healthResponse.timings.duration}ms`);
        }
    });
    
    // Consistent sleep for predictable load
    sleep(1);
}

export function setup() {
    console.log('Starting soak test - this will run for 30 minutes');
    console.log(`Target URL: ${BASE_URL}`);
    console.log(`Start time: ${new Date().toISOString()}`);
    
    // Initial health check
    const response = http.get(`${BASE_URL}/health`);
    if (response.status !== 200) {
        throw new Error('Service not healthy at start');
    }
    
    return { 
        startTime: new Date().toISOString(),
        initialResponseTime: response.timings.duration 
    };
}

export function teardown(data) {
    console.log('Soak test completed');
    console.log(`Test started: ${data.startTime}`);
    console.log(`Test ended: ${new Date().toISOString()}`);
    console.log(`Initial response time: ${data.initialResponseTime}ms`);
}
