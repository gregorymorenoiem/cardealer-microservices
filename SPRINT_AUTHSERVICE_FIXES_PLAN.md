# 🔧 Plan de Sprints: Corrección de Issues en AuthService

**Proyecto:** CarDealer Microservices  
**Fecha Inicio:** 1 Enero 2026  
**Sprint Base:** Post-1.1  
**Versión:** 1.0  

---

## 📋 RESUMEN EJECUTIVO

Plan para resolver los 5 issues críticos y medios identificados durante Sprint 1.1 (Auditoría AuthService). Los issues impactan funcionalidad core de autenticación y deben resolverse antes de producción.

### Issues a Resolver

| # | Issue | Severidad | Impacto | Prioridad | Sprint |
|---|-------|:---------:|---------|:---------:|:------:|
| 1 | RefreshToken retorna 401 | 🔴 Crítico | Alto - Renovación de sesión | P0 | 1.1.1 |
| 2 | Logout retorna 400 | 🟡 Medio | Medio - Cierre de sesión | P1 | 1.1.2 |
| 3 | TwoFactor/enable retorna 400 | 🟡 Medio | Bajo - Feature opcional | P2 | 1.1.3 |
| 4 | TwoFactor/generate-recovery-codes retorna 400 | 🟡 Medio | Bajo - Depende de #3 | P2 | 1.1.3 |
| 5 | ExternalAuth/linked-accounts retorna 400 | 🟡 Medio | Bajo - Feature opcional | P3 | 1.1.4 |

**Progreso General:** 0/5 issues resueltos (0%)  
**Tokens Totales Estimados:** ~35,000 tokens  
**Duración Estimada:** 4-5 sprints (4-6 horas)

---

## 🎯 OBJETIVOS POR SPRINT

### Fase 1: Correcciones Críticas (P0)
- ✅ Sprint 1.1.1: Resolver RefreshToken 401 (crítico para producción)

### Fase 2: Correcciones Importantes (P1)
- ✅ Sprint 1.1.2: Resolver Logout 400 (afecta UX)

### Fase 3: Features Opcionales (P2)
- ✅ Sprint 1.1.3: Resolver 2FA endpoints (enable + recovery codes)

### Fase 4: Integraciones Externas (P3)
- ✅ Sprint 1.1.4: Resolver ExternalAuth (requiere OAuth config)

---

## 🔴 SPRINT 1.1.1: FIX RefreshToken 401 (CRÍTICO)

**Prioridad:** P0 (Crítico)  
**Tokens estimados:** ~12,000  
**Duración estimada:** 1-2 sesiones (1-2 horas)  
**Bloqueante:** Sí - Impide renovación de sesiones en producción

### Contexto del Problema

**Endpoint:** `POST /api/Auth/refresh-token`  
**Status Code:** 401 Unauthorized  
**Request probado:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "cd606df54d2b4f25b7a2a8d4b6714588..."
}
```

**Posibles causas:**
1. RefreshToken no guardado en tabla `RefreshTokens` al hacer login
2. RefreshToken expiró (configuración de TTL)
3. RefreshToken invalidado prematuramente (one-time-use policy)
4. SecurityStamp cambió y tokens ya no son válidos
5. Validación incorrecta en `RefreshTokenCommandHandler`

### Tareas del Sprint

| ID | Tarea | Tokens | Descripción |
|----|-------|--------|-------------|
| 1.1.1.1 | Investigar tabla RefreshTokens | ~2,000 | Verificar que login guarda refreshToken en BD |
| 1.1.1.2 | Revisar RefreshTokenCommandHandler | ~3,000 | Analizar lógica de validación y renovación |
| 1.1.1.3 | Verificar TTL y expiración | ~1,500 | Revisar configuración de tiempo de vida |
| 1.1.1.4 | Test flujo completo | ~2,500 | Login → Esperar 30s → Refresh → Validar |
| 1.1.1.5 | Corregir código si necesario | ~3,000 | Aplicar fixes en Handler o Repository |
| 1.1.1.6 | Validar con múltiples refreshes | ~1,000 | Probar 3 refreshes consecutivos |

### Investigación Inicial

```powershell
# 1. Verificar estructura tabla RefreshTokens
docker exec authservice-db psql -U postgres -d authservice -c "\d \"RefreshTokens\""

# 2. Verificar si login guarda refreshToken
docker exec authservice-db psql -U postgres -d authservice -c "SELECT COUNT(*) FROM \"RefreshTokens\";"

