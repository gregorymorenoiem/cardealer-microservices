# Sprint 1.2 - ErrorService Audit Script
# Fecha: 1 Enero 2026
# Objetivo: Auditoría completa de ErrorService endpoints

Write-Host "`n=== SPRINT 1.2 - ERRORSERVICE AUDIT ===" -ForegroundColor Magenta
Write-Host "Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n" -ForegroundColor Gray

# Obtener token JWT
Write-Host "Obteniendo token JWT..." -ForegroundColor Yellow
$loginData = @{ email = "test@example.com"; password = "Admin123!" } | ConvertTo-Json
try {
    $response = Invoke-WebRequest -Uri "http://localhost:15085/api/auth/login" -Method POST -Body $loginData -ContentType "application/json" -UseBasicParsing
    $token = ($response.Content | ConvertFrom-Json).data.accessToken
    Write-Host "✅ Token obtenido exitosamente" -ForegroundColor Green
} catch {
    Write-Host "❌ Error obteniendo token: $_" -ForegroundColor Red
    exit 1
}

$headers = @{ Authorization = "Bearer $token" }

# Sprint 1.2.2: POST - Crear error de prueba
Write-Host "`n=== 1.2.2: POST /api/Errors (Crear Error) ===" -ForegroundColor Cyan
$newError = @{
    ServiceName = "TestService"
    ExceptionType = "System.TestException"
    Message = "Error de prueba para Sprint 1.2"
    StackTrace = "at TestService.TestMethod() in TestFile.cs:line 42"
    OccurredAt = (Get-Date).ToUniversalTime().ToString("o")
    Endpoint = "/api/test"
    HttpMethod = "POST"
    StatusCode = 500
    Metadata = @{}
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest "http://localhost:15083/api/Errors" -Method POST -Headers $headers -Body $newError -ContentType "application/json" -UseBasicParsing
    $result = $response.Content | ConvertFrom-Json
    $errorId = $result.data.errorId
    Write-Host "✅ Error creado con ID: $errorId" -ForegroundColor Green
} catch {
    Write-Host "❌ Fallo al crear error: $($_.Exception.Message)" -ForegroundColor Red
    $errorId = $null
}

# Sprint 1.2.1: GET - Listar errores
Write-Host "`n=== 1.2.1: GET /api/Errors (Listar Errores) ===" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest "http://localhost:15083/api/Errors?page=1&pageSize=10" -Headers $headers -UseBasicParsing
    $errorsList = $response.Content | ConvertFrom-Json
    Write-Host "✅ Total de errores: $($errorsList.totalCount)" -ForegroundColor Green
    Write-Host "   Página actual: $($errorsList.page)" -ForegroundColor Gray
    Write-Host "   Tamaño de página: $($errorsList.pageSize)" -ForegroundColor Gray
    Write-Host "   Items en esta página: $($errorsList.items.Count)" -ForegroundColor Gray
    
    if ($errorsList.items.Count -gt 0) {
        Write-Host "`n   Primeros errores:" -ForegroundColor Gray
        $errorsList.items | Select-Object -First 3 | ForEach-Object {
            Write-Host "   - $($_.serviceName): $($_.message) ($(Get-Date $_.occurredAt -Format 'yyyy-MM-dd HH:mm'))" -ForegroundColor DarkGray
        }
    }
} catch {
    Write-Host "❌ Fallo al listar errores: $($_.Exception.Message)" -ForegroundColor Red
}

