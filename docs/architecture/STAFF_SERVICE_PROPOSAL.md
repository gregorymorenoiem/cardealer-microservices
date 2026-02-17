# StaffService - Microservicio de Gestión de Empleados de Plataforma

## 📋 Descripción

StaffService es el microservicio responsable de gestionar todo el ciclo de vida de los empleados de la plataforma OKLA (admins, moderadores, soporte, analistas, compliance).

## 🎯 Responsabilidades

### Core Features

- **Gestión de Empleados**: CRUD de empleados de plataforma
- **Sistema de Invitaciones**: Flujo completo de invitación y onboarding
- **Roles y Permisos**: Gestión granular de permisos de staff
- **Departamentos**: Organización por departamentos
- **Actividad y Auditoría**: Tracking de acciones de empleados
- **Onboarding/Offboarding**: Flujos automatizados

### Integrations

- **AuthService**: Creación de cuentas y autenticación
- **NotificationService**: Envío de invitaciones y notificaciones
- **AuditService**: Registro de acciones
- **AdminService**: Datos para dashboard

## 📊 Dominio

### Entidades

```csharp
// Empleado de plataforma
public class StaffMember
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }              // Referencia a AuthService
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }

    // Rol y permisos
    public StaffRole Role { get; set; }
    public List<Permission> Permissions { get; set; }

    // Organización
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public Guid? SupervisorId { get; set; }
    public StaffMember? Supervisor { get; set; }

    // Estado
    public StaffStatus Status { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string? TerminationReason { get; set; }

    // Metadatos
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}

// Departamento
public class Department
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public Guid? ManagerId { get; set; }
    public StaffMember? Manager { get; set; }
    public List<StaffMember> Members { get; set; }
    public bool IsActive { get; set; }
}

// Invitación
public class StaffInvitation
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public StaffRole Role { get; set; }
    public List<string> Permissions { get; set; }
    public Guid? DepartmentId { get; set; }
    public string Token { get; set; }              // Token único para aceptar
    public InvitationStatus Status { get; set; }
    public DateTime InvitedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public Guid InvitedBy { get; set; }
    public string? Notes { get; set; }
}

// Permiso granular
public class Permission
{
    public Guid Id { get; set; }
    public string Code { get; set; }              // e.g., "vehicles.approve"
    public string Name { get; set; }              // e.g., "Aprobar vehículos"
    public string Category { get; set; }          // e.g., "Moderación"
    public string? Description { get; set; }
}

// Actividad del empleado
public class StaffActivity
{
    public Guid Id { get; set; }
    public Guid StaffMemberId { get; set; }
    public string Action { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### Enums

```csharp
public enum StaffRole
{
    SuperAdmin,      // Acceso total
    Admin,           // Gestión completa
    Moderator,       // Moderación de contenido
    Support,         // Atención al cliente
    Analyst,         // Solo lectura analytics
    Compliance,      // KYC y cumplimiento
    Finance,         // Facturación y pagos
    Marketing        // Campañas y contenido
}

public enum StaffStatus
{
    Pending,         // Invitación enviada
    Active,          // Activo
    Suspended,       // Suspendido temporalmente
    OnLeave,         // De licencia
    Terminated       // Terminado
}

public enum InvitationStatus
{
    Pending,
    Accepted,
    Expired,
    Cancelled,
    Revoked
}
```

## 🔌 API Endpoints

### Staff Members

```
GET    /api/staff                           # Listar empleados (con filtros)
GET    /api/staff/{id}                      # Obtener empleado
GET    /api/staff/me                        # Mi perfil de staff
PUT    /api/staff/{id}                      # Actualizar empleado
DELETE /api/staff/{id}                      # Desactivar empleado (soft delete)
POST   /api/staff/{id}/suspend              # Suspender empleado
POST   /api/staff/{id}/reactivate           # Reactivar empleado
POST   /api/staff/{id}/terminate            # Terminar empleado
GET    /api/staff/{id}/activity             # Actividad del empleado
```

### Invitations

```
POST   /api/staff/invitations               # Crear invitación
GET    /api/staff/invitations               # Listar invitaciones
GET    /api/staff/invitations/{id}          # Obtener invitación
DELETE /api/staff/invitations/{id}          # Cancelar invitación
POST   /api/staff/invitations/{id}/resend   # Reenviar invitación

