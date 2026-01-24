# 🔓 Unlink Active OAuth Provider - Matriz de Procesos

> **Servicio:** AuthService (ExternalAuthController)  
> **Puerto:** 5001  
> **Última actualización:** Enero 24, 2026  
> **Estado:** 🟡 PENDIENTE IMPLEMENTACIÓN  
> **Procesos ID:** AUTH-EXT-008, AUTH-PWD-001

---

## 📊 Resumen de Implementación

| Componente                    | Total | Implementado | Pendiente | Estado |
| ----------------------------- | ----- | ------------ | --------- | ------ |
| **Backend Handlers**          | 4     | 0            | 4         | 🔴 0%  |
| **Frontend Components**       | 3     | 0            | 3         | 🔴 0%  |
| **Validaciones de Seguridad** | 10    | 2            | 8         | 🟡 20% |
| **Tests Unitarios**           | 12    | 0            | 12        | 🔴 0%  |

### Leyenda de Estados

- ✅ **IMPLEMENTADO Y PROBADO**: Código completo con tests
- 🟢 **IMPLEMENTADO**: Código completo, falta testing
- 🟡 **EN PROGRESO**: Implementación parcial
- 🔴 **PENDIENTE**: No implementado

---

## 📑 Índice de Procesos

| Proceso ID   | Nombre                       | Descripción                                         |
| ------------ | ---------------------------- | --------------------------------------------------- |
| AUTH-PWD-001 | Set Password for OAuth User  | Configurar contraseña para usuarios sin password    |
| AUTH-EXT-008 | Unlink Active OAuth Provider | Desvincular proveedor OAuth activo con verificación |

---

## 1. Descripción del Problema

### 1.1 Escenario Actual

Cuando un usuario se registró/logueó usando un proveedor OAuth (ej: Google) y NO tiene contraseña configurada:

```
┌──────────────────────────────────────────────────────────────────────────┐
│                    ESCENARIO DE RIESGO ACTUAL                            │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  Usuario "Juan" se registró con Google                                   │
│  ├─▶ email: juan@gmail.com                                              │
│  ├─▶ ExternalAuthProvider: Google                                       │
│  ├─▶ PasswordHash: NULL (nunca configuró contraseña)                    │
│  └─▶ Está logueado usando su sesión de Google                          │
│                                                                          │
│  Si Juan intenta UNLINK su cuenta de Google:                            │
│                                                                          │
│  ❌ SIN PROCESO SEGURO:                                                 │
│  └─▶ Podría perder acceso permanente a su cuenta                       │
│  └─▶ No puede recuperar vía "forgot password" (no hay password)         │
│  └─▶ Sus datos, favoritos, publicaciones quedan inaccesibles           │
│                                                                          │
│  ✅ CON PROCESO SEGURO (AUTH-EXT-008):                                  │
│  └─▶ Sistema detecta que está desvinculando proveedor activo           │
│  └─▶ OBLIGA a configurar contraseña ANTES de desvincular               │
│  └─▶ Requiere verificación por email con código                        │
│  └─▶ Revoca todas las sesiones después de unlink                       │
│  └─▶ Envía notificación de seguridad                                   │
│                                                                          │
└──────────────────────────────────────────────────────────────────────────┘
```

### 1.2 ¿Por qué es crítico?

| Riesgo                             | Impacto    | Sin AUTH-EXT-008                           | Con AUTH-EXT-008                   |
| ---------------------------------- | ---------- | ------------------------------------------ | ---------------------------------- |
| **Pérdida de acceso**              | 🔴 CRÍTICO | Usuario bloqueado permanentemente          | Obligado a tener contraseña        |
| **Cuenta huérfana**                | 🔴 CRÍTICO | Datos sin dueño, posible leak              | Siempre hay método de acceso       |
| **Ataque de ingeniería social**    | 🟠 ALTO    | Atacante con acceso temporal puede excluir | Verificación por email obligatoria |
| **Sesión compartida comprometida** | 🟠 ALTO    | Dispositivo robado puede desvincular       | Código al email real del usuario   |
| **Audit trail incompleto**         | 🟡 MEDIO   | No hay registro detallado                  | Log completo con TraceId           |

---

## 2. AUTH-PWD-001: Configurar Contraseña para Usuarios OAuth

### 2.1 Descripción

Los usuarios que se registraron usando OAuth (Google, Facebook, Microsoft, Apple) **NO tienen contraseña configurada**. Antes de poder desvincular su cuenta OAuth, DEBEN configurar una contraseña para garantizar acceso alternativo.

### 2.2 Flujo Completo - Set Password via Email

