# 🔐 Session Security & Device Management - Matriz de Procesos

> **Servicio:** AuthService (SecurityController)  
> **Puerto:** 5001  
> **Última actualización:** Enero 26, 2026  
> **Estado:** 🟢 ACTIVO  
> **Estado de Implementación:** ✅ 100% Backend | ✅ 100% Tests | ✅ 100% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                           | Backend               | UI Access             | Observación            |
| --------------------------------- | --------------------- | --------------------- | ---------------------- |
| AUTH-SEC-001 Ver Sesiones         | ✅ SecurityController | ✅ ActiveSessionsPage | Lista completa         |
| AUTH-SEC-002 Revocar Sesión       | ✅ SecurityController | ✅ ActiveSessionsPage | Con código email       |
| AUTH-SEC-003 Revocar Todas        | ✅ SecurityController | ✅ Modal confirmación | Logout global          |
| AUTH-SEC-004 Cambiar Password     | ✅ SecurityController | ✅ ChangePasswordPage | Con validación         |
| AUTH-SEC-005 Dispositivo Revocado | ✅ SecurityController | ✅ LoginPage (inline) | Verificación integrada |

### Rutas UI Existentes ✅

- `/settings/security` → SecuritySettingsPage (general)
- `/settings/sessions` → ActiveSessionsPage (lista de sesiones)
- `/settings/password` → ChangePasswordPage
- `/login` → LoginPage (incluye verificación de dispositivo revocado inline)

**Verificación Backend:** AuthService/SecurityController existe en `/backend/AuthService/` ✅

---

## 📊 Resumen de Implementación

| Componente                 | Total | Implementado | Pendiente | Estado  |
| -------------------------- | ----- | ------------ | --------- | ------- |
| **Controllers**            | 1     | 1            | 0         | ✅ 100% |
| **Procesos (AUTH-SEC-\*)** | 5     | 5            | 0         | ✅ 100% |
| **Tests Unitarios**        | 5     | 5            | 0         | ✅ 100% |

### Leyenda de Estados

- ✅ **IMPLEMENTADO Y PROBADO**: Código completo con tests
- 🟢 **IMPLEMENTADO**: Código completo, falta testing
- 🟡 **EN PROGRESO**: Implementación parcial
- 🔴 **PENDIENTE**: No implementado

---

## 1. Información General

### 1.1 Descripción

Sistema de gestión de sesiones activas con seguridad avanzada para OKLA. Permite a los usuarios ver, revocar y gestionar sus sesiones de forma segura, con verificación por email y tracking de dispositivos revocados.

### 1.2 Dependencias

| Servicio            | Propósito                                          |
| ------------------- | -------------------------------------------------- |
| AuthService         | Servicio principal de autenticación                |
| NotificationService | Envío de códigos y alertas por email               |
| Redis               | Almacenamiento de códigos y dispositivos revocados |
| Gateway (Ocelot)    | Enrutamiento de APIs                               |

### 1.3 Tecnologías

- **Códigos de verificación:** 6 dígitos, SHA256 hash
- **Device Fingerprinting:** SHA256(IP + UserAgent)
- **Almacenamiento:** Redis con TTL configurable
- **Rate Limiting:** 3 requests/hora por sesión

---

## 2. Endpoints API

| Método   | Endpoint                                          | Descripción                    | Auth | Roles |
| -------- | ------------------------------------------------- | ------------------------------ | ---- | ----- |
| `GET`    | `/api/auth/security/sessions`                     | Listar sesiones activas        | ✅   | User  |
| `POST`   | `/api/auth/security/sessions/{id}/request-revoke` | Solicitar código de revocación | ✅   | User  |
| `DELETE` | `/api/auth/security/sessions/{id}`                | Revocar sesión con código      | ✅   | User  |
| `POST`   | `/api/auth/security/sessions/revoke-all`          | Revocar todas las sesiones     | ✅   | User  |
| `POST`   | `/api/auth/security/change-password`              | Cambiar contraseña             | ✅   | User  |
| `POST`   | `/api/auth/revoked-device/request-code`           | Solicitar código dispositivo   | ❌   | -     |
| `POST`   | `/api/auth/revoked-device/verify-login`           | Verificar código dispositivo   | ❌   | -     |

