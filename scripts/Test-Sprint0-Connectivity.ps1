# ===========================================================================
# Sprint 0 - Connectivity Testing Script
# ===========================================================================
# Este script valida la conectividad entre frontend y backend
# Verifica health checks de los servicios principales
# ===========================================================================

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SPRINT 0 - CONNECTIVITY TESTING" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Servicios a verificar
$services = @(
    @{Name="Gateway"; Url="http://localhost:18443/health"; Port=18443},
    @{Name="AuthService"; Url="http://localhost:15085/health"; Port=15085},
    @{Name="ErrorService"; Url="http://localhost:15083/health"; Port=15083},
    @{Name="NotificationService"; Url="http://localhost:15084/health"; Port=15084},
    @{Name="ProductService"; Url="http://localhost:15006/health"; Port=15006},
    @{Name="UserService"; Url="http://localhost:15100/health"; Port=15100},
    @{Name="RoleService"; Url="http://localhost:15101/health"; Port=15101},
    @{Name="MediaService"; Url="http://localhost:15090/health"; Port=15090},
    @{Name="BillingService"; Url="http://localhost:15008/health"; Port=15008}
)

$passed = 0
$failed = 0
$results = @()

Write-Host "Testing services..." -ForegroundColor Yellow
Write-Host ""

foreach ($service in $services) {
    $status = "UNKNOWN"
    $responseTime = "N/A"
    $statusCode = "N/A"
    
    try {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        $response = Invoke-WebRequest -Uri $service.Url -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
        $stopwatch.Stop()
        
        $responseTime = "$($stopwatch.ElapsedMilliseconds)ms"
        $statusCode = $response.StatusCode
        
        if ($response.StatusCode -eq 200) {
            $status = "OK"
            $passed++
            Write-Host "✅ $($service.Name) - $status ($responseTime)" -ForegroundColor Green
        }
        else {
            $status = "FAILED"
            $failed++
            Write-Host "❌ $($service.Name) - HTTP $statusCode ($responseTime)" -ForegroundColor Red
        }
    }
    catch {
        $failed++
        $status = "UNREACHABLE"
        if ($_.Exception.Message -like "*Unable to connect*") {
            Write-Host "❌ $($service.Name) - Service not running (port $($service.Port))" -ForegroundColor Red
        }
        elseif ($_.Exception.Message -like "*timed out*") {
            Write-Host "❌ $($service.Name) - Timeout" -ForegroundColor Red
        }
        else {
            Write-Host "❌ $($service.Name) - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    $results += [PSCustomObject]@{
        Service = $service.Name
        Status = $status
        ResponseTime = $responseTime
        StatusCode = $statusCode
        Port = $service.Port
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total Services: $($services.Count)" -ForegroundColor White
Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor Red
Write-Host ""

# Mostrar tabla de resultados
$results | Format-Table -AutoSize

# Test Gateway routes via Ocelot
if ($passed -gt 0) {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "GATEWAY ROUTING TEST" -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
    
    $gatewayRoutes = @(
        @{Name="Auth via Gateway"; Url="http://localhost:18443/api/auth/health"},
        @{Name="Errors via Gateway"; Url="http://localhost:18443/api/errors/health"},
        @{Name="Products via Gateway"; Url="http://localhost:18443/api/products/health"},
        @{Name="Media via Gateway"; Url="http://localhost:18443/api/media/health"}
    )
    
    $gatewayPassed = 0
    $gatewayFailed = 0
    
    foreach ($route in $gatewayRoutes) {
        try {
            $response = Invoke-WebRequest -Uri $route.Url -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
            if ($response.StatusCode -eq 200) {
                Write-Host "✅ $($route.Name) - OK" -ForegroundColor Green
                $gatewayPassed++
            }
        }
        catch {
            Write-Host "❌ $($route.Name) - FAILED" -ForegroundColor Red
            $gatewayFailed++
        }
    }
    
    Write-Host ""
    Write-Host "Gateway Routes - Passed: $gatewayPassed, Failed: $gatewayFailed" -ForegroundColor $(if($gatewayFailed -eq 0){"Green"}else{"Yellow"})
}

# Frontend connectivity test
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "FRONTEND CONFIGURATION TEST" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$frontendEnv = "frontend/web/original/.env"
if (Test-Path $frontendEnv) {
    Write-Host "✅ Frontend .env file exists" -ForegroundColor Green
    
    # Verificar variables críticas
    $envContent = Get-Content $frontendEnv -Raw
    $criticalVars = @("VITE_API_URL", "VITE_AUTH_SERVICE_URL", "VITE_GOOGLE_MAPS_API_KEY")
    
    foreach ($var in $criticalVars) {
        if ($envContent -match $var) {
            Write-Host "✅ $var configured" -ForegroundColor Green
        }
        else {
            Write-Host "⚠️  $var missing" -ForegroundColor Yellow
        }
    }
}
else {
    Write-Host "❌ Frontend .env file not found" -ForegroundColor Red
    Write-Host "   Run: cp frontend/web/original/.env.example frontend/web/original/.env" -ForegroundColor Yellow
}

# Docker secrets test
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "DOCKER SECRETS TEST" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

if (Test-Path "compose.secrets.yaml") {
    Write-Host "✅ compose.secrets.yaml exists" -ForegroundColor Green
    
    $secretsFiles = @(
        "secrets/jwt_secret_key.txt",
        "secrets/db_password.txt",
        "secrets/rabbitmq_password.txt",
        "secrets/redis_password.txt"
    )
    
    $allSecretsExist = $true
    foreach ($file in $secretsFiles) {
        if (Test-Path $file) {
            Write-Host "✅ $file exists" -ForegroundColor Green
        }
        else {
            Write-Host "❌ $file missing" -ForegroundColor Red
            $allSecretsExist = $false
        }
    }
    
    if ($allSecretsExist) {
        Write-Host "`n✅ All critical secrets configured" -ForegroundColor Green
    }
}
else {
    Write-Host "❌ compose.secrets.yaml not found" -ForegroundColor Red
    Write-Host "   Run: cp compose.secrets.example.yaml compose.secrets.yaml" -ForegroundColor Yellow
}

# Final verdict
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "FINAL VERDICT" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

if ($failed -eq 0) {
    Write-Host "✅ ALL SYSTEMS OPERATIONAL" -ForegroundColor Green
    Write-Host "   Frontend can now connect to backend services" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. cd frontend/web/original" -ForegroundColor White
    Write-Host "2. npm install" -ForegroundColor White
    Write-Host "3. npm run dev" -ForegroundColor White
    Write-Host "4. Open http://localhost:5174" -ForegroundColor White
    exit 0
}
elseif ($passed -gt 0 -and $failed -le 3) {
    Write-Host "⚠️  PARTIAL SUCCESS" -ForegroundColor Yellow
    Write-Host "   Some services are not running" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To start missing services:" -ForegroundColor Cyan
    Write-Host "docker-compose up -d" -ForegroundColor White
    exit 1
}
else {
    Write-Host "❌ CONNECTIVITY FAILED" -ForegroundColor Red
    Write-Host "   Most services are unreachable" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting steps:" -ForegroundColor Cyan
    Write-Host "1. Check if Docker is running: docker ps" -ForegroundColor White
    Write-Host "2. Start services: docker-compose up -d" -ForegroundColor White
    Write-Host "3. Check logs: docker-compose logs -f gateway authservice" -ForegroundColor White
    Write-Host "4. Verify ports are not in use: netstat -an | findstr '18443 15085'" -ForegroundColor White
    exit 2
}