```
┌──────────────────────────────────────────────────────────────────────────────────────────┐
│                    AUTH-PWD-001: SET PASSWORD FOR OAUTH USER                             │
├──────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                          │
│  TRIGGER: Usuario intenta desvincular OAuth pero NO tiene contraseña                    │
│  ════════════════════════════════════════════════════════════                           │
│                                                                                          │
│  PASO 1: Validación detecta que no tiene contraseña                                     │
│  ══════════════════════════════════════════════════                                     │
│  │                                                                                       │
│  └─▶ POST /api/ExternalAuth/unlink-account/validate                                     │
│      │                                                                                   │
│      └─▶ Response (200 - Requires Password):                                            │
│          {                                                                               │
│            "canUnlink": false,                                                          │
│            "isActiveProvider": true,                                                    │
│            "hasPassword": false,                                                        │
│            "blockReason": "PASSWORD_REQUIRED",                                          │
│            "message": "Debes configurar una contraseña antes de desvincular...",       │
│            "actionRequired": {                                                          │
│              "type": "SET_PASSWORD_VIA_EMAIL",                                          │
│              "description": "Te enviaremos un correo para configurar tu contraseña"    │
│            }                                                                            │
│          }                                                                               │
│                                                                                          │
│  PASO 2: Frontend muestra modal de configuración de contraseña                          │
│  ═════════════════════════════════════════════════════════════                          │
│  │                                                                                       │
│  └─▶ Modal muestra:                                                                     │
│      ┌─────────────────────────────────────────────────────────────────────┐            │
│      │ 🔐 CONTRASEÑA REQUERIDA                                            │            │
│      │─────────────────────────────────────────────────────────────────────│            │
│      │                                                                     │            │
│      │ Para desvincular tu cuenta de Google, primero necesitas            │            │
│      │ configurar una contraseña.                                         │            │
│      │                                                                     │            │
│      │ Sin una contraseña, perderías acceso permanente a tu cuenta.       │            │
│      │                                                                     │            │
│      │ Te enviaremos un enlace seguro a:                                  │            │
│      │ 📧 j***n@gmail.com                                                 │            │
│      │                                                                     │            │
│      │ El enlace expirará en 1 hora.                                      │            │
│      │                                                                     │            │
│      │        [Cancelar]    [Enviar Enlace de Configuración]              │            │
│      └─────────────────────────────────────────────────────────────────────┘            │
│                                                                                          │
│  PASO 3: Usuario solicita email de configuración                                        │
│  ═══════════════════════════════════════════════                                        │
│  │                                                                                       │
│  └─▶ POST /api/auth/password/setup-request                                              │
│      │                                                                                   │
│      │  Request Body: (No body needed - uses JWT userId)                                │
│      │                                                                                   │
│      └─▶ Backend:                                                                       │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ STEP 1: Validar que usuario NO tiene contraseña                    │        │
│          │ ═══════════════════════════════════════════════                    │        │
│          │                                                                     │        │
│          │ var user = await _userRepository.GetByIdAsync(userId);             │        │
│          │                                                                     │        │
│          │ IF (!string.IsNullOrEmpty(user.PasswordHash))                       │        │
│          │    throw BadRequest("Ya tienes una contraseña configurada.         │        │
│          │                      Usa 'Cambiar Contraseña' en su lugar.");      │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ STEP 2: Generar token seguro                                       │        │
│          │ ══════════════════════════════                                     │        │
│          │                                                                     │        │
│          │ var token = GenerateSecureToken(64); // 64 bytes random            │        │
│          │ var tokenHash = SHA256(token);                                      │        │
│          │                                                                     │        │
│          │ // Almacenar en Redis                                               │        │
│          │ await _cache.SetAsync(                                              │        │
│          │     $"password_setup_token:{userId}",                               │        │
│          │     new PasswordSetupData {                                         │        │
│          │         TokenHash = tokenHash,                                      │        │
│          │         ExpiresAt = DateTime.UtcNow.AddHours(1),                    │        │
│          │         UserId = userId,                                            │        │
│          │         Email = user.Email,                                         │        │
│          │         RequestedAt = DateTime.UtcNow,                              │        │
│          │         IpAddress = request.IpAddress                               │        │
│          │     },                                                              │        │
│          │     TimeSpan.FromHours(1) // TTL                                    │        │
│          │ );                                                                  │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ STEP 3: Enviar email con enlace seguro                             │        │
│          │ ═════════════════════════════════════                              │        │
│          │                                                                     │        │
│          │ var setupUrl = $"{frontendUrl}/auth/set-password?token={token}";   │        │
│          │                                                                     │        │
│          │ await _notificationService.SendEmailAsync(new {                    │        │
│          │     To = user.Email,                                                │        │
│          │     Template = "PASSWORD_SETUP_OAUTH_USER",                         │        │
│          │     Data = new {                                                    │        │
│          │         UserName = user.FirstName ?? user.Email,                   │        │
│          │         SetupUrl = setupUrl,                                        │        │
│          │         ExpiresIn = "1 hora",                                       │        │
│          │         IpAddress = request.IpAddress,                              │        │
│          │         Timestamp = DateTime.UtcNow                                 │        │
│          │     }                                                               │        │
│          │ });                                                                 │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│      Response (200 OK):                                                                  │
│      {                                                                                   │
│        "success": true,                                                                 │
│        "message": "Te hemos enviado un correo con el enlace para configurar            │
│                    tu contraseña",                                                      │
│        "emailSentTo": "j***n@gmail.com",                                               │
│        "expiresAt": "2026-01-24T17:00:00Z"                                             │
│      }                                                                                   │
│                                                                                          │
│  PASO 4: Usuario recibe email                                                           │
│  ════════════════════════════                                                           │
│  │                                                                                       │
│  └─▶ Email contiene:                                                                    │
│      ┌─────────────────────────────────────────────────────────────────────┐            │
│      │ 🔐 CONFIGURA TU CONTRASEÑA - OKLA                                  │            │
│      │─────────────────────────────────────────────────────────────────────│            │
│      │                                                                     │            │
│      │ Hola Juan,                                                         │            │
│      │                                                                     │            │
│      │ Recibimos tu solicitud para configurar una contraseña en tu        │            │
│      │ cuenta de OKLA.                                                    │            │
│      │                                                                     │            │
│      │ Actualmente inicias sesión con Google. Al configurar una           │            │
│      │ contraseña, podrás:                                                │            │
│      │ • Iniciar sesión con email y contraseña                           │            │
│      │ • Desvincular tu cuenta de Google si lo deseas                    │            │
│      │ • Tener un método de acceso de respaldo                           │            │
│      │                                                                     │            │
│      │           [ CONFIGURAR CONTRASEÑA ]                                │            │
│      │                                                                     │            │
│      │ Este enlace expira en 1 hora.                                      │            │
│      │                                                                     │            │
│      │ Si no solicitaste esto, puedes ignorar este correo.               │            │
│      │ Tu cuenta seguirá funcionando con Google normalmente.             │            │
│      │                                                                     │            │
│      │ Detalles de la solicitud:                                          │            │
│      │ • Fecha: 24 de enero 2026, 4:00 PM                                 │            │
│      │ • IP: 192.168.1.100                                                │            │
│      │                                                                     │            │
│      └─────────────────────────────────────────────────────────────────────┘            │
│                                                                                          │
│  PASO 5: Usuario hace click en enlace                                                   │
│  ════════════════════════════════════                                                   │
│  │                                                                                       │
│  └─▶ Redirige a: /auth/set-password?token=abc123...                                    │
│      │                                                                                   │
│      └─▶ Frontend SetPasswordPage.tsx:                                                 │
│          1. Extrae token de URL                                                         │
│          2. Valida token: POST /api/auth/password/setup-validate                       │
│          3. Si válido → Muestra formulario                                             │
│          4. Si inválido/expirado → Muestra error + botón reenviar                     │
│                                                                                          │
│  PASO 6: Validar token                                                                  │
│  ═════════════════════                                                                  │
│  │                                                                                       │
│  └─▶ POST /api/auth/password/setup-validate                                             │
│      │                                                                                   │
│      │  Request Body:                                                                    │
│      │  {                                                                                │
│      │    "token": "abc123..."                                                          │
│      │  }                                                                                │
│      │                                                                                   │
│      └─▶ Backend:                                                                       │
│          • Busca token en Redis por hash                                               │
│          • Verifica que no haya expirado                                               │
│          • Retorna email enmascarado para mostrar                                      │
│                                                                                          │
│      Response (200 OK):                                                                  │
│      {                                                                                   │
│        "valid": true,                                                                   │
│        "email": "j***n@gmail.com",                                                     │
│        "expiresAt": "2026-01-24T17:00:00Z"                                             │
│      }                                                                                   │
│                                                                                          │
│      Response (400 - Invalid/Expired):                                                   │
│      {                                                                                   │
│        "valid": false,                                                                  │
│        "error": "TOKEN_EXPIRED",                                                        │
│        "message": "Este enlace ha expirado. Solicita uno nuevo."                       │
│      }                                                                                   │
│                                                                                          │
│  PASO 7: Usuario configura contraseña                                                   │
│  ════════════════════════════════════                                                   │
│  │                                                                                       │
│  └─▶ Formulario en SetPasswordPage:                                                    │
│      ┌─────────────────────────────────────────────────────────────────────┐            │
│      │ 🔐 CONFIGURA TU CONTRASEÑA                                         │            │
│      │─────────────────────────────────────────────────────────────────────│            │
│      │                                                                     │            │
│      │ Email: j***n@gmail.com                                             │            │
│      │                                                                     │            │
│      │ Nueva Contraseña:                                                   │            │
│      │ ┌───────────────────────────────────────────────────────┐          │            │
│      │ │ ••••••••••••                                     👁️  │          │            │
│      │ └───────────────────────────────────────────────────────┘          │            │
│      │                                                                     │            │
│      │ ✅ Mínimo 8 caracteres                                             │            │
│      │ ✅ Al menos una mayúscula                                          │            │
│      │ ✅ Al menos un número                                              │            │
│      │ ⬜ Al menos un carácter especial (!@#$%...)                        │            │
│      │                                                                     │            │
│      │ Confirmar Contraseña:                                               │            │
│      │ ┌───────────────────────────────────────────────────────┐          │            │
│      │ │ ••••••••••••                                     👁️  │          │            │
│      │ └───────────────────────────────────────────────────────┘          │            │
│      │                                                                     │            │
│      │ ✅ Las contraseñas coinciden                                       │            │
│      │                                                                     │            │
│      │              [ Configurar Contraseña ]                              │            │
│      └─────────────────────────────────────────────────────────────────────┘            │
│                                                                                          │
│  PASO 8: Guardar contraseña                                                             │
│  ══════════════════════════                                                             │
│  │                                                                                       │
│  └─▶ POST /api/auth/password/setup-complete                                             │
│      │                                                                                   │
│      │  Request Body:                                                                    │
│      │  {                                                                                │
│      │    "token": "abc123...",                                                         │
│      │    "newPassword": "MySecureP@ssword123"                                          │
│      │  }                                                                                │
│      │                                                                                   │
│      └─▶ Backend Handler (SetPasswordForOAuthUserCommandHandler):                       │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ STEP 1: Validar token nuevamente                                   │        │
│          │ ═══════════════════════════════                                    │        │
│          │                                                                     │        │
│          │ var tokenHash = SHA256(request.Token);                              │        │
│          │ var data = await FindTokenDataByHash(tokenHash);                    │        │
│          │                                                                     │        │
│          │ IF (data == null || data.ExpiresAt < DateTime.UtcNow)               │        │
│          │    throw BadRequest("Token inválido o expirado")                   │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ STEP 2: Validar contraseña                                         │        │
│          │ ═══════════════════════════                                        │        │
│          │                                                                     │        │
│          │ // Validar requisitos de contraseña                                 │        │
│          │ ValidatePasswordStrength(request.NewPassword);                      │        │
│          │ // Min 8 chars, 1 uppercase, 1 number, 1 special                   │        │
│          │                                                                     │        │
│          │ // Verificar que no sea contraseña común                           │        │
│          │ await CheckAgainstCommonPasswords(request.NewPassword);            │        │
│          │                                                                     │        │
│          │ // Verificar que no contenga email                                 │        │
│          │ CheckPasswordNotContainsEmail(request.NewPassword, user.Email);    │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ STEP 3: Guardar contraseña hasheada                                │        │
│          │ ═══════════════════════════════════                                │        │
│          │                                                                     │        │
│          │ var user = await _userRepository.GetByIdAsync(data.UserId);        │        │
│          │                                                                     │        │
│          │ // Re-verificar que no tenga password (race condition check)       │        │
│          │ IF (!string.IsNullOrEmpty(user.PasswordHash))                       │        │
│          │    throw BadRequest("Ya tienes contraseña configurada")            │        │
│          │                                                                     │        │
│          │ // Hash de contraseña con Argon2 o BCrypt                          │        │
│          │ user.PasswordHash = _passwordHasher.HashPassword(                  │        │
│          │     user, request.NewPassword);                                     │        │
│          │ user.PasswordSetAt = DateTime.UtcNow;                               │        │
│          │                                                                     │        │
│          │ await _userRepository.UpdateAsync(user);                            │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ STEP 4: Limpiar token usado                                        │        │
│          │ ════════════════════════════                                       │        │
│          │                                                                     │        │
│          │ await _cache.RemoveAsync($"password_setup_token:{data.UserId}");   │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ STEP 5: Enviar notificación de confirmación                        │        │
│          │ ══════════════════════════════════════════                         │        │
│          │                                                                     │        │
│          │ await _notificationService.SendEmailAsync(new {                    │        │
│          │     To = user.Email,                                                │        │
│          │     Template = "PASSWORD_SET_CONFIRMATION",                         │        │
│          │     Data = new {                                                    │        │
│          │         UserName = user.FirstName,                                  │        │
│          │         Timestamp = DateTime.UtcNow,                                │        │
│          │         IpAddress = request.IpAddress                               │        │
│          │     }                                                               │        │
│          │ });                                                                 │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ STEP 6: Audit log                                                  │        │
│          │ ══════════════════                                                 │        │
│          │                                                                     │        │
│          │ _logger.LogInformation(                                             │        │
│          │     "AUTH-PWD-001: Password set for OAuth user. UserId={UserId}, " │        │
│          │     "Provider={Provider}, IP={IP}",                                 │        │
│          │     user.Id, user.ExternalAuthProvider, request.IpAddress);        │        │
│          │                                                                     │        │
│          │ await _mediator.Publish(new PasswordSetForOAuthUserEvent {         │        │
│          │     UserId = user.Id,                                               │        │
│          │     Provider = user.ExternalAuthProvider,                           │        │
│          │     SetAt = DateTime.UtcNow                                         │        │
│          │ });                                                                 │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│      Response (200 OK):                                                                  │
│      {                                                                                   │
│        "success": true,                                                                 │
│        "message": "Contraseña configurada exitosamente. Ahora puedes iniciar           │
│                    sesión con tu email y contraseña.",                                  │
│        "canNowUnlinkProvider": true,                                                    │
│        "redirectUrl": "/settings/security?passwordSet=true"                            │
│      }                                                                                   │
│                                                                                          │
│  PASO 9: Usuario regresa a Security Settings                                            │
│  ════════════════════════════════════════════                                           │
│  │                                                                                       │
│  └─▶ Ahora puede continuar con AUTH-EXT-008 (Unlink Active Provider)                   │
│      porque YA tiene contraseña configurada                                             │
│                                                                                          │
└──────────────────────────────────────────────────────────────────────────────────────────┘
```

