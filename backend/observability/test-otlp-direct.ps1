# Test Telemetry via OTLP HTTP (Direct to OTel Collector)
# Sends traces in OTLP format to the collector at port 4318

Write-Host "`nTesting Telemetry Pipeline (OTLP HTTP Direct)" -ForegroundColor Cyan
Write-Host "Sending sample traces to OTel Collector OTLP HTTP endpoint..." -ForegroundColor Yellow
Write-Host "Endpoint: http://localhost:4318/v1/traces`n"

# Function to generate hex string
function Get-RandomHex($length) {
    $chars = '0123456789abcdef'.ToCharArray()
    $result = ""
    for ($i = 0; $i -lt $length; $i++) {
        $result += $chars[(Get-Random -Minimum 0 -Maximum 16)]
    }
    return $result
}

# Function to get Unix nanoseconds timestamp
function Get-UnixNanos {
    $epoch = [DateTime]::new(1970, 1, 1, 0, 0, 0, [DateTimeKind]::Utc)
    $now = [DateTime]::UtcNow
    $timeSpan = $now - $epoch
    # Convert to nanoseconds (multiply milliseconds by 1,000,000)
    return [string]([int64]($timeSpan.TotalMilliseconds * 1000000))
}

# Function to send OTLP trace
function Send-OTLPTrace($serviceName, $spanName, $statusCode, $iteration) {
    $traceId = Get-RandomHex 32  # 16 bytes = 32 hex chars
    $spanId = Get-RandomHex 16   # 8 bytes = 16 hex chars
    $timestamp = Get-UnixNanos
    $durationNs = (Get-Random -Minimum 10 -Maximum 500) * 1000000  # Convert ms to ns
    
    # OTLP JSON format
    $otlpPayload = @{
        resourceSpans = @(
            @{
                resource = @{
                    attributes = @(
                        @{ key = "service.name"; value = @{ stringValue = $serviceName } }
                        @{ key = "service.namespace"; value = @{ stringValue = "cardealer" } }
                        @{ key = "deployment.environment"; value = @{ stringValue = "test" } }
                        @{ key = "test.iteration"; value = @{ intValue = $iteration } }
                    )
                }
                scopeSpans = @(
                    @{
                        scope = @{
                            name = "manual-instrumentation"
                            version = "1.0.0"
                        }
                        spans = @(
                            @{
                                traceId = $traceId
                                spanId = $spanId
                                name = $spanName
                                kind = 1  # SPAN_KIND_INTERNAL
                                startTimeUnixNano = $timestamp
                                endTimeUnixNano = ([string]([int64]$timestamp + $durationNs))
                                attributes = @(
                                    @{ key = "http.method"; value = @{ stringValue = "GET" } }
                                    @{ key = "http.status_code"; value = @{ intValue = $statusCode } }
                                    @{ key = "http.url"; value = @{ stringValue = "http://localhost:5000/$spanName" } }
                                )
                                status = @{
                                    code = if ($statusCode -lt 400) { 0 } else { 2 }  # 0=OK, 2=ERROR
                                }
                            }
                        )
                    }
                )
            }
        )
    } | ConvertTo-Json -Depth 10 -Compress
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:4318/v1/traces" `
            -Method POST `
            -ContentType "application/json" `
            -Body $otlpPayload `
            -UseBasicParsing
        
        Write-Host "[OK] Sent trace: $serviceName - $spanName (TraceID: $($traceId.Substring(0,8))...)" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "[ERROR] Failed to send trace: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Send 5 sample traces to different services
$traces = @(
    @{ Service = "AuthService"; Span = "api/auth/login"; Status = 200 }
    @{ Service = "UserService"; Span = "api/users"; Status = 200 }
    @{ Service = "Gateway"; Span = "api/health"; Status = 200 }
    @{ Service = "ErrorService"; Span = "api/errors"; Status = 201 }
    @{ Service = "MediaService"; Span = "api/media/upload"; Status = 200 }
)

$successCount = 0
for ($i = 0; $i -lt $traces.Count; $i++) {
    $trace = $traces[$i]
    if (Send-OTLPTrace -serviceName $trace.Service -spanName $trace.Span -statusCode $trace.Status -iteration ($i + 1)) {
        $successCount++
    }
    Start-Sleep -Milliseconds 200
}

Write-Host "`nWaiting for traces to be processed..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

# Check OTel Collector metrics
Write-Host "`nChecking OTel Collector metrics..." -ForegroundColor Yellow
try {
    $metrics = Invoke-WebRequest -Uri "http://localhost:8888/metrics" -UseBasicParsing
    $metricsText = $metrics.Content
    
    # Extract trace metrics
    $receivedSpans = if ($metricsText -match 'otelcol_receiver_accepted_spans\{.*?receiver="otlp".*?\}\s+(\d+)') { $matches[1] } else { "0" }
    $sentSpans = if ($metricsText -match 'otelcol_exporter_sent_spans\{.*?exporter="otlp/jaeger".*?\}\s+(\d+)') { $matches[1] } else { "0" }
    
    Write-Host "- Received spans (OTLP): $receivedSpans" -ForegroundColor Cyan
    Write-Host "- Sent spans (to Jaeger): $sentSpans" -ForegroundColor Cyan
}
catch {
    Write-Host "[WARN] Could not fetch collector metrics" -ForegroundColor Yellow
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Traces sent: $successCount / $($traces.Count)" -ForegroundColor $(if ($successCount -eq $traces.Count) { "Green" } else { "Yellow" })

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "NEXT STEPS" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "1. View traces in Jaeger UI:" -ForegroundColor White
Write-Host "   http://localhost:16686" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. Check services dropdown and search for traces from:" -ForegroundColor White
Write-Host "   - AuthService, UserService, Gateway, ErrorService, MediaService" -ForegroundColor Cyan
Write-Host ""
Write-Host "3. If traces appear, telemetry pipeline is working!" -ForegroundColor White
Write-Host "   Next: Instrument real services to send telemetry automatically" -ForegroundColor Yellow
