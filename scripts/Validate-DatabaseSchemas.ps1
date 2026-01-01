# 🔍 Script de Validación de Schemas - Base de Datos vs Entidades
# Proyecto: CarDealer Microservices
# Sprint: 0.6.3 - Schema Validation
# Fecha: 1 Enero 2026

param(
    [string]$ServiceName = "",  # Validar un servicio específico o todos
    [switch]$FixMismatches = $false,  # Generar migraciones para corregir
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Continue"

Write-Host "`n╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                                                              ║" -ForegroundColor Cyan
Write-Host "║         🔍 VALIDACIÓN DE SCHEMAS DB vs ENTITIES 🔍          ║" -ForegroundColor Cyan
Write-Host "║                                                              ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# Configuración
$backendPath = "backend"
$results = @{
    TotalServices = 0
    ServicesWithDB = 0
    ServicesValidated = 0
    Mismatches = @()
    Errors = @()
}

# Servicios con bases de datos PostgreSQL
$servicesWithDB = @(
    @{Name="AuthService"; DB="authservice"; Port=25432},
    @{Name="UserService"; DB="userservice"; Port=25434},
    @{Name="RoleService"; DB="roleservice"; Port=25435},
    @{Name="ProductService"; DB="productservice"; Port=25440},
    @{Name="ErrorService"; DB="errorservice"; Port=25433},
    @{Name="MediaService"; DB="mediaservice"; Port=25441},
    @{Name="NotificationService"; DB="notificationservice"; Port=25437},
    @{Name="AuditService"; DB="auditservice"; Port=25436},
    @{Name="BillingService"; DB="billingservice"; Port=25442},
    @{Name="CRMService"; DB="crmservice"; Port=25443},
    @{Name="ContactService"; DB="contactservice"; Port=25444},
    @{Name="AppointmentService"; DB="appointmentservice"; Port=25445},
    @{Name="MarketingService"; DB="marketingservice"; Port=25446},
    @{Name="ReportsService"; DB="reportsservice"; Port=25438},
    @{Name="RealEstateService"; DB="realestateservice"; Port=25447},
    @{Name="FinanceService"; DB="financeservice"; Port=25448},
    @{Name="InvoicingService"; DB="invoicingservice"; Port=25449},
    @{Name="SchedulerService"; DB="schedulerservice"; Port=25450},
    @{Name="BackupDRService"; DB="backupdrservice"; Port=25451},
    @{Name="FileStorageService"; DB="filestorageservice"; Port=25452}
)

$results.TotalServices = $servicesWithDB.Count

Write-Host "📊 SERVICIOS A VALIDAR: $($servicesWithDB.Count)`n" -ForegroundColor Cyan

# Función para obtener entidades de un servicio
function Get-ServiceEntities {
    param([string]$ServicePath)
    
    $domainPath = Join-Path $ServicePath "*Domain/Entities/*.cs"
    $entityFiles = Get-ChildItem $domainPath -File -ErrorAction SilentlyContinue
    
    $entities = @()
    foreach ($file in $entityFiles) {
        $content = Get-Content $file.FullName -Raw
        
        # Extraer nombre de clase
        if ($content -match 'public\s+class\s+(\w+)') {
            $className = $Matches[1]
            
            # Extraer propiedades públicas
            $properties = @()
            $propertyMatches = [regex]::Matches($content, 'public\s+(\w+\??)\s+(\w+)\s*{\s*get;\s*set;')
            
            foreach ($match in $propertyMatches) {
                $properties += @{
                    Type = $match.Groups[1].Value
                    Name = $match.Groups[2].Value
                }
            }
            
            $entities += @{
                ClassName = $className
                FilePath = $file.FullName
                Properties = $properties
            }
        }
    }
    
    return $entities
}

# Función para obtener columnas de una tabla PostgreSQL
function Get-DatabaseColumns {
    param(
        [string]$Database,
        [string]$TableName,
        [int]$Port
    )
    
    try {
        $query = @"
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = '$($TableName.ToLower())'
ORDER BY ordinal_position;
"@
        
        $result = docker exec authservice-db psql -U postgres -d $Database -t -c $query 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            $columns = @()
            $lines = $result -split "`n" | Where-Object { $_.Trim() -ne "" }
            
            foreach ($line in $lines) {
                $parts = $line -split '\|' | ForEach-Object { $_.Trim() }
                if ($parts.Count -ge 3) {
                    $columns += @{
                        Name = $parts[0]
                        Type = $parts[1]
                        Nullable = $parts[2]
                    }
                }
            }
            
            return $columns
        }
    } catch {
        return $null
    }
    
    return $null
}

# Función para comparar entidad con tabla
function Compare-EntityWithTable {
    param(
        [object]$Entity,
        [array]$DbColumns,
        [string]$ServiceName
    )
    
    $mismatches = @()
    
    # Verificar cada propiedad de la entidad
    foreach ($prop in $Entity.Properties) {
        $propName = $prop.Name
        $dbColumn = $DbColumns | Where-Object { $_.Name -eq $propName.ToLower() }
        
        if (-not $dbColumn) {
            $mismatches += @{
                Type = "MissingColumn"
                Entity = $Entity.ClassName
                Property = $propName
                Description = "Propiedad '$propName' existe en C# pero no en BD"
            }
        }
    }
    
    # Verificar columnas en BD que no existen en entidad
    foreach ($col in $DbColumns) {
        $colName = $col.Name
        $entityProp = $Entity.Properties | Where-Object { $_.Name.ToLower() -eq $colName }
        
        if (-not $entityProp) {
            $mismatches += @{
                Type = "MissingProperty"
                Entity = $Entity.ClassName
                Column = $colName
                Description = "Columna '$colName' existe en BD pero no en C#"
            }
        }
    }
    
    return $mismatches
}