### 2.3 Endpoints de AUTH-PWD-001

| Método | Endpoint                            | Descripción                         | Auth |
| ------ | ----------------------------------- | ----------------------------------- | ---- |
| `POST` | `/api/auth/password/setup-request`  | Solicitar email para configurar pwd | ✅   |
| `POST` | `/api/auth/password/setup-validate` | Validar token del email             | ❌   |
| `POST` | `/api/auth/password/setup-complete` | Configurar la contraseña            | ❌   |

### 2.4 Almacenamiento en Redis

| Clave                            | Contenido                                      | TTL    |
| -------------------------------- | ---------------------------------------------- | ------ |
| `password_setup_token:{userId}`  | TokenHash, ExpiresAt, UserId, Email, IpAddress | 1 hora |
| `password_setup_lockout:{email}` | Lockout por demasiadas solicitudes             | 1 hora |

### 2.5 Validaciones de Contraseña

| Requisito                    | Regex/Validación                               |
| ---------------------------- | ---------------------------------------------- | ---- |
| Mínimo 8 caracteres          | `password.Length >= 8`                         |
| Al menos 1 mayúscula         | `[A-Z]`                                        |
| Al menos 1 minúscula         | `[a-z]`                                        |
| Al menos 1 número            | `[0-9]`                                        |
| Al menos 1 carácter especial | `[!@#$%^&\*(),.?":{}                           | <>]` |
| No contiene el email         | `!password.Contains(email.Split('@')[0])`      |
| No es contraseña común       | Check contra lista de 10,000 passwords comunes |

### 2.6 Rate Limiting

| Acción                    | Límite     | Lockout          |
| ------------------------- | ---------- | ---------------- |
| Solicitar email           | 3 por hora | 1 hora           |
| Validar token inválido    | 5 intentos | 30 minutos       |
| Configurar con token malo | 3 intentos | Token invalidado |

---

## 3. AUTH-EXT-008: Unlink Active OAuth Provider

### 3.1 Diagrama de Flujo Completo