# Públicos (sin auth - token provee seguridad)
GET    /api/staff/invitations/{token}/validate  # Validar token
POST   /api/staff/invitations/{token}/accept    # Aceptar invitación
```

### Departments

```
GET    /api/staff/departments               # Listar departamentos
POST   /api/staff/departments               # Crear departamento
GET    /api/staff/departments/{id}          # Obtener departamento
PUT    /api/staff/departments/{id}          # Actualizar departamento
DELETE /api/staff/departments/{id}          # Eliminar departamento
GET    /api/staff/departments/{id}/members  # Miembros del departamento
```

### Permissions

```
GET    /api/staff/permissions               # Listar permisos disponibles
GET    /api/staff/roles                     # Listar roles y sus permisos
GET    /api/staff/{id}/permissions          # Permisos de un empleado
PUT    /api/staff/{id}/permissions          # Actualizar permisos
```

### Security (SuperAdmin only)

```
GET    /api/staff/security/status           # Estado de seguridad
DELETE /api/staff/security/default-admin    # Eliminar admin por defecto
GET    /api/staff/security/audit            # Auditoría de seguridad
```

## 🔄 Eventos de Dominio (RabbitMQ)

### Eventos Publicados

```csharp
// Empleados
staff.member.invited       // Invitación creada
staff.member.accepted      // Invitación aceptada
staff.member.created       // Empleado creado
staff.member.updated       // Empleado actualizado
staff.member.suspended     // Empleado suspendido
staff.member.reactivated   // Empleado reactivado
staff.member.terminated    // Empleado terminado

// Permisos
staff.permissions.updated  // Permisos actualizados

// Seguridad
staff.security.default_admin_deleted  // Admin por defecto eliminado
staff.security.alert                  // Alerta de seguridad
```

### Eventos Consumidos

```csharp
// De AuthService
auth.user.password_changed   // Notificar cambio de password de staff
auth.user.login_failed       // Alertar intentos fallidos de staff
auth.user.2fa_enabled        // Staff habilitó 2FA

// De AuditService
audit.suspicious_activity    // Actividad sospechosa de staff
```

## 🏗️ Estructura del Proyecto

```
StaffService/
├── StaffService.Api/
│   ├── Controllers/
│   │   ├── StaffMembersController.cs
│   │   ├── InvitationsController.cs
│   │   ├── DepartmentsController.cs
│   │   ├── PermissionsController.cs
│   │   └── SecurityController.cs
│   ├── Middleware/
│   ├── Program.cs
│   └── Dockerfile
├── StaffService.Application/
│   ├── UseCases/
│   │   ├── Members/
│   │   │   ├── CreateStaffMember/
│   │   │   ├── UpdateStaffMember/
│   │   │   ├── SuspendStaffMember/
│   │   │   └── TerminateStaffMember/
│   │   ├── Invitations/
│   │   │   ├── CreateInvitation/
│   │   │   ├── AcceptInvitation/
│   │   │   └── CancelInvitation/
│   │   ├── Departments/
│   │   ├── Permissions/
│   │   └── Security/
│   ├── Interfaces/
│   │   ├── IAuthServiceClient.cs
│   │   ├── INotificationServiceClient.cs
│   │   └── IAuditServiceClient.cs
│   ├── Events/
│   └── Validators/
├── StaffService.Domain/
│   ├── Entities/
│   ├── Enums/
│   ├── Interfaces/
│   └── Events/
├── StaffService.Infrastructure/
│   ├── Persistence/
│   ├── External/
│   │   ├── AuthServiceClient.cs
│   │   └── NotificationServiceClient.cs
│   └── Messaging/
└── StaffService.Tests/
```

## 🔐 Seguridad

### Autenticación

- JWT Bearer tokens (validados contra AuthService)
- Refresh tokens para sesiones largas

### Autorización

- RBAC (Role-Based Access Control)
- Permisos granulares por acción
- Middleware de verificación de permisos

### Protecciones

- Rate limiting por usuario
- Audit logging de todas las acciones
- Detección de actividad sospechosa
- Bloqueo automático por intentos fallidos

## 📊 Métricas y Observabilidad

### Health Checks

```
GET /health          # Estado general
GET /health/ready    # Readiness probe
GET /health/live     # Liveness probe
```

### Métricas Prometheus

```
staff_members_total{status="active|suspended|terminated"}
staff_invitations_total{status="pending|accepted|expired"}
staff_actions_total{action="login|approve|reject|..."}
staff_api_requests_total{endpoint="/api/staff/..."}
staff_api_latency_seconds{endpoint="/api/staff/..."}
```

## 🔄 Sincronización con otros servicios

### Con AuthService

```
StaffService                          AuthService
    │                                      │
    │  POST /api/auth/admin/register       │
    │─────────────────────────────────────▶│
    │     {email, password, role}          │
    │                                      │
    │  {userId, accessToken}               │
    │◀─────────────────────────────────────│
    │                                      │
    │  GET /api/auth/admin/security-status │
    │─────────────────────────────────────▶│
    │                                      │
    │  {defaultAdminExists, realAdminCount}│
    │◀─────────────────────────────────────│
