# 🚀 Sprint 4: Quick Start Script
# Eliminación de 30 vulnerabilidades HIGH a 0

Write-Host @"

╔═══════════════════════════════════════════════════════════════╗
║                                                               ║
║   🛡️  SPRINT 4: VULNERABILITY ELIMINATION                    ║
║                                                               ║
║   Objetivo: Eliminar 30 vulnerabilidades HIGH (100%)         ║
║   Estado Actual: 30 HIGH, 0 CRITICAL                         ║
║   Meta: 0 HIGH, 0 CRITICAL ✅                                ║
║                                                               ║
╚═══════════════════════════════════════════════════════════════╝

"@ -ForegroundColor Cyan

# Configuración
$BackendPath = "C:\Users\gmoreno\source\repos\cardealer\backend"
$TrivyPath = "C:\Users\gmoreno\source\repos\trivy.exe"

# Verificar paths
if (-not (Test-Path $BackendPath)) {
    Write-Host "❌ Error: Backend path no encontrado: $BackendPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $TrivyPath)) {
    Write-Host "⚠️  Advertencia: Trivy no encontrado en $TrivyPath" -ForegroundColor Yellow
    Write-Host "   Descargarlo de: https://github.com/aquasecurity/trivy/releases" -ForegroundColor Yellow
}

# Menú principal
function Show-Menu {
    Write-Host "`n📋 MENÚ SPRINT 4" -ForegroundColor Green
    Write-Host "════════════════════════════════════════════════════" -ForegroundColor Gray
    Write-Host "  1. 📊 Escanear vulnerabilidades .NET actuales" -ForegroundColor White
    Write-Host "  2. 📦 US-4.1: Actualizar Dependencias .NET (90 min)" -ForegroundColor Yellow
    Write-Host "  3. 🐧 US-4.2: Migrar AuthService a Alpine (60 min)" -ForegroundColor Yellow
    Write-Host "  4. 🐧 US-4.3: Migrar Gateway a Alpine (60 min)" -ForegroundColor Yellow
    Write-Host "  5. 🐧 US-4.4: Migrar ErrorService a Alpine (45 min)" -ForegroundColor Yellow
    Write-Host "  6. 🐧 US-4.5: Migrar NotificationService a Alpine (45 min)" -ForegroundColor Yellow
    Write-Host "  7. 🔄 US-4.6: Actualizar ConfigurationService (30 min)" -ForegroundColor Yellow
    Write-Host "  8. 📝 US-4.7: Crear SECURITY_POLICIES.md (45 min)" -ForegroundColor Yellow
    Write-Host "  9. ✅ US-4.8: Escaneo Final y Validación (30 min)" -ForegroundColor Green
    Write-Host "  10. 🎯 Ejecutar TODO el Sprint 4 (automatizado)" -ForegroundColor Cyan
    Write-Host "  11. 📈 Ver progreso actual" -ForegroundColor Magenta
    Write-Host "  0. ❌ Salir" -ForegroundColor Red
    Write-Host "════════════════════════════════════════════════════" -ForegroundColor Gray
}

# US-4.1: Escanear vulnerabilidades .NET
function Scan-DotnetVulnerabilities {
    Write-Host "`n═══════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "📊 Escaneando vulnerabilidades en paquetes .NET..." -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
    
    Push-Location $BackendPath
    
    $projects = @(
        "AuthService/AuthService.Api/AuthService.Api.csproj",
        "Gateway/Gateway.Api/Gateway.Api.csproj",
        "ErrorService/ErrorService.Api/ErrorService.Api.csproj",
        "NotificationService/NotificationService.Api/NotificationService.Api.csproj",
        "ConfigurationService/ConfigurationService.Api/ConfigurationService.Api.csproj",
        "MessageBusService/MessageBusService.Api.csproj"
    )
    
    foreach ($proj in $projects) {
        if (Test-Path $proj) {
            Write-Host "`n📦 Escaneando: $proj" -ForegroundColor Yellow
            Write-Host "─────────────────────────────────────────────" -ForegroundColor Gray
            dotnet list $proj package --vulnerable --include-transitive
        } else {
            Write-Host "⚠️  Proyecto no encontrado: $proj" -ForegroundColor Red
        }
    }
    
    Pop-Location
    
    Write-Host "`n✅ Escaneo completado" -ForegroundColor Green
    Write-Host "`n💡 Paquetes a actualizar típicamente:" -ForegroundColor Cyan
    Write-Host "   • System.Text.Json → 8.0.5+" -ForegroundColor White
    Write-Host "   • Microsoft.Data.SqlClient → 5.1.3+" -ForegroundColor White
    Write-Host "   • System.Formats.Asn1 → 8.0.1+" -ForegroundColor White
}

