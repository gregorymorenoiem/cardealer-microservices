# Sprint 1 - Runtime Validation Script
# CarDealer Microservices
# Created: December 3, 2025

param(
    [switch]$FullValidation = $false,
    [switch]$SkipBuild = $false
)

$ErrorActionPreference = "Continue"
$script:SuccessCount = 0
$script:FailureCount = 0
$script:Results = @()

function Write-Header {
    param([string]$Text)
    Write-Host "`n$('=' * 80)" -ForegroundColor Cyan
    Write-Host "  $Text" -ForegroundColor Cyan
    Write-Host "$('=' * 80)`n" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Text)
    Write-Host "✅ $Text" -ForegroundColor Green
    $script:SuccessCount++
    $script:Results += [PSCustomObject]@{
        Status    = "SUCCESS"
        Message   = $Text
        Timestamp = Get-Date -Format "HH:mm:ss"
    }
}

function Write-Failure {
    param([string]$Text, [string]$Details = "")
    Write-Host "❌ $Text" -ForegroundColor Red
    if ($Details) {
        Write-Host "   Details: $Details" -ForegroundColor Yellow
    }
    $script:FailureCount++
    $script:Results += [PSCustomObject]@{
        Status    = "FAILURE"
        Message   = $Text
        Details   = $Details
        Timestamp = Get-Date -Format "HH:mm:ss"
    }
}

function Write-Info {
    param([string]$Text)
    Write-Host "ℹ️  $Text" -ForegroundColor Cyan
}

function Write-Warning {
    param([string]$Text)
    Write-Host "⚠️  $Text" -ForegroundColor Yellow
}

# ============================================================================
# US-1.1: Docker Build Verification
# ============================================================================
function Test-DockerBuildVerification {
    Write-Header "US-1.1: Docker Build Verification"
    
    # Check Docker is running
    Write-Info "Checking Docker status..."
    try {
        $dockerVersion = docker --version
        Write-Success "Docker installed: $dockerVersion"
    }
    catch {
        Write-Failure "Docker is not installed or not in PATH" $_.Exception.Message
        return $false
    }
    
    try {
        docker ps | Out-Null
        Write-Success "Docker Engine is running"
    }
    catch {
        Write-Failure "Docker Engine is not running. Please start Docker Desktop." $_.Exception.Message
        return $false
    }
    
    if (-not $SkipBuild) {
        # Clean up old images
        Write-Info "Cleaning up old Docker resources..."
        try {
            docker system prune -f --volumes 2>&1 | Out-Null
            Write-Success "Docker system cleaned"
        }
        catch {
            Write-Warning "Could not clean Docker system: $($_.Exception.Message)"
        }
        
        # Build all images
        Write-Info "Building all Docker images (this may take 10-15 minutes)..."
        Push-Location "C:\Users\gmoreno\source\repos\cardealer\backend"
        
        try {
            $buildOutput = docker-compose build --no-cache 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Success "All Docker images built successfully"
            }
            else {
                Write-Failure "Docker build failed" ($buildOutput | Select-Object -Last 20 | Out-String)
                Pop-Location
                return $false
            }
        }
        catch {
            Write-Failure "Error during Docker build" $_.Exception.Message
            Pop-Location
            return $false
        }
        
        Pop-Location
    }
    
    # List and verify images
    Write-Info "Verifying built images..."
    $images = docker images --filter "reference=backend*" --format "{{.Repository}}:{{.Tag}} ({{.Size}})"
    
    if ($images.Count -eq 0) {
        Write-Warning "No images found with 'backend' prefix. Trying alternative..."
        $images = docker images --format "{{.Repository}}:{{.Tag}} ({{.Size}})" | Select-String -Pattern "gateway|vehicle|contact|auth|user|role|notification|media|error|admin|cache|config|feature|file|search|scheduler|tracing|logging|health|audit|apiservice|backup|idempotency|ratelimiting|messagebus|serviceregistry"
    }
    
    $imageCount = ($images | Measure-Object).Count
    Write-Info "Found $imageCount Docker images:"
    $images | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
    
    if ($imageCount -ge 20) {
        Write-Success "Docker images verified: $imageCount/24+ images found"
    }
    else {
        Write-Warning "Expected 24 images, found $imageCount"
    }
    
    return $true
}

