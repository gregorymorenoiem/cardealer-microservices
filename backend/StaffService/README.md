# StaffService

Microservicio para gestión de empleados/staff interno de OKLA.

## Responsabilidades

- **Gestión de Staff**: CRUD de empleados internos
- **Invitaciones**: Sistema de invitación para onboarding de nuevos empleados
- **Departamentos**: Estructura organizacional por departamentos
- **Posiciones**: Cargos/títulos de trabajo
- **Permisos**: Permisos granulares adicionales a los roles

## Arquitectura

```
StaffService/
├── StaffService.Api/           # Controllers, Program.cs
├── StaffService.Application/   # Features (CQRS), DTOs, Validators
├── StaffService.Domain/        # Entities, Interfaces, Enums
├── StaffService.Infrastructure/# DbContext, Repositories, Clients
└── StaffService.Tests/         # Unit & Integration tests
```

## Entidades

| Entidad           | Descripción                          |
| ----------------- | ------------------------------------ |
| `Staff`           | Empleado interno con datos de perfil |
| `StaffInvitation` | Invitación para onboarding           |
| `Department`      | Departamento organizacional          |
| `Position`        | Cargo/título de trabajo              |
| `StaffPermission` | Permisos adicionales por empleado    |

## Roles

| Rol        | Descripción              | Nivel |
| ---------- | ------------------------ | ----- |
| SuperAdmin | Acceso total al sistema  | 0     |
| Admin      | Gestión general          | 1     |
| Compliance | Cumplimiento regulatorio | 2     |
| Moderator  | Moderación de contenido  | 3     |
| Support    | Soporte al cliente       | 4     |
| Analyst    | Análisis (solo lectura)  | 5     |

## Endpoints

### Staff (`/api/staff`)

- `GET /` - Listar staff con filtros
- `GET /{id}` - Obtener staff por ID
- `GET /auth/{authUserId}` - Obtener por AuthService ID
- `GET /summary` - Estadísticas
- `PUT /{id}` - Actualizar staff
- `POST /{id}/status` - Cambiar estado
- `POST /{id}/terminate` - Terminar empleado
- `DELETE /{id}` - Eliminar (SuperAdmin)

### Invitations (`/api/staff/invitations`)

- `GET /` - Listar invitaciones
- `GET /{id}` - Obtener invitación
- `GET /validate/{token}` - Validar token (público)
- `POST /` - Crear invitación
- `POST /accept` - Aceptar invitación (público)
- `POST /{id}/resend` - Reenviar email
- `POST /{id}/revoke` - Revocar invitación

### Departments (`/api/staff/departments`)

- `GET /` - Listar departamentos
- `GET /tree` - Árbol jerárquico
- `GET /{id}` - Obtener departamento
- `POST /` - Crear departamento
- `PUT /{id}` - Actualizar
- `DELETE /{id}` - Eliminar (SuperAdmin)

### Positions (`/api/staff/positions`)

- `GET /` - Listar posiciones
- `GET /department/{id}` - Por departamento
- `GET /{id}` - Obtener posición
- `POST /` - Crear posición
- `PUT /{id}` - Actualizar
- `DELETE /{id}` - Eliminar (SuperAdmin)

## Integración con otros servicios

| Servicio            | Comunicación | Propósito                                 |
| ------------------- | ------------ | ----------------------------------------- |
| AuthService         | HTTP         | Crear/gestionar usuarios de autenticación |
| NotificationService | HTTP         | Enviar emails de invitación/bienvenida    |
| AuditService        | HTTP         | Logging de acciones                       |

## Puerto

- **Desarrollo**: 15200
- **Docker/K8s**: 8080

## Base de datos

- **PostgreSQL**: `staffservice`
- **Migraciones**: Automáticas al iniciar

## Configuración

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=staffservice;Username=postgres;Password=postgres"
  },
  "ServiceUrls": {
    "AuthService": "http://localhost:15001",
    "NotificationService": "http://localhost:15005",
    "AuditService": "http://localhost:15112"
  }
}
```

## Ejecutar localmente

```bash
cd backend/StaffService
dotnet restore
dotnet run --project StaffService.Api
```

## Docker

```bash
docker build -t staffservice -f backend/StaffService/Dockerfile .
docker run -p 15200:8080 staffservice
```
