# 📋 Quejas y Reclamos - Pro Consumidor - Matriz de Procesos

> **Marco Legal:** Ley 358-05 - Protección al Consumidor  
> **Regulador:** Pro Consumidor  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** 🔴 0% Backend | 🔴 0% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                      | Backend      | UI Access | Observación    |
| ---------------------------- | ------------ | --------- | -------------- |
| QUEJA-CREATE-001 Crear queja | 🔴 Pendiente | 🔴 Falta  | Sin formulario |
| QUEJA-TRACK-001 Seguimiento  | 🔴 Pendiente | 🔴 Falta  | Sin tracking   |
| QUEJA-RESOLVE-001 Resolución | 🔴 Pendiente | 🔴 Falta  | Sin workflow   |
| QUEJA-REPORT-001 Reportes    | 🔴 Pendiente | 🔴 Falta  | Sin dashboard  |

### Rutas UI Existentes ✅

- `/help` → Centro de ayuda básico (FAQ)

### Rutas UI Faltantes 🔴

- `/complaints` → Formulario de quejas
- `/complaints/my` → Mis quejas/reclamos
- `/complaints/:id` → Detalle y seguimiento
- `/admin/complaints` → Gestión de quejas (admin)

---

## 📊 Resumen de Implementación

| Componente                        | Total | Implementado | Pendiente | Estado        |
| --------------------------------- | ----- | ------------ | --------- | ------------- |
| **QUEJA-CREATE-\*** (Creación)    | 4     | 0            | 4         | 🔴 Pendiente  |
| **QUEJA-TRACK-\*** (Seguimiento)  | 3     | 0            | 3         | 🔴 Pendiente  |
| **QUEJA-RESOLVE-\*** (Resolución) | 4     | 0            | 4         | 🔴 Pendiente  |
| **QUEJA-ESCALATE-\*** (Escalado)  | 3     | 0            | 3         | 🔴 Pendiente  |
| **QUEJA-REPORT-\*** (Reportes)    | 3     | 0            | 3         | 🔴 Pendiente  |
| **Tests**                         | 15    | 0            | 15        | 🔴 Pendiente  |
| **TOTAL**                         | 32    | 0            | 32        | 🔴 0% Backend |

---

## 1. Información General

### 1.1 Descripción

La Ley 358-05 establece que todo consumidor tiene derecho a presentar quejas y reclamos, y las empresas están obligadas a proporcionar un canal accesible para recibirlas y resolverlas.

### 1.2 Tipos de Quejas

| Tipo                     | Descripción                     | Plazo Respuesta |
| ------------------------ | ------------------------------- | --------------- |
| **Producto no conforme** | Vehículo diferente al anunciado | 5 días hábiles  |
| **Publicidad engañosa**  | Información falsa en anuncio    | 5 días hábiles  |
| **Incumplimiento**       | Vendedor no cumple acuerdo      | 10 días hábiles |
| **Garantía**             | Problema cubierto por garantía  | 15 días hábiles |
| **Cobro indebido**       | Cargo no autorizado             | 5 días hábiles  |
| **Servicio deficiente**  | Mal servicio de la plataforma   | 5 días hábiles  |
| **Fraude**               | Estafa o engaño                 | 24 horas        |

### 1.3 Partes Involucradas

| Parte              | Rol                      |
| ------------------ | ------------------------ |
| **Consumidor**     | Quien presenta la queja  |
| **Vendedor**       | Contra quien se presenta |
| **OKLA**           | Mediador/Facilitador     |
| **Pro Consumidor** | Escalamiento externo     |

---

## 2. Procesos de Implementación

### 2.1 QUEJA-CREATE: Creación de Queja

#### QUEJA-CREATE-001: Formulario de Queja

| Campo       | Valor                           |
| ----------- | ------------------------------- |
| **Proceso** | QUEJA-CREATE-001                |
| **Nombre**  | Formulario de Quejas y Reclamos |
| **Ruta**    | `/complaints/new`               |
| **Estado**  | 🔴 Pendiente                    |

**Campos del Formulario:**

