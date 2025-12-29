# CarDealer Mobile - Build Script (PowerShell)
# Genera builds para todos los flavors y plataformas

param(
    [string]$Action = "menu",
    [string]$Platform = "all",
    [string]$Flavor = "all"
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 CarDealer Mobile Build Script" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# Verificar Flutter
if (-not (Get-Command flutter -ErrorAction SilentlyContinue)) {
    Write-Host "✗ Flutter no está instalado" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Flutter version:" -ForegroundColor Green
flutter --version

# Función para build Android
function Build-Android {
    param(
        [string]$Flavor,
        [string]$Target,
        [string]$BuildType = "apk"
    )
    
    Write-Host "Building Android $Flavor ($BuildType)..." -ForegroundColor Yellow
    
    if ($BuildType -eq "apk") {
        flutter build apk --release --flavor $Flavor -t $Target
    }
    else {
        flutter build appbundle --release --flavor $Flavor -t $Target
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Android $Flavor $BuildType build completado" -ForegroundColor Green
    }
    else {
        Write-Host "✗ Error en Android $Flavor build" -ForegroundColor Red
        exit 1
    }
}

# Función para build iOS
function Build-iOS {
    param(
        [string]$Flavor,
        [string]$Target,
        [bool]$BuildIPA = $false
    )
    
    Write-Host "Building iOS $Flavor..." -ForegroundColor Yellow
    
    if ($BuildIPA) {
        flutter build ipa --release --flavor $Flavor -t $Target
    }
    else {
        flutter build ios --release --flavor $Flavor -t $Target --no-codesign
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ iOS $Flavor build completado" -ForegroundColor Green
    }
    else {
        Write-Host "✗ Error en iOS $Flavor build" -ForegroundColor Red
        exit 1
    }
}

# Función para limpiar
function Clean-Build {
    Write-Host "Limpiando builds anteriores..." -ForegroundColor Yellow
    flutter clean
    Remove-Item -Path "build" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "✓ Limpieza completada" -ForegroundColor Green
}

# Menú interactivo
function Show-Menu {
    Write-Host ""
    Write-Host "Selecciona una opción:" -ForegroundColor Cyan
    Write-Host "1.  Build Android DEV (APK)"
    Write-Host "2.  Build Android STAGING (APK)"
    Write-Host "3.  Build Android PROD (APK)"
    Write-Host "4.  Build Android PROD (AAB - Play Store)"
    Write-Host "5.  Build iOS DEV"
    Write-Host "6.  Build iOS STAGING"
    Write-Host "7.  Build iOS PROD"
    Write-Host "8.  Build iOS PROD (IPA - App Store)"
    Write-Host "9.  Build TODO Android"
    Write-Host "10. Build TODO iOS"
    Write-Host "11. Build TODO (Android + iOS)"
    Write-Host "12. Clean build"
    Write-Host "0.  Salir"
    Write-Host ""
    
    $option = Read-Host "Opción"
    
    switch ($option) {
        "1" {
            Build-Android -Flavor "dev" -Target "lib/main_dev.dart" -BuildType "apk"
        }
        "2" {
            Build-Android -Flavor "staging" -Target "lib/main_staging.dart" -BuildType "apk"
        }
        "3" {
            Build-Android -Flavor "prod" -Target "lib/main_prod.dart" -BuildType "apk"
        }
        "4" {
            Build-Android -Flavor "prod" -Target "lib/main_prod.dart" -BuildType "aab"
        }
        "5" {
            Build-iOS -Flavor "dev" -Target "lib/main_dev.dart"
        }
        "6" {
            Build-iOS -Flavor "staging" -Target "lib/main_staging.dart"
        }
        "7" {
            Build-iOS -Flavor "prod" -Target "lib/main_prod.dart"
        }
        "8" {
            Build-iOS -Flavor "prod" -Target "lib/main_prod.dart" -BuildIPA $true
        }
        "9" {
            Write-Host "Building todos los flavors de Android..." -ForegroundColor Cyan
            Build-Android -Flavor "dev" -Target "lib/main_dev.dart" -BuildType "apk"
            Build-Android -Flavor "staging" -Target "lib/main_staging.dart" -BuildType "apk"
            Build-Android -Flavor "prod" -Target "lib/main_prod.dart" -BuildType "apk"
            Build-Android -Flavor "prod" -Target "lib/main_prod.dart" -BuildType "aab"
        }
        "10" {
            Write-Host "Building todos los flavors de iOS..." -ForegroundColor Cyan
            Build-iOS -Flavor "dev" -Target "lib/main_dev.dart"
            Build-iOS -Flavor "staging" -Target "lib/main_staging.dart"
            Build-iOS -Flavor "prod" -Target "lib/main_prod.dart"
        }
        "11" {
            Write-Host "Building todos los flavors..." -ForegroundColor Cyan
            Build-Android -Flavor "dev" -Target "lib/main_dev.dart" -BuildType "apk"
            Build-Android -Flavor "staging" -Target "lib/main_staging.dart" -BuildType "apk"
            Build-Android -Flavor "prod" -Target "lib/main_prod.dart" -BuildType "aab"
            Build-iOS -Flavor "dev" -Target "lib/main_dev.dart"
            Build-iOS -Flavor "staging" -Target "lib/main_staging.dart"
            Build-iOS -Flavor "prod" -Target "lib/main_prod.dart"
        }
        "12" {
            Clean-Build
        }
        "0" {
            Write-Host "✓ Saliendo..." -ForegroundColor Green
            exit 0
        }
        default {
            Write-Host "✗ Opción inválida" -ForegroundColor Red
            Show-Menu
        }
    }
}

# Verificar dependencias
Write-Host "Verificando dependencias..." -ForegroundColor Yellow
flutter pub get

# Ejecutar según parámetros o menú
if ($Action -eq "menu") {
    Show-Menu
}
elseif ($Action -eq "clean") {
    Clean-Build
}
elseif ($Action -eq "android") {
    if ($Flavor -eq "all") {
        Build-Android -Flavor "dev" -Target "lib/main_dev.dart" -BuildType "apk"
        Build-Android -Flavor "staging" -Target "lib/main_staging.dart" -BuildType "apk"
        Build-Android -Flavor "prod" -Target "lib/main_prod.dart" -BuildType "aab"
    }
    else {
        $target = "lib/main_$Flavor.dart"
        Build-Android -Flavor $Flavor -Target $target -BuildType "aab"
    }
}
elseif ($Action -eq "ios") {
    if ($Flavor -eq "all") {
        Build-iOS -Flavor "dev" -Target "lib/main_dev.dart"
        Build-iOS -Flavor "staging" -Target "lib/main_staging.dart"
        Build-iOS -Flavor "prod" -Target "lib/main_prod.dart" -BuildIPA $true
    }
    else {
        $target = "lib/main_$Flavor.dart"
        Build-iOS -Flavor $Flavor -Target $target
    }
}

Write-Host ""
Write-Host "✓ Build completado!" -ForegroundColor Green
Write-Host ""
Write-Host "Ubicación de builds:" -ForegroundColor Cyan
Write-Host "  Android APK:  build\app\outputs\apk\"
Write-Host "  Android AAB:  build\app\outputs\bundle\"
Write-Host "  iOS:          build\ios\iphoneos\"
Write-Host "  iOS IPA:      build\ios\ipa\"