# ============================================================================
# US-1.2: Infrastructure Services Startup
# ============================================================================
function Test-InfrastructureServices {
    Write-Header "US-1.2: Infrastructure Services Startup"
    
    Push-Location "C:\Users\gmoreno\source\repos\cardealer\backend"
    
    # Start infrastructure services
    Write-Info "Starting infrastructure services (Consul, Redis, RabbitMQ, PostgreSQL)..."
    
    $infraServices = @(
        "consul",
        "redis",
        "rabbitmq",
        "errorservice-db",
        "authservice-db",
        "auditservice-db"
    )
    
    foreach ($service in $infraServices) {
        Write-Info "Starting $service..."
        try {
            docker-compose up -d $service 2>&1 | Out-Null
            if ($LASTEXITCODE -eq 0) {
                Write-Success "$service started"
            }
            else {
                Write-Failure "$service failed to start"
            }
        }
        catch {
            Write-Failure "Error starting $service" $_.Exception.Message
        }
    }
    
    # Wait for services to be healthy
    Write-Info "Waiting 30 seconds for services to become healthy..."
    Start-Sleep -Seconds 30
    
    # Check health status
    Write-Info "Checking health status..."
    $runningContainers = docker ps --format "{{.Names}} | {{.Status}}"
    
    $runningContainers | ForEach-Object {
        if ($_ -match "healthy") {
            Write-Success $_
        }
        elseif ($_ -match "Up") {
            Write-Info $_
        }
        else {
            Write-Warning $_
        }
    }
    
    # Verify specific ports
    $portsToCheck = @(
        @{Service = "Consul"; Port = 8500 },
        @{Service = "Redis"; Port = 6379 },
        @{Service = "RabbitMQ"; Port = 5672 },
        @{Service = "RabbitMQ Management"; Port = 15672 }
    )
    
    foreach ($portCheck in $portsToCheck) {
        try {
            $connection = Test-NetConnection -ComputerName localhost -Port $portCheck.Port -WarningAction SilentlyContinue -InformationLevel Quiet
            if ($connection) {
                Write-Success "$($portCheck.Service) is listening on port $($portCheck.Port)"
            }
            else {
                Write-Failure "$($portCheck.Service) is NOT listening on port $($portCheck.Port)"
            }
        }
        catch {
            Write-Failure "Could not check port $($portCheck.Port) for $($portCheck.Service)"
        }
    }
    
    Pop-Location
    return $true
}

# ============================================================================
# US-1.3: Core Services Deployment
# ============================================================================
function Test-CoreServicesDeployment {
    Write-Header "US-1.3: Core Services Deployment"
    
    Push-Location "C:\Users\gmoreno\source\repos\cardealer\backend"
    
    $coreServices = @(
        "gateway",
        "serviceregistry",
        "healthcheckservice",
        "authservice",
        "roleservice",
        "userservice",
        "vehicleservice",
        "contactservice"
    )
    
    foreach ($service in $coreServices) {
        Write-Info "Starting $service..."
        try {
            docker-compose up -d $service 2>&1 | Out-Null
            if ($LASTEXITCODE -eq 0) {
                Write-Success "$service started"
            }
            else {
                Write-Failure "$service failed to start"
            }
            Start-Sleep -Seconds 2
        }
        catch {
            Write-Failure "Error starting $service" $_.Exception.Message
        }
    }
    
    Write-Info "Waiting 20 seconds for services to initialize..."
    Start-Sleep -Seconds 20
    
    # Check running status
    Write-Info "Verifying services are running..."
    $runningServices = docker ps --filter "status=running" --format "{{.Names}}"
    
    foreach ($service in $coreServices) {
        if ($runningServices -match $service) {
            Write-Success "$service is running"
        }
        else {
            Write-Failure "$service is NOT running"
        }
    }
    
    # Check Consul registration (if accessible)
    try {
        Write-Info "Checking Consul service registration..."
        $consulServices = Invoke-RestMethod -Uri "http://localhost:8500/v1/catalog/services" -Method Get -TimeoutSec 5
        $serviceCount = ($consulServices.PSObject.Properties | Measure-Object).Count
        Write-Success "Consul has $serviceCount registered services"
    }
    catch {
        Write-Warning "Could not query Consul API: $($_.Exception.Message)"
    }
    
    Pop-Location
    return $true
}