| Campo                | Tipo        | Obligatorio | Descripción             |
| -------------------- | ----------- | ----------- | ----------------------- |
| Tipo de queja        | Select      | ✅          | Categoría del reclamo   |
| Vendedor/Dealer      | Search      | ✅          | Contra quién            |
| Vehículo/Transacción | Search      | 🟡          | Relacionado (si aplica) |
| Descripción          | Textarea    | ✅          | Detalle del problema    |
| Expectativa          | Textarea    | ✅          | Qué solución espera     |
| Evidencias           | File upload | 🟡          | Fotos, documentos       |
| Monto reclamado      | Number      | 🟡          | Si aplica compensación  |

**UI Propuesta:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│  📋 PRESENTAR QUEJA O RECLAMO                                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Tipo de queja *                                                        │
│  [▼ Seleccionar tipo de queja                                    ]     │
│                                                                         │
│  Vendedor o Dealer *                                                    │
│  [🔍 Buscar vendedor...                                          ]     │
│                                                                         │
│  Vehículo relacionado (opcional)                                        │
│  [🔍 Buscar vehículo o ingresa ID de transacción...              ]     │
│                                                                         │
│  Describe tu problema *                                                 │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │                                                                   │  │
│  │ Explica con detalle qué sucedió, cuándo ocurrió y cualquier     │  │
│  │ información relevante...                                         │  │
│  │                                                                   │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│  Mínimo 50 caracteres                                                   │
│                                                                         │
│  ¿Qué solución esperas? *                                               │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │                                                                   │  │
│  │ Describe qué resultado esperas de este reclamo...                │  │
│  │                                                                   │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  Evidencias (opcional)                                                  │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │  📎 Arrastra archivos aquí o haz clic para subir                │  │
│  │  Formatos: JPG, PNG, PDF | Máximo: 10MB por archivo             │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  Monto reclamado (si aplica)                                            │
│  RD$ [                                                            ]     │
│                                                                         │
│  [ ] Acepto que OKLA contacte al vendedor para mediar              ✓   │
│  [ ] He leído los términos del proceso de quejas                   ✓   │
│                                                                         │
│  [Cancelar]                              [📤 Enviar Queja]             │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

#### QUEJA-CREATE-002: Confirmación

| Campo       | Valor                          |
| ----------- | ------------------------------ |
| **Proceso** | QUEJA-CREATE-002               |
| **Nombre**  | Confirmación de Queja Recibida |
| **Trigger** | Después de enviar queja        |
| **Estado**  | 🔴 Pendiente                   |

**Acciones:**

1. Generar número de caso (QUEJA-2026-00001)
2. Email de confirmación al usuario
3. Email de notificación al vendedor
4. Crear ticket en sistema interno

---

### 2.2 QUEJA-TRACK: Seguimiento de Queja

#### QUEJA-TRACK-001: Lista de Mis Quejas

| Campo       | Valor            |
| ----------- | ---------------- |
| **Proceso** | QUEJA-TRACK-001  |
| **Ruta**    | `/complaints/my` |
| **Estado**  | 🔴 Pendiente     |

**UI Propuesta:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│  📋 MIS QUEJAS Y RECLAMOS                                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  [+ Nueva Queja]                                                        │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ QUEJA-2026-00015                                    🟡 En Proceso │  │
│  │ Producto no conforme                                              │  │
│  │ Contra: AutoMax Dealer                                            │  │
│  │ Fecha: 20/01/2026                                                 │  │
│  │ Último movimiento: Esperando respuesta del vendedor              │  │
│  │ [Ver Detalle]                                                     │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ QUEJA-2025-00892                                    ✅ Resuelta   │  │
│  │ Cobro indebido                                                    │  │
│  │ Contra: MotorRD                                                   │  │
│  │ Fecha: 15/12/2025                                                 │  │
│  │ Resolución: Reembolso procesado                                  │  │
│  │ [Ver Detalle]                                                     │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

#### QUEJA-TRACK-002: Detalle de Queja

