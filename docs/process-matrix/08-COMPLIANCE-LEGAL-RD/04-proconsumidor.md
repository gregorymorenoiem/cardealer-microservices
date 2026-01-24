# 🚗 Pro Consumidor - Protección al Consumidor - Matriz de Procesos

> **Entidad:** Proconsumidor (Instituto Nacional de Protección de los Derechos del Consumidor)  
> **Marco Legal:** Ley 358-05 de Protección al Consumidor  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO (Obligatorio)

---

## 📊 Resumen de Implementación

| Componente                     | Total | Implementado | Pendiente | Estado         |
| ------------------------------ | ----- | ------------ | --------- | -------------- |
| **CONS-INFO-\*** (Información) | 3     | 0            | 3         | 🔴 Pendiente   |
| **CONS-GAR-\*** (Garantías)    | 4     | 0            | 4         | 🔴 Pendiente   |
| **CONS-QUEJA-\*** (Quejas)     | 4     | 0            | 4         | 🔴 Pendiente   |
| **CONS-DEV-\*** (Devoluciones) | 3     | 0            | 3         | 🔴 Pendiente   |
| **CONS-REP-\*** (Reportes)     | 3     | 0            | 3         | 🔴 Pendiente   |
| **Tests**                      | 0     | 0            | 15        | 🔴 Pendiente   |
| **TOTAL**                      | 17    | 0            | 17        | 🔴 0% Completo |

---

## 1. Información General

### 1.1 Descripción

La Ley 358-05 establece los derechos fundamentales de los consumidores en República Dominicana. OKLA debe cumplir con estas regulaciones al facilitar transacciones de vehículos entre compradores y vendedores.

### 1.2 Derechos Fundamentales del Consumidor

| Derecho            | Descripción                          | Implementación OKLA       |
| ------------------ | ------------------------------------ | ------------------------- |
| **Información**    | Conocer características del producto | Ficha técnica detallada   |
| **Elección**       | Libertad de escoger                  | Sin cláusulas abusivas    |
| **Seguridad**      | Productos seguros                    | Verificación de vehículos |
| **Indemnización**  | Compensación por daños               | Proceso de disputas       |
| **Representación** | Ser escuchado                        | Soporte 24/7              |
| **Educación**      | Información clara                    | Guías de compra           |

---

## 2. Obligaciones de OKLA

### 2.1 Como Plataforma

| Obligación                    | Descripción                          |
| ----------------------------- | ------------------------------------ |
| **Transparencia**             | Mostrar comisiones y fees claramente |
| **Información veraz**         | No permitir publicidad engañosa      |
| **Garantía mínima**           | Facilitar reclamos de garantía       |
| **Derecho de retracto**       | 48 horas para cancelar (servicios)   |
| **Atención al cliente**       | Canal de reclamaciones               |
| **Registro de transacciones** | Mantener por 3 años                  |

### 2.2 Responsabilidad del Vendedor

| Obligación          | Descripción                 |
| ------------------- | --------------------------- |
| Descripción exacta  | Vehículo como se describe   |
| Historial completo  | Accidentes, reparaciones    |
| Documentos en regla | Título, impuestos           |
| Garantía mínima     | 30 días mecánica básica     |
| Sin vicios ocultos  | Declarar todos los defectos |

---

## 3. Endpoints API

### 3.1 ConsumerProtectionController

| Método | Endpoint                                 | Descripción         | Auth | Roles |
| ------ | ---------------------------------------- | ------------------- | ---- | ----- |
| `POST` | `/api/consumer/complaints`               | Crear queja/reclamo | ✅   | User  |
| `GET`  | `/api/consumer/complaints/my`            | Mis quejas          | ✅   | User  |
| `GET`  | `/api/consumer/complaints/{id}`          | Ver queja           | ✅   | Owner |
| `POST` | `/api/consumer/complaints/{id}/evidence` | Agregar evidencia   | ✅   | Owner |
| `POST` | `/api/consumer/retraction`               | Ejercer retracto    | ✅   | User  |
| `GET`  | `/api/consumer/warranty/{vehicleId}`     | Info garantía       | ✅   | Buyer |

