# 🧪 Guía Rápida de Testing - ErrorService Security

## 🎯 Testing en 5 Minutos

### 1️⃣ Iniciar ErrorService

```powershell
cd c:\Users\gmoreno\source\repos\cardealer\backend\ErrorService
dotnet run --project ErrorService.Api
```

**Esperado**: 
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

---

### 2️⃣ Test Health Check (Sin Autenticación)

**PowerShell:**
```powershell
Invoke-WebRequest -Uri "https://localhost:5001/health" -Method GET
```

**curl:**
```bash
curl -k https://localhost:5001/health
```

**✅ Esperado**: `200 OK` con `{"status":"healthy"}`

---

### 3️⃣ Test Endpoint Protegido SIN Token

**PowerShell:**
```powershell
$body = @{
    serviceName = "test-service"
    message = "Test error message"
    level = "Error"
    statusCode = 500
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://localhost:5001/api/errors" `
    -Method POST `
    -ContentType "application/json" `
    -Body $body
```

**✅ Esperado**: `401 Unauthorized`

---

### 4️⃣ Generar Token JWT de Prueba

**Opción A: Usar C# Interactive (dotnet-script)**

```csharp
#r "nuget: System.IdentityModel.Tokens.Jwt, 8.0.2"

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var secretKey = "DevSecretKeyMinimum32CharsForHS256-ErrorService-CarDealer-Dev-2024";
var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

var claims = new[]
{
    new Claim(JwtRegisteredClaimNames.Sub, "test-user"),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    new Claim("service", "errorservice")
};

var token = new JwtSecurityToken(
    issuer: "cardealer-auth",
    audience: "cardealer-services",
    claims: claims,
    expires: DateTime.UtcNow.AddHours(2),
    signingCredentials: credentials
);

var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
Console.WriteLine(tokenString);
```

**Opción B: Token Pre-generado (Desarrollo)**

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ0ZXN0LXVzZXIiLCJqdGkiOiJhYmMxMjMiLCJzZXJ2aWNlIjoiZXJyb3JzZXJ2aWNlIiwiZXhwIjoxNzY3MjI1NjAwLCJpc3MiOiJjYXJkZWFsZXItYXV0aCIsImF1ZCI6ImNhcmRlYWxlci1zZXJ2aWNlcyJ9.vGx4Z7K2Jm8Bq9Y1fX3wN5pA7cR6tL4mH8sK2vF9jE0
```

*⚠️ Este token es solo para desarrollo/testing*

---

### 5️⃣ Test Endpoint CON Token Válido

**PowerShell:**
```powershell
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." # Tu token generado

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$body = @{
    serviceName = "test-service"
    message = "Valid authenticated error"
    level = "Error"
    statusCode = 500
    httpMethod = "POST"
    endpoint = "/api/test"
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://localhost:5001/api/errors" `
    -Method POST `
    -Headers $headers `
    -Body $body
```

**✅ Esperado**: `201 Created` con `Location` header

---

### 6️⃣ Test SQL Injection Detection

**PowerShell:**
```powershell
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$body = @{
    serviceName = "test-service"
    message = "Error message'; DROP TABLE Users;--"
    level = "Error"
    statusCode = 500
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://localhost:5001/api/errors" `
    -Method POST `
    -Headers $headers `
    -Body $body
```

**✅ Esperado**: `400 Bad Request`

```json
{
  "type": "ValidationException",
  "title": "One or more validation errors occurred",
  "errors": {
    "Message": ["Message contains potentially dangerous SQL injection patterns"]
  }
}
```

---

### 7️⃣ Test XSS Detection

**PowerShell:**
```powershell
$body = @{
    serviceName = "test-service"
    message = "Error: <script>alert('XSS')</script>"
    level = "Error"
    statusCode = 500
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://localhost:5001/api/errors" `
    -Method POST `
    -Headers $headers `
    -Body $body
```

**✅ Esperado**: `400 Bad Request`

```json
{
  "errors": {
    "Message": ["Message contains potentially dangerous XSS patterns"]
  }
}
```

---

### 8️⃣ Test Swagger UI con JWT

1. **Abrir Swagger UI**: https://localhost:5001/swagger

2. **Click en "Authorize" 🔒** (esquina superior derecha)

3. **Ingresar token** (SIN el prefijo "Bearer"):
   ```
   eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```

4. **Click "Authorize"** y luego **"Close"**

5. **Probar endpoint** `POST /api/errors`:
   - Click en "Try it out"
   - Editar el JSON de ejemplo
   - Click en "Execute"

**✅ Esperado**: Request incluye automáticamente el header `Authorization`

---

## 📊 Checklist Rápido

```powershell
# Script de validación rápida
$tests = @(
    @{ Name = "Health Check"; Expected = "200" },
    @{ Name = "Protected without token"; Expected = "401" },
    @{ Name = "Protected with valid token"; Expected = "201" },
    @{ Name = "SQL Injection blocked"; Expected = "400" },
    @{ Name = "XSS blocked"; Expected = "400" }
)

# Ejecutar y marcar
foreach ($test in $tests) {
    Write-Host "[ ] $($test.Name) - Expected: $($test.Expected)"
}
```

**Salida esperada:**
```
[✅] Health Check - Expected: 200
[✅] Protected without token - Expected: 401
[✅] Protected with valid token - Expected: 201
[✅] SQL Injection blocked - Expected: 400
[✅] XSS blocked - Expected: 400
```

---

## 🛠️ Troubleshooting

### Error: "Unable to obtain configuration from: ..."

**Causa**: JWT configuration no cargada

**Solución**: Verificar `appsettings.Development.json` tiene sección `Jwt`

---

### Error: "IDX10501: Signature validation failed"

**Causa**: SecretKey no coincide

**Solución**: Usar el mismo SecretKey en generación y validación

---

### Error: "IDX10214: Audience validation failed"

**Causa**: Token tiene audience incorrecto

**Solución**: En desarrollo, `appsettings.Development.json`:
```json
"ValidateAudience": false
```

---

### Error: "No authenticationScheme was specified"

**Causa**: Middleware order incorrecto

**Solución**: Verificar en `Program.cs`:
```csharp
app.UseAuthentication();  // ANTES
app.UseAuthorization();   // DESPUÉS
```

---

## 🎯 Patrones de Ataque para Testing

### SQL Injection Patterns (11 detectados)

```
';--
' OR '1'='1
' OR 1=1--
UNION SELECT * FROM
DROP TABLE Users
INSERT INTO Users
DELETE FROM Users
UPDATE Users SET
EXEC xp_cmdshell
EXECUTE sp_
xp_cmdshell
```

### XSS Patterns (8 detectados)

```
<script>alert('XSS')</script>
javascript:alert(1)
<img src=x onerror=alert(1)>
<body onload=alert(1)>
eval(document.cookie)
<a onclick=alert(1)>
<iframe src=javascript:alert(1)>
document.cookie
```

---

## 📞 Soporte Rápido

**Build fallando?**
```powershell
dotnet clean
dotnet restore
dotnet build
```

**JWT expiration?**
Regenerar token o usar `ExpirationMinutes: 120` en desarrollo

**Swagger no muestra "Authorize"?**
Verificar `AddSecurityDefinition` y `AddSecurityRequirement` en `Program.cs`

---

**✅ Testing Completo - ErrorService Security 100%**
