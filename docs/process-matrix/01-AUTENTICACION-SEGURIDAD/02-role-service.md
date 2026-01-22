# 🛡️ RoleService - Matriz de Procesos

> **Servicio:** RoleService  
> **Puerto:** 15107  
> **Base de Datos:** role_db  
> **Última actualización:** Enero 21, 2026

---

## 1. Información General

### 1.1 Descripción

El RoleService implementa el sistema RBAC (Role-Based Access Control) de OKLA. Gestiona roles, permisos y la asignación de estos a usuarios. Es el servicio central de autorización que determina qué acciones puede realizar cada usuario en la plataforma.

### 1.2 Dependencias

| Servicio     | Propósito                           |
| ------------ | ----------------------------------- |
| UserService  | Obtener roles asignados a usuarios  |
| Gateway      | Verificar permisos en cada request  |
| AuditService | Registrar cambios en roles/permisos |

### 1.3 Controllers

| Controller                | Archivo                      | Endpoints |
| ------------------------- | ---------------------------- | --------- |
| RolesController           | RolesController.cs           | 5         |
| PermissionsController     | PermissionsController.cs     | 2         |
| RolePermissionsController | RolePermissionsController.cs | 3         |

---

## 2. Endpoints API

### 2.1 RolesController

| Método | Endpoint          | Descripción              | Auth     | Rate Limit |
| ------ | ----------------- | ------------------------ | -------- | ---------- |
| POST   | `/api/roles`      | Crear nuevo rol          | ✅ Admin | 100/min    |
| GET    | `/api/roles`      | Listar roles (paginado)  | ✅       | 150/min    |
| GET    | `/api/roles/{id}` | Obtener rol con permisos | ✅       | 200/min    |
| PUT    | `/api/roles/{id}` | Actualizar rol           | ✅ Admin | 100/min    |
| DELETE | `/api/roles/{id}` | Eliminar rol             | ✅ Admin | 50/min     |

### 2.2 PermissionsController

| Método | Endpoint           | Descripción     | Auth     | Rate Limit |
| ------ | ------------------ | --------------- | -------- | ---------- |
| POST   | `/api/permissions` | Crear permiso   | ✅ Admin | 100/min    |
| GET    | `/api/permissions` | Listar permisos | ✅       | 150/min    |

### 2.3 RolePermissionsController

| Método | Endpoint                       | Descripción            | Auth     | Rate Limit |
| ------ | ------------------------------ | ---------------------- | -------- | ---------- |
| POST   | `/api/role-permissions/assign` | Asignar permiso a rol  | ✅ Admin | 100/min    |
| POST   | `/api/role-permissions/remove` | Remover permiso de rol | ✅ Admin | 100/min    |
| POST   | `/api/role-permissions/check`  | Verificar permiso      | ✅       | 200/min    |

---

## 3. Modelo de Datos RBAC

### 3.1 Estructura Jerárquica

```
Usuarios
    └── Roles (asignados via UserService)
            └── Permisos
                    └── Resource + Action
```

### 3.2 Roles Predefinidos del Sistema

| Rol                 | Descripción              | Permisos Principales                  |
| ------------------- | ------------------------ | ------------------------------------- |
| `SuperAdmin`        | Control total            | `*:*` (todos los recursos y acciones) |
| `Admin`             | Administrador plataforma | Gestión usuarios, vehículos, dealers  |
| `DealerOwner`       | Dueño de concesionario   | CRUD inventario, empleados, reportes  |
| `DealerEmployee`    | Empleado de dealer       | CRUD vehículos, ver leads             |
| `Seller`            | Vendedor individual      | CRUD sus vehículos                    |
| `Buyer`             | Comprador                | Ver vehículos, favoritos, alertas     |
| `ComplianceOfficer` | Oficial compliance       | Ver reportes, STR, AML                |
| `CustomerSupport`   | Soporte cliente          | Ver usuarios, gestionar tickets       |

### 3.3 Convención de Permisos

**Formato:** `resource:action`