---

## 3. Entidades y Estructuras

### 3.1 ActiveSessionDto

```csharp
public class ActiveSessionDto
{
    public string Id { get; set; }
    public string Device { get; set; }
    public string Browser { get; set; }
    public string OperatingSystem { get; set; }
    public string Location { get; set; }
    public string IpAddress { get; set; }      // Enmascarada: "192.168.1.***"
    public DateTime LastActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsExpiringSoon { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

### 3.2 RevokedDeviceData

```csharp
public class RevokedDeviceData
{
    public string UserId { get; set; }
    public string DeviceFingerprint { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public string Browser { get; set; }
    public string OperatingSystem { get; set; }
    public DateTime RevokedAt { get; set; }
}
```

### 3.3 RevocationCodeData

```csharp
public class RevocationCodeData
{
    public string CodeHash { get; set; }        // SHA256 del código
    public DateTime ExpiresAt { get; set; }     // Expiración
    public int RemainingAttempts { get; set; }  // Intentos restantes (default: 3)
}
```

---

## 4. Procesos Detallados

### 4.1 AUTH-SEC-002: Listar Sesiones Activas

| Campo             | Valor                                          |
| ----------------- | ---------------------------------------------- |
| **ID Proceso**    | AUTH-SEC-002                                   |
| **Nombre**        | Listar Sesiones Activas                        |
| **Descripción**   | Obtiene todas las sesiones activas del usuario |
| **Endpoint**      | `GET /api/auth/security/sessions`              |
| **Auth Required** | ✅ Sí                                          |
| **Estado**        | ✅ IMPLEMENTADO Y PROBADO                      |

#### Request

```http
GET /api/auth/security/sessions
Authorization: Bearer {token}
```

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "sessions": [
      {
        "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "device": "MacBook Pro 14",
        "browser": "Chrome 120",
        "operatingSystem": "macOS Sonoma",
        "location": "Santo Domingo, RD",
        "ipAddress": "192.168.1.***",
        "lastActive": "2026-01-24T15:30:00Z",
        "createdAt": "2026-01-20T10:00:00Z",
        "isCurrent": true,
        "isExpiringSoon": false,
        "expiresAt": "2026-01-25T10:00:00Z"
      }
    ],
    "totalCount": 3,
    "currentSessionId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
  }
}
```

#### Características de Seguridad

- ✅ IP parcialmente enmascarada (`192.168.1.***`)
- ✅ Sesión actual marcada con `isCurrent: true`
- ✅ Sesiones próximas a expirar marcadas con `isExpiringSoon`
- ✅ Sanitización XSS en output

---

### 4.2 AUTH-SEC-003-A: Solicitar Código de Revocación

| Campo             | Valor                                                         |
| ----------------- | ------------------------------------------------------------- |
| **ID Proceso**    | AUTH-SEC-003-A                                                |
| **Nombre**        | Solicitar Código de Revocación                                |
| **Descripción**   | Envía código de 6 dígitos por email para revocar sesión       |
| **Endpoint**      | `POST /api/auth/security/sessions/{sessionId}/request-revoke` |
| **Auth Required** | ✅ Sí                                                         |
| **Estado**        | ✅ IMPLEMENTADO Y PROBADO                                     |

#### Request

```http
POST /api/auth/security/sessions/{sessionId}/request-revoke
Authorization: Bearer {token}
```

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "message": "Verification code sent to your email",
    "codeExpiresAt": "2026-01-24T15:35:00Z",
    "sessionId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
  }
}
```

#### Parámetros de Seguridad

| Parámetro            | Valor                  |
| -------------------- | ---------------------- |
| Código               | 6 dígitos numéricos    |
| Expiración           | 5 minutos              |
| Intentos máximos     | 3 por código           |
| Rate limit           | 3 solicitudes por hora |
| Bloqueo si se supera | 15 minutos lockout     |

#### Errores

| Código | Error                                       | Descripción                          |
| ------ | ------------------------------------------- | ------------------------------------ |
| 400    | "You cannot terminate your current session" | Intentó revocar sesión actual        |
| 400    | "Too many requests"                         | Rate limit excedido                  |
| 404    | "Session not found"                         | Sesión no existe o no es del usuario |

---

### 4.3 AUTH-SEC-003: Revocar Sesión con Código

| Campo             | Valor                                                        |
| ----------------- | ------------------------------------------------------------ |
| **ID Proceso**    | AUTH-SEC-003                                                 |
| **Nombre**        | Revocar Sesión con Verificación                              |
| **Descripción**   | Revoca una sesión específica después de verificar código     |
| **Endpoint**      | `DELETE /api/auth/security/sessions/{sessionId}?code={code}` |
| **Auth Required** | ✅ Sí                                                        |
| **Estado**        | ✅ IMPLEMENTADO Y PROBADO                                    |

#### Request

```http
DELETE /api/auth/security/sessions/{sessionId}?code=123456
Authorization: Bearer {token}
```

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "message": "Session terminated successfully. The device has been logged out.",
    "wasCurrentSession": false,
    "refreshTokenRevoked": true
  }
}
```