```
┌──────────────────────────────────────────────────────────────────────────────────────────┐
│                              AUTH-EXT-008: UNLINK ACTIVE PROVIDER                        │
├──────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                          │
│  PREREQUISITO: Usuario YA tiene contraseña (completó AUTH-PWD-001 si era necesario)     │
│  ════════════════════════════════════════════════════════════════════════════════       │
│                                                                                          │
│  PASO 1: Usuario en SecuritySettingsPage                                                 │
│  ═══════════════════════════════════════                                                 │
│  │                                                                                       │
│  └─▶ Ve sección "Linked Accounts"                                                       │
│      └─▶ [Google] juan@gmail.com • Connected ✓              [Unlink]                    │
│                                                                                          │
│  PASO 2: Click en "Unlink" para Google                                                   │
│  ═════════════════════════════════════                                                   │
│  │                                                                                       │
│  └─▶ Frontend llama: POST /api/ExternalAuth/unlink-account/validate                    │
│      │                                                                                   │
│      │  Request Body:                                                                    │
│      │  {                                                                                │
│      │    "provider": "Google"                                                          │
│      │  }                                                                                │
│      │                                                                                   │
│      └─▶ Backend ejecuta validaciones:                                                  │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ VALIDACIÓN 1: ¿Es el proveedor activo (con el que se logueó)?      │        │
│          │ ═══════════════════════════════════════════════════════════════     │        │
│          │                                                                     │        │
│          │ Leer claim del JWT: "external_provider" = "Google"                  │        │
│          │ Comparar con request.Provider                                       │        │
│          │                                                                     │        │
│          │ IF (jwt.external_provider == request.Provider)                      │        │
│          │    isActiveProvider = TRUE → Requiere flujo extendido              │        │
│          │ ELSE                                                                │        │
│          │    isActiveProvider = FALSE → Flujo normal (AUTH-EXT-006)           │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ VALIDACIÓN 2: ¿Tiene contraseña configurada?                       │        │
│          │ ═══════════════════════════════════════════════════════════════     │        │
│          │                                                                     │        │
│          │ var user = await _userRepository.GetByIdAsync(userId);             │        │
│          │                                                                     │        │
│          │ IF (string.IsNullOrEmpty(user.PasswordHash))                        │        │
│          │    hasPassword = FALSE                                              │        │
│          │    → BLOQUEAR: "Debes configurar una contraseña primero"           │        │
│          │ ELSE                                                                │        │
│          │    hasPassword = TRUE → Puede continuar                            │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ VALIDACIÓN 3: ¿Tiene 2FA habilitado?                               │        │
│          │ ═══════════════════════════════════════════════════════════════     │        │
│          │                                                                     │        │
│          │ IF (user.TwoFactorEnabled)                                          │        │
│          │    requires2FA = TRUE → Agregar verificación 2FA al flujo          │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│      Response (200 OK):                                                                  │
│      {                                                                                   │
│        "canUnlink": true,                                                               │
│        "isActiveProvider": true,                                                        │
│        "hasPassword": true,                                                             │
│        "requires2FA": false,                                                            │
│        "requiresEmailVerification": true,                                               │
│        "warnings": [                                                                    │
│          "You are currently signed in with this account",                               │
│          "All your sessions will be terminated after unlinking",                        │
│          "You will need to sign in again with your email and password"                  │
│        ]                                                                                │
│      }                                                                                   │
│                                                                                          │
│      Response (400 - No Password):                                                       │
│      {                                                                                   │
│        "canUnlink": false,                                                              │
│        "isActiveProvider": true,                                                        │
│        "hasPassword": false,                                                            │
│        "blockReason": "PASSWORD_REQUIRED",                                              │
│        "message": "You must set a password before unlinking your Google account...",   │
│        "actionRequired": {                                                              │
│          "type": "SET_PASSWORD",                                                        │
│          "redirectUrl": "/settings/security?action=set-password"                        │
│        }                                                                                │
│      }                                                                                   │
│                                                                                          │
│  PASO 3: Frontend muestra modal de confirmación extendido                               │
│  ════════════════════════════════════════════════════════                               │
│  │                                                                                       │
│  └─▶ Modal muestra:                                                                     │
│      ┌─────────────────────────────────────────────────────────────────────┐            │
│      │ ⚠️ DESVINCULACIÓN DE CUENTA ACTIVA                                 │            │
│      │─────────────────────────────────────────────────────────────────────│            │
│      │                                                                     │            │
│      │ Estás a punto de desvincular la cuenta de Google con la que        │            │
│      │ iniciaste sesión actualmente.                                      │            │
│      │                                                                     │            │
│      │ 🔴 ADVERTENCIAS IMPORTANTES:                                        │            │
│      │ • Todas tus sesiones serán cerradas inmediatamente                 │            │
│      │ • Deberás iniciar sesión con tu email y contraseña                 │            │
│      │ • Ya no podrás usar "Iniciar con Google"                           │            │
│      │                                                                     │            │
│      │ Para continuar, enviaremos un código de verificación a:            │            │
│      │ 📧 j***n@gmail.com                                                 │            │
│      │                                                                     │            │
│      │        [Cancelar]    [Enviar Código de Verificación]               │            │
│      └─────────────────────────────────────────────────────────────────────┘            │
│                                                                                          │
│  PASO 4: Usuario confirma → Enviar código de verificación                               │
│  ════════════════════════════════════════════════════════                               │
│  │                                                                                       │
│  └─▶ Frontend llama: POST /api/ExternalAuth/unlink-account/request-code                │
│      │                                                                                   │
│      │  Request Body:                                                                    │
│      │  {                                                                                │
│      │    "provider": "Google"                                                          │
│      │  }                                                                                │
│      │                                                                                   │
│      └─▶ Backend:                                                                       │
│          1. Genera código de 6 dígitos                                                  │
│          2. Hash SHA256 del código                                                      │
│          3. Almacena en Redis:                                                          │
│             Key: "unlink_active_provider_code:{userId}:{provider}"                      │
│             Value: { codeHash, expiresAt, remainingAttempts: 3 }                        │
│             TTL: 10 minutos                                                             │
│          4. Envía email con código                                                      │
│          5. Log de auditoría                                                            │
│                                                                                          │
│      Response (200 OK):                                                                  │
│      {                                                                                   │
│        "success": true,                                                                 │
│        "message": "Verification code sent to j***n@gmail.com",                          │
│        "codeExpiresAt": "2026-01-24T16:10:00Z",                                        │
│        "maskedEmail": "j***n@gmail.com"                                                │
│      }                                                                                   │
│                                                                                          │
│  PASO 5: Usuario ingresa código                                                         │
│  ══════════════════════════════                                                         │
│  │                                                                                       │
│  └─▶ Modal actualiza para mostrar input de código:                                      │
│      ┌─────────────────────────────────────────────────────────────────────┐            │
│      │ 🔐 VERIFICACIÓN REQUERIDA                                          │            │
│      │─────────────────────────────────────────────────────────────────────│            │
│      │                                                                     │            │
│      │ Ingresa el código de 6 dígitos enviado a j***n@gmail.com           │            │
│      │                                                                     │            │
│      │         ┌───┬───┬───┬───┬───┬───┐                                  │            │
│      │         │ 1 │ 2 │ 3 │ 4 │ 5 │ 6 │                                  │            │
│      │         └───┴───┴───┴───┴───┴───┘                                  │            │
│      │                                                                     │            │
│      │ El código expira en 9:45                                           │            │
│      │                                                                     │            │
│      │ ¿No recibiste el código? [Reenviar]                                │            │
│      │                                                                     │            │
│      │         [Cancelar]    [Confirmar Desvinculación]                   │            │
│      └─────────────────────────────────────────────────────────────────────┘            │
│                                                                                          │
│  PASO 6: Verificar código y ejecutar unlink                                             │
│  ══════════════════════════════════════════                                             │
│  │                                                                                       │
│  └─▶ Frontend llama: DELETE /api/ExternalAuth/unlink-active-provider                   │
│      │                                                                                   │
│      │  Request Body:                                                                    │
│      │  {                                                                                │
│      │    "provider": "Google",                                                         │
│      │    "verificationCode": "123456"                                                  │
│      │  }                                                                                │
│      │                                                                                   │
│      └─▶ Backend Handler (UnlinkActiveProviderCommandHandler):                          │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ STEP 1: Validar código                                             │        │
│          │ ═══════════════════════                                            │        │
│          │                                                                     │        │
│          │ var codeData = await _cache.GetAsync<UnlinkCodeData>(              │        │
│          │     $"unlink_active_provider_code:{userId}:{provider}");           │        │
│          │                                                                     │        │
│          │ IF (codeData == null)                                               │        │
│          │    throw BadRequest("Verification code expired or not found")      │        │
│          │                                                                     │        │
│          │ var requestCodeHash = SHA256(request.Code);                         │        │
│          │                                                                     │        │
│          │ IF (requestCodeHash != codeData.CodeHash)                           │        │
│          │    codeData.RemainingAttempts--;                                    │        │
│          │    IF (codeData.RemainingAttempts <= 0)                             │        │
│          │       await _cache.RemoveAsync(key);                                │        │
│          │       await SetLockout(userId, provider, 30 minutes);               │        │
│          │       throw BadRequest("Too many failed attempts. Try again later")│        │
│          │    ELSE                                                             │        │
│          │       await _cache.SetAsync(key, codeData); // Update attempts     │        │
│          │       throw BadRequest("Invalid code. X attempts remaining")       │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ STEP 2: Re-verificar condiciones de seguridad                      │        │
│          │ ════════════════════════════════════════════                       │        │
│          │                                                                     │        │
│          │ // Re-validar que aún tiene password (pudo cambiar entre steps)    │        │
│          │ var user = await _userRepository.GetByIdAsync(userId);             │        │
│          │                                                                     │        │
│          │ IF (string.IsNullOrEmpty(user.PasswordHash))                        │        │
│          │    throw BadRequest("Password no longer set. Cannot unlink.")      │        │
│          │                                                                     │        │
│          │ // Verificar que aún es el proveedor activo                        │        │
│          │ IF (user.ExternalAuthProvider?.ToString() != provider)              │        │
│          │    throw BadRequest("Provider already unlinked or changed")        │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ STEP 3: Ejecutar unlink                                            │        │
│          │ ════════════════════════                                           │        │
│          │                                                                     │        │
│          │ // Guardar datos antes de unlink para audit                        │        │
│          │ var unlinkData = new {                                              │        │
│          │     Provider = user.ExternalAuthProvider,                           │        │
│          │     ExternalUserId = user.ExternalUserId,                           │        │
│          │     ExternalEmail = user.ExternalEmail                              │        │
│          │ };                                                                  │        │
│          │                                                                     │        │
│          │ // Desvincular proveedor                                            │        │
│          │ user.UnlinkExternalAccount();                                       │        │
│          │ await _userRepository.UpdateAsync(user);                            │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ STEP 4: Revocar TODAS las sesiones                                 │        │
│          │ ══════════════════════════════════                                 │        │
│          │                                                                     │        │
│          │ // Obtener todas las sesiones activas del usuario                  │        │
│          │ var sessions = await _sessionRepository.GetActiveSessionsAsync(    │        │
│          │     userId, includeCurrentSession: true);                           │        │
│          │                                                                     │        │
│          │ foreach (var session in sessions)                                   │        │
│          │ {                                                                   │        │
│          │     session.Revoke("Unlink active provider - security logout");    │        │
│          │     await _sessionRepository.UpdateAsync(session);                  │        │
│          │                                                                     │        │
│          │     // Revocar refresh token asociado                               │        │
│          │     await _refreshTokenRepository.RevokeBySessionIdAsync(          │        │
│          │         session.Id);                                                │        │
│          │ }                                                                   │        │
│          │                                                                     │        │
│          │ // Marcar todos los tokens como inválidos en Redis                  │        │
│          │ await _cache.SetAsync(                                              │        │
│          │     $"user_tokens_invalidated:{userId}",                            │        │
│          │     DateTime.UtcNow,                                                │        │
│          │     TimeSpan.FromDays(7));                                          │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ STEP 5: Enviar notificaciones                                      │        │
│          │ ══════════════════════════════                                     │        │
│          │                                                                     │        │
│          │ // Email de seguridad al usuario                                   │        │
│          │ await _notificationService.SendSecurityAlertAsync(new {            │        │
│          │     UserId = userId,                                                │        │
│          │     Type = "EXTERNAL_ACCOUNT_UNLINKED",                             │        │
│          │     Title = "Cuenta externa desvinculada",                          │        │
│          │     Message = $"Tu cuenta de {provider} ha sido desvinculada...",  │        │
│          │     IpAddress = request.IpAddress,                                  │        │
│          │     UserAgent = request.UserAgent,                                  │        │
│          │     Timestamp = DateTime.UtcNow                                     │        │
│          │ });                                                                 │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ STEP 6: Audit log                                                  │        │
│          │ ══════════════════                                                 │        │
│          │                                                                     │        │
│          │ _logger.LogWarning(                                                 │        │
│          │     "AUTH-EXT-008: Active provider unlinked. " +                    │        │
│          │     "UserId={UserId}, Provider={Provider}, " +                      │        │
│          │     "ExternalUserId={ExtId}, SessionsRevoked={Count}, " +           │        │
│          │     "IP={IP}",                                                      │        │
│          │     userId, provider, unlinkData.ExternalUserId,                    │        │
│          │     sessions.Count, request.IpAddress);                             │        │
│          │                                                                     │        │
│          │ // Publicar evento de dominio                                       │        │
│          │ await _mediator.Publish(new ActiveProviderUnlinkedEvent {           │        │
│          │     UserId = userId,                                                │        │
│          │     Provider = provider,                                            │        │
│          │     UnlinkedAt = DateTime.UtcNow,                                   │        │
│          │     SessionsRevoked = sessions.Count                                │        │
│          │ });                                                                 │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│          ┌─────────────────────────────────────────────────────────────────────┐        │
│          │ STEP 7: Limpiar código de verificación                             │        │
│          │ ═════════════════════════════════════                              │        │
│          │                                                                     │        │
│          │ await _cache.RemoveAsync(                                           │        │
│          │     $"unlink_active_provider_code:{userId}:{provider}");           │        │
│          └─────────────────────────────────────────────────────────────────────┘        │
│                                                                                          │
│      Response (200 OK):                                                                  │
│      {                                                                                   │
│        "success": true,                                                                 │
│        "message": "Google account unlinked successfully",                               │
│        "provider": "Google",                                                            │
│        "sessionsRevoked": 3,                                                            │
│        "unlinkedAt": "2026-01-24T16:05:30Z",                                           │
│        "requiresRelogin": true,                                                         │
│        "redirectUrl": "/login?reason=provider-unlinked"                                 │
│      }                                                                                   │
│                                                                                          │
│  PASO 7: Frontend maneja respuesta                                                       │
│  ══════════════════════════════════                                                      │
│  │                                                                                       │
│  └─▶ Frontend:                                                                          │
│      1. Muestra toast de éxito                                                          │
│      2. Limpia localStorage (tokens)                                                    │
│      3. Redirige a /login con mensaje                                                   │
│                                                                                          │
│      ┌─────────────────────────────────────────────────────────────────────┐            │
│      │ ✅ CUENTA DESVINCULADA                                             │            │
│      │─────────────────────────────────────────────────────────────────────│            │
│      │                                                                     │            │
│      │ Tu cuenta de Google ha sido desvinculada exitosamente.             │            │
│      │                                                                     │            │
│      │ Por seguridad, todas tus sesiones han sido cerradas.               │            │
│      │ Ahora puedes iniciar sesión con tu email y contraseña.             │            │
│      │                                                                     │            │
│      │              [Iniciar Sesión]                                       │            │
│      └─────────────────────────────────────────────────────────────────────┘            │
│                                                                                          │
└──────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Endpoints API

### 3.1 Nuevo Endpoint: Validar Unlink

| Campo             | Valor                                            |
| ----------------- | ------------------------------------------------ |
| **ID Proceso**    | AUTH-EXT-008-A                                   |
| **Nombre**        | Validar Desvinculación de Proveedor              |
| **Descripción**   | Verifica condiciones antes de permitir unlink    |
| **Endpoint**      | `POST /api/ExternalAuth/unlink-account/validate` |
| **Auth Required** | ✅ Sí                                            |
| **Estado**        | 🔴 PENDIENTE                                     |

#### Request

```http
POST /api/ExternalAuth/unlink-account/validate
Authorization: Bearer {token}
Content-Type: application/json

