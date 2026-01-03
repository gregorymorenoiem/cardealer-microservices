#!/usr/bin/env pwsh
# ===========================================================================
# Setup-Secrets-Interactive.ps1 - Configuración interactiva de secrets
# ===========================================================================
# Este script te guía para crear todos los archivos de secrets necesarios.
# ===========================================================================

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SPRINT 1 - SETUP SECRETS INTERACTIVO" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$secretsDir = "./secrets"

# Crear directorio si no existe
if (-not (Test-Path $secretsDir)) {
    New-Item -ItemType Directory -Path $secretsDir -Force | Out-Null
    Write-Host "✅ Directorio secrets/ creado" -ForegroundColor Green
}

Write-Host "📝 Voy a pedirte tus API keys y las guardaré en archivos seguros." -ForegroundColor Yellow
Write-Host "   Puedes presionar ENTER para omitir opcionales.`n" -ForegroundColor Gray

function Save-Secret {
    param(
        [string]$FileName,
        [string]$Description,
        [string]$Example,
        [bool]$Required = $true,
        [string]$ValidationPattern = ""
    )
    
    $filePath = Join-Path $secretsDir $FileName
    $priority = if ($Required) { "🔴 CRÍTICO" } else { "🟡 OPCIONAL" }
    
    Write-Host "`n$priority - $Description" -ForegroundColor $(if ($Required) { "Red" } else { "Yellow" })
    Write-Host "   Ejemplo: $Example" -ForegroundColor Gray
    
    # Verificar si ya existe
    if (Test-Path $filePath) {
        $existing = Get-Content $filePath -Raw | ForEach-Object { $_.Trim() }
        if ($existing.Length -gt 0 -and $existing -notmatch "^(placeholder|example|TODO|changeme)") {
            Write-Host "   ✅ Ya existe: $($existing.Substring(0, [Math]::Min(30, $existing.Length)))..." -ForegroundColor Green
            
            $overwrite = Read-Host "   ¿Reemplazar? (s/N)"
            if ($overwrite -ne "s" -and $overwrite -ne "S") {
                return
            }
        }
    }
    
    # Pedir valor
    $value = Read-Host "   Ingresa el valor"
    
    # Permitir omitir opcionales
    if ([string]::IsNullOrWhiteSpace($value)) {
        if (-not $Required) {
            Write-Host "   ⚪ Omitido (opcional)" -ForegroundColor Gray
            return
        } else {
            Write-Host "   ❌ Este campo es obligatorio" -ForegroundColor Red
            $value = Read-Host "   Ingresa el valor"
        }
    }
    
    # Validar patrón si se especifica
    if ($ValidationPattern -and $value -notmatch $ValidationPattern) {
        Write-Host "   ⚠️  El formato no parece correcto (esperado: $Example)" -ForegroundColor Yellow
        $continue = Read-Host "   ¿Guardar de todas formas? (s/N)"
        if ($continue -ne "s" -and $continue -ne "S") {
            return
        }
    }
    
    # Guardar
    $value | Out-File -FilePath $filePath -Encoding utf8 -NoNewline
    Write-Host "   ✅ Guardado en $FileName" -ForegroundColor Green
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SECCIÓN 1: SECRETS LOCALES" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# JWT Secret
if (-not (Test-Path "$secretsDir/jwt_secret_key.txt")) {
    Write-Host "`n🔑 Generando JWT Secret automáticamente..." -ForegroundColor Cyan
    $jwtSecret = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 48 | ForEach-Object {[char]$_})
    $jwtSecret | Out-File -FilePath "$secretsDir/jwt_secret_key.txt" -Encoding utf8 -NoNewline
    Write-Host "   ✅ jwt_secret_key.txt generado (48 caracteres)" -ForegroundColor Green
} else {
    Write-Host "`n🔑 JWT Secret ya existe" -ForegroundColor Green
}