| Campo       | Valor             |
| ----------- | ----------------- |
| **Proceso** | QUEJA-TRACK-002   |
| **Ruta**    | `/complaints/:id` |
| **Estado**  | 🔴 Pendiente      |

**Información a Mostrar:**

| Sección     | Contenido                      |
| ----------- | ------------------------------ |
| Encabezado  | Número de caso, estado, fechas |
| Partes      | Consumidor, vendedor, asignado |
| Descripción | Problema y expectativa         |
| Timeline    | Historial de eventos           |
| Evidencias  | Archivos adjuntos              |
| Mensajes    | Comunicación entre partes      |
| Resolución  | Resultado final (si aplica)    |

---

### 2.3 QUEJA-RESOLVE: Resolución de Quejas

#### QUEJA-RESOLVE-001: Workflow de Resolución

| Campo       | Valor               |
| ----------- | ------------------- |
| **Proceso** | QUEJA-RESOLVE-001   |
| **Nombre**  | Flujo de Resolución |
| **Estado**  | 🔴 Pendiente        |

**Estados de la Queja:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    FLUJO DE RESOLUCIÓN DE QUEJAS                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   📥 NUEVA                                                              │
│   └── Queja recibida, pendiente de asignación                          │
│                ↓                                                        │
│   👤 ASIGNADA                                                           │
│   └── Agente de soporte asignado                                       │
│                ↓                                                        │
│   📞 CONTACTANDO VENDEDOR                                               │
│   └── Se notifica al vendedor y se espera respuesta                    │
│                ↓                                                        │
│   🔄 EN MEDIACIÓN                                                       │
│   └── OKLA media entre las partes                                      │
│                ↓                                                        │
│   ┌────────────┬────────────┬────────────┐                              │
│   ↓            ↓            ↓            ↓                              │
│   ✅ RESUELTA  ⚖️ ESCALADA   ❌ CERRADA   ⏰ VENCIDA                     │
│   └ Acuerdo   └ A Pro      └ Sin       └ Sin respuesta                │
│     alcanzado   Consumidor   mérito      en plazo                      │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

#### QUEJA-RESOLVE-002: Respuesta del Vendedor

| Campo       | Valor             |
| ----------- | ----------------- |
| **Proceso** | QUEJA-RESOLVE-002 |
| **Plazo**   | 5 días hábiles    |
| **Estado**  | 🔴 Pendiente      |

**Opciones de Respuesta:**

| Opción             | Descripción                          |
| ------------------ | ------------------------------------ |
| **Aceptar**        | Acepta el reclamo y propone solución |
| **Proponer**       | Propone solución alternativa         |
| **Rechazar**       | Rechaza el reclamo con justificación |
| **Solicitar info** | Pide más información                 |

---

### 2.4 QUEJA-ESCALATE: Escalamiento

#### QUEJA-ESCALATE-001: Escalamiento a Pro Consumidor

| Campo       | Valor                            |
| ----------- | -------------------------------- |
| **Proceso** | QUEJA-ESCALATE-001               |
| **Nombre**  | Escalamiento Externo             |
| **Trigger** | Sin acuerdo después de mediación |
| **Estado**  | 🔴 Pendiente                     |

**Condiciones para Escalar:**

| Condición                       | Acción                |
| ------------------------------- | --------------------- |
| Vendedor no responde en 10 días | Ofrecer escalar       |
| Partes no llegan a acuerdo      | Ofrecer escalar       |
| Usuario solicita escalar        | Procesar escalamiento |
| Monto > RD$50,000               | Sugerir escalar       |

**Información para Pro Consumidor:**

1. Datos del consumidor
2. Datos del vendedor
3. Descripción del caso
4. Historial de mediación
5. Evidencias
6. Propuestas rechazadas

---

## 3. Endpoints API

### 3.1 ComplaintsController

