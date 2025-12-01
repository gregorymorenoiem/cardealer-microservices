# E2E-TESTING-SCRIPT.ps1
param(
    [Parameter(Mandatory = $false)]
    [string]$BaseUrl = "http://localhost:5000"
)

$ErrorActionPreference = "Stop"

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  E2E TESTING - AuthService" -ForegroundColor Cyan
Write-Host "  Base URL: $BaseUrl" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Resultados
$TestResults = @{
    Total  = 0
    Passed = 0
    Failed = 0
}

function Test-Endpoint {
    param(
        [string]$Name,
        [scriptblock]$Test
    )
    
    $TestResults.Total++
    Write-Host "[TEST $($TestResults.Total)] $Name..." -NoNewline
    
    try {
        & $Test
        $TestResults.Passed++
        Write-Host " ✅ PASSED" -ForegroundColor Green
        return $true
    }
    catch {
        $TestResults.Failed++
        Write-Host " ❌ FAILED" -ForegroundColor Red
        Write-Host "  Error: $_" -ForegroundColor Red
        return $false
    }
}

# TEST 1: Health Check
Test-Endpoint "Health Check" {
    $response = Invoke-RestMethod -Uri "$BaseUrl/health" -Method GET -TimeoutSec 5
    if ($response.status -ne "healthy") {
        throw "Health check failed"
    }
}

# TEST 2: Register User
Test-Endpoint "Register User" {
    $body = @{
        userName = "e2euser"
        email    = "e2e@example.com"
        password = "Password123!"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod `
        -Uri "$BaseUrl/api/auth/register" `
        -Method POST `
        -Body $body `
        -ContentType "application/json" `
        -TimeoutSec 10
        
    if (-not $response.success) {
        throw "Registration failed"
    }
}

# TEST 3: Login User
Test-Endpoint "Login User" {
    $body = @{
        email    = "e2e@example.com"
        password = "Password123!"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod `
        -Uri "$BaseUrl/api/auth/login" `
        -Method POST `
        -Body $body `
        -ContentType "application/json" `
        -TimeoutSec 10
        
    if (-not $response.success) {
        throw "Login failed"
    }
    
    if ([string]::IsNullOrEmpty($response.data.token)) {
        throw "Token is missing"
    }
}

# RESUMEN
Write-Host ""
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  RESULTADOS E2E TESTING" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  Total Tests: $($TestResults.Total)" -ForegroundColor White
Write-Host "  Passed:      $($TestResults.Passed)" -ForegroundColor Green
Write-Host "  Failed:      $($TestResults.Failed)" -ForegroundColor Red
Write-Host ""

if ($TestResults.Failed -eq 0) {
    Write-Host "✅ AuthService E2E Testing: PASSED ✅" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "❌ AuthService E2E Testing: FAILED ❌" -ForegroundColor Red
    exit 1
}
