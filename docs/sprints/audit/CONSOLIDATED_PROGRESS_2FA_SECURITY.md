# 🔐 CONTROL DE AVANCES - Sistema 2FA y Seguridad AuthService

> **Fecha de Creación**: 22 de Enero, 2026  
> **Última Actualización**: 22 de Enero, 2026  
> **Servicio**: AuthService  
> **Versión**: 1.0.0  
> **Framework**: .NET 8.0 + Docker (puerto 15085)

---

## 📋 RESUMEN EJECUTIVO

Este documento consolida el estado de **TODOS los procesos de autenticación de dos factores (2FA)** implementados y probados en AuthService, incluyendo las correcciones de seguridad aplicadas.

### Estado General

| Categoría                          | Estado            | Notas                            |
| ---------------------------------- | ----------------- | -------------------------------- |
| **2FA Authenticator (TOTP)**       | ✅ 100% Funcional | Probado con Google Authenticator |
| **2FA SMS**                        | ✅ 100% Funcional | Probado con número real          |
| **Recovery Codes**                 | ✅ 100% Funcional | 10 códigos, uso único            |
| **Recovery con Todos los Códigos** | ✅ 100% Funcional | Recuperación completa            |
| **Seguridad (Timing Attacks)**     | ✅ CORREGIDO      | Constant-time comparison         |
| **Rate Limiting SMS**              | ✅ IMPLEMENTADO   | Max 3 SMS/hora                   |
| **Lockout Protection**             | ✅ IMPLEMENTADO   | 5 intentos → 30 min              |

---

## 🎯 ENDPOINTS 2FA - ESTADO DE PRUEBAS

### Controlador: TwoFactorController

Ubicación: `AuthService.Api/Controllers/TwoFactorController.cs`

| #   | Endpoint                           | Método | Auth    | Estado     | Última Prueba |
| --- | ---------------------------------- | ------ | ------- | ---------- | ------------- |
| 1   | `/api/2fa/enable`                  | POST   | ✅ JWT  | ✅ PROBADO | 22/01/2026    |
| 2   | `/api/2fa/verify`                  | POST   | ✅ JWT  | ✅ PROBADO | 22/01/2026    |
| 3   | `/api/2fa/disable`                 | POST   | ✅ JWT  | ✅ PROBADO | 22/01/2026    |
| 4   | `/api/2fa/generate-recovery-codes` | POST   | ✅ JWT  | ✅ PROBADO | 22/01/2026    |
| 5   | `/api/2fa/verify-recovery-code`    | POST   | ❌ Anon | ✅ PROBADO | 22/01/2026    |
| 6   | `/api/2fa/login`                   | POST   | ❌ Anon | ✅ PROBADO | 22/01/2026    |
| 7   | `/api/2fa/login-with-recovery`     | POST   | ❌ Anon | ✅ PROBADO | 22/01/2026    |
| 8   | `/api/2fa/recover-with-all-codes`  | POST   | ❌ Anon | ✅ PROBADO | 22/01/2026    |
| 9   | `/api/2fa/send-sms-code`           | POST   | ❌ Anon | ✅ PROBADO | 22/01/2026    |
| 10  | `/api/2fa/verify-sms-code`         | POST   | ❌ Anon | ✅ PROBADO | 22/01/2026    |

---

## ✅ FLUJOS PROBADOS Y FUNCIONANDO

### 1️⃣ Flujo: Habilitar 2FA con Authenticator

**Estado**: ✅ COMPLETAMENTE PROBADO

```
Usuario autenticado
    ↓
POST /api/2fa/enable
    ↓ Retorna: QR Code + Secret + Recovery Codes (10)
    ↓
Usuario escanea QR en Google Authenticator
    ↓
POST /api/2fa/verify { code: "123456" }
    ↓ 2FA activado exitosamente
```

**Cuenta de Prueba**:

- Email: `gregorymoreno_iem@hotmail.com`
- UserId: `ff5c251b-e554-4e03-8d36-83afa3851255`
- Método 2FA: Authenticator (TOTP)

---

### 2️⃣ Flujo: Login con 2FA Authenticator

**Estado**: ✅ COMPLETAMENTE PROBADO

```
POST /api/auth/login { email, password }
    ↓ Retorna: requiresTwoFactor: true, tempToken: "xxx"
    ↓
POST /api/2fa/login { tempToken, code: "123456" }
    ↓ Retorna: accessToken, refreshToken, user
```