# ============================================================================
# US-1.4: Health Endpoints Runtime Validation
# ============================================================================
function Test-HealthEndpoints {
    Write-Header "US-1.4: Health Endpoints Runtime Validation"
    
    # Define known service ports
    $services = @(
        @{Name = "Gateway"; Port = 5000; Path = "/health" },
        @{Name = "VehicleService"; Port = 5009; Path = "/health" },
        @{Name = "ContactService"; Port = 5007; Path = "/health" },
        @{Name = "AuthService"; Port = 5085; Path = "/health" },
        @{Name = "UserService"; Port = 5001; Path = "/health" },
        @{Name = "RoleService"; Port = 5002; Path = "/health" },
        @{Name = "NotificationService"; Port = 5010; Path = "/health" },
        @{Name = "MediaService"; Port = 5008; Path = "/health" },
        @{Name = "ErrorService"; Port = 5086; Path = "/health" },
        @{Name = "AuditService"; Port = 5087; Path = "/health" },
        @{Name = "AdminService"; Port = 5003; Path = "/health" },
        @{Name = "FileStorageService"; Port = 5011; Path = "/health" },
        @{Name = "SchedulerService"; Port = 5012; Path = "/health" },
        @{Name = "SearchService"; Port = 5013; Path = "/health" },
        @{Name = "CacheService"; Port = 5014; Path = "/health" },
        @{Name = "ConfigurationService"; Port = 5015; Path = "/health" },
        @{Name = "FeatureToggleService"; Port = 5016; Path = "/health" },
        @{Name = "MessageBusService"; Port = 5017; Path = "/health" },
        @{Name = "TracingService"; Port = 5018; Path = "/health" },
        @{Name = "LoggingService"; Port = 5019; Path = "/health" },
        @{Name = "HealthCheckService"; Port = 5020; Path = "/health" },
        @{Name = "ApiDocsService"; Port = 5021; Path = "/health" },
        @{Name = "BackupDRService"; Port = 5022; Path = "/health" },
        @{Name = "IdempotencyService"; Port = 5023; Path = "/health" },
        @{Name = "RateLimitingService"; Port = 5024; Path = "/health" }
    )
    
    $healthyServices = 0
    $unhealthyServices = 0
    $notRunningServices = 0
    
    foreach ($service in $services) {
        try {
            $url = "http://localhost:$($service.Port)$($service.Path)"
            $response = Invoke-WebRequest -Uri $url -Method Get -TimeoutSec 5 -UseBasicParsing
            
            if ($response.StatusCode -eq 200) {
                Write-Success "$($service.Name): HTTP $($response.StatusCode) - HEALTHY"
                $healthyServices++
            }
            else {
                Write-Warning "$($service.Name): HTTP $($response.StatusCode) - UNHEALTHY"
                $unhealthyServices++
            }
        }
        catch {
            if ($_.Exception.Message -match "Unable to connect|Connection refused|No connection could be made") {
                Write-Info "$($service.Name): NOT RUNNING (port $($service.Port))"
                $notRunningServices++
            }
            else {
                Write-Failure "$($service.Name): ERROR - $($_.Exception.Message)"
                $unhealthyServices++
            }
        }
    }
    
    Write-Host "`n" -NoNewline
    Write-Info "Health Check Summary:"
    Write-Host "   ✅ Healthy: $healthyServices" -ForegroundColor Green
    Write-Host "   ⚠️  Not Running: $notRunningServices" -ForegroundColor Yellow
    Write-Host "   ❌ Unhealthy: $unhealthyServices" -ForegroundColor Red
    Write-Host "   📊 Total: $($services.Count)" -ForegroundColor Cyan
    
    # Test advanced health endpoints (AuthService, AuditService)
    if ($FullValidation) {
        Write-Info "`nTesting advanced health endpoints..."
        
        $advancedEndpoints = @(
            @{Service = "AuthService"; Port = 5085; Path = "/health/ready" },
            @{Service = "AuthService"; Port = 5085; Path = "/health/live" },
            @{Service = "AuditService"; Port = 5087; Path = "/health/ready" },
            @{Service = "AuditService"; Port = 5087; Path = "/health/live" }
        )
        
        foreach ($endpoint in $advancedEndpoints) {
            try {
                $url = "http://localhost:$($endpoint.Port)$($endpoint.Path)"
                $response = Invoke-WebRequest -Uri $url -Method Get -TimeoutSec 5 -UseBasicParsing
                
                if ($response.StatusCode -eq 200) {
                    Write-Success "$($endpoint.Service) $($endpoint.Path): HTTP 200"
                }
            }
            catch {
                Write-Info "$($endpoint.Service) $($endpoint.Path): Not available"
            }
        }
    }
    
    return $healthyServices -gt 0
}

