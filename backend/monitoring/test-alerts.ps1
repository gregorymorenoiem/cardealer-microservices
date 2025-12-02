# Monitoring Stack - Test Alert Script
# This script simulates conditions to trigger alerts for testing

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("error-rate", "circuit-breaker", "slow-query", "memory", "all")]
    [string]$TestType = "all"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Alert Testing Tool" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

function Test-ErrorRate {
    Write-Host "[Test 1] Simulating High Error Rate..." -ForegroundColor Yellow
    Write-Host "Sending 100 requests with 50% error rate to AuditService..." -ForegroundColor Gray
    
    for ($i = 1; $i -le 100; $i++) {
        if ($i % 2 -eq 0) {
            # Simulate failed request
            Invoke-WebRequest -Uri "http://localhost:5001/api/audit/nonexistent" -Method GET -ErrorAction SilentlyContinue | Out-Null
        } else {
            # Simulate successful request
            Invoke-WebRequest -Uri "http://localhost:5001/health" -Method GET -ErrorAction SilentlyContinue | Out-Null
        }
        if ($i % 10 -eq 0) {
            Write-Host "  Progress: $i/100 requests sent" -ForegroundColor Gray
        }
    }
    
    Write-Host "✓ Error rate test completed" -ForegroundColor Green
    Write-Host "  Expected alert: HighAuditQueryErrorRate (triggers after 5 minutes)" -ForegroundColor Yellow
    Write-Host ""
}

function Test-SlowQuery {
    Write-Host "[Test 2] Simulating Slow Queries..." -ForegroundColor Yellow
    Write-Host "Sending requests with artificial delays..." -ForegroundColor Gray
    
    # This would require the service to have a test endpoint that introduces delay
    Write-Host "⚠ This test requires services to expose /test/slow-query endpoint" -ForegroundColor Yellow
    Write-Host "  Implement endpoint that sleeps for 5+ seconds to trigger alert" -ForegroundColor Gray
    Write-Host ""
}

function Test-CircuitBreaker {
    Write-Host "[Test 3] Testing Circuit Breaker..." -ForegroundColor Yellow
    Write-Host "Sending rapid failed requests to open circuit breaker..." -ForegroundColor Gray
    
    # Send 50 rapid failed requests
    for ($i = 1; $i -le 50; $i++) {
        Invoke-WebRequest -Uri "http://localhost:5001/api/audit/trigger-circuit-breaker" -Method GET -ErrorAction SilentlyContinue | Out-Null
        Start-Sleep -Milliseconds 100
    }
    
    Write-Host "✓ Circuit breaker test completed" -ForegroundColor Green
    Write-Host "  Expected alert: AuditCircuitBreakerOpen (if circuit opens)" -ForegroundColor Yellow
    Write-Host ""
}

function Test-Memory {
    Write-Host "[Test 4] Checking Memory Usage..." -ForegroundColor Yellow
    Write-Host "Current service memory usage:" -ForegroundColor Gray
    
    docker stats --no-stream --format "table {{.Name}}\t{{.MemUsage}}" | Where-Object { $_ -match "auditservice|mediaservice|notificationservice|gateway" }
    
    Write-Host ""
    Write-Host "⚠ Memory alerts trigger at:" -ForegroundColor Yellow
    Write-Host "  • AuditService: >512MB" -ForegroundColor Gray
    Write-Host "  • MediaService: >512MB" -ForegroundColor Gray
    Write-Host "  • NotificationService: >512MB" -ForegroundColor Gray
    Write-Host "  • Gateway: >256MB" -ForegroundColor Gray
    Write-Host ""
}

function Show-AlertStatus {
    Write-Host "Current Alert Status:" -ForegroundColor Cyan
    Write-Host "Querying Prometheus for active alerts..." -ForegroundColor Gray
    Write-Host ""
    
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:9090/api/v1/alerts"
        $alerts = $response.data.alerts
        
        if ($alerts.Count -eq 0) {
            Write-Host "✓ No alerts currently firing" -ForegroundColor Green
        } else {
            Write-Host "Active Alerts:" -ForegroundColor Red
            foreach ($alert in $alerts) {
                Write-Host "  • $($alert.labels.alertname) [$($alert.labels.severity)]" -ForegroundColor Yellow
                Write-Host "    State: $($alert.state)" -ForegroundColor Gray
                Write-Host "    Summary: $($alert.annotations.summary)" -ForegroundColor Gray
                Write-Host ""
            }
        }
    } catch {
        Write-Host "✗ Could not connect to Prometheus. Is it running?" -ForegroundColor Red
    }
}

# Run tests based on parameter
switch ($TestType) {
    "error-rate" { Test-ErrorRate }
    "circuit-breaker" { Test-CircuitBreaker }
    "slow-query" { Test-SlowQuery }
    "memory" { Test-Memory }
    "all" {
        Test-ErrorRate
        Test-CircuitBreaker
        Test-SlowQuery
        Test-Memory
    }
}

# Show current alert status
Show-AlertStatus

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Wait 5-10 minutes for alerts to evaluate" -ForegroundColor White
Write-Host "  2. Check Prometheus: http://localhost:9090/alerts" -ForegroundColor White
Write-Host "  3. Check Alertmanager: http://localhost:9093" -ForegroundColor White
Write-Host "  4. Verify email notifications (if configured)" -ForegroundColor White
Write-Host ""
Write-Host "Note: Some alerts require sustained conditions before firing." -ForegroundColor Gray
Write-Host ""