#### Flujo del Proceso

```
┌──────────────────────────────────────────────────────────────────────────┐
│                    FLUJO DE REVOCACIÓN DE SESIÓN                         │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  1. Usuario en SecuritySettingsPage                                      │
│     └─▶ Ve lista de sesiones activas                                    │
│                                                                          │
│  2. Click "Terminar sesión" en sesión remota                             │
│     └─▶ NO puede terminar sesión ACTUAL (bloqueado)                     │
│                                                                          │
│  3. Modal solicita confirmación                                          │
│     └─▶ Muestra detalles: dispositivo, navegador, IP, ubicación         │
│                                                                          │
│  4. Click "Continuar" → POST /request-revoke                             │
│     └─▶ Backend genera código 6 dígitos                                 │
│     └─▶ Almacena hash en Redis (5 min TTL)                              │
│     └─▶ Envía email con código                                          │
│                                                                          │
│  5. Usuario ingresa código en modal                                      │
│     └─▶ Frontend → DELETE /sessions/{id}?code=123456                    │
│                                                                          │
│  6. Backend valida código                                                │
│     └─▶ ❌ Código inválido → Decrementa intentos                        │
│     └─▶ ✅ Código válido → Continúa                                     │
│                                                                          │
│  7. Backend revoca sesión                                                │
│     a) Revoca sesión en UserSession                                     │
│     b) Revoca refresh token asociado                                    │
│     c) Marca dispositivo como revocado (30 días en Redis)               │
│     d) Envía email de notificación                                      │
│                                                                          │
│  8. Respuesta al frontend                                                │
│     └─▶ Toast de éxito                                                  │
│     └─▶ Lista de sesiones se actualiza                                  │
│                                                                          │
└──────────────────────────────────────────────────────────────────────────┘
```

#### Características de Seguridad

| Característica                  | Descripción                                           |
| ------------------------------- | ----------------------------------------------------- |
| **Bloqueo sesión actual**       | No permite revocar la sesión desde la que se opera    |
| **Verificación de propiedad**   | Valida que la sesión pertenece al usuario autenticado |
| **IDOR Prevention**             | Retorna 404 incluso para sesiones de otros usuarios   |
| **Revocación de refresh token** | Revoca token de refresco asociado                     |
| **Auditoría**                   | Log completo con TraceId/SpanId                       |
| **Notificación**                | Email al usuario del dispositivo revocado             |
| **Dispositivo marcado**         | Fingerprint almacenado para futuro control            |

