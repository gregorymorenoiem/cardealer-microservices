# AuthService

## 📋 Overview
The **AuthService** is responsible for user authentication, authorization, and identity management within the CarDealer ecosystem. It handles user registration, login, token generation (JWT), and role management.

## 🚀 Features
- **Authentication**: User login with email/password.
- **Authorization**: Role-based access control (RBAC).
- **Token Management**: JWT generation, validation, and refresh tokens.
- **Two-Factor Authentication (2FA)**: TOTP-based 2FA with recovery codes.
- **Security**: Password hashing, account lockout, and rate limiting.
- **Integration**: Publishes events to RabbitMQ for other services.

### 🔐 Sprint 18: Seguridad Avanzada 2FA (v3.0.0)

| Feature | Description |
|---------|-------------|
| **Recovery Codes Dual Persistence** | Codes stored in Redis (365d) + PostgreSQL for resilience |
| **Security Alert Notifications** | Email alerts after 3+ failed login/2FA attempts |
| **CAPTCHA Integration** | Google reCAPTCHA v3 required after 2 failed logins |
| **Device Fingerprinting** | Track and manage trusted devices per user |
| **SIEM Audit Logging** | Structured logs for Splunk/Elasticsearch/Datadog |

## 🔐 Two-Factor Authentication (2FA)

### Endpoints

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/TwoFactor/enable` | POST | ✅ | Enable 2FA for user |
| `/api/TwoFactor/verify` | POST | ✅ | Verify TOTP code |
| `/api/TwoFactor/disable` | POST | ✅ | Disable 2FA |
| `/api/TwoFactor/login` | POST | ❌ | Login with TOTP code |
| `/api/TwoFactor/login-with-recovery` | POST | ❌ | Login with single recovery code |
| `/api/TwoFactor/recover-with-all-codes` | POST | ❌ | **Account recovery with ALL 10 codes** |
| `/api/TwoFactor/generate-recovery-codes` | POST | ✅ | Generate new recovery codes |

### Account Recovery Flow

When a user loses access to their authenticator app, they can recover their account by providing **ALL 10 original recovery codes**:

```
POST /api/TwoFactor/recover-with-all-codes
{
  "tempToken": "<from login response>",
  "recoveryCodes": ["CODE1", "CODE2", ..., "CODE10"]
}
```

**Response (success):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJ...",
    "refreshToken": "abc123...",
    "newAuthenticatorSecret": "ABCD1234...",
    "newQrCodeUri": "<base64 PNG image>",
    "newRecoveryCodes": ["NEW1", "NEW2", ..., "NEW10"],
    "message": "Account recovered successfully..."
  }
}
```

### ⚠️ Nota Temporal: Envío de QR por Correo

**Estado actual (Enero 2026):**
- 🔧 **TEMPORAL**: El código QR se envía por correo electrónico al usuario durante la configuración de 2FA y recuperación de cuenta.
- 📧 El email incluye: QR code (imagen base64), secret manual, y los 10 recovery codes.

**Cuando el UI esté listo:**
- ✅ El código QR se mostrará **directamente en la interfaz de usuario**.
- 📧 Por correo solo se enviarán los **10 recovery codes** como respaldo.
- 🔐 El secret nunca viajará por email en producción final.

### Recovery Codes

- Se generan **10 códigos únicos** de 8 caracteres cuando se habilita 2FA.
- Cada código es de **un solo uso** para `login-with-recovery`.
- Para **recuperación total** (`recover-with-all-codes`) se requieren los 10 códigos.
- Después de una recuperación exitosa, se generan **10 códigos nuevos**.

## � Session Management & Security (AUTH-SEC)

### Overview

El sistema de gestión de sesiones implementa seguridad avanzada para proteger las cuentas de usuarios:

| Proceso | Descripción |
|---------|-------------|
| **AUTH-SEC-001** | Cambio de contraseña con revocación de sesiones |
| **AUTH-SEC-002** | Listar sesiones activas (IP enmascarada) |
| **AUTH-SEC-003** | Revocar sesión específica con verificación por email |
| **AUTH-SEC-003-A** | Solicitar código de verificación para revocación |
| **AUTH-SEC-004** | Revocar todas las sesiones (logout masivo) |
| **AUTH-SEC-005** | Verificación de login desde dispositivo revocado |

### Session Security Endpoints

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/auth/security/sessions` | GET | ✅ | Listar sesiones activas |
| `/api/auth/security/sessions/{id}/request-revoke` | POST | ✅ | Solicitar código de revocación |
| `/api/auth/security/sessions/{id}` | DELETE | ✅ | Revocar sesión con código |
| `/api/auth/security/sessions/revoke-all` | POST | ✅ | Revocar todas las sesiones |
| `/api/auth/security/change-password` | POST | ✅ | Cambiar contraseña |
| `/api/auth/revoked-device/request-code` | POST | ❌ | Solicitar código para dispositivo revocado |
| `/api/auth/revoked-device/verify-login` | POST | ❌ | Verificar código y completar login |

### Session Revocation Flow

```
1. Usuario ve lista de sesiones activas
2. Click "Terminar sesión" en sesión remota
3. Backend envía código 6 dígitos por email (5 min TTL)
4. Usuario ingresa código
5. Sesión revocada + refresh token invalidado
6. Dispositivo marcado como "revocado" (30 días)
```

### Revoked Device Flow

Cuando un dispositivo previamente revocado intenta hacer login:

```
1. Login con credenciales válidas
2. Sistema detecta dispositivo revocado
3. Envía código de verificación por email
4. Usuario verifica código
5. Dispositivo limpiado, login permitido
```

### Security Features

- ✅ **Bloqueo de sesión actual**: No puede revocar su propia sesión activa
- ✅ **IP enmascarada**: Muestra `192.168.1.***` en listado
- ✅ **Rate limiting**: 3 solicitudes de código por hora
- ✅ **Lockout**: 15-30 minutos después de 3 intentos fallidos
- ✅ **IDOR prevention**: Retorna 404 para sesiones de otros usuarios
- ✅ **Device fingerprinting**: SHA256(userId + IP + UserAgent)
- ✅ **Audit logging**: TraceId/SpanId para correlación

📚 **Documentación completa**: [05-session-security.md](../../docs/process-matrix/01-AUTENTICACION-SEGURIDAD/05-session-security.md)

## �🛠️ Tech Stack
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Database**: PostgreSQL (Entity Framework Core)
- **Caching**: Redis (Distributed Cache)
- **Messaging**: RabbitMQ (MassTransit/Raw Client)
- **Observability**: OpenTelemetry, Serilog
- **Resilience**: Polly

## 🏃‍♂️ Getting Started

### Prerequisites
- .NET 8 SDK
- Docker & Docker Compose
- PostgreSQL
- Redis
- RabbitMQ

### Running Locally
```bash
cd backend/AuthService/AuthService.Api
dotnet run
```

### Running with Docker
```bash
docker-compose up -d authservice
```

## 🧪 Testing
```bash
dotnet test backend/AuthService/AuthService.Tests
```

## 📚 Documentation
- [Architecture](ARCHITECTURE.md)
- [Advanced Features Implementation](ADVANCED_FEATURES_IMPLEMENTATION.md)
- [Session Security & Device Management](../../docs/process-matrix/01-AUTENTICACION-SEGURIDAD/05-session-security.md)
- [Changelog](CHANGELOG.md)
- [Troubleshooting](TROUBLESHOOTING.md)
- [API Documentation](http://localhost:5000/swagger)