**Resultado**: Tokens válidos, login exitoso

---

### 3️⃣ Flujo: Login con Recovery Code

**Estado**: ✅ COMPLETAMENTE PROBADO

```
POST /api/auth/login { email, password }
    ↓ Retorna: requiresTwoFactor: true, tempToken: "xxx"
    ↓
POST /api/2fa/login-with-recovery { tempToken, recoveryCode: "H29S41MV" }
    ↓ Retorna: accessToken, refreshToken, remainingCodes: 9
```

**Características probadas**:

- ✅ Código se consume después de uso (single-use)
- ✅ Contador de códigos restantes
- ✅ Warning cuando quedan 3 o menos códigos
- ✅ **NUEVO**: Lockout después de 5 intentos fallidos (30 min)

---

### 4️⃣ Flujo: Recuperación Completa con 10 Códigos

**Estado**: ✅ COMPLETAMENTE PROBADO

```
POST /api/auth/login { email, password }
    ↓ Retorna: requiresTwoFactor: true, tempToken: "xxx"
    ↓
POST /api/2fa/recover-with-all-codes {
    tempToken,
    recoveryCodes: ["code1", "code2", ..., "code10"]
}
    ↓ Retorna:
    ↓   - Nuevo QR Code + Secret
    ↓   - 10 nuevos Recovery Codes
    ↓   - accessToken, refreshToken
```

**Casos de uso**:

- ✅ Usuario perdió acceso al authenticator
- ✅ Recovery codes individuales expiraron en Redis
- ✅ Usuario tiene los 10 códigos originales en papel/guardados
- ✅ **NUEVO**: Lockout después de 3 intentos fallidos (60 min)

---

### 5️⃣ Flujo: 2FA con SMS

**Estado**: ✅ COMPLETAMENTE PROBADO

```
POST /api/auth/login { email, password }
    ↓ Retorna: requiresTwoFactor: true, tempToken: "xxx"
    ↓
POST /api/2fa/send-sms-code { tempToken }
    ↓ SMS enviado al teléfono registrado
    ↓ Retorna: maskedPhone: "+1***567", expiresIn: 300
    ↓
POST /api/2fa/verify-sms-code { tempToken, code: "123456" }
    ↓ Retorna: accessToken, refreshToken, user
```

**Características probadas**:

- ✅ SMS real enviado (Twilio integrado)
- ✅ Código expira en 5 minutos
- ✅ Número enmascarado para privacidad
- ✅ **NUEVO**: Rate limiting (max 3 SMS/hora)
- ✅ **NUEVO**: Lockout después de 5 intentos fallidos (15 min)

---

### 6️⃣ Flujo: Deshabilitar 2FA

**Estado**: ✅ COMPLETAMENTE PROBADO

```
POST /api/2fa/disable {
    password: "actual_password",
    code: "123456"  // Código TOTP actual
}
    ↓ 2FA deshabilitado
    ↓ Recovery codes eliminados
```

**Seguridad**:

- ✅ Requiere contraseña actual
- ✅ Requiere código 2FA válido
- ✅ Limpia todos los códigos de recuperación

---

### 7️⃣ Flujo: Regenerar Recovery Codes

**Estado**: ✅ COMPLETAMENTE PROBADO

```
POST /api/2fa/generate-recovery-codes {
    password: "actual_password"
}
    ↓ Retorna: 10 nuevos Recovery Codes
    ↓ Códigos anteriores invalidados
```

**Seguridad**:

- ✅ Requiere contraseña actual
- ✅ Invalida códigos anteriores
- ✅ Códigos de 8 caracteres alfanuméricos

---

## 🔒 CORRECCIONES DE SEGURIDAD IMPLEMENTADAS

### Sesión del 22 de Enero, 2026

Se identificaron y corrigieron **6 vulnerabilidades críticas**:

### ✅ FIX 1: Timing Attack en Verificación SMS

**Archivo**: `VerifySms2FACodeCommandHandler.cs`

**Antes** (Vulnerable):

```csharp
if (storedCode != code)
    throw new Exception("Invalid code");
```

**Después** (Seguro):

```csharp
var storedBytes = Encoding.UTF8.GetBytes(storedCode);
var codeBytes = Encoding.UTF8.GetBytes(code);
if (!CryptographicOperations.FixedTimeEquals(storedBytes, codeBytes))
    throw new Exception("Invalid code");
```

