# Script para validar URLs de imagenes de Unsplash
Write-Host "Validando URLs de imagenes..." -ForegroundColor Cyan

$urls = @(
    "https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1561580125-028ee3bd62eb?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1536700503339-1e4b06520771?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1617531653332-bd46c24f2068?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1617531653520-bd788419ce59?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1580273916550-e323be2ae537?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1619767886558-efdc259cde1a?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1584345604476-8ec5f5c4c728?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1589922944975-1095f7bef789?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1612825173281-9a193378527e?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1590362891991-f776e747a588?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1614162692292-7ac56d7f1f2e?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1617814076367-b759c7d7e738?w=800&h=600&fit=crop",
    "https://images.unsplash.com/photo-1622577575264-44c8a39b6258?w=800&h=600&fit=crop"
)

$vehicles = @(
    "Tesla Model 3 #1",
    "Tesla Model 3 #2",
    "Tesla Model 3 #3",
    "Tesla Model 3 #4",
    "BMW 3 Series #1",
    "BMW 3 Series #2",
    "BMW 3 Series #3",
    "Toyota Camry #1",
    "Toyota Camry #2",
    "Ford Mustang #1 (reportada)",
    "Ford Mustang #2",
    "Ford Mustang #3",
    "Honda Accord",
    "Audi A4 #1",
    "Audi A4 #2",
    "Mercedes C-Class",
    "Chevrolet Silverado",
    "Mazda CX-5",
    "Volkswagen Jetta"
)

$workingUrls = @()
$brokenUrls = @()

for ($i = 0; $i -lt $urls.Count; $i++) {
    $url = $urls[$i]
    $vehicle = $vehicles[$i]
    
    Write-Host "" 
    Write-Host "Validando $vehicle..." -ForegroundColor Yellow
    Write-Host "URL: $url" -ForegroundColor Gray
    
    try {
        $response = Invoke-WebRequest -Uri $url -Method Head -TimeoutSec 10 -ErrorAction Stop
        
        if ($response.StatusCode -eq 200) {
            Write-Host "OK (Status: $($response.StatusCode))" -ForegroundColor Green
            $workingUrls += [PSCustomObject]@{
                Vehicle = $vehicle
                URL     = $url
                Status  = $response.StatusCode
            }
        }
        else {
            Write-Host "FALLO (Status: $($response.StatusCode))" -ForegroundColor Red
            $brokenUrls += [PSCustomObject]@{
                Vehicle = $vehicle
                URL     = $url
                Status  = $response.StatusCode
            }
        }
    }
    catch {
        $statusCode = "N/A"
        if ($_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }
        
        Write-Host "ERROR (Status: $statusCode)" -ForegroundColor Red
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
        
        $brokenUrls += [PSCustomObject]@{
            Vehicle = $vehicle
            URL     = $url
            Status  = $statusCode
            Error   = $_.Exception.Message
        }
    }
    
    Start-Sleep -Milliseconds 500
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "RESUMEN DE VALIDACION" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

Write-Host ""
Write-Host "URLs FUNCIONANDO: $($workingUrls.Count)/$($urls.Count)" -ForegroundColor Green
if ($workingUrls.Count -gt 0) {
    $workingUrls | Format-Table -AutoSize
}

Write-Host ""
Write-Host "URLs ROTAS: $($brokenUrls.Count)/$($urls.Count)" -ForegroundColor Red
if ($brokenUrls.Count -gt 0) {
    $brokenUrls | Format-Table -AutoSize
    
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Red
    Write-Host "URLs QUE NECESITAN SER REEMPLAZADAS:" -ForegroundColor Red
    Write-Host "================================================" -ForegroundColor Red
    foreach ($broken in $brokenUrls) {
        Write-Host ""
        Write-Host "$($broken.Vehicle):" -ForegroundColor Yellow
        Write-Host "  URL rota: $($broken.URL)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Validacion completada." -ForegroundColor Cyan
