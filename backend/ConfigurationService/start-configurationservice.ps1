#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Start ConfigurationService and its dependencies
.DESCRIPTION
    This script starts ConfigurationService database, Consul, and the ConfigurationService container
#>

Write-Host "`n=== Starting ConfigurationService Stack ===" -ForegroundColor Green

# Check if Docker is running
try {
    docker info | Out-Null
} catch {
    Write-Host "ERROR: Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

# Navigate to backend directory
$backendDir = Split-Path -Parent $PSScriptRoot
Set-Location $backendDir

Write-Host "`n1. Starting dependencies (consul, configurationservice-db)..." -ForegroundColor Yellow
docker-compose up -d consul configurationservice-db

Write-Host "`n2. Waiting for services to be healthy (20s)..." -ForegroundColor Yellow
Start-Sleep -Seconds 20

Write-Host "`n3. Building and starting ConfigurationService..." -ForegroundColor Yellow
docker-compose up -d --build configurationservice

Write-Host "`n4. Waiting for ConfigurationService to initialize (15s)..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

Write-Host "`n=== Service Status ===" -ForegroundColor Green
docker-compose ps configurationservice configurationservice-db consul

Write-Host "`n=== ConfigurationService Logs ===" -ForegroundColor Green
docker logs configurationservice --tail 20

Write-Host "`n=== Access Points ===" -ForegroundColor Cyan
Write-Host "  ConfigurationService API: http://localhost:5085" -ForegroundColor White
Write-Host "  Swagger UI:               http://localhost:5085/swagger" -ForegroundColor White
Write-Host "  Health Endpoint:          http://localhost:5085/health" -ForegroundColor White
Write-Host "  Consul UI:                http://localhost:8500" -ForegroundColor White
Write-Host "  Database:                 localhost:5434 (configurationservice/postgres/password)" -ForegroundColor White

Write-Host "`n=== ConfigurationService Started Successfully ===" -ForegroundColor Green