# 3. Verificar último refreshToken del usuario test
docker exec authservice-db psql -U postgres -d authservice -c "SELECT \"Token\", \"ExpiresAt\", \"IsUsed\", \"IsRevoked\" FROM \"RefreshTokens\" WHERE \"UserId\" = '4a09dd28-a85a-4299-865c-d1df223ac2e4' ORDER BY \"CreatedAt\" DESC LIMIT 1;"
```

### Archivos a Revisar

```
AuthService.Application/
├── Features/Auth/Commands/
│   ├── LoginCommand.cs
│   ├── LoginCommandHandler.cs
│   ├── RefreshTokenCommand.cs
│   └── RefreshTokenCommandHandler.cs  ← CRÍTICO
AuthService.Domain/
├── Entities/
│   └── RefreshToken.cs
AuthService.Infrastructure/
├── Persistence/
│   └── Repositories/
│       └── RefreshTokenRepository.cs
```

### Criterios de Aceptación

- ✅ RefreshToken se guarda correctamente en BD al hacer login
- ✅ RefreshToken se valida correctamente (no expirado, no usado, no revocado)
- ✅ Endpoint `/refresh-token` retorna 200 OK con nuevo accessToken
- ✅ Nuevo refreshToken se genera y el anterior se marca como usado
- ✅ Test: Login → Refresh → Refresh → Refresh (todos exitosos)

### Expected Output

```json
POST /api/Auth/refresh-token
Status: 200 OK
{
  "success": true,
  "data": {
    "userId": "4a09dd28-a85a-4299-865c-d1df223ac2e4",
    "email": "test@example.com",
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",  // NUEVO
    "refreshToken": "f7c8e9d0a1b2c3d4e5f6g7h8i9j0k1l2...",       // NUEVO
    "expiresAt": "2026-01-01T08:00:00Z",
    "requiresTwoFactor": false
  }
}
```

---

## 🟡 SPRINT 1.1.2: FIX Logout 400 (IMPORTANTE)

**Prioridad:** P1 (Importante)  
**Tokens estimados:** ~8,000  
**Duración estimada:** 1 sesión (45 minutos)  
**Bloqueante:** No - Afecta UX pero no es crítico

### Contexto del Problema

**Endpoint:** `POST /api/Auth/logout`  
**Status Code:** 400 Bad Request  
**Requests probados:**
```http
# Intento 1: Sin Content-Type
POST /api/Auth/logout
Authorization: Bearer {token}
Result: 415 Unsupported Media Type

# Intento 2: Con Content-Type
POST /api/Auth/logout
Authorization: Bearer {token}
Content-Type: application/json
Result: 400 Bad Request
```

**Posibles causas:**
1. Endpoint requiere body con refreshToken para invalidar
2. Validación de modelo incorrecta (requiere DTO vacío)
3. Atributo `[FromBody]` mal configurado
4. Middleware de validación rechaza request vacío

### Tareas del Sprint

| ID | Tarea | Tokens | Descripción |
|----|-------|--------|-------------|
| 1.1.2.1 | Revisar LogoutController | ~2,000 | Verificar signature del endpoint |
| 1.1.2.2 | Revisar LogoutCommand | ~1,500 | Verificar DTO y validaciones |
| 1.1.2.3 | Test con diferentes payloads | ~2,000 | Probar: {}, null, {refreshToken} |
| 1.1.2.4 | Corregir código si necesario | ~2,000 | Hacer refreshToken opcional o remover validación |
| 1.1.2.5 | Validar flujo completo | ~500 | Login → Logout → Validar token inválido |

### Archivos a Revisar

```
AuthService.Api/
├── Controllers/
│   └── AuthController.cs           ← Revisar [HttpPost("logout")]
AuthService.Application/
├── Features/Auth/Commands/
│   ├── LogoutCommand.cs             ← Verificar DTO
│   └── LogoutCommandHandler.cs
```

### Tests a Ejecutar

```powershell
# Test 1: Body vacío
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}
Invoke-WebRequest "http://localhost:15085/api/Auth/logout" `
    -Method POST -Headers $headers -Body "{}" -UseBasicParsing

# Test 2: Con refreshToken
$body = @{ refreshToken = $refreshToken } | ConvertTo-Json
Invoke-WebRequest "http://localhost:15085/api/Auth/logout" `
    -Method POST -Headers $headers -Body $body -UseBasicParsing

# Test 3: Sin body (solo headers)
Invoke-WebRequest "http://localhost:15085/api/Auth/logout" `
    -Method POST -Headers $headers -UseBasicParsing
```

### Posibles Soluciones

