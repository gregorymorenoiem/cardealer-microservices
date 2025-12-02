# Update Services Configuration Script
# Updates all microservices appsettings.json to use OpenTelemetry Collector

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Update Services for OTel Collector" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$services = @(
    @{Name="AuditService"; Path="backend/AuditService/AuditService.Api"},
    @{Name="AuthService"; Path="backend/AuthService/AuthService.Api"},
    @{Name="ErrorService"; Path="backend/ErrorService/ErrorService.Api"},
    @{Name="Gateway"; Path="backend/Gateway/Gateway.Api"},
    @{Name="MediaService"; Path="backend/MediaService/MediaService.Api"},
    @{Name="NotificationService"; Path="backend/NotificationService/NotificationService.Api"},
    @{Name="RoleService"; Path="backend/RoleService/RoleService.Api"},
    @{Name="UserService"; Path="backend/UserService/UserService.Api"}
)

$otelConfig = @{
    "Exporter" = @{
        "Otlp" = @{
            "Endpoint" = "http://otel-collector:4318"
            "Protocol" = "HttpProtobuf"
        }
    }
    "OtlpEndpoint" = "http://otel-collector:4318"
    "Tracing" = @{
        "Enabled" = $true
        "SamplingRatio" = 0.1
        "ExportIntervalMilliseconds" = 5000
        "MaxQueueSize" = 2048
        "MaxExportBatchSize" = 512
    }
    "Metrics" = @{
        "Enabled" = $true
        "ExportIntervalMilliseconds" = 60000
        "MaxQueueSize" = 2048
        "MaxExportBatchSize" = 512
    }
}

$updatedCount = 0
$errorCount = 0

foreach ($service in $services) {
    Write-Host "Processing $($service.Name)..." -ForegroundColor Yellow
    
    $appSettingsPath = "c:\Users\gmoreno\source\repos\cardealer\$($service.Path)\appsettings.json"
    
    if (Test-Path $appSettingsPath) {
        try {
            # Read existing appsettings
            $appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
            
            # Add or update OpenTelemetry section
            $otelConfig.ServiceName = $service.Name
            $otelConfig.Resources = @{
                "service.name" = $service.Name
                "service.namespace" = "cardealer"
                "service.version" = "1.0.0"
                "deployment.environment" = "development"
            }
            
            # Update OpenTelemetry configuration
            if ($appSettings.PSObject.Properties.Name -contains "OpenTelemetry") {
                Write-Host "  Updating existing OpenTelemetry configuration..." -ForegroundColor Gray
            } else {
                Write-Host "  Adding new OpenTelemetry configuration..." -ForegroundColor Gray
            }
            
            $appSettings | Add-Member -MemberType NoteProperty -Name "OpenTelemetry" -Value $otelConfig -Force
            
            # Write back to file
            $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
            
            Write-Host "  ✓ $($service.Name) updated successfully" -ForegroundColor Green
            $updatedCount++
            
        } catch {
            Write-Host "  ✗ Error updating $($service.Name): $($_.Exception.Message)" -ForegroundColor Red
            $errorCount++
        }
    } else {
        Write-Host "  ⚠ appsettings.json not found for $($service.Name)" -ForegroundColor Yellow
        $errorCount++
    }
    
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Update Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Services updated: $updatedCount" -ForegroundColor Green
if ($errorCount -gt 0) {
    Write-Host "Errors encountered: $errorCount" -ForegroundColor Red
}
Write-Host ""
Write-Host "OpenTelemetry Configuration:" -ForegroundColor Yellow
Write-Host "  • Endpoint: http://otel-collector:4318 (HTTP)" -ForegroundColor White
Write-Host "  • Protocol: HttpProtobuf" -ForegroundColor White
Write-Host "  • Sampling: 10%" -ForegroundColor White
Write-Host "  • Export Interval: 5s (traces), 60s (metrics)" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Start the observability stack: .\start-observability.ps1" -ForegroundColor White
Write-Host "  2. Rebuild services: docker-compose build" -ForegroundColor White
Write-Host "  3. Restart services: docker-compose up -d" -ForegroundColor White
Write-Host "  4. Generate traffic and view traces in Jaeger" -ForegroundColor White
Write-Host ""
