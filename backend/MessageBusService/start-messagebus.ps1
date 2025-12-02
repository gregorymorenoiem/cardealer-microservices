# Start MessageBusService with dependencies
Write-Host "=================================" -ForegroundColor Cyan
Write-Host "Starting MessageBusService Stack" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to backend directory
$backendPath = Split-Path -Parent $PSScriptRoot
Set-Location $backendPath

# Start dependencies
Write-Host "[1/3] Starting dependencies..." -ForegroundColor Yellow
docker-compose up -d rabbitmq messagebus-db consul
Start-Sleep -Seconds 20
Write-Host ""

# Build and start MessageBusService
Write-Host "[2/3] Starting MessageBusService..." -ForegroundColor Yellow
docker-compose up -d --build messagebusservice
Start-Sleep -Seconds 15
Write-Host ""

# Check status
Write-Host "[3/3] Service Status:" -ForegroundColor Yellow
docker-compose ps rabbitmq messagebus-db consul messagebusservice
Write-Host ""

Write-Host "=================================" -ForegroundColor Cyan
Write-Host "Access Points" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host "MessageBusService API:     http://localhost:5009" -ForegroundColor White
Write-Host "Swagger UI:                http://localhost:5009/swagger" -ForegroundColor White
Write-Host "RabbitMQ Management:       http://localhost:15672 (guest/guest)" -ForegroundColor White
Write-Host "Consul UI:                 http://localhost:8500" -ForegroundColor White
Write-Host ""
Write-Host "To test the service run:" -ForegroundColor Yellow
Write-Host "  .\MessageBusService\test-messagebus.ps1" -ForegroundColor Gray