---

### ✅ FIX 2: Timing Attack en Recovery Codes

**Archivo**: `TwoFactorService.cs` - Método `VerifyRecoveryCodeAsync`

**Antes** (Vulnerable):

```csharp
return recoveryCodes.Contains(code);
```

**Después** (Seguro):

```csharp
bool isValid = false;
var codeBytes = Encoding.UTF8.GetBytes(code.Trim().ToUpperInvariant());

foreach (var storedCode in recoveryCodes)
{
    var storedBytes = Encoding.UTF8.GetBytes(storedCode.Trim().ToUpperInvariant());
    if (CryptographicOperations.FixedTimeEquals(codeBytes, storedBytes))
    {
        isValid = true;
        // Continuar iterando para evitar timing leak
    }
}
return isValid;
```

---

### ✅ FIX 3: Generación Insegura de Códigos

**Archivo**: `TwoFactorService.cs` - Método `GenerateRandomCode`

**Antes** (Vulnerable):

```csharp
var random = new System.Random();
return random.Next(100000, 999999).ToString("D6");
```

**Después** (Seguro):

```csharp
using var rng = RandomNumberGenerator.Create();
var bytes = new byte[4];
rng.GetBytes(bytes);
var value = BitConverter.ToUInt32(bytes, 0) % 900000 + 100000;
return value.ToString("D6");
```

---

### ✅ FIX 4: Rate Limiting en SMS

**Archivo**: `SendSms2FACodeCommandHandler.cs`

**Implementación**:

```csharp
private const int MAX_SMS_PER_HOUR = 3;
private const int SMS_RATE_LIMIT_MINUTES = 60;

// En Handle():
var rateLimitKey = $"sms_rate_limit:{userId}";
var currentCount = await GetCurrentSmsCountAsync(rateLimitKey);

if (currentCount >= MAX_SMS_PER_HOUR)
{
    throw new InvalidOperationException(
        $"Too many SMS requests. Maximum {MAX_SMS_PER_HOUR} SMS per hour. Please wait before requesting another code.");
}
```

---

### ✅ FIX 5: Lockout en RecoveryCodeLogin

**Archivo**: `RecoveryCodeLoginCommandHandler.cs`

**Implementación**:

```csharp
private const int MAX_FAILED_ATTEMPTS = 5;
private const int LOCKOUT_MINUTES = 30;

// Verifica lockout antes de procesar
var lockoutKey = $"recovery_login_lockout:{userId}";
if (await IsLockedOutAsync(lockoutKey))
    throw new InvalidOperationException($"Too many failed attempts. Account locked for {LOCKOUT_MINUTES} minutes.");

// Incrementa contador en caso de fallo
await TrackFailedRecoveryAttemptAsync(lockoutKey);
```

---

### ✅ FIX 6: Lockout Estricto en Full Recovery

**Archivo**: `RecoveryAccountWithAllCodesCommandHandler.cs`

**Implementación**:

```csharp
private const int MAX_FAILED_ATTEMPTS = 3;  // Más estricto
private const int LOCKOUT_MINUTES = 60;     // 1 hora

// Verifica lockout antes de procesar
var lockoutKey = $"full_recovery_lockout:{userId}";
if (await IsLockedOutAsync(lockoutKey))
    throw new InvalidOperationException($"Too many failed recovery attempts. Locked for {LOCKOUT_MINUTES} minutes.");
```

---

## 📊 TESTS EXISTENTES

### Archivos de Tests en AuthService.Tests

| Carpeta             | Archivo                                   | Tipo               |
| ------------------- | ----------------------------------------- | ------------------ |
| `Unit/Controllers/` | `AuthControllerTests.cs`                  | Unit               |
| `Integration/Api/`  | `TwoFactorEndpointTests.cs`               | Integration        |
| `Integration/Api/`  | `TwoFactorEndpointDockerTests.cs`         | Integration Docker |
| `Integration/Api/`  | `TwoFactorRealFlowTests.cs`               | Integration Real   |
| `Integration/Api/`  | `TwoFactorRealFlowDockerTests.cs`         | Integration Docker |
| `Integration/Api/`  | `PhoneVerificationEndpointTests.cs`       | Integration        |
| `Integration/Api/`  | `PhoneVerificationRealFlowTests.cs`       | Integration Real   |
| `Integration/Api/`  | `PhoneVerificationRealFlowDockerTests.cs` | Integration Docker |
| `Integration/Api/`  | `AuthEndpointTests.cs`                    | Integration        |
| `Integration/Api/`  | `AuthEndpointDockerTests.cs`              | Integration Docker |
| `Integration/Api/`  | `ExternalAuthEndpointTests.cs`            | Integration        |
| `Integration/Api/`  | `ExternalAuthRealFlowTests.cs`            | Integration Real   |
| `E2E/`              | `AuthFlowE2ETests.cs`                     | End-to-End         |
| `E2E/`              | `AuthFlowE2EDockerTests.cs`               | E2E Docker         |
| `E2E/`              | `CompleteApiDockerE2ETests.cs`            | E2E Completo       |