```

### Con NotificationService

```
StaffService                          NotificationService
    │                                      │
    │  POST /api/notifications/send        │
    │─────────────────────────────────────▶│
    │  {                                   │
    │    template: "staff-invitation",     │
    │    to: "nuevo@admin.com",            │
    │    data: {inviteLink, role, ...}     │
    │  }                                   │
    │                                      │
```

### Con AdminService

```
AdminService                          StaffService
    │                                      │
    │  GET /api/staff/stats                │
    │─────────────────────────────────────▶│
    │                                      │
    │  {totalStaff, activeToday, ...}      │
    │◀─────────────────────────────────────│
    │                                      │
```

## 🚀 Migración desde AdminService

### Fase 1: Crear StaffService (2-3 días)

1. Scaffold del proyecto
2. Entidades y migraciones
3. Endpoints básicos CRUD

### Fase 2: Implementar Lógica (3-4 días)

1. Sistema de invitaciones
2. Integración con AuthService
3. Permisos granulares

### Fase 3: Migrar Datos (1 día)

1. Script de migración de datos
2. Validación de integridad

### Fase 4: Actualizar Dependencias (1-2 días)

1. AdminService consume StaffService
2. Frontend actualiza endpoints
3. Gateway routes actualizadas

### Fase 5: Deprecar código antiguo (1 día)

1. Eliminar PlatformEmployees de AdminService
2. Actualizar documentación

## 📝 Notas de Implementación

### Base de Datos

- PostgreSQL dedicada: `staffservice`
- Separación completa de AdminService

### Puerto de Desarrollo

- API: 15XXX (asignar nuevo puerto)
- Expuesto en docker-compose

### Configuración

```json
// appsettings.json
{
  "Services": {
    "AuthService": "http://authservice:80",
    "NotificationService": "http://notificationservice:80",
    "AuditService": "http://auditservice:80"
  },
  "Jwt": {
    "Key": "...",
    "Issuer": "AuthService",
    "Audience": "OKLA"
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Exchange": "cardealer.events"
  }
}
```

## ✅ Beneficios de esta Arquitectura

| Aspecto             | Antes (AdminService)  | Después (StaffService)   |
| ------------------- | --------------------- | ------------------------ |
| **Responsabilidad** | Múltiples (viola SRP) | Única (gestión de staff) |
| **Escalabilidad**   | Escala todo junto     | Escala independiente     |
| **Mantenimiento**   | Código acoplado       | Código aislado           |
| **Testing**         | Tests complejos       | Tests focalizados        |
| **Deployment**      | Deploya todo          | Deploy independiente     |
| **Fallas**          | Falla todo            | Falla aislado            |
| **Equipo**          | Un equipo             | Equipo dedicado posible  |

---

**Autor:** GitHub Copilot  
**Fecha:** Febrero 2026  
**Estado:** Propuesta de Arquitectura
