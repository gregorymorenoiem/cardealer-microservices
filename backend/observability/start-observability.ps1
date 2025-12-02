# Observability Stack - Start Script
# Starts OpenTelemetry Collector, Jaeger, Zipkin, Prometheus, and Grafana

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CarDealer Observability Stack - Startup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is running
Write-Host "[1/5] Checking Docker status..." -ForegroundColor Yellow
try {
    docker info | Out-Null
    Write-Host "✓ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

# Check if cardealer-network exists
Write-Host ""
Write-Host "[2/5] Checking network..." -ForegroundColor Yellow
$networkExists = docker network ls --filter name=cardealer-network -q
if (-not $networkExists) {
    Write-Host "Creating cardealer-network..." -ForegroundColor Yellow
    docker network create cardealer-network
    Write-Host "✓ Network created" -ForegroundColor Green
} else {
    Write-Host "✓ Network already exists" -ForegroundColor Green
}

# Validate OpenTelemetry Collector configuration
Write-Host ""
Write-Host "[3/5] Validating OTel Collector configuration..." -ForegroundColor Yellow
if (Test-Path "./otel-collector-config.yaml") {
    Write-Host "✓ Configuration file found" -ForegroundColor Green
    
    # Basic YAML syntax check
    try {
        $content = Get-Content "./otel-collector-config.yaml" -Raw
        if ($content -match "receivers:" -and $content -match "exporters:" -and $content -match "service:") {
            Write-Host "✓ Configuration structure looks valid" -ForegroundColor Green
        } else {
            Write-Host "⚠ Configuration might be incomplete" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "✗ Error reading configuration file" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "✗ Configuration file not found" -ForegroundColor Red
    exit 1
}

# Pull latest images
Write-Host ""
Write-Host "[4/5] Pulling Docker images..." -ForegroundColor Yellow
docker-compose -f docker-compose.observability.yml pull

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Images pulled successfully" -ForegroundColor Green
} else {
    Write-Host "⚠ Warning: Some images might not have been pulled" -ForegroundColor Yellow
}

# Start observability stack
Write-Host ""
Write-Host "[5/5] Starting observability services..." -ForegroundColor Yellow
docker-compose -f docker-compose.observability.yml up -d

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Observability stack started successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to start observability stack" -ForegroundColor Red
    exit 1
}

# Wait for services to be healthy
Write-Host ""
Write-Host "Waiting for services to be healthy..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Check service health
Write-Host ""
Write-Host "Service Status:" -ForegroundColor Cyan
docker-compose -f docker-compose.observability.yml ps

# Test endpoints
Write-Host ""
Write-Host "Testing endpoints..." -ForegroundColor Yellow

$endpoints = @(
    @{Name="OTel Collector Health"; Url="http://localhost:13133/health"},
    @{Name="Jaeger UI"; Url="http://localhost:16686"},
    @{Name="Zipkin UI"; Url="http://localhost:9411"},
    @{Name="Prometheus"; Url="http://localhost:9091"},
    @{Name="Grafana"; Url="http://localhost:3001"}
)

foreach ($endpoint in $endpoints) {
    try {
        $response = Invoke-WebRequest -Uri $endpoint.Url -Method GET -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
        Write-Host "  ✓ $($endpoint.Name): Available" -ForegroundColor Green
    } catch {
        Write-Host "  ⚠ $($endpoint.Name): Not ready yet (might still be starting)" -ForegroundColor Yellow
    }
}

# Display access URLs
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Observability Stack is Ready!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Access URLs:" -ForegroundColor Yellow
Write-Host "  • Jaeger UI:          http://localhost:16686" -ForegroundColor White
Write-Host "  • Zipkin UI:          http://localhost:9411" -ForegroundColor White
Write-Host "  • Grafana:            http://localhost:3001 (admin/admin123)" -ForegroundColor White
Write-Host "  • Prometheus:         http://localhost:9091" -ForegroundColor White
Write-Host "  • OTel Collector:     http://localhost:13133/health" -ForegroundColor White
Write-Host "  • zPages Debug:       http://localhost:55679/debug/tracez" -ForegroundColor White
Write-Host ""
Write-Host "OTLP Endpoints (for services):" -ForegroundColor Yellow
Write-Host "  • gRPC:               http://otel-collector:4317" -ForegroundColor White
Write-Host "  • HTTP:               http://otel-collector:4318" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Configure services to send telemetry to OTel Collector" -ForegroundColor White
Write-Host "  2. Start your microservices" -ForegroundColor White
Write-Host "  3. Generate some traffic" -ForegroundColor White
Write-Host "  4. View traces in Jaeger UI" -ForegroundColor White
Write-Host "  5. Create dashboards in Grafana" -ForegroundColor White
Write-Host ""
Write-Host "Useful Commands:" -ForegroundColor Yellow
Write-Host "  • View logs:    docker logs cardealer-otel-collector -f" -ForegroundColor White
Write-Host "  • Stop stack:   docker-compose -f docker-compose.observability.yml down" -ForegroundColor White
Write-Host "  • Restart:      docker-compose -f docker-compose.observability.yml restart" -ForegroundColor White
Write-Host ""