**Opción A:** Hacer refreshToken opcional
```csharp
public record LogoutCommand(string? RefreshToken) : IRequest<Result>;
```

**Opción B:** Eliminar parámetro body
```csharp
[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    // Obtener userId del token JWT (Claims)
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var command = new LogoutCommand(userId);
    // ...
}
```

### Criterios de Aceptación

- ✅ Endpoint `/logout` retorna 200 OK (o 204 No Content)
- ✅ RefreshToken del usuario se invalida (IsRevoked = true)
- ✅ AccessToken sigue siendo válido hasta expiración (stateless JWT)
- ✅ Swagger documentation actualizada con ejemplo correcto

---

## 🟡 SPRINT 1.1.3: FIX TwoFactor Endpoints (OPCIONAL)

**Prioridad:** P2 (Feature opcional)  
**Tokens estimados:** ~10,000  
**Duración estimada:** 1-2 sesiones (1 hora)  
**Bloqueante:** No - Feature de seguridad avanzada

### Contexto del Problema

**Endpoints:**
1. `POST /api/TwoFactor/enable` → 400 Bad Request
2. `POST /api/TwoFactor/generate-recovery-codes` → 400 Bad Request

**Requests probados:**
```http
POST /api/TwoFactor/enable
Authorization: Bearer {token}
Content-Type: application/json
Body: (vacío)
Result: 400 Bad Request
```

**Posibles causas:**
1. Endpoint requiere body con configuración TOTP (secret key)
2. Usuario ya tiene 2FA habilitado (validación duplicada)
3. Falta validación de email/phone confirmado
4. Library de TOTP (Otp.NET) requiere configuración adicional

### Tareas del Sprint

| ID | Tarea | Tokens | Descripción |
|----|-------|--------|-------------|
| 1.1.3.1 | Revisar TwoFactorController | ~2,000 | Verificar DTOs requeridos |
| 1.1.3.2 | Revisar EnableTwoFactorCommand | ~2,000 | Verificar validaciones y lógica |
| 1.1.3.3 | Test flujo completo | ~3,000 | Enable → Verify → Disable |
| 1.1.3.4 | Corregir validaciones | ~2,000 | Ajustar requisitos si necesario |
| 1.1.3.5 | Documentar en Swagger | ~1,000 | Agregar ejemplos de request/response |

### Archivos a Revisar

```
AuthService.Api/
├── Controllers/
│   └── TwoFactorController.cs
AuthService.Application/
├── Features/TwoFactor/Commands/
│   ├── EnableTwoFactorCommand.cs
│   ├── EnableTwoFactorCommandHandler.cs
│   ├── GenerateRecoveryCodesCommand.cs
│   └── GenerateRecoveryCodesCommandHandler.cs
AuthService.Domain/
├── Entities/
│   └── UserTwoFactorToken.cs
```

### Flujo Esperado

```
1. POST /TwoFactor/enable
   → Genera secretKey (TOTP)
   → Genera QR code (otpauth://totp/...)
   → Retorna secretKey + qrCodeUrl + recoveryCodes (opcional)

2. POST /TwoFactor/verify
   → Body: { code: "123456" }
   → Valida código TOTP
   → Marca TwoFactorEnabled = true

3. POST /TwoFactor/generate-recovery-codes
   → Genera 10 códigos únicos
   → Retorna lista de códigos
   → Usuario debe guardarlos de forma segura
```

### Criterios de Aceptación

- ✅ `/TwoFactor/enable` retorna 200 OK con secretKey y QR code
- ✅ QR code es válido (puede escanearse con Google Authenticator)
- ✅ `/TwoFactor/verify` valida correctamente código TOTP
- ✅ `/TwoFactor/generate-recovery-codes` retorna 10 códigos
- ✅ Login con 2FA habilitado requiere código adicional

### Swagger Documentation

```yaml
/api/TwoFactor/enable:
  post:
    summary: Habilita autenticación de dos factores para el usuario actual
    responses:
      200:
        description: 2FA habilitado exitosamente
        content:
          application/json:
            example:
              success: true
              data:
                secretKey: "JBSWY3DPEHPK3PXP"
                qrCodeUrl: "otpauth://totp/CarDealer:test@example.com?secret=JBSWY3DPEHPK3PXP&issuer=CarDealer"
      400:
        description: Usuario ya tiene 2FA habilitado o email no confirmado
```

---

## 🟡 SPRINT 1.1.4: FIX ExternalAuth (CONFIGURACIÓN OAUTH)

**Prioridad:** P3 (Configuración externa)  
**Tokens estimados:** ~5,000  
**Duración estimada:** 1 sesión (30 minutos)  
**Bloqueante:** No - Feature opcional, requiere credenciales

