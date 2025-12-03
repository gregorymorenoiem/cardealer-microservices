# Test Saga Orchestration - MessageBusService
# Este script prueba el servicio Saga completo

$baseUrl = "http://localhost:5000"
$sagaEndpoint = "$baseUrl/api/saga"

Write-Host "🚀 Testing MessageBusService - Saga Orchestration" -ForegroundColor Cyan
Write-Host ""

# Test 1: Start a new saga
Write-Host "📝 Test 1: Starting a new saga..." -ForegroundColor Yellow

$sagaRequest = @{
    name          = "TestOrderSaga"
    description   = "Test saga for order processing"
    type          = "Orchestration"
    correlationId = "test-order-$(Get-Random)"
    timeout       = "00:05:00"
    steps         = @(
        @{
            name                   = "ValidateInventory"
            serviceName            = "InventoryService"
            actionType             = "http.get.inventory"
            actionPayload          = '{"url":"http://localhost:5001/api/inventory/validate","body":""}'
            compensationActionType = "http.post.inventory"
            compensationPayload    = '{"url":"http://localhost:5001/api/inventory/release","body":""}'
            maxRetries             = 3
            timeout                = "00:01:00"
        },
        @{
            name                   = "ProcessPayment"
            serviceName            = "PaymentService"
            actionType             = "http.post.payment"
            actionPayload          = '{"url":"http://localhost:5002/api/payments/process","body":"{\"amount\":100.00}"}'
            compensationActionType = "http.post.payment"
            compensationPayload    = '{"url":"http://localhost:5002/api/payments/refund","body":""}'
            maxRetries             = 3
            timeout                = "00:02:00"
        },
        @{
            name                   = "CreateOrder"
            serviceName            = "OrderService"
            actionType             = "http.post.order"
            actionPayload          = '{"url":"http://localhost:5003/api/orders/create","body":"{\"orderId\":\"test-123\"}"}'
            compensationActionType = "http.delete.order"
            compensationPayload    = '{"url":"http://localhost:5003/api/orders/test-123"}'
            maxRetries             = 3
            timeout                = "00:01:00"
        }
    )
} | ConvertTo-Json -Depth 10

try {
    $response = Invoke-RestMethod -Uri "$sagaEndpoint/start" -Method Post -Body $sagaRequest -ContentType "application/json"
    Write-Host "✅ Saga started successfully!" -ForegroundColor Green
    Write-Host "Saga ID: $($response.sagaId)" -ForegroundColor White
    Write-Host "Status: $($response.status)" -ForegroundColor White
    Write-Host "Correlation ID: $($response.correlationId)" -ForegroundColor White
    Write-Host ""
    
    $sagaId = $response.sagaId
    
    # Test 2: Get saga status
    Write-Host "📝 Test 2: Getting saga status..." -ForegroundColor Yellow
    Start-Sleep -Seconds 2
    
    $sagaDetails = Invoke-RestMethod -Uri "$sagaEndpoint/$sagaId" -Method Get
    Write-Host "✅ Saga details retrieved!" -ForegroundColor Green
    Write-Host "Name: $($sagaDetails.name)" -ForegroundColor White
    Write-Host "Status: $($sagaDetails.status)" -ForegroundColor White
    Write-Host "Total Steps: $($sagaDetails.totalSteps)" -ForegroundColor White
    Write-Host "Current Step: $($sagaDetails.currentStepIndex)" -ForegroundColor White
    Write-Host ""
    
    Write-Host "Steps:" -ForegroundColor Cyan
    foreach ($step in $sagaDetails.steps) {
        Write-Host "  - $($step.name): $($step.status)" -ForegroundColor White
    }
    Write-Host ""
    
    # Test 3: Get sagas by status
    Write-Host "📝 Test 3: Getting sagas by status..." -ForegroundColor Yellow
    
    $sagas = Invoke-RestMethod -Uri "$sagaEndpoint/status/Running" -Method Get
    Write-Host "✅ Found $($sagas.Count) running sagas" -ForegroundColor Green
    Write-Host ""
    
    # Test 4: Abort saga (optional)
    # Write-Host "📝 Test 4: Aborting saga..." -ForegroundColor Yellow
    # $abortRequest = @{ reason = "Test abort" } | ConvertTo-Json
    # $aborted = Invoke-RestMethod -Uri "$sagaEndpoint/$sagaId/abort" -Method Post -Body $abortRequest -ContentType "application/json"
    # Write-Host "✅ Saga aborted: $($aborted.status)" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "🎉 All tests completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "⚠️  Make sure MessageBusService is running on $baseUrl" -ForegroundColor Yellow
    Write-Host "   Run: dotnet run --project MessageBusService.Api" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "📚 For more examples, see: SAGA_ORCHESTRATION_EXAMPLES.md" -ForegroundColor Cyan
