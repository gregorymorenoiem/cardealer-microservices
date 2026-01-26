# 🕵️ Reporte de Operaciones Sospechosas (ROS) - Matriz de Procesos

> **Marco Legal:** Ley 155-17 - Lavado de Activos y Financiamiento del Terrorismo  
> **Regulador:** Unidad de Análisis Financiero (UAF)  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** 🔴 20% Backend | 🔴 0% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                     | Backend      | UI Access | Observación     |
| --------------------------- | ------------ | --------- | --------------- |
| ROS-DETECT-001 Detección    | 🟡 Parcial   | 🔴 Falta  | Sin alertas     |
| ROS-EVALUATE-001 Evaluación | 🔴 Pendiente | 🔴 Falta  | Sin workflow    |
| ROS-REPORT-001 Reporte UAF  | 🔴 Pendiente | 🔴 Falta  | Sin integración |
| ROS-AUDIT-001 Auditoría     | 🔴 Pendiente | 🔴 Falta  | Sin registro    |

### Rutas UI Existentes ✅

- Ninguna específica para ROS

### Rutas UI Faltantes 🔴

- `/admin/compliance/alerts` → Alertas sospechosas
- `/admin/compliance/ros` → Gestión de ROS
- `/admin/compliance/ros/:id` → Detalle de ROS
- `/admin/compliance/ros/report` → Generar reporte UAF

---

## 📊 Resumen de Implementación

| Componente                       | Total | Implementado | Pendiente | Estado       |
| -------------------------------- | ----- | ------------ | --------- | ------------ |
| **ROS-DETECT-\*** (Detección)    | 5     | 1            | 4         | 🔴 20%       |
| **ROS-EVALUATE-\*** (Evaluación) | 4     | 0            | 4         | 🔴 Pendiente |
| **ROS-REPORT-\*** (Reporte)      | 4     | 0            | 4         | 🔴 Pendiente |
| **ROS-AUDIT-\*** (Auditoría)     | 3     | 0            | 3         | 🔴 Pendiente |
| **Tests**                        | 15    | 2            | 13        | 🔴 Pendiente |
| **TOTAL**                        | 31    | 3            | 28        | 🔴 10%       |

---

## 1. Información General

### 1.1 Aclaración del Modelo de Negocio

> ⚠️ **IMPORTANTE:** OKLA es una plataforma de anuncios clasificados (como SuperCarros.com). Solo cobra por publicación de anuncios, NO participa en las transacciones de vehículos. Los dealers/vendedores y compradores realizan las transacciones directamente fuera de OKLA.

**Por tanto:** OKLA probablemente **NO es Sujeto Obligado** de la UAF y no tiene obligación directa de presentar ROS.

### 1.2 Quiénes SÍ deben reportar ROS

Los **Dealers** que venden vehículos profesionalmente SÍ son Sujetos Obligados y deben:

1. **Identificar operaciones sospechosas** mediante monitoreo
2. **Evaluar** las alertas generadas
3. **Reportar a la UAF** en máximo 15 días calendario
4. **Mantener confidencialidad** absoluta (no informar al cliente)

### 1.2 Señales de Alerta (Red Flags)

| Categoría           | Señales                                                  | Peso     |
| ------------------- | -------------------------------------------------------- | -------- |
| **Transaccionales** | Pagos fragmentados, montos inusuales, múltiples tarjetas | 🔴 Alto  |
| **Documentales**    | Documentos falsos, inconsistencias                       | 🔴 Alto  |
| **Conductuales**    | Nerviosismo, prisa, no quiere dejar rastro               | 🟡 Medio |
| **Financieras**     | Pagos en efectivo grandes, fuentes no claras             | 🔴 Alto  |
| **Geográficas**     | Zonas de alto riesgo, países sancionados                 | 🟡 Medio |

### 1.3 Umbrales de Reporte

| Tipo de Operación              | Umbral ROS                    |
| ------------------------------ | ----------------------------- |
| Transacción única en efectivo  | > RD$500,000 (automático RTN) |
| Transacciones fraccionadas 24h | > RD$500,000 combinado        |
| Operación sospechosa           | Cualquier monto               |
| PEP involucrado                | Cualquier monto               |

---

## 2. Procesos de Implementación

### 2.1 ROS-DETECT: Detección de Señales

#### ROS-DETECT-001: Motor de Reglas

| Campo       | Valor                         |
| ----------- | ----------------------------- |
| **Proceso** | ROS-DETECT-001                |
| **Nombre**  | Motor de Detección de Alertas |
| **Estado**  | 🟡 Parcial                    |