| Resource   | Actions                                                    |
| ---------- | ---------------------------------------------------------- |
| `vehicles` | `create`, `read`, `update`, `delete`, `publish`, `feature` |
| `users`    | `create`, `read`, `update`, `delete`, `ban`, `verify`      |
| `dealers`  | `create`, `read`, `update`, `delete`, `verify`, `suspend`  |
| `billing`  | `read`, `create`, `refund`, `manage-plans`                 |
| `reports`  | `read`, `create`, `export`, `schedule`                     |
| `kyc`      | `read`, `approve`, `reject`, `request-documents`           |
| `aml`      | `read`, `create-str`, `escalate`, `clear`                  |
| `admin`    | `access`, `manage-roles`, `manage-permissions`             |

---

## 4. Procesos Detallados

### ROLE-CRT-001: Crear Rol

| Campo          | Valor             |
| -------------- | ----------------- |
| **ID**         | ROLE-CRT-001      |
| **Nombre**     | Crear Nuevo Rol   |
| **Actor**      | SuperAdmin, Admin |
| **Criticidad** | 🔴 CRÍTICO        |
| **Estado**     | 🟢 ACTIVO         |

#### Precondiciones

- [ ] Usuario autenticado con permiso `admin:manage-roles`
- [ ] Nombre del rol único
- [ ] Datos válidos según schema

#### Request Body

```json
{
  "name": "string (3-50 chars, unique)",
  "displayName": "string (descripción legible)",
  "description": "string (opcional)",
  "isActive": true,
  "permissionIds": ["guid1", "guid2"]
}
```

#### Flujo Paso a Paso

| Paso | Acción                      | Servicio   | Validación         |
| ---- | --------------------------- | ---------- | ------------------ |
| 1    | Recibir request             | Controller | Schema válido      |
| 2    | Verificar autorización      | Policy     | admin:manage-roles |
| 3    | Validar nombre único        | Repository | Query por nombre   |
| 4    | Validar permisos existen    | Repository | Query permisos     |
| 5    | Crear entidad Role          | Handler    | Con defaults       |
| 6    | Asignar permisos            | Repository | Many-to-many       |
| 7    | Guardar en DB               | Repository | Transacción        |
| 8    | Publicar evento RoleCreated | RabbitMQ   | Para cache refresh |
| 9    | Retornar rol creado         | Controller | 200 OK             |

#### Response Success (200)

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "name": "DealerManager",
    "displayName": "Gerente de Concesionario",
    "description": "Rol para gerentes de dealers",
    "isActive": true,
    "permissionCount": 15,
    "createdAt": "2026-01-21T10:30:00Z"
  }
}
```

#### Postcondiciones

- [x] Rol creado en DB
- [x] Permisos asignados
- [x] Cache de roles invalidado
- [x] Evento publicado

#### Errores Posibles

| Código | Error                    | Causa                        | Acción                 |
| ------ | ------------------------ | ---------------------------- | ---------------------- |
| 400    | INVALID_ROLE_NAME        | Nombre vacío o muy corto     | Ingresar nombre válido |
| 409    | ROLE_EXISTS              | Rol con ese nombre ya existe | Usar nombre diferente  |
| 400    | INVALID_PERMISSION       | Permiso no existe            | Verificar IDs          |
| 403    | INSUFFICIENT_PERMISSIONS | Sin permiso admin            | N/A                    |

---

### ROLE-GET-001: Obtener Roles (Paginado)

| Campo          | Valor               |
| -------------- | ------------------- |
| **ID**         | ROLE-GET-001        |
| **Nombre**     | Listar Roles        |
| **Actor**      | Usuario Autenticado |
| **Criticidad** | 🟡 MEDIO            |
| **Estado**     | 🟢 ACTIVO           |

#### Query Parameters

| Parámetro  | Tipo  | Default | Descripción                |
| ---------- | ----- | ------- | -------------------------- |
| `isActive` | bool? | null    | Filtrar por estado         |
| `page`     | int   | 1       | Número de página           |
| `pageSize` | int   | 50      | Tamaño de página (max 100) |

#### Flujo Paso a Paso

| Paso | Acción                      | Servicio   | Validación         |
| ---- | --------------------------- | ---------- | ------------------ |
| 1    | Recibir query params        | Controller | Defaults aplicados |
| 2    | Validar paginación          | Handler    | pageSize <= 100    |
| 3    | Construir query             | Repository | Con filtros        |
| 4    | Ejecutar con Count          | DB         | Optimizado         |
| 5    | Mapear a DTOs               | Handler    | RoleListItemDto    |
| 6    | Retornar resultado paginado | Controller | 200 OK             |

#### Response Success (200)

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "name": "Admin",
        "displayName": "Administrador",
        "isActive": true,
        "userCount": 5,
        "permissionCount": 45
      }
      // ...más roles
    ],
    "totalCount": 12,
    "page": 1,
    "pageSize": 50,
    "totalPages": 1
  }
}
```

