# 👥 Gestión de Usuarios Admin - Matriz de Procesos

> **Servicio:** AdminService  
> **Base de datos:** PostgreSQL (adminservice)  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 📊 Resumen de Implementación

| Componente   | Total | Implementado | Pendiente | Estado |
| ------------ | ----- | ------------ | --------- | ------ |
| Controllers  | 1     | 0            | 1         | 🔴     |
| ADM-USER-\*  | 5     | 0            | 5         | 🔴     |
| ADM-ROLE-\*  | 4     | 0            | 4         | 🔴     |
| ADM-PERM-\*  | 4     | 0            | 4         | 🔴     |
| ADM-AUDIT-\* | 3     | 0            | 3         | 🔴     |
| Tests        | 0     | 0            | 12        | 🔴     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## 1. Información General

### 1.1 Descripción

Sistema de gestión de usuarios administrativos que permite crear, editar, asignar roles y gestionar permisos de los usuarios que acceden al panel de administración de OKLA.

### 1.2 Roles Administrativos

| Rol            | Descripción             | Permisos                            |
| -------------- | ----------------------- | ----------------------------------- |
| **SuperAdmin** | Acceso total al sistema | Todos                               |
| **Admin**      | Administrador general   | CRUD usuarios, configuración        |
| **Moderator**  | Moderación de contenido | Aprobar/rechazar vehículos, dealers |
| **Support**    | Soporte al cliente      | Ver usuarios, atender tickets       |
| **Analyst**    | Análisis y reportes     | Solo lectura de analytics           |
| **Finance**    | Gestión financiera      | Ver transacciones, refunds          |

### 1.3 Dependencias

| Servicio            | Propósito               |
| ------------------- | ----------------------- |
| AuthService         | Autenticación y tokens  |
| RoleService         | Roles y permisos        |
| NotificationService | Notificaciones a admins |
| AuditService        | Log de acciones         |

---

## 2. Endpoints

### 2.1 Admin Users

| Método   | Endpoint                               | Descripción               | Rol Requerido     |
| -------- | -------------------------------------- | ------------------------- | ----------------- |
| `GET`    | `/api/admin/users`                     | Listar usuarios admin     | SuperAdmin, Admin |
| `GET`    | `/api/admin/users/{id}`                | Obtener usuario admin     | SuperAdmin, Admin |
| `POST`   | `/api/admin/users`                     | Crear usuario admin       | SuperAdmin        |
| `PUT`    | `/api/admin/users/{id}`                | Actualizar usuario admin  | SuperAdmin        |
| `DELETE` | `/api/admin/users/{id}`                | Desactivar usuario admin  | SuperAdmin        |
| `POST`   | `/api/admin/users/{id}/roles`          | Asignar roles             | SuperAdmin        |
| `DELETE` | `/api/admin/users/{id}/roles/{roleId}` | Quitar rol                | SuperAdmin        |
| `POST`   | `/api/admin/users/{id}/reset-password` | Reset password            | SuperAdmin        |
| `POST`   | `/api/admin/users/{id}/2fa/enable`     | Habilitar 2FA             | SuperAdmin, Self  |
| `POST`   | `/api/admin/users/{id}/2fa/disable`    | Deshabilitar 2FA          | SuperAdmin        |
| `GET`    | `/api/admin/users/{id}/sessions`       | Ver sesiones activas      | SuperAdmin, Self  |
| `DELETE` | `/api/admin/users/{id}/sessions`       | Cerrar todas las sesiones | SuperAdmin, Self  |

### 2.2 Activity Log

| Método | Endpoint                         | Descripción                  | Rol Requerido |
| ------ | -------------------------------- | ---------------------------- | ------------- |
| `GET`  | `/api/admin/users/{id}/activity` | Log de actividad del usuario | SuperAdmin    |
| `GET`  | `/api/admin/activity`            | Log de actividad global      | SuperAdmin    |

---

## 3. Entidades

