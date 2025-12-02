#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Stop ConfigurationService and its database
.DESCRIPTION
    This script stops ConfigurationService container and its database
#>

Write-Host "`n=== Stopping ConfigurationService ===" -ForegroundColor Yellow

# Navigate to backend directory
$backendDir = Split-Path -Parent $PSScriptRoot
Set-Location $backendDir

docker-compose stop configurationservice configurationservice-db

Write-Host "`n=== ConfigurationService Stopped ===" -ForegroundColor Green