{
  "provider": "Google"
}
```

#### Response: Puede desvincular (200 OK)

```json
{
  "canUnlink": true,
  "isActiveProvider": true,
  "hasPassword": true,
  "requires2FA": false,
  "requiresEmailVerification": true,
  "warnings": [
    "Estás intentando desvincular la cuenta con la que iniciaste sesión",
    "Todas tus sesiones serán cerradas inmediatamente",
    "Deberás iniciar sesión con tu email y contraseña"
  ],
  "userEmail": "j***n@gmail.com"
}
```

#### Response: No tiene contraseña (400 Bad Request)

```json
{
  "canUnlink": false,
  "isActiveProvider": true,
  "hasPassword": false,
  "blockReason": "PASSWORD_REQUIRED",
  "message": "Debes configurar una contraseña antes de desvincular tu cuenta de Google. Sin una contraseña, perderías acceso a tu cuenta.",
  "actionRequired": {
    "type": "SET_PASSWORD",
    "redirectUrl": "/settings/security?action=set-password"
  }
}
```

---

### 3.2 Nuevo Endpoint: Solicitar Código

| Campo             | Valor                                                |
| ----------------- | ---------------------------------------------------- |
| **ID Proceso**    | AUTH-EXT-008-B                                       |
| **Nombre**        | Solicitar Código de Verificación                     |
| **Descripción**   | Envía código de 6 dígitos por email                  |
| **Endpoint**      | `POST /api/ExternalAuth/unlink-account/request-code` |
| **Auth Required** | ✅ Sí                                                |
| **Estado**        | 🔴 PENDIENTE                                         |

#### Request

```http
POST /api/ExternalAuth/unlink-account/request-code
Authorization: Bearer {token}
Content-Type: application/json

