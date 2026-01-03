#!/usr/bin/env pwsh
# ===========================================================================
# Validate-Secrets.ps1 - Sprint 1 Secrets Validation Script
# ===========================================================================
# Este script verifica que todos los archivos de secrets existan y tengan
# contenido válido (no placeholders).
# ===========================================================================

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SPRINT 1 - SECRETS VALIDATION" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$secretsPath = "./secrets"
$passed = 0
$failed = 0
$warnings = 0

# Lista de secrets requeridos
$requiredSecrets = @(
    @{
        File = "jwt_secret_key.txt"
        MinLength = 32
        Pattern = "^[A-Za-z0-9+/=]+$"
        Critical = $true
        Description = "JWT Secret Key (mínimo 32 caracteres)"
    },
    @{
        File = "db_password.txt"
        MinLength = 8
        Pattern = "^.{8,}$"
        Critical = $true
        Description = "PostgreSQL Password"
    },
    @{
        File = "google_maps_api_key.txt"
        MinLength = 35
        Pattern = "^AIzaSy"
        Critical = $true
        Description = "Google Maps API Key"
    },
    @{
        File = "firebase_service_account.json"
        MinLength = 100
        Pattern = '"type":\s*"service_account"'
        Critical = $true
        Description = "Firebase Service Account JSON"
    },
    @{
        File = "stripe_secret_key.txt"
        MinLength = 20
        Pattern = "^sk_(test|live)_"
        Critical = $true
        Description = "Stripe Secret Key"
    },
    @{
        File = "stripe_webhook_secret.txt"
        MinLength = 20
        Pattern = "^whsec_"
        Critical = $false
        Description = "Stripe Webhook Secret (opcional para dev)"
    },
    @{
        File = "sendgrid_api_key.txt"
        MinLength = 20
        Pattern = "^SG\."
        Critical = $false
        Description = "SendGrid API Key"
    },
    @{
        File = "twilio_account_sid.txt"
        MinLength = 30
        Pattern = "^AC"
        Critical = $false
        Description = "Twilio Account SID (opcional)"
    },
    @{
        File = "twilio_auth_token.txt"
        MinLength = 30
        Pattern = "^[a-f0-9]{32}$"
        Critical = $false
        Description = "Twilio Auth Token (opcional)"
    },
    @{
        File = "aws_access_key_id.txt"
        MinLength = 16
        Pattern = "^AKIA"
        Critical = $true
        Description = "AWS Access Key ID"
    },
    @{
        File = "aws_secret_access_key.txt"
        MinLength = 30
        Pattern = "^[A-Za-z0-9+/]{40}$"
        Critical = $true
        Description = "AWS Secret Access Key"
    },
    @{
        File = "aws_s3_bucket.txt"
        MinLength = 3
        Pattern = "^[a-z0-9\-]+$"
        Critical = $true
        Description = "AWS S3 Bucket Name"
    },
    @{
        File = "aws_region.txt"
        MinLength = 9
        Pattern = "^[a-z]{2}-[a-z]+-[0-9]{1}$"
        Critical = $true
        Description = "AWS Region"
    }
)

Write-Host "Validando archivos de secrets..." -ForegroundColor Yellow
Write-Host ""

foreach ($secret in $requiredSecrets) {
    $filePath = Join-Path $secretsPath $secret.File
    $status = "UNKNOWN"
    $message = ""
    
    # Verificar si el archivo existe
    if (-not (Test-Path $filePath)) {
        if ($secret.Critical) {
            $failed++
            $status = "MISSING"
            $icon = "❌"
            $color = "Red"
            $message = "Archivo crítico no encontrado"
        }
        else {
            $warnings++
            $status = "MISSING"
            $icon = "⚠️ "
            $color = "Yellow"
            $message = "Archivo opcional no encontrado"
        }
    }
    else {
        # Leer contenido
        $content = Get-Content $filePath -Raw
        $content = $content.Trim()
        
        # Verificar placeholders
        if ($content -match "placeholder|CHANGE|TODO|XXX|example") {
            if ($secret.Critical) {
                $failed++
                $status = "PLACEHOLDER"
                $icon = "❌"
                $color = "Red"
                $message = "Contiene placeholder - reemplazar con valor real"
            }
            else {
                $warnings++
                $status = "PLACEHOLDER"
                $icon = "⚠️ "
                $color = "Yellow"
                $message = "Contiene placeholder (opcional)"
            }
        }
        # Verificar longitud mínima
        elseif ($content.Length -lt $secret.MinLength) {
            $failed++
            $status = "INVALID"
            $icon = "❌"
            $color = "Red"
            $message = "Muy corto (mín: $($secret.MinLength) chars, actual: $($content.Length))"
        }
        # Verificar patrón
        elseif (-not ($content -match $secret.Pattern)) {
            if ($secret.Critical) {
                $failed++
                $status = "INVALID"
                $icon = "❌"
                $color = "Red"
                $message = "Formato inválido"
            }
            else {
                $warnings++
                $status = "INVALID"
                $icon = "⚠️ "
                $color = "Yellow"
                $message = "Formato inválido (opcional)"
            }
        }
        else {
            $passed++
            $status = "OK"
            $icon = "✅"
            $color = "Green"
            $message = "Válido ($($content.Length) chars)"
        }
    }
    
    Write-Host "$icon $($secret.File) - $status" -ForegroundColor $color
    Write-Host "   $($secret.Description)" -ForegroundColor Gray
    if ($message) {
        Write-Host "   $message" -ForegroundColor $color
    }
    Write-Host ""
}