---

### ROLE-DTL-001: Obtener Rol con Permisos

| Campo          | Valor               |
| -------------- | ------------------- |
| **ID**         | ROLE-DTL-001        |
| **Nombre**     | Detalle de Rol      |
| **Actor**      | Usuario Autenticado |
| **Criticidad** | 🟡 MEDIO            |
| **Estado**     | 🟢 ACTIVO           |

#### Request

```http
GET /api/roles/{id:guid}
```

#### Flujo Paso a Paso

| Paso | Acción                    | Servicio   | Validación       |
| ---- | ------------------------- | ---------- | ---------------- |
| 1    | Recibir role ID           | Controller | GUID válido      |
| 2    | Buscar rol con Include    | Repository | Include permisos |
| 3    | Si no existe: 404         | Handler    |                  |
| 4    | Mapear a DTO con permisos | Handler    | RoleDetailsDto   |
| 5    | Retornar detalle          | Controller | 200 OK           |

#### Response Success (200)

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "name": "DealerOwner",
    "displayName": "Dueño de Concesionario",
    "description": "Acceso completo a su dealer",
    "isActive": true,
    "createdAt": "2026-01-01T00:00:00Z",
    "updatedAt": "2026-01-15T12:00:00Z",
    "permissions": [
      {
        "id": "guid",
        "name": "vehicles:create",
        "displayName": "Crear Vehículos",
        "module": "vehicles"
      },
      {
        "id": "guid",
        "name": "vehicles:update",
        "displayName": "Actualizar Vehículos",
        "module": "vehicles"
      }
      // ...más permisos
    ]
  }
}
```

---

### ROLE-UPD-001: Actualizar Rol

| Campo          | Valor             |
| -------------- | ----------------- |
| **ID**         | ROLE-UPD-001      |
| **Nombre**     | Actualizar Rol    |
| **Actor**      | SuperAdmin, Admin |
| **Criticidad** | 🔴 CRÍTICO        |
| **Estado**     | 🟢 ACTIVO         |

#### Request Body

```json
{
  "displayName": "string (opcional)",
  "description": "string (opcional)",
  "isActive": false,
  "permissionIds": ["guid1", "guid2"]
}
```

#### Flujo Paso a Paso

| Paso | Acción                      | Servicio     | Validación                     |
| ---- | --------------------------- | ------------ | ------------------------------ |
| 1    | Verificar rol existe        | Repository   | 404 si no                      |
| 2    | Verificar rol no es sistema | Handler      | Admin, SuperAdmin no editables |
| 3    | Actualizar propiedades      | Handler      | Solo las enviadas              |
| 4    | Sincronizar permisos        | Repository   | Add/Remove                     |
| 5    | Guardar cambios             | Repository   | UPDATE                         |
| 6    | Invalidar cache             | CacheService | Clave del rol                  |
| 7    | Publicar evento RoleUpdated | RabbitMQ     |                                |
| 8    | Retornar rol actualizado    | Controller   | 200 OK                         |

#### Reglas Especiales

- ⚠️ No se puede modificar `SuperAdmin`
- ⚠️ No se puede desactivar `Admin` si hay usuarios asignados
- ⚠️ El campo `name` no es editable (inmutable)

---

### PERM-CRT-001: Crear Permiso

| Campo          | Valor         |
| -------------- | ------------- |
| **ID**         | PERM-CRT-001  |
| **Nombre**     | Crear Permiso |
| **Actor**      | SuperAdmin    |
| **Criticidad** | 🔴 CRÍTICO    |
| **Estado**     | 🟢 ACTIVO     |

#### Request Body

```json
{
  "name": "vehicles:feature",
  "displayName": "Destacar Vehículos",
  "description": "Permite marcar vehículos como destacados",
  "module": "vehicles",
  "resource": "vehicles",
  "action": "feature"
}
```

#### Flujo Paso a Paso

| Paso | Acción                            | Servicio   | Validación        |
| ---- | --------------------------------- | ---------- | ----------------- |
| 1    | Validar formato nombre            | Validator  | `resource:action` |
| 2    | Verificar nombre único            | Repository | Query             |
| 3    | Validar módulo existe             | Handler    | Lista permitida   |
| 4    | Crear entidad Permission          | Handler    |                   |
| 5    | Guardar en DB                     | Repository | INSERT            |
| 6    | Publicar evento PermissionCreated | RabbitMQ   |                   |
| 7    | Retornar permiso creado           | Controller | 200 OK            |

#### Módulos Permitidos

- `auth`, `users`, `vehicles`, `dealers`, `billing`, `media`, `notifications`
- `reports`, `analytics`, `kyc`, `aml`, `compliance`, `admin`, `crm`

---

### PERM-LST-001: Listar Permisos

| Campo          | Valor           |
| -------------- | --------------- |
| **ID**         | PERM-LST-001    |
| **Nombre**     | Listar Permisos |
| **Actor**      | Admin           |
| **Criticidad** | 🟡 MEDIO        |
| **Estado**     | 🟢 ACTIVO       |

#### Query Parameters

| Parámetro  | Tipo    | Descripción         |
| ---------- | ------- | ------------------- |
| `module`   | string? | Filtrar por módulo  |
| `resource` | string? | Filtrar por recurso |

#### Response Success (200)

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "vehicles:create",
      "displayName": "Crear Vehículos",
      "description": "Permite crear nuevos vehículos",
      "module": "vehicles",
      "resource": "vehicles",
      "action": "create"
    }
    // ...más permisos
  ]
}
```