{
  "provider": "Google"
}
```

#### Response (200 OK)

```json
{
  "success": true,
  "message": "Código de verificación enviado a j***n@gmail.com",
  "codeExpiresAt": "2026-01-24T16:10:00Z",
  "maskedEmail": "j***n@gmail.com"
}
```

#### Rate Limiting

| Parámetro            | Valor      |
| -------------------- | ---------- |
| Máximo solicitudes   | 3 por hora |
| Lockout si se supera | 1 hora     |
| Código expira        | 10 minutos |
| Intentos por código  | 3          |

---

### 3.3 Nuevo Endpoint: Ejecutar Desvinculación

| Campo             | Valor                                             |
| ----------------- | ------------------------------------------------- |
| **ID Proceso**    | AUTH-EXT-008-C                                    |
| **Nombre**        | Desvincular Proveedor Activo con Verificación     |
| **Descripción**   | Desvincula proveedor después de verificar código  |
| **Endpoint**      | `DELETE /api/ExternalAuth/unlink-active-provider` |
| **Auth Required** | ✅ Sí                                             |
| **Estado**        | 🔴 PENDIENTE                                      |

#### Request

```http
DELETE /api/ExternalAuth/unlink-active-provider
Authorization: Bearer {token}
Content-Type: application/json

{
  "provider": "Google",
  "verificationCode": "123456"
}
```

#### Response (200 OK)

```json
{
  "success": true,
  "message": "Cuenta de Google desvinculada exitosamente",
  "provider": "Google",
  "sessionsRevoked": 3,
  "unlinkedAt": "2026-01-24T16:05:30Z",
  "requiresRelogin": true,
  "redirectUrl": "/login?reason=provider-unlinked"
}
```

---

## 4. Almacenamiento en Redis

### 4.1 Claves Utilizadas

| Prefijo                                              | Propósito                           | TTL    |
| ---------------------------------------------------- | ----------------------------------- | ------ |
| `unlink_active_provider_code:{userId}:{provider}`    | Código de verificación              | 10 min |
| `unlink_active_provider_lockout:{userId}:{provider}` | Lockout por intentos fallidos       | 1 hora |
| `unlink_active_provider_requests:{userId}`           | Rate limiting de solicitudes        | 1 hora |
| `user_tokens_invalidated:{userId}`                   | Timestamp de invalidación de tokens | 7 días |

### 4.2 Estructura de Datos

```csharp
public class UnlinkActiveProviderCodeData
{
    public string CodeHash { get; set; }        // SHA256 del código
    public DateTime ExpiresAt { get; set; }     // Expiración
    public int RemainingAttempts { get; set; }  // Intentos restantes (default: 3)
    public string Provider { get; set; }        // Proveedor a desvincular
    public string IpAddress { get; set; }       // IP que solicitó
    public string UserAgent { get; set; }       // UserAgent
    public DateTime CreatedAt { get; set; }     // Timestamp de creación
}
```

---

## 5. Notificaciones por Email

### 5.1 Código de Verificación (AUTH-EXT-008-B)

**Asunto:** `🔐 Código de verificación para desvincular cuenta - OKLA`

**Contenido:**

```
Hola {Nombre},

Recibimos una solicitud para desvincular tu cuenta de {Provider} de OKLA.

Tu código de verificación es: {CODE}

Este código expira en 10 minutos.

⚠️ ADVERTENCIA: Si completas esta acción:
• Todas tus sesiones serán cerradas inmediatamente
• Ya no podrás usar "{Provider}" para iniciar sesión
• Deberás iniciar sesión con tu email y contraseña

Si no solicitaste esto, ignora este email y considera cambiar tu contraseña.

Detalles de la solicitud:
• Fecha: {Timestamp}
• IP: {IpAddress}
• Dispositivo: {Device}

Equipo de Seguridad de OKLA
```

### 5.2 Confirmación de Desvinculación (AUTH-EXT-008-C)

**Asunto:** `⚠️ Cuenta de {Provider} desvinculada - OKLA`

**Contenido:**

```
Hola {Nombre},

Tu cuenta de {Provider} ha sido desvinculada de OKLA.

Detalles:
• Proveedor: {Provider}
• Email externo: {ExternalEmail}
• Fecha: {Timestamp}
• IP: {IpAddress}
• Sesiones cerradas: {SessionCount}

A partir de ahora, deberás iniciar sesión con:
• Email: {UserEmail}
• Contraseña: (tu contraseña configurada)

Si no realizaste esta acción, contacta a soporte inmediatamente:
📧 security@okla.com.do
📞 +1 809-XXX-XXXX

