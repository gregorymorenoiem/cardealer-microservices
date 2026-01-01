# Script para validar health checks de todos los microservicios
# Sprint 0.7.2 - Secrets Validation

Write-Host "`n===================================" -ForegroundColor Cyan
Write-Host "   HEALTH CHECK - 35 MICROSERVICIOS" -ForegroundColor Cyan
Write-Host "===================================`n" -ForegroundColor Cyan

# Definir todos los servicios con sus puertos
$services = @(
    # Core Services
    @{Name="Gateway"; Port=18443; Category="Core"},
    @{Name="AuthService"; Port=15085; Category="Core"},
    @{Name="ErrorService"; Port=15083; Category="Core"},
    @{Name="NotificationService"; Port=15084; Category="Core"},
    @{Name="UserService"; Port=15100; Category="Core"},
    @{Name="RoleService"; Port=15101; Category="Core"},
    @{Name="ProductService"; Port=15006; Category="Core"},
    
    # Infrastructure Services
    @{Name="ServiceDiscovery"; Port=15005; Category="Infra"},
    @{Name="ConfigurationService"; Port=15007; Category="Infra"},
    @{Name="HealthCheckService"; Port=15008; Category="Infra"},
    @{Name="LoggingService"; Port=15009; Category="Infra"},
    @{Name="TracingService"; Port=15010; Category="Infra"},
    @{Name="CacheService"; Port=15011; Category="Infra"},
    @{Name="MessageBusService"; Port=15012; Category="Infra"},
    @{Name="SchedulerService"; Port=15013; Category="Infra"},
    @{Name="SearchService"; Port=15014; Category="Infra"},
    @{Name="FeatureToggleService"; Port=15015; Category="Infra"},
    @{Name="IdempotencyService"; Port=15016; Category="Infra"},
    
    # Business Services
    @{Name="MediaService"; Port=15017; Category="Business"},
    @{Name="BillingService"; Port=15018; Category="Business"},
    @{Name="CRMService"; Port=15019; Category="Business"},
    @{Name="AuditService"; Port=15020; Category="Business"},
    @{Name="ReportsService"; Port=15021; Category="Business"},
    @{Name="MarketingService"; Port=15022; Category="Business"},
    @{Name="IntegrationService"; Port=15023; Category="Business"},
    @{Name="FinanceService"; Port=15024; Category="Business"},
    @{Name="InvoicingService"; Port=15025; Category="Business"},
    
    # Specialized Services
    @{Name="AdminService"; Port=24037; Category="Specialized"},
    @{Name="ApiDocsService"; Port=15027; Category="Specialized"},
    @{Name="RateLimitingService"; Port=15028; Category="Specialized"},
    @{Name="ContactService"; Port=15029; Category="Specialized"},
    @{Name="AppointmentService"; Port=15030; Category="Specialized"},
    @{Name="BackupDRService"; Port=15031; Category="Specialized"},
    @{Name="FileStorageService"; Port=15032; Category="Specialized"},
    @{Name="RealEstateService"; Port=15033; Category="Specialized"}
)

# Contadores
$totalServices = $services.Count
$okCount = 0
$failedCount = 0
$results = @{
    OK = @()
    FAILED = @()
}

Write-Host "Validando $totalServices servicios...`n" -ForegroundColor White

# Agrupar por categoría
$categories = $services | Group-Object -Property Category

foreach ($category in $categories) {
    Write-Host "`n[$($category.Name)]" -ForegroundColor Yellow
    Write-Host ("-" * 60)
    
    foreach ($svc in $category.Group) {
        $statusIcon = ""
        $statusColor = "White"
        $statusText = ""
        
        try {
            $response = Invoke-WebRequest "http://localhost:$($svc.Port)/health" -UseBasicParsing -TimeoutSec 5
            
            if ($response.StatusCode -eq 200) {
                $statusIcon = "OK"
                $statusColor = "Green"
                $statusText = "Healthy"
                $okCount++
                $results.OK += $svc.Name
            }
            else {
                $statusIcon = "WARN"
                $statusColor = "Yellow"
                $statusText = "HTTP $($response.StatusCode)"
                $failedCount++
                $results.FAILED += "$($svc.Name) (HTTP $($response.StatusCode))"
            }
        }
        catch {
            $statusIcon = "FAIL"
            $statusColor = "Red"
            $statusText = $_.Exception.Message.Split("`n")[0].Substring(0, [Math]::Min(40, $_.Exception.Message.Length))
            $failedCount++
            $results.FAILED += "$($svc.Name) ($statusText)"
        }
        
        $serviceName = $svc.Name.PadRight(25)
        $port = "[$($svc.Port)]".PadRight(10)
        Write-Host "  $serviceName $port $statusIcon  $statusText" -ForegroundColor $statusColor
    }
}

# Resumen
Write-Host "`n`n===================================" -ForegroundColor Cyan
Write-Host "           RESUMEN FINAL" -ForegroundColor Cyan
Write-Host "===================================`n" -ForegroundColor Cyan

$successRate = [Math]::Round(($okCount / $totalServices) * 100, 2)

Write-Host "Total Servicios:      $totalServices" -ForegroundColor White
Write-Host "Exitosos (OK):        $okCount" -ForegroundColor Green
Write-Host "Fallidos:             $failedCount" -ForegroundColor Red
Write-Host "Tasa de Exito:        $successRate%" -ForegroundColor $(if ($successRate -ge 90) { "Green" } elseif ($successRate -ge 70) { "Yellow" } else { "Red" })

if ($failedCount -gt 0) {
    Write-Host "`nServicios Fallidos:" -ForegroundColor Red
    foreach ($failed in $results.FAILED) {
        Write-Host "  - $failed" -ForegroundColor Red
    }
}

Write-Host "`n===================================`n" -ForegroundColor Cyan

# Retornar código de salida
if ($failedCount -eq 0) {
    Write-Host "TODOS LOS SERVICIOS OPERACIONALES" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "ALGUNOS SERVICIOS REQUIEREN ATENCION" -ForegroundColor Yellow
    exit 1
}