### 3.1 AdminUser

```csharp
public class AdminUser
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } // FK a AuthService.Users
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }

    // Status
    public AdminUserStatus Status { get; set; } = AdminUserStatus.Active;
    public DateTime? DeactivatedAt { get; set; }
    public string? DeactivationReason { get; set; }

    // Security
    public bool IsTwoFactorEnabled { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndAt { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public ICollection<AdminUserRole> Roles { get; set; } = new List<AdminUserRole>();
    public ICollection<AdminSession> Sessions { get; set; } = new List<AdminSession>();
}

public enum AdminUserStatus
{
    Active,
    Inactive,
    Locked,
    PendingActivation
}
```

### 3.2 AdminUserRole

```csharp
public class AdminUserRole
{
    public Guid Id { get; set; }
    public Guid AdminUserId { get; set; }
    public Guid RoleId { get; set; }

    public DateTime AssignedAt { get; set; }
    public Guid AssignedBy { get; set; }
    public DateTime? ExpiresAt { get; set; } // Para roles temporales

    // Navigation
    public AdminUser AdminUser { get; set; } = null!;
}
```

### 3.3 AdminSession

```csharp
public class AdminSession
{
    public Guid Id { get; set; }
    public Guid AdminUserId { get; set; }
    public string SessionToken { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? Location { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    // Navigation
    public AdminUser AdminUser { get; set; } = null!;
}
```

### 3.4 AdminActivityLog

```csharp
public class AdminActivityLog
{
    public Guid Id { get; set; }
    public Guid AdminUserId { get; set; }
    public Guid? TargetUserId { get; set; }

    public AdminAction Action { get; set; }
    public string ResourceType { get; set; } = string.Empty; // User, Vehicle, Dealer, etc.
    public string? ResourceId { get; set; }

    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? Notes { get; set; }

    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public enum AdminAction
{
    Login,
    Logout,
    CreateUser,
    UpdateUser,
    DeleteUser,
    AssignRole,
    RemoveRole,
    ResetPassword,
    Enable2FA,
    Disable2FA,
    ApproveVehicle,
    RejectVehicle,
    ApproveDealer,
    RejectDealer,
    ProcessRefund,
    UpdateConfig,
    ViewSensitiveData
}
```

---

## 4. Procesos Detallados

### 4.1 ADM-001: Crear Usuario Administrativo

