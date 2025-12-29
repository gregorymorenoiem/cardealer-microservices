# CarDealer App Icon Generator (PowerShell)
# Generates app icons using Python or provides manual instructions

$ErrorActionPreference = "Stop"

Write-Host "🎨 CarDealer App Icon Generator" -ForegroundColor Cyan
Write-Host "=" -NoNewline -ForegroundColor Gray
Write-Host ("=" * 49) -ForegroundColor Gray
Write-Host ""

# Check if Python is available
$pythonAvailable = $false
try {
    $pythonVersion = python --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        $pythonAvailable = $true
        Write-Host "✅ Python found: $pythonVersion" -ForegroundColor Green
    }
}
catch {
    Write-Host "⚠️  Python not found in PATH" -ForegroundColor Yellow
}

# Check if Pillow is installed
$pillowAvailable = $false
if ($pythonAvailable) {
    try {
        $pillowCheck = python -c "import PIL; print(PIL.__version__)" 2>&1
        if ($LASTEXITCODE -eq 0) {
            $pillowAvailable = $true
            Write-Host "✅ Pillow found: version $pillowCheck" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "⚠️  Pillow not installed" -ForegroundColor Yellow
    }
}

Write-Host ""

# Option 1: Generate with Python script
if ($pythonAvailable -and $pillowAvailable) {
    Write-Host "🚀 Generating icons with Python..." -ForegroundColor Cyan
    python generate_icons.py
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✨ Icons generated successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📦 Installing flutter_launcher_icons..." -ForegroundColor Cyan
        flutter pub get
        
        Write-Host ""
        Write-Host "🎯 Generating platform-specific icons..." -ForegroundColor Cyan
        flutter pub run flutter_launcher_icons
        
        Write-Host ""
        Write-Host "=" -NoNewline -ForegroundColor Gray
        Write-Host ("=" * 49) -ForegroundColor Gray
        Write-Host "✅ App icon setup complete!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📱 Icons have been generated for:" -ForegroundColor Cyan
        Write-Host "   - iOS (all sizes)" -ForegroundColor White
        Write-Host "   - Android (all densities + adaptive)" -ForegroundColor White
        Write-Host ""
        Write-Host "🔍 Review generated icons in:" -ForegroundColor Cyan
        Write-Host "   - android/app/src/main/res/mipmap-*/" -ForegroundColor White
        Write-Host "   - ios/Runner/Assets.xcassets/AppIcon.appiconset/" -ForegroundColor White
        exit 0
    }
}

# Option 2: Manual instructions
Write-Host "📝 Manual Icon Creation Instructions" -ForegroundColor Cyan
Write-Host "=" -NoNewline -ForegroundColor Gray
Write-Host ("=" * 49) -ForegroundColor Gray
Write-Host ""

if (-not $pythonAvailable) {
    Write-Host "⚠️  Python is not installed or not in PATH" -ForegroundColor Yellow
    Write-Host "   Install Python from: https://www.python.org/downloads/" -ForegroundColor Gray
    Write-Host ""
}

if ($pythonAvailable -and -not $pillowAvailable) {
    Write-Host "⚠️  Pillow library is not installed" -ForegroundColor Yellow
    Write-Host "   Install with: pip install Pillow" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "Alternative options:" -ForegroundColor Cyan
Write-Host ""

Write-Host "1️⃣  Install Python + Pillow and run again:" -ForegroundColor Yellow
Write-Host "   pip install Pillow" -ForegroundColor White
Write-Host "   .\generate-app-icons.ps1" -ForegroundColor White
Write-Host ""

Write-Host "2️⃣  Use online icon generator:" -ForegroundColor Yellow
Write-Host "   https://icon.kitchen/" -ForegroundColor Blue
Write-Host "   https://www.appicon.co/" -ForegroundColor Blue
Write-Host ""
Write-Host "   Design specifications:" -ForegroundColor White
Write-Host "   - Size: 1024x1024 pixels" -ForegroundColor Gray
Write-Host "   - Background: #001F54 (Deep Blue)" -ForegroundColor Gray
Write-Host "   - Car icon in white/gold with orange accents" -ForegroundColor Gray
Write-Host "   - Save as: assets/icons/app_icon.png" -ForegroundColor Gray
Write-Host ""

Write-Host "3️⃣  Design in Figma/Adobe XD/Canva:" -ForegroundColor Yellow
Write-Host "   - Create 1024x1024 canvas" -ForegroundColor White
Write-Host "   - Use CarDealer brand colors:" -ForegroundColor White
Write-Host "     • Deep Blue: #001F54" -ForegroundColor Gray
Write-Host "     • Orange: #FF6B35" -ForegroundColor Gray
Write-Host "     • Gold: #FFD700" -ForegroundColor Gray
Write-Host "   - Export as PNG" -ForegroundColor White
Write-Host "   - Save to: assets/icons/app_icon.png" -ForegroundColor White
Write-Host ""

Write-Host "4️⃣  After creating the icon manually:" -ForegroundColor Yellow
Write-Host "   flutter pub get" -ForegroundColor White
Write-Host "   flutter pub run flutter_launcher_icons" -ForegroundColor White
Write-Host ""

Write-Host "=" -NoNewline -ForegroundColor Gray
Write-Host ("=" * 49) -ForegroundColor Gray
Write-Host ""
Write-Host "📖 See ICON_DESIGN.md for detailed specifications" -ForegroundColor Cyan
Write-Host ""