---

## 📦 DEPENDENCIAS DE SEGURIDAD

```xml
<!-- System.Security.Cryptography - Built-in .NET 8 -->
using System.Security.Cryptography;

// Métodos utilizados:
- CryptographicOperations.FixedTimeEquals()  // Constant-time comparison
- RandomNumberGenerator.Create()              // CSPRNG
```

---

## 🔴 TAREAS PENDIENTES (Sprint 18)

Ver documento completo: [SPRINT_PLAN_PENDING_TASKS.md](./SPRINT_PLAN_PENDING_TASKS.md)

| User Story | Descripción                                           | Esfuerzo | Estado       |
| ---------- | ----------------------------------------------------- | -------- | ------------ |
| US-18.1    | Recovery Codes Persistencia Dual (Redis + PostgreSQL) | 2-3h     | ⏳ Pendiente |
| US-18.2    | Notificación de Intentos Fallidos por Email           | 3-4h     | ⏳ Pendiente |
| US-18.3    | CAPTCHA después de 2 Intentos Fallidos                | 4-5h     | ⏳ Pendiente |
| US-18.4    | Device Fingerprinting                                 | 5-6h     | ⏳ Pendiente |
| US-18.5    | Audit Logging a SIEM                                  | 3-4h     | ⏳ Pendiente |

---

## 🧪 CUENTA DE PRUEBAS

```yaml
Email: gregorymoreno_iem@hotmail.com
Password: $Gregory12
UserId: ff5c251b-e554-4e03-8d36-83afa3851255
2FA Method: Authenticator (TOTP)
Phone: +1XXXXXXXXX (verificado)
```

---

## 🚀 CÓMO PROBAR

### 1. Levantar el servicio

```bash
cd backend/AuthService
docker compose up -d
# O con docker-compose global desde la raíz:
docker compose -f compose.yaml up authservice -d
```

### 2. Verificar Health

```bash
curl http://localhost:15085/health/ready
# Respuesta esperada: Healthy
```

### 3. Login con 2FA

```bash
# Paso 1: Login inicial
curl -X POST http://localhost:15085/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"gregorymoreno_iem@hotmail.com","password":"$Gregory12"}'

# Respuesta: { "requiresTwoFactor": true, "tempToken": "xxx" }

# Paso 2: Completar 2FA
curl -X POST http://localhost:15085/api/2fa/login \
  -H "Content-Type: application/json" \
  -d '{"tempToken":"xxx","code":"123456"}'

# Respuesta: { "accessToken": "...", "refreshToken": "..." }
```

---

## 📈 BUILD STATUS

```bash
$ dotnet build AuthService.Api/AuthService.Api.csproj

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.45
```

---

## 🏁 CONCLUSIÓN

**AuthService 2FA está LISTO PARA PRODUCCIÓN** con todas las correcciones de seguridad implementadas:

1. ✅ **Timing attacks** prevenidos con constant-time comparison
2. ✅ **Generación de códigos** usando CSPRNG criptográficamente seguro
3. ✅ **Rate limiting** en envío de SMS (3/hora)
4. ✅ **Account lockout** en todos los flujos de recovery
5. ✅ **Flujos completos** probados manualmente
6. ✅ **Tests automatizados** existentes para regresión

### Siguiente Paso

Implementar Sprint 18 (Seguridad Avanzada) para:

- Notificaciones por email de intentos fallidos
- CAPTCHA para prevenir bots
- Device fingerprinting
- Integración con SIEM

---

_Documento generado: 22 de Enero, 2026_  
_Autor: Análisis de Seguridad Automatizado_  
_Revisado por: Gregory Moreno_