---

### RPERM-ASN-001: Asignar Permiso a Rol

| Campo          | Valor             |
| -------------- | ----------------- |
| **ID**         | RPERM-ASN-001     |
| **Nombre**     | Asignar Permiso   |
| **Actor**      | SuperAdmin, Admin |
| **Criticidad** | 🔴 CRÍTICO        |
| **Estado**     | 🟢 ACTIVO         |

#### Request Body

```json
{
  "roleId": "guid",
  "permissionId": "guid"
}
```

#### Flujo Paso a Paso

| Paso | Acción                  | Servicio     | Validación         |
| ---- | ----------------------- | ------------ | ------------------ |
| 1    | Validar rol existe      | Repository   | 404                |
| 2    | Validar permiso existe  | Repository   | 404                |
| 3    | Verificar no duplicado  | Repository   | Query              |
| 4    | Crear asignación        | Repository   | RolePermission     |
| 5    | Guardar                 | Repository   | INSERT             |
| 6    | Invalidar cache del rol | CacheService |                    |
| 7    | Publicar evento         | RabbitMQ     | PermissionAssigned |
| 8    | Retornar éxito          | Controller   | 200 OK             |

#### Postcondiciones

- [x] Permiso asignado al rol
- [x] Usuarios con ese rol ahora tienen el permiso
- [x] Cache actualizado

---

### RPERM-REM-001: Remover Permiso de Rol

| Campo          | Valor             |
| -------------- | ----------------- |
| **ID**         | RPERM-REM-001     |
| **Nombre**     | Remover Permiso   |
| **Actor**      | SuperAdmin, Admin |
| **Criticidad** | 🔴 CRÍTICO        |
| **Estado**     | 🟢 ACTIVO         |

#### Request Body

```json
{
  "roleId": "guid",
  "permissionId": "guid"
}
```

#### Flujo Paso a Paso

| Paso | Acción              | Servicio     | Validación        |
| ---- | ------------------- | ------------ | ----------------- |
| 1    | Validar rol existe  | Repository   | 404               |
| 2    | Buscar asignación   | Repository   | Query             |
| 3    | Si no existe: 404   | Handler      |                   |
| 4    | Eliminar asignación | Repository   | DELETE            |
| 5    | Invalidar cache     | CacheService |                   |
| 6    | Publicar evento     | RabbitMQ     | PermissionRemoved |
| 7    | Retornar éxito      | Controller   | 200 OK            |

