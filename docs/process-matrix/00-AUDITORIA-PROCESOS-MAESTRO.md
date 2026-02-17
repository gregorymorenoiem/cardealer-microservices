# 📋 OKLA - Documento Maestro de Procesos Auditables

> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Propósito:** Trazabilidad completa de procesos para auditoría  
> **Clasificación:** Documento Interno - Confidencial

---

## 📑 Índice

1. [Introducción y Propósito](#1-introducción-y-propósito)
2. [Tipos de Actores](#2-tipos-de-actores)
3. [Tipos de Evidencias](#3-tipos-de-evidencias)
4. [Procesos de Autenticación y Seguridad](#4-procesos-de-autenticación-y-seguridad)
5. [Procesos de Gestión de Usuarios](#5-procesos-de-gestión-de-usuarios)
6. [Procesos de Dealers](#6-procesos-de-dealers)
7. [Procesos de Vehículos e Inventario](#7-procesos-de-vehículos-e-inventario)
8. [Procesos de Pagos y Facturación](#8-procesos-de-pagos-y-facturación)
9. [Procesos de CRM y Leads](#9-procesos-de-crm-y-leads)
10. [Procesos de Compliance](#10-procesos-de-compliance)
11. [Procesos de Administración](#11-procesos-de-administración)
12. [Matriz de Retención de Evidencias](#12-matriz-de-retención-de-evidencias)

---

## 1. Introducción y Propósito

### 1.1 Objetivo del Documento

Este documento establece la **trazabilidad completa** de todos los procesos de la plataforma OKLA, identificando:

- **Quién** inicia cada proceso (actor humano o sistema)
- **Qué** pasos se ejecutan secuencialmente
- **Qué evidencias** se generan en cada paso
- **Dónde** se almacenan las evidencias
- **Cuánto tiempo** se retienen

### 1.2 Marco Regulatorio

Las evidencias cumplen con:

| Regulación           | Requisito                 | Retención |
| -------------------- | ------------------------- | --------- |
| **Ley 155-17**       | Anti-lavado de activos    | 10 años   |
| **Ley 172-13**       | Protección de datos       | 5 años    |
| **DGII**             | Documentos fiscales       | 10 años   |
| **Norma 01-2015 SB** | Transacciones financieras | 5 años    |

### 1.3 Arquitectura de Auditoría

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    ARQUITECTURA DE AUDITORÍA OKLA                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   ┌───────────────────────────────────────────────────────────────────┐ │
│   │                         CAPA DE CAPTURA                           │ │
│   │                                                                    │ │
│   │   API Request → Middleware → Controller → Service → Repository    │ │
│   │        │            │           │           │           │         │ │
│   │        ▼            ▼           ▼           ▼           ▼         │ │
│   │   [Request   [Auth      [Action    [Domain   [DB        │         │ │
│   │    Log]       Log]       Log]       Event]    Change]   │         │ │
│   └───────────────────────────────────────────────────────────────────┘ │
│                                   │                                      │
│                                   ▼                                      │
│   ┌───────────────────────────────────────────────────────────────────┐ │
│   │                     CAPA DE ALMACENAMIENTO                        │ │
│   │                                                                    │ │
│   │   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐               │ │
│   │   │  PostgreSQL │  │    Redis    │  │     S3      │               │ │
│   │   │             │  │             │  │             │               │ │
│   │   │ audit_logs  │  │ session:*   │  │ /audit/     │               │ │
│   │   │ audit_trail │  │ token:*     │  │ /documents/ │               │ │
│   │   │ *_history   │  │ rate:*      │  │ /exports/   │               │ │
│   │   └─────────────┘  └─────────────┘  └─────────────┘               │ │
│   │                                                                    │ │
│   │   ┌─────────────┐  ┌─────────────┐                                │ │
│   │   │  RabbitMQ   │  │ Elasticsearch│                               │ │
│   │   │             │  │             │                                │ │
│   │   │ audit.*     │  │ logs-*      │                                │ │
│   │   │ events.*    │  │ audit-*     │                                │ │
│   │   └─────────────┘  └─────────────┘                                │ │
│   └───────────────────────────────────────────────────────────────────┘ │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Tipos de Actores

### 2.1 Actores Humanos

| Código        | Actor               | Descripción               | Permisos                      |
| ------------- | ------------------- | ------------------------- | ----------------------------- |
| `USR-ANON`    | Usuario Anónimo     | Visitante sin cuenta      | Solo lectura pública          |
| `USR-REG`     | Usuario Registrado  | Comprador con cuenta      | Búsqueda, favoritos, contacto |
| `USR-SELLER`  | Vendedor Individual | Vende su propio vehículo  | Publicar 1 listing            |
| `DLR-STAFF`   | Staff de Dealer     | Empleado de concesionario | Según rol asignado            |
| `DLR-ADMIN`   | Admin de Dealer     | Administrador del dealer  | Full access al dealer         |
| `ADM-SUPPORT` | Soporte OKLA        | Agente de soporte         | Ver usuarios, tickets         |
| `ADM-MOD`     | Moderador OKLA      | Moderador de contenido    | Aprobar/rechazar listings     |
| `ADM-COMP`    | Compliance Officer  | Oficial de cumplimiento   | Reportes regulatorios         |
| `ADM-ADMIN`   | Administrador OKLA  | Administrador del sistema | Configuración, usuarios       |
| `ADM-SUPER`   | Super Admin OKLA    | Acceso total              | Todo el sistema               |

### 2.2 Actores Sistema (Microservicios)

| Código           | Sistema             | Descripción             | Triggers          |
| ---------------- | ------------------- | ----------------------- | ----------------- |
| `SYS-SCHEDULER`  | SchedulerService    | Jobs programados        | Cron expressions  |
| `SYS-BILLING`    | BillingService      | Procesos de pago        | Webhooks, timers  |
| `SYS-NOTIF`      | NotificationService | Envío de notificaciones | Eventos           |
| `SYS-ANALYTICS`  | AnalyticsService    | Agregaciones            | Diario/horario    |
| `SYS-COMPLIANCE` | ComplianceService   | Verificaciones          | Reglas de negocio |
| `SYS-ML`         | MLService           | Modelos de IA           | Batch processing  |

---

## 3. Tipos de Evidencias

### 3.1 Clasificación de Evidencias

| Tipo                     | Código       | Descripción           | Formato   | Retención  |
| ------------------------ | ------------ | --------------------- | --------- | ---------- |
| **Log de Aplicación**    | `EVD-LOG`    | Registro técnico      | JSON      | 90 días    |
| **Audit Trail**          | `EVD-AUDIT`  | Registro de auditoría | JSON      | 10 años    |
| **Evento de Dominio**    | `EVD-EVENT`  | Evento de negocio     | JSON      | 5 años     |
| **Snapshot de Estado**   | `EVD-SNAP`   | Estado antes/después  | JSON      | 5 años     |
| **Documento Generado**   | `EVD-DOC`    | PDF, factura, reporte | PDF/XML   | 10 años    |
| **Comunicación**         | `EVD-COMM`   | Email, SMS, WhatsApp  | JSON      | 5 años     |
| **Firma/Consentimiento** | `EVD-SIGN`   | Aceptación legal      | JSON+Hash | 10 años    |
| **Archivo Subido**       | `EVD-FILE`   | Documento del usuario | Original  | 10 años    |
| **Captura de Pantalla**  | `EVD-SCREEN` | Screenshot automático | PNG       | 2 años     |
| **Hash de Integridad**   | `EVD-HASH`   | SHA256 de datos       | String    | Permanente |

### 3.2 Estructura de Audit Trail

```csharp
public class AuditTrailEntry
{
    // Identificación
    public Guid Id { get; set; }
    public string TraceId { get; set; }           // Correlación entre sistemas
    public string SpanId { get; set; }            // Paso específico

    // Actor
    public string ActorType { get; set; }         // USR-REG, DLR-ADMIN, SYS-SCHEDULER
    public Guid? ActorId { get; set; }            // ID del usuario o null si sistema
    public string ActorName { get; set; }         // Nombre legible
    public string ActorIp { get; set; }           // IP si aplica
    public string ActorUserAgent { get; set; }    // Browser/App

    // Acción
    public string ProcessCode { get; set; }       // AUTH-001, PAY-003
    public string ProcessName { get; set; }       // "Login Usuario"
    public string StepNumber { get; set; }        // "1.1", "1.2", "2.1"
    public string StepName { get; set; }          // "Validar credenciales"
    public string Action { get; set; }            // CREATE, UPDATE, DELETE, READ, EXECUTE

    // Recurso
    public string ResourceType { get; set; }      // User, Vehicle, Payment
    public string ResourceId { get; set; }        // ID del recurso
    public string ResourceName { get; set; }      // Nombre legible

    // Cambios
    public string OldValues { get; set; }         // JSON del estado anterior
    public string NewValues { get; set; }         // JSON del nuevo estado
    public string Changes { get; set; }           // JSON de los cambios

    // Resultado
    public string Status { get; set; }            // Success, Failed, Pending
    public string ErrorCode { get; set; }         // Código de error si falló
    public string ErrorMessage { get; set; }      // Mensaje de error

    // Metadata
    public string ServiceName { get; set; }       // Microservicio
    public string ServiceVersion { get; set; }    // Versión
    public string Environment { get; set; }       // Production, Staging
    public DateTime Timestamp { get; set; }       // UTC
    public string TimestampLocal { get; set; }    // America/Santo_Domingo

    // Integridad
    public string PreviousHash { get; set; }      // Hash del registro anterior
    public string CurrentHash { get; set; }       // Hash de este registro
}
```

---

## 4. Procesos de Autenticación y Seguridad

### 4.1 AUTH-001: Registro de Usuario

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: AUTH-001 - Registro de Usuario                                │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-ANON (Usuario Anónimo)                            │
│ Sistemas: AuthService, UserService, NotificationService                │
│ Duración Estimada: 30 segundos - 5 minutos                             │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                      | Sistema             | Actor     | Evidencia                 | Código Evidencia |
| ---- | ------- | --------------------------- | ------------------- | --------- | ------------------------- | ---------------- |
| 1    | 1.1     | Usuario accede a /register  | Frontend            | USR-ANON  | Log de navegación         | EVD-LOG          |
| 1    | 1.2     | Renderiza formulario        | Frontend            | Sistema   | -                         | -                |
| 2    | 2.1     | Usuario completa formulario | Frontend            | USR-ANON  | Timestamp inicio          | EVD-LOG          |
| 2    | 2.2     | Validación client-side      | Frontend            | Sistema   | Errores de validación     | EVD-LOG          |
| 3    | 3.1     | POST /api/auth/register     | Gateway             | USR-ANON  | Request log completo      | EVD-AUDIT        |
| 3    | 3.2     | Rate limit check            | Gateway             | Sistema   | Rate limit log            | EVD-LOG          |
| 3    | 3.3     | Validación de payload       | AuthService         | Sistema   | Validation result         | EVD-LOG          |
| 4    | 4.1     | Verificar email único       | AuthService         | Sistema   | Query log                 | EVD-LOG          |
| 4    | 4.2     | Verificar teléfono único    | AuthService         | Sistema   | Query log                 | EVD-LOG          |
| 5    | 5.1     | Hash de contraseña          | AuthService         | Sistema   | Hash algorithm used       | EVD-LOG          |
| 5    | 5.2     | Generar código verificación | AuthService         | Sistema   | Código generado (hash)    | EVD-AUDIT        |
| 6    | 6.1     | Crear registro User         | UserService         | Sistema   | **User Created Event**    | EVD-EVENT        |
| 6    | 6.2     | Snapshot estado inicial     | UserService         | Sistema   | **Estado completo**       | EVD-SNAP         |
| 7    | 7.1     | Enviar email verificación   | NotificationService | SYS-NOTIF | **Email enviado**         | EVD-COMM         |
| 7    | 7.2     | Log de envío                | NotificationService | Sistema   | Delivery status           | EVD-LOG          |
| 8    | 8.1     | Retornar respuesta          | AuthService         | Sistema   | Response log              | EVD-LOG          |
| 8    | 8.2     | **Audit trail completo**    | AuditService        | Sistema   | **Registro de auditoría** | EVD-AUDIT        |

**Evidencias Generadas:**

```json
{
  "processCode": "AUTH-001",
  "evidences": [
    {
      "step": "3.1",
      "type": "EVD-AUDIT",
      "description": "Request de registro recibido",
      "data": {
        "endpoint": "POST /api/auth/register",
        "ip": "190.52.xx.xx",
        "userAgent": "Mozilla/5.0...",
        "payload": {
          "email": "usuario@email.com",
          "phone": "+1809555****",
          "firstName": "Juan",
          "lastName": "Pérez"
        }
      },
      "timestamp": "2026-01-21T10:30:00Z"
    },
    {
      "step": "6.1",
      "type": "EVD-EVENT",
      "description": "Usuario creado en sistema",
      "data": {
        "eventType": "UserCreatedEvent",
        "userId": "550e8400-e29b-41d4-a716-446655440000",
        "email": "usuario@email.com",
        "status": "PendingVerification"
      },
      "timestamp": "2026-01-21T10:30:02Z"
    },
    {
      "step": "6.2",
      "type": "EVD-SNAP",
      "description": "Estado inicial del usuario",
      "data": {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "email": "usuario@email.com",
        "status": "PendingVerification",
        "emailVerified": false,
        "phoneVerified": false,
        "createdAt": "2026-01-21T10:30:02Z"
      },
      "timestamp": "2026-01-21T10:30:02Z"
    },
    {
      "step": "7.1",
      "type": "EVD-COMM",
      "description": "Email de verificación enviado",
      "data": {
        "type": "Email",
        "to": "usuario@email.com",
        "template": "email-verification",
        "provider": "SendGrid",
        "messageId": "sg-12345",
        "status": "Sent"
      },
      "timestamp": "2026-01-21T10:30:03Z"
    }
  ]
}
```

---

### 4.2 AUTH-002: Login de Usuario

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: AUTH-002 - Login de Usuario                                   │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-ANON (Usuario con cuenta)                         │
│ Sistemas: AuthService, UserService, AuditService                       │
│ Duración Estimada: 1-3 segundos                                        │
│ Criticidad: CRÍTICA                                                    │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                        | Sistema      | Actor    | Evidencia                | Código Evidencia |
| ---- | ------- | ----------------------------- | ------------ | -------- | ------------------------ | ---------------- |
| 1    | 1.1     | POST /api/auth/login          | Gateway      | USR-ANON | Request con credenciales | EVD-AUDIT        |
| 1    | 1.2     | Rate limit check              | Gateway      | Sistema  | IP + intentos            | EVD-LOG          |
| 2    | 2.1     | Buscar usuario por email      | AuthService  | Sistema  | Query result             | EVD-LOG          |
| 2    | 2.2     | Verificar cuenta activa       | AuthService  | Sistema  | Account status           | EVD-LOG          |
| 3    | 3.1     | Verificar contraseña          | AuthService  | Sistema  | **Auth attempt log**     | EVD-AUDIT        |
| 3    | 3.2     | Incrementar/resetear intentos | AuthService  | Sistema  | Failed attempts count    | EVD-LOG          |
| 4    | 4.1     | Verificar si requiere 2FA     | AuthService  | Sistema  | 2FA status               | EVD-LOG          |
| 4    | 4.2     | Si 2FA: generar challenge     | AuthService  | Sistema  | Challenge created        | EVD-AUDIT        |
| 5    | 5.1     | Generar JWT token             | AuthService  | Sistema  | Token claims             | EVD-LOG          |
| 5    | 5.2     | Generar refresh token         | AuthService  | Sistema  | Refresh token ID         | EVD-LOG          |
| 5    | 5.3     | Guardar sesión                | AuthService  | Sistema  | **Session created**      | EVD-AUDIT        |
| 6    | 6.1     | Actualizar last login         | UserService  | Sistema  | User updated             | EVD-LOG          |
| 6    | 6.2     | Registrar dispositivo         | UserService  | Sistema  | Device fingerprint       | EVD-AUDIT        |
| 7    | 7.1     | **Login audit entry**         | AuditService | Sistema  | **Audit trail completo** | EVD-AUDIT        |
| 7    | 7.2     | Publicar LoginEvent           | RabbitMQ     | Sistema  | Event published          | EVD-EVENT        |

**Evidencias Críticas para Auditoría de Accesos:**

```json
{
  "processCode": "AUTH-002",
  "auditEntry": {
    "id": "aud-001-login-xyz",
    "action": "LOGIN",
    "actor": {
      "type": "USR-REG",
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "usuario@email.com"
    },
    "request": {
      "ip": "190.52.xx.xx",
      "userAgent": "Mozilla/5.0...",
      "geoLocation": {
        "country": "DO",
        "city": "Santo Domingo"
      }
    },
    "result": {
      "status": "SUCCESS",
      "sessionId": "sess-12345",
      "tokenExpiry": "2026-01-21T11:30:00Z"
    },
    "security": {
      "2faRequired": false,
      "newDevice": false,
      "riskScore": 0.1
    },
    "timestamp": "2026-01-21T10:30:00Z"
  }
}
```

---

### 4.3 AUTH-003: Login Fallido (Evidencia de Seguridad)

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: AUTH-003 - Login Fallido                                      │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-ANON (Atacante potencial o usuario olvidadizo)    │
│ Sistemas: AuthService, SecurityService, NotificationService            │
│ Criticidad: CRÍTICA (Detección de ataques)                             │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                | Sistema             | Actor     | Evidencia              | Código Evidencia |
| ---- | ------- | --------------------- | ------------------- | --------- | ---------------------- | ---------------- |
| 1    | 1.1     | POST /api/auth/login  | Gateway             | USR-ANON  | **Request completo**   | EVD-AUDIT        |
| 2    | 2.1     | Verificar contraseña  | AuthService         | Sistema   | **Fallo registrado**   | EVD-AUDIT        |
| 2    | 2.2     | Incrementar contador  | AuthService         | Sistema   | Attempt count          | EVD-LOG          |
| 3    | 3.1     | Evaluar riesgo        | SecurityService     | Sistema   | **Risk assessment**    | EVD-AUDIT        |
| 3    | 3.2     | Si >3 intentos: alert | SecurityService     | Sistema   | Security alert         | EVD-EVENT        |
| 4    | 4.1     | Si >5 intentos: lock  | AuthService         | Sistema   | **Account locked**     | EVD-AUDIT        |
| 4    | 4.2     | Notificar al usuario  | NotificationService | SYS-NOTIF | **Email de seguridad** | EVD-COMM         |
| 5    | 5.1     | Log para análisis     | SecurityService     | Sistema   | Threat intelligence    | EVD-LOG          |

**Evidencia de Intento de Acceso No Autorizado:**

```json
{
  "processCode": "AUTH-003",
  "securityEvent": {
    "type": "FAILED_LOGIN_ATTEMPT",
    "severity": "MEDIUM",
    "target": {
      "email": "usuario@email.com",
      "userId": "550e8400-e29b-41d4-a716-446655440000"
    },
    "attacker": {
      "ip": "45.67.89.123",
      "geoLocation": {
        "country": "RU",
        "city": "Moscow"
      },
      "userAgent": "curl/7.68.0"
    },
    "attempt": {
      "number": 4,
      "totalLastHour": 12,
      "passwordUsed": "[REDACTED-HASH]"
    },
    "actions": [
      {
        "action": "RATE_LIMIT_APPLIED",
        "delay": "30s"
      },
      {
        "action": "ALERT_GENERATED",
        "alertId": "sec-alert-789"
      }
    ],
    "timestamp": "2026-01-21T10:30:00Z"
  }
}
```

---

### 4.4 AUTH-004: Two-Factor Authentication

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: AUTH-004 - Verificación 2FA                                   │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG (Usuario con 2FA habilitado)                  │
│ Sistemas: AuthService, NotificationService                             │
│ Criticidad: CRÍTICA                                                    │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                     | Sistema             | Actor     | Evidencia               | Código Evidencia |
| ---- | ------- | -------------------------- | ------------------- | --------- | ----------------------- | ---------------- |
| 1    | 1.1     | Generar código OTP         | AuthService         | Sistema   | **OTP generado (hash)** | EVD-AUDIT        |
| 1    | 1.2     | Almacenar en Redis         | AuthService         | Sistema   | TTL: 5 min              | EVD-LOG          |
| 2    | 2.1     | Enviar código por SMS      | NotificationService | SYS-NOTIF | **SMS enviado**         | EVD-COMM         |
| 2    | 2.2     | O enviar por email         | NotificationService | SYS-NOTIF | **Email enviado**       | EVD-COMM         |
| 3    | 3.1     | Usuario ingresa código     | Frontend            | USR-REG   | Input timestamp         | EVD-LOG          |
| 4    | 4.1     | POST /api/auth/verify-2fa  | Gateway             | USR-REG   | Request                 | EVD-AUDIT        |
| 4    | 4.2     | Verificar código           | AuthService         | Sistema   | **Verificación result** | EVD-AUDIT        |
| 5    | 5.1     | Si válido: completar login | AuthService         | Sistema   | Session created         | EVD-AUDIT        |
| 5    | 5.2     | Si inválido: registrar     | AuthService         | Sistema   | **Failed 2FA attempt**  | EVD-AUDIT        |

---

## 5. Procesos de Gestión de Usuarios

### 5.1 USR-001: Actualización de Perfil

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: USR-001 - Actualización de Perfil                             │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG, DLR-STAFF, DLR-ADMIN                         │
│ Sistemas: UserService, MediaService, AuditService                      │
│ Criticidad: MEDIA                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                       | Sistema      | Actor   | Evidencia               | Código Evidencia |
| ---- | ------- | ---------------------------- | ------------ | ------- | ----------------------- | ---------------- |
| 1    | 1.1     | GET /api/users/me            | UserService  | USR-REG | Request log             | EVD-LOG          |
| 1    | 1.2     | Renderizar perfil actual     | Frontend     | Sistema | -                       | -                |
| 2    | 2.1     | Usuario modifica datos       | Frontend     | USR-REG | Cambios tracked         | EVD-LOG          |
| 3    | 3.1     | PUT /api/users/me            | Gateway      | USR-REG | **Request con cambios** | EVD-AUDIT        |
| 3    | 3.2     | Validar payload              | UserService  | Sistema | Validation log          | EVD-LOG          |
| 4    | 4.1     | **Snapshot estado anterior** | UserService  | Sistema | **Before state**        | EVD-SNAP         |
| 4    | 4.2     | Aplicar cambios              | UserService  | Sistema | Query execution         | EVD-LOG          |
| 4    | 4.3     | **Snapshot estado nuevo**    | UserService  | Sistema | **After state**         | EVD-SNAP         |
| 5    | 5.1     | **Calcular diff**            | AuditService | Sistema | **Changes diff**        | EVD-AUDIT        |
| 5    | 5.2     | Registrar audit trail        | AuditService | Sistema | Audit entry             | EVD-AUDIT        |
| 6    | 6.1     | Publicar UserUpdatedEvent    | RabbitMQ     | Sistema | Event published         | EVD-EVENT        |

**Evidencia de Cambio con Diff:**

```json
{
  "processCode": "USR-001",
  "auditEntry": {
    "action": "UPDATE",
    "resourceType": "User",
    "resourceId": "550e8400-e29b-41d4-a716-446655440000",
    "actor": {
      "type": "USR-REG",
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Juan Pérez"
    },
    "before": {
      "firstName": "Juan",
      "lastName": "Perez",
      "phone": "+18095551234"
    },
    "after": {
      "firstName": "Juan Carlos",
      "lastName": "Pérez",
      "phone": "+18095551234"
    },
    "changes": [
      {
        "field": "firstName",
        "from": "Juan",
        "to": "Juan Carlos"
      },
      {
        "field": "lastName",
        "from": "Perez",
        "to": "Pérez"
      }
    ],
    "timestamp": "2026-01-21T10:30:00Z"
  }
}
```

---

### 5.2 USR-002: Cambio de Contraseña

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: USR-002 - Cambio de Contraseña                                │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG, DLR-STAFF, DLR-ADMIN                         │
│ Sistemas: AuthService, NotificationService, AuditService               │
│ Criticidad: CRÍTICA                                                    │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                         | Sistema             | Actor     | Evidencia               | Código Evidencia |
| ---- | ------- | ------------------------------ | ------------------- | --------- | ----------------------- | ---------------- |
| 1    | 1.1     | POST /api/auth/change-password | Gateway             | USR-REG   | Request (sin password)  | EVD-AUDIT        |
| 2    | 2.1     | Verificar password actual      | AuthService         | Sistema   | **Verificación result** | EVD-AUDIT        |
| 2    | 2.2     | Si falla: registrar intento    | AuthService         | Sistema   | Failed attempt          | EVD-AUDIT        |
| 3    | 3.1     | Validar nuevo password         | AuthService         | Sistema   | Policy check            | EVD-LOG          |
| 3    | 3.2     | Verificar no repetido          | AuthService         | Sistema   | History check           | EVD-LOG          |
| 4    | 4.1     | Hash nuevo password            | AuthService         | Sistema   | Algorithm log           | EVD-LOG          |
| 4    | 4.2     | Actualizar en DB               | AuthService         | Sistema   | **Password changed**    | EVD-AUDIT        |
| 5    | 5.1     | Invalidar todas las sesiones   | AuthService         | Sistema   | **Sessions revoked**    | EVD-AUDIT        |
| 5    | 5.2     | Excepto sesión actual          | AuthService         | Sistema   | Current session kept    | EVD-LOG          |
| 6    | 6.1     | **Notificar cambio**           | NotificationService | SYS-NOTIF | **Security email**      | EVD-COMM         |
| 7    | 7.1     | Audit trail                    | AuditService        | Sistema   | Complete entry          | EVD-AUDIT        |

**Evidencia de Cambio de Contraseña:**

```json
{
  "processCode": "USR-002",
  "securityEvent": {
    "type": "PASSWORD_CHANGED",
    "severity": "HIGH",
    "actor": {
      "type": "USR-REG",
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "usuario@email.com"
    },
    "action": {
      "previousPasswordAge": "45 days",
      "newPasswordStrength": "Strong",
      "sessionsRevoked": 3,
      "currentSessionRetained": true
    },
    "notification": {
      "sent": true,
      "channel": "Email",
      "messageId": "msg-12345"
    },
    "request": {
      "ip": "190.52.xx.xx",
      "device": "Chrome on Windows"
    },
    "timestamp": "2026-01-21T10:30:00Z"
  }
}
```

---

### 5.3 USR-003: Eliminación de Cuenta (Ley 172-13)

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: USR-003 - Eliminación de Cuenta (Derecho al Olvido)           │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG (Titular de datos)                            │
│ Sistemas: UserService, ComplianceService, Multiple Services            │
│ Criticidad: CRÍTICA (Regulatorio)                                      │
│ Marco Legal: Ley 172-13 Art. 35 - Derecho de Cancelación               │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                         | Sistema             | Actor     | Evidencia                | Código Evidencia |
| ---- | ------- | ------------------------------ | ------------------- | --------- | ------------------------ | ---------------- |
| 1    | 1.1     | Usuario solicita eliminación   | Frontend            | USR-REG   | **Solicitud registrada** | EVD-AUDIT        |
| 1    | 1.2     | Verificar identidad (2FA)      | AuthService         | USR-REG   | Identity verified        | EVD-AUDIT        |
| 2    | 2.1     | **Capturar consentimiento**    | Frontend            | USR-REG   | **Consent signature**    | EVD-SIGN         |
| 2    | 2.2     | Hash del consentimiento        | ComplianceService   | Sistema   | Consent hash             | EVD-HASH         |
| 3    | 3.1     | Crear DeleteRequest            | ComplianceService   | Sistema   | Request created          | EVD-AUDIT        |
| 3    | 3.2     | Período de gracia 14 días      | ComplianceService   | Sistema   | Grace period start       | EVD-LOG          |
| 4    | 4.1     | Notificar inicio proceso       | NotificationService | SYS-NOTIF | **Email confirmación**   | EVD-COMM         |
| 5    | 5.1     | [Día 14] Ejecutar eliminación  | SYS-SCHEDULER       | Sistema   | Job execution            | EVD-LOG          |
| 5    | 5.2     | **Snapshot final completo**    | Multiple            | Sistema   | **Full data export**     | EVD-SNAP         |
| 6    | 6.1     | Anonimizar datos personales    | UserService         | Sistema   | **Anonymization log**    | EVD-AUDIT        |
| 6    | 6.2     | Mantener datos transaccionales | BillingService      | Sistema   | Transactions kept        | EVD-LOG          |
| 6    | 6.3     | Eliminar media personal        | MediaService        | Sistema   | Files deleted            | EVD-AUDIT        |
| 7    | 7.1     | **Certificado de eliminación** | ComplianceService   | Sistema   | **Deletion certificate** | EVD-DOC          |
| 7    | 7.2     | Enviar certificado             | NotificationService | SYS-NOTIF | Email with PDF           | EVD-COMM         |

**Certificado de Eliminación (EVD-DOC):**

```json
{
  "processCode": "USR-003",
  "deletionCertificate": {
    "certificateId": "DEL-2026-001234",
    "requestDate": "2026-01-07T10:30:00Z",
    "executionDate": "2026-01-21T00:00:00Z",
    "subject": {
      "originalId": "550e8400-e29b-41d4-a716-446655440000",
      "email": "[ELIMINADO]",
      "anonymizedId": "ANON-7f8a9b2c"
    },
    "dataDeleted": {
      "personalInfo": true,
      "profilePhoto": true,
      "savedSearches": true,
      "favorites": true,
      "messages": true
    },
    "dataRetained": {
      "transactions": {
        "retained": true,
        "reason": "Obligación fiscal DGII - 10 años",
        "anonymized": true
      },
      "auditTrail": {
        "retained": true,
        "reason": "Ley 155-17 - Anti-lavado",
        "anonymized": true
      }
    },
    "legalBasis": "Ley 172-13 Art. 35",
    "hash": "sha256:abc123...",
    "signature": {
      "signedBy": "ComplianceService",
      "timestamp": "2026-01-21T00:00:01Z"
    }
  }
}
```

---

## 6. Procesos de Dealers

### 6.1 DLR-001: Registro de Dealer

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: DLR-001 - Registro de Dealer                                  │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG (Empresario/Concesionario)                    │
│ Sistemas: DealerManagementService, ComplianceService, BillingService   │
│ Duración: 24-48 horas (incluye verificación)                           │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                   | Sistema             | Actor   | Evidencia                | Código Evidencia |
| ---- | ------- | ------------------------ | ------------------- | ------- | ------------------------ | ---------------- |
| 1    | 1.1     | POST /api/dealers        | Gateway             | USR-REG | **Solicitud registrada** | EVD-AUDIT        |
| 1    | 1.2     | Validar datos básicos    | DealerMgmtSvc       | Sistema | Validation log           | EVD-LOG          |
| 2    | 2.1     | Verificar RNC en DGII    | ComplianceService   | Sistema | **DGII query result**    | EVD-AUDIT        |
| 2    | 2.2     | Almacenar respuesta DGII | ComplianceService   | Sistema | DGII response            | EVD-DOC          |
| 3    | 3.1     | Crear Dealer (Pending)   | DealerMgmtSvc       | Sistema | **Dealer created**       | EVD-EVENT        |
| 3    | 3.2     | Snapshot estado inicial  | DealerMgmtSvc       | Sistema | Initial state            | EVD-SNAP         |
| 4    | 4.1     | Usuario sube documentos  | MediaService        | USR-REG | **Documents uploaded**   | EVD-FILE         |
| 4    | 4.2     | Hash de cada documento   | MediaService        | Sistema | Document hashes          | EVD-HASH         |
| 5    | 5.1     | Notificar a Compliance   | NotificationService | Sistema | Internal alert           | EVD-COMM         |
| 5    | 5.2     | Crear task de revisión   | AdminService        | Sistema | Task created             | EVD-LOG          |
| 6    | 6.1     | **Audit trail completo** | AuditService        | Sistema | Full audit               | EVD-AUDIT        |

---

### 6.2 DLR-002: Verificación de Dealer (Admin)

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: DLR-002 - Verificación de Dealer                              │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: ADM-COMP (Compliance Officer)                         │
│ Sistemas: DealerManagementService, ComplianceService, NotificationSvc  │
│ Duración: 1-24 horas                                                   │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                         | Sistema             | Actor     | Evidencia               | Código Evidencia |
| ---- | ------- | ------------------------------ | ------------------- | --------- | ----------------------- | ---------------- |
| 1    | 1.1     | Admin accede a revisión        | Frontend            | ADM-COMP  | Access log              | EVD-LOG          |
| 1    | 1.2     | Cargar documentos dealer       | MediaService        | Sistema   | Documents viewed        | EVD-AUDIT        |
| 2    | 2.1     | Verificar RNC                  | ComplianceService   | ADM-COMP  | **Manual verification** | EVD-AUDIT        |
| 2    | 2.2     | Verificar licencia comercial   | ComplianceService   | ADM-COMP  | Document check          | EVD-AUDIT        |
| 2    | 2.3     | Verificar dirección            | ComplianceService   | ADM-COMP  | Address check           | EVD-AUDIT        |
| 3    | 3.1     | **Decisión: Aprobar/Rechazar** | DealerMgmtSvc       | ADM-COMP  | **Decision logged**     | EVD-AUDIT        |
| 3    | 3.2     | Registrar razón                | DealerMgmtSvc       | ADM-COMP  | Decision reason         | EVD-AUDIT        |
| 4    | 4.1     | Si aprobado: activar           | DealerMgmtSvc       | Sistema   | **Dealer activated**    | EVD-EVENT        |
| 4    | 4.2     | Actualizar status              | DealerMgmtSvc       | Sistema   | Status change           | EVD-SNAP         |
| 5    | 5.1     | **Notificar al dealer**        | NotificationService | SYS-NOTIF | **Email resultado**     | EVD-COMM         |
| 6    | 6.1     | Si rechazado: documentar       | DealerMgmtSvc       | ADM-COMP  | Rejection docs          | EVD-AUDIT        |
| 6    | 6.2     | Indicar mejoras necesarias     | DealerMgmtSvc       | ADM-COMP  | Requirements            | EVD-DOC          |

**Evidencia de Decisión de Verificación:**

```json
{
  "processCode": "DLR-002",
  "verificationDecision": {
    "dealerId": "dlr-12345",
    "dealerName": "Auto Express SRL",
    "rnc": "130123456",
    "decision": "APPROVED",
    "verifiedBy": {
      "type": "ADM-COMP",
      "id": "adm-001",
      "name": "María Compliance"
    },
    "verification": {
      "rncValid": true,
      "rncStatus": "ACTIVO",
      "licenseValid": true,
      "licenseExpiry": "2027-12-31",
      "addressVerified": true,
      "documentsComplete": true
    },
    "documents": [
      {
        "type": "RNC_CERTIFICATE",
        "hash": "sha256:abc...",
        "verified": true
      },
      {
        "type": "BUSINESS_LICENSE",
        "hash": "sha256:def...",
        "verified": true
      }
    ],
    "notes": "Documentación completa, RNC activo, licencia vigente",
    "timestamp": "2026-01-21T14:30:00Z"
  }
}
```

---

### 6.3 DLR-003: Suscripción de Dealer

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: DLR-003 - Suscripción de Dealer                               │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: DLR-ADMIN (Administrador del Dealer)                  │
│ Sistemas: BillingService, DealerManagementService, NotificationService │
│ Criticidad: ALTA (Financiero)                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                  | Sistema             | Actor     | Evidencia                | Código Evidencia |
| ---- | ------- | ----------------------- | ------------------- | --------- | ------------------------ | ---------------- |
| 1    | 1.1     | Seleccionar plan        | Frontend            | DLR-ADMIN | Plan selection           | EVD-LOG          |
| 1    | 1.2     | POST /api/subscriptions | Gateway             | DLR-ADMIN | **Subscription request** | EVD-AUDIT        |
| 2    | 2.1     | Crear checkout session  | BillingService      | Sistema   | Session created          | EVD-LOG          |
| 2    | 2.2     | Redirect a pasarela     | BillingService      | Sistema   | Redirect log             | EVD-LOG          |
| 3    | 3.1     | Usuario completa pago   | Stripe/Azul         | DLR-ADMIN | **Payment attempt**      | EVD-AUDIT        |
| 3    | 3.2     | Pasarela procesa        | Stripe/Azul         | Sistema   | Gateway log              | EVD-LOG          |
| 4    | 4.1     | Webhook recibido        | BillingService      | Sistema   | **Webhook received**     | EVD-AUDIT        |
| 4    | 4.2     | Verificar firma webhook | BillingService      | Sistema   | Signature valid          | EVD-LOG          |
| 5    | 5.1     | Crear Subscription      | BillingService      | Sistema   | **Subscription created** | EVD-EVENT        |
| 5    | 5.2     | Actualizar dealer plan  | DealerMgmtSvc       | Sistema   | Plan activated           | EVD-SNAP         |
| 6    | 6.1     | **Generar factura**     | BillingService      | Sistema   | **Invoice generated**    | EVD-DOC          |
| 6    | 6.2     | Generar NCF             | BillingService      | Sistema   | **NCF assigned**         | EVD-DOC          |
| 7    | 7.1     | **Enviar factura**      | NotificationService | SYS-NOTIF | **Invoice email**        | EVD-COMM         |
| 8    | 8.1     | Audit trail             | AuditService        | Sistema   | Complete audit           | EVD-AUDIT        |

**Evidencia de Transacción de Suscripción:**

```json
{
  "processCode": "DLR-003",
  "subscription": {
    "id": "sub-12345",
    "dealerId": "dlr-12345",
    "plan": "PRO",
    "billing": {
      "amount": 129.0,
      "currency": "USD",
      "amountDOP": 7740.0,
      "exchangeRate": 60.0
    },
    "payment": {
      "gateway": "STRIPE",
      "transactionId": "pi_3abc123",
      "last4": "4242",
      "brand": "VISA"
    },
    "invoice": {
      "invoiceId": "INV-2026-001234",
      "ncf": "B0100000001",
      "ncfType": "CREDITO_FISCAL"
    },
    "period": {
      "start": "2026-01-21",
      "end": "2026-02-20"
    },
    "earlyBird": {
      "applied": true,
      "discount": 20,
      "freeMonths": 3
    }
  },
  "timestamp": "2026-01-21T10:30:00Z"
}
```

---

## 7. Procesos de Vehículos e Inventario

### 7.1 VEH-001: Publicación de Vehículo

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: VEH-001 - Publicación de Vehículo                             │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: DLR-STAFF, DLR-ADMIN, USR-SELLER                      │
│ Sistemas: VehiclesSaleService, MediaService, ModerationService         │
│ Criticidad: MEDIA                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                       | Sistema       | Actor     | Evidencia            | Código Evidencia |
| ---- | ------- | ---------------------------- | ------------- | --------- | -------------------- | ---------------- |
| 1    | 1.1     | POST /api/vehicles           | Gateway       | DLR-STAFF | **Request completo** | EVD-AUDIT        |
| 1    | 1.2     | Validar payload              | VehiclesSvc   | Sistema   | Validation log       | EVD-LOG          |
| 2    | 2.1     | Verificar límite listings    | VehiclesSvc   | Sistema   | Limit check          | EVD-LOG          |
| 2    | 2.2     | Verificar dealer activo      | VehiclesSvc   | Sistema   | Dealer status        | EVD-LOG          |
| 3    | 3.1     | Crear Vehicle (Draft)        | VehiclesSvc   | Sistema   | **Vehicle created**  | EVD-EVENT        |
| 3    | 3.2     | Snapshot estado inicial      | VehiclesSvc   | Sistema   | Initial state        | EVD-SNAP         |
| 4    | 4.1     | Upload de imágenes           | MediaService  | DLR-STAFF | **Images uploaded**  | EVD-FILE         |
| 4    | 4.2     | Hash de cada imagen          | MediaService  | Sistema   | Image hashes         | EVD-HASH         |
| 4    | 4.3     | Procesar (resize, watermark) | MediaService  | Sistema   | Processing log       | EVD-LOG          |
| 5    | 5.1     | Enviar a moderación          | ModerationSvc | Sistema   | **Moderation queue** | EVD-AUDIT        |
| 5    | 5.2     | AI check automático          | ModerationSvc | Sistema   | AI result            | EVD-LOG          |
| 6    | 6.1     | Si auto-approve: activar     | VehiclesSvc   | Sistema   | Auto-activated       | EVD-EVENT        |
| 6    | 6.2     | Si requiere review: queue    | ModerationSvc | Sistema   | Pending review       | EVD-LOG          |
| 7    | 7.1     | Indexar en Elasticsearch     | SearchService | Sistema   | Indexed              | EVD-LOG          |
| 8    | 8.1     | Audit trail                  | AuditService  | Sistema   | Complete audit       | EVD-AUDIT        |

**Evidencia de Publicación:**

```json
{
  "processCode": "VEH-001",
  "listing": {
    "vehicleId": "veh-12345",
    "dealerId": "dlr-12345",
    "createdBy": {
      "type": "DLR-STAFF",
      "id": "user-789",
      "name": "Pedro Vendedor"
    },
    "vehicle": {
      "make": "Toyota",
      "model": "Corolla",
      "year": 2024,
      "price": 1850000,
      "currency": "DOP"
    },
    "media": {
      "images": [
        {
          "id": "img-001",
          "hash": "sha256:abc...",
          "size": 2048576
        }
      ],
      "totalImages": 12
    },
    "moderation": {
      "aiScore": 0.95,
      "autoApproved": true,
      "reason": "High confidence, verified dealer"
    },
    "status": "ACTIVE",
    "timestamp": "2026-01-21T10:30:00Z"
  }
}
```

---

### 7.2 VEH-002: Moderación de Vehículo

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: VEH-002 - Moderación de Vehículo                              │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: ADM-MOD (Moderador) o SYS-ML (Auto-moderación)        │
│ Sistemas: ModerationService, VehiclesSaleService, NotificationService  │
│ Criticidad: MEDIA                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                     | Sistema             | Actor     | Evidencia               | Código Evidencia |
| ---- | ------- | -------------------------- | ------------------- | --------- | ----------------------- | ---------------- |
| 1    | 1.1     | Cargar listing para review | Frontend            | ADM-MOD   | Access log              | EVD-LOG          |
| 1    | 1.2     | Ver imágenes y datos       | Frontend            | ADM-MOD   | View log                | EVD-LOG          |
| 2    | 2.1     | Verificar imágenes         | ModerationSvc       | ADM-MOD   | **Image review**        | EVD-AUDIT        |
| 2    | 2.2     | Verificar precio razonable | ModerationSvc       | ADM-MOD   | Price check             | EVD-LOG          |
| 2    | 2.3     | Verificar descripción      | ModerationSvc       | ADM-MOD   | Content check           | EVD-LOG          |
| 3    | 3.1     | **Decisión**               | ModerationSvc       | ADM-MOD   | **Moderation decision** | EVD-AUDIT        |
| 3    | 3.2     | Registrar razón            | ModerationSvc       | ADM-MOD   | Decision reason         | EVD-AUDIT        |
| 4    | 4.1     | Si aprobado: activar       | VehiclesSvc         | Sistema   | **Vehicle activated**   | EVD-EVENT        |
| 4    | 4.2     | Actualizar status          | VehiclesSvc         | Sistema   | Status change           | EVD-SNAP         |
| 5    | 5.1     | Si rechazado: documentar   | ModerationSvc       | ADM-MOD   | Rejection docs          | EVD-AUDIT        |
| 5    | 5.2     | Indicar problemas          | ModerationSvc       | ADM-MOD   | Issues list             | EVD-DOC          |
| 6    | 6.1     | **Notificar al vendedor**  | NotificationService | SYS-NOTIF | **Result email**        | EVD-COMM         |
| 7    | 7.1     | Audit trail                | AuditService        | Sistema   | Complete audit          | EVD-AUDIT        |

**Evidencia de Decisión de Moderación:**

```json
{
  "processCode": "VEH-002",
  "moderation": {
    "vehicleId": "veh-12345",
    "moderator": {
      "type": "ADM-MOD",
      "id": "mod-001",
      "name": "Ana Moderadora"
    },
    "decision": "APPROVED",
    "checks": {
      "images": {
        "passed": true,
        "score": 0.98,
        "issues": []
      },
      "price": {
        "passed": true,
        "marketAverage": 1900000,
        "deviation": -2.6
      },
      "content": {
        "passed": true,
        "flaggedWords": []
      }
    },
    "duration": "45 seconds",
    "notes": "Listing completo, imágenes claras, precio competitivo",
    "timestamp": "2026-01-21T10:35:00Z"
  }
}
```

---

### 7.3 VEH-003: Cambio de Precio

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: VEH-003 - Cambio de Precio                                    │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: DLR-STAFF, DLR-ADMIN, USR-SELLER                      │
│ Sistemas: VehiclesSaleService, AlertService, AnalyticsService          │
│ Criticidad: BAJA (pero auditable por competencia desleal)              │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                          | Sistema          | Actor     | Evidencia                | Código Evidencia |
| ---- | ------- | ------------------------------- | ---------------- | --------- | ------------------------ | ---------------- |
| 1    | 1.1     | PUT /api/vehicles/{id}/price    | Gateway          | DLR-STAFF | **Price change request** | EVD-AUDIT        |
| 2    | 2.1     | **Snapshot precio anterior**    | VehiclesSvc      | Sistema   | **Before price**         | EVD-SNAP         |
| 2    | 2.2     | Actualizar precio               | VehiclesSvc      | Sistema   | Update query             | EVD-LOG          |
| 2    | 2.3     | **Snapshot precio nuevo**       | VehiclesSvc      | Sistema   | **After price**          | EVD-SNAP         |
| 3    | 3.1     | Registrar en historial          | VehiclesSvc      | Sistema   | **Price history**        | EVD-AUDIT        |
| 3    | 3.2     | Calcular % cambio               | VehiclesSvc      | Sistema   | Change percentage        | EVD-LOG          |
| 4    | 4.1     | Publicar PriceChangedEvent      | RabbitMQ         | Sistema   | Event published          | EVD-EVENT        |
| 5    | 5.1     | Notificar a usuarios con alerta | AlertService     | Sistema   | **Alerts triggered**     | EVD-COMM         |
| 6    | 6.1     | Actualizar analytics            | AnalyticsService | Sistema   | Stats updated            | EVD-LOG          |
| 7    | 7.1     | Audit trail                     | AuditService     | Sistema   | Complete audit           | EVD-AUDIT        |

**Historial de Precios (Evidencia para análisis de competencia):**

```json
{
  "processCode": "VEH-003",
  "priceHistory": {
    "vehicleId": "veh-12345",
    "changes": [
      {
        "date": "2026-01-01",
        "price": 2000000,
        "changedBy": "user-789",
        "reason": "Publicación inicial"
      },
      {
        "date": "2026-01-15",
        "price": 1950000,
        "change": -2.5,
        "changedBy": "user-789",
        "reason": "Ajuste de mercado"
      },
      {
        "date": "2026-01-21",
        "price": 1850000,
        "change": -5.1,
        "changedBy": "user-789",
        "reason": "Promoción de enero"
      }
    ],
    "alertsTriggered": 15,
    "usersNotified": ["user-001", "user-002"]
  }
}
```

---

## 8. Procesos de Pagos y Facturación

### 8.1 PAY-001: Pago de Listing Individual

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: PAY-001 - Pago de Listing Individual                          │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-SELLER (Vendedor individual)                      │
│ Sistemas: BillingService, VehiclesSaleService, NotificationService     │
│ Criticidad: ALTA (Financiero + Fiscal)                                 │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                       | Sistema             | Actor      | Evidencia            | Código Evidencia |
| ---- | ------- | ---------------------------- | ------------------- | ---------- | -------------------- | ---------------- |
| 1    | 1.1     | POST /api/payments/listing   | Gateway             | USR-SELLER | **Payment request**  | EVD-AUDIT        |
| 1    | 1.2     | Validar listing exists       | VehiclesSvc         | Sistema    | Listing validated    | EVD-LOG          |
| 2    | 2.1     | Calcular monto               | BillingService      | Sistema    | Amount calculated    | EVD-LOG          |
| 2    | 2.2     | Aplicar impuestos (ITBIS)    | BillingService      | Sistema    | **Tax calculation**  | EVD-AUDIT        |
| 3    | 3.1     | Crear payment intent         | BillingService      | Sistema    | Intent created       | EVD-LOG          |
| 3    | 3.2     | Seleccionar pasarela         | BillingService      | Sistema    | Gateway selected     | EVD-LOG          |
| 4    | 4.1     | Redirect a checkout          | BillingService      | Sistema    | Redirect log         | EVD-LOG          |
| 4    | 4.2     | **Usuario completa pago**    | Stripe/Azul         | USR-SELLER | **Payment captured** | EVD-AUDIT        |
| 5    | 5.1     | Webhook payment.success      | BillingService      | Sistema    | **Webhook received** | EVD-AUDIT        |
| 5    | 5.2     | Verificar idempotencia       | BillingService      | Sistema    | Idempotency check    | EVD-LOG          |
| 6    | 6.1     | Crear Payment record         | BillingService      | Sistema    | **Payment created**  | EVD-EVENT        |
| 6    | 6.2     | **Generar NCF**              | BillingService      | Sistema    | **NCF assigned**     | EVD-DOC          |
| 6    | 6.3     | **Generar factura PDF**      | BillingService      | Sistema    | **Invoice PDF**      | EVD-DOC          |
| 7    | 7.1     | Activar listing              | VehiclesSvc         | Sistema    | Listing activated    | EVD-EVENT        |
| 7    | 7.2     | Actualizar status            | VehiclesSvc         | Sistema    | Status change        | EVD-SNAP         |
| 8    | 8.1     | **Enviar factura por email** | NotificationService | SYS-NOTIF  | **Invoice sent**     | EVD-COMM         |
| 9    | 9.1     | Registrar para DGII          | ComplianceService   | Sistema    | **DGII record**      | EVD-AUDIT        |
| 10   | 10.1    | Audit trail completo         | AuditService        | Sistema    | Complete audit       | EVD-AUDIT        |

**Evidencia Fiscal Completa:**

```json
{
  "processCode": "PAY-001",
  "fiscalRecord": {
    "paymentId": "pay-12345",
    "invoiceId": "INV-2026-005678",
    "ncf": {
      "number": "B0200000456",
      "type": "CONSUMIDOR_FINAL",
      "sequence": 456,
      "validUntil": "2027-12-31"
    },
    "customer": {
      "type": "INDIVIDUAL",
      "cedula": "001-1234567-8",
      "name": "Juan Pérez"
    },
    "items": [
      {
        "description": "Publicación de Vehículo - 30 días",
        "quantity": 1,
        "unitPrice": 24.58,
        "subtotal": 24.58,
        "itbis": 4.42,
        "total": 29.0
      }
    ],
    "totals": {
      "subtotal": 24.58,
      "itbis": 4.42,
      "total": 29.0,
      "currency": "USD",
      "totalDOP": 1740.0,
      "exchangeRate": 60.0
    },
    "payment": {
      "method": "CREDIT_CARD",
      "gateway": "AZUL",
      "transactionId": "azul-tx-789",
      "authCode": "123456",
      "last4": "1234",
      "brand": "MASTERCARD"
    },
    "dgii": {
      "reportable": true,
      "format607Line": "00112345678|1|B0200000456||01|20260121||2900|522|0|0|0|0|0|0|0|0|0|0|0|2900|0|0|0"
    },
    "timestamp": "2026-01-21T10:30:00Z"
  }
}
```

---

### 8.2 PAY-002: Reembolso

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: PAY-002 - Reembolso                                           │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: ADM-SUPPORT (Soporte) o SYS-BILLING (Automático)      │
│ Sistemas: BillingService, VehiclesSaleService, NotificationService     │
│ Criticidad: ALTA (Financiero + Fiscal)                                 │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                      | Sistema             | Actor       | Evidencia           | Código Evidencia |
| ---- | ------- | --------------------------- | ------------------- | ----------- | ------------------- | ---------------- |
| 1    | 1.1     | POST /api/refunds           | Gateway             | ADM-SUPPORT | **Refund request**  | EVD-AUDIT        |
| 1    | 1.2     | Validar motivo              | BillingService      | ADM-SUPPORT | Reason documented   | EVD-AUDIT        |
| 2    | 2.1     | Verificar pago original     | BillingService      | Sistema     | Original payment    | EVD-LOG          |
| 2    | 2.2     | Verificar elegibilidad      | BillingService      | Sistema     | Eligibility check   | EVD-LOG          |
| 3    | 3.1     | **Aprobar reembolso**       | BillingService      | ADM-SUPPORT | **Approval logged** | EVD-AUDIT        |
| 3    | 3.2     | Calcular monto a devolver   | BillingService      | Sistema     | Amount calculated   | EVD-LOG          |
| 4    | 4.1     | Ejecutar refund en gateway  | Stripe/Azul         | Sistema     | **Refund executed** | EVD-AUDIT        |
| 4    | 4.2     | Respuesta de gateway        | Stripe/Azul         | Sistema     | Gateway response    | EVD-LOG          |
| 5    | 5.1     | Crear Refund record         | BillingService      | Sistema     | **Refund created**  | EVD-EVENT        |
| 5    | 5.2     | **Generar Nota de Crédito** | BillingService      | Sistema     | **Credit note**     | EVD-DOC          |
| 5    | 5.3     | **NCF de NC**               | BillingService      | Sistema     | **NCF NC assigned** | EVD-DOC          |
| 6    | 6.1     | Desactivar listing          | VehiclesSvc         | Sistema     | Listing deactivated | EVD-EVENT        |
| 7    | 7.1     | **Notificar al usuario**    | NotificationService | SYS-NOTIF   | **Refund email**    | EVD-COMM         |
| 8    | 8.1     | Registrar para DGII         | ComplianceService   | Sistema     | **DGII NC record**  | EVD-AUDIT        |
| 9    | 9.1     | Audit trail                 | AuditService        | Sistema     | Complete audit      | EVD-AUDIT        |

**Evidencia de Nota de Crédito:**

```json
{
  "processCode": "PAY-002",
  "creditNote": {
    "creditNoteId": "NC-2026-000123",
    "originalInvoice": "INV-2026-005678",
    "originalNcf": "B0200000456",
    "ncf": {
      "number": "B0400000123",
      "type": "NOTA_CREDITO",
      "referencedNcf": "B0200000456"
    },
    "reason": "CUSTOMER_REQUEST",
    "reasonDetail": "Cliente solicitó cancelación dentro del período de garantía",
    "approvedBy": {
      "type": "ADM-SUPPORT",
      "id": "support-001",
      "name": "Carlos Soporte"
    },
    "amounts": {
      "originalAmount": 29.0,
      "refundAmount": 29.0,
      "partial": false
    },
    "refund": {
      "gateway": "AZUL",
      "transactionId": "azul-refund-456",
      "status": "COMPLETED"
    },
    "timestamp": "2026-01-21T14:30:00Z"
  }
}
```

---

## 9. Procesos de CRM y Leads

### 9.1 CRM-001: Creación de Lead

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: CRM-001 - Creación de Lead                                    │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG (Comprador interesado)                        │
│ Sistemas: CRMService, ContactService, NotificationService, MLService   │
│ Criticidad: MEDIA                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                        | Sistema             | Actor     | Evidencia            | Código Evidencia |
| ---- | ------- | ----------------------------- | ------------------- | --------- | -------------------- | ---------------- |
| 1    | 1.1     | Usuario hace clic "Contactar" | Frontend            | USR-REG   | Click tracked        | EVD-LOG          |
| 1    | 1.2     | POST /api/leads               | Gateway             | USR-REG   | **Lead request**     | EVD-AUDIT        |
| 2    | 2.1     | Validar usuario y vehículo    | CRMService          | Sistema   | Validation           | EVD-LOG          |
| 2    | 2.2     | Verificar duplicados          | CRMService          | Sistema   | Duplicate check      | EVD-LOG          |
| 3    | 3.1     | Crear Lead                    | CRMService          | Sistema   | **Lead created**     | EVD-EVENT        |
| 3    | 3.2     | Snapshot inicial              | CRMService          | Sistema   | Lead state           | EVD-SNAP         |
| 4    | 4.1     | **Lead Scoring IA**           | MLService           | SYS-ML    | **Score calculated** | EVD-AUDIT        |
| 4    | 4.2     | Asignar categoría             | CRMService          | Sistema   | Category assigned    | EVD-LOG          |
| 5    | 5.1     | Crear mensaje inicial         | ContactService      | Sistema   | Message created      | EVD-LOG          |
| 5    | 5.2     | Notificar al vendedor         | NotificationService | SYS-NOTIF | **Seller notified**  | EVD-COMM         |
| 6    | 6.1     | Si WhatsApp activo: enviar    | NotificationService | SYS-NOTIF | **WhatsApp sent**    | EVD-COMM         |
| 7    | 7.1     | Audit trail                   | AuditService        | Sistema   | Complete audit       | EVD-AUDIT        |

**Evidencia de Lead con Score:**

```json
{
  "processCode": "CRM-001",
  "lead": {
    "leadId": "lead-12345",
    "source": "VEHICLE_CONTACT_FORM",
    "buyer": {
      "userId": "user-001",
      "name": "María Compradora",
      "phone": "+18095551234",
      "email": "maria@email.com"
    },
    "vehicle": {
      "vehicleId": "veh-12345",
      "make": "Toyota",
      "model": "Corolla",
      "price": 1850000
    },
    "seller": {
      "dealerId": "dlr-12345",
      "type": "DEALER"
    },
    "scoring": {
      "score": 85,
      "category": "HOT",
      "factors": [
        { "factor": "FINANCING_INTENT", "weight": 25 },
        { "factor": "IMMEDIATE_PURCHASE", "weight": 30 },
        { "factor": "BUDGET_MATCH", "weight": 20 },
        { "factor": "ENGAGEMENT_HISTORY", "weight": 10 }
      ],
      "model": "lead-scoring-v2.1",
      "confidence": 0.92
    },
    "message": "Estoy interesada en este Corolla. ¿Está disponible para prueba de manejo?",
    "notifications": {
      "email": { "sent": true, "messageId": "msg-123" },
      "whatsapp": { "sent": true, "messageId": "wa-456" },
      "push": { "sent": true, "deviceId": "dev-789" }
    },
    "timestamp": "2026-01-21T10:30:00Z"
  }
}
```

---

### 9.2 CRM-002: Seguimiento de Lead

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: CRM-002 - Seguimiento de Lead                                 │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: DLR-STAFF (Vendedor del dealer)                       │
│ Sistemas: CRMService, ContactService, NotificationService              │
│ Criticidad: MEDIA                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                 | Sistema        | Actor     | Evidencia          | Código Evidencia |
| ---- | ------- | ---------------------- | -------------- | --------- | ------------------ | ---------------- |
| 1    | 1.1     | Vendedor abre lead     | Frontend       | DLR-STAFF | Lead viewed        | EVD-LOG          |
| 1    | 1.2     | GET /api/leads/{id}    | CRMService     | DLR-STAFF | Access logged      | EVD-AUDIT        |
| 2    | 2.1     | Actualizar status      | CRMService     | DLR-STAFF | **Status change**  | EVD-AUDIT        |
| 2    | 2.2     | Snapshot antes/después | CRMService     | Sistema   | State change       | EVD-SNAP         |
| 3    | 3.1     | Agregar nota/actividad | CRMService     | DLR-STAFF | **Activity added** | EVD-AUDIT        |
| 3    | 3.2     | Programar follow-up    | CRMService     | DLR-STAFF | Reminder set       | EVD-LOG          |
| 4    | 4.1     | Enviar mensaje         | ContactService | DLR-STAFF | **Message sent**   | EVD-COMM         |
| 4    | 4.2     | Log de comunicación    | ContactService | Sistema   | Comm log           | EVD-AUDIT        |
| 5    | 5.1     | Actualizar scoring     | MLService      | SYS-ML    | Score updated      | EVD-LOG          |
| 6    | 6.1     | Audit trail            | AuditService   | Sistema   | Complete audit     | EVD-AUDIT        |

**Historial de Actividades del Lead:**

```json
{
  "processCode": "CRM-002",
  "leadHistory": {
    "leadId": "lead-12345",
    "activities": [
      {
        "id": "act-001",
        "type": "LEAD_CREATED",
        "actor": "SYSTEM",
        "timestamp": "2026-01-21T10:30:00Z"
      },
      {
        "id": "act-002",
        "type": "LEAD_VIEWED",
        "actor": {
          "type": "DLR-STAFF",
          "id": "user-789",
          "name": "Pedro Vendedor"
        },
        "timestamp": "2026-01-21T10:35:00Z"
      },
      {
        "id": "act-003",
        "type": "STATUS_CHANGED",
        "actor": {
          "type": "DLR-STAFF",
          "id": "user-789"
        },
        "data": {
          "from": "NEW",
          "to": "CONTACTED"
        },
        "timestamp": "2026-01-21T10:36:00Z"
      },
      {
        "id": "act-004",
        "type": "MESSAGE_SENT",
        "actor": {
          "type": "DLR-STAFF",
          "id": "user-789"
        },
        "data": {
          "channel": "WHATSAPP",
          "message": "Hola María, gracias por su interés...",
          "messageId": "wa-789"
        },
        "timestamp": "2026-01-21T10:37:00Z"
      },
      {
        "id": "act-005",
        "type": "NOTE_ADDED",
        "actor": {
          "type": "DLR-STAFF",
          "id": "user-789"
        },
        "data": {
          "note": "Cliente muy interesada, quiere financiamiento"
        },
        "timestamp": "2026-01-21T10:38:00Z"
      },
      {
        "id": "act-006",
        "type": "APPOINTMENT_SCHEDULED",
        "actor": {
          "type": "DLR-STAFF",
          "id": "user-789"
        },
        "data": {
          "type": "TEST_DRIVE",
          "date": "2026-01-23T15:00:00Z"
        },
        "timestamp": "2026-01-21T10:40:00Z"
      }
    ],
    "currentStatus": "TEST_DRIVE_SCHEDULED",
    "currentScore": 92,
    "daysInPipeline": 0
  }
}
```

---

## 10. Procesos de Compliance

### 10.1 COMP-001: Generación de Reporte 607

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: COMP-001 - Generación de Reporte 607 DGII                     │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: SYS-SCHEDULER (Automático) o ADM-COMP (Manual)        │
│ Sistemas: ComplianceService, BillingService, MediaService              │
│ Criticidad: CRÍTICA (Regulatorio)                                      │
│ Frecuencia: Mensual (día 15 del mes siguiente)                         │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                     | Sistema             | Actor         | Evidencia              | Código Evidencia |
| ---- | ------- | -------------------------- | ------------------- | ------------- | ---------------------- | ---------------- |
| 1    | 1.1     | Trigger generación         | SchedulerService    | SYS-SCHEDULER | Job execution          | EVD-LOG          |
| 1    | 1.2     | Crear ReportRequest        | ComplianceService   | Sistema       | **Report requested**   | EVD-AUDIT        |
| 2    | 2.1     | Obtener transacciones      | BillingService      | Sistema       | Data query             | EVD-LOG          |
| 2    | 2.2     | Filtrar por período        | BillingService      | Sistema       | Filter applied         | EVD-LOG          |
| 3    | 3.1     | Validar NCFs               | ComplianceService   | Sistema       | **NCF validation**     | EVD-AUDIT        |
| 3    | 3.2     | Detectar inconsistencias   | ComplianceService   | Sistema       | Issues found           | EVD-LOG          |
| 4    | 4.1     | **Generar archivo 607**    | ComplianceService   | Sistema       | **607 file generated** | EVD-DOC          |
| 4    | 4.2     | Calcular hash              | ComplianceService   | Sistema       | File hash              | EVD-HASH         |
| 5    | 5.1     | Subir a S3                 | MediaService        | Sistema       | File stored            | EVD-FILE         |
| 5    | 5.2     | Crear registro             | ComplianceService   | Sistema       | Report record          | EVD-AUDIT        |
| 6    | 6.1     | **Notificar a Compliance** | NotificationService | SYS-NOTIF     | **Report ready**       | EVD-COMM         |
| 7    | 7.1     | Audit trail                | AuditService        | Sistema       | Complete audit         | EVD-AUDIT        |

**Evidencia de Reporte 607:**

```json
{
  "processCode": "COMP-001",
  "report607": {
    "reportId": "RPT-607-2026-01",
    "period": {
      "year": 2026,
      "month": 1,
      "from": "2026-01-01",
      "to": "2026-01-31"
    },
    "company": {
      "rnc": "130987654",
      "name": "OKLA SRL"
    },
    "summary": {
      "totalRecords": 156,
      "totalAmount": 4500000.0,
      "totalITBIS": 810000.0,
      "byNCFType": {
        "B01": { "count": 120, "amount": 3500000.0 },
        "B02": { "count": 36, "amount": 1000000.0 }
      }
    },
    "file": {
      "name": "607_130987654_202601.txt",
      "size": 45678,
      "hash": "sha256:abc123def456...",
      "s3Key": "compliance/2026/01/607_130987654_202601.txt"
    },
    "validation": {
      "passed": true,
      "warnings": 0,
      "errors": 0
    },
    "generatedAt": "2026-02-15T02:00:00Z",
    "generatedBy": "SYS-SCHEDULER"
  }
}
```

---

### 10.2 COMP-002: Verificación AML (Ley 155-17)

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: COMP-002 - Verificación AML                                   │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: SYS-COMPLIANCE (Automático en transacciones >$10K)    │
│ Sistemas: ComplianceService, BillingService, External AML APIs         │
│ Criticidad: CRÍTICA (Regulatorio)                                      │
│ Marco Legal: Ley 155-17, Norma 01-2015 Superintendencia de Bancos      │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                      | Sistema             | Actor     | Evidencia               | Código Evidencia |
| ---- | ------- | --------------------------- | ------------------- | --------- | ----------------------- | ---------------- |
| 1    | 1.1     | Transacción >$10K detectada | BillingService      | Sistema   | **Threshold triggered** | EVD-AUDIT        |
| 1    | 1.2     | Publicar AMLCheckEvent      | RabbitMQ            | Sistema   | Event published         | EVD-EVENT        |
| 2    | 2.1     | Obtener datos del cliente   | UserService         | Sistema   | Customer data           | EVD-LOG          |
| 2    | 2.2     | Verificar en listas PEP     | ComplianceService   | Sistema   | **PEP check**           | EVD-AUDIT        |
| 2    | 2.3     | Verificar en listas OFAC    | ComplianceService   | Sistema   | **OFAC check**          | EVD-AUDIT        |
| 3    | 3.1     | Calcular risk score         | ComplianceService   | Sistema   | **Risk score**          | EVD-AUDIT        |
| 3    | 3.2     | Categorizar nivel de riesgo | ComplianceService   | Sistema   | Risk category           | EVD-LOG          |
| 4    | 4.1     | Si HIGH RISK: alertar       | ComplianceService   | Sistema   | **Alert generated**     | EVD-AUDIT        |
| 4    | 4.2     | Si HIGH RISK: bloquear      | BillingService      | Sistema   | **Transaction blocked** | EVD-AUDIT        |
| 5    | 5.1     | Crear AMLRecord             | ComplianceService   | Sistema   | **AML record**          | EVD-AUDIT        |
| 5    | 5.2     | Notificar a Compliance      | NotificationService | SYS-NOTIF | **Urgent alert**        | EVD-COMM         |
| 6    | 6.1     | Audit trail                 | AuditService        | Sistema   | Complete audit          | EVD-AUDIT        |

**Evidencia de Verificación AML:**

```json
{
  "processCode": "COMP-002",
  "amlCheck": {
    "checkId": "AML-2026-001234",
    "triggeredBy": {
      "type": "TRANSACTION",
      "transactionId": "tx-12345",
      "amount": 15000.0,
      "currency": "USD"
    },
    "subject": {
      "type": "INDIVIDUAL",
      "userId": "user-001",
      "name": "Juan Pérez",
      "cedula": "001-1234567-8",
      "nationality": "DO"
    },
    "checks": {
      "pep": {
        "checked": true,
        "source": "DGII_PEP_LIST",
        "match": false
      },
      "ofac": {
        "checked": true,
        "source": "OFAC_SDN",
        "match": false
      },
      "internalBlacklist": {
        "checked": true,
        "match": false
      },
      "transactionPattern": {
        "checked": true,
        "unusual": false,
        "avgTransaction": 5000.0
      }
    },
    "result": {
      "riskScore": 25,
      "riskCategory": "LOW",
      "action": "APPROVED",
      "reviewRequired": false
    },
    "timestamp": "2026-01-21T10:30:00Z"
  }
}
```

---

## 11. Procesos de Administración

### 11.1 ADM-001: Suspensión de Usuario

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: ADM-001 - Suspensión de Usuario                               │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: ADM-ADMIN, ADM-MOD                                    │
│ Sistemas: UserService, AuthService, NotificationService                │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                             | Sistema             | Actor     | Evidencia             | Código Evidencia |
| ---- | ------- | ---------------------------------- | ------------------- | --------- | --------------------- | ---------------- |
| 1    | 1.1     | POST /api/admin/users/{id}/suspend | Gateway             | ADM-ADMIN | **Suspend request**   | EVD-AUDIT        |
| 1    | 1.2     | Validar permisos                   | AuthService         | Sistema   | Permission check      | EVD-LOG          |
| 2    | 2.1     | **Documentar razón**               | UserService         | ADM-ADMIN | **Reason recorded**   | EVD-AUDIT        |
| 2    | 2.2     | Especificar duración               | UserService         | ADM-ADMIN | Duration set          | EVD-LOG          |
| 3    | 3.1     | **Snapshot estado anterior**       | UserService         | Sistema   | **Before state**      | EVD-SNAP         |
| 3    | 3.2     | Actualizar status a Suspended      | UserService         | Sistema   | Status updated        | EVD-LOG          |
| 3    | 3.3     | **Snapshot estado nuevo**          | UserService         | Sistema   | **After state**       | EVD-SNAP         |
| 4    | 4.1     | **Invalidar todas las sesiones**   | AuthService         | Sistema   | **Sessions revoked**  | EVD-AUDIT        |
| 4    | 4.2     | Bloquear nuevos logins             | AuthService         | Sistema   | Login blocked         | EVD-LOG          |
| 5    | 5.1     | Si dealer: desactivar listings     | VehiclesSvc         | Sistema   | Listings deactivated  | EVD-AUDIT        |
| 6    | 6.1     | **Notificar al usuario**           | NotificationService | SYS-NOTIF | **Suspension email**  | EVD-COMM         |
| 6    | 6.2     | Incluir razón y apelación          | NotificationService | Sistema   | Details included      | EVD-LOG          |
| 7    | 7.1     | Crear SuspensionRecord             | UserService         | Sistema   | **Suspension record** | EVD-AUDIT        |
| 8    | 8.1     | Audit trail completo               | AuditService        | Sistema   | Complete audit        | EVD-AUDIT        |

**Evidencia de Suspensión:**

```json
{
  "processCode": "ADM-001",
  "suspension": {
    "suspensionId": "SUSP-2026-001234",
    "subject": {
      "userId": "user-001",
      "email": "usuario@email.com",
      "type": "DEALER_ADMIN"
    },
    "action": {
      "type": "SUSPENSION",
      "duration": "30 days",
      "startDate": "2026-01-21T10:30:00Z",
      "endDate": "2026-02-20T10:30:00Z"
    },
    "reason": {
      "category": "POLICY_VIOLATION",
      "subcategory": "FRAUDULENT_LISTING",
      "description": "Publicación de vehículo con información falsa (kilometraje alterado)",
      "evidence": ["report-12345", "screenshot-789"]
    },
    "executedBy": {
      "type": "ADM-ADMIN",
      "id": "admin-001",
      "name": "Roberto Admin"
    },
    "effects": {
      "sessionsRevoked": 3,
      "listingsDeactivated": 15,
      "subscriptionPaused": true
    },
    "appeal": {
      "allowed": true,
      "deadline": "2026-01-28T23:59:59Z",
      "instructions": "Enviar apelación a compliance@okla.com.do"
    },
    "notification": {
      "sent": true,
      "channel": "EMAIL",
      "messageId": "msg-suspend-123"
    },
    "timestamp": "2026-01-21T10:30:00Z"
  }
}
```

---

### 11.2 ADM-002: Cambio de Configuración del Sistema

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: ADM-002 - Cambio de Configuración del Sistema                 │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: ADM-SUPER (Super Admin únicamente)                    │
│ Sistemas: ConfigurationService, AuditService, All Services             │
│ Criticidad: CRÍTICA                                                    │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                            | Sistema             | Actor     | Evidencia          | Código Evidencia |
| ---- | ------- | --------------------------------- | ------------------- | --------- | ------------------ | ---------------- |
| 1    | 1.1     | PUT /api/config/{namespace}/{key} | Gateway             | ADM-SUPER | **Config request** | EVD-AUDIT        |
| 1    | 1.2     | Verificar 2FA                     | AuthService         | ADM-SUPER | 2FA verified       | EVD-AUDIT        |
| 2    | 2.1     | **Snapshot valor anterior**       | ConfigService       | Sistema   | **Before value**   | EVD-SNAP         |
| 2    | 2.2     | Validar nuevo valor               | ConfigService       | Sistema   | Validation         | EVD-LOG          |
| 3    | 3.1     | **Documentar razón**              | ConfigService       | ADM-SUPER | **Change reason**  | EVD-AUDIT        |
| 3    | 3.2     | Actualizar valor                  | ConfigService       | Sistema   | Value updated      | EVD-LOG          |
| 3    | 3.3     | **Snapshot valor nuevo**          | ConfigService       | Sistema   | **After value**    | EVD-SNAP         |
| 4    | 4.1     | Crear ConfigHistory               | ConfigService       | Sistema   | **History entry**  | EVD-AUDIT        |
| 4    | 4.2     | Invalidar cache                   | Redis               | Sistema   | Cache invalidated  | EVD-LOG          |
| 5    | 5.1     | Publicar ConfigChangedEvent       | RabbitMQ            | Sistema   | Event published    | EVD-EVENT        |
| 5    | 5.2     | Servicios recargan config         | All Services        | Sistema   | Config reloaded    | EVD-LOG          |
| 6    | 6.1     | **Notificar a admins**            | NotificationService | SYS-NOTIF | **Change alert**   | EVD-COMM         |
| 7    | 7.1     | Audit trail completo              | AuditService        | Sistema   | Complete audit     | EVD-AUDIT        |

**Evidencia de Cambio de Configuración:**

```json
{
  "processCode": "ADM-002",
  "configChange": {
    "changeId": "CFG-2026-001234",
    "namespace": "billing",
    "key": "stripe.mode",
    "before": {
      "value": "test",
      "version": 5
    },
    "after": {
      "value": "live",
      "version": 6
    },
    "changedBy": {
      "type": "ADM-SUPER",
      "id": "super-001",
      "name": "Juan SuperAdmin",
      "ip": "10.0.0.1"
    },
    "reason": "Activación de producción después de pruebas exitosas",
    "approval": {
      "required": true,
      "approvedBy": "CEO",
      "ticketId": "JIRA-1234"
    },
    "impact": {
      "servicesAffected": ["BillingService", "Gateway"],
      "servicesReloaded": 2,
      "cacheInvalidated": true
    },
    "notification": {
      "sent": true,
      "recipients": ["dev-team", "compliance", "ceo"],
      "channel": "EMAIL+TEAMS"
    },
    "timestamp": "2026-01-21T10:30:00Z"
  }
}
```

---

## 12. Matriz de Retención de Evidencias

### 12.1 Retención por Tipo de Evidencia

| Tipo Evidencia       | Código    | Retención  | Base Legal     | Almacenamiento       |
| -------------------- | --------- | ---------- | -------------- | -------------------- |
| Log de Aplicación    | EVD-LOG   | 90 días    | Operativo      | Elasticsearch        |
| Audit Trail          | EVD-AUDIT | 10 años    | Ley 155-17     | PostgreSQL + S3      |
| Evento de Dominio    | EVD-EVENT | 5 años     | Ley 172-13     | PostgreSQL           |
| Snapshot de Estado   | EVD-SNAP  | 5 años     | Ley 172-13     | PostgreSQL           |
| Documento Generado   | EVD-DOC   | 10 años    | DGII           | S3 Glacier           |
| Comunicación         | EVD-COMM  | 5 años     | Pro Consumidor | PostgreSQL           |
| Firma/Consentimiento | EVD-SIGN  | 10 años    | Ley 172-13     | S3 + Blockchain hash |
| Archivo Subido       | EVD-FILE  | 10 años    | Ley 155-17     | S3 Glacier           |
| Hash de Integridad   | EVD-HASH  | Permanente | Auditoría      | PostgreSQL           |

### 12.2 Política de Archivado

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    CICLO DE VIDA DE EVIDENCIAS                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   ACTIVO (0-90 días)                                                    │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │  PostgreSQL Principal + Redis Cache                              │   │
│   │  - Acceso inmediato                                              │   │
│   │  - Full text search                                              │   │
│   │  - Dashboards en tiempo real                                     │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│                                   │                                      │
│                                   ▼ (90 días)                            │
│   ARCHIVO CALIENTE (90 días - 2 años)                                   │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │  PostgreSQL Secundario + S3 Standard                             │   │
│   │  - Acceso en segundos                                            │   │
│   │  - Queries disponibles                                           │   │
│   │  - Exportable bajo demanda                                       │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│                                   │                                      │
│                                   ▼ (2 años)                             │
│   ARCHIVO FRÍO (2-10 años)                                              │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │  S3 Glacier Deep Archive                                         │   │
│   │  - Acceso en 12-48 horas                                         │   │
│   │  - Solicitud formal requerida                                    │   │
│   │  - Costo mínimo de almacenamiento                                │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│                                   │                                      │
│                                   ▼ (10 años)                            │
│   DESTRUCCIÓN SEGURA                                                    │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │  - Certificado de destrucción generado                           │   │
│   │  - Hash de integridad preservado                                 │   │
│   │  - Metadatos mínimos retenidos                                   │   │
│   └─────────────────────────────────────────────────────────────────┘   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 12.3 Integridad de Evidencias (Cadena de Custodia)

```csharp
public class EvidenceChain
{
    public Guid EvidenceId { get; set; }
    public string Type { get; set; }

    // Hash chain para integridad
    public string ContentHash { get; set; }           // SHA256 del contenido
    public string PreviousEvidenceHash { get; set; }  // Hash del registro anterior
    public string ChainHash { get; set; }             // Hash de toda la cadena

    // Timestamps inmutables
    public DateTime CreatedAt { get; set; }           // UTC
    public string CreatedAtProof { get; set; }        // Timestamp de autoridad externa

    // Firma digital
    public string Signature { get; set; }             // Firma del servicio
    public string SigningKeyId { get; set; }          // ID de la llave usada

    // Verificación
    public bool IsVerified { get; set; }
    public DateTime? LastVerifiedAt { get; set; }
    public string VerificationResult { get; set; }
}
```

---

## 📋 Anexo A: Índice de Procesos

| Código   | Proceso                   | Actor Iniciador | Criticidad |
| -------- | ------------------------- | --------------- | ---------- |
| AUTH-001 | Registro de Usuario       | USR-ANON        | ALTA       |
| AUTH-002 | Login de Usuario          | USR-ANON        | CRÍTICA    |
| AUTH-003 | Login Fallido             | USR-ANON        | CRÍTICA    |
| AUTH-004 | Two-Factor Authentication | USR-REG         | CRÍTICA    |
| USR-001  | Actualización de Perfil   | USR-REG         | MEDIA      |
| USR-002  | Cambio de Contraseña      | USR-REG         | CRÍTICA    |
| USR-003  | Eliminación de Cuenta     | USR-REG         | CRÍTICA    |
| DLR-001  | Registro de Dealer        | USR-REG         | ALTA       |
| DLR-002  | Verificación de Dealer    | ADM-COMP        | ALTA       |
| DLR-003  | Suscripción de Dealer     | DLR-ADMIN       | ALTA       |
| VEH-001  | Publicación de Vehículo   | DLR-STAFF       | MEDIA      |
| VEH-002  | Moderación de Vehículo    | ADM-MOD         | MEDIA      |
| VEH-003  | Cambio de Precio          | DLR-STAFF       | BAJA       |
| PAY-001  | Pago de Listing           | USR-SELLER      | ALTA       |
| PAY-002  | Reembolso                 | ADM-SUPPORT     | ALTA       |
| CRM-001  | Creación de Lead          | USR-REG         | MEDIA      |
| CRM-002  | Seguimiento de Lead       | DLR-STAFF       | MEDIA      |
| COMP-001 | Generación Reporte 607    | SYS-SCHEDULER   | CRÍTICA    |
| COMP-002 | Verificación AML          | SYS-COMPLIANCE  | CRÍTICA    |
| ADM-001  | Suspensión de Usuario     | ADM-ADMIN       | ALTA       |
| ADM-002  | Cambio de Configuración   | ADM-SUPER       | CRÍTICA    |

---

## 📋 Anexo B: Códigos de Evidencia

| Código     | Nombre            | Descripción                  | Ejemplo               |
| ---------- | ----------------- | ---------------------------- | --------------------- |
| EVD-LOG    | Log de Aplicación | Registro técnico operativo   | Request/response logs |
| EVD-AUDIT  | Audit Trail       | Registro formal de auditoría | Cambio de datos       |
| EVD-EVENT  | Evento de Dominio | Evento de negocio            | UserCreatedEvent      |
| EVD-SNAP   | Snapshot          | Estado antes/después         | JSON del objeto       |
| EVD-DOC    | Documento         | Archivo generado             | Factura PDF           |
| EVD-COMM   | Comunicación      | Email/SMS/WhatsApp           | Notificación enviada  |
| EVD-SIGN   | Firma             | Consentimiento digital       | Términos aceptados    |
| EVD-FILE   | Archivo           | Documento subido             | Licencia comercial    |
| EVD-HASH   | Hash              | Integridad de datos          | SHA256                |
| EVD-SCREEN | Captura           | Screenshot automático        | Estado de UI          |

---

## 📋 Anexo C: Contactos de Auditoría

| Rol                     | Nombre           | Email                  | Responsabilidad             |
| ----------------------- | ---------------- | ---------------------- | --------------------------- |
| Compliance Officer      | María Compliance | compliance@okla.com.do | Cumplimiento regulatorio    |
| Data Protection Officer | Juan DPO         | dpo@okla.com.do        | Ley 172-13                  |
| Security Officer        | Carlos Security  | security@okla.com.do   | Seguridad de la información |
| Internal Auditor        | Ana Auditora     | audit@okla.com.do      | Auditorías internas         |

---

_Documento generado el 21 de enero de 2026_  
_Próxima revisión: 21 de julio de 2026_  
_Versión: 1.0_