Equipo de Seguridad de OKLA
```

---

## 6. Eventos de Dominio

### 6.1 ActiveProviderUnlinkedEvent

```csharp
public record ActiveProviderUnlinkedEvent : INotification
{
    public string UserId { get; init; }
    public string Provider { get; init; }
    public string? ExternalUserId { get; init; }
    public string? ExternalEmail { get; init; }
    public DateTime UnlinkedAt { get; init; }
    public int SessionsRevoked { get; init; }
    public string IpAddress { get; init; }
    public string UserAgent { get; init; }
}
```

### 6.2 Handlers del Evento

| Handler                     | Acción                             |
| --------------------------- | ---------------------------------- |
| `SendSecurityAlertHandler`  | Envía email de confirmación        |
| `AuditLogHandler`           | Registra en audit log              |
| `AnalyticsHandler`          | Tracking para métricas             |
| `InvalidateAllCacheHandler` | Limpia cualquier cache del usuario |

---

## 7. Seguridad - Validaciones

### 7.1 Checklist de Validaciones

| #   | Validación                                           | Cuándo              | Acción si falla                               |
| --- | ---------------------------------------------------- | ------------------- | --------------------------------------------- |
| 1   | Usuario autenticado                                  | Todos los endpoints | 401 Unauthorized                              |
| 2   | Provider es válido (Google/Microsoft/Facebook/Apple) | Validate            | 400 Bad Request                               |
| 3   | Usuario tiene el provider vinculado                  | Validate            | 400 "No tienes este proveedor vinculado"      |
| 4   | Es el proveedor activo (logueó con él)               | Validate            | Info: usar flujo normal AUTH-EXT-006          |
| 5   | Usuario tiene password configurado                   | Validate            | 400 "Debes configurar contraseña primero"     |
| 6   | No está en lockout                                   | Request-code        | 429 "Demasiados intentos. Espera X minutos"   |
| 7   | No superó rate limit                                 | Request-code        | 429 "Máximo 3 solicitudes por hora"           |
| 8   | Código no expirado                                   | Unlink              | 400 "Código expirado"                         |
| 9   | Código correcto                                      | Unlink              | 400 "Código incorrecto. X intentos restantes" |
| 10  | Re-verificar password existe                         | Unlink              | 400 "Password ya no está configurado"         |
| 11  | Re-verificar provider sigue vinculado                | Unlink              | 400 "Provider ya fue desvinculado"            |

### 7.2 Protección contra ataques

| Ataque                           | Mitigación                                  |
| -------------------------------- | ------------------------------------------- |
| **Brute force código**           | 3 intentos máximo, lockout 1 hora           |
| **Rate limiting bypass**         | Rate limit por userId + IP                  |
| **Session hijacking**            | Verificación por email al usuario real      |
| **CSRF**                         | Validación de token CSRF en requests        |
| **Replay attack**                | Código single-use, eliminado después de uso |
| **Race condition**               | Transacciones atómicas en DB y Redis        |
| **Account takeover post-unlink** | Todas las sesiones revocadas inmediatamente |

---

## 8. Frontend - Cambios Requeridos

### 8.1 Nuevo Modal: UnlinkActiveProviderModal

```tsx
interface UnlinkActiveProviderModalProps {
  isOpen: boolean;
  provider: string;
  onClose: () => void;
  onSuccess: () => void;
}

// Estados del modal:
// 1. VALIDATING - Verificando condiciones
// 2. NEEDS_PASSWORD - Requiere configurar contraseña primero
// 3. CONFIRM_WARNINGS - Mostrar advertencias y pedir confirmación
// 4. CODE_SENT - Código enviado, esperando input
// 5. VERIFYING - Verificando código
// 6. SUCCESS - Desvinculado exitosamente
// 7. ERROR - Error en el proceso
```

### 8.2 Modificaciones a SecuritySettingsPage.tsx

```tsx
// Detectar si es proveedor activo antes de mostrar modal
const handleUnlinkClick = async (provider: string) => {
  // Llamar a /unlink-account/validate
  const validation = await authService.validateUnlink(provider);

  if (validation.isActiveProvider) {
    // Usar modal especial para proveedor activo
    setShowActiveProviderModal(true);
    setActiveProviderToUnlink(provider);
  } else {
    // Usar flujo normal AUTH-EXT-006
    openUnlinkModal(provider);
  }
};
```

---

## 9. Testing

### 9.1 Casos de Prueba

| #   | Escenario                                         | Esperado                                         |
| --- | ------------------------------------------------- | ------------------------------------------------ |
| 1   | Validate con password configurado                 | canUnlink: true, warnings mostradas              |
| 2   | Validate sin password configurado                 | canUnlink: false, blockReason: PASSWORD_REQUIRED |
| 3   | Request-code éxito                                | Email enviado, código en Redis                   |
| 4   | Request-code en lockout                           | 429, mensaje de espera                           |
| 5   | Unlink con código correcto                        | Provider desvinculado, sesiones revocadas        |
| 6   | Unlink con código incorrecto                      | 400, intentos decrementados                      |
| 7   | Unlink con código expirado                        | 400, "Código expirado"                           |
| 8   | Unlink después de lockout por intentos            | 429, "Demasiados intentos"                       |
| 9   | Unlink cuando password fue removido durante flujo | 400, "Password ya no configurado"                |
| 10  | Validar que todas las sesiones fueron revocadas   | Ningún token funciona post-unlink                |

### 9.2 Comandos de Prueba

```bash
# Validar unlink
curl -X POST "http://localhost:18443/api/ExternalAuth/unlink-account/validate" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"provider": "Google"}'

# Solicitar código
curl -X POST "http://localhost:18443/api/ExternalAuth/unlink-account/request-code" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"provider": "Google"}'