---

### 4.4 AUTH-SEC-004: Revocar Todas las Sesiones

| Campo             | Valor                                         |
| ----------------- | --------------------------------------------- |
| **ID Proceso**    | AUTH-SEC-004                                  |
| **Nombre**        | Revocar Todas las Sesiones                    |
| **Descripción**   | Logout masivo de todos los dispositivos       |
| **Endpoint**      | `POST /api/auth/security/sessions/revoke-all` |
| **Auth Required** | ✅ Sí                                         |
| **Estado**        | ✅ IMPLEMENTADO Y PROBADO                     |

#### Request

```http
POST /api/auth/security/sessions/revoke-all?keepCurrentSession=true
Authorization: Bearer {token}
```

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "message": "All other sessions have been terminated",
    "sessionsRevoked": 4,
    "refreshTokensRevoked": 4,
    "currentSessionKept": true,
    "securityAlertSent": true,
    "revokedAt": "2026-01-24T15:30:00Z"
  }
}
```

#### Parámetros

| Parámetro          | Tipo | Default | Descripción                      |
| ------------------ | ---- | ------- | -------------------------------- |
| keepCurrentSession | bool | true    | Mantener la sesión actual activa |

---

### 4.5 AUTH-SEC-005: Login desde Dispositivo Revocado

| Campo             | Valor                                                              |
| ----------------- | ------------------------------------------------------------------ |
| **ID Proceso**    | AUTH-SEC-005                                                       |
| **Nombre**        | Verificación de Dispositivo Revocado                               |
| **Descripción**   | Flujo de verificación cuando un dispositivo revocado intenta login |
| **Endpoints**     | `POST /api/auth/revoked-device/*`                                  |
| **Auth Required** | ❌ No                                                              |
| **Estado**        | ✅ IMPLEMENTADO Y PROBADO                                          |

#### ¿Qué es un dispositivo revocado?

Cuando un usuario revoca una sesión remota (AUTH-SEC-003), el sistema:

1. Termina la sesión
2. Marca el **fingerprint del dispositivo** como revocado
3. Almacena en Redis por 30 días

Si alguien intenta hacer login desde ese dispositivo revocado, se activa este flujo.

#### Flujo del Proceso

```
┌──────────────────────────────────────────────────────────────────────────┐
│               FLUJO DE LOGIN DESDE DISPOSITIVO REVOCADO                  │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  1. Usuario intenta login desde dispositivo revocado                     │
│     └─▶ POST /api/auth/login                                            │
│                                                                          │
│  2. LoginCommandHandler verifica credenciales ✅                         │
│     └─▶ Credenciales válidas                                            │
│                                                                          │
│  3. Verifica si dispositivo está revocado                                │
│     └─▶ RevokedDeviceService.CheckIfDeviceIsRevokedAsync()              │
│     └─▶ Genera fingerprint: SHA256(userId + ipAddress + userAgent)       │
│     └─▶ Busca en Redis: revoked_device:{userId}:{fingerprint}           │
│                                                                          │
│  4. Dispositivo REVOCADO detectado                                       │
│     └─▶ Retorna LoginResponse con:                                      │
│         • requiresRevokedDeviceVerification: true                       │
│         • deviceFingerprint: "abc123..."                                │
│         • accessToken: "" (vacío)                                       │
│         • refreshToken: "" (vacío)                                      │
│                                                                          │
│  5. Frontend detecta requiresRevokedDeviceVerification                   │
│     └─▶ Almacena datos pendientes en localStorage                       │
│     └─▶ Llama POST /api/auth/revoked-device/request-code                │
│                                                                          │
│  6. Backend genera código de verificación                                │
│     └─▶ Código 6 dígitos, expira en 10 minutos                         │
│     └─▶ Máximo 3 intentos, lockout 30 minutos                          │
│     └─▶ Email enviado: "Alguien está intentando acceder..."            │
│                                                                          │
│  7. Usuario ingresa código en LoginPage                                  │
│     └─▶ POST /api/auth/revoked-device/verify-login                      │
│                                                                          │
│  8. Backend verifica código                                              │
│     └─▶ ❌ Inválido: Decrementa intentos                                │
│     └─▶ ✅ Válido: Limpia dispositivo de lista revocados               │
│                                                                          │
│  9. Frontend continúa con login normal                                   │
│     └─▶ Recupera datos de localStorage                                  │
│     └─▶ Reinicia login completo                                        │
│     └─▶ Ahora dispositivo está limpio                                  │
│                                                                          │
└──────────────────────────────────────────────────────────────────────────┘
```

#### Endpoint: Solicitar Código

```http
POST /api/auth/revoked-device/request-code
Content-Type: application/json

{
  "userId": "user-123",
  "email": "user@example.com",
  "deviceFingerprint": "abc123...",
  "ipAddress": "192.168.1.100",
  "userAgent": "Mozilla/5.0...",
  "browser": "Chrome 120",
  "operatingSystem": "macOS"
}
```

**Response:**

```json
{
  "requiresVerification": true,
  "message": "Se ha detectado un intento de inicio de sesión desde un dispositivo previamente desconectado...",
  "verificationToken": "vt_abc123...",
  "codeExpiresAt": "2026-01-24T15:40:00Z"
}
```

#### Endpoint: Verificar Código

```http
POST /api/auth/revoked-device/verify-login
Content-Type: application/json

{
  "verificationToken": "vt_abc123...",
  "code": "123456",
  "ipAddress": "192.168.1.100"
}
```

**Response:**

```json
{
  "success": true,
  "message": "Dispositivo verificado. Puedes continuar con el inicio de sesión.",
  "deviceCleared": true
}
```

---

## 5. Almacenamiento en Redis

### 5.1 Claves Utilizadas

| Prefijo                                       | Propósito                          | TTL     |
| --------------------------------------------- | ---------------------------------- | ------- |
| `session_revoke_code:{userId}:{sessionId}`    | Código de revocación de sesión     | 5 min   |
| `session_revoke_lockout:{userId}:{sessionId}` | Lockout por intentos fallidos      | 15 min  |
| `revoked_device:{userId}:{fingerprint}`       | Dispositivo marcado como revocado  | 30 días |
| `revoked_devices_list:{userId}`               | Lista de fingerprints revocados    | 30 días |
| `revoked_device_login:{token}`                | Código de verificación dispositivo | 10 min  |
| `revoked_device_lockout:{fingerprint}`        | Lockout de dispositivo             | 30 min  |

### 5.2 Device Fingerprint

El fingerprint del dispositivo se genera usando SHA256:

```csharp
public string GenerateDeviceFingerprint(string ipAddress, string userAgent)
{
    var combined = $"{ipAddress}:{userAgent}";
    using var sha256 = SHA256.Create();
    var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
    return Convert.ToBase64String(hash);
}
```

---

## 6. Notificaciones por Email

### 6.1 Código de Revocación (AUTH-SEC-003-A)

**Asunto:** `🔐 Código de verificación para cerrar sesión - OKLA`

**Contenido:**

- Código de 6 dígitos
- Información del dispositivo a cerrar
- Expiración del código
- Advertencia si no fue el usuario

### 6.2 Sesión Terminada (AUTH-SEC-003)

**Asunto:** `⚠️ Sesión cerrada en tu cuenta - OKLA`

**Contenido:**

- Detalles del dispositivo cerrado
- IP y ubicación
- Fecha/hora de cierre
- CTA para reportar si no fue el usuario

### 6.3 Alerta de Dispositivo Revocado (AUTH-SEC-005)

**Asunto:** `🚨 Intento de acceso desde dispositivo desconectado - OKLA`

**Contenido:**

- Alerta de que alguien intenta acceder
- Detalles del dispositivo
- Código de verificación
- Advertencia si no reconoce el intento

---

## 7. Configuración Gateway (Ocelot)

```json
{
  "Routes": [
    {
      "UpstreamPathTemplate": "/api/auth/security/sessions/{sessionId}/request-revoke",
      "UpstreamHttpMethod": ["POST"],
      "DownstreamPathTemplate": "/api/auth/security/sessions/{sessionId}/request-revoke",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "authservice", "Port": 8080 }],
      "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" }
    },
    {
      "UpstreamPathTemplate": "/api/auth/security/sessions/{sessionId}",
      "UpstreamHttpMethod": ["DELETE"],
      "DownstreamPathTemplate": "/api/auth/security/sessions/{sessionId}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "authservice", "Port": 8080 }],
      "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" }
    },
    {
      "UpstreamPathTemplate": "/api/auth/security/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"],
      "DownstreamPathTemplate": "/api/auth/security/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "authservice", "Port": 8080 }],
      "AuthenticationOptions": { "AuthenticationProviderKey": "Bearer" }
    },
    {
      "UpstreamPathTemplate": "/api/auth/revoked-device/{everything}",
      "UpstreamHttpMethod": ["POST"],
      "DownstreamPathTemplate": "/api/auth/revoked-device/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "authservice", "Port": 8080 }]
    }
  ]
}
```

---

## 8. Logs de Auditoría

### 8.1 Formato de Log

Todos los logs incluyen TraceId/SpanId para correlación:

```
[14:23:45 INF] AUTH-SEC-003: Session a1b2c3d4 successfully revoked by user user123
  with verification. Device: MacBook Pro, RefreshTokenRevoked: True,
  DeviceMarkedRevoked: True
  TraceId=4bf92f3577b34da6a3ce929d0e0e4736 SpanId=00f067aa0ba902b7
```

### 8.2 Eventos Logueados

| Evento                            | Nivel | Proceso      |
| --------------------------------- | ----- | ------------ |
| Session revocation initiated      | INFO  | AUTH-SEC-003 |
| Lockout activated                 | WARN  | AUTH-SEC-003 |
| Invalid code attempt              | WARN  | AUTH-SEC-003 |
| Session revoked successfully      | INFO  | AUTH-SEC-003 |
| SECURITY ALERT: IDOR attempt      | WARN  | AUTH-SEC-003 |
| Revoked device login detected     | WARN  | AUTH-SEC-005 |
| Device verification sent          | INFO  | AUTH-SEC-005 |
| Device cleared after verification | INFO  | AUTH-SEC-005 |

---

## 9. Manejo de Errores

### 9.1 Códigos HTTP

| Código | Significado                                            |
| ------ | ------------------------------------------------------ |
| 200    | Operación exitosa                                      |
| 400    | Request inválido (código incorrecto, formato inválido) |
| 401    | No autenticado                                         |
| 404    | Sesión no encontrada (o no pertenece al usuario)       |
| 429    | Rate limit excedido                                    |
| 500    | Error interno                                          |

### 9.2 Mensajes de Error Comunes

| Error                                       | Causa                                | Solución                    |
| ------------------------------------------- | ------------------------------------ | --------------------------- |
| "You cannot terminate your current session" | Intentar revocar sesión actual       | Usar logout en su lugar     |
| "Verification code expired"                 | Código expiró (5 min)                | Solicitar nuevo código      |
| "Invalid verification code"                 | Código incorrecto                    | Verificar código del email  |
| "Too many failed attempts"                  | 3+ intentos fallidos                 | Esperar 15/30 minutos       |
| "Session not found"                         | Sesión no existe o no es del usuario | Refrescar lista de sesiones |

---

## 10. Testing

### 10.1 Endpoints de Prueba

```bash
# Listar sesiones
curl -X GET "http://localhost:18443/api/auth/security/sessions" \
  -H "Authorization: Bearer {token}"

# Solicitar código de revocación
curl -X POST "http://localhost:18443/api/auth/security/sessions/{sessionId}/request-revoke" \
  -H "Authorization: Bearer {token}"

# Revocar sesión
curl -X DELETE "http://localhost:18443/api/auth/security/sessions/{sessionId}?code=123456" \
  -H "Authorization: Bearer {token}"

# Revocar todas las sesiones
curl -X POST "http://localhost:18443/api/auth/security/sessions/revoke-all?keepCurrentSession=true" \
  -H "Authorization: Bearer {token}"
```

### 10.2 Verificar Estado de Gateway

```bash
# Debe retornar 401 (ruta existe pero requiere auth)
curl -s -o /dev/null -w "%{http_code}" \
  http://localhost:18443/api/auth/security/sessions

# 401 = ✅ Ruta configurada correctamente
# 404 = ❌ Ruta no encontrada en Gateway
```

---

## 11. Archivos de Implementación

### Backend (AuthService)

| Archivo                                                                                     | Propósito                                      |
| ------------------------------------------------------------------------------------------- | ---------------------------------------------- |
| `Controllers/SecurityController.cs`                                                         | Endpoints REST para gestión de sesiones        |
| `Features/Auth/Commands/RevokeSession/RevokeSessionCommand.cs`                              | Command para revocar sesión                    |
| `Features/Auth/Commands/RevokeSession/RevokeSessionCommandHandler.cs`                       | Handler principal de revocación                |
| `Features/Auth/Commands/RevokeSession/RevokeSessionCommandValidator.cs`                     | Validador FluentValidation                     |
| `Features/Auth/Commands/VerifyRevokedDeviceLogin/VerifyRevokedDeviceLoginCommands.cs`       | Commands para dispositivo revocado             |
| `Features/Auth/Commands/VerifyRevokedDeviceLogin/VerifyRevokedDeviceLoginCommandHandler.cs` | Handler de verificación                        |
| `Services/RevokedDeviceService.cs`                                                          | Servicio de tracking de dispositivos revocados |
| `Features/Auth/Commands/Login/LoginCommandHandler.cs`                                       | Login handler (detecta dispositivo revocado)   |

### Frontend (React/TypeScript)

| Archivo                               | Propósito                                         |
| ------------------------------------- | ------------------------------------------------- |
| `services/securitySessionService.ts`  | Servicio de API para sesiones                     |
| `services/authService.ts`             | Servicio auth con métodos de dispositivo revocado |
| `pages/user/SecuritySettingsPage.tsx` | UI de configuración de seguridad                  |
| `pages/auth/LoginPage.tsx`            | Login con flujo de dispositivo revocado           |

---

## 12. Checklist de Implementación

### Backend ✅

- [x] SecurityController con endpoints de sesiones
- [x] RevokeSessionCommand y Handler
- [x] RequestSessionRevocationCommand y Handler
- [x] VerifyRevokedDeviceLoginCommand y Handler
- [x] RevokedDeviceService para tracking
- [x] LoginCommandHandler con check de dispositivo revocado
- [x] Integración con Redis (IDistributedCache)
- [x] Integración con NotificationService (emails)
- [x] Validadores FluentValidation
- [x] Logs de auditoría con TraceId/SpanId

### Frontend ✅

- [x] securitySessionService.ts con todos los métodos
- [x] authService.ts con métodos de dispositivo revocado
- [x] SecuritySettingsPage con modal de revocación
- [x] LoginPage con flujo de dispositivo revocado
- [x] Manejo de errores y toast notifications
- [x] localStorage para datos pendientes

### Gateway ✅

- [x] Rutas en ocelot.dev.json
- [x] Rutas en ocelot.prod.json
- [x] Autenticación configurada para rutas protegidas

### Testing ✅

- [x] Endpoints probados con curl
- [x] Gateway verificado (401 = ruta existe)
- [x] Flujo completo probado en UI

---

**Última actualización:** Enero 24, 2026  
**Autor:** Equipo de Desarrollo OKLA  
**Versión:** 1.0.0
