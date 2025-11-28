# Script: Create-MasterSolution-DotNet.ps1
# Descripción: Crea una solución maestra usando comandos dotnet
# Ejecutar desde la raíz del backend

# Configuración
$SolutionName = "CarDealer"
$BackendRoot = "./backend"
$Services = @(
    "AdminService",
    "AuthService", 
    "ContactService",
    "ErrorService",
    "Gateway", 
    "MediaService",
    "NotificationService",
    "VehicleService"
)

Write-Host "Creando solución maestra: $SolutionName" -ForegroundColor Green

# 1. Crear la solución
Write-Host "Creando solución..." -ForegroundColor Yellow
dotnet new sln -n $SolutionName -o $BackendRoot

# 2. Agregar proyectos de cada microservicio
foreach ($Service in $Services) {
    Write-Host "`n Procesando microservicio: $Service" -ForegroundColor Cyan
    
    $ProjectTypes = @("Api", "Application", "Domain", "Infrastructure", "Shared")
    
    foreach ($ProjectType in $ProjectTypes) {
        $ProjectPath = "$BackendRoot\$Service\$Service.$ProjectType\$Service.$ProjectType.csproj"
        
        if (Test-Path $ProjectPath) {
            Write-Host "   Agregando: $Service.$ProjectType" -ForegroundColor Green
            dotnet sln $BackendRoot\$SolutionName.sln add $ProjectPath
        } else {
            Write-Host "   No encontrado: $ProjectPath" -ForegroundColor Yellow
        }
    }
}

# 3. Verificar proyectos agregados
Write-Host "`nVerificando proyectos en la solución..." -ForegroundColor Yellow
dotnet sln $BackendRoot\$SolutionName.sln list

Write-Host "`n ¡Solución maestra creada exitosamente!" -ForegroundColor Green
Write-Host " Ubicación: $BackendRoot\$SolutionName.sln" -ForegroundColor White