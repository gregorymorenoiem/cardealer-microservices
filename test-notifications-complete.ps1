param(
    [string]$BaseUrl = "http://localhost:5084",
    [switch]$UseGateway,
    [string]$GatewayUrl = "http://localhost:8443"
)

if ($UseGateway) {
    $BaseUrl = $GatewayUrl
}

Write-Host "=== Notification Service Test ===" -ForegroundColor Cyan
Write-Host "Target URL: $BaseUrl" -ForegroundColor Yellow
Write-Host ""

# Verificar si el servicio está activo
Write-Host "Checking if service is reachable..." -ForegroundColor Yellow
try {
    $ping = Invoke-WebRequest -Uri "$BaseUrl/health" -Method GET -TimeoutSec 5
    Write-Host "Service is ONLINE" -ForegroundColor Green
}
catch {
    Write-Host "Service is OFFLINE or not reachable" -ForegroundColor Red
    Write-Host "Make sure the NotificationService is running on $BaseUrl" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Test Health Check
Write-Host "1. Testing Health Check..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "$BaseUrl/health" -Method GET
    Write-Host "✅ Health Check: SUCCESS" -ForegroundColor Green
    Write-Host "   Status: Healthy" -ForegroundColor White
}
catch {
    Write-Host "❌ Health Check: FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test Email
Write-Host "2. Testing Email Notification..." -ForegroundColor Yellow
$emailBody = @{
    to = "test@example.com"
    subject = "Test Email from PowerShell"
    body = "<h1>Test Email</h1><p>This is a test email sent from PowerShell script</p>"
    isHtml = $true
    metadata = @{
        source = "powershell-test-script"
        timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    }
} | ConvertTo-Json -Depth 3

try {
    $emailResponse = Invoke-RestMethod -Uri "$BaseUrl/api/notifications/email" -Method POST -Body $emailBody -ContentType "application/json"
    Write-Host "✅ Email Test: SUCCESS" -ForegroundColor Green
    Write-Host "   Notification ID: $($emailResponse.notificationId)" -ForegroundColor White
    Write-Host "   Status: $($emailResponse.status)" -ForegroundColor White
    Write-Host "   Message: $($emailResponse.message)" -ForegroundColor White
}
catch {
    Write-Host "❌ Email Test: FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test SMS
Write-Host "3. Testing SMS Notification..." -ForegroundColor Yellow
$smsBody = @{
    to = "+15005550001"  # Número de prueba de Twilio
    message = "Test SMS from NotificationService PowerShell script"
    metadata = @{
        test = $true
        script = "powershell"
    }
} | ConvertTo-Json -Depth 3

try {
    $smsResponse = Invoke-RestMethod -Uri "$BaseUrl/api/notifications/sms" -Method POST -Body $smsBody -ContentType "application/json"
    Write-Host "✅ SMS Test: SUCCESS" -ForegroundColor Green
    Write-Host "   Notification ID: $($smsResponse.notificationId)" -ForegroundColor White
    Write-Host "   Status: $($smsResponse.status)" -ForegroundColor White
    Write-Host "   Message: $($smsResponse.message)" -ForegroundColor White
}
catch {
    Write-Host "❌ SMS Test: FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test Push
Write-Host "4. Testing Push Notification..." -ForegroundColor Yellow
$pushBody = @{
    deviceToken = "test-device-token-12345"
    title = "Test Push from PowerShell"
    body = "This is a test push notification sent via PowerShell"
    data = @{
        action = "test"
        screen = "home"
        id = "12345"
    }
    metadata = @{
        testRun = (Get-Date).ToString("yyyyMMdd-HHmmss")
    }
} | ConvertTo-Json -Depth 3

try {
    $pushResponse = Invoke-RestMethod -Uri "$BaseUrl/api/notifications/push" -Method POST -Body $pushBody -ContentType "application/json"
    Write-Host "✅ Push Test: SUCCESS" -ForegroundColor Green
    Write-Host "   Notification ID: $($pushResponse.notificationId)" -ForegroundColor White
    Write-Host "   Status: $($pushResponse.status)" -ForegroundColor White
    Write-Host "   Message: $($pushResponse.message)" -ForegroundColor White
}
catch {
    Write-Host "❌ Push Test: FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Resumen
Write-Host "=== Test Summary ===" -ForegroundColor Cyan
Write-Host "All tests completed! Check the results above." -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Check the application logs for detailed processing information" -ForegroundColor White
Write-Host "2. Verify notifications were queued/processed correctly" -ForegroundColor White
Write-Host "3. Check database for notification records" -ForegroundColor White