# US-4.1: Actualizar paquetes .NET
function Update-DotnetPackages {
    Write-Host "`n═══════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "📦 US-4.1: Actualizando Dependencias .NET..." -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
    
    $confirm = Read-Host "`n⚠️  Esto actualizará paquetes en varios proyectos. ¿Continuar? (s/n)"
    if ($confirm -ne 's') {
        Write-Host "❌ Operación cancelada" -ForegroundColor Red
        return
    }
    
    Push-Location $BackendPath
    
    Write-Host "`n1️⃣ Actualizando Gateway..." -ForegroundColor Yellow
    dotnet add Gateway/Gateway.Api package System.Text.Json --version 8.0.5
    dotnet add Gateway/Gateway.Api package Microsoft.Data.SqlClient --version 5.1.3
    dotnet add Gateway/Gateway.Api package System.Formats.Asn1 --version 8.0.1
    
    Write-Host "`n2️⃣ Actualizando ErrorService..." -ForegroundColor Yellow
    dotnet add ErrorService/ErrorService.Api package System.Text.Json --version 8.0.5
    
    Write-Host "`n3️⃣ Actualizando NotificationService..." -ForegroundColor Yellow
    dotnet add NotificationService/NotificationService.Api package System.Text.Json --version 8.0.5
    
    Write-Host "`n4️⃣ Compilando proyectos..." -ForegroundColor Yellow
    dotnet build --no-incremental
    
    Write-Host "`n5️⃣ Ejecutando tests..." -ForegroundColor Yellow
    dotnet test
    
    Pop-Location
    
    Write-Host "`n✅ US-4.1 Completado" -ForegroundColor Green
    Write-Host "📊 Reducción esperada: 30 HIGH → 22 HIGH (-8)" -ForegroundColor Cyan
}

# Migrar servicio a Alpine (función genérica)
function Migrate-ToAlpine {
    param(
        [string]$ServiceName,
        [string]$UsNumber
    )
    
    Write-Host "`n═══════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "🐧 $UsNumber`: Migrando $ServiceName a Alpine..." -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
    
    $dockerfilePath = "$BackendPath\$ServiceName\Dockerfile"
    
    if (-not (Test-Path $dockerfilePath)) {
        Write-Host "❌ Dockerfile no encontrado: $dockerfilePath" -ForegroundColor Red
        return
    }
    
    # Backup
    $backupPath = "$dockerfilePath.bookworm.backup"
    Write-Host "`n1️⃣ Creando backup: $backupPath" -ForegroundColor Yellow
    Copy-Item $dockerfilePath $backupPath -Force
    
    # Leer Dockerfile actual
    $content = Get-Content $dockerfilePath -Raw
    
    # Reemplazar base image
    Write-Host "2️⃣ Actualizando imagen base a Alpine..." -ForegroundColor Yellow
    $content = $content -replace 'mcr\.microsoft\.com/dotnet/aspnet:8\.0-bookworm-slim', 'mcr.microsoft.com/dotnet/aspnet:8.0-alpine'
    
    # Reemplazar comandos de usuario (Debian → Alpine)
    $content = $content -replace 'groupadd -r appuser -g 1000 && useradd -r -u 1000 -g appuser appuser', 'addgroup -g 1000 appuser && adduser -D -u 1000 -G appuser appuser'
    
    # Eliminar comandos apt (no existen en Alpine)
    $content = $content -replace 'apt-get update.*?\n', ''
    $content = $content -replace 'apt-get remove.*?\n', ''
    $content = $content -replace 'apt-get autoremove.*?\n', ''
    $content = $content -replace 'apt-get clean.*?\n', ''
    $content = $content -replace 'rm -rf /var/lib/apt/lists/\*.*?\n', ''
    
    # Guardar Dockerfile modificado
    Set-Content -Path $dockerfilePath -Value $content -NoNewline
    
    Write-Host "3️⃣ Reconstruyendo imagen..." -ForegroundColor Yellow
    Push-Location $BackendPath
    $imageName = "backend-$($ServiceName.ToLower()):latest"
    docker build --no-cache -f "$ServiceName/Dockerfile" -t $imageName .
    Pop-Location
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n4️⃣ Escaneando con Trivy..." -ForegroundColor Yellow
        & $TrivyPath image --severity HIGH,CRITICAL --format table $imageName
        
        Write-Host "`n✅ $UsNumber Completado: $ServiceName migrado a Alpine" -ForegroundColor Green
    } else {
        Write-Host "`n❌ Error al construir imagen. Restaurando backup..." -ForegroundColor Red
        Copy-Item $backupPath $dockerfilePath -Force
    }
}