### 3.2 AdminComplaintsController

| Método | Endpoint                              | Descripción              | Auth | Roles   |
| ------ | ------------------------------------- | ------------------------ | ---- | ------- |
| `GET`  | `/api/admin/complaints`               | Listar quejas            | ✅   | Support |
| `PUT`  | `/api/admin/complaints/{id}`          | Actualizar queja         | ✅   | Support |
| `POST` | `/api/admin/complaints/{id}/escalate` | Escalar a Pro Consumidor | ✅   | Admin   |
| `POST` | `/api/admin/complaints/{id}/resolve`  | Resolver queja           | ✅   | Support |

---

## 4. Entidades y Enums

### 4.1 ComplaintType (Enum)

```csharp
public enum ComplaintType
{
    VehicleNotAsDescribed = 0,    // Vehículo diferente a descripción
    HiddenDefects = 1,            // Vicios ocultos
    DocumentationIssues = 2,      // Problemas de documentación
    WarrantyDenied = 3,           // Garantía no honrada
    RefundDenied = 4,             // Reembolso denegado
    PriceDiscrepancy = 5,         // Precio diferente al acordado
    DeliveryIssues = 6,           // Problemas de entrega
    FraudSuspicion = 7,           // Sospecha de fraude
    PoorService = 8,              // Mal servicio
    Other = 99                    // Otro
}
```

### 4.2 ComplaintStatus (Enum)

```csharp
public enum ComplaintStatus
{
    Submitted = 0,                // Enviada
    UnderReview = 1,              // En revisión
    AwaitingResponse = 2,         // Esperando respuesta del vendedor
    InMediation = 3,              // En mediación
    EscalatedToProConsumidor = 4, // Escalada a Pro Consumidor
    ResolvedInFavor = 5,          // Resuelta a favor del consumidor
    ResolvedAgainst = 6,          // Resuelta a favor del vendedor
    Closed = 7,                   // Cerrada
    Withdrawn = 8                 // Retirada por el consumidor
}
```

### 4.3 Complaint (Entidad)

```csharp
public class Complaint
{
    public Guid Id { get; set; }
    public string ComplaintNumber { get; set; }   // QJ-2026-00001

    // Partes
    public Guid ConsumerId { get; set; }
    public Guid SellerId { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? TransactionId { get; set; }

    // Detalles
    public ComplaintType Type { get; set; }
    public ComplaintStatus Status { get; set; }
    public string Description { get; set; }
    public decimal? ClaimedAmount { get; set; }
    public string RequestedResolution { get; set; }

    // Evidencia
    public List<ComplaintEvidence> Evidence { get; set; }

    // Respuesta del vendedor
    public string? SellerResponse { get; set; }
    public DateTime? SellerRespondedAt { get; set; }

    // Resolución
    public string? Resolution { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? ProConsumidorCaseNumber { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime Deadline { get; set; }        // 15 días para resolver
}
```

---

## 5. Procesos Detallados

### 5.1 PC-001: Crear Queja de Consumidor

| Campo       | Valor                         |
| ----------- | ----------------------------- |
| **ID**      | PC-001                        |
| **Nombre**  | Crear Queja de Consumidor     |
| **Actor**   | Comprador                     |
| **Trigger** | POST /api/consumer/complaints |

#### Flujo del Proceso

| Paso | Acción                      | Sistema             | Validación            |
| ---- | --------------------------- | ------------------- | --------------------- |
| 1    | Comprador tiene problema    | Situación           | Post-compra           |
| 2    | Acceder a "Ayuda"           | Frontend            | /help                 |
| 3    | Seleccionar tipo de queja   | Frontend            | Formulario            |
| 4    | Describir problema          | Frontend            | Min 100 caracteres    |
| 5    | Adjuntar evidencia          | MediaService        | Fotos, docs           |
| 6    | Indicar resolución esperada | Frontend            | Reembolso, reparación |
| 7    | Submit queja                | API                 | POST                  |
| 8    | Generar número de caso      | ConsumerService     | QJ-2026-00001         |
| 9    | Notificar vendedor          | NotificationService | 48h para responder    |
| 10   | Notificar soporte           | NotificationService | Nuevo caso            |
| 11   | Iniciar timer de 15 días    | SchedulerService    | Deadline              |
| 12   | Publicar evento             | RabbitMQ            | complaint.created     |