# Validaciones adicionales
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "VALIDACIONES ADICIONALES" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Verificar frontend .env
$frontendEnvPath = "./frontend/web/original/.env"
if (Test-Path $frontendEnvPath) {
    Write-Host "✅ Frontend .env existe" -ForegroundColor Green
    
    $envContent = Get-Content $frontendEnvPath -Raw
    
    # Verificar que no tenga placeholders en keys críticas
    $criticalKeys = @(
        "VITE_GOOGLE_MAPS_API_KEY",
        "VITE_FIREBASE_API_KEY",
        "VITE_STRIPE_PUBLIC_KEY"
    )
    
    foreach ($key in $criticalKeys) {
        if ($envContent -match "$key=placeholder") {
            Write-Host "⚠️  $key tiene placeholder" -ForegroundColor Yellow
            $warnings++
        }
        elseif ($envContent -match "$key=AIzaSy|$key=pk_test_|$key=sk_test_") {
            Write-Host "✅ $key configurado" -ForegroundColor Green
        }
    }
}
else {
    Write-Host "❌ Frontend .env NO existe" -ForegroundColor Red
    $failed++
}

Write-Host ""

# Verificar compose.secrets.yaml
$composeSecretsPath = "./compose.secrets.yaml"
if (Test-Path $composeSecretsPath) {
    Write-Host "✅ compose.secrets.yaml existe" -ForegroundColor Green
}
else {
    Write-Host "❌ compose.secrets.yaml NO existe" -ForegroundColor Red
    $failed++
}

Write-Host ""

# Resumen
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RESUMEN" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Secrets validados: $($requiredSecrets.Count)" -ForegroundColor White
Write-Host "Pasados: $passed" -ForegroundColor Green
Write-Host "Fallidos: $failed" -ForegroundColor Red
Write-Host "Advertencias: $warnings" -ForegroundColor Yellow
Write-Host ""

# Status final
if ($failed -eq 0) {
    Write-Host "🎉 ¡TODOS LOS SECRETS CRÍTICOS VÁLIDOS!" -ForegroundColor Green
    
    if ($warnings -gt 0) {
        Write-Host "⚠️  Hay $warnings secrets opcionales pendientes" -ForegroundColor Yellow
        Write-Host "   (puedes continuar, pero algunas features no funcionarán)" -ForegroundColor Yellow
    }
    else {
        Write-Host "✅ Todos los secrets están configurados correctamente" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "Próximos pasos:" -ForegroundColor Cyan
    Write-Host "1. Reiniciar servicios: docker-compose down && docker-compose up -d" -ForegroundColor White
    Write-Host "2. Esperar 60 segundos" -ForegroundColor White
    Write-Host "3. Probar conectividad: .\scripts\Test-Sprint0-Connectivity.ps1" -ForegroundColor White
    Write-Host ""
    
    exit 0
}
else {
    Write-Host "❌ HAY SECRETS CRÍTICOS FALTANTES O INVÁLIDOS" -ForegroundColor Red
    Write-Host ""
    Write-Host "Pasos para corregir:" -ForegroundColor Yellow
    Write-Host "1. Revisa los errores arriba marcados con ❌" -ForegroundColor White
    Write-Host "2. Sigue la guía: docs/sprints/frontend-backend-integration/SPRINT_1_SETUP_GUIDE.md" -ForegroundColor White
    Write-Host "3. Configura cada servicio según las instrucciones" -ForegroundColor White
    Write-Host "4. Vuelve a ejecutar este script para validar" -ForegroundColor White
    Write-Host ""
    
    exit 1
}
