# Test CacheService with Docker
Write-Host "Starting CacheService with Redis..." -ForegroundColor Cyan

# Navigate to backend folder
Set-Location -Path "c:\Users\gmoreno\source\repos\cardealer\backend"

# Stop any running containers
Write-Host "`nStopping existing containers..." -ForegroundColor Yellow
docker-compose down cacheservice redis

# Build and start services
Write-Host "`nBuilding and starting CacheService + Redis..." -ForegroundColor Green
docker-compose up -d --build redis cacheservice

# Wait for health checks
Write-Host "`nWaiting for services to be healthy..." -ForegroundColor Cyan
Start-Sleep -Seconds 10

# Check container status
Write-Host "`nContainer Status:" -ForegroundColor Magenta
docker ps --filter "name=redis" --filter "name=cacheservice" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# Test Redis connectivity
Write-Host "`nTesting Redis connectivity..." -ForegroundColor Cyan
docker exec redis redis-cli ping

# Test CacheService health endpoint
Write-Host "`nTesting CacheService health endpoint..." -ForegroundColor Cyan
Start-Sleep -Seconds 5
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5095/health" -Method GET
    Write-Host "Health check response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 3
} catch {
    Write-Host "Health check failed: $_" -ForegroundColor Red
}

# Test cache operations
Write-Host "`nTesting cache SET operation..." -ForegroundColor Cyan
try {
    $setBody = @{
        key = "test:docker"
        value = '{"message":"Hello from Docker!"}'
        tenantId = "tenant-1"
        ttlSeconds = 300
    } | ConvertTo-Json

    $setResponse = Invoke-RestMethod -Uri "http://localhost:5095/api/cache" -Method POST -Body $setBody -ContentType "application/json"
    Write-Host "SET response:" -ForegroundColor Green
    $setResponse | ConvertTo-Json -Depth 3
} catch {
    Write-Host "SET operation failed: $_" -ForegroundColor Red
}

# Test cache GET operation
Write-Host "`nTesting cache GET operation..." -ForegroundColor Cyan
Start-Sleep -Seconds 2
try {
    $getResponse = Invoke-RestMethod -Uri "http://localhost:5095/api/cache/test:docker?tenantId=tenant-1" -Method GET
    Write-Host "GET response:" -ForegroundColor Green
    $getResponse | ConvertTo-Json -Depth 3
} catch {
    Write-Host "GET operation failed: $_" -ForegroundColor Red
}

# Test statistics endpoint
Write-Host "`nTesting statistics endpoint..." -ForegroundColor Cyan
try {
    $statsResponse = Invoke-RestMethod -Uri "http://localhost:5095/api/statistics" -Method GET
    Write-Host "Statistics:" -ForegroundColor Green
    $statsResponse | ConvertTo-Json -Depth 3
} catch {
    Write-Host "Statistics failed: $_" -ForegroundColor Red
}

# Show logs
Write-Host "`nCacheService logs (last 20 lines):" -ForegroundColor Magenta
docker logs --tail 20 cacheservice

Write-Host "`nCacheService is running!" -ForegroundColor Green
Write-Host "Swagger UI: http://localhost:5095/swagger" -ForegroundColor Cyan
Write-Host "Health Check: http://localhost:5095/health" -ForegroundColor Cyan
Write-Host "`nTo stop: docker-compose down cacheservice redis" -ForegroundColor Yellow
