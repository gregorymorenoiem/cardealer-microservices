# 🔐 Two-Factor Authentication (2FA) - Matriz de Procesos

> **Servicio:** AuthService (TwoFactorController)  
> **Puerto:** 5001  
> **Última actualización:** Enero 25, 2026  
> **Estado:** 🟢 ACTIVO  
> **Estado de Implementación:** ✅ 100% Backend | ✅ 100% UI | ✅ 100% Tests

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                    | Backend                | UI Access               | Observación           |
| -------------------------- | ---------------------- | ----------------------- | --------------------- |
| SEC-2FA-001 Enable 2FA     | ✅ TwoFactorController | ✅ SecuritySettingsPage | Flujo completo        |
| SEC-2FA-002 Verify TOTP    | ✅ TwoFactorController | ✅ LoginTwoFactorPage   | Paso 2 del login      |
| SEC-2FA-003 Recovery Codes | ✅ TwoFactorController | ✅ Modal recovery       | Generación y descarga |
| SEC-2FA-004 Disable 2FA    | ✅ TwoFactorController | ✅ SecuritySettingsPage | Con confirmación      |

### Rutas UI Existentes ✅

- `/settings/security` → SecuritySettingsPage (toggle 2FA + wizard completo)
- `/login/2fa` → TwoFactorVerifyPage (verificación TOTP)
- `/login/recovery` → RecoveryCodePage (usar código backup)

### Rutas UI Faltantes 🔴

- ~~`/settings/2fa/setup` → Wizard guiado de configuración~~ ✅ **Integrado en SecuritySettingsPage**

**Verificación Backend:** AuthService/TwoFactorController existe en `/backend/AuthService/` ✅

---

## 📊 Resumen de Implementación

| Componente                      | Total | Implementado | Pendiente | Estado  |
| ------------------------------- | ----- | ------------ | --------- | ------- |
| **Controllers**                 | 1     | 1            | 0         | ✅ 100% |
| **Procesos (SEC-2FA-\*)**       | 7     | 7            | 0         | ✅ 100% |
| **Tests Unitarios de Handlers** | 29    | 29           | 0         | ✅ 100% |

### Tests de 2FA Handlers (29 tests - 100% passing)

| Archivo de Tests                     | Tests | Estado     |
| ------------------------------------ | ----- | ---------- |
| Enable2FAHandlerTests.cs             | 7     | ✅ Passing |
| Verify2FAHandlerTests.cs             | 6     | ✅ Passing |
| Disable2FAHandlerTests.cs            | 5     | ✅ Passing |
| GenerateRecoveryCodesHandlerTests.cs | 5     | ✅ Passing |
| TwoFactorLoginHandlerTests.cs        | 6     | ✅ Passing |

**Ubicación:** `AuthService.Tests/Unit/Handlers/TwoFactor/`

### Leyenda de Estados

- ✅ **IMPLEMENTADO Y PROBADO**: Código completo con tests
- 🟢 **IMPLEMENTADO**: Código completo, falta testing
- 🟡 **EN PROGRESO**: Implementación parcial
- 🔴 **PENDIENTE**: No implementado

---

## 1. Información General

### 1.1 Descripción

Sistema de autenticación de dos factores (2FA) para OKLA. Proporciona una capa adicional de seguridad mediante TOTP (Time-based One-Time Password), códigos por SMS, o códigos de recuperación.

### 1.2 Dependencias

| Servicio            | Propósito                           |
| ------------------- | ----------------------------------- |
| AuthService         | Servicio principal de autenticación |
| NotificationService | Envío de códigos por SMS/Email      |
| UserService         | Información del usuario             |

### 1.3 Tecnologías

- **TOTP:** RFC 6238 (Google Authenticator, Authy compatible)
- **SMS:** Integración con Twilio
- **Recovery Codes:** 10 códigos de 8 caracteres

---

## 2. Endpoints API