### Contexto del Problema

**Endpoint:** `GET /api/ExternalAuth/linked-accounts`  
**Status Code:** 400 Bad Request  
**Request probado:**
```http
GET /api/ExternalAuth/linked-accounts
Authorization: Bearer {token}
Result: 400 Bad Request
```

**Causa probable:**
- Endpoint requiere configuración OAuth (Google/Microsoft Client ID/Secret)
- Sin configuración, middleware rechaza requests

### Tareas del Sprint

| ID | Tarea | Tokens | Descripción |
|----|-------|--------|-------------|
| 1.1.4.1 | Verificar appsettings.json | ~1,000 | Revisar Authentication:Google/Microsoft config |
| 1.1.4.2 | Revisar ExternalAuthController | ~1,500 | Verificar validaciones de config |
| 1.1.4.3 | Agregar validación condicional | ~2,000 | Retornar 501 Not Implemented si no hay config |
| 1.1.4.4 | Documentar configuración | ~500 | README con instrucciones OAuth setup |

### Archivos a Revisar

```
AuthService.Api/
├── Controllers/
│   └── ExternalAuthController.cs
├── appsettings.json                 ← Verificar Authentication section
└── appsettings.Development.json
```

### Configuración Requerida

```json
// appsettings.json
{
  "Authentication": {
    "Google": {
      "ClientId": "123456789-abc.apps.googleusercontent.com",
      "ClientSecret": "GOCSPX-xxxxxxxxxxxxxxxxxxxxxxxx"
    },
    "Microsoft": {
      "ClientId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
      "ClientSecret": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
    }
  }
}
```

### Solución Propuesta

**Hacer OAuth opcional:**

```csharp
[HttpGet("linked-accounts")]
public async Task<IActionResult> GetLinkedAccounts()
{
    if (!_oAuthConfig.IsConfigured)
    {
        return StatusCode(501, new { 
            error = "OAuth providers not configured",
            message = "Google and Microsoft authentication require configuration"
        });
    }
    
    // Lógica normal
}
```

### Criterios de Aceptación

- ✅ Sin configuración OAuth: Endpoint retorna 501 Not Implemented
- ✅ Con configuración OAuth: Endpoint funciona correctamente
- ✅ Documentación de configuración en README
- ✅ Swagger indica que feature es opcional

---

## 📊 MÉTRICAS DE PROGRESO

### Tracking de Issues

| Sprint | Issue | Severidad | Estado | Tokens | Duración |
|--------|-------|:---------:|:------:|:------:|:--------:|
| 1.1.1 | RefreshToken 401 | 🔴 | ⏳ Pendiente | 12,000 | 1-2h |
| 1.1.2 | Logout 400 | 🟡 | ⏳ Pendiente | 8,000 | 45min |
| 1.1.3 | TwoFactor 400 | 🟡 | ⏳ Pendiente | 10,000 | 1h |
| 1.1.4 | ExternalAuth 400 | 🟡 | ⏳ Pendiente | 5,000 | 30min |
| **TOTAL** | **5 issues** | - | **0%** | **35,000** | **4-6h** |

### Priorización

```
P0 (CRÍTICO) - Bloquea producción
└── Sprint 1.1.1: RefreshToken 401
    Impacto: Alto - Sin esto, usuarios deben re-login cada 1 hora
    
P1 (IMPORTANTE) - Afecta UX
└── Sprint 1.1.2: Logout 400
    Impacto: Medio - Usuarios no pueden cerrar sesión correctamente
    
P2 (OPCIONAL) - Features de seguridad
└── Sprint 1.1.3: TwoFactor endpoints
    Impacto: Bajo - Feature avanzada, no crítica
    
P3 (CONFIGURACIÓN) - Requiere setup externo
└── Sprint 1.1.4: ExternalAuth
    Impacto: Muy bajo - Feature opcional, requiere OAuth config
```

### Estimación de Esfuerzo

| Fase | Sprints | Tokens | Horas | Días |
|------|:-------:|:------:|:-----:|:----:|
| **Fase 1 (P0)** | 1 | 12,000 | 1-2 | 1 |
| **Fase 2 (P1)** | 1 | 8,000 | 1 | 1 |
| **Fase 3 (P2)** | 1 | 10,000 | 1 | 1 |
| **Fase 4 (P3)** | 1 | 5,000 | 0.5 | 1 |
| **TOTAL** | **4** | **35,000** | **3.5-4.5** | **1-2** |

---

## 🎯 CRITERIOS DE ACEPTACIÓN GLOBALES

