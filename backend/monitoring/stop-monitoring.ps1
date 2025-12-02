# Monitoring Stack - Stop Script
# This script stops and optionally removes the monitoring infrastructure

param(
    [switch]$RemoveVolumes = $false
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CarDealer Monitoring Stack - Shutdown" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Stop monitoring stack
Write-Host "Stopping monitoring services..." -ForegroundColor Yellow
if ($RemoveVolumes) {
    Write-Host "Warning: This will remove all data volumes!" -ForegroundColor Red
    $confirm = Read-Host "Are you sure? (yes/no)"
    if ($confirm -eq "yes") {
        docker-compose -f docker-compose.monitoring.yml down -v
        Write-Host "✓ Monitoring stack stopped and volumes removed" -ForegroundColor Green
    } else {
        Write-Host "Operation cancelled" -ForegroundColor Yellow
        exit 0
    }
} else {
    docker-compose -f docker-compose.monitoring.yml down
    Write-Host "✓ Monitoring stack stopped (data volumes preserved)" -ForegroundColor Green
}

Write-Host ""
Write-Host "Monitoring stack has been shut down." -ForegroundColor Green
Write-Host ""
Write-Host "To restart: .\start-monitoring.ps1" -ForegroundColor Yellow
Write-Host "To remove volumes: .\stop-monitoring.ps1 -RemoveVolumes" -ForegroundColor Yellow
Write-Host ""
