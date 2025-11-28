# ESTE SCRIPT SE EJECUTA DESDE DENTRO DEL FOLDER DEPLOY - LINUX EN WINDOWS SERVER
param()

Write-Host "=== INICIANDO DEPLOY CON CONTENEDORES LINUX EN WINDOWS SERVER ==="

# Verificar que estamos en el directorio correcto
$currentDir = Get-Location
Write-Host "Directorio actual: $currentDir"

# Verificar archivos esenciales
$essentialFiles = @("docker-compose.yml", ".env.production")
foreach ($file in $essentialFiles) {
    if (-not (Test-Path $file)) {
        Write-Host "ERROR: No se encuentra $file en el directorio actual"
        Write-Host "Ejecuta este script desde el folder deploy que contiene los microservicios"
        exit 1
    }
}

# Verificar que Docker esté instalado
try {
    $dockerVersion = docker --version
    Write-Host "Docker está instalado: $dockerVersion"
    
    $composeVersion = docker compose version
    Write-Host "Docker Compose V2 disponible"
}
catch {
    Write-Host "ERROR: Docker no está instalado o no está corriendo"
    exit 1
}

# Forzar plataforma Linux
$env:DOCKER_DEFAULT_PLATFORM = "linux"
Write-Host "Plataforma forzada: Linux"

# Verificar capacidad de ejecutar contenedores Linux
Write-Host "Verificando compatibilidad con contenedores Linux..."
try {
    $testRun = docker run --rm --platform linux hello-world
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Contenedores Linux compatibles"
    }
    else {
        Write-Host "❌ Problemas con contenedores Linux"
    }
}
catch {
    Write-Host "❌ No se pueden ejecutar contenedores Linux"
    Write-Host "Solución:"
    Write-Host "1. Instala WSL2: wsl --install"
    Write-Host "2. O habilita Hyper-V y contenedores Linux"
    Write-Host "3. O usa Docker Desktop con WSL2 backend"
    exit 1
}

# Verificar y cargar variables de entorno
Write-Host "Verificando variables de entorno..."
if (-not (Test-Path .env.production)) {
    Write-Host "ERROR: No se encuentra .env.production"
    Write-Host "Creando archivo .env.production con valores por defecto..."
    
    @"
# Database
DB_USER=cargurus_user
DB_PASSWORD=StrongPassword123!

# RabbitMQ
RABBITMQ_USER=admin
RABBITMQ_PASS=rabbitmq_pass_123

# JWT
JWT_SECRET=YourSuperSecretJWTKeyForProduction123!

# External Services (configura según necesites)
SENDGRID_API_KEY=your_sendgrid_key
SENDGRID_FROM_EMAIL=noreply@cargurus.com
TWILIO_ACCOUNT_SID=your_twilio_sid
TWILIO_AUTH_TOKEN=your_twilio_token
TWILIO_FROM_NUMBER=+1234567890
FIREBASE_PROJECT_ID=your_firebase_project
"@ | Out-File -FilePath .env.production -Encoding UTF8

    Write-Host "Archivo .env.production creado. Por favor, edita los valores antes de producción."
}
else {
    Write-Host "✅ Archivo .env.production encontrado"
}

# Verificar que los servicios estén presentes
$services = @("errorservice", "authservice", "gateway")
foreach ($service in $services) {
    if (-not (Test-Path $service)) {
        Write-Host "ERROR: No se encuentra el servicio (carpeta): $service"
        exit 1
    }
    
    # Verificar que los Dockerfiles usen imágenes Linux
    $dockerfilePath = "$service/Dockerfile"
    if (Test-Path $dockerfilePath) {
        $content = Get-Content $dockerfilePath -Raw
        if ($content -match "mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver" -or $content -match "mcr.microsoft.com/dotnet/aspnet:8.0-windowsservercore") {
            Write-Host "ADVERTENCIA: $service usa imagen Windows. Cambiando a Linux..."
            $content -replace "mcr\.microsoft\.com/dotnet/aspnet:8\.0-(nanoserver|windowsservercore).*", "mcr.microsoft.com/dotnet/aspnet:8.0" | Set-Content $dockerfilePath
            Write-Host "  ✅ Dockerfile actualizado a imagen Linux"
        }
    }
}