#### Request

```json
{
  "transactionId": "uuid",
  "vehicleId": "uuid",
  "type": "VehicleNotAsDescribed",
  "description": "El vehículo tiene daños en el motor que no fueron mencionados en la descripción. Al revisar con un mecánico, encontró que necesita reparación de $150,000. Las fotos mostraban el motor limpio pero hay fugas de aceite evidentes.",
  "evidence": [
    {
      "type": "image",
      "url": "https://...",
      "description": "Foto del motor con fuga"
    },
    {
      "type": "document",
      "url": "https://...",
      "description": "Diagnóstico del mecánico"
    }
  ],
  "claimedAmount": 150000,
  "requestedResolution": "FullRefund"
}
```

---

### 5.2 PC-002: Proceso de Mediación

| Campo       | Valor                        |
| ----------- | ---------------------------- |
| **ID**      | PC-002                       |
| **Nombre**  | Mediación entre Partes       |
| **Actor**   | Soporte OKLA                 |
| **Trigger** | Vendedor responde o deadline |

#### Flujo del Proceso

| Paso | Acción                 | Sistema             | Validación        |
| ---- | ---------------------- | ------------------- | ----------------- |
| 1    | Vendedor responde      | API                 | Dentro de 48h     |
| 2    | Asignar mediador       | SupportService      | Automático        |
| 3    | Revisar caso completo  | SupportService      | Evidencias        |
| 4    | Contactar ambas partes | NotificationService | Reunión virtual   |
| 5    | Proponer solución      | Mediador            | Basada en hechos  |
| 6    | Si acuerdo             | ConsumerService     | Documentar        |
| 7    | Si no acuerdo          | ConsumerService     | Escalar           |
| 8    | Ejecutar resolución    | BillingService      | Si reembolso      |
| 9    | Cerrar caso            | ConsumerService     | Status = Resolved |
| 10   | Solicitar feedback     | NotificationService | Encuesta          |

#### Opciones de Resolución

| Resolución        | Descripción                       |
| ----------------- | --------------------------------- |
| `FullRefund`      | Reembolso completo                |
| `PartialRefund`   | Reembolso parcial                 |
| `VehicleRepair`   | Vendedor paga reparación          |
| `VehicleExchange` | Cambio por otro vehículo          |
| `Compensation`    | Compensación adicional            |
| `NoAction`        | Sin acción (a favor del vendedor) |

---

### 5.3 PC-003: Escalar a Pro Consumidor

| Campo       | Valor                    |
| ----------- | ------------------------ |
| **ID**      | PC-003                   |
| **Nombre**  | Escalar a Autoridad      |
| **Actor**   | Admin/Sistema            |
| **Trigger** | No resolución en 15 días |

#### Flujo del Proceso

| Paso | Acción                  | Sistema             | Validación         |
| ---- | ----------------------- | ------------------- | ------------------ |
| 1    | Deadline cumplido       | SchedulerService    | 15 días            |
| 2    | Verificar no resuelto   | ConsumerService     | Status != Resolved |
| 3    | Generar expediente      | ConsumerService     | PDF completo       |
| 4    | Incluir toda evidencia  | ConsumerService     | Ambas partes       |
| 5    | Enviar a Pro Consumidor | IntegrationService  | API o email        |
| 6    | Obtener número de caso  | ProConsumidor       | Externo            |
| 7    | Actualizar queja        | Database            | Status = Escalated |
| 8    | Notificar ambas partes  | NotificationService | Con instrucciones  |
| 9    | Log para auditoría      | AuditService        | Registro completo  |