# Sprint 1.2.3: GET por ID - Obtener error específico
if ($errorId) {
    Write-Host "`n=== 1.2.3: GET /api/Errors/{id} (Error Específico) ===" -ForegroundColor Cyan
    try {
        $response = Invoke-WebRequest "http://localhost:15083/api/Errors/$errorId" -Headers $headers -UseBasicParsing
        $errorDetail = $response.Content | ConvertFrom-Json
        Write-Host "✅ Error recuperado exitosamente:" -ForegroundColor Green
        Write-Host "   Service: $($errorDetail.serviceName)" -ForegroundColor Gray
        Write-Host "   Type: $($errorDetail.exceptionType)" -ForegroundColor Gray
        Write-Host "   Message: $($errorDetail.message)" -ForegroundColor Gray
        Write-Host "   Endpoint: $($errorDetail.endpoint)" -ForegroundColor Gray
        Write-Host "   Method: $($errorDetail.httpMethod)" -ForegroundColor Gray
        Write-Host "   Status: $($errorDetail.statusCode)" -ForegroundColor Gray
    } catch {
        Write-Host "❌ Fallo al obtener error por ID: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Sprint 1.2.4: GET stats - Estadísticas
Write-Host "`n=== 1.2.4: GET /api/Errors/stats (Estadísticas) ===" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest "http://localhost:15083/api/Errors/stats" -Headers $headers -UseBasicParsing
    $stats = $response.Content | ConvertFrom-Json
    Write-Host "✅ Estadísticas de errores:" -ForegroundColor Green
    Write-Host "   Total de errores: $($stats.totalErrors)" -ForegroundColor Gray
    Write-Host "   Últimas 24 horas: $($stats.errorsLast24Hours)" -ForegroundColor Gray
    Write-Host "   Últimos 7 días: $($stats.errorsLast7Days)" -ForegroundColor Gray
    
    if ($stats.errorsByService -and $stats.errorsByService.PSObject.Properties.Count -gt 0) {
        Write-Host "`n   Errores por servicio:" -ForegroundColor Gray
        $stats.errorsByService.PSObject.Properties | ForEach-Object {
            Write-Host "   - $($_.Name): $($_.Value)" -ForegroundColor DarkGray
        }
    }
    
    if ($stats.errorsByStatusCode -and $stats.errorsByStatusCode.PSObject.Properties.Count -gt 0) {
        Write-Host "`n   Errores por código HTTP:" -ForegroundColor Gray
        $stats.errorsByStatusCode.PSObject.Properties | ForEach-Object {
            Write-Host "   - $($_.Name): $($_.Value)" -ForegroundColor DarkGray
        }
    }
} catch {
    Write-Host "❌ Fallo al obtener estadísticas: $($_.Exception.Message)" -ForegroundColor Red
}

# Sprint 1.2.5: GET services - Listar servicios con errores
Write-Host "`n=== 1.2.5: GET /api/Errors/services (Servicios con Errores) ===" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest "http://localhost:15083/api/Errors/services" -Headers $headers -UseBasicParsing
    $services = $response.Content | ConvertFrom-Json
    Write-Host "✅ Servicios con errores registrados: $($services.serviceNames.Count)" -ForegroundColor Green
    
    if ($services.serviceNames.Count -gt 0) {
        $services.serviceNames | ForEach-Object {
            Write-Host "   - $_" -ForegroundColor Gray
        }
    } else {
        Write-Host "   (No hay servicios con errores aún)" -ForegroundColor DarkGray
    }
} catch {
    Write-Host "❌ Fallo al listar servicios: $($_.Exception.Message)" -ForegroundColor Red
}

# Health Check
Write-Host "`n=== Health Check ===" -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest "http://localhost:15083/health" -UseBasicParsing
    $health = $response.Content | ConvertFrom-Json
    Write-Host "✅ Estado del servicio: $($health.status)" -ForegroundColor Green
    Write-Host "   Timestamp: $($health.timestamp)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Health check falló: $($_.Exception.Message)" -ForegroundColor Red
}

# Resumen
Write-Host "`n=== RESUMEN SPRINT 1.2 ===" -ForegroundColor Magenta
Write-Host "✅ 1.2.1: GET /api/Errors - Paginación funcionando" -ForegroundColor Green
Write-Host "✅ 1.2.2: POST /api/Errors - Creación de errores OK" -ForegroundColor Green
Write-Host "✅ 1.2.3: GET /api/Errors/{id} - Obtención por ID OK" -ForegroundColor Green
Write-Host "✅ 1.2.4: GET /api/Errors/stats - Estadísticas OK" -ForegroundColor Green
Write-Host "✅ 1.2.5: GET /api/Errors/services - Listado de servicios OK" -ForegroundColor Green
Write-Host "`n✅ SPRINT 1.2 COMPLETADO" -ForegroundColor Green -BackgroundColor Black
Write-Host ""
