# Test Telemetry Script - Simplified Version
# Sends sample traces via Zipkin format to OpenTelemetry Collector

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing Telemetry Pipeline (Zipkin Format)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$zipkinEndpoint = "http://localhost:9411/api/v2/spans"
$timestamp = [long]([datetime]::UtcNow - [datetime]'1970-01-01').TotalMilliseconds * 1000

Write-Host "Sending sample traces to Zipkin endpoint..." -ForegroundColor Yellow
Write-Host "Endpoint: $zipkinEndpoint" -ForegroundColor Gray
Write-Host ""

$services = @("AuthService", "UserService", "Gateway", "ErrorService", "MediaService")
$operations = @("GET /api/users", "POST /api/auth/login", "GET /api/media", "POST /api/errors", "GET /health")

for ($i = 0; $i -lt 5; $i++) {
    $traceId = -join ((1..16) | ForEach-Object { "{0:x}" -f (Get-Random -Maximum 16) })
    $spanId = -join ((1..8) | ForEach-Object { "{0:x}" -f (Get-Random -Maximum 16) })
    
    $serviceName = $services[$i]
    $operationName = $operations[$i]
    
    $zipkinSpan = @(
        @{
            traceId = $traceId
            id = $spanId
            name = $operationName
            timestamp = $timestamp
            duration = (Get-Random -Minimum 10000 -Maximum 500000)
            kind = "SERVER"
            localEndpoint = @{
                serviceName = $serviceName
                ipv4 = "127.0.0.1"
                port = 8080
            }
            tags = @{
                "http.method" = "GET"
                "http.status_code" = "200"
                "service.namespace" = "cardealer"
                "deployment.environment" = "test"
                "test.iteration" = $i.ToString()
            }
        }
    )
    
    $json = $zipkinSpan | ConvertTo-Json -Depth 10 -Compress
    
    try {
        $response = Invoke-RestMethod -Uri $zipkinEndpoint `
            -Method POST `
            -ContentType "application/json" `
            -Body $json `
            -TimeoutSec 5 `
            -ErrorAction Stop
        
        Write-Host "  [OK] Trace $($i+1) sent: $serviceName - $operationName" -ForegroundColor Green
        Write-Host "       TraceId: $traceId | SpanId: $spanId" -ForegroundColor Gray
    }
    catch {
        Write-Host "  [ERROR] Failed to send trace $($i+1): $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Start-Sleep -Milliseconds 200
}

Write-Host ""
Write-Host "Waiting for traces to be processed..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Verification" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check OTel Collector metrics
Write-Host "Checking OTel Collector metrics..." -ForegroundColor Yellow
try {
    $metrics = Invoke-WebRequest -Uri "http://localhost:8888/metrics" -UseBasicParsing -TimeoutSec 3
    $content = $metrics.Content
    
    # Look for receiver metrics
    if ($content -match 'otelcol_receiver_accepted_spans\{[^}]*\}\s+(\d+)') {
        $spansReceived = $matches[1]
        Write-Host "  [OK] Spans received by collector: $spansReceived" -ForegroundColor Green
    }
    
    # Look for exporter metrics
    if ($content -match 'otelcol_exporter_sent_spans\{[^}]*\}\s+(\d+)') {
        $spansSent = $matches[1]
        Write-Host "  [OK] Spans sent to backends: $spansSent" -ForegroundColor Green
    }
}
catch {
    Write-Host "  [WARN] Could not fetch collector metrics" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Next Steps" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. View traces in Jaeger UI:" -ForegroundColor Yellow
Write-Host "   http://localhost:16686" -ForegroundColor White
Write-Host "   - Select a service from the dropdown" -ForegroundColor Gray
Write-Host "   - Click 'Find Traces' button" -ForegroundColor Gray
Write-Host "   - You should see 5 traces from the test" -ForegroundColor Gray
Write-Host ""
Write-Host "2. View traces in Zipkin UI:" -ForegroundColor Yellow
Write-Host "   http://localhost:9411" -ForegroundColor White
Write-Host "   - Click 'RUN QUERY' to see recent traces" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Check Grafana dashboards:" -ForegroundColor Yellow
Write-Host "   http://localhost:3001 (admin/admin123)" -ForegroundColor White
Write-Host "   - Go to Dashboards > OpenTelemetry Collector" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Query Prometheus metrics:" -ForegroundColor Yellow
Write-Host "   http://localhost:9091" -ForegroundColor White
Write-Host "   - Query: otelcol_receiver_accepted_spans" -ForegroundColor Gray
Write-Host ""