**Reglas Implementadas:**

| Regla    | Descripción                                    | Estado |
| -------- | ---------------------------------------------- | ------ |
| RULE-001 | Transacción > RD$500,000 en efectivo           | ✅     |
| RULE-002 | Múltiples transacciones mismo día > RD$500,000 | 🔴     |
| RULE-003 | Múltiples tarjetas diferentes mismo día        | 🔴     |
| RULE-004 | Usuario en lista PEP                           | 🔴     |
| RULE-005 | País de origen en lista negra                  | 🔴     |
| RULE-006 | Inconsistencia ingresos vs transacción         | 🔴     |
| RULE-007 | Cambio frecuente de método de pago             | 🔴     |
| RULE-008 | Operación cerca de umbral (structuring)        | 🔴     |

**Arquitectura del Motor:**

```csharp
public interface IAlertRule
{
    string RuleId { get; }
    string Description { get; }
    AlertSeverity Severity { get; }
    Task<AlertResult> Evaluate(TransactionContext context);
}

public class TransactionAmountRule : IAlertRule
{
    public string RuleId => "RULE-001";
    public string Description => "Transacción mayor a RD$500,000";
    public AlertSeverity Severity => AlertSeverity.High;

    public async Task<AlertResult> Evaluate(TransactionContext context)
    {
        if (context.Amount > 500000 && context.PaymentMethod == PaymentMethod.Cash)
        {
            return AlertResult.Triggered(
                "Transacción en efectivo excede umbral RTN",
                new { Amount = context.Amount }
            );
        }
        return AlertResult.NotTriggered();
    }
}
```

#### ROS-DETECT-002: Alertas Automáticas

| Campo       | Valor                 |
| ----------- | --------------------- |
| **Proceso** | ROS-DETECT-002        |
| **Nombre**  | Generación de Alertas |
| **Estado**  | 🔴 Pendiente          |

**Flujo:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    DETECCIÓN DE ALERTAS                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1️⃣ Transacción/Evento ocurre                                          │
│   └── Registro de usuario, pago, publicación, etc.                     │
│                                                                         │
│   2️⃣ Motor de Reglas evalúa                                             │
│   ├── Ejecutar todas las reglas aplicables                             │
│   ├── Calcular score de riesgo                                         │
│   └── Determinar si se genera alerta                                   │
│                                                                         │
│   3️⃣ Si score >= umbral                                                 │
│   ├── Crear registro de Alerta                                         │
│   ├── Notificar a Oficial de Cumplimiento                              │
│   └── Bloquear operación si es crítico                                 │
│                                                                         │
│   4️⃣ Alerta en cola de evaluación                                       │
│   └── Esperando revisión manual                                        │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

### 2.2 ROS-EVALUATE: Evaluación de Alertas

#### ROS-EVALUATE-001: Panel de Alertas

| Campo       | Valor                      |
| ----------- | -------------------------- |
| **Proceso** | ROS-EVALUATE-001           |
| **Ruta**    | `/admin/compliance/alerts` |
| **Estado**  | 🔴 Pendiente               |

**UI Propuesta:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│  🚨 ALERTAS DE CUMPLIMIENTO                                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Filtros: [Todas ▼] [Esta Semana ▼] [Ordenar: Más Recientes ▼]         │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ 🔴 ALTA │ ALT-2026-00089 │ Transacción > RD$500K              │  │
│  │ Usuario: Juan Pérez (user123)                                    │  │
│  │ Monto: RD$850,000 │ Método: Efectivo                             │  │
│  │ Fecha: 25/01/2026 14:35                                          │  │
│  │ Estado: ⏳ Pendiente                                              │  │
│  │ [Evaluar] [Ver Perfil] [Ver Transacción]                         │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ 🟡 MEDIA │ ALT-2026-00088 │ Múltiples tarjetas mismo día       │  │
│  │ Usuario: María López (user456)                                   │  │
│  │ Tarjetas: 4 diferentes │ Monto total: RD$120,000                │  │
│  │ Fecha: 24/01/2026 16:20                                          │  │
│  │ Estado: ⏳ Pendiente                                              │  │
│  │ [Evaluar] [Ver Perfil] [Ver Transacciones]                       │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ 🟢 BAJA │ ALT-2026-00087 │ País de origen en lista de riesgo   │  │
│  │ Usuario: Carlos Ruiz (user789)                                   │  │
│  │ País: Venezuela │ Monto: RD$45,000                              │  │
│  │ Fecha: 24/01/2026 10:15                                          │  │
│  │ Estado: ✅ Descartada (documentación válida)                     │  │
│  │ [Ver Resolución]                                                 │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

