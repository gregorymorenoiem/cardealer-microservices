param(
    [string]$Configuration = "Release"
)

# =========================
# Configuracion de rutas
# =========================
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$DeployPath = $ScriptDir
$BasePath = Split-Path $ScriptDir -Parent

Write-Host "=== PUBLICANDO MICROSERVICIOS PARA DEPLOY ==="
Write-Host "Ruta base del proyecto: $BasePath"
Write-Host "Directorio de despliegue: $DeployPath"

# =========================
# Diagnostico de estructura de carpetas
# =========================
Write-Host ""
Write-Host "=== DIAGNOSTICO DE ESTRUCTURA ==="
Write-Host "Contenido del directorio base:"
Get-ChildItem $BasePath -Directory | Select-Object Name | Format-Table -AutoSize

# =========================
# Buscar proyectos .csproj recursivamente
# =========================
Write-Host ""
Write-Host "=== BUSCANDO PROYECTOS .CSPROJ ==="
$allCsprojFiles = Get-ChildItem $BasePath -Recurse -Filter "*.csproj" | Select-Object FullName, Name

if ($allCsprojFiles.Count -eq 0) {
    Write-Host "No se encontraron archivos .csproj en el directorio base"
}
else {
    Write-Host "Archivos .csproj encontrados:"
    $allCsprojFiles | ForEach-Object { 
        $relativePath = $_.FullName.Replace($BasePath, "").TrimStart("\")
        Write-Host "  - $relativePath"
    }
}

# =========================
# Definicion de Microservicios con busqueda flexible
# =========================
$servicePatterns = @(
    @{Name = "ErrorService"; Pattern = "*ErrorService*.Api.csproj" },
    @{Name = "AuthService"; Pattern = "*AuthService*.Api.csproj" },
    @{Name = "NotificationService"; Pattern = "*NotificationService*.Api.csproj" },
    @{Name = "VehicleService"; Pattern = "*VehicleService*.Api.csproj" },
    @{Name = "ContactService"; Pattern = "*ContactService*.Api.csproj" },
    @{Name = "MediaService"; Pattern = "*MediaService*.Api.csproj" },
    @{Name = "AuditService"; Pattern = "*AuditService*.Api.csproj" },
    @{Name = "AdminService"; Pattern = "*AdminService*.Api.csproj" },
    @{Name = "Gateway"; Pattern = "*Gateway*.Api.csproj" }
)

# Buscar proyectos usando patrones
$services = @()
foreach ($servicePattern in $servicePatterns) {
    $foundProject = Get-ChildItem $BasePath -Recurse -Filter $servicePattern.Pattern | Select-Object -First 1
    
    if ($foundProject) {
        $relativePath = $foundProject.FullName.Replace($BasePath, "").TrimStart("\")
        $services += @{Name = $servicePattern.Name; ProjectPath = $relativePath; FullPath = $foundProject.FullName }
    }
    else {
        Write-Host "NO ENCONTRADO: $($servicePattern.Name) - Patron: $($servicePattern.Pattern)"
    }
}

Write-Host ""
Write-Host "=== PROYECTOS ENCONTRADOS ==="
foreach ($service in $services) {
    Write-Host "  $($service.Name): $($service.ProjectPath)"
}

if ($services.Count -eq 0) {
    Write-Host "No se encontraron proyectos. Verifica la estructura de carpetas."
    exit 1
}

# =========================
# Preparacion del directorio
# =========================
if (-not (Test-Path $DeployPath)) {
    Write-Host "Creando directorio de despliegue: $DeployPath"
    New-Item -ItemType Directory -Path $DeployPath -Force | Out-Null
}

# =========================
# Publicacion de servicios
# =========================
foreach ($service in $services) {
    Write-Host ""
    Write-Host "PUBLICANDO $($service.Name)..."
    
    # Configurar rutas
    $ServiceDir = $service.Name.ToLower()
    $servicePath = Join-Path $DeployPath $ServiceDir
    $appPath = Join-Path $servicePath "app"
    $ProjectFullPath = $service.FullPath
    $ApiDllName = "$($service.Name).Api.dll"
    
    # Limpiar directorio existente
    if (Test-Path $servicePath) {
        Write-Host "  Limpiando directorio existente..."
        Remove-Item $servicePath -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    # Crear estructura de directorios
    New-Item -ItemType Directory -Path $appPath -Force | Out-Null
    
    # Publicar
    Write-Host "  Ejecutando: dotnet publish $($service.ProjectPath) -c $Configuration -o $appPath"
    
    # Cambiar al directorio base para publicar
    Push-Location $BasePath
    dotnet publish $service.ProjectPath -c $Configuration -o $appPath --verbosity minimal
    $publishExitCode = $LASTEXITCODE
    Pop-Location
    
    if ($publishExitCode -ne 0) {
        Write-Host "  ERROR: dotnet publish fallo. Codigo de salida: $publishExitCode"
        continue
    }
    
    # Verificar que se crearon archivos
    $filesInApp = Get-ChildItem $appPath -ErrorAction SilentlyContinue
    if ($filesInApp.Count -eq 0) {
        Write-Host "  ADVERTENCIA: La carpeta 'app' esta vacia despues de publicar"
        continue
    }
    
    Write-Host "  Publicacion exitosa. Archivos generados: $($filesInApp.Count)"
    
    # Crear Dockerfile
    $dockerfileContent = @"
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY app/ .
ENTRYPOINT ["dotnet", "$ApiDllName"]
"@
    
    $dockerfileContent | Out-File -FilePath (Join-Path $servicePath "Dockerfile") -Encoding UTF8
    Write-Host "  Dockerfile creado"
    
    # Buscar el archivo DLL principal
    $dllFile = Get-ChildItem $appPath -Filter "*.dll" | Where-Object { $_.Name -like "*$($service.Name)*" } | Select-Object -First 1
    
    if ($dllFile) {
        Write-Host "  Archivo principal: $($dllFile.Name)"
    }
    else {
        Write-Host "  ADVERTENCIA: No se encontro DLL con nombre relacionado a $($service.Name)"
        Write-Host "  Archivos DLL encontrados:"
        Get-ChildItem $appPath -Filter "*.dll" | ForEach-Object { Write-Host "    - $($_.Name)" }
    }
    
    # Limpiar archivos de debug
    Get-ChildItem $appPath -Filter "*.pdb" -ErrorAction SilentlyContinue | Remove-Item -ErrorAction SilentlyContinue
    Get-ChildItem $appPath -Filter "*.xml" -ErrorAction SilentlyContinue | Remove-Item -ErrorAction SilentlyContinue
}

# =========================
# Verificacion final
# =========================
Write-Host ""
Write-Host "=== VERIFICACION FINAL ==="

$createdServices = Get-ChildItem $DeployPath -Directory | Where-Object { $services.Name -contains $_.Name.Replace("service", "Service") }

if ($createdServices.Count -eq 0) {
    Write-Host "No se crearon directorios de servicios."
}
else {
    foreach ($serviceDir in $createdServices) {
        $servicePath = $serviceDir.FullName
        $appPath = Join-Path $servicePath "app"
        
        Write-Host ""
        Write-Host "$($serviceDir.Name):"
        
        # Verificar Dockerfile
        $dockerfileExists = Test-Path (Join-Path $servicePath "Dockerfile")
        if ($dockerfileExists) {
            Write-Host "  Dockerfile: EXISTE"
        }
        else {
            Write-Host "  Dockerfile: NO EXISTE"
        }
        
        # Verificar contenido de app
        if (Test-Path $appPath) {
            $files = Get-ChildItem $appPath -File
            Write-Host "  Archivos en 'app': $($files.Count)"
            
            if ($files.Count -gt 0) {
                # Mostrar primeros 5 archivos
                $files | Select-Object -First 5 | ForEach-Object { 
                    Write-Host "    - $($_.Name)" 
                }
                if ($files.Count -gt 5) {
                    Write-Host "    ... y $($files.Count - 5) mas"
                }
            }
            else {
                Write-Host "  ADVERTENCIA: Carpeta 'app' vacia"
            }
        }
        else {
            Write-Host "  ERROR: Carpeta 'app' no existe"
        }
    }
}

Write-Host ""
Write-Host "=== ANALISIS COMPLETADO ==="