Write-Host "Estructura de deploy verificada correctamente"

# 1. Parar servicios existentes
Write-Host ""
Write-Host "1. Deteniendo servicios anteriores..."
docker compose down

# 2. Construir y levantar servicios con plataforma Linux forzada
Write-Host ""
Write-Host "2. Construyendo e iniciando servicios (Linux)..."
docker compose up -d --build

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Falló el despliegue con Docker Compose"
    Write-Host "Intentando construcción individual..."
    
    # Construir servicios individualmente
    foreach ($service in @("errorservice", "authservice", "gateway")) {
        Write-Host "Construyendo $service..."
        docker compose build $service
        if ($LASTEXITCODE -ne 0) {
            Write-Host "ERROR: Falló la construcción de $service"
            exit 1
        }
    }
    
    Write-Host "Iniciando servicios construidos..."
    docker compose up -d
}

# 3. Esperar a que los servicios inicien
Write-Host ""
Write-Host "3. Esperando que servicios inicien (45 segundos)..."
Start-Sleep -Seconds 45

# 4. Verificar estado de los contenedores
Write-Host ""
Write-Host "4. Estado de los servicios:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# 5. Health checks básicos
Write-Host ""
Write-Host "5. Verificación de salud:"
$servicesContainers = @("errorservice-prod", "authservice-prod", "gateway-prod")

$allRunning = $true
foreach ($service in $servicesContainers) {
    try {
        $status = docker inspect --format='{{.State.Status}}' $service 2>$null
        if ($status -eq "running") {
            Write-Host "  ✅ Servicio $service está ejecutándose"
            
            # Verificar healthcheck adicional
            Start-Sleep -Seconds 2
            $health = docker inspect --format='{{.State.Health.Status}}' $service 2>$null
            if ($health -and $health -ne "") {
                Write-Host "     Estado de salud: $health"
            }
            
            # Verificar logs de error recientes
            $errorLogs = docker logs $service --tail 5 2>&1 | Select-String -Pattern "error|exception|fail" -CaseSensitive:$false
            if ($errorLogs) {
                Write-Host "     ⚠️  Se encontraron errores en logs"
            }
        }
        else {
            Write-Host "  ❌ Servicio $service NO está ejecutándose: $status"
            $allRunning = $false
            
            # Mostrar últimos logs del servicio problemático
            Write-Host "     Últimos logs:"
            docker logs $service --tail 10 2>$null
        }
    }
    catch {
        Write-Host "  ❌ Servicio $service NO está ejecutándose: Contenedor no encontrado"
        $allRunning = $false
    }
}

# 6. Verificar logs de inicio
Write-Host ""
Write-Host "6. Resumen de logs de inicio:"

Write-Host "Gateway (últimas 15 líneas):"
docker logs gateway-prod --tail 15 2>$null

Write-Host ""
Write-Host "Servicios con problemas:"
$failedServices = docker compose ps --filter "status=stopped" --services
if ($failedServices) {
    foreach ($service in $failedServices) {
        Write-Host "  ❌ $service - DETENIDO"
        Write-Host "     Comando para ver logs: docker compose logs $service"
    }
}
else {
    Write-Host "  ✅ Todos los servicios están ejecutándose"
}

if ($allRunning) {
    Write-Host ""
    Write-Host "✅ DEPLOY COMPLETADO EXITOSAMENTE."
    Write-Host "Todos los servicios están ejecutándose en contenedores Linux."
}
else {
    Write-Host ""
    Write-Host "⚠️  DEPLOY COMPLETADO CON PROBLEMAS."
    Write-Host "Algunos servicios no están ejecutándose correctamente."
}

Write-Host ""
Write-Host "Comandos útiles:"
Write-Host "  Monitorear logs: docker compose logs -f"
Write-Host "  Ver estado: docker compose ps"
Write-Host "  Ver logs específicos: docker compose logs [nombre-servicio]"
Write-Host "  Detener todo: docker compose down"
Write-Host "  Reiniciar servicio: docker compose restart [nombre-servicio]"