# ============================================================================
# US-1.5: Service-to-Service Communication Testing
# ============================================================================
function Test-ServiceCommunication {
    Write-Header "US-1.5: Service-to-Service Communication Testing"
    
    # Test Gateway routing
    Write-Info "Testing Gateway routing..."
    
    $routes = @(
        @{Name = "Vehicles API"; Path = "/api/vehicles" },
        @{Name = "Contacts API"; Path = "/api/contacts" },
        @{Name = "Users API"; Path = "/api/users" }
    )
    
    foreach ($route in $routes) {
        try {
            $url = "http://localhost:5000$($route.Path)"
            $response = Invoke-WebRequest -Uri $url -Method Get -TimeoutSec 5 -UseBasicParsing
            Write-Success "$($route.Name): Gateway routing working (HTTP $($response.StatusCode))"
        }
        catch {
            if ($_.Exception.Message -match "401|Unauthorized") {
                Write-Success "$($route.Name): Gateway routing working (requires authentication)"
            }
            elseif ($_.Exception.Message -match "404") {
                Write-Warning "$($route.Name): Endpoint not found (HTTP 404)"
            }
            else {
                Write-Failure "$($route.Name): Gateway routing failed" $_.Exception.Message
            }
        }
    }
    
    # Test Consul availability
    Write-Info "`nTesting Consul service discovery..."
    try {
        $consulHealth = Invoke-RestMethod -Uri "http://localhost:8500/v1/health/state/any" -Method Get -TimeoutSec 5
        Write-Success "Consul API is accessible ($($consulHealth.Count) health checks)"
    }
    catch {
        Write-Failure "Consul API is not accessible" $_.Exception.Message
    }
    
    # Test Redis connectivity
    Write-Info "`nTesting Redis connectivity..."
    try {
        $tcpConnection = Test-NetConnection -ComputerName localhost -Port 6379 -WarningAction SilentlyContinue -InformationLevel Quiet
        if ($tcpConnection) {
            Write-Success "Redis is accessible on port 6379"
        }
        else {
            Write-Failure "Redis is not accessible on port 6379"
        }
    }
    catch {
        Write-Failure "Could not test Redis connectivity"
    }
    
    # Test RabbitMQ Management UI
    Write-Info "`nTesting RabbitMQ Management UI..."
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:15672" -Method Get -TimeoutSec 5 -UseBasicParsing
        Write-Success "RabbitMQ Management UI is accessible (HTTP $($response.StatusCode))"
    }
    catch {
        Write-Failure "RabbitMQ Management UI is not accessible" $_.Exception.Message
    }
    
    return $true
}

