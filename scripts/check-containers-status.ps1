# Script para verificar estado de contenedores Docker
# Sprint 0.7.2 - Secrets Validation

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "   ESTADO DE CONTENEDORES DOCKER" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Obtener todos los contenedores
$containers = docker ps --format "{{.Names}}\t{{.Status}}\t{{.Ports}}"

# Separar por tipo
$microservices = @()
$databases = @()
$infrastructure = @()

foreach ($line in $containers) {
    $parts = $line.Split("`t")
    $name = $parts[0]
    
    if ($name -match "-db$") {
        $databases += $name
    }
    elseif ($name -match "redis|rabbitmq|consul") {
        $infrastructure += $name
    }
    else {
        $microservices += $name
    }
}

# Contadores
$totalContainers = $containers.Count
$microservicesCount = $microservices.Count
$databasesCount = $databases.Count
$infrastructureCount = $infrastructure.Count

Write-Host "Total Contenedores Running: $totalContainers`n" -ForegroundColor White

# Microservicios
Write-Host "[MICROSERVICIOS] - $microservicesCount/35" -ForegroundColor Yellow
Write-Host ("-" * 60)
foreach ($svc in $microservices | Sort-Object) {
    Write-Host "  OK  $svc" -ForegroundColor Green
}

# Bases de Datos
Write-Host "`n[BASES DE DATOS] - $databasesCount/13" -ForegroundColor Yellow
Write-Host ("-" * 60)
foreach ($db in $databases | Sort-Object) {
    Write-Host "  OK  $db" -ForegroundColor Green
}

# Infraestructura
Write-Host "`n[INFRAESTRUCTURA] - $infrastructureCount/3" -ForegroundColor Yellow
Write-Host ("-" * 60)
foreach ($infra in $infrastructure | Sort-Object) {
    Write-Host "  OK  $infra" -ForegroundColor Green
}

# Resumen
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "              RESUMEN" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$expectedTotal = 35 + 13 + 3  # Microservicios + DBs + Infra
Write-Host "Esperados:        $expectedTotal" -ForegroundColor White
Write-Host "Running:          $totalContainers" -ForegroundColor $(if ($totalContainers -eq $expectedTotal) { "Green" } else { "Yellow" })
Write-Host "Microservicios:   $microservicesCount/35" -ForegroundColor $(if ($microservicesCount -eq 35) { "Green" } else { "Yellow" })
Write-Host "Bases de Datos:   $databasesCount/13" -ForegroundColor $(if ($databasesCount -eq 13) { "Green" } else { "Yellow" })
Write-Host "Infraestructura:  $infrastructureCount/3" -ForegroundColor $(if ($infrastructureCount -eq 3) { "Green" } else { "Yellow" })

if ($totalContainers -eq $expectedTotal) {
    Write-Host "`nTODOS LOS CONTENEDORES ESTAN CORRIENDO" -ForegroundColor Green
}
else {
    $missing = $expectedTotal - $totalContainers
    Write-Host "`nFALTAN $missing CONTENEDORES" -ForegroundColor Yellow
    
    # Identificar cuáles faltan (requiere lista completa)
    $expectedServices = @(
        "authservice", "gateway", "errorservice", "notificationservice", "userservice", 
        "roleservice", "productservice", "servicediscovery", "configurationservice",
        "healthcheckservice", "loggingservice", "tracingservice", "cacheservice",
        "messagebusservice", "schedulerservice", "searchservice", "featuretoggleservice",
        "idempotencyservice", "mediaservice", "billingservice", "crmservice",
        "auditservice", "reportsservice", "marketingservice", "integrationservice",
        "financeservice", "invoicingservice", "adminservice", "apidocsservice",
        "ratelimitingservice", "contactservice", "appointmentservice", "backupdrservice",
        "filestorageservice", "realestateservice"
    )
    
    $runningServices = $microservices | ForEach-Object { $_.ToLower() }
    $missingServices = $expectedServices | Where-Object { $runningServices -notcontains $_ }
    
    if ($missingServices) {
        Write-Host "`nServicios faltantes:" -ForegroundColor Red
        foreach ($missing in $missingServices) {
            Write-Host "  - $missing" -ForegroundColor Red
        }
    }
}

Write-Host "`n========================================`n" -ForegroundColor Cyan