# DB Password
if (-not (Test-Path "$secretsDir/db_password.txt")) {
    $defaultDbPass = "CarDealer2026_SecureDB!"
    Write-Host "`n🔐 Password de PostgreSQL" -ForegroundColor Cyan
    Write-Host "   Sugerencia: $defaultDbPass" -ForegroundColor Gray
    $dbPass = Read-Host "   Ingresa password (ENTER para usar sugerencia)"
    if ([string]::IsNullOrWhiteSpace($dbPass)) {
        $dbPass = $defaultDbPass
    }
    $dbPass | Out-File -FilePath "$secretsDir/db_password.txt" -Encoding utf8 -NoNewline
    Write-Host "   ✅ db_password.txt guardado" -ForegroundColor Green
} else {
    Write-Host "`n🔐 DB Password ya existe" -ForegroundColor Green
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SECCIÓN 2: GOOGLE CLOUD PLATFORM" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Save-Secret -FileName "google_maps_api_key.txt" `
    -Description "Google Maps API Key" `
    -Example "AIzaSyABC123..." `
    -Required $true `
    -ValidationPattern "^AIzaSy"

Save-Secret -FileName "google_oauth_client_id.txt" `
    -Description "Google OAuth Client ID" `
    -Example "123456789.apps.googleusercontent.com" `
    -Required $true `
    -ValidationPattern "\.apps\.googleusercontent\.com$"

Save-Secret -FileName "google_oauth_client_secret.txt" `
    -Description "Google OAuth Client Secret" `
    -Example "GOCSPX-..." `
    -Required $true `
    -ValidationPattern "^GOCSPX-"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SECCIÓN 3: AWS S3" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Save-Secret -FileName "aws_access_key_id.txt" `
    -Description "AWS Access Key ID" `
    -Example "AKIAIOSFODNN7EXAMPLE" `
    -Required $true `
    -ValidationPattern "^AKIA"

Save-Secret -FileName "aws_secret_access_key.txt" `
    -Description "AWS Secret Access Key" `
    -Example "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY" `
    -Required $true

Save-Secret -FileName "aws_s3_bucket.txt" `
    -Description "AWS S3 Bucket Name" `
    -Example "cardealer-media-dev" `
    -Required $true `
    -ValidationPattern "^[a-z0-9\-]+$"

Save-Secret -FileName "aws_region.txt" `
    -Description "AWS Region" `
    -Example "us-east-1" `
    -Required $true `
    -ValidationPattern "^[a-z]{2}-[a-z]+-\d{1}$"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SECCIÓN 4: STRIPE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Save-Secret -FileName "stripe_secret_key.txt" `
    -Description "Stripe Secret Key" `
    -Example "sk_test_..." `
    -Required $true `
    -ValidationPattern "^sk_(test|live)_"

Save-Secret -FileName "stripe_publishable_key.txt" `
    -Description "Stripe Publishable Key" `
    -Example "pk_test_..." `
    -Required $true `
    -ValidationPattern "^pk_(test|live)_"

Save-Secret -FileName "stripe_webhook_secret.txt" `
    -Description "Stripe Webhook Secret" `
    -Example "whsec_..." `
    -Required $false `
    -ValidationPattern "^whsec_"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SECCIÓN 5: EMAIL (SendGrid o Resend)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n¿Qué proveedor de email usas?" -ForegroundColor Yellow
Write-Host "   1) SendGrid" -ForegroundColor White
Write-Host "   2) Resend" -ForegroundColor White
$emailChoice = Read-Host "   Elige (1 o 2)"

if ($emailChoice -eq "1") {
    Save-Secret -FileName "sendgrid_api_key.txt" `
        -Description "SendGrid API Key" `
        -Example "SG...." `
        -Required $true `
        -ValidationPattern "^SG\."
} elseif ($emailChoice -eq "2") {
    Save-Secret -FileName "resend_api_key.txt" `
        -Description "Resend API Key" `
        -Example "re_..." `
        -Required $true `
        -ValidationPattern "^re_"
} else {
    Write-Host "   ⚪ Email provider omitido" -ForegroundColor Gray
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SECCIÓN 6: FIREBASE (Push Notifications)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n🔥 Firebase Service Account JSON" -ForegroundColor Yellow
Write-Host "   Este es un archivo JSON que descargaste de Firebase." -ForegroundColor Gray
Write-Host "   Ejemplo: cardealer-prod-firebase-adminsdk-abc123.json" -ForegroundColor Gray

$firebasePath = Read-Host "`n   Ingresa la ruta completa del archivo JSON (o ENTER para omitir)"

if (-not [string]::IsNullOrWhiteSpace($firebasePath)) {
    if (Test-Path $firebasePath) {
        Copy-Item $firebasePath "$secretsDir/firebase_service_account.json" -Force
        Write-Host "   ✅ firebase_service_account.json copiado" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Archivo no encontrado: $firebasePath" -ForegroundColor Red
    }
} else {
    Write-Host "   ⚪ Firebase omitido (opcional)" -ForegroundColor Gray
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SECCIÓN 7: TWILIO (SMS) - OPCIONAL" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$useTwilio = Read-Host "`n¿Configurar Twilio? (s/N)"

if ($useTwilio -eq "s" -or $useTwilio -eq "S") {
    Save-Secret -FileName "twilio_account_sid.txt" `
        -Description "Twilio Account SID" `
        -Example "AC..." `
        -Required $false `
        -ValidationPattern "^AC"

    Save-Secret -FileName "twilio_auth_token.txt" `
        -Description "Twilio Auth Token" `
        -Example "abc123..." `
        -Required $false

    Save-Secret -FileName "twilio_phone_number.txt" `
        -Description "Twilio Phone Number" `
        -Example "+15551234567" `
        -Required $false `
        -ValidationPattern "^\+"
} else {
    Write-Host "   ⚪ Twilio omitido" -ForegroundColor Gray
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "RESUMEN" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$files = Get-ChildItem -Path $secretsDir -File | Where-Object { $_.Length -gt 0 }
Write-Host "✅ Archivos de secrets creados: $($files.Count)" -ForegroundColor Green

foreach ($file in $files) {
    $preview = (Get-Content $file.FullName -Raw).Trim()
    $preview = if ($preview.Length -gt 30) { $preview.Substring(0, 30) + "..." } else { $preview }
    Write-Host "   - $($file.Name): $preview" -ForegroundColor Gray
}

Write-Host "`n🎉 ¡Secrets configurados!" -ForegroundColor Green
Write-Host "`n📋 Próximos pasos:" -ForegroundColor Yellow
Write-Host "   1. Valida tu configuración:" -ForegroundColor White
Write-Host "      ./scripts/Validate-Secrets.ps1`n" -ForegroundColor Cyan
Write-Host "   2. Genera compose.secrets.yaml:" -ForegroundColor White
Write-Host "      ./scripts/Update-ComposeSecrets.ps1`n" -ForegroundColor Cyan
Write-Host "   3. Prueba conectividad:" -ForegroundColor White
Write-Host "      ./scripts/Test-Third-Party-Services.ps1`n" -ForegroundColor Cyan