# Validar servicios
foreach ($svc in $servicesWithDB) {
    # Filtrar por servicio específico si se proporcionó
    if ($ServiceName -ne "" -and $svc.Name -ne $ServiceName) {
        continue
    }
    
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
    Write-Host "🔍 Validando: $($svc.Name)" -ForegroundColor Cyan
    
    # Verificar si el contenedor de DB está corriendo
    $dbContainer = "$($svc.Name.ToLower())-db"
    $containerStatus = docker ps --filter "name=$dbContainer" --format "{{.Names}}" 2>$null
    
    if (-not $containerStatus) {
        Write-Host "   ⚠️  Base de datos no corriendo ($dbContainer)" -ForegroundColor Yellow
        $results.Errors += "$($svc.Name): DB container not running"
        continue
    }
    
    $results.ServicesWithDB++
    
    # Obtener entidades del servicio
    $servicePath = Join-Path $backendPath $svc.Name
    $entities = Get-ServiceEntities -ServicePath $servicePath
    
    if ($entities.Count -eq 0) {
        Write-Host "   ⚠️  No se encontraron entidades" -ForegroundColor Yellow
        continue
    }
    
    Write-Host "   📦 Entidades encontradas: $($entities.Count)" -ForegroundColor Gray
    
    # Validar cada entidad
    $serviceMismatches = 0
    foreach ($entity in $entities) {
        $tableName = $entity.ClassName
        
        # Obtener columnas de la tabla
        $dbColumns = Get-DatabaseColumns -Database $svc.DB -TableName $tableName -Port $svc.Port
        
        if ($null -eq $dbColumns -or $dbColumns.Count -eq 0) {
            if ($Verbose) {
                Write-Host "      ⚪ $tableName → Tabla no existe en BD (puede ser normal)" -ForegroundColor Gray
            }
            continue
        }
        
        # Comparar entidad con tabla
        $mismatches = Compare-EntityWithTable -Entity $entity -DbColumns $dbColumns -ServiceName $svc.Name
        
        if ($mismatches.Count -gt 0) {
            $serviceMismatches += $mismatches.Count
            
            foreach ($mismatch in $mismatches) {
                $results.Mismatches += @{
                    Service = $svc.Name
                    Entity = $mismatch.Entity
                    Type = $mismatch.Type
                    Description = $mismatch.Description
                }
                
                if ($mismatch.Type -eq "MissingColumn") {
                    Write-Host "      ❌ $($mismatch.Entity).$($mismatch.Property) → Falta en BD" -ForegroundColor Red
                } else {
                    Write-Host "      ⚠️  $($mismatch.Entity).$($mismatch.Column) → Falta en C#" -ForegroundColor Yellow
                }
            }
        } else {
            Write-Host "      ✅ $tableName → Schema sincronizado" -ForegroundColor Green
        }
    }
    
    if ($serviceMismatches -eq 0) {
        Write-Host "   ✅ Todas las entidades sincronizadas" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️  $serviceMismatches desincronizaciones encontradas" -ForegroundColor Yellow
    }
    
    $results.ServicesValidated++
}

# Resumen final
Write-Host "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host "📊 RESUMEN DE VALIDACIÓN:" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray

Write-Host "`n✅ Servicios validados:       $($results.ServicesValidated) / $($results.TotalServices)" -ForegroundColor $(if($results.ServicesValidated -eq $results.TotalServices){"Green"}else{"Yellow"})
Write-Host "🗄️  Servicios con DB activa:  $($results.ServicesWithDB) / $($results.TotalServices)" -ForegroundColor Gray
Write-Host "⚠️  Desincronizaciones:       $($results.Mismatches.Count)" -ForegroundColor $(if($results.Mismatches.Count -eq 0){"Green"}else{"Yellow"})
Write-Host "❌ Errores:                   $($results.Errors.Count)`n" -ForegroundColor $(if($results.Errors.Count -eq 0){"Green"}else{"Red"})

if ($results.Mismatches.Count -gt 0) {
    Write-Host "⚠️  DESINCRONIZACIONES DETECTADAS:`n" -ForegroundColor Yellow
    
    $grouped = $results.Mismatches | Group-Object Service
    foreach ($group in $grouped) {
        Write-Host "   📦 $($group.Name): $($group.Count) issues" -ForegroundColor Yellow
        foreach ($issue in $group.Group) {
            Write-Host "      • $($issue.Description)" -ForegroundColor Gray
        }
    }
    
    if ($FixMismatches) {
        Write-Host "`n💡 Generando migraciones para corregir...`n" -ForegroundColor Cyan
        # TODO: Implementar generación automática de migraciones
        Write-Host "   ⚠️  Feature pendiente de implementación" -ForegroundColor Yellow
    }
} else {
    Write-Host "🎉 ¡TODOS LOS SCHEMAS SINCRONIZADOS!`n" -ForegroundColor Green
}

# Guardar resultados
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputFile = "SCHEMA_VALIDATION_RESULTS_$timestamp.json"

$results | ConvertTo-Json -Depth 10 | Out-File $outputFile -Encoding utf8

Write-Host "💾 Resultados guardados en: $outputFile" -ForegroundColor Cyan

if ($results.Errors.Count -gt 0) {
    Write-Host "`n⚠️  ERRORES ENCONTRADOS:" -ForegroundColor Yellow
    foreach ($error in $results.Errors) {
        Write-Host "   • $error" -ForegroundColor Red
    }
}

Write-Host "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Gray

# Retornar código de salida
if ($results.Mismatches.Count -eq 0 -and $results.Errors.Count -eq 0) {
    exit 0
} else {
    exit 1
}