---

### RPERM-CHK-001: Verificar Permiso

| Campo          | Valor              |
| -------------- | ------------------ |
| **ID**         | RPERM-CHK-001      |
| **Nombre**     | Check Permission   |
| **Actor**      | Gateway, Servicios |
| **Criticidad** | 🔴 CRÍTICO         |
| **Estado**     | 🟢 ACTIVO          |

#### Descripción

Este endpoint es llamado por el Gateway y otros servicios para verificar si un usuario (via sus roles) tiene un permiso específico. Es el corazón del sistema de autorización.

#### Request Body (Preferido - con RoleIds)

```json
{
  "roleIds": ["guid1", "guid2"],
  "resource": "vehicles",
  "action": "create"
}
```

#### Request Body (Deprecado - con UserId)

```json
{
  "userId": "guid",
  "resource": "vehicles",
  "action": "create"
}
```

#### Flujo Paso a Paso (Preferido)

| Paso | Acción                            | Servicio     | Validación                   |
| ---- | --------------------------------- | ------------ | ---------------------------- |
| 1    | Recibir roleIds, resource, action | Controller   | No vacíos                    |
| 2    | Buscar en cache                   | CacheService | Key: `role:{id}:permissions` |
| 3    | Si cache hit: evaluar             | Handler      | Rápido                       |
| 4    | Si cache miss: query DB           | Repository   | Include permisos             |
| 5    | Construir permiso buscado         | Handler      | `{resource}:{action}`        |
| 6    | Verificar en cualquier rol        | Handler      | OR lógico                    |
| 7    | Guardar en cache                  | CacheService | TTL 5 min                    |
| 8    | Retornar resultado                | Controller   | 200 OK                       |

#### Response Success (200)

```json
{
  "success": true,
  "data": {
    "hasPermission": true,
    "permission": "vehicles:create",
    "grantedByRole": "DealerOwner",
    "cached": true
  }
}
```

#### Performance

- Cache hit: < 5ms
- Cache miss: < 50ms
- Rate limit: 200 req/min (alto por uso interno)

---

## 5. Flujos de Integración

### 5.1 Verificación de Permiso en Gateway

```mermaid
sequenceDiagram
    participant C as Cliente
    participant G as Gateway
    participant R as RoleService
    participant S as Servicio Destino

    C->>G: Request + JWT
    G->>G: Extraer roleIds del JWT
    G->>R: POST /role-permissions/check
    R->>R: Buscar en cache
    alt Cache Hit
        R->>G: {hasPermission: true}
    else Cache Miss
        R->>R: Query DB
        R->>R: Guardar en cache
        R->>G: {hasPermission: true}
    end
    alt Tiene Permiso
        G->>S: Forward request
        S->>G: Response
        G->>C: Response
    else No Tiene Permiso
        G->>C: 403 Forbidden
    end
```

### 5.2 Asignación de Rol a Usuario

```mermaid
sequenceDiagram
    participant A as Admin
    participant US as UserService
    participant RS as RoleService
    participant AU as AuditService

    A->>US: POST /users/{id}/roles
    US->>RS: GET /roles/{roleId} (validar existe)
    RS->>US: Role details
    US->>US: Crear UserRole
    US->>AU: Evento: RoleAssigned
    US->>A: 201 Created
```

---

## 6. Reglas de Negocio

### 6.1 Jerarquía de Roles

| Rol         | Puede Asignar                       | Puede Revocar              |
| ----------- | ----------------------------------- | -------------------------- |
| SuperAdmin  | Todos                               | Todos                      |
| Admin       | DealerOwner, Seller, Buyer, Support | DealerOwner, Seller, Buyer |
| DealerOwner | DealerEmployee                      | DealerEmployee             |
| Otros       | Ninguno                             | Ninguno                    |

### 6.2 Restricciones

| Restricción                            | Descripción                       |
| -------------------------------------- | --------------------------------- |
| Un usuario puede tener múltiples roles | Permisos se acumulan (OR)         |
| Roles del sistema son inmutables       | SuperAdmin, Admin no modificables |
| No se puede eliminar rol con usuarios  | Primero desasignar                |
| Permisos son atómicos                  | No hay herencia                   |