#### ROS-EVALUATE-002: Evaluación Individual

| Campo       | Valor                                   |
| ----------- | --------------------------------------- |
| **Proceso** | ROS-EVALUATE-002                        |
| **Ruta**    | `/admin/compliance/alerts/:id/evaluate` |
| **Estado**  | 🔴 Pendiente                            |

**UI de Evaluación:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│  📋 EVALUACIÓN DE ALERTA ALT-2026-00089                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  INFORMACIÓN DE LA ALERTA                                               │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ Regla: RULE-001 - Transacción mayor a RD$500,000                 │  │
│  │ Severidad: 🔴 Alta                                                │  │
│  │ Fecha/Hora: 25/01/2026 14:35:22                                  │  │
│  │ Score de Riesgo: 85/100                                          │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  USUARIO                                                                │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ Nombre: Juan Pérez                                               │  │
│  │ Cédula: 001-XXXXXXX-X                                            │  │
│  │ Email: juan.perez@email.com                                      │  │
│  │ Teléfono: 809-XXX-XXXX                                           │  │
│  │ Miembro desde: 15/06/2025                                        │  │
│  │ Verificación KYC: ✅ Completada                                   │  │
│  │ Alertas previas: 0                                               │  │
│  │ [Ver Perfil Completo] [Ver Historial Transacciones]              │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  TRANSACCIÓN SOSPECHOSA                                                 │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ ID: TXN-2026-45678                                               │  │
│  │ Tipo: Compra de vehículo                                         │  │
│  │ Vehículo: Toyota Camry 2024                                      │  │
│  │ Vendedor: AutoMax Dealer (RNC: 1-31-XXXXX-X)                     │  │
│  │ Monto: RD$850,000.00                                             │  │
│  │ Método: Efectivo                                                 │  │
│  │ Fecha: 25/01/2026 14:30                                          │  │
│  │ [Ver Detalles Completos]                                         │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  DOCUMENTOS ADJUNTOS                                                    │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ 📄 Declaración de fondos: Pendiente                              │  │
│  │ 📄 Comprobante de ingresos: Pendiente                            │  │
│  │                                                                   │  │
│  │ [📎 Solicitar Documentación al Usuario]                          │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  DECISIÓN                                                               │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ ( ) Descartar - Sin mérito (explicar razón)                      │  │
│  │ (•) Escalar a ROS - Generar reporte para UAF                     │  │
│  │ ( ) Solicitar más información - Documentos adicionales           │  │
│  │ ( ) Bloquear usuario - Actividad claramente sospechosa          │  │
│  │                                                                   │  │
│  │ Comentarios internos (no visibles para usuario):                 │  │
│  │ ┌───────────────────────────────────────────────────────────────┐│  │
│  │ │ El usuario no pudo justificar origen de fondos...            ││  │
│  │ └───────────────────────────────────────────────────────────────┘│  │
│  │                                                                   │  │
│  │ [Guardar como Borrador]              [Confirmar Decisión]        │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

### 2.3 ROS-REPORT: Reporte a UAF

#### ROS-REPORT-001: Formulario ROS

| Campo       | Valor                       |
| ----------- | --------------------------- |
| **Proceso** | ROS-REPORT-001              |
| **Ruta**    | `/admin/compliance/ros/new` |
| **Estado**  | 🔴 Pendiente                |

**Campos del ROS:**

| Sección                         | Campos                                    |
| ------------------------------- | ----------------------------------------- |
| **Datos del Sujeto Obligado**   | RNC, Nombre, Oficial de Cumplimiento      |
| **Datos del Reportado**         | Nombre, Cédula/RNC, Dirección, Teléfono   |
| **Descripción de la Operación** | Tipo, Monto, Fecha, Descripción detallada |
| **Razones de Sospecha**         | Indicadores detectados, análisis          |
| **Documentación Adjunta**       | Transacciones, documentos, evidencia      |

#### ROS-REPORT-002: Envío a UAF

| Campo       | Valor                              |
| ----------- | ---------------------------------- |
| **Proceso** | ROS-REPORT-002                     |
| **Nombre**  | Transmisión Electrónica            |
| **Plazo**   | 15 días calendario desde detección |
| **Estado**  | 🔴 Pendiente                       |

**Canales de Envío:**