#### Formato de Expediente

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     EXPEDIENTE DE QUEJA                                 │
│                     Pro Consumidor RD                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  NÚMERO DE CASO OKLA: QJ-2026-00001                                     │
│  FECHA DE INICIO: 15/01/2026                                            │
│  FECHA DE ESCALAMIENTO: 30/01/2026                                      │
│                                                                          │
│  ═══════════════════════════════════════════════════════════════════   │
│                                                                          │
│  CONSUMIDOR:                                                            │
│  Nombre: Juan Pérez                                                     │
│  Cédula: 001-1234567-8                                                  │
│  Email: juan@email.com                                                  │
│  Teléfono: 829-555-0100                                                 │
│                                                                          │
│  PROVEEDOR (VENDEDOR):                                                  │
│  Nombre/Razón Social: Autos del Caribe SRL                             │
│  RNC: 131-12345-6                                                       │
│  Dirección: Av. Churchill #75, Santo Domingo                           │
│                                                                          │
│  ═══════════════════════════════════════════════════════════════════   │
│                                                                          │
│  DETALLE DE LA TRANSACCIÓN:                                             │
│  Vehículo: Toyota RAV4 XLE 2023                                        │
│  VIN: 1ABCD23EFGH456789                                                │
│  Precio: RD$ 2,500,000                                                 │
│  Fecha de compra: 10/01/2026                                           │
│                                                                          │
│  ═══════════════════════════════════════════════════════════════════   │
│                                                                          │
│  DESCRIPCIÓN DE LA QUEJA:                                               │
│  El vehículo presenta daños en el motor no declarados...               │
│                                                                          │
│  EVIDENCIA ADJUNTA:                                                     │
│  1. Foto del motor con fuga de aceite (3 imágenes)                     │
│  2. Diagnóstico del mecánico certificado                               │
│  3. Captura de pantalla del anuncio original                           │
│  4. Conversaciones con el vendedor                                      │
│                                                                          │
│  RESPUESTA DEL VENDEDOR:                                                │
│  "El vehículo fue entregado en perfectas condiciones..."               │
│                                                                          │
│  INTENTOS DE MEDIACIÓN:                                                 │
│  - 18/01/2026: Primera mediación - Sin acuerdo                         │
│  - 25/01/2026: Segunda mediación - Vendedor no se presentó             │
│                                                                          │
│  ═══════════════════════════════════════════════════════════════════   │
│                                                                          │
│  SOLICITUD DEL CONSUMIDOR:                                              │
│  Reembolso completo del precio pagado (RD$ 2,500,000)                  │
│                                                                          │
│  Firma digital: [OKLA Technologies SRL]                                 │
│  Fecha: 30/01/2026                                                      │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

### 5.4 PC-004: Derecho de Retracto

| Campo       | Valor                         |
| ----------- | ----------------------------- |
| **ID**      | PC-004                        |
| **Nombre**  | Ejercer Derecho de Retracto   |
| **Actor**   | Comprador                     |
| **Trigger** | POST /api/consumer/retraction |

#### Condiciones

| Condición                         | Aplica       |
| --------------------------------- | ------------ |
| Servicios de OKLA (suscripciones) | ✅ 48 horas  |
| Compra de vehículo                | ❌ No aplica |
| Productos personalizados          | ❌ No aplica |
| Servicios ya ejecutados           | ❌ No aplica |

#### Flujo del Proceso

| Paso | Acción                    | Sistema             | Validación    |
| ---- | ------------------------- | ------------------- | ------------- |
| 1    | Usuario solicita retracto | API                 | POST          |
| 2    | Verificar tipo de compra  | ConsumerService     | Servicio OKLA |
| 3    | Verificar dentro de 48h   | ConsumerService     | Desde compra  |
| 4    | Si aplica                 | Continuar           | Proceder      |
| 5    | Cancelar servicio         | BillingService      | Stripe cancel |
| 6    | Procesar reembolso        | BillingService      | 100%          |
| 7    | Enviar confirmación       | NotificationService | Email         |
| 8    | Log para auditoría        | AuditService        | Registro      |

---

## 6. Garantías

### 6.1 Garantía Legal Mínima