### 6.3 Cache de Permisos

| Configuración | Valor                    |
| ------------- | ------------------------ |
| TTL default   | 5 minutos                |
| Invalidación  | Al modificar rol/permiso |
| Estrategia    | Write-through            |
| Backend       | Redis                    |

---

## 7. Manejo de Errores

### 7.1 Códigos de Error

| Código   | Nombre                    | HTTP | Descripción                           |
| -------- | ------------------------- | ---- | ------------------------------------- |
| ROLE001  | ROLE_NOT_FOUND            | 404  | Rol no existe                         |
| ROLE002  | ROLE_EXISTS               | 409  | Rol con ese nombre existe             |
| ROLE003  | ROLE_IS_SYSTEM            | 400  | No se puede modificar rol del sistema |
| ROLE004  | ROLE_HAS_USERS            | 400  | Rol tiene usuarios asignados          |
| PERM001  | PERMISSION_NOT_FOUND      | 404  | Permiso no existe                     |
| PERM002  | PERMISSION_EXISTS         | 409  | Permiso con ese nombre existe         |
| PERM003  | INVALID_PERMISSION_FORMAT | 400  | Formato debe ser resource:action      |
| RPERM001 | ASSIGNMENT_EXISTS         | 409  | Permiso ya asignado al rol            |
| RPERM002 | ASSIGNMENT_NOT_FOUND      | 404  | Asignación no existe                  |
| AUTH001  | INSUFFICIENT_PERMISSIONS  | 403  | Sin permiso para esta acción          |

---

## 8. Eventos Publicados (RabbitMQ)

| Evento             | Exchange    | Routing Key         | Consumers                  |
| ------------------ | ----------- | ------------------- | -------------------------- |
| RoleCreated        | role.events | role.created        | AuditService, CacheService |
| RoleUpdated        | role.events | role.updated        | AuditService, CacheService |
| RoleDeleted        | role.events | role.deleted        | AuditService, CacheService |
| PermissionCreated  | role.events | permission.created  | AuditService               |
| PermissionAssigned | role.events | permission.assigned | AuditService, CacheService |
| PermissionRemoved  | role.events | permission.removed  | AuditService, CacheService |

---

## 9. Configuración

### 9.1 appsettings.json

```json
{
  "RoleService": {
    "CacheEnabled": true,
    "CacheTtlMinutes": 5,
    "MaxRolesPerUser": 10,
    "MaxPermissionsPerRole": 100
  },
  "Authorization": {
    "SystemRoles": ["SuperAdmin", "Admin"],
    "DefaultBuyerRole": "Buyer",
    "DefaultSellerRole": "Seller"
  }
}
```

---

## 10. Permisos Predefinidos del Sistema

### 10.1 Módulo: Vehicles

| Permiso            | Descripción           |
| ------------------ | --------------------- |
| `vehicles:create`  | Crear vehículos       |
| `vehicles:read`    | Ver vehículos         |
| `vehicles:update`  | Actualizar vehículos  |
| `vehicles:delete`  | Eliminar vehículos    |
| `vehicles:publish` | Publicar/despublicar  |
| `vehicles:feature` | Marcar como destacado |
| `vehicles:import`  | Importar en batch     |

### 10.2 Módulo: Users

| Permiso              | Descripción         |
| -------------------- | ------------------- |
| `users:create`       | Crear usuarios      |
| `users:read`         | Ver usuarios        |
| `users:update`       | Actualizar usuarios |
| `users:delete`       | Eliminar usuarios   |
| `users:ban`          | Banear usuarios     |
| `users:verify`       | Verificar identidad |
| `users:assign-roles` | Asignar roles       |

### 10.3 Módulo: Admin

| Permiso                    | Descripción           |
| -------------------------- | --------------------- |
| `admin:access`             | Acceso al panel admin |
| `admin:manage-roles`       | Gestionar roles       |
| `admin:manage-permissions` | Gestionar permisos    |
| `admin:view-logs`          | Ver logs del sistema  |
| `admin:manage-settings`    | Configuración global  |

---

**Documento generado:** Enero 21, 2026  
**Versión:** 1.0.0  
**Autor:** Equipo OKLA
