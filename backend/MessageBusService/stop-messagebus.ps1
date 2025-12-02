# Stop MessageBusService and dependencies

Write-Host "=================================" -ForegroundColor Cyan
Write-Host "Stopping MessageBusService Stack" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

$backendPath = Split-Path -Parent $PSScriptRoot
Set-Location $backendPath

Write-Host "Stopping services..." -ForegroundColor Yellow
docker-compose stop messagebusservice messagebus-db

Write-Host ""
Write-Host "Services stopped. To remove containers:" -ForegroundColor Yellow
Write-Host "  docker-compose down messagebusservice messagebus-db" -ForegroundColor Gray
Write-Host ""
Write-Host "To remove volumes (data will be lost):" -ForegroundColor Yellow
Write-Host "  docker-compose down -v" -ForegroundColor Gray
