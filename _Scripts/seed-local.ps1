# Script: seed-local.ps1
# Descripción: Seed de datos de prueba en ambiente local (PowerShell)
# Uso: .\seed-local.ps1
# Compatible: Windows PowerShell 5.1+

param(
    [string]$ApiUrl = "http://localhost:18443",
    [string]$DbPassword = "postgres",
    [string]$DbUser = "postgres",
    [string]$DbName = "cardealer",
    [string]$DbHost = "localhost",
    [switch]$Clean = $false
)

# Funciones de logging
function Log-Step {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Log-Error {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Log-Info {
    param([string]$Message)
    Write-Host "ℹ $Message" -ForegroundColor Cyan
}

function Log-Warning {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

# Encabezado
Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Blue
Write-Host "║         OKLA - Data Seeding Script (Local)                 ║" -ForegroundColor Blue
Write-Host "║          30 Dealers | 20 Users | 150 Vehicles             ║" -ForegroundColor Blue
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Blue
Write-Host ""

# PASO 0: Validaciones
Log-Info "Validando requisitos previos..."

# Verificar API
try {
    $response = Invoke-RestMethod -Uri "$ApiUrl/health" -Method Get -ErrorAction SilentlyContinue
    Log-Step "API disponible en $ApiUrl"
}
catch {
    Log-Error "API no disponible en $ApiUrl"
    Write-Host "Asegúrate de que:"
    Write-Host "  1. La API Gateway está corriendo"
    Write-Host "  2. PostgreSQL está corriendo"
    exit 1
}

# PASO 1: Limpiar datos (opcional)
if ($Clean) {
    Log-Warning "Limpiando datos existentes..."
    
    $cleanQuery = @"
    SET session_replication_role = 'replica';
    TRUNCATE TABLE vehicle_images CASCADE;
    TRUNCATE TABLE vehicles CASCADE;
    TRUNCATE TABLE dealers CASCADE;
    TRUNCATE TABLE users CASCADE;
    SET session_replication_role = 'origin';
    ALTER SEQUENCE users_id_seq RESTART WITH 1;
    ALTER SEQUENCE dealers_id_seq RESTART WITH 1;
    ALTER SEQUENCE vehicles_id_seq RESTART WITH 1;
    ALTER SEQUENCE vehicle_images_id_seq RESTART WITH 1;
"@

    # Ejecutar query PostgreSQL
    # Nota: Requiere pgAdmin instalado o PSQL CLI
    # psql -h $DbHost -U $DbUser -d $DbName -c $cleanQuery
    
    Log-Step "Base de datos limpiada"
}

# PASO 2: Crear usuarios
Write-Host ""
Log-Info "Creando 20 usuarios (10 buyers + 10 sellers)..."

function Create-Users {
    param(
        [int]$Count,
        [string]$Type
    )
    
    for ($i = 1; $i -le $Count; $i++) {
        $email = "${Type}${i}@okla.local"
        $firstName = "User"
        $lastName = "${Type}${i}"
        $password = "SecurePass123!@"
        
        $body = @{
            email = $email
            firstName = $firstName
            lastName = $lastName
            password = $password
            accountType = $Type
        } | ConvertTo-Json
        
        try {
            $response = Invoke-RestMethod -Uri "$ApiUrl/api/auth/register" `
                -Method Post `
                -ContentType "application/json" `
                -Body $body `
                -ErrorAction SilentlyContinue
            
            if ($response.id) {
                Write-Host "  ✓ $email" -ForegroundColor Green
            }
        }
        catch {
            Write-Host "  ✗ $email (error)" -ForegroundColor Red
        }
    }
}

Create-Users -Count 10 -Type "Buyer"
Create-Users -Count 10 -Type "Seller"

Log-Step "Usuarios creados"

# PASO 3: Dealers y Vehículos
Write-Host ""
Log-Info "Para crear dealers y vehículos:"
Log-Info "  1. Abre una PowerShell como Administrador"
Log-Info "  2. Ve a: cd $PSScriptRoot"
Log-Info "  3. Ejecuta: dotnet run --project VehiclesSaleService.Api seed:all"
Log-Info ""
Log-Warning "Alternativamente, usa el C# Seeding Service"
Log-Warning "Ver: docs/DATA_SEEDING_STRATEGY.md"

# RESUMEN FINAL
Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Blue
Write-Host "║ 📊 RESUMEN DE SEEDING                                      ║" -ForegroundColor Blue
Write-Host "║                                                            ║" -ForegroundColor Blue
Write-Host "║ ✓ 👥 Usuarios: 20                                          ║" -ForegroundColor Blue
Write-Host "║ ⏳ 🏪 Dealers: 30 (usa C# service)                         ║" -ForegroundColor Blue
Write-Host "║ ⏳ 🚗 Vehículos: 150 (usa C# service)                      ║" -ForegroundColor Blue
Write-Host "║ ⏳ 🖼️  Imágenes: 7,500 (URLs auto-generadas)              ║" -ForegroundColor Blue
Write-Host "║                                                            ║" -ForegroundColor Blue
Write-Host "║ API: $ApiUrl" -ForegroundColor Blue
Write-Host "║ DB:  $DbHost/$DbName" -ForegroundColor Blue
Write-Host "║                                                            ║" -ForegroundColor Blue
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Blue

Write-Host ""
Log-Step "Seeding completado!"
Write-Host ""
Write-Host "Próximos pasos:" -ForegroundColor Cyan
Write-Host "  1. Ejecutar C# Seeding Service para dealers/vehículos"
Write-Host "  2. Validar en: $ApiUrl/api/vehicles"
Write-Host "  3. Probar APIs con datos reales"
Write-Host ""
