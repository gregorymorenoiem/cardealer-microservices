#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Script para crear vehículos de prueba para dealers con imágenes de Unsplash.

.DESCRIPTION
    Este script:
    1. Lee dealers de UserService
    2. Lee catálogo de VehiclesSaleService
    3. Crea vehículos con datos realistas
    4. Inserta imágenes usando URLs de Unsplash

.PARAMETER DealerIndex
    Índice del dealer (1-20). Si es 0, procesa todos los dealers.

.PARAMETER VehiclesPerDealer
    Número de vehículos a crear por dealer (default: 5)

.PARAMETER ImagesPerVehicle
    Número de imágenes por vehículo (default: 5)

.EXAMPLE
    .\Seed-VehiclesForDealer.ps1 -DealerIndex 1
    # Procesa solo el primer dealer

.EXAMPLE
    .\Seed-VehiclesForDealer.ps1 -DealerIndex 0
    # Procesa todos los dealers
#>

param(
    [int]$DealerIndex = 1,
    [int]$VehiclesPerDealer = 5,
    [int]$ImagesPerVehicle = 5
)

$ErrorActionPreference = "Stop"

# Colores para output
function Write-Success { param($msg) Write-Host "✅ $msg" -ForegroundColor Green }
function Write-Info { param($msg) Write-Host "ℹ️  $msg" -ForegroundColor Cyan }
function Write-Warning { param($msg) Write-Host "⚠️  $msg" -ForegroundColor Yellow }
function Write-Error { param($msg) Write-Host "❌ $msg" -ForegroundColor Red }

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host "  🚗 SEED VEHICLES FOR DEALERS - CarDealer Microservices" -ForegroundColor Magenta
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host ""

# ============================================================================
# PASO 1: Obtener dealers de UserService
# ============================================================================
Write-Info "Obteniendo dealers de UserService..."

$dealersQuery = @'
SELECT "Id", "BusinessName", "TradeName", "Email", "Phone", "Address", "City", "State", "Latitude", "Longitude"
FROM "Dealers" 
WHERE "IsDeleted" = false AND "IsActive" = true
ORDER BY "BusinessName"
'@

$dealersRaw = docker exec userservice-db psql -U postgres -d userservice -t -A -F '|' -c $dealersQuery 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error obteniendo dealers: $dealersRaw"
    exit 1
}

$dealers = @()
foreach ($line in $dealersRaw -split "`n") {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    $parts = $line -split '\|'
    if ($parts.Count -ge 10) {
        $dealers += [PSCustomObject]@{
            Id = $parts[0]
            BusinessName = $parts[1]
            TradeName = $parts[2]
            Email = $parts[3]
            Phone = $parts[4]
            Address = $parts[5]
            City = $parts[6]
            State = $parts[7]
            Latitude = $parts[8]
            Longitude = $parts[9]
        }
    }
}

Write-Success "Encontrados $($dealers.Count) dealers"

# ============================================================================
# PASO 2: Obtener catálogo de VehiclesSaleService
# ============================================================================
Write-Info "Obteniendo catálogo de vehículos..."

$catalogQuery = @'
SELECT 
    m."Id", m."Name", m."Slug",
    mo."Id", mo."Name", mo."Slug", mo."VehicleType", mo."BodyStyle",
    t."Id", t."Name", t."Year", t."BaseMSRP", t."EngineSize", t."Horsepower", t."Torque",
    t."FuelType", t."Transmission", t."DriveType", t."MpgCity", t."MpgHighway", t."MpgCombined"
FROM vehicle_makes m 
JOIN vehicle_models mo ON mo."MakeId" = m."Id" 
JOIN vehicle_trims t ON t."ModelId" = mo."Id" 
WHERE t."IsActive" = true
ORDER BY RANDOM()
'@

$catalogRaw = docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -t -A -F '|' -c $catalogQuery 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error obteniendo catálogo: $catalogRaw"
    exit 1
}