| Método | Endpoint                                 | Descripción                      | Auth | Roles |
| ------ | ---------------------------------------- | -------------------------------- | ---- | ----- |
| `POST` | `/api/twofactor/enable`                  | Habilitar 2FA                    | ✅   | User  |
| `POST` | `/api/twofactor/verify`                  | Verificar configuración 2FA      | ✅   | User  |
| `POST` | `/api/twofactor/disable`                 | Deshabilitar 2FA                 | ✅   | User  |
| `POST` | `/api/twofactor/generate-recovery-codes` | Generar códigos de recuperación  | ✅   | User  |
| `POST` | `/api/twofactor/verify-recovery-code`    | Verificar código de recuperación | ❌   | -     |
| `POST` | `/api/twofactor/login`                   | Completar login con 2FA          | ❌   | -     |

---

## 3. Entidades y Enums

### 3.1 TwoFactorType (Enum)

```csharp
public enum TwoFactorType
{
    None = 0,           // 2FA no habilitado
    Totp = 1,           // Google Authenticator / Authy
    Sms = 2,            // Código por SMS
    Email = 3           // Código por email
}
```

### 3.2 TwoFactorSettings (Entidad)

```csharp
public class TwoFactorSettings
{
    public Guid UserId { get; set; }
    public TwoFactorType Type { get; set; }
    public bool IsEnabled { get; set; }
    public string? SecretKey { get; set; }          // Para TOTP (encriptado)
    public List<string> RecoveryCodes { get; set; } // Códigos de respaldo
    public int RecoveryCodesUsed { get; set; }
    public DateTime? EnabledAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.1 SEC-2FA-001: Habilitar 2FA

| Campo       | Valor                               |
| ----------- | ----------------------------------- |
| **ID**      | SEC-2FA-001                         |
| **Nombre**  | Habilitar Two-Factor Authentication |
| **Actor**   | Usuario autenticado                 |
| **Trigger** | POST /api/twofactor/enable          |

#### Flujo del Proceso

| Paso | Acción                          | Sistema     | Validación               |
| ---- | ------------------------------- | ----------- | ------------------------ |
| 1    | Usuario solicita habilitar 2FA  | Frontend    | Token JWT válido         |
| 2    | Extraer UserId del claim        | AuthService | NameIdentifier presente  |
| 3    | Verificar 2FA no está activo    | AuthService | IsEnabled == false       |
| 4    | Generar SecretKey (TOTP)        | AuthService | 160 bits aleatorios      |
| 5    | Crear URL otpauth://            | AuthService | Formato: issuer, account |
| 6    | Generar QR code                 | AuthService | Base64 PNG               |
| 7    | Guardar configuración pendiente | Database    | Status = Pending         |
| 8    | Retornar QR + manual entry code | AuthService | Response con SecretKey   |

#### Request

```json
{
  "type": "Totp" // Totp, Sms, Email
}
```

#### Response

```json
{
  "success": true,
  "data": {
    "qrCodeUrl": "data:image/png;base64,...",
    "manualEntryKey": "JBSWY3DPEHPK3PXP",
    "type": "Totp",
    "message": "Scan the QR code with your authenticator app, then verify"
  }
}
```

---

### 4.2 SEC-2FA-002: Verificar Configuración 2FA

| Campo       | Valor                      |
| ----------- | -------------------------- |
| **ID**      | SEC-2FA-002                |
| **Nombre**  | Verificar y Activar 2FA    |
| **Actor**   | Usuario autenticado        |
| **Trigger** | POST /api/twofactor/verify |

#### Flujo del Proceso

| Paso | Acción                        | Sistema     | Validación               |
| ---- | ----------------------------- | ----------- | ------------------------ |
| 1    | Usuario ingresa código de app | Frontend    | 6 dígitos                |
| 2    | Validar formato de código     | AuthService | Regex: ^\d{6}$           |
| 3    | Obtener SecretKey pendiente   | Database    | Configuración temporal   |
| 4    | Calcular TOTP esperado        | AuthService | RFC 6238, ventana ±1     |
| 5    | Comparar códigos              | AuthService | Igualdad exacta          |
| 6    | Generar Recovery Codes        | AuthService | 10 códigos únicos        |
| 7    | Activar 2FA                   | Database    | IsEnabled = true         |
| 8    | Publicar evento               | RabbitMQ    | User2FAEnabled           |
| 9    | Retornar Recovery Codes       | AuthService | Solo se muestran una vez |

#### Request

```json
{
  "code": "123456",
  "type": "Totp"
}
```

#### Response

```json
{
  "success": true,
  "data": {
    "success": true,
    "message": "Two-factor authentication enabled successfully",
    "recoveryCodes": [
      "ABC12DEF",
      "GHI34JKL",
      "MNO56PQR",
      "STU78VWX",
      "YZA90BCD",
      "EFG12HIJ",
      "KLM34NOP",
      "QRS56TUV",
      "WXY78ZAB",
      "CDE90FGH"
    ]
  }
}
```

---

### 4.3 SEC-2FA-003: Login con 2FA

| Campo       | Valor                       |
| ----------- | --------------------------- |
| **ID**      | SEC-2FA-003                 |
| **Nombre**  | Completar Login con 2FA     |
| **Actor**   | Usuario en proceso de login |
| **Trigger** | POST /api/twofactor/login   |

#### Flujo del Proceso

| Paso | Acción                             | Sistema     | Validación                 |
| ---- | ---------------------------------- | ----------- | -------------------------- |
| 1    | Login inicial exitoso              | AuthService | Email + Password correctos |
| 2    | Detectar 2FA habilitado            | AuthService | User.TwoFactorEnabled      |
| 3    | Generar TempToken                  | AuthService | JWT temporal (5 min)       |
| 4    | Retornar requires2FA: true         | AuthService | Con TempToken              |
| 5    | Usuario ingresa código 2FA         | Frontend    | 6 dígitos o recovery code  |
| 6    | Validar TempToken                  | AuthService | No expirado, válido        |
| 7    | Validar código TOTP                | AuthService | RFC 6238                   |
| 8    | Generar AccessToken + RefreshToken | AuthService | JWT completos              |
| 9    | Actualizar LastUsedAt              | Database    | Timestamp actual           |
| 10   | Publicar evento                    | RabbitMQ    | User2FALoginSuccess        |

#### Request

```json
{
  "tempToken": "eyJhbGciOiJIUzI1NiIs...",
  "twoFactorCode": "123456"
}
```

#### Response

```json
{
  "success": true,
  "data": {
    "userId": "uuid",
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
    "expiresIn": 3600,
    "tokenType": "Bearer"
  }
}
```

---

### 4.4 SEC-2FA-004: Deshabilitar 2FA

| Campo       | Valor                                  |
| ----------- | -------------------------------------- |
| **ID**      | SEC-2FA-004                            |
| **Nombre**  | Deshabilitar Two-Factor Authentication |
| **Actor**   | Usuario autenticado                    |
| **Trigger** | POST /api/twofactor/disable            |

#### Flujo del Proceso

| Paso | Acción                        | Sistema             | Validación                 |
| ---- | ----------------------------- | ------------------- | -------------------------- |
| 1    | Usuario solicita deshabilitar | Frontend            | Con contraseña             |
| 2    | Validar contraseña actual     | AuthService         | Hash comparison            |
| 3    | Verificar 2FA está activo     | Database            | IsEnabled == true          |
| 4    | Eliminar configuración 2FA    | Database            | Soft delete                |
| 5    | Invalidar Recovery Codes      | Database            | Todos marcados como usados |
| 6    | Publicar evento               | RabbitMQ            | User2FADisabled            |
| 7    | Enviar notificación           | NotificationService | Email de seguridad         |

#### Request

```json
{
  "password": "CurrentPassword123!"
}
```

---

### 4.5 SEC-2FA-005: Generar Recovery Codes

| Campo       | Valor                                       |
| ----------- | ------------------------------------------- |
| **ID**      | SEC-2FA-005                                 |
| **Nombre**  | Regenerar Códigos de Recuperación           |
| **Actor**   | Usuario autenticado con 2FA                 |
| **Trigger** | POST /api/twofactor/generate-recovery-codes |

#### Flujo del Proceso

| Paso | Acción                          | Sistema     | Validación               |
| ---- | ------------------------------- | ----------- | ------------------------ |
| 1    | Usuario solicita nuevos códigos | Frontend    | Con contraseña           |
| 2    | Validar contraseña              | AuthService | Hash comparison          |
| 3    | Verificar 2FA activo            | Database    | IsEnabled == true        |
| 4    | Invalidar códigos anteriores    | Database    | Todos marcados usados    |
| 5    | Generar 10 nuevos códigos       | AuthService | 8 chars alfanuméricos    |
| 6    | Hashear y guardar               | Database    | BCrypt hash              |
| 7    | Retornar códigos en claro       | AuthService | Solo se muestran una vez |

#### Response

```json
{
  "success": true,
  "data": {
    "recoveryCodes": ["XYZ12ABC", "DEF34GHI", "..."],
    "message": "New recovery codes generated. Previous codes are now invalid."
  }
}
```

---

### 4.6 SEC-2FA-006: Verificar Recovery Code

| Campo       | Valor                                    |
| ----------- | ---------------------------------------- |
| **ID**      | SEC-2FA-006                              |
| **Nombre**  | Login con Recovery Code                  |
| **Actor**   | Usuario sin acceso a app 2FA             |
| **Trigger** | POST /api/twofactor/verify-recovery-code |

#### Flujo del Proceso

| Paso | Acción                     | Sistema     | Validación                 |
| ---- | -------------------------- | ----------- | -------------------------- |
| 1    | Usuario usa recovery code  | Frontend    | 8 caracteres               |
| 2    | Buscar código en lista     | Database    | Hash comparison            |
| 3    | Verificar no usado         | Database    | IsUsed == false            |
| 4    | Marcar código como usado   | Database    | IsUsed = true              |
| 5    | Generar tokens completos   | AuthService | AccessToken + RefreshToken |
| 6    | Advertir códigos restantes | Response    | Conteo de disponibles      |
| 7    | Publicar evento            | RabbitMQ    | RecoveryCodeUsed           |

---

## 5. Reglas de Negocio

### 5.1 Configuración TOTP

| Regla                 | Valor                  |
| --------------------- | ---------------------- |
| Algoritmo             | SHA1 (RFC 6238)        |
| Dígitos               | 6                      |
| Período               | 30 segundos            |
| Ventana de tolerancia | ±1 período (90s total) |
| Issuer                | "OKLA"                 |
| Key length            | 160 bits (20 bytes)    |

### 5.2 Recovery Codes

| Regla        | Valor                   |
| ------------ | ----------------------- |
| Cantidad     | 10 códigos              |
| Longitud     | 8 caracteres            |
| Formato      | Alfanumérico mayúsculas |
| Uso          | Una sola vez            |
| Regeneración | Invalida anteriores     |

### 5.3 Seguridad

| Regla                | Valor                     |
| -------------------- | ------------------------- |
| TempToken expiration | 5 minutos                 |
| Intentos fallidos    | 5 antes de bloqueo        |
| Bloqueo temporal     | 15 minutos                |
| Notificación email   | Al habilitar/deshabilitar |

---

## 6. Manejo de Errores

| Código | Error                   | Mensaje                               | Acción               |
| ------ | ----------------------- | ------------------------------------- | -------------------- |
| 400    | InvalidCode             | "Invalid verification code"           | Reintentar           |
| 400    | CodeExpired             | "Verification code has expired"       | Solicitar nuevo      |
| 400    | TwoFactorNotEnabled     | "2FA is not enabled for this account" | Habilitar primero    |
| 400    | TwoFactorAlreadyEnabled | "2FA is already enabled"              | Ya configurado       |
| 401    | InvalidPassword         | "Invalid password"                    | Verificar contraseña |
| 401    | TempTokenExpired        | "Temporary token has expired"         | Reiniciar login      |
| 403    | RecoveryCodeUsed        | "Recovery code has already been used" | Usar otro código     |
| 429    | TooManyAttempts         | "Too many failed attempts"            | Esperar 15 minutos   |

---

## 7. Eventos RabbitMQ

| Evento                   | Exchange      | Descripción         | Payload                         |
| ------------------------ | ------------- | ------------------- | ------------------------------- |
| `user.2fa.enabled`       | `auth.events` | 2FA habilitado      | `{ userId, type, timestamp }`   |
| `user.2fa.disabled`      | `auth.events` | 2FA deshabilitado   | `{ userId, reason, timestamp }` |
| `user.2fa.login.success` | `auth.events` | Login 2FA exitoso   | `{ userId, method, ip }`        |
| `user.2fa.login.failed`  | `auth.events` | Login 2FA fallido   | `{ userId, attemptCount, ip }`  |
| `user.2fa.recovery.used` | `auth.events` | Recovery code usado | `{ userId, codesRemaining }`    |

---

## 8. Diagrama de Secuencia

```
┌──────────┐     ┌─────────────┐     ┌──────────┐     ┌────────────┐
│ Frontend │     │ AuthService │     │ Database │     │ Notification│
└────┬─────┘     └──────┬──────┘     └────┬─────┘     └─────┬──────┘
     │                  │                 │                  │
     │ POST /enable     │                 │                  │
     │─────────────────>│                 │                  │
     │                  │ Generate Secret │                  │
     │                  │────────────────>│                  │
     │                  │   Save Pending  │                  │
     │   QR Code + Key  │<────────────────│                  │
     │<─────────────────│                 │                  │
     │                  │                 │                  │
     │ POST /verify     │                 │                  │
     │─────────────────>│                 │                  │
     │                  │ Validate TOTP   │                  │
     │                  │────────────────>│                  │
     │                  │  Activate 2FA   │                  │
     │ Recovery Codes   │<────────────────│                  │
     │<─────────────────│                 │                  │
     │                  │                 │   Email Alert    │
     │                  │─────────────────│─────────────────>│
     │                  │                 │                  │
