# Observability Stack - Stop Script

param(
    [switch]$RemoveVolumes = $false
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CarDealer Observability Stack - Shutdown" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Stop observability stack
Write-Host "Stopping observability services..." -ForegroundColor Yellow
if ($RemoveVolumes) {
    Write-Host "Warning: This will remove all data volumes!" -ForegroundColor Red
    $confirm = Read-Host "Are you sure? (yes/no)"
    if ($confirm -eq "yes") {
        docker-compose -f docker-compose.observability.yml down -v
        Write-Host "✓ Observability stack stopped and volumes removed" -ForegroundColor Green
    } else {
        Write-Host "Operation cancelled" -ForegroundColor Yellow
        exit 0
    }
} else {
    docker-compose -f docker-compose.observability.yml down
    Write-Host "✓ Observability stack stopped (data volumes preserved)" -ForegroundColor Green
}

Write-Host ""
Write-Host "Observability stack has been shut down." -ForegroundColor Green
Write-Host ""
Write-Host "To restart: .\start-observability.ps1" -ForegroundColor Yellow
Write-Host "To remove volumes: .\stop-observability.ps1 -RemoveVolumes" -ForegroundColor Yellow
Write-Host ""