| Método | Endpoint                       | Descripción              | Auth | Estado |
| ------ | ------------------------------ | ------------------------ | ---- | ------ |
| `POST` | `/api/complaints`              | Crear queja              | ✅   | 🔴     |
| `GET`  | `/api/complaints/my`           | Mis quejas               | ✅   | 🔴     |
| `GET`  | `/api/complaints/:id`          | Detalle de queja         | ✅   | 🔴     |
| `POST` | `/api/complaints/:id/message`  | Agregar mensaje          | ✅   | 🔴     |
| `POST` | `/api/complaints/:id/evidence` | Agregar evidencia        | ✅   | 🔴     |
| `POST` | `/api/complaints/:id/escalate` | Escalar a Pro Consumidor | ✅   | 🔴     |

### 3.2 Admin ComplaintsController

| Método | Endpoint                            | Descripción     | Auth  | Estado |
| ------ | ----------------------------------- | --------------- | ----- | ------ |
| `GET`  | `/api/admin/complaints`             | Listar todas    | Admin | 🔴     |
| `PUT`  | `/api/admin/complaints/:id/assign`  | Asignar agente  | Admin | 🔴     |
| `PUT`  | `/api/admin/complaints/:id/status`  | Cambiar estado  | Admin | 🔴     |
| `POST` | `/api/admin/complaints/:id/resolve` | Marcar resuelta | Admin | 🔴     |
| `GET`  | `/api/admin/complaints/stats`       | Estadísticas    | Admin | 🔴     |

### 3.3 Vendor ComplaintsController

| Método | Endpoint                             | Descripción       | Auth   | Estado |
| ------ | ------------------------------------ | ----------------- | ------ | ------ |
| `GET`  | `/api/vendor/complaints`             | Quejas contra mí  | Vendor | 🔴     |
| `POST` | `/api/vendor/complaints/:id/respond` | Responder queja   | Vendor | 🔴     |
| `POST` | `/api/vendor/complaints/:id/propose` | Proponer solución | Vendor | 🔴     |

---

## 4. Modelos de Datos

### 4.1 Complaint Entity

```csharp
public class Complaint
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } // QUEJA-2026-00001
    public ComplaintType Type { get; set; }
    public ComplaintStatus Status { get; set; }

    // Partes
    public Guid ConsumerId { get; set; }
    public Guid VendorId { get; set; }
    public Guid? AssignedAgentId { get; set; }

    // Relacionado
    public Guid? VehicleId { get; set; }
    public Guid? TransactionId { get; set; }

    // Contenido
    public string Description { get; set; }
    public string ExpectedResolution { get; set; }
    public decimal? ClaimedAmount { get; set; }

    // Resolución
    public string Resolution { get; set; }
    public ResolutionType? ResolutionType { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Auditoría
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? EscalatedAt { get; set; }

    // Navegación
    public List<ComplaintMessage> Messages { get; set; }
    public List<ComplaintEvidence> Evidences { get; set; }
    public List<ComplaintStatusHistory> StatusHistory { get; set; }
}
```

### 4.2 Enums

```csharp
public enum ComplaintType
{
    ProductNotAsDescribed,    // Producto no conforme
    MisleadingAdvertising,    // Publicidad engañosa
    Breach,                   // Incumplimiento
    Warranty,                 // Garantía
    UnauthorizedCharge,       // Cobro indebido
    PoorService,              // Servicio deficiente
    Fraud                     // Fraude
}

public enum ComplaintStatus
{
    New,                      // Nueva
    Assigned,                 // Asignada
    ContactingVendor,         // Contactando vendedor
    AwaitingVendorResponse,   // Esperando respuesta
    InMediation,              // En mediación
    Resolved,                 // Resuelta
    Escalated,                // Escalada a Pro Consumidor
    Closed,                   // Cerrada sin mérito
    Expired                   // Vencida
}

public enum ResolutionType
{
    Refund,                   // Reembolso
    Replacement,              // Reemplazo
    Repair,                   // Reparación
    Compensation,             // Compensación
    Apology,                  // Disculpa formal
    NoFault,                  // Sin culpa del vendedor
    Escalated                 // Escalado externamente
}
```

---

## 5. Notificaciones

### 5.1 Emails Automáticos

