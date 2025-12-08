# CarDealer Mobile - Setup Script
# PowerShell script to setup the Flutter mobile project

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "CarDealer Mobile - Setup Script" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Check if Flutter is installed
Write-Host "Checking Flutter installation..." -ForegroundColor Yellow
$flutterInstalled = Get-Command flutter -ErrorAction SilentlyContinue

if (-not $flutterInstalled) {
    Write-Host "❌ Flutter is not installed!" -ForegroundColor Red
    Write-Host "Please install Flutter from: https://flutter.dev/docs/get-started/install" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Flutter is installed" -ForegroundColor Green

# Show Flutter version
Write-Host ""
Write-Host "Flutter version:" -ForegroundColor Yellow
flutter --version

# Navigate to mobile directory
Write-Host ""
Write-Host "Navigating to mobile directory..." -ForegroundColor Yellow
Set-Location -Path "mobile"

# Check if pubspec.yaml exists
if (-not (Test-Path "pubspec.yaml")) {
    Write-Host "❌ pubspec.yaml not found!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ pubspec.yaml found" -ForegroundColor Green

# Clean previous builds
Write-Host ""
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
flutter clean

# Get dependencies
Write-Host ""
Write-Host "Installing dependencies..." -ForegroundColor Yellow
flutter pub get

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to install dependencies!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Dependencies installed successfully" -ForegroundColor Green

# Generate code (dependency injection, models, etc.)
Write-Host ""
Write-Host "Generating code (DI, models, etc.)..." -ForegroundColor Yellow
flutter pub run build_runner build --delete-conflicting-outputs

if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  Code generation had some issues, but continuing..." -ForegroundColor Yellow
}

# Check for connected devices
Write-Host ""
Write-Host "Checking for connected devices..." -ForegroundColor Yellow
flutter devices

# Analyze code
Write-Host ""
Write-Host "Analyzing code..." -ForegroundColor Yellow
flutter analyze

Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "✅ Setup completed successfully!" -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Connect a device or start an emulator" -ForegroundColor White
Write-Host "2. Run: flutter run" -ForegroundColor White
Write-Host "3. Or run in VS Code: Press F5" -ForegroundColor White
Write-Host ""
Write-Host "Available commands:" -ForegroundColor Yellow
Write-Host "  flutter run              - Run the app in debug mode" -ForegroundColor White
Write-Host "  flutter run --release    - Run the app in release mode" -ForegroundColor White
Write-Host "  flutter test             - Run tests" -ForegroundColor White
Write-Host "  flutter build apk        - Build APK for Android" -ForegroundColor White
Write-Host "  flutter build ios        - Build for iOS" -ForegroundColor White
Write-Host ""