$catalog = @()
foreach ($line in $catalogRaw -split "`n") {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    $parts = $line -split '\|'
    if ($parts.Count -ge 21) {
        $catalog += [PSCustomObject]@{
            MakeId = $parts[0]
            Make = $parts[1]
            MakeSlug = $parts[2]
            ModelId = $parts[3]
            Model = $parts[4]
            ModelSlug = $parts[5]
            VehicleType = $parts[6]
            BodyStyle = $parts[7]
            TrimId = $parts[8]
            Trim = $parts[9]
            Year = [int]$parts[10]
            BaseMSRP = [decimal]($parts[11] -replace '[^\d.]', '')
            EngineSize = $parts[12]
            Horsepower = if ($parts[13]) { [int]$parts[13] } else { 200 }
            Torque = if ($parts[14]) { [int]$parts[14] } else { 200 }
            FuelType = $parts[15]
            Transmission = $parts[16]
            DriveType = $parts[17]
            MpgCity = if ($parts[18]) { [int]$parts[18] } else { 25 }
            MpgHighway = if ($parts[19]) { [int]$parts[19] } else { 32 }
            MpgCombined = if ($parts[20]) { [int]$parts[20] } else { 28 }
        }
    }
}

Write-Success "Encontrados $($catalog.Count) trims en el catálogo"

# ============================================================================
# PASO 3: Obtener categorías
# ============================================================================
Write-Info "Obteniendo categorías..."

$categoriesQuery = 'SELECT "Id", "Name" FROM categories ORDER BY "Name"'
$categoriesRaw = docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -t -A -F '|' -c $categoriesQuery 2>&1

$categories = @{}
foreach ($line in $categoriesRaw -split "`n") {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    $parts = $line -split '\|'
    if ($parts.Count -ge 2) {
        $categories[$parts[1]] = $parts[0]
    }
}

Write-Success "Encontradas $($categories.Count) categorías"

# ============================================================================
# FUNCIONES AUXILIARES
# ============================================================================

function Get-CategoryId {
    param($vehicleType, $bodyStyle)
    
    switch -Regex ($vehicleType) {
        "Car|Sedan|Coupe" { return $categories["Cars"] }
        "Truck|Pickup" { return $categories["Trucks"] }
        "SUV|Crossover" { return $categories["SUVs & Crossovers"] }
        "Motorcycle" { return $categories["Motorcycles"] }
        default { return $categories["Cars"] }
    }
}

function Get-RandomMileage {
    param($condition)
    if ($condition -eq "New") {
        return Get-Random -Minimum 0 -Maximum 500
    } else {
        return Get-Random -Minimum 5000 -Maximum 80000
    }
}

function Get-RandomCondition {
    $rand = Get-Random -Minimum 1 -Maximum 100
    if ($rand -le 30) { return "New" }
    elseif ($rand -le 70) { return "Used" }
    else { return "Certified" }
}

function Get-RandomColor {
    $colors = @("Black", "White", "Silver", "Gray", "Red", "Blue", "Pearl White", "Midnight Black", "Ocean Blue", "Burgundy")
    return $colors | Get-Random
}

function Get-RandomInteriorColor {
    $colors = @("Black", "Beige", "Gray", "Brown", "Tan", "Cream")
    return $colors | Get-Random
}

function Get-RandomInteriorMaterial {
    $materials = @("Leather", "Cloth", "Leatherette", "Premium Leather", "Synthetic")
    return $materials | Get-Random
}

