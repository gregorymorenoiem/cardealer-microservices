# Script para validar que todas las entidades que implementan ITenantEntity tienen DealerId en la BD

$serviciosConDB = @(
    @{Name="AuthService"; DB="authservice-db"; DBName="authservice"; Tablas=@("Users", "RefreshTokens")},
    @{Name="UserService"; DB="userservice-db"; DBName="userservice"; Tablas=@("ApplicationUsers", "Dealers")},
    @{Name="RoleService"; DB="roleservice-db"; DBName="roleservice"; Tablas=@("Roles", "Permissions")},
    @{Name="ProductService"; DB="productservice-db"; DBName="productservice"; Tablas=@("products", "product_images", "categories")},
    @{Name="MediaService"; DB="mediaservice-db"; DBName="mediaservice"; Tablas=@("MediaFiles", "MediaFolders")},
    @{Name="ErrorService"; DB="errorservice-db"; DBName="errorservice"; Tablas=@("Errors")},
    @{Name="NotificationService"; DB="notificationservice-db"; DBName="notificationservice"; Tablas=@("Notifications")},
    @{Name="ReportsService"; DB="reportsservice-db"; DBName="reportsservice"; Tablas=@("Reports")},
    @{Name="BillingService"; DB="billingservice-db"; DBName="billingservice"; Tablas=@("Invoices", "Payments")},
    @{Name="FinanceService"; DB="financeservice-db"; DBName="financeservice"; Tablas=@("Accounts", "Transactions")},
    @{Name="InvoicingService"; DB="invoicingservice-db"; DBName="invoicingservice"; Tablas=@("Invoices", "InvoiceItems")},
    @{Name="CRMService"; DB="crmservice-db"; DBName="crmservice"; Tablas=@("Contacts", "Opportunities")},
    @{Name="AdminService"; DB="adminservice-db"; DBName="adminservice"; Tablas=@()}
)

Write-Host "Validando columnas DealerId en bases de datos..." -ForegroundColor Cyan
Write-Host ""

$resumen = @()

foreach ($servicio in $serviciosConDB) {
    Write-Host "$($servicio.Name)" -ForegroundColor Yellow
    
    # Verificar si el contenedor está corriendo
    $containerStatus = docker ps --filter "name=$($servicio.DB)" --format "{{.Names}}\t{{.Status}}" 2>$null
    
    if (-not $containerStatus) {
        Write-Host "  Contenedor $($servicio.DB) no esta corriendo" -ForegroundColor Red
        $resumen += @{Servicio=$servicio.Name; Estado="DB no disponible"; DetalerId="N/A"}
        continue
    }
    
    $tablesSinDealerId = @()
    $tablesConDealerId = @()
    
    foreach ($tabla in $servicio.Tablas) {
        # Verificar si la tabla existe y tiene columna DealerId
        $columnas = docker exec -i $($servicio.DB) psql -U postgres -d $($servicio.DBName) -t -c "\d+ `"$tabla`"" 2>$null
        
        if ($LASTEXITCODE -ne 0) {
            # Probar sin comillas (lowercase)
            $columnas = docker exec -i $($servicio.DB) psql -U postgres -d $($servicio.DBName) -t -c "\d+ $tabla" 2>$null
        }
        
        if ($columnas -match "DealerId") {
            Write-Host "  OK $tabla tiene DealerId" -ForegroundColor Green
            $tablesConDealerId += $tabla
        } else {
            Write-Host "  FALTA $tabla NO tiene DealerId" -ForegroundColor Red
            $tablesSinDealerId += $tabla
        }
    }
    
    if ($tablesSinDealerId.Count -eq 0) {
        $resumen += @{Servicio=$servicio.Name; Estado="OK"; DealerId="Todas las tablas ($($tablesConDealerId.Count))"}
    } else {
        $resumen += @{Servicio=$servicio.Name; Estado="Falta DealerId"; DealerId="$($tablesSinDealerId -join ', ')"}
    }
    
    Write-Host ""
}

Write-Host ""
Write-Host "RESUMEN" -ForegroundColor Cyan
Write-Host ("=" * 80)

foreach ($item in $resumen) {
    $color = "Yellow"
    if ($item.Estado -match "OK") { $color = "Green" }
    elseif ($item.Estado -match "no disponible") { $color = "Red" }
    
    Write-Host "$($item.Servicio.PadRight(25)) $($item.Estado.PadRight(20)) $($item.DealerId)" -ForegroundColor $color
}

Write-Host ""
Write-Host "Validacion completada" -ForegroundColor Green
