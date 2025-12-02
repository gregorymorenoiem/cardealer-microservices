# Test Observability Stack
# Sends sample traces and metrics to verify the pipeline

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Observability Stack - Test Tool" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if OTel Collector is running
Write-Host "[1/4] Checking OTel Collector status..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "http://localhost:13133/health" -Method GET
    Write-Host "✓ OTel Collector is healthy" -ForegroundColor Green
} catch {
    Write-Host "✗ OTel Collector is not responding" -ForegroundColor Red
    Write-Host "  Please start the stack with: .\start-observability.ps1" -ForegroundColor Yellow
    exit 1
}

# Check Jaeger
Write-Host ""
Write-Host "[2/4] Checking Jaeger status..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:16686" -Method GET -UseBasicParsing
    Write-Host "✓ Jaeger UI is accessible" -ForegroundColor Green
} catch {
    Write-Host "✗ Jaeger UI is not responding" -ForegroundColor Red
}

# Check Prometheus
Write-Host ""
Write-Host "[3/4] Checking Prometheus status..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:9091" -Method GET -UseBasicParsing
    Write-Host "✓ Prometheus is accessible" -ForegroundColor Green
} catch {
    Write-Host "✗ Prometheus is not responding" -ForegroundColor Red
}

# Check Grafana
Write-Host ""
Write-Host "[4/4] Checking Grafana status..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:3001" -Method GET -UseBasicParsing
    Write-Host "✓ Grafana is accessible" -ForegroundColor Green
} catch {
    Write-Host "✗ Grafana is not responding" -ForegroundColor Red
}

# Display collector stats
Write-Host ""
Write-Host "OpenTelemetry Collector Statistics:" -ForegroundColor Cyan
try {
    $metrics = Invoke-RestMethod -Uri "http://localhost:8888/metrics" -Method GET
    
    # Parse key metrics
    $spansReceived = ($metrics -split "`n" | Select-String "otelcol_receiver_accepted_spans" | Select-Object -First 1)
    $metricsReceived = ($metrics -split "`n" | Select-String "otelcol_receiver_accepted_metric_points" | Select-Object -First 1)
    
    Write-Host "  Collector is exporting metrics" -ForegroundColor Green
    Write-Host "  View full metrics at: http://localhost:8888/metrics" -ForegroundColor Gray
} catch {
    Write-Host "  ⚠ Could not fetch collector metrics" -ForegroundColor Yellow
}

# Check for traces in Jaeger
Write-Host ""
Write-Host "Checking for traces in Jaeger..." -ForegroundColor Yellow
try {
    $services = Invoke-RestMethod -Uri "http://localhost:16686/api/services" -Method GET
    if ($services.data.Count -gt 0) {
        Write-Host "✓ Found $($services.data.Count) services in Jaeger:" -ForegroundColor Green
        foreach ($service in $services.data) {
            Write-Host "  • $service" -ForegroundColor Gray
        }
    } else {
        Write-Host "⚠ No services found in Jaeger yet" -ForegroundColor Yellow
        Write-Host "  This is normal if you haven't sent any traces yet" -ForegroundColor Gray
    }
} catch {
    Write-Host "⚠ Could not query Jaeger API" -ForegroundColor Yellow
}

# Display summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "To send test traces:" -ForegroundColor Yellow
Write-Host "  1. Start your microservices" -ForegroundColor White
Write-Host "  2. Make some HTTP requests to services" -ForegroundColor White
Write-Host "  3. View traces in Jaeger: http://localhost:16686" -ForegroundColor White
Write-Host ""
Write-Host "Example test request:" -ForegroundColor Yellow
Write-Host '  Invoke-WebRequest -Uri "http://localhost:5001/health" -Method GET' -ForegroundColor Gray
Write-Host ""
Write-Host "View OTel Collector logs:" -ForegroundColor Yellow
Write-Host "  docker logs cardealer-otel-collector -f" -ForegroundColor Gray
Write-Host ""
Write-Host "Debug with zPages:" -ForegroundColor Yellow
Write-Host "  http://localhost:55679/debug/tracez" -ForegroundColor Gray
Write-Host ""