function Get-VehicleFeatures {
    $allFeatures = @(
        "Bluetooth", "Apple CarPlay", "Android Auto", "Navigation", "Backup Camera",
        "Blind Spot Monitor", "Lane Departure Warning", "Adaptive Cruise Control",
        "Heated Seats", "Cooled Seats", "Sunroof", "Panoramic Roof", "LED Headlights",
        "Keyless Entry", "Push Button Start", "Remote Start", "Power Liftgate",
        "Wireless Charging", "Premium Sound System", "360 Camera", "Parking Sensors"
    )
    $count = Get-Random -Minimum 5 -Maximum 12
    $selected = $allFeatures | Get-Random -Count $count
    return ($selected | ForEach-Object { "`"$_`"" }) -join ","
}

function Get-UnsplashImageUrl {
    param($make, $model, $index)
    
    # Limpiar nombres para URL
    $searchMake = $make -replace '\s+', '-' -replace '[^a-zA-Z0-9-]', ''
    $searchModel = $model -replace '\s+', '-' -replace '[^a-zA-Z0-9-]', ''
    
    # Generar signature único para cada imagen
    $sig = [guid]::NewGuid().ToString().Substring(0, 8)
    
    # URL de Unsplash Source (redirects a imagen aleatoria)
    return "https://source.unsplash.com/800x600/?$searchMake,$searchModel,car&sig=$sig"
}

function Get-UnsplashThumbnailUrl {
    param($make, $model, $index)
    
    $searchMake = $make -replace '\s+', '-' -replace '[^a-zA-Z0-9-]', ''
    $searchModel = $model -replace '\s+', '-' -replace '[^a-zA-Z0-9-]', ''
    $sig = [guid]::NewGuid().ToString().Substring(0, 8)
    
    return "https://source.unsplash.com/400x300/?$searchMake,$searchModel,car&sig=$sig"
}

# ============================================================================
# PASO 4: Procesar dealers
# ============================================================================

# Filtrar dealers según parámetro
if ($DealerIndex -gt 0) {
    if ($DealerIndex -gt $dealers.Count) {
        Write-Error "DealerIndex $DealerIndex es mayor que el número de dealers ($($dealers.Count))"
        exit 1
    }
    $dealersToProcess = @($dealers[$DealerIndex - 1])
    Write-Info "Procesando solo dealer #$DealerIndex : $($dealersToProcess[0].BusinessName)"
} else {
    $dealersToProcess = $dealers
    Write-Info "Procesando TODOS los $($dealers.Count) dealers"
}

$totalVehiclesCreated = 0
$totalImagesCreated = 0

foreach ($dealer in $dealersToProcess) {
    Write-Host ""
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Blue
    Write-Host "  🏢 Dealer: $($dealer.BusinessName)" -ForegroundColor Blue
    Write-Host "  📍 $($dealer.City), $($dealer.State)" -ForegroundColor Blue
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Blue
    
    # Seleccionar trims aleatorios para este dealer
    $selectedTrims = $catalog | Get-Random -Count ([Math]::Min($VehiclesPerDealer, $catalog.Count))
    
    $vehicleIndex = 0
    foreach ($trim in $selectedTrims) {
        $vehicleIndex++
        $vehicleId = [guid]::NewGuid().ToString()
        
        # Datos del vehículo
        $condition = Get-RandomCondition
        $mileage = Get-RandomMileage -condition $condition
        $exteriorColor = Get-RandomColor
        $interiorColor = Get-RandomInteriorColor
        $interiorMaterial = Get-RandomInteriorMaterial
        $categoryId = Get-CategoryId -vehicleType $trim.VehicleType -bodyStyle $trim.BodyStyle
        
        # Precio con variación
        $priceVariation = Get-Random -Minimum -3000 -Maximum 5000
        $price = [Math]::Max(15000, $trim.BaseMSRP + $priceVariation)
        
        # Título y descripción
        $title = "$($trim.Year) $($trim.Make) $($trim.Model) $($trim.Trim)"
        $description = "Hermoso $($trim.Year) $($trim.Make) $($trim.Model) $($trim.Trim) en excelentes condiciones. " +
                       "Motor $($trim.EngineSize) con $($trim.Horsepower) HP. " +
                       "Transmisión $($trim.Transmission), tracción $($trim.DriveType). " +
                       "Color exterior $exteriorColor con interior $interiorColor en $interiorMaterial. " +
                       "Consumo: $($trim.MpgCity) ciudad / $($trim.MpgHighway) carretera MPG. " +
                       "Disponible en $($dealer.TradeName), $($dealer.City)."
        
        # VIN aleatorio
        $vin = "1" + (-join ((65..90) + (48..57) | Get-Random -Count 16 | ForEach-Object { [char]$_ }))
        $stockNumber = "STK-" + (Get-Random -Minimum 10000 -Maximum 99999)
        
        # Features JSON
        $featuresJson = "[" + (Get-VehicleFeatures) + "]"
        
        Write-Info "  Creando vehículo $vehicleIndex/$VehiclesPerDealer : $title"
        
        # SQL para insertar vehículo
        $vehicleSql = @"
INSERT INTO vehicles (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerId", "SellerName", "SellerType", "SellerPhone", "SellerEmail", "SellerVerified", "SellerCity", "SellerState",
    "VIN", "StockNumber", "MakeId", "Make", "ModelId", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats", "FuelType", "EngineSize", "Horsepower", "Torque",
    "Transmission", "DriveType", "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor", "InteriorMaterial",
    "MpgCity", "MpgHighway", "MpgCombined",
    "City", "State", "Country", "Latitude", "Longitude",
    "IsCertified", "HasCleanTitle", "AccidentHistory", "PreviousOwners",
    "FeaturesJson", "PackagesJson", "ViewCount", "FavoriteCount", "InquiryCount",
    "CreatedAt", "UpdatedAt", "PublishedAt", "IsDeleted", "IsFeatured", "CategoryId"
) VALUES (
    '$vehicleId', '$($dealer.Id)', '$($title -replace "'", "''")', '$($description -replace "'", "''"))', 
    $price, 'USD', 'Active',
    '$($dealer.Id)', '$($dealer.TradeName -replace "'", "''")', 1, '$($dealer.Phone)', '$($dealer.Email)', true, '$($dealer.City)', '$($dealer.State)',
    '$vin', '$stockNumber', '$($trim.MakeId)', '$($trim.Make)', '$($trim.ModelId)', '$($trim.Model)', '$($trim.Trim)', $($trim.Year),
    '$($trim.VehicleType)', '$($trim.BodyStyle)', 4, 5, '$($trim.FuelType)', '$($trim.EngineSize)', $($trim.Horsepower), $($trim.Torque),
    '$($trim.Transmission)', '$($trim.DriveType)', $mileage, 'Miles', '$condition',
    '$exteriorColor', '$interiorColor', '$interiorMaterial',
    $($trim.MpgCity), $($trim.MpgHighway), $($trim.MpgCombined),
    '$($dealer.City)', '$($dealer.State)', 'DO', $($dealer.Latitude), $($dealer.Longitude),
    $(if ($condition -eq "Certified") { "true" } else { "false" }), true, false, $(Get-Random -Minimum 0 -Maximum 3),
    '$featuresJson', '[]', $(Get-Random -Minimum 50 -Maximum 500), $(Get-Random -Minimum 5 -Maximum 50), $(Get-Random -Minimum 1 -Maximum 20),
    NOW(), NOW(), NOW(), false, $(if ((Get-Random -Minimum 1 -Maximum 10) -le 3) { "true" } else { "false" }), '$categoryId'
);
"@
        
        # Ejecutar INSERT del vehículo
        $result = docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -c $vehicleSql 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Error creando vehículo: $result"
            continue
        }
        
        $totalVehiclesCreated++
        
        # Crear imágenes
        for ($imgIndex = 1; $imgIndex -le $ImagesPerVehicle; $imgIndex++) {
            $imageId = [guid]::NewGuid().ToString()
            $imageUrl = Get-UnsplashImageUrl -make $trim.Make -model $trim.Model -index $imgIndex
            $thumbnailUrl = Get-UnsplashThumbnailUrl -make $trim.Make -model $trim.Model -index $imgIndex
            $isPrimary = if ($imgIndex -eq 1) { "true" } else { "false" }
            $caption = if ($imgIndex -eq 1) { "Vista principal" } 
                       elseif ($imgIndex -eq 2) { "Interior" }
                       elseif ($imgIndex -eq 3) { "Vista lateral" }
                       elseif ($imgIndex -eq 4) { "Motor" }
                       else { "Vista adicional" }
            
            $imageSql = @"
INSERT INTO vehicle_images (
    "Id", "DealerId", "VehicleId", "Url", "ThumbnailUrl", "Caption", 
    "ImageType", "SortOrder", "IsPrimary", "CreatedAt"
) VALUES (
    '$imageId', '$($dealer.Id)', '$vehicleId', 
    '$imageUrl', '$thumbnailUrl', '$caption',
    0, $imgIndex, $isPrimary, NOW()
);
"@
            
            $result = docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -c $imageSql 2>&1
            if ($LASTEXITCODE -ne 0) {
                Write-Warning "Error creando imagen $imgIndex : $result"
            } else {
                $totalImagesCreated++
            }
        }
        
        Write-Success "    ✓ Vehículo creado con $ImagesPerVehicle imágenes"
    }
}

# ============================================================================
# RESUMEN FINAL
# ============================================================================
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  ✅ SEED COMPLETADO" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "  📊 Resumen:" -ForegroundColor White
Write-Host "     • Dealers procesados: $($dealersToProcess.Count)" -ForegroundColor White
Write-Host "     • Vehículos creados:  $totalVehiclesCreated" -ForegroundColor White
Write-Host "     • Imágenes creadas:   $totalImagesCreated" -ForegroundColor White
Write-Host ""

# Verificar conteo final
$countQuery = 'SELECT COUNT(*) FROM vehicles WHERE "IsDeleted" = false'
$vehicleCount = docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -t -c $countQuery 2>&1
Write-Host "  🚗 Total vehículos en BD: $($vehicleCount.Trim())" -ForegroundColor Cyan

$imageCountQuery = 'SELECT COUNT(*) FROM vehicle_images'
$imageCount = docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -t -c $imageCountQuery 2>&1
Write-Host "  🖼️  Total imágenes en BD:  $($imageCount.Trim())" -ForegroundColor Cyan
Write-Host ""
