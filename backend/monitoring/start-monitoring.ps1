# Monitoring Stack - Start Script
# This script starts the complete monitoring infrastructure for CarDealer microservices

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CarDealer Monitoring Stack - Startup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is running
Write-Host "[1/6] Checking Docker status..." -ForegroundColor Yellow
try {
    docker info | Out-Null
    Write-Host "✓ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

# Check if cardealer-network exists
Write-Host ""
Write-Host "[2/6] Checking network..." -ForegroundColor Yellow
$networkExists = docker network ls --filter name=cardealer-network -q
if (-not $networkExists) {
    Write-Host "Creating cardealer-network..." -ForegroundColor Yellow
    docker network create cardealer-network
    Write-Host "✓ Network created" -ForegroundColor Green
} else {
    Write-Host "✓ Network already exists" -ForegroundColor Green
}

# Validate Prometheus configuration
Write-Host ""
Write-Host "[3/6] Validating Prometheus configuration..." -ForegroundColor Yellow
docker run --rm -v "${PWD}/prometheus:/etc/prometheus" prom/prometheus:v2.48.0 promtool check config /etc/prometheus/prometheus.yml
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Prometheus configuration is valid" -ForegroundColor Green
} else {
    Write-Host "✗ Prometheus configuration has errors" -ForegroundColor Red
    exit 1
}

# Validate alert rules
Write-Host ""
Write-Host "[4/6] Validating alert rules..." -ForegroundColor Yellow
$ruleFiles = Get-ChildItem -Path "./prometheus/rules/*.yml"
$allValid = $true
foreach ($file in $ruleFiles) {
    Write-Host "  Checking $($file.Name)..." -ForegroundColor Gray
    docker run --rm -v "${PWD}/prometheus:/etc/prometheus" prom/prometheus:v2.48.0 promtool check rules "/etc/prometheus/rules/$($file.Name)"
    if ($LASTEXITCODE -ne 0) {
        $allValid = $false
    }
}
if ($allValid) {
    Write-Host "✓ All alert rules are valid ($($ruleFiles.Count) files)" -ForegroundColor Green
} else {
    Write-Host "✗ Some alert rules have errors" -ForegroundColor Red
    exit 1
}

# Validate Alertmanager configuration
Write-Host ""
Write-Host "[5/6] Validating Alertmanager configuration..." -ForegroundColor Yellow
docker run --rm -v "${PWD}/prometheus:/etc/alertmanager" prom/alertmanager:v0.26.0 amtool check-config /etc/alertmanager/alertmanager.yml
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Alertmanager configuration is valid" -ForegroundColor Green
} else {
    Write-Host "✗ Alertmanager configuration has errors" -ForegroundColor Red
    exit 1
}

# Start monitoring stack
Write-Host ""
Write-Host "[6/6] Starting monitoring services..." -ForegroundColor Yellow
docker-compose -f docker-compose.monitoring.yml up -d

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Monitoring stack started successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to start monitoring stack" -ForegroundColor Red
    exit 1
}

# Wait for services to be healthy
Write-Host ""
Write-Host "Waiting for services to be healthy..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check service health
Write-Host ""
Write-Host "Service Status:" -ForegroundColor Cyan
docker-compose -f docker-compose.monitoring.yml ps

# Display access URLs
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Monitoring Stack is Ready!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Access URLs:" -ForegroundColor Yellow
Write-Host "  • Prometheus:    http://localhost:9090" -ForegroundColor White
Write-Host "  • Alertmanager:  http://localhost:9093" -ForegroundColor White
Write-Host "  • Grafana:       http://localhost:3000 (admin/admin123)" -ForegroundColor White
Write-Host ""
Write-Host "Metrics Endpoints:" -ForegroundColor Yellow
Write-Host "  • Node Exporter: http://localhost:9100/metrics" -ForegroundColor White
Write-Host "  • PostgreSQL:    http://localhost:9187/metrics" -ForegroundColor White
Write-Host ""
Write-Host "Alert Rules Loaded:" -ForegroundColor Yellow
Write-Host "  • audit-alerts.yml (10 rules)" -ForegroundColor White
Write-Host "  • media-alerts.yml (10 rules)" -ForegroundColor White
Write-Host "  • notification-alerts.yml (10 rules)" -ForegroundColor White
Write-Host "  • gateway-alerts.yml (5 rules)" -ForegroundColor White
Write-Host "  • auth-alerts.yml (10 rules)" -ForegroundColor White
Write-Host "  • error-alerts.yml (10 rules)" -ForegroundColor White
Write-Host "  • user-alerts.yml (10 rules)" -ForegroundColor White
Write-Host "  • role-alerts.yml (10 rules)" -ForegroundColor White
Write-Host "  • Total: 75 alert rules" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Open Grafana and verify Prometheus datasource" -ForegroundColor White
Write-Host "  2. Import or create dashboards" -ForegroundColor White
Write-Host "  3. Configure Alertmanager email/Slack notifications" -ForegroundColor White
Write-Host "  4. Test alert firing by triggering conditions" -ForegroundColor White
Write-Host ""
Write-Host "Useful Commands:" -ForegroundColor Yellow
Write-Host "  • View logs:    docker logs cardealer-prometheus -f" -ForegroundColor White
Write-Host "  • Stop stack:   docker-compose -f docker-compose.monitoring.yml down" -ForegroundColor White
Write-Host "  • Restart:      docker-compose -f docker-compose.monitoring.yml restart" -ForegroundColor White
Write-Host ""