| Canal                                | Descripción           | Estado        |
| ------------------------------------ | --------------------- | ------------- |
| SIAF (Sistema Integrado Anti-Fraude) | API oficial de la UAF | 🔴 Pendiente  |
| Portal Web UAF                       | Formulario manual     | 🟡 Manual     |
| Email encriptado                     | Respaldo              | ✅ Disponible |

#### ROS-REPORT-003: Seguimiento

| Campo       | Valor                   |
| ----------- | ----------------------- |
| **Proceso** | ROS-REPORT-003          |
| **Ruta**    | `/admin/compliance/ros` |
| **Estado**  | 🔴 Pendiente            |

**Estados del ROS:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│                       CICLO DE VIDA DEL ROS                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   📝 BORRADOR                                                           │
│   └── ROS en preparación                                               │
│                ↓                                                        │
│   ✅ APROBADO                                                           │
│   └── Oficial de Cumplimiento aprueba                                  │
│                ↓                                                        │
│   📤 ENVIADO                                                            │
│   └── Transmitido a UAF                                                │
│                ↓                                                        │
│   🔄 RECIBIDO                                                           │
│   └── UAF confirma recepción                                           │
│                ↓                                                        │
│   ┌────────────┬────────────┬────────────┐                              │
│   ↓            ↓            ↓            │                              │
│   📁 ARCHIVADO 🔎 INVESTIGACIÓN 📋 INFO   │                              │
│   └ Sin mérito  └ UAF investiga └ Solicitan│                              │
│                                   más info │                              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

### 2.4 ROS-AUDIT: Auditoría Interna

#### ROS-AUDIT-001: Registro de Actividades

| Campo       | Valor                 |
| ----------- | --------------------- |
| **Proceso** | ROS-AUDIT-001         |
| **Nombre**  | Trazabilidad Completa |
| **Estado**  | 🔴 Pendiente          |

**Eventos a Registrar:**

| Evento              | Datos                              |
| ------------------- | ---------------------------------- |
| Alerta generada     | ID, regla, score, timestamp        |
| Alerta evaluada     | ID, decisión, evaluador, timestamp |
| ROS creado          | ID, ID alerta relacionada, creador |
| ROS enviado         | ID, canal, timestamp, confirmación |
| Comentario agregado | ID, autor, contenido, timestamp    |

#### ROS-AUDIT-002: Reportes Internos

| Campo       | Valor                       |
| ----------- | --------------------------- |
| **Proceso** | ROS-AUDIT-002               |
| **Ruta**    | `/admin/compliance/reports` |
| **Estado**  | 🔴 Pendiente                |

**Reportes Disponibles:**

| Reporte                | Frecuencia | Destinatario         |
| ---------------------- | ---------- | -------------------- |
| Alertas del mes        | Mensual    | Oficial Cumplimiento |
| ROS enviados           | Mensual    | Junta Directiva      |
| Métricas de detección  | Trimestral | Auditoría Interna    |
| Estadísticas por regla | Trimestral | IT + Cumplimiento    |

---

## 3. Modelo de Datos

### 3.1 Entidades

```csharp
public class Alert
{
    public Guid Id { get; set; }
    public string AlertNumber { get; set; }  // ALT-2026-00089
    public string RuleId { get; set; }
    public AlertSeverity Severity { get; set; }
    public AlertStatus Status { get; set; }
    public int RiskScore { get; set; }

    // Contexto
    public Guid? UserId { get; set; }
    public Guid? TransactionId { get; set; }
    public string TriggerData { get; set; } // JSON

    // Evaluación
    public Guid? EvaluatedById { get; set; }
    public DateTime? EvaluatedAt { get; set; }
    public string EvaluationNotes { get; set; }
    public AlertDecision? Decision { get; set; }

    // ROS relacionado
    public Guid? RosId { get; set; }

    // Auditoría
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SuspiciousOperationReport // ROS
{
    public Guid Id { get; set; }
    public string RosNumber { get; set; }  // ROS-2026-00012
    public RosStatus Status { get; set; }

    // Alertas relacionadas
    public List<Guid> AlertIds { get; set; }

    // Datos del reportado
    public string SubjectName { get; set; }
    public string SubjectIdNumber { get; set; }
    public string SubjectIdType { get; set; }

    // Operación sospechosa
    public string OperationType { get; set; }
    public decimal Amount { get; set; }
    public DateTime OperationDate { get; set; }
    public string Description { get; set; }

    // Razones
    public string SuspicionReasons { get; set; }
    public List<string> Indicators { get; set; }

    // Documentos
    public List<Guid> AttachmentIds { get; set; }

    // Workflow
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string UafConfirmationCode { get; set; }
}
```