| Tipo de Vehículo            | Garantía Mínima      | Cobertura          |
| --------------------------- | -------------------- | ------------------ |
| Vehículo nuevo              | 12 meses / 20,000 km | Mecánica completa  |
| Vehículo usado (Dealer)     | 3 meses / 5,000 km   | Motor, transmisión |
| Vehículo usado (Particular) | 30 días              | Vicios ocultos     |

### 6.2 Exclusiones de Garantía

| Exclusión              | Descripción                 |
| ---------------------- | --------------------------- |
| Desgaste normal        | Frenos, neumáticos, batería |
| Mal uso                | Daños por negligencia       |
| Modificaciones         | Alteraciones no autorizadas |
| Falta de mantenimiento | Sin registros de servicio   |
| Accidentes             | Después de la venta         |

---

## 7. Información Obligatoria en Listados

### 7.1 Para Dealers

| Información             | Obligatorio |
| ----------------------- | ----------- |
| RNC                     | ✅          |
| Razón social            | ✅          |
| Dirección física        | ✅          |
| Teléfono                | ✅          |
| Email                   | ✅          |
| Políticas de garantía   | ✅          |
| Políticas de devolución | ✅          |

### 7.2 Para Vehículos

| Información                       | Obligatorio |
| --------------------------------- | ----------- |
| Marca, modelo, año                | ✅          |
| Kilometraje real                  | ✅          |
| VIN                               | ✅          |
| Historial de accidentes           | ✅          |
| Reparaciones mayores              | ✅          |
| Estado general                    | ✅          |
| Precio total (sin cargos ocultos) | ✅          |

---

## 8. Eventos RabbitMQ

| Evento                 | Exchange          | Payload                       |
| ---------------------- | ----------------- | ----------------------------- |
| `complaint.created`    | `consumer.events` | `{ complaintId, type }`       |
| `complaint.responded`  | `consumer.events` | `{ complaintId }`             |
| `complaint.escalated`  | `consumer.events` | `{ complaintId, authority }`  |
| `complaint.resolved`   | `consumer.events` | `{ complaintId, resolution }` |
| `retraction.requested` | `consumer.events` | `{ orderId }`                 |
| `retraction.processed` | `consumer.events` | `{ orderId, refundAmount }`   |

---

## 9. Métricas

```
# Quejas
consumer_complaints_total{type="...", status="..."}
consumer_complaints_resolution_time_days
consumer_complaints_resolved_in_favor_percent
consumer_complaints_escalated_total

# Retracto
consumer_retractions_total
consumer_retractions_approved_total
consumer_retractions_denied_total

# Satisfacción
consumer_satisfaction_score{category="support|resolution"}
consumer_nps_score
```

---

## 10. Configuración

```json
{
  "ConsumerProtection": {
    "ComplaintDeadlineDays": 15,
    "SellerResponseHours": 48,
    "RetractionHours": 48,
    "MediationMaxAttempts": 3,
    "EscalationEnabled": true,
    "ProConsumidorEmail": "quejas@proconsumidor.gob.do"
  },
  "Warranty": {
    "NewVehicle": { "Months": 12, "Km": 20000 },
    "UsedDealer": { "Months": 3, "Km": 5000 },
    "UsedPrivate": { "Days": 30 }
  }
}
```

---

## 11. Sanciones por Incumplimiento

| Infracción          | Sanción                       |
| ------------------- | ----------------------------- |
| Publicidad engañosa | Multa 10-100 salarios mínimos |
| No honrar garantía  | Multa + reembolso obligatorio |
| Cláusulas abusivas  | Nulidad + multa               |
| No atender queja    | Multa + cierre temporal       |

---

## 📚 Referencias

- [Pro Consumidor](https://proconsumidor.gob.do) - Portal oficial
- [Ley 358-05](https://proconsumidor.gob.do/ley-358-05) - Texto completo
- [05-escrow-service.md](../05-PAGOS-FACTURACION/05-escrow-service.md) - Escrow
- [01-ley-155-17.md](01-ley-155-17.md) - AML
