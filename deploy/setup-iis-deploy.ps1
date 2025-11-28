# setup-iis-deploy.ps1
# EJECUTAR COMO ADMINISTRADOR

Write-Host "=== INICIANDO DESPLIEGUE EN IIS ===" -ForegroundColor Cyan

# 1. Definición de puertos (Igual que en tu docker-compose)
$services = @{
    "errorservice"        = 5000
    "authservice"         = 5001
    "notificationservice" = 5002
    "vehicleservice"      = 5003
    "contactservice"      = 5004
    "mediaservice"        = 5005
    "auditservice"        = 5006
    "adminservice"        = 5007
    "gateway"             = 8027 # Este es el público
}

# Directorio base donde se alojarán los sitios
$iisBaseDir = "C:\inetpub\vhosts\cargurus"
if (-not (Test-Path $iisBaseDir)) { New-Item -Path $iisBaseDir -ItemType Directory | Out-Null }

# Importar módulo de IIS
Import-Module WebAdministration

foreach ($serviceName in $services.Keys) {
    $port = $services[$serviceName]
    $sourceAppPath = ".\$serviceName\app"
    $destPath = "$iisBaseDir\$serviceName"

    Write-Host "`nProcesando: $serviceName (Puerto $port)..." -ForegroundColor Yellow

    # A. Verificar que existe la carpeta origen
    if (-not (Test-Path $sourceAppPath)) {
        Write-Host "  [ERROR] No se encuentra la carpeta origen: $sourceAppPath" -ForegroundColor Red
        continue
    }

    # B. Copiar Archivos
    Write-Host "  Copiando archivos a $destPath..."
    if (Test-Path $destPath) {
        # Limpiar despliegue anterior (opcional, cuidado con archivos de config manuales)
        # Remove-Item "$destPath\*" -Recurse -Force -ErrorAction SilentlyContinue
    }
    else {
        New-Item -Path $destPath -ItemType Directory | Out-Null
    }
    Copy-Item -Path "$sourceAppPath\*" -Destination $destPath -Recurse -Force

    # C. Crear/Actualizar Application Pool
    $poolName = "Pool_$serviceName"
    if (-not (Test-Path "IIS:\AppPools\$poolName")) {
        Write-Host "  Creando Application Pool: $poolName"
        New-WebAppPool -Name $poolName
        # Configurar para que no se suspenda por inactividad (opcional, bueno para APIs)
        Set-ItemProperty "IIS:\AppPools\$poolName" -Name "processModel.idleTimeout" -Value "00:00:00"
        Set-ItemProperty "IIS:\AppPools\$poolName" -Name "recycling.periodicRestart.time" -Value "00:00:00"
    }

    # D. Crear/Actualizar Sitio Web
    if (Test-Path "IIS:\Sites\$serviceName") {
        Write-Host "  El sitio $serviceName ya existe. Actualizando archivos..."
        # Aquí podrías reiniciar el sitio si fuera necesario
        # Restart-WebItem "IIS:\Sites\$serviceName"
    }
    else {
        Write-Host "  Creando Sitio Web: $serviceName en puerto $port"
        New-WebSite -Name $serviceName -Port $port -PhysicalPath $destPath -ApplicationPool $poolName
    }

    # E. Firewall (Solo para Gateway)
    if ($serviceName -eq "gateway") {
        Write-Host "  Abriendo puerto $port en Firewall para acceso externo..."
        New-NetFirewallRule -DisplayName "CarGurus Gateway API" -Direction Inbound -LocalPort $port -Protocol TCP -Action Allow -ErrorAction SilentlyContinue
    }
}

Write-Host "`n=== DESPLIEGUE EN IIS FINALIZADO ===" -ForegroundColor Green
Write-Host "IMPORTANTE: Ahora debes editar los archivos appsettings.json en C:\inetpub\vhosts\cargurus\..." -ForegroundColor Cyan