### 3.2 Enums

```csharp
public enum AlertSeverity { Low, Medium, High, Critical }

public enum AlertStatus { Pending, UnderReview, Dismissed, EscalatedToRos, Blocked }

public enum AlertDecision { Dismiss, EscalateToRos, RequestInfo, Block }

public enum RosStatus { Draft, Approved, Sent, Received, UnderInvestigation, Archived }
```

---

## 4. Endpoints API

### 4.1 AlertsController (Compliance)

| Método | Endpoint                                  | Descripción       | Auth       | Estado |
| ------ | ----------------------------------------- | ----------------- | ---------- | ------ |
| `GET`  | `/api/compliance/alerts`                  | Listar alertas    | Compliance | 🔴     |
| `GET`  | `/api/compliance/alerts/:id`              | Detalle de alerta | Compliance | 🔴     |
| `PUT`  | `/api/compliance/alerts/:id/evaluate`     | Evaluar alerta    | Compliance | 🔴     |
| `POST` | `/api/compliance/alerts/:id/request-docs` | Solicitar docs    | Compliance | 🔴     |

### 4.2 RosController (Compliance)

| Método | Endpoint                          | Descripción    | Auth       | Estado |
| ------ | --------------------------------- | -------------- | ---------- | ------ |
| `GET`  | `/api/compliance/ros`             | Listar ROS     | Compliance | 🔴     |
| `POST` | `/api/compliance/ros`             | Crear ROS      | Compliance | 🔴     |
| `GET`  | `/api/compliance/ros/:id`         | Detalle de ROS | Compliance | 🔴     |
| `PUT`  | `/api/compliance/ros/:id`         | Actualizar ROS | Compliance | 🔴     |
| `POST` | `/api/compliance/ros/:id/approve` | Aprobar ROS    | Compliance | 🔴     |
| `POST` | `/api/compliance/ros/:id/send`    | Enviar a UAF   | Compliance | 🔴     |

---

## 5. Confidencialidad

⚠️ **IMPORTANTE: La información de ROS es CONFIDENCIAL**

| Acción                  | Permitido          | Prohibido      |
| ----------------------- | ------------------ | -------------- |
| Informar al cliente     | ❌                 | ✅ Nunca       |
| Compartir con terceros  | ❌                 | ✅ Solo UAF    |
| Documentar internamente | ✅ Con restricción | Acceso general |
| Bloquear cuenta         | ✅ Sin explicar    | Mencionar ROS  |

**Consecuencias de divulgación:**

- Responsabilidad penal para el oficial
- Multas para la empresa
- Pérdida de licencia

---

## 6. Cronograma de Implementación

### Fase 1: Q1 2026 - Detección 🔴

- [ ] Motor de reglas básico
- [ ] Reglas principales (5 reglas)
- [ ] Generación de alertas
- [ ] Notificación al Oficial

### Fase 2: Q2 2026 - Evaluación 🔴

- [ ] Panel de alertas
- [ ] Workflow de evaluación
- [ ] Solicitud de documentos
- [ ] Decisiones y notas

### Fase 3: Q3 2026 - Reporte 🔴

- [ ] Formulario ROS
- [ ] Aprobación workflow
- [ ] Integración SIAF (si disponible)
- [ ] Seguimiento de ROS

### Fase 4: Q4 2026 - Auditoría 🔴

- [ ] Trazabilidad completa
- [ ] Reportes internos
- [ ] Dashboard de métricas
- [ ] Capacitación del equipo

---

## 7. Referencias

| Documento         | Ubicación                               |
| ----------------- | --------------------------------------- |
| Ley 155-17        | congreso.gob.do                         |
| Normas UAF        | uaf.gob.do                              |
| 01-ley-155-17.md  | 08-COMPLIANCE-LEGAL-RD                  |
| ComplianceService | backend/ComplianceService (planificado) |

---

**Última revisión:** Enero 25, 2026  
**Próxima revisión:** Febrero 25, 2026  
**Responsable:** Oficial de Cumplimiento OKLA  
**Prioridad:** 🔴 CRÍTICA (Obligación legal, sanciones penales)

---

⚠️ **ADVERTENCIA LEGAL**

Este documento es CONFIDENCIAL y de uso exclusivo del personal autorizado de OKLA. La divulgación de información relacionada con ROS constituye un delito según la Ley 155-17.

Acceso autorizado:

- Oficial de Cumplimiento
- Gerencia General
- Junta Directiva (reportes agregados)
- Auditoría Interna (sin datos personales)
