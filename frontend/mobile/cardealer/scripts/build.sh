#!/bin/bash

# CarDealer Mobile - Build Script
# Genera builds para todos los flavors y plataformas

set -e

echo "🚀 CarDealer Mobile Build Script"
echo "================================="

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Función para mostrar mensajes
log_info() {
    echo -e "${GREEN}✓${NC} $1"
}

log_error() {
    echo -e "${RED}✗${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

# Verificar que Flutter está instalado
if ! command -v flutter &> /dev/null; then
    log_error "Flutter no está instalado"
    exit 1
fi

log_info "Flutter version:"
flutter --version

# Limpiar builds anteriores
clean_build() {
    log_info "Limpiando builds anteriores..."
    flutter clean
    rm -rf build/
}

# Build Android
build_android() {
    local flavor=$1
    local target=$2
    local build_type=$3
    
    log_info "Building Android $flavor ($build_type)..."
    
    if [ "$build_type" == "apk" ]; then
        flutter build apk --release --flavor $flavor -t $target
    else
        flutter build appbundle --release --flavor $flavor -t $target
    fi
    
    if [ $? -eq 0 ]; then
        log_info "Android $flavor $build_type build completado"
    else
        log_error "Error en Android $flavor build"
        return 1
    fi
}

# Build iOS
build_ios() {
    local flavor=$1
    local target=$2
    
    log_info "Building iOS $flavor..."
    flutter build ios --release --flavor $flavor -t $target --no-codesign
    
    if [ $? -eq 0 ]; then
        log_info "iOS $flavor build completado"
    else
        log_error "Error en iOS $flavor build"
        return 1
    fi
}

# Build iOS IPA
build_ios_ipa() {
    local flavor=$1
    local target=$2
    
    log_info "Building iOS IPA $flavor..."
    flutter build ipa --release --flavor $flavor -t $target
    
    if [ $? -eq 0 ]; then
        log_info "iOS IPA $flavor build completado"
    else
        log_error "Error en iOS IPA $flavor build"
        return 1
    fi
}

# Menú principal
show_menu() {
    echo ""
    echo "Selecciona una opción:"
    echo "1. Build Android DEV (APK)"
    echo "2. Build Android STAGING (APK)"
    echo "3. Build Android PROD (APK)"
    echo "4. Build Android PROD (AAB - Play Store)"
    echo "5. Build iOS DEV"
    echo "6. Build iOS STAGING"
    echo "7. Build iOS PROD"
    echo "8. Build iOS PROD (IPA - App Store)"
    echo "9. Build TODO Android"
    echo "10. Build TODO iOS"
    echo "11. Build TODO (Android + iOS)"
    echo "12. Clean build"
    echo "0. Salir"
    echo ""
    read -p "Opción: " option
    
    case $option in
        1)
            build_android "dev" "lib/main_dev.dart" "apk"
            ;;
        2)
            build_android "staging" "lib/main_staging.dart" "apk"
            ;;
        3)
            build_android "prod" "lib/main_prod.dart" "apk"
            ;;
        4)
            build_android "prod" "lib/main_prod.dart" "aab"
            ;;
        5)
            build_ios "dev" "lib/main_dev.dart"
            ;;
        6)
            build_ios "staging" "lib/main_staging.dart"
            ;;
        7)
            build_ios "prod" "lib/main_prod.dart"
            ;;
        8)
            build_ios_ipa "prod" "lib/main_prod.dart"
            ;;
        9)
            log_info "Building todos los flavors de Android..."
            build_android "dev" "lib/main_dev.dart" "apk"
            build_android "staging" "lib/main_staging.dart" "apk"
            build_android "prod" "lib/main_prod.dart" "apk"
            build_android "prod" "lib/main_prod.dart" "aab"
            ;;
        10)
            log_info "Building todos los flavors de iOS..."
            build_ios "dev" "lib/main_dev.dart"
            build_ios "staging" "lib/main_staging.dart"
            build_ios "prod" "lib/main_prod.dart"
            ;;
        11)
            log_info "Building todos los flavors de Android e iOS..."
            build_android "dev" "lib/main_dev.dart" "apk"
            build_android "staging" "lib/main_staging.dart" "apk"
            build_android "prod" "lib/main_prod.dart" "aab"
            build_ios "dev" "lib/main_dev.dart"
            build_ios "staging" "lib/main_staging.dart"
            build_ios "prod" "lib/main_prod.dart"
            ;;
        12)
            clean_build
            ;;
        0)
            log_info "Saliendo..."
            exit 0
            ;;
        *)
            log_error "Opción inválida"
            show_menu
            ;;
    esac
}

# Verificar dependencias
log_info "Verificando dependencias..."
flutter pub get

# Ejecutar menú
show_menu

log_info "Build completado!"
echo ""
echo "Ubicación de builds:"
echo "  Android APK:  build/app/outputs/apk/"
echo "  Android AAB:  build/app/outputs/bundle/"
echo "  iOS:          build/ios/iphoneos/"
echo "  iOS IPA:      build/ios/ipa/"
