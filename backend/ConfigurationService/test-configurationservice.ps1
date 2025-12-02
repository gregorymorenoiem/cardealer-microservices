#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Test ConfigurationService endpoints
.DESCRIPTION
    This script performs basic API tests on ConfigurationService endpoints
#>

$baseUrl = "http://localhost:5085"
$testResults = @()

Write-Host "`n=== Testing ConfigurationService ===" -ForegroundColor Green

# Test 1: Health Check
Write-Host "`nTEST 1 - Health Check" -ForegroundColor Cyan
try {
    $health = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get
    if ($health.status -eq "Healthy") {
        Write-Host "  PASS - Health: $($health.status)" -ForegroundColor Green
        $testResults += @{ Test = "Health Check"; Status = "PASS" }
    } else {
        Write-Host "  FAIL - Health: $($health.status)" -ForegroundColor Red
        $testResults += @{ Test = "Health Check"; Status = "FAIL" }
    }
} catch {
    Write-Host "  ERROR - $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{ Test = "Health Check"; Status = "ERROR" }
}

# Test 2: Create Configuration
Write-Host "`nTEST 2 - Create Configuration" -ForegroundColor Cyan
try {
    $configKey = "TestConfig_" + (Get-Random)
    $body = @{
        key = $configKey
        value = "TestValue123"
        environment = "Dev"
        description = "Test configuration created by test script"
        createdBy = "test-script"
    } | ConvertTo-Json

    $config = Invoke-RestMethod -Uri "$baseUrl/api/configurations" -Method Post -Body $body -ContentType "application/json"
    Write-Host "  PASS - Created configuration: $($config.key)" -ForegroundColor Green
    $testResults += @{ Test = "Create Configuration"; Status = "PASS"; ConfigId = $config.id }
} catch {
    Write-Host "  ERROR - $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{ Test = "Create Configuration"; Status = "ERROR" }
}

# Test 3: Get Configuration by Key
Write-Host "`nTEST 3 - Get Configuration by Key" -ForegroundColor Cyan
try {
    if ($testResults[1].Status -eq "PASS") {
        $retrievedConfig = Invoke-RestMethod -Uri "$baseUrl/api/configurations/$configKey`?environment=Dev" -Method Get
        if ($retrievedConfig.value -eq "TestValue123") {
            Write-Host "  PASS - Retrieved configuration: $($retrievedConfig.key) = $($retrievedConfig.value)" -ForegroundColor Green
            $testResults += @{ Test = "Get Configuration"; Status = "PASS" }
        } else {
            Write-Host "  FAIL - Value mismatch" -ForegroundColor Red
            $testResults += @{ Test = "Get Configuration"; Status = "FAIL" }
        }
    } else {
        Write-Host "  SKIP - Previous test failed" -ForegroundColor Yellow
        $testResults += @{ Test = "Get Configuration"; Status = "SKIP" }
    }
} catch {
    Write-Host "  ERROR - $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{ Test = "Get Configuration"; Status = "ERROR" }
}

# Test 4: Get All Configurations
Write-Host "`nTEST 4 - Get All Configurations" -ForegroundColor Cyan
try {
    $allConfigs = Invoke-RestMethod -Uri "$baseUrl/api/configurations?environment=Dev" -Method Get
    Write-Host "  PASS - Retrieved $($allConfigs.Count) configurations from Dev environment" -ForegroundColor Green
    $testResults += @{ Test = "Get All Configurations"; Status = "PASS"; Count = $allConfigs.Count }
} catch {
    Write-Host "  ERROR - $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{ Test = "Get All Configurations"; Status = "ERROR" }
}

# Test 5: Create Feature Flag
Write-Host "`nTEST 5 - Create Feature Flag" -ForegroundColor Cyan
try {
    $flagKey = "test_feature_" + (Get-Random)
    $body = @{
        name = "Test Feature Flag"
        key = $flagKey
        description = "Test feature flag for gradual rollout"
        isEnabled = $true
        environment = "Dev"
        rolloutPercentage = 50
        createdBy = "test-script"
    } | ConvertTo-Json

    $flag = Invoke-RestMethod -Uri "$baseUrl/api/featureflags" -Method Post -Body $body -ContentType "application/json"
    Write-Host "  PASS - Created feature flag: $($flag.key) (rollout: $($flag.rolloutPercentage)%)" -ForegroundColor Green
    $testResults += @{ Test = "Create Feature Flag"; Status = "PASS"; FlagId = $flag.id }
} catch {
    Write-Host "  ERROR - $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{ Test = "Create Feature Flag"; Status = "ERROR" }
}

# Test 6: Check Feature Flag Status
Write-Host "`nTEST 6 - Check Feature Flag Status" -ForegroundColor Cyan
try {
    if ($testResults[4].Status -eq "PASS") {
        $isEnabled = Invoke-RestMethod -Uri "$baseUrl/api/featureflags/$flagKey/enabled?environment=Dev&userId=test-user-123" -Method Get
        Write-Host "  PASS - Feature flag evaluation: $isEnabled" -ForegroundColor Green
        $testResults += @{ Test = "Check Feature Flag"; Status = "PASS"; IsEnabled = $isEnabled }
    } else {
        Write-Host "  SKIP - Previous test failed" -ForegroundColor Yellow
        $testResults += @{ Test = "Check Feature Flag"; Status = "SKIP" }
    }
} catch {
    Write-Host "  ERROR - $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{ Test = "Check Feature Flag"; Status = "ERROR" }
}

# Test 7: Swagger UI Accessibility
Write-Host "`nTEST 7 - Swagger UI Accessibility" -ForegroundColor Cyan
try {
    $swagger = Invoke-WebRequest -Uri "$baseUrl/swagger/index.html" -Method Get -UseBasicParsing
    if ($swagger.StatusCode -eq 200) {
        Write-Host "  PASS - Swagger UI accessible" -ForegroundColor Green
        $testResults += @{ Test = "Swagger UI"; Status = "PASS" }
    } else {
        Write-Host "  FAIL - Status: $($swagger.StatusCode)" -ForegroundColor Red
        $testResults += @{ Test = "Swagger UI"; Status = "FAIL" }
    }
} catch {
    Write-Host "  ERROR - $($_.Exception.Message)" -ForegroundColor Red
    $testResults += @{ Test = "Swagger UI"; Status = "ERROR" }
}

# Summary
Write-Host "`n=== Test Summary ===" -ForegroundColor Green
$passed = ($testResults | Where-Object { $_.Status -eq "PASS" }).Count
$total = $testResults.Count
Write-Host "Passed: $passed/$total tests" -ForegroundColor $(if ($passed -eq $total) { "Green" } else { "Yellow" })

foreach ($result in $testResults) {
    $color = switch ($result.Status) {
        "PASS" { "Green" }
        "FAIL" { "Red" }
        "ERROR" { "Red" }
        "SKIP" { "Yellow" }
    }
    Write-Host "  $($result.Status) - $($result.Test)" -ForegroundColor $color
}

Write-Host "`n=== Testing Complete ===" -ForegroundColor Green
