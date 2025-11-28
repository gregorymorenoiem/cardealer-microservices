# Script para generar bundle de microservicios
# CONFIGURACION - EDITA ESTAS VARIABLES
$basePath = 'C:\Users\gmoreno\source\repos\cardealer'
# $MicroservicePath = 'backend\ErrorService'
# $OutputFileName = 'bundle\error_service_bundle.txt'
# $MicroservicePath = 'backend\AuthService'
# $OutputFileName = 'bundle\auth_service_bundle.txt'
# $MicroservicePath = 'backend\NotificationService'
# $OutputFileName = 'bundle\notification_service_bundle.txt'
# $MicroservicePath = 'backend\AuditService'
# $OutputFileName = 'bundle\audit_service_bundle.txt'
$MicroservicePath = 'backend\MediaService'
$OutputFileName = 'bundle\media_service_bundle.txt'


# Extensiones permitidas
$allowedExtensions = @('.cs', '.csproj', '.sln', '.json', '.yml', '.dev', '.prod', '.html', '.http')

# Carpetas a excluir (cualquier ruta que contenga estas carpetas será excluida)
$excludedFolders = @('bin', 'obj', 'Properties')

# NO EDITAR A PARTIR DE AQUI
$microserviceFullPath = Join-Path $basePath $MicroservicePath
$outputFile = Join-Path $basePath $OutputFileName

Write-Host "INICIANDO GENERACION DE BUNDLE"
Write-Host "Microservicio: $MicroservicePath"
Write-Host "Ruta: $microserviceFullPath"
Write-Host "Excluyendo cualquier archivo en carpetas: $($excludedFolders -join ', ')"

if (-not (Test-Path $microserviceFullPath)) {
    Write-Host "ERROR: La ruta no existe" -ForegroundColor Red
    exit
}

Write-Host "Buscando archivos..."

# Función mejorada para verificar si una ruta debe ser excluida
function Should-ExcludePath {
    param([string]$Path)
    
    # Convertir a minúsculas para comparación case-insensitive
    $lowerPath = $Path.ToLower()
    
    foreach ($excludedFolder in $excludedFolders) {
        # Verificar si la ruta contiene la carpeta excluida
        if ($lowerPath -match "\\$($excludedFolder.ToLower())\\") {
            return $true
        }
    }
    return $false
}

# Obtener todos los archivos primero
$allFiles = Get-ChildItem -Path $microserviceFullPath -Recurse -File

# Filtrar por extensión y excluir rutas no deseadas
$filteredFiles = $allFiles | Where-Object { 
    $allowedExtensions -contains $_.Extension -and -not (Should-ExcludePath $_.FullName)
}

Write-Host "Archivos encontrados después de filtrar: $($filteredFiles.Count)"

if ($filteredFiles.Count -eq 0) {
    Write-Host "No se encontraron archivos después del filtrado. Mostrando diagnóstico..." -ForegroundColor Yellow
    
    # Mostrar qué archivos se encontraron pero fueron excluidos
    Write-Host "Archivos excluidos (por estar en bin, obj o Properties):"
    $excludedFiles = $allFiles | Where-Object { 
        $allowedExtensions -contains $_.Extension -and (Should-ExcludePath $_.FullName)
    }
    
    foreach ($file in $excludedFiles) {
        $relativePath = $file.FullName.Replace($microserviceFullPath, "").TrimStart('\')
        Write-Host "  EXCLUIDO: $relativePath"
    }
    
    # Mostrar archivos que no coinciden con las extensiones
    $wrongExtensionFiles = $allFiles | Where-Object { 
        -not ($allowedExtensions -contains $_.Extension) -and -not (Should-ExcludePath $_.FullName)
    }
    
    if ($wrongExtensionFiles.Count -gt 0) {
        Write-Host "Archivos con extensiones no permitidas:"
        foreach ($file in $wrongExtensionFiles) {
            $relativePath = $file.FullName.Replace($microserviceFullPath, "").TrimStart('\')
            Write-Host "  EXTENSION NO PERMITIDA: $relativePath ($($file.Extension))"
        }
    }
    
    exit
}

if (Test-Path $outputFile) {
    Remove-Item $outputFile -Force
    Write-Host "Archivo anterior eliminado"
}

"## Code Bundle - Aggregated Files" | Out-File -FilePath $outputFile -Encoding UTF8
"## Microservicio: $MicroservicePath" | Out-File -Append -FilePath $outputFile -Encoding UTF8
"## Generado: $(Get-Date)" | Out-File -Append -FilePath $outputFile -Encoding UTF8
"## Carpetas excluidas: $($excludedFolders -join ', ')" | Out-File -Append -FilePath $outputFile -Encoding UTF8
"" | Out-File -Append -FilePath $outputFile -Encoding UTF8

$counter = 0
foreach ($file in $filteredFiles) {
    $counter++
    $relativePath = $file.FullName.Replace($microserviceFullPath, "").TrimStart('\')
    Write-Host "Procesando ($counter/$($filteredFiles.Count)): $relativePath"
    
    "---- File: $relativePath ----" | Out-File -Append -FilePath $outputFile -Encoding UTF8
    try {
        Get-Content -Path $file.FullName -Encoding UTF8 | Out-File -Append -FilePath $outputFile -Encoding UTF8
        "" | Out-File -Append -FilePath $outputFile -Encoding UTF8
    }
    catch {
        "## ERROR: No se pudo leer el archivo" | Out-File -Append -FilePath $outputFile -Encoding UTF8
        "" | Out-File -Append -FilePath $outputFile -Encoding UTF8
    }
}

Write-Host "BUNDLE GENERADO EXITOSAMENTE!"
Write-Host "Archivo: $outputFile"
Write-Host "Total de archivos incluidos: $($filteredFiles.Count)"
Write-Host "Tamaño: $([math]::Round((Get-Item $outputFile).Length / 1KB, 2)) KB"

Write-Host "Resumen por extensiones:"
$filteredFiles | Group-Object Extension | Sort-Object Count -Descending | ForEach-Object {
    Write-Host "  $($_.Name): $($_.Count) archivos"
}

# Mostrar estadísticas de exclusión
$totalFound = ($allFiles | Where-Object { $allowedExtensions -contains $_.Extension }).Count
$excludedCount = $totalFound - $filteredFiles.Count
if ($excludedCount -gt 0) {
    Write-Host "Archivos excluidos: $excludedCount (por estar en bin, obj o Properties)"
}