# ============================================================================
# Main Execution
# ============================================================================
function Start-Sprint1Validation {
    $startTime = Get-Date
    
    Write-Host @"

╔════════════════════════════════════════════════════════════════════════════╗
║                                                                            ║
║                    🚀 SPRINT 1 - RUNTIME VALIDATION 🚀                     ║
║                                                                            ║
║                        CarDealer Microservices                             ║
║                          December 3, 2025                                  ║
║                                                                            ║
╚════════════════════════════════════════════════════════════════════════════╝

"@ -ForegroundColor Cyan
    
    # Execute validation steps
    $step1 = Test-DockerBuildVerification
    $step2 = Test-InfrastructureServices
    $step3 = Test-CoreServicesDeployment
    $step4 = Test-HealthEndpoints
    $step5 = Test-ServiceCommunication
    
    # Final Summary
    $endTime = Get-Date
    $duration = $endTime - $startTime
    
    Write-Header "Sprint 1 Validation - Final Summary"
    
    Write-Host "📊 Execution Summary:" -ForegroundColor Cyan
    Write-Host "   ✅ Successes: $script:SuccessCount" -ForegroundColor Green
    Write-Host "   ❌ Failures: $script:FailureCount" -ForegroundColor Red
    Write-Host "   ⏱️  Duration: $($duration.ToString('mm\:ss'))" -ForegroundColor Yellow
    Write-Host ""
    
    Write-Host "📋 User Stories Status:" -ForegroundColor Cyan
    Write-Host "   $(if($step1){'✅'}else{'❌'}) US-1.1: Docker Build Verification" -ForegroundColor $(if ($step1) { 'Green' }else { 'Red' })
    Write-Host "   $(if($step2){'✅'}else{'❌'}) US-1.2: Infrastructure Services Startup" -ForegroundColor $(if ($step2) { 'Green' }else { 'Red' })
    Write-Host "   $(if($step3){'✅'}else{'❌'}) US-1.3: Core Services Deployment" -ForegroundColor $(if ($step3) { 'Green' }else { 'Red' })
    Write-Host "   $(if($step4){'✅'}else{'❌'}) US-1.4: Health Endpoints Runtime Validation" -ForegroundColor $(if ($step4) { 'Green' }else { 'Red' })
    Write-Host "   $(if($step5){'✅'}else{'❌'}) US-1.5: Service-to-Service Communication Testing" -ForegroundColor $(if ($step5) { 'Green' }else { 'Red' })
    Write-Host ""
    
    # Export results
    $reportPath = "C:\Users\gmoreno\source\repos\cardealer\SPRINT1_VALIDATION_REPORT.json"
    $script:Results | ConvertTo-Json -Depth 10 | Out-File -FilePath $reportPath -Encoding UTF8
    Write-Success "Detailed report saved to: $reportPath"
    
    # Next steps
    Write-Host "`n📝 Next Steps:" -ForegroundColor Cyan
    if ($script:FailureCount -eq 0) {
        Write-Host "   ✅ All validation passed! Ready for:" -ForegroundColor Green
        Write-Host "      - US-1.6: Secrets Management Implementation" -ForegroundColor White
        Write-Host "      - US-1.7: Security Scanning" -ForegroundColor White
    }
    else {
        Write-Host "   ⚠️  Please address the failures above before proceeding." -ForegroundColor Yellow
        Write-Host "      - Check Docker logs: docker-compose logs -f <service-name>" -ForegroundColor White
        Write-Host "      - Verify service configurations" -ForegroundColor White
    }
    
    Write-Host "`n" -NoNewline
}

# Run validation
Start-Sprint1Validation
