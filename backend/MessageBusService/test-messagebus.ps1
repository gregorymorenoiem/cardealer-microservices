# Test MessageBusService
# This script tests the MessageBusService API endpoints

Write-Host "=================================" -ForegroundColor Cyan
Write-Host "MessageBusService - API Testing" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5009"
$headers = @{
    "Content-Type" = "application/json"
}

# Test 1: Health Check
Write-Host "TEST 1 - Health Check..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get
    Write-Host "  ✓ Health Status: $($response.status)" -ForegroundColor Green
    Write-Host "  ✓ Service: $($response.service)" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Health check failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 2: Publish a single message
Write-Host "TEST 2 - Publish Single Message..." -ForegroundColor Yellow
try {
    $message = @{
        topic = "user.registered"
        payload = "{`"userId`": 123, `"email`": `"test@example.com`"}"
        priority = "Normal"
        headers = @{
            "correlationId" = [System.Guid]::NewGuid().ToString()
            "source" = "test-script"
        }
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$baseUrl/api/messages" -Method Post -Body $message -Headers $headers -ContentType "application/json"
    Write-Host "  ✓ Message published successfully" -ForegroundColor Green
    Write-Host "  ✓ Response: $response" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Publish failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "  Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}
Write-Host ""

# Test 3: Subscribe to a topic
Write-Host "TEST 3 - Subscribe to Topic..." -ForegroundColor Yellow
try {
    $subscription = @{
        topic = "user.registered"
        consumerName = "test-consumer-" + [System.Guid]::NewGuid().ToString().Substring(0, 8)
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$baseUrl/api/subscriptions" -Method Post -Body $subscription -Headers $headers -ContentType "application/json"
    Write-Host "  ✓ Subscription created successfully" -ForegroundColor Green
    Write-Host "  ✓ Subscription ID: $($response.id)" -ForegroundColor Green
    Write-Host "  ✓ Queue Name: $($response.queueName)" -ForegroundColor Green
    $subscriptionId = $response.id
} catch {
    Write-Host "  ✗ Subscribe failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "  Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}
Write-Host ""

# Test 4: Get subscriptions
Write-Host "TEST 4 - Get All Subscriptions..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/subscriptions" -Method Get
    Write-Host "  ✓ Retrieved $($response.Count) subscription(s)" -ForegroundColor Green
    foreach ($sub in $response) {
        Write-Host "    - Topic: $($sub.topic), Consumer: $($sub.consumerName), Active: $($sub.isActive)" -ForegroundColor Gray
    }
} catch {
    Write-Host "  ✗ Get subscriptions failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 5: Publish batch of messages
Write-Host "TEST 5 - Publish Batch Messages..." -ForegroundColor Yellow
try {
    $batch = @{
        topic = "order.created"
        payloads = @(
            "{`"orderId`": 1001, `"amount`": 150.50}",
            "{`"orderId`": 1002, `"amount`": 200.00}",
            "{`"orderId`": 1003, `"amount`": 99.99}"
        )
        priority = "High"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$baseUrl/api/messages/batch" -Method Post -Body $batch -Headers $headers -ContentType "application/json"
    Write-Host "  ✓ Batch published successfully" -ForegroundColor Green
    Write-Host "  ✓ Response: $response" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Batch publish failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 6: Get dead letter messages
Write-Host "TEST 6 - Get Dead Letter Messages..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/deadletters" -Method Get
    Write-Host "  ✓ Retrieved $($response.Count) dead letter message(s)" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Get dead letters failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 7: Check RabbitMQ Management UI
Write-Host "TEST 7 - RabbitMQ Management UI..." -ForegroundColor Yellow
try {
    $rabbitMqUrl = "http://localhost:15672"
    $response = Invoke-WebRequest -Uri $rabbitMqUrl -Method Get -TimeoutSec 5 -UseBasicParsing
    Write-Host "  ✓ RabbitMQ Management UI accessible at $rabbitMqUrl" -ForegroundColor Green
    Write-Host "    Login: guest / guest" -ForegroundColor Gray
} catch {
    Write-Host "  ✗ RabbitMQ Management UI not accessible" -ForegroundColor Red
}
Write-Host ""

Write-Host "=================================" -ForegroundColor Cyan
Write-Host "Testing Complete!" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Check RabbitMQ Management UI: http://localhost:15672" -ForegroundColor White
Write-Host "2. View Swagger API docs: http://localhost:5009/swagger" -ForegroundColor White
Write-Host "3. Monitor message queues and exchanges in RabbitMQ" -ForegroundColor White