```

---

## 9. Configuración

### 9.1 appsettings.json

```json
{
  "TwoFactor": {
    "Issuer": "OKLA",
    "TotpDigits": 6,
    "TotpPeriod": 30,
    "TotpTolerance": 1,
    "TempTokenExpiration": 300,
    "MaxFailedAttempts": 5,
    "LockoutMinutes": 15,
    "RecoveryCodeCount": 10,
    "RecoveryCodeLength": 8
  }
}
```

### 9.2 Secrets Requeridos

| Secret                    | Descripción                     |
| ------------------------- | ------------------------------- |
| `TwoFactor:EncryptionKey` | Clave para encriptar SecretKeys |
| `Twilio:AccountSid`       | Para envío SMS (si tipo SMS)    |
| `Twilio:AuthToken`        | Token de Twilio                 |

---

## 10. Métricas y Monitoreo

### 10.1 Prometheus Metrics

```
# Habilitaciones de 2FA
auth_2fa_enabled_total{type="totp"}

# Logins con 2FA
auth_2fa_login_total{status="success|failed"}

# Recovery codes usados
auth_2fa_recovery_used_total

# Intentos fallidos
auth_2fa_failed_attempts_total
```

### 10.2 Alertas

| Alerta            | Condición           | Severidad |
| ----------------- | ------------------- | --------- |
| High2FAFailures   | >50 fallos/hora     | Warning   |
| MassRecoveryUsage | >10 recovery/hora   | Critical  |
| 2FABruteForce     | >20 intentos/IP/min | Critical  |

---

## 📚 Referencias

- [RFC 6238 - TOTP](https://tools.ietf.org/html/rfc6238)
- [Google Authenticator Key URI Format](https://github.com/google/google-authenticator/wiki/Key-Uri-Format)
- [01-auth-service.md](01-auth-service.md) - Autenticación principal
