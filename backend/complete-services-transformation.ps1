# Script maestro para completar RoleService y UserService
# Ejecuta todas las transformaciones necesarias

Write-Host "=== TRANSFORMACIÓN COMPLETA DE MICROSERVICIOS ===" -ForegroundColor Cyan

$basePath = "C:\Users\gmoreno\source\repos\cardealer\backend"

# PASO 1: Renombrar directorios y archivos de UserService que aún tienen "RoleService"
Write-Host "`n[1/5] Renombrando estructura de UserService..." -ForegroundColor Yellow
cd "$basePath\UserService"

Get-ChildItem -Recurse -Directory | Where-Object { $_.Name -like "*RoleService*" } | ForEach-Object {
    $newName = $_.Name -replace 'RoleService', 'UserService'
    $newPath = Join-Path $_.Parent.FullName $newName
    if (Test-Path $newPath) {
        Write-Host "  ⚠ Ya existe: $newPath" -ForegroundColor DarkYellow
    } else {
        Rename-Item $_.FullName -NewName $newName -ErrorAction SilentlyContinue
        Write-Host "  ✓ Renombrado: $($_.Name) -> $newName" -ForegroundColor Green
    }
}

Get-ChildItem -Recurse -File | Where-Object { $_.Name -like "*RoleService*" } | ForEach-Object {
    $newName = $_.Name -replace 'RoleService', 'UserService'
    $newPath = Join-Path $_.Directory.FullName $newName
    if (!(Test-Path $newPath)) {
        Rename-Item $_.FullName -NewName $newName -ErrorAction SilentlyContinue
        Write-Host "  ✓ Archivo: $($_.Name) -> $newName" -ForegroundColor Green
    }
}

# PASO 2: Reemplazar contenido en todos los archivos de UserService
Write-Host "`n[2/5] Reemplazando contenido en UserService..." -ForegroundColor Yellow
Get-ChildItem -Recurse -Include *.cs,*.csproj,*.sln,*.json,*.md -Exclude bin,obj | ForEach-Object {
    try {
        $content = Get-Content $_.FullName -Raw -ErrorAction Stop
        $updated = $content -replace 'RoleService', 'UserService' -replace 'roleservice', 'userservice'
        if ($content -ne $updated) {
            Set-Content $_.FullName -Value $updated -NoNewline
            Write-Host "  ✓ Actualizado: $($_.FullName)" -ForegroundColor Green
        }
    } catch {
        Write-Host "  ⚠ Error en: $($_.FullName)" -ForegroundColor Red
    }
}

# PASO 3: Limpiar archivos de documentación innecesarios
Write-Host "`n[3/5] Limpiando archivos de documentación..." -ForegroundColor Yellow
$docsToRemove = @(
    "$basePath\RoleService\*.md",
    "$basePath\UserService\*.md",
    "$basePath\RoleService\GenerateTestToken.cs",
    "$basePath\UserService\GenerateTestToken.cs",
    "$basePath\RoleService\E2E-TESTING-SCRIPT.ps1",
    "$basePath\UserService\E2E-TESTING-SCRIPT.ps1"
)

foreach ($pattern in $docsToRemove) {
    Get-Item $pattern -ErrorAction SilentlyContinue | ForEach-Object {
        Remove-Item $_.FullName -Force
        Write-Host "  ✓ Eliminado: $($_.Name)" -ForegroundColor Green
    }
}

# PASO 4: Crear README.md para cada servicio
Write-Host "`n[4/5] Creando README.md..." -ForegroundColor Yellow

$roleReadme = @"
# RoleService

Microservicio para gestión de roles y permisos en CarDealer.

## Características
- CRUD completo de roles
- Gestión de permisos
- Asignación de permisos a roles
- Control de acceso basado en roles (RBAC)
- Rate limiting
- Autenticación JWT
- OpenTelemetry integration
- PostgreSQL database

## Endpoints principales
- `POST /api/roles` - Crear rol
- `GET /api/roles` - Listar roles
- `GET /api/roles/{id}` - Obtener rol
- `PUT /api/roles/{id}` - Actualizar rol
- `DELETE /api/roles/{id}` - Eliminar rol
- `POST /api/roles/{id}/permissions` - Asignar permiso
- `GET /api/permissions` - Listar permisos

## Tecnologías
- .NET 8
- Entity Framework Core
- PostgreSQL
- RabbitMQ
- MediatR + FluentValidation
"@

$userReadme = @"
# UserService

Microservicio para gestión de usuarios en CarDealer.

## Características
- CRUD completo de usuarios
- Gestión de perfiles de usuario
- Asignación de roles a usuarios
- Autenticación y autorización
- Rate limiting
- JWT authentication
- OpenTelemetry integration
- PostgreSQL database

## Endpoints principales
- `POST /api/users` - Crear usuario
- `GET /api/users` - Listar usuarios
- `GET /api/users/{id}` - Obtener usuario
- `PUT /api/users/{id}` - Actualizar usuario
- `DELETE /api/users/{id}` - Eliminar usuario
- `POST /api/users/{id}/roles` - Asignar rol
- `GET /api/users/{id}/permissions` - Obtener permisos del usuario

## Tecnologías
- .NET 8
- Entity Framework Core
- PostgreSQL
- RabbitMQ
- MediatR + FluentValidation
"@

Set-Content "$basePath\RoleService\README.md" -Value $roleReadme
Set-Content "$basePath\UserService\README.md" -Value $userReadme

Write-Host "  ✓ READMEs creados" -ForegroundColor Green

# PASO 5: Compilar para verificar
Write-Host "`n[5/5] Verificando compilación..." -ForegroundColor Yellow

cd "$basePath\RoleService"
$buildRole = dotnet build RoleService.sln --no-restore 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✅ RoleService compila correctamente" -ForegroundColor Green
} else {
    Write-Host "  ⚠ RoleService tiene errores de compilación" -ForegroundColor Red
}

cd "$basePath\UserService"
$buildUser = dotnet build UserService.sln --no-restore 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✅ UserService compila correctamente" -ForegroundColor Green
} else {
    Write-Host "  ⚠ UserService tiene errores de compilación" -ForegroundColor Red
}

Write-Host "`n✅ TRANSFORMACIÓN COMPLETADA!" -ForegroundColor Green
Write-Host "Próximo paso: Ajustar entidades específicas de negocio para cada servicio" -ForegroundColor Cyan