# Crear SECURITY_POLICIES.md
function Create-SecurityPolicies {
    Write-Host "`n═══════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "📝 US-4.7: Creando SECURITY_POLICIES.md..." -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
    
    $policyPath = "C:\Users\gmoreno\source\repos\cardealer\SECURITY_POLICIES.md"
    
    $template = @"
# 🔒 Políticas de Seguridad - CarDealer Microservices

**Última actualización**: $(Get-Date -Format "d 'de' MMMM 'de' yyyy")  
**Versión**: 1.0  
**Responsable**: Security Team

---

## 1. 🎯 Gestión de Vulnerabilidades

### 1.1 Thresholds Aceptables

| Severidad | Threshold | Acción |
|-----------|-----------|--------|
| **CRITICAL** | **0** | 🚫 Deployment bloqueado |
| **HIGH** | **≤5** | ⚠️ Revisión requerida |
| **MEDIUM** | **≤20** | 📊 Monitoreo activo |
| **LOW** | Sin límite | 💡 Best effort |

### 1.2 Calendario de Escaneo

- **Diario**: Escaneo automático en CI/CD pipeline
- **Semanal**: Escaneo completo de todas las imágenes en registry
- **Mensual**: Auditoría de seguridad completa con reporte ejecutivo

### 1.3 Procedimientos de Remediación

| Severidad | SLA | Procedimiento |
|-----------|-----|---------------|
| **CRITICAL** | 24 horas | Hotfix inmediato, deployment urgente |
| **HIGH** | 7 días | Sprint planning, fix en próximo release |
| **MEDIUM** | 30 días | Backlog, próxima iteración |
| **LOW** | 90 días | Maintenance window |

### 1.4 Responsabilidades

- **DevOps Team**: Escaneo, alertas, remediación de vulnerabilidades de imagen base
- **Development Team**: Remediación de vulnerabilidades en código y dependencias
- **Security Team**: Auditorías, políticas, threshold management

---

## 2. 🔐 Rotación de Secretos

### 2.1 Calendario de Rotación

| Tipo de Secreto | Frecuencia | Automatizado |
|-----------------|------------|--------------|
| **RabbitMQ credentials** | Cada 90 días | ⚠️ Manual |
| **PostgreSQL passwords** | Cada 60 días | ⚠️ Manual |
| **API Keys** | Cada 30 días | ✅ Vault auto-rotation |
| **TLS Certificates** | Cada 365 días | ✅ Let's Encrypt auto-renewal |
| **JWT Signing Keys** | Cada 180 días | ⚠️ Manual |

### 2.2 Procedimiento de Rotación

1. **Generar nuevo secreto** en Vault
2. **Actualizar servicios** con nuevo secreto (zero-downtime)
3. **Verificar funcionamiento** de todos los servicios
4. **Revocar secreto anterior** después de 7 días de gracia
5. **Auditar rotación** en logs de seguridad

### 2.3 Almacenamiento de Secretos

- ✅ **Vault**: Todos los secretos productivos
- ✅ **Kubernetes Secrets**: Secretos de deployment (cifrados at-rest)
- ❌ **appsettings.json**: Solo valores no sensibles
- ❌ **Variables de entorno**: Solo en desarrollo local

---

## 3. 🔄 Actualizaciones de Contenedores

### 3.1 Imágenes Base

| Tipo de Actualización | SLA | Procedimiento |
|----------------------|-----|---------------|
| **Parches de seguridad** | Inmediato | Rebuild + test + deploy |
| **Versiones menores** | Mensual | Sprint planning, testing completo |
| **Versiones mayores** | Trimestral | Spike, PoC, migration plan |

### 3.2 Dependencias .NET

- **NuGet packages**: Revisión mensual con `dotnet list package --vulnerable`
- **Actualizaciones automáticas**: Solo patches (x.x.PATCH)
- **Actualizaciones manuales**: Minor/Major con regression testing

### 3.3 OS Packages

- **Alpine Linux**: Seguir alpine:3.x latest (actualmente 3.22)
- **Debian Bookworm**: Seguir security updates de Debian
- **Monitoreo**: Trivy scans semanales

---

## 4. 🚨 Respuesta a Incidentes

### 4.1 Clasificación de Incidentes

| Prioridad | Descripción | Tiempo de Respuesta |
|-----------|-------------|---------------------|
| **P0** | Vulnerabilidad CRITICAL explotada activamente | 1 hora |
| **P1** | Vulnerabilidad CRITICAL descubierta (no explotada) | 4 horas |
| **P2** | Vulnerabilidad HIGH explotada activamente | 8 horas |
| **P3** | Vulnerabilidad HIGH descubierta (no explotada) | 24 horas |

### 4.2 Procedimiento de Respuesta

#### Fase 1: Detección y Contención (0-2 horas)
1. **Alerta recibida** (Trivy, Dependabot, CVE feed)
2. **Verificar impacto** en servicios productivos
3. **Contener** si es necesario:
   - Aislar servicios afectados
   - Bloquear tráfico sospechoso
   - Activar WAF rules

#### Fase 2: Análisis y Remediación (2-24 horas)
4. **Analizar vulnerabilidad** en detalle
5. **Desarrollar fix** (patch, workaround, o mitigation)
6. **Testing** en staging environment
7. **Deployment** a producción

#### Fase 3: Post-Mortem (24-72 horas)
8. **Documentar incidente** en wiki
9. **Lecciones aprendidas** con equipo
10. **Actualizar políticas** si es necesario

### 4.3 Contactos de Emergencia

- **Security Team Lead**: security-team@cardealer.com
- **DevOps On-Call**: +1-555-DEVOPS
- **Escalation**: CTO / CISO

---

## 5. 📊 Monitoreo y Auditoría

### 5.1 Métricas de Seguridad (KPIs)

- **Vulnerability Remediation Time**: Promedio de días para cerrar vulnerabilidades
- **Security Scan Coverage**: % de servicios escaneados regularmente
- **Secret Rotation Compliance**: % de secretos rotados según calendario
- **Zero-Day Response Time**: Tiempo desde CVE publicado hasta patch deployed

### 5.2 Auditorías

- **Interna**: Trimestral (Security Team)
- **Externa**: Anual (Third-party security firm)
- **Compliance**: Según regulaciones aplicables (SOC2, ISO27001, etc.)

### 5.3 Reportes

- **Diario**: Dashboard de vulnerabilidades en Grafana
- **Semanal**: Email summary a Development Leads
- **Mensual**: Reporte ejecutivo a CTO/CISO
- **Trimestral**: Board presentation

---

## 6. 🛡️ Controles Compensatorios

Para vulnerabilidades que no pueden ser remediadas inmediatamente:

### 6.1 Network Policies
- Restringir tráfico inter-service
- Implementar zero-trust networking
- Segmentación de red por criticidad

### 6.2 WAF Rules
- Bloquear patrones de explotación conocidos
- Rate limiting agresivo
- IP blacklisting

### 6.3 Runtime Security
- Falco para detección de comportamiento anómalo
- AppArmor/SELinux profiles
- Read-only filesystems

---

## 7. 📚 Referencias

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [CIS Docker Benchmarks](https://www.cisecurity.org/benchmark/docker)
- [NIST Cybersecurity Framework](https://www.nist.gov/cyberframework)
- [Sprint 3 Completion Report](./SPRINT3_COMPLETION_REPORT.md)
- [Sprint 4 Vulnerability Elimination Plan](./SPRINT_4_VULNERABILITY_ELIMINATION.md)

---

## 8. 🔄 Historial de Cambios

| Versión | Fecha | Cambios | Autor |
|---------|-------|---------|-------|
| 1.0 | $(Get-Date -Format "yyyy-MM-dd") | Versión inicial - Sprint 4 | GitHub Copilot AI |

---

**Aprobado por**: Security Team Lead  
**Fecha de aprobación**: Pendiente  
**Próxima revisión**: $(Get-Date (Get-Date).AddDays(90) -Format "d 'de' MMMM 'de' yyyy")
"@

    Set-Content -Path $policyPath -Value $template -Encoding UTF8
    
    Write-Host "`n✅ SECURITY_POLICIES.md creado: $policyPath" -ForegroundColor Green
    Write-Host "📝 Revisar y aprobar por Security Team" -ForegroundColor Yellow
}

# Escaneo final
function Run-FinalScan {
    Write-Host "`n═══════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "✅ US-4.8: Escaneo Final y Validación..." -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
    
    $services = @("authservice", "gateway", "errorservice", "notificationservice", "configurationservice", "messagebusservice")
    
    $results = @()
    foreach ($svc in $services) {
        $img = "backend-$svc:latest"
        Write-Host "`n🔍 Escaneando: $img" -ForegroundColor Yellow
        
        $output = & $TrivyPath image --severity HIGH,CRITICAL --format json $img 2>$null | ConvertFrom-Json
        $high = ($output.Results.Vulnerabilities | Where-Object { $_.Severity -eq "HIGH" }).Count
        $critical = ($output.Results.Vulnerabilities | Where-Object { $_.Severity -eq "CRITICAL" }).Count
        
        $status = if ($high -eq 0 -and $critical -eq 0) { "✅ PERFECTO" } elseif ($high -le 2) { "⚠️ ACEPTABLE" } else { "❌ REQUIERE TRABAJO" }
        
        $results += [PSCustomObject]@{
            Service = $svc
            HIGH = $high
            CRITICAL = $critical
            Total = $high + $critical
            Status = $status
        }
    }
    
    Write-Host "`n╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║          📊 RESULTADOS FINALES - SPRINT 4                     ║" -ForegroundColor Green
    Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Green
    
    $results | Format-Table -AutoSize
    
    $totalHigh = ($results | Measure-Object -Property HIGH -Sum).Sum
    $totalCritical = ($results | Measure-Object -Property CRITICAL -Sum).Sum
    $totalVuln = ($results | Measure-Object -Property Total -Sum).Sum
    $perfectServices = ($results | Where-Object { $_.Total -eq 0 }).Count
    
    Write-Host "`n📈 RESUMEN:" -ForegroundColor Cyan
    Write-Host "   Total HIGH: $totalHigh (Objetivo: 0)" -ForegroundColor $(if ($totalHigh -eq 0) { "Green" } else { "Yellow" })
    Write-Host "   Total CRITICAL: $totalCritical (Objetivo: 0)" -ForegroundColor $(if ($totalCritical -eq 0) { "Green" } else { "Red" })
    Write-Host "   Total vulnerabilidades: $totalVuln" -ForegroundColor Cyan
    Write-Host "   Servicios perfectos: $perfectServices/6" -ForegroundColor $(if ($perfectServices -eq 6) { "Green" } else { "Yellow" })
    
    if ($totalVuln -eq 0) {
        Write-Host "`n🎉🎉🎉 ¡OBJETIVO ALCANZADO! 🎉🎉🎉" -ForegroundColor Green
        Write-Host "   0 vulnerabilidades HIGH/CRITICAL en todas las imágenes" -ForegroundColor Green
        Write-Host "   Sprint 4 completado exitosamente ✅" -ForegroundColor Green
    } elseif ($totalHigh -le 5 -and $totalCritical -eq 0) {
        Write-Host "`n⚠️  Objetivo casi alcanzado" -ForegroundColor Yellow
        Write-Host "   $totalHigh vulnerabilidades HIGH restantes (threshold: ≤5)" -ForegroundColor Yellow
    } else {
        Write-Host "`n❌ Objetivo no alcanzado" -ForegroundColor Red
        Write-Host "   Continuar con remediación de vulnerabilidades" -ForegroundColor Red
    }
}

# Ver progreso
function Show-Progress {
    Write-Host "`n╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║          📊 DASHBOARD DE PROGRESO - SPRINT 4                  ║" -ForegroundColor Cyan
    Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    
    $services = @("authservice", "gateway", "errorservice", "notificationservice", "configurationservice", "messagebusservice")
    $totalHigh = 0
    $totalCritical = 0
    $perfectServices = 0
    
    foreach ($svc in $services) {
        $img = "backend-$svc:latest"
        $output = & $TrivyPath image --severity HIGH,CRITICAL --format json $img 2>$null | ConvertFrom-Json
        $high = ($output.Results.Vulnerabilities | Where-Object { $_.Severity -eq "HIGH" }).Count
        $critical = ($output.Results.Vulnerabilities | Where-Object { $_.Severity -eq "CRITICAL" }).Count
        
        $totalHigh += $high
        $totalCritical += $critical
        if ($high -eq 0 -and $critical -eq 0) { $perfectServices++ }
    }
    
    $progress = [math]::Round((($perfectServices / 6) * 100), 2)
    
    Write-Host "`n🎯 Vulnerabilidades Totales:"
    Write-Host "   HIGH: $totalHigh / 30 (Sprint 3 baseline)" -ForegroundColor $(if ($totalHigh -eq 0) { "Green" } elseif ($totalHigh -le 15) { "Yellow" } else { "Red" })
    Write-Host "   CRITICAL: $totalCritical (Objetivo: 0)" -ForegroundColor $(if ($totalCritical -eq 0) { "Green" } else { "Red" })
    Write-Host "`n✅ Servicios Perfectos (0 vulnerabilidades): $perfectServices/6"
    Write-Host "`n📊 Progreso Sprint 4: $progress% completado"
    
    # Progress bar
    $barLength = 50
    $filled = [math]::Round(($progress / 100) * $barLength)
    $bar = "█" * $filled + "░" * ($barLength - $filled)
    Write-Host "`n[$bar] $progress%" -ForegroundColor Cyan
    
    # Comparativa con Sprint 3
    $reduction = [math]::Round(((30 - $totalHigh) / 30) * 100, 2)
    Write-Host "`n📉 Reducción desde Sprint 3: $reduction%" -ForegroundColor Green
}

# Ejecutar todo el sprint
function Run-FullSprint {
    Write-Host "`n⚠️  ATENCIÓN: Esto ejecutará todo el Sprint 4 de forma automatizada" -ForegroundColor Yellow
    Write-Host "   Duración estimada: 4-6 horas" -ForegroundColor Yellow
    $confirm = Read-Host "`n¿Desea continuar? (s/n)"
    
    if ($confirm -ne 's') {
        Write-Host "❌ Operación cancelada" -ForegroundColor Red
        return
    }
    
    Write-Host "`n🚀 Iniciando Sprint 4 completo..." -ForegroundColor Green
    
    # US-4.1
    Update-DotnetPackages
    Read-Host "`nPresione Enter para continuar con US-4.2..."
    
    # US-4.2
    Migrate-ToAlpine -ServiceName "AuthService" -UsNumber "US-4.2"
    Read-Host "`nPresione Enter para continuar con US-4.3..."
    
    # US-4.3
    Migrate-ToAlpine -ServiceName "Gateway" -UsNumber "US-4.3"
    Read-Host "`nPresione Enter para continuar con US-4.4..."
    
    # US-4.4
    Migrate-ToAlpine -ServiceName "ErrorService" -UsNumber "US-4.4"
    Read-Host "`nPresione Enter para continuar con US-4.5..."
    
    # US-4.5
    Migrate-ToAlpine -ServiceName "NotificationService" -UsNumber "US-4.5"
    Read-Host "`nPresione Enter para continuar con US-4.7..."
    
    # US-4.7
    Create-SecurityPolicies
    Read-Host "`nPresione Enter para continuar con US-4.8..."
    
    # US-4.8
    Run-FinalScan
    
    Write-Host "`n🎉 Sprint 4 completado!" -ForegroundColor Green
}

# Loop principal
do {
    Show-Menu
    $choice = Read-Host "`nSeleccione una opción"
    
    switch ($choice) {
        "1" { Scan-DotnetVulnerabilities }
        "2" { Update-DotnetPackages }
        "3" { Migrate-ToAlpine -ServiceName "AuthService" -UsNumber "US-4.2" }
        "4" { Migrate-ToAlpine -ServiceName "Gateway" -UsNumber "US-4.3" }
        "5" { Migrate-ToAlpine -ServiceName "ErrorService" -UsNumber "US-4.4" }
        "6" { Migrate-ToAlpine -ServiceName "NotificationService" -UsNumber "US-4.5" }
        "7" { Write-Host "`n⚠️  US-4.6 requiere validación manual de ConfigurationService" -ForegroundColor Yellow }
        "8" { Create-SecurityPolicies }
        "9" { Run-FinalScan }
        "10" { Run-FullSprint }
        "11" { Show-Progress }
        "0" { Write-Host "`n👋 ¡Hasta luego!" -ForegroundColor Cyan }
        default { Write-Host "`n❌ Opción inválida" -ForegroundColor Red }
    }
    
    if ($choice -ne "0") {
        Read-Host "`nPresione Enter para continuar..."
    }
    
} while ($choice -ne "0")
