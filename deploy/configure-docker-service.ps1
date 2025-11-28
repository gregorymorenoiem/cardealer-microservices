# ESTE SCRIPT SE EJECUTA DESDE DENTRO DEL FOLDER DEPLOY
Write-Host "=== CONFIGURANDO DOCKER EN EL SERVIDOR ==="

# Verificar que somos administrador
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal(
    [Security.Principal.WindowsIdentity]::GetCurrent()
)

if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Este script requiere permisos de Administrador"
    Write-Host "Ejecuta PowerShell como Administrador"
    exit 1
}

# Verificar instalacion de Docker
try {
    docker --version | Out-Null
    Write-Host "Docker esta instalado"
}
catch {
    Write-Host "Docker no esta instalado"
    Write-Host "Instalando Docker..."

    # Instalar Docker (ejemplo para Windows Server)
    Install-Module -Name DockerMsftProvider -Repository PSGallery -Force
    Install-Package -Name docker -ProviderName DockerMsftProvider -Force

    Write-Host "Docker instalado. Reinicia el servidor y ejecuta este script nuevamente."
    exit 0
}

# Configurar servicio Docker
$dockerService = Get-Service -Name "Docker" -ErrorAction SilentlyContinue

if ($dockerService) {
    # Configurar para inicio automatico
    Set-Service -Name "Docker" -StartupType Automatic
    Write-Host "Servicio Docker configurado para inicio automatico"
    
    # Iniciar el servicio si no esta corriendo
    if ($dockerService.Status -ne "Running") {
        Start-Service -Name "Docker"
        Write-Host "Servicio Docker iniciado"
    }
    else {
        Write-Host "Servicio Docker ya esta ejecutandose"
    }
}
else {
    Write-Host "Servicio Docker no encontrado"
}

# Verificar que Docker este funcionando
Write-Host ""
Write-Host "Verificando funcionamiento de Docker..."
try {
    docker info | Out-Null
    Write-Host "Docker esta funcionando correctamente"
}
catch {
    Write-Host "Docker no esta funcionando correctamente"
}

Write-Host ""
Write-Host "=== CONFIGURACION COMPLETADA ==="
Write-Host "Ahora puedes ejecutar .\deploy-on-server.ps1 para desplegar los microservicios"