| Paso | Acción                                   | Sistema             | Validación           |
| ---- | ---------------------------------------- | ------------------- | -------------------- |
| 1    | SuperAdmin ingresa datos del nuevo admin | Frontend Admin      | Formulario válido    |
| 2    | Verificar que email no existe            | AdminService        | Email único          |
| 3    | Verificar permisos del creador           | RoleService         | Es SuperAdmin        |
| 4    | Crear usuario en AuthService             | AuthService         | Usuario creado       |
| 5    | Crear AdminUser                          | AdminService        | AdminUser creado     |
| 6    | Asignar roles iniciales                  | AdminService        | Roles válidos        |
| 7    | Generar password temporal                | AuthService         | Password seguro      |
| 8    | Enviar email de bienvenida               | NotificationService | Email enviado        |
| 9    | Registrar en audit log                   | AdminService        | Log creado           |
| 10   | Notificar a otros SuperAdmins            | NotificationService | Notificación enviada |

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Create Admin User Flow                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   SuperAdmin                  AdminService                AuthService   │
│       │                            │                           │        │
│       │  POST /admin/users         │                           │        │
│       │ ─────────────────────────▶ │                           │        │
│       │                            │                           │        │
│       │                            │   Verificar email único   │        │
│       │                            │ ◀─────────────────────────│        │
│       │                            │                           │        │
│       │                            │   Crear User              │        │
│       │                            │ ─────────────────────────▶│        │
│       │                            │                           │        │
│       │                            │   UserId                  │        │
│       │                            │ ◀─────────────────────────│        │
│       │                            │                           │        │
│       │                            │   Crear AdminUser         │        │
│       │                            │ ──────────┐               │        │
│       │                            │           │               │        │
│       │                            │ ◀─────────┘               │        │
│       │                            │                           │        │
│       │                            │   Asignar roles           │        │
│       │                            │ ──────────┐               │        │
│       │                            │           │               │        │
│       │                            │ ◀─────────┘               │        │
│       │                            │                           │        │
│       │   AdminUser creado         │                           │        │
│       │ ◀───────────────────────── │                           │        │
│       │                            │                           │        │
│       │                            │   Publish AdminUserCreated│        │
│       │                            │ ────────────────────▶ MQ  │        │
│       │                            │                           │        │
└─────────────────────────────────────────────────────────────────────────┘
```

### 4.2 ADM-002: Asignar Roles a Usuario

| Paso | Acción                        | Sistema             | Validación         |
| ---- | ----------------------------- | ------------------- | ------------------ |
| 1    | SuperAdmin selecciona usuario | Frontend Admin      | Usuario existe     |
| 2    | Selecciona roles a asignar    | Frontend Admin      | Roles válidos      |
| 3    | Verificar permisos            | AdminService        | Es SuperAdmin      |
| 4    | Verificar que roles existen   | RoleService         | Roles existen      |
| 5    | Verificar conflictos de roles | AdminService        | Sin conflictos     |
| 6    | Asignar roles                 | AdminService        | Roles asignados    |
| 7    | Invalidar tokens existentes   | AuthService         | Tokens invalidados |
| 8    | Registrar en audit log        | AdminService        | Log creado         |
| 9    | Notificar al usuario          | NotificationService | Email enviado      |

### 4.3 ADM-003: Habilitar 2FA

| Paso | Acción                                 | Sistema           | Validación          |
| ---- | -------------------------------------- | ----------------- | ------------------- |
| 1    | Admin inicia configuración 2FA         | Frontend Admin    | Usuario autenticado |
| 2    | Generar secret TOTP                    | AuthService       | Secret generado     |
| 3    | Generar QR code                        | AuthService       | QR generado         |
| 4    | Mostrar QR al usuario                  | Frontend Admin    | QR visible          |
| 5    | Usuario escanea con app                | Authenticator App | Código escaneado    |
| 6    | Usuario ingresa código de verificación | Frontend Admin    | Código válido       |
| 7    | Verificar código                       | AuthService       | Código correcto     |
| 8    | Activar 2FA                            | AdminService      | 2FA habilitado      |
| 9    | Generar códigos de respaldo            | AuthService       | Códigos generados   |
| 10   | Mostrar códigos de respaldo            | Frontend Admin    | Códigos mostrados   |

### 4.4 ADM-004: Gestión de Sesiones

| Paso | Acción                           | Sistema        | Validación          |
| ---- | -------------------------------- | -------------- | ------------------- |
| 1    | Admin ve sus sesiones activas    | Frontend Admin | Usuario autenticado |
| 2    | Obtener sesiones del usuario     | AdminService   | Sesiones obtenidas  |
| 3    | Mostrar sesiones con detalles    | Frontend Admin | Lista visible       |
| 4    | Admin selecciona sesión a cerrar | Frontend Admin | Sesión válida       |
| 5    | Revocar sesión                   | AdminService   | Sesión revocada     |
| 6    | Invalidar token                  | AuthService    | Token invalidado    |
| 7    | Registrar en audit log           | AdminService   | Log creado          |

---

## 5. Reglas de Negocio

| Código  | Regla                                                | Validación                     |
| ------- | ---------------------------------------------------- | ------------------------------ |
| ADM-R01 | Solo SuperAdmin puede crear otros admins             | Role == SuperAdmin             |
| ADM-R02 | No se puede eliminar el último SuperAdmin            | Count(SuperAdmin) > 1          |
| ADM-R03 | 2FA obligatorio para SuperAdmin                      | IsTwoFactorEnabled == true     |
| ADM-R04 | Sesiones expiran después de 8 horas de inactividad   | LastActivityAt + 8h < Now      |
| ADM-R05 | Bloqueo después de 5 intentos fallidos               | FailedLoginAttempts >= 5       |
| ADM-R06 | Bloqueo dura 30 minutos                              | LockoutEndAt = Now + 30min     |
| ADM-R07 | Password debe cambiarse cada 90 días                 | LastPasswordChange + 90d < Now |
| ADM-R08 | No puede tener roles conflictivos                    | Ej: Support + Finance          |
| ADM-R09 | Audit log es inmutable                               | No DELETE en audit             |
| ADM-R10 | Admin no puede modificarse a sí mismo ciertos campos | Role, Status                   |

---

## 6. Códigos de Error

| Código    | HTTP | Mensaje                       | Causa               |
| --------- | ---- | ----------------------------- | ------------------- |
| `ADM_001` | 404  | Admin user not found          | Usuario no existe   |
| `ADM_002` | 409  | Email already exists          | Email duplicado     |
| `ADM_003` | 403  | Insufficient permissions      | Sin permisos        |
| `ADM_004` | 400  | Cannot delete last SuperAdmin | Único SuperAdmin    |
| `ADM_005` | 400  | Role conflict detected        | Roles incompatibles |
| `ADM_006` | 400  | 2FA required for SuperAdmin   | 2FA obligatorio     |
| `ADM_007` | 423  | Account is locked             | Cuenta bloqueada    |
| `ADM_008` | 400  | Invalid 2FA code              | Código incorrecto   |
| `ADM_009` | 400  | Session already revoked       | Sesión ya revocada  |
| `ADM_010` | 400  | Cannot modify own role        | Auto-modificación   |

---

## 7. Eventos RabbitMQ

| Evento                      | Exchange       | Descripción        |
| --------------------------- | -------------- | ------------------ |
| `AdminUserCreatedEvent`     | `admin.events` | Nuevo admin creado |
| `AdminUserUpdatedEvent`     | `admin.events` | Admin actualizado  |
| `AdminUserDeactivatedEvent` | `admin.events` | Admin desactivado  |
| `AdminRoleAssignedEvent`    | `admin.events` | Rol asignado       |
| `AdminRoleRemovedEvent`     | `admin.events` | Rol removido       |
| `Admin2FAEnabledEvent`      | `admin.events` | 2FA habilitado     |
| `AdminSessionRevokedEvent`  | `admin.events` | Sesión revocada    |
| `AdminActionLoggedEvent`    | `admin.events` | Acción registrada  |

---

## 8. Configuración

```json
{
  "AdminService": {
    "Security": {
      "MaxFailedLoginAttempts": 5,
      "LockoutDurationMinutes": 30,
      "SessionTimeoutMinutes": 480,
      "PasswordExpirationDays": 90,
      "Require2FAForSuperAdmin": true
    },
    "AuditLog": {
      "RetentionDays": 365,
      "LogSensitiveDataAccess": true
    },
    "Notifications": {
      "NotifyOnNewAdmin": true,
      "NotifyOnRoleChange": true,
      "NotifyOnSecurityEvent": true
    }
  }
}
```

---

## 9. Métricas Prometheus

```
# Usuarios admin activos
admin_users_active_total{role="..."}

# Sesiones activas
admin_sessions_active_total

# Intentos de login
admin_login_attempts_total{status="success|failed"}

# Acciones de admin
admin_actions_total{action="...", admin="..."}

# Bloqueos de cuenta
admin_account_lockouts_total
```

---

## 📚 Referencias

- [AuthService](../../01-AUTENTICACION-USUARIOS/01-auth-service.md) - Autenticación
- [RoleService](../../01-AUTENTICACION-USUARIOS/03-role-service.md) - Roles
- [AuditService](../../08-COMPLIANCE-LEGAL-RD/03-audit-log.md) - Auditoría