| Evento                   | Destinatario | Template                    |
| ------------------------ | ------------ | --------------------------- |
| Queja creada             | Consumidor   | complaint-created           |
| Nueva queja              | Vendedor     | complaint-received          |
| Respuesta del vendedor   | Consumidor   | complaint-vendor-response   |
| Respuesta del consumidor | Vendedor     | complaint-consumer-response |
| Queja escalada           | Ambos        | complaint-escalated         |
| Queja resuelta           | Ambos        | complaint-resolved          |
| Recordatorio respuesta   | Vendedor     | complaint-reminder          |

### 5.2 Plazos y Recordatorios

| Día | Acción                            |
| --- | --------------------------------- |
| 0   | Queja creada, notificar vendedor  |
| 3   | Recordatorio si no hay respuesta  |
| 5   | Último recordatorio               |
| 7   | Notificar posibilidad de escalar  |
| 10  | Cerrar por no respuesta o escalar |

---

## 6. Métricas y Reportes

### 6.1 KPIs

| Métrica                          | Objetivo |
| -------------------------------- | -------- |
| Tiempo promedio de resolución    | < 7 días |
| Tasa de resolución satisfactoria | > 85%    |
| Tasa de escalamiento             | < 10%    |
| Tasa de respuesta del vendedor   | > 90%    |
| NPS post-resolución              | > 50     |

### 6.2 Dashboard Admin

```
┌─────────────────────────────────────────────────────────────────────────┐
│  📊 DASHBOARD DE QUEJAS - Enero 2026                                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐           │
│  │ NUEVAS HOY      │ │ EN PROCESO      │ │ RESUELTAS MES   │           │
│  │      12         │ │      45         │ │      127        │           │
│  │ +3 vs ayer      │ │ -5 vs ayer      │ │ 89% satisfacción│           │
│  └─────────────────┘ └─────────────────┘ └─────────────────┘           │
│                                                                         │
│  Por Tipo                          Por Estado                           │
│  ┌───────────────────────────┐    ┌───────────────────────────┐        │
│  │ Producto no conforme: 35% │    │ ● Nuevas: 12              │        │
│  │ Publicidad engañosa: 25%  │    │ ● Asignadas: 8            │        │
│  │ Incumplimiento: 20%       │    │ ● En mediación: 25        │        │
│  │ Cobro indebido: 12%       │    │ ● Esperando resp: 12      │        │
│  │ Otros: 8%                 │    │ ● Escaladas: 3            │        │
│  └───────────────────────────┘    └───────────────────────────┘        │
│                                                                         │
│  Vendedores con Más Quejas                                              │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ 1. AutoMax Dealer - 8 quejas (5 resueltas, 3 en proceso)         │  │
│  │ 2. CarrosRD - 6 quejas (4 resueltas, 2 en proceso)               │  │
│  │ 3. MotorPlus - 5 quejas (5 resueltas)                            │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 7. Cronograma de Implementación

### Fase 1: Q1 2026 - MVP 🔴

- [ ] Modelo de datos Complaint
- [ ] Formulario de creación
- [ ] Lista de mis quejas
- [ ] Notificaciones básicas

### Fase 2: Q1 2026 - Mediación 🔴

- [ ] Respuesta del vendedor
- [ ] Timeline de mensajes
- [ ] Subida de evidencias
- [ ] Cambio de estados

### Fase 3: Q2 2026 - Admin 🔴

- [ ] Dashboard de quejas
- [ ] Asignación de agentes
- [ ] Estadísticas
- [ ] Escalamiento a Pro Consumidor

---

## 8. Referencias

| Documento           | Ubicación            |
| ------------------- | -------------------- |
| Ley 358-05          | congreso.gob.do      |
| Pro Consumidor      | proconsumidor.gob.do |
| 04-proconsumidor.md | Este directorio      |
| Centro de Ayuda     | /help                |

---

**Última revisión:** Enero 25, 2026  
**Próxima revisión:** Febrero 25, 2026  
**Responsable:** Equipo de Soporte + Desarrollo OKLA  
**Prioridad:** 🔴 ALTA (Obligación legal Ley 358-05)
