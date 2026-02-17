# RoleService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** RoleService
- **Puerto en Kubernetes:** 8080
- **Puerto en Desarrollo:** 5003
- **Estado:** ✅ **EN PRODUCCIÓN**
- **Base de Datos:** PostgreSQL (`roleservice`)
- **Imagen Docker:** ghcr.io/gregorymorenoiem/cardealer-roleservice:latest

### Propósito
Servicio de gestión de roles y permisos (RBAC - Role-Based Access Control). Maneja roles predefinidos y personalizados, asignación de roles a usuarios, y verificación de permisos para control de acceso granular.

---

## 🏗️ ARQUITECTURA

```
RoleService/
├── RoleService.Api/
│   ├── Controllers/
│   │   ├── RolesController.cs
│   │   ├── PermissionsController.cs
│   │   └── UserRolesController.cs
│   └── Program.cs
├── RoleService.Application/
│   ├── Features/
│   │   ├── Commands/
│   │   │   ├── CreateRoleCommand.cs
│   │   │   ├── AssignRoleToUserCommand.cs
│   │   │   └── RevokeRoleFromUserCommand.cs
│   │   └── Queries/
│   │       ├── GetUserRolesQuery.cs
│   │       ├── CheckPermissionQuery.cs
│   │       └── GetRolePermissionsQuery.cs
│   └── DTOs/
├── RoleService.Domain/
│   ├── Entities/
│   │   ├── Role.cs
│   │   ├── Permission.cs
│   │   ├── RolePermission.cs
│   │   └── UserRole.cs
│   └── Enums/
│       └── PermissionType.cs
└── RoleService.Infrastructure/
```

---

## 📦 ENTIDADES

### Role
```csharp
public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; }              // "Admin", "Dealer", "User"
    public string DisplayName { get; set; }
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }        // No se puede eliminar
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public ICollection<RolePermission> RolePermissions { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }
}
```

### Permission
```csharp
public class Permission
{
    public Guid Id { get; set; }
    public string Name { get; set; }              // "vehicles.create", "users.delete"
    public string Resource { get; set; }          // "vehicles", "users", "roles"
    public string Action { get; set; }            // "create", "read", "update", "delete"
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public ICollection<RolePermission> RolePermissions { get; set; }
}
```

### UserRole
```csharp
public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; }
    public Guid? AssignedBy { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
    public Role Role { get; set; }
}
```

---

## 📡 ENDPOINTS API

### Roles

#### GET `/api/roles`
Listar todos los roles.

**Response:**
```json
{
  "items": [
    {
      "id": "...",
      "name": "Admin",
      "displayName": "Administrador",
      "description": "Acceso completo al sistema",
      "isSystemRole": true,
      "permissionsCount": 50
    }
  ]
}
```

#### POST `/api/roles`
Crear rol personalizado.

**Request:**
```json
{
  "name": "SalesManager",
  "displayName": "Gerente de Ventas",
  "description": "Gestión de inventario y ventas",
  "permissionIds": ["...", "..."]
}
```

#### GET `/api/roles/{id}/permissions`
Obtener permisos de un rol.

### Asignación de Roles

#### POST `/api/users/{userId}/roles`
Asignar rol a usuario.

**Request:**
```json
{
  "roleId": "...",
  "expiresAt": "2027-01-01T00:00:00Z"
}
```

#### GET `/api/users/{userId}/roles`
Obtener roles de un usuario.

**Response:**
```json
{
  "userId": "...",
  "roles": [
    {
      "roleId": "...",
      "roleName": "Dealer",
      "assignedAt": "2026-01-01T00:00:00Z",
      "expiresAt": null
    }
  ]
}
```

#### DELETE `/api/users/{userId}/roles/{roleId}`
Revocar rol de un usuario.

### Verificación de Permisos

#### GET `/api/users/{userId}/permissions/check`
Verificar si usuario tiene un permiso específico.

**Query Parameters:**
- `permission`: Nombre del permiso (ej: `vehicles.create`)

**Response:**
```json
{
  "userId": "...",
  "permission": "vehicles.create",
  "hasPermission": true
}
```

---

## 🔐 PERMISOS PREDEFINIDOS

### Categorías de Permisos

| Recurso | Acciones | Ejemplos |
|---------|----------|----------|
| **vehicles** | create, read, update, delete, publish | vehicles.create, vehicles.delete |
| **users** | create, read, update, delete, ban | users.update, users.ban |
| **roles** | create, read, update, delete, assign | roles.assign, roles.delete |
| **dealers** | create, read, update, delete, approve | dealers.approve |
| **reports** | view, export, create | reports.export |
| **billing** | view, process, refund | billing.refund |

### Roles del Sistema

#### Admin (Super Administrador)
- **Permisos:** ALL (*)
- **Descripción:** Acceso completo

#### Dealer (Concesionario)
- **Permisos:**
  - vehicles.* (sus propios vehículos)
  - dealers.read (su propio dealer)
  - reports.view (sus reportes)

#### User (Usuario Individual)
- **Permisos:**
  - vehicles.create
  - vehicles.read
  - vehicles.update (propios)
  - vehicles.delete (propios)

#### Guest (Invitado)
- **Permisos:**
  - vehicles.read (solo públicos)

---

## 🔄 EVENTOS

### Eventos Publicados

#### RoleAssignedEvent
```csharp
public record RoleAssignedEvent(
    Guid UserId,
    Guid RoleId,
    string RoleName,
    DateTime AssignedAt
);
```

**Exchange:** `role.events`  
**Routing Key:** `role.assigned`  
**Consumidores:**
- **AuditService**: Registro de cambios
- **NotificationService**: Notificar al usuario

#### RoleRevokedEvent
Cuando se revoca un rol a un usuario.

---

## 🔧 TECNOLOGÍAS

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="FluentValidation" Version="11.9.0" />
<PackageReference Include="RabbitMQ.Client" Version="6.8.1" />
```

---

## 📝 REGLAS DE NEGOCIO

1. **Roles del sistema no se pueden eliminar**
2. **Un usuario puede tener múltiples roles**
3. **Los permisos son heredados de todos los roles del usuario**
4. **Verificación en cache (Redis) con TTL de 5 minutos**
5. **Roles pueden tener fecha de expiración**

---

## 🔗 RELACIONES

### Consultado Por:
- **Todos los servicios**: Verificación de permisos
- **Gateway**: Autorización de rutas

### Publica Eventos A:
- **AuditService**: Cambios en roles
- **NotificationService**: Notificaciones

---

## 📅 ÚLTIMA ACTUALIZACIÓN

**Fecha:** Enero 7, 2026  
**Versión:** 1.0.0  
**Estado:** Producción en DOKS
