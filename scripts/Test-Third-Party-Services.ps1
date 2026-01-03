#!/usr/bin/env pwsh
# ===========================================================================
# Test-Third-Party-Services.ps1 - Probar conectividad con servicios externos
# ===========================================================================
# Este script prueba la conectividad con todos los servicios de terceros
# configurados en Sprint 1.
# ===========================================================================

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "TESTING THIRD-PARTY SERVICES" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$secretsDir = "./secrets"
$passed = 0
$failed = 0

function Test-GoogleMapsAPI {
    param([string]$apiKey)
    
    Write-Host "🗺️  Testing Google Maps API..." -ForegroundColor Cyan
    
    try {
        $url = "https://maps.googleapis.com/maps/api/js/QuotaService.RecordEvent?1=&3=$apiKey&callback=_xdc_._abc123"
        $response = Invoke-WebRequest -Uri $url -TimeoutSec 10 -UseBasicParsing
        
        if ($response.StatusCode -eq 200 -and $response.Content -notmatch "API key not valid") {
            Write-Host "   ✅ Google Maps API - OK" -ForegroundColor Green
            return $true
        } else {
            Write-Host "   ❌ Google Maps API - INVALID KEY" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "   ❌ Google Maps API - FAILED: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Test-StripeAPI {
    param([string]$secretKey)
    
    Write-Host "💳 Testing Stripe API..." -ForegroundColor Cyan
    
    try {
        $headers = @{
            "Authorization" = "Bearer $secretKey"
        }
        $response = Invoke-RestMethod -Uri "https://api.stripe.com/v1/balance" -Headers $headers -TimeoutSec 10
        
        Write-Host "   ✅ Stripe API - OK" -ForegroundColor Green
        Write-Host "      Balance: $($response.available[0].amount / 100) $($response.available[0].currency.ToUpper())" -ForegroundColor Gray
        return $true
    } catch {
        Write-Host "   ❌ Stripe API - FAILED: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Test-SendGridAPI {
    param([string]$apiKey)
    
    Write-Host "📧 Testing SendGrid API..." -ForegroundColor Cyan
    
    try {
        $headers = @{
            "Authorization" = "Bearer $apiKey"
        }
        $response = Invoke-RestMethod -Uri "https://api.sendgrid.com/v3/scopes" -Headers $headers -TimeoutSec 10
        
        Write-Host "   ✅ SendGrid API - OK" -ForegroundColor Green
        Write-Host "      Scopes: $($response.scopes -join ', ')" -ForegroundColor Gray
        return $true
    } catch {
        Write-Host "   ❌ SendGrid API - FAILED: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Test-TwilioAPI {
    param([string]$accountSid, [string]$authToken)
    
    Write-Host "📱 Testing Twilio API..." -ForegroundColor Cyan
    
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes("${accountSid}:${authToken}")
        $base64 = [System.Convert]::ToBase64String($bytes)
        $headers = @{
            "Authorization" = "Basic $base64"
        }
        $response = Invoke-RestMethod -Uri "https://api.twilio.com/2010-04-01/Accounts/$accountSid.json" -Headers $headers -TimeoutSec 10
        
        Write-Host "   ✅ Twilio API - OK" -ForegroundColor Green
        Write-Host "      Account: $($response.friendly_name)" -ForegroundColor Gray
        return $true
    } catch {
        Write-Host "   ❌ Twilio API - FAILED: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Test-AWSS3 {
    param([string]$accessKey, [string]$secretKey, [string]$region, [string]$bucket)
    
    Write-Host "☁️  Testing AWS S3..." -ForegroundColor Cyan
    
    # Verificar si AWS CLI está instalado
    $awsCli = Get-Command aws -ErrorAction SilentlyContinue
    if (-not $awsCli) {
        Write-Host "   ⚠️  AWS CLI not installed - skipping test" -ForegroundColor Yellow
        Write-Host "      Install: https://aws.amazon.com/cli/" -ForegroundColor Gray
        return $null
    }
    
    try {
        $env:AWS_ACCESS_KEY_ID = $accessKey
        $env:AWS_SECRET_ACCESS_KEY = $secretKey
        $env:AWS_DEFAULT_REGION = $region
        
        $result = aws s3 ls "s3://$bucket" 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ AWS S3 - OK" -ForegroundColor Green
            Write-Host "      Bucket: s3://$bucket" -ForegroundColor Gray
            return $true
        } else {
            Write-Host "   ❌ AWS S3 - FAILED" -ForegroundColor Red
            Write-Host "      Error: $result" -ForegroundColor Gray
            return $false
        }
    } catch {
        Write-Host "   ❌ AWS S3 - FAILED: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Test-Firebase {
    param([string]$serviceAccountJson)
    
    Write-Host "🔥 Testing Firebase..." -ForegroundColor Cyan
    
    try {
        $json = $serviceAccountJson | ConvertFrom-Json
        
        if ($json.type -eq "service_account" -and $json.project_id) {
            Write-Host "   ✅ Firebase Service Account - OK" -ForegroundColor Green
            Write-Host "      Project: $($json.project_id)" -ForegroundColor Gray
            return $true
        } else {
            Write-Host "   ❌ Firebase - INVALID JSON" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "   ❌ Firebase - INVALID JSON: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# ============================================================================
# EJECUTAR TESTS
# ============================================================================

# Google Maps
if (Test-Path "$secretsDir/google_maps_api_key.txt") {
    $apiKey = Get-Content "$secretsDir/google_maps_api_key.txt" -Raw | ForEach-Object { $_.Trim() }
    if (Test-GoogleMapsAPI $apiKey) { $passed++ } else { $failed++ }
} else {
    Write-Host "⚠️  Google Maps API key not found - skipping" -ForegroundColor Yellow
}

Write-Host ""

# Stripe
if (Test-Path "$secretsDir/stripe_secret_key.txt") {
    $secretKey = Get-Content "$secretsDir/stripe_secret_key.txt" -Raw | ForEach-Object { $_.Trim() }
    if (Test-StripeAPI $secretKey) { $passed++ } else { $failed++ }
} else {
    Write-Host "⚠️  Stripe secret key not found - skipping" -ForegroundColor Yellow
}

Write-Host ""

# SendGrid
if (Test-Path "$secretsDir/sendgrid_api_key.txt") {
    $apiKey = Get-Content "$secretsDir/sendgrid_api_key.txt" -Raw | ForEach-Object { $_.Trim() }
    if (Test-SendGridAPI $apiKey) { $passed++ } else { $failed++ }
} elseif (Test-Path "$secretsDir/resend_api_key.txt") {
    Write-Host "📧 Resend API detected (no test available)" -ForegroundColor Cyan
    Write-Host "   ⚠️  Manual test required" -ForegroundColor Yellow
} else {
    Write-Host "⚠️  Email provider not configured - skipping" -ForegroundColor Yellow
}

Write-Host ""

# Twilio
if (Test-Path "$secretsDir/twilio_account_sid.txt" -and (Test-Path "$secretsDir/twilio_auth_token.txt")) {
    $accountSid = Get-Content "$secretsDir/twilio_account_sid.txt" -Raw | ForEach-Object { $_.Trim() }
    $authToken = Get-Content "$secretsDir/twilio_auth_token.txt" -Raw | ForEach-Object { $_.Trim() }
    if (Test-TwilioAPI $accountSid $authToken) { $passed++ } else { $failed++ }
} else {
    Write-Host "⚠️  Twilio credentials not found - skipping" -ForegroundColor Yellow
}

Write-Host ""

# AWS S3
if ((Test-Path "$secretsDir/aws_access_key_id.txt") -and 
    (Test-Path "$secretsDir/aws_secret_access_key.txt") -and
    (Test-Path "$secretsDir/aws_s3_bucket.txt")) {
    
    $accessKey = Get-Content "$secretsDir/aws_access_key_id.txt" -Raw | ForEach-Object { $_.Trim() }
    $secretKey = Get-Content "$secretsDir/aws_secret_access_key.txt" -Raw | ForEach-Object { $_.Trim() }
    $bucket = Get-Content "$secretsDir/aws_s3_bucket.txt" -Raw | ForEach-Object { $_.Trim() }
    $region = if (Test-Path "$secretsDir/aws_region.txt") { Get-Content "$secretsDir/aws_region.txt" -Raw | ForEach-Object { $_.Trim() } } else { "us-east-1" }
    
    $result = Test-AWSS3 $accessKey $secretKey $region $bucket
    if ($result -eq $true) { $passed++ } elseif ($result -eq $false) { $failed++ }
} else {
    Write-Host "⚠️  AWS credentials not found - skipping" -ForegroundColor Yellow
}

Write-Host ""

# Firebase
if (Test-Path "$secretsDir/firebase_service_account.json") {
    $json = Get-Content "$secretsDir/firebase_service_account.json" -Raw
    if (Test-Firebase $json) { $passed++ } else { $failed++ }
} else {
    Write-Host "⚠️  Firebase service account not found - skipping" -ForegroundColor Yellow
}

# ============================================================================
# RESUMEN
# ============================================================================

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "TEST RESULTS" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "✅ Passed: $passed" -ForegroundColor Green
Write-Host "❌ Failed: $failed" -ForegroundColor Red

if ($failed -eq 0) {
    Write-Host "`n🎉 All tests passed! Sprint 1 is complete." -ForegroundColor Green
} else {
    Write-Host "`n⚠️  Some tests failed. Check the errors above." -ForegroundColor Yellow
}

Write-Host ""