# Ejecutar unlink
curl -X DELETE "http://localhost:18443/api/ExternalAuth/unlink-active-provider" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"provider": "Google", "verificationCode": "123456"}'
```

---

## 10. Checklist de Implementación

### Backend - AUTH-PWD-001 (Set Password for OAuth User)

- [ ] `RequestPasswordSetupCommand` y Handler
- [ ] `ValidatePasswordSetupTokenCommand` y Handler
- [ ] `SetPasswordForOAuthUserCommand` y Handler
- [ ] `PasswordSetupService` (lógica de Redis)
- [ ] Nuevo `PasswordController` o agregar a `AuthController`
- [ ] Event `PasswordSetForOAuthUserEvent`
- [ ] Email template: `PASSWORD_SETUP_OAUTH_USER`
- [ ] Email template: `PASSWORD_SET_CONFIRMATION`
- [ ] Validadores de contraseña (FluentValidation)
- [ ] Unit tests (mínimo 6)

### Backend - AUTH-EXT-008 (Unlink Active Provider)

- [ ] `ValidateUnlinkAccountCommand` y Handler
- [ ] `RequestUnlinkCodeCommand` y Handler
- [ ] `UnlinkActiveProviderCommand` y Handler
- [ ] `UnlinkActiveProviderService` (lógica de Redis)
- [ ] Actualizar `ExternalAuthController` con nuevos endpoints
- [ ] Event `ActiveProviderUnlinkedEvent`
- [ ] Email template: `UNLINK_VERIFICATION_CODE`
- [ ] Email template: `PROVIDER_UNLINKED_CONFIRMATION`
- [ ] Integración con Redis (IDistributedCache)
- [ ] Unit tests (mínimo 8)
- [ ] Integration tests

### Frontend

- [ ] `SetPasswordPage.tsx` - Nueva página para configurar contraseña
- [ ] `SetPasswordModal.tsx` - Modal cuando no tiene password
- [ ] `UnlinkActiveProviderModal.tsx` - Modal con verificación por código
- [ ] `requestPasswordSetup()` en authService.ts
- [ ] `validatePasswordSetupToken()` en authService.ts
- [ ] `completePasswordSetup()` en authService.ts
- [ ] `validateUnlink()` en authService.ts
- [ ] `requestUnlinkCode()` en authService.ts
- [ ] `unlinkActiveProvider()` en authService.ts
- [ ] Actualizar `SecuritySettingsPage.tsx`
- [ ] Password strength indicator component
- [ ] Countdown timer para expiración de código
- [ ] Manejo de redirección post-unlink

### Gateway

- [ ] Agregar rutas de `/api/auth/password/*` en `ocelot.dev.json`
- [ ] Agregar rutas de `/api/ExternalAuth/unlink-*` en `ocelot.dev.json`
- [ ] Agregar rutas en `ocelot.prod.json`

### Testing

- [ ] Tests unitarios AUTH-PWD-001
- [ ] Tests unitarios AUTH-EXT-008
- [ ] Tests de integración
- [ ] Test E2E del flujo completo

---

## 11. Flujo Completo Unificado

```
┌──────────────────────────────────────────────────────────────────────────────────────────┐
│                    FLUJO COMPLETO: UNLINK OAUTH CON CONFIGURACIÓN DE PASSWORD            │
├──────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                          │
│                          ┌─────────────────────────┐                                    │
│                          │ Usuario click "Unlink"  │                                    │
│                          │ en proveedor OAuth      │                                    │
│                          └───────────┬─────────────┘                                    │
│                                      │                                                   │
│                                      ▼                                                   │
│                     ┌───────────────────────────────────┐                               │
│                     │ POST /unlink-account/validate     │                               │
│                     │ ¿Tiene contraseña configurada?    │                               │
│                     └───────────────┬───────────────────┘                               │
│                                     │                                                    │
│                    ┌────────────────┴────────────────┐                                  │
│                    │                                 │                                   │
│               ❌ NO                              ✅ SÍ                                  │
│                    │                                 │                                   │
│                    ▼                                 │                                   │
│   ┌─────────────────────────────────┐               │                                   │
│   │   AUTH-PWD-001                  │               │                                   │
│   │   Configurar Password           │               │                                   │
│   │                                 │               │                                   │
│   │   1. Modal: "Necesitas pwd"     │               │                                   │
│   │   2. Click "Enviar enlace"      │               │                                   │
│   │   3. Email con link seguro      │               │                                   │
│   │   4. SetPasswordPage            │               │                                   │
│   │   5. Configurar nueva pwd       │               │                                   │
│   │   6. Confirmación               │               │                                   │
│   └───────────────┬─────────────────┘               │                                   │
│                   │                                  │                                   │
│                   │ ✅ Password configurado         │                                   │
│                   │                                  │                                   │
│                   └──────────────┬───────────────────┘                                  │
│                                  │                                                       │
│                                  ▼                                                       │
│                ┌────────────────────────────────────────┐                               │
│                │   AUTH-EXT-008                         │                               │
│                │   Unlink Active Provider               │                               │
│                │                                        │                               │
│                │   1. Modal: advertencias de unlink     │                               │
│                │   2. Click "Enviar código"             │                               │
│                │   3. Email con código 6 dígitos        │                               │
│                │   4. Ingresar código                   │                               │
│                │   5. Verificar y ejecutar unlink       │                               │
│                │   6. Revocar TODAS las sesiones        │                               │
│                │   7. Redirigir a /login                │                               │
│                └────────────────────────────────────────┘                               │
│                                  │                                                       │
│                                  ▼                                                       │
│                      ┌─────────────────────┐                                            │
│                      │ ✅ COMPLETADO       │                                            │
│                      │ Provider desvinculado│                                           │
│                      │ Usuario debe re-login│                                           │
│                      │ con email + password │                                           │
│                      └─────────────────────┘                                            │
│                                                                                          │
└──────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 12. Comparación con AUTH-EXT-006

| Aspecto                    | AUTH-EXT-006 (Normal)         | AUTH-EXT-008 (Proveedor Activo)        |
| -------------------------- | ----------------------------- | -------------------------------------- |
| **Cuándo aplica**          | Unlink de proveedor NO activo | Unlink del proveedor con el que logueó |
| **Validación de password** | ✅ Requerida                  | ✅ Requerida + flujo para configurar   |
| **Verificación por email** | ❌ No                         | ✅ Código de 6 dígitos                 |
| **Revoca sesiones**        | ❌ No                         | ✅ TODAS las sesiones                  |
| **Fuerza re-login**        | ❌ No                         | ✅ Redirige a /login                   |
| **Rate limiting**          | ❌ No específico              | ✅ 3 req/hora, lockout 1 hora          |
| **Notificación email**     | ❌ No                         | ✅ Código + Confirmación               |
| **Audit level**            | INFO                          | WARNING                                |

---

## 13. Emails Templates

### 13.1 PASSWORD_SETUP_OAUTH_USER

```html
Asunto: 🔐 Configura tu contraseña - OKLA Hola {UserName}, Recibimos tu
solicitud para configurar una contraseña en tu cuenta de OKLA. Actualmente
inicias sesión con {Provider}. Al configurar una contraseña, podrás: • Iniciar
sesión con tu email y contraseña • Desvincular tu cuenta de {Provider} si lo
deseas • Tener un método de acceso de respaldo [CONFIGURAR CONTRASEÑA] ← Link
válido por 1 hora Si no solicitaste esto, puedes ignorar este correo. Detalles:
• Fecha: {Timestamp} • IP: {IpAddress}
```

### 13.2 PASSWORD_SET_CONFIRMATION

```html
Asunto: ✅ Contraseña configurada exitosamente - OKLA Hola {UserName}, Tu
contraseña ha sido configurada exitosamente. Ahora puedes: • Iniciar sesión con
{Email} y tu nueva contraseña • Desvincular tu cuenta de {Provider} desde
Configuración de Seguridad • Recuperar tu cuenta si pierdes acceso a {Provider}
Si no realizaste esta acción, contacta a soporte inmediatamente. Fecha:
{Timestamp} IP: {IpAddress}
```

### 13.3 UNLINK_VERIFICATION_CODE

```html
Asunto: 🔐 Código de verificación para desvincular cuenta - OKLA Hola
{UserName}, Tu código de verificación es: {CODE} Este código expira en 10
minutos. ⚠️ ADVERTENCIA: Si completas esta acción: • Todas tus sesiones serán
cerradas • Ya no podrás usar "{Provider}" para iniciar sesión • Deberás usar tu
email y contraseña Detalles: • Fecha: {Timestamp} • IP: {IpAddress}
```

### 13.4 PROVIDER_UNLINKED_CONFIRMATION

```html
Asunto: ⚠️ Cuenta de {Provider} desvinculada - OKLA Hola {UserName}, Tu cuenta
de {Provider} ({ExternalEmail}) ha sido desvinculada. • Sesiones cerradas:
{SessionCount} • Ahora debes usar: {Email} + contraseña Si no realizaste esta
acción, contacta a soporte. Fecha: {Timestamp} IP: {IpAddress}
```

---

**Última actualización:** Enero 24, 2026  
**Autor:** Equipo de Desarrollo OKLA  
**Versión:** 1.1.0 (Propuesta con AUTH-PWD-001)  
**Estado:** 🟡 PENDIENTE APROBACIÓN PARA IMPLEMENTACIÓN