### Para considerar los sprints completados:

**Fase 1 (P0) - CRÍTICA:**
- ✅ RefreshToken funciona correctamente (200 OK)
- ✅ Usuarios pueden renovar sesiones sin re-login
- ✅ Test: 5 refreshes consecutivos exitosos

**Fase 2 (P1) - IMPORTANTE:**
- ✅ Logout funciona correctamente (200 OK o 204 No Content)
- ✅ RefreshToken se invalida al hacer logout
- ✅ Test: Login → Logout → Refresh debe fallar (401)

**Fase 3 (P2) - OPCIONAL:**
- ✅ TwoFactor/enable genera QR code válido
- ✅ TwoFactor/generate-recovery-codes retorna 10 códigos
- ✅ Login con 2FA habilitado requiere código TOTP

**Fase 4 (P3) - CONFIGURACIÓN:**
- ✅ ExternalAuth retorna 501 sin config (no 400)
- ✅ Documentación de OAuth setup en README
- ✅ Con config, endpoint funciona correctamente

---

## 🚀 PLAN DE EJECUCIÓN

### Orden Recomendado

```
DÍA 1:
├── Sprint 1.1.1: RefreshToken (CRÍTICO) ← COMENZAR AQUÍ
│   └── Tiempo: 1-2 horas
│   └── Bloqueante: Sí
│
└── Sprint 1.1.2: Logout (IMPORTANTE)
    └── Tiempo: 45 minutos
    └── Bloqueante: No

DÍA 2:
├── Sprint 1.1.3: TwoFactor (OPCIONAL)
│   └── Tiempo: 1 hora
│   └── Bloqueante: No
│
└── Sprint 1.1.4: ExternalAuth (CONFIGURACIÓN)
    └── Tiempo: 30 minutos
    └── Bloqueante: No
```

### Dependencias

```
Sprint 1.1.1 (RefreshToken)
  ↓ NO depende de otros
  
Sprint 1.1.2 (Logout)
  ↓ NO depende de 1.1.1 (independiente)
  
Sprint 1.1.3 (TwoFactor)
  ├── TwoFactor/enable debe completarse ANTES de
  └── TwoFactor/generate-recovery-codes
  
Sprint 1.1.4 (ExternalAuth)
  ↓ NO depende de otros (configuración externa)
```

---

## 📝 NOTAS ADICIONALES

### Testing Environment

**Credenciales de prueba:**
```
Email: test@example.com
Password: Admin123!
UserId: 4a09dd28-a85a-4299-865c-d1df223ac2e4
```

**Base URL:**
```
http://localhost:15085/api
```

**Headers necesarios:**
```http
Authorization: Bearer {accessToken}
Content-Type: application/json
```

### Comandos Útiles

```powershell
# Generar nuevo login
$loginBody = @{email="test@example.com"; password="Admin123!"} | ConvertTo-Json
$loginResp = Invoke-WebRequest "http://localhost:15085/api/Auth/login" `
    -Method POST -Body $loginBody -ContentType "application/json" -UseBasicParsing
$result = $loginResp.Content | ConvertFrom-Json
$global:authToken = $result.data.accessToken
$global:refreshToken = $result.data.refreshToken

# Ver tabla RefreshTokens
docker exec authservice-db psql -U postgres -d authservice `
    -c "SELECT * FROM \"RefreshTokens\" WHERE \"UserId\" = '4a09dd28-a85a-4299-865c-d1df223ac2e4';"

# Ver logs de AuthService
docker logs authservice --tail 50
```

---

## 🏆 DEFINICIÓN DE HECHO (Definition of Done)

### Para cada sprint:

- ✅ Código corregido y compilando sin errores
- ✅ Endpoint retorna status code esperado (200 OK)
- ✅ Response body tiene estructura correcta
- ✅ Test manual exitoso (3 intentos consecutivos)
- ✅ Logs sin errores/warnings
- ✅ Swagger documentation actualizada
- ✅ Commit con mensaje descriptivo
- ✅ Documentación en sprint report

### Para el plan completo:

- ✅ 4/4 sprints completados
- ✅ 5/5 issues resueltos
- ✅ Documento de cierre generado
- ✅ Reporte final actualizado en SPRINT_1.1_AUTHSERVICE_AUDIT_REPORT.md
- ✅ Plan de sprints actualizado en MICROSERVICES_AUDIT_SPRINT_PLAN.md

---

*Documento generado: 1 Enero 2026 - 03:45*  
*Versión: 1.0*  
*Autor: Claude Opus 4.5*
