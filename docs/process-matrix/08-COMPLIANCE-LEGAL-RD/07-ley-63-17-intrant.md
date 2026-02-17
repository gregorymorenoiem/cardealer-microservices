# 🚗 Ley 63-17 - INTRANT - Registro Vehicular - Matriz de Procesos

> **Marco Legal:** Ley 63-17 de Movilidad, Transporte Terrestre, Tránsito y Seguridad Vial  
> **Regulador:** INTRANT (Instituto Nacional de Tránsito y Transporte Terrestre)  
> **Última actualización:** Enero 25, 2026  
> **Estado:** 🟢 ACTIVO (Obligatorio para vehículos)  
> **Estado de Implementación:** 🔴 0% Backend | 🔴 0% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                                 | Backend      | UI Access | Observación     |
| --------------------------------------- | ------------ | --------- | --------------- |
| INTRANT-VER-001 Verificar Matrícula     | 🔴 Pendiente | 🔴 Falta  | Sin integración |
| INTRANT-HIST-001 Historial Propietarios | 🔴 Pendiente | 🔴 Falta  | Sin integración |
| INTRANT-MULTA-001 Multas Pendientes     | 🔴 Pendiente | 🔴 Falta  | Sin integración |
| INTRANT-REV-001 Revisión Técnica        | 🔴 Pendiente | 🔴 Falta  | Sin integración |

### Rutas UI Existentes ✅

- Ninguna

### Rutas UI Faltantes 🔴

- `/vehicles/:id/intrant-report` → Reporte INTRANT del vehículo
- `/verify/vehicle` → Verificar vehículo por placa
- `/admin/intrant/sync` → Sincronización con INTRANT

**Verificación Backend:** Sin integración con INTRANT 🔴

---

## 📊 Resumen de Implementación

| Componente                           | Total | Implementado | Pendiente | Estado        |
| ------------------------------------ | ----- | ------------ | --------- | ------------- |
| **INTRANT-VER-\*** (Verificación)    | 4     | 0            | 4         | 🔴 Pendiente  |
| **INTRANT-HIST-\*** (Historial)      | 3     | 0            | 3         | 🔴 Pendiente  |
| **INTRANT-MULTA-\*** (Multas)        | 3     | 0            | 3         | 🔴 Pendiente  |
| **INTRANT-REV-\*** (Revisión)        | 3     | 0            | 3         | 🔴 Pendiente  |
| **INTRANT-TRANS-\*** (Transferencia) | 4     | 0            | 4         | 🔴 Pendiente  |
| **Tests**                            | 15    | 0            | 15        | 🔴 Pendiente  |
| **TOTAL**                            | 32    | 0            | 32        | 🔴 0% Backend |

---

## 1. Información General

### 1.1 Descripción

La Ley 63-17 establece el marco regulatorio para el tránsito y transporte terrestre en República Dominicana. INTRANT es la entidad encargada del registro vehicular, historial de propietarios, multas y revisiones técnicas.

### 1.2 Importancia para OKLA

| Aspecto                       | Relevancia                                        |
| ----------------------------- | ------------------------------------------------- |
| **Verificación de propiedad** | Confirmar que el vendedor es el propietario legal |
| **Historial de accidentes**   | Informar al comprador sobre siniestros previos    |
| **Multas pendientes**         | Alertar sobre deudas del vehículo                 |
| **Revisión técnica**          | Verificar vigencia de la inspección               |
| **Transferencia digital**     | Facilitar cambio de propietario                   |

### 1.3 Datos de INTRANT

| Campo                   | Valor                                                 |
| ----------------------- | ----------------------------------------------------- |
| **Nombre**              | Instituto Nacional de Tránsito y Transporte Terrestre |
| **Siglas**              | INTRANT                                               |
| **Web**                 | intrant.gob.do                                        |
| **Portal de Servicios** | servicios.intrant.gob.do                              |
| **Teléfono**            | (809) 920-2020                                        |

---

## 2. Datos Disponibles en INTRANT

### 2.1 Información por Placa

| Campo            | Descripción                 | Disponible |
| ---------------- | --------------------------- | ---------- |
| Número de placa  | Identificador único         | ✅         |
| Marca            | Fabricante del vehículo     | ✅         |
| Modelo           | Modelo específico           | ✅         |
| Año              | Año de fabricación          | ✅         |
| Color            | Color registrado            | ✅         |
| VIN/Chasis       | Número de identificación    | ✅         |
| Tipo de vehículo | Sedan, SUV, etc.            | ✅         |
| Combustible      | Gasolina, diésel, eléctrico | ✅         |
| Cilindrada       | Capacidad del motor         | ✅         |

### 2.2 Información del Propietario

| Campo                          | Descripción                 | Requiere Auth |
| ------------------------------ | --------------------------- | ------------- |
| Nombre del propietario actual  | Persona o empresa           | ✅            |
| Cédula/RNC                     | Identificación fiscal       | ✅            |
| Fecha de adquisición           | Cuándo adquirió el vehículo | ✅            |
| Historial de propietarios      | Cadena de titularidad       | ✅            |
| Número de propietarios previos | Cantidad de dueños          | 🟡            |

### 2.3 Estado Legal del Vehículo

| Campo                      | Descripción                   | Crítico |
| -------------------------- | ----------------------------- | ------- |
| Multas pendientes          | Infracciones no pagadas       | 🔴 Sí   |
| Monto de multas            | Total a pagar                 | 🔴 Sí   |
| Embargos/Gravámenes        | Restricciones legales         | 🔴 Sí   |
| Reporte de robo            | Si está reportado como robado | 🔴 Sí   |
| Restricción de circulación | Prohibiciones                 | 🔴 Sí   |

### 2.4 Revisión Técnica

| Campo                 | Descripción            |
| --------------------- | ---------------------- |
| Fecha última revisión | Cuándo se inspeccionó  |
| Fecha de vencimiento  | Hasta cuándo es válida |
| Centro de inspección  | Dónde se realizó       |
| Resultado             | Aprobado/Rechazado     |
| Observaciones         | Notas del inspector    |

---

## 3. Procesos a Implementar

### 3.1 INTRANT-VER: Verificación de Vehículo

#### INTRANT-VER-001: Consulta por Placa

| Campo           | Valor                                     |
| --------------- | ----------------------------------------- |
| **Proceso**     | INTRANT-VER-001                           |
| **Nombre**      | Verificación de Vehículo por Placa        |
| **Descripción** | Consultar información básica del vehículo |
| **Estado**      | 🔴 Pendiente                              |

**Flujo Propuesto:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    VERIFICACIÓN DE VEHÍCULO                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   1️⃣ VENDEDOR PUBLICA VEHÍCULO                                          │
│   └── Ingresa número de placa                                           │
│                                                                         │
│   2️⃣ OKLA CONSULTA INTRANT                                              │
│   ├── API Request → INTRANT                                             │
│   ├── Valida datos del vehículo                                         │
│   └── Obtiene información básica                                        │
│                                                                         │
│   3️⃣ VERIFICACIÓN AUTOMÁTICA                                            │
│   ├── ✅ Placa válida → Continuar                                       │
│   ├── ⚠️ Multas pendientes → Alertar vendedor                           │
│   ├── ⚠️ Revisión vencida → Alertar                                     │
│   └── 🔴 Reporte de robo → BLOQUEAR publicación                         │
│                                                                         │
│   4️⃣ BADGE DE VERIFICACIÓN                                              │
│   ├── ✅ "Verificado INTRANT" → Si todo OK                              │
│   ├── ⚠️ "Pendientes" → Si hay multas/revisión                          │
│   └── 🔴 "No verificado" → Si no se pudo consultar                      │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

#### INTRANT-VER-002: Validación de VIN/Chasis

| Campo           | Valor                                |
| --------------- | ------------------------------------ |
| **Proceso**     | INTRANT-VER-002                      |
| **Nombre**      | Validación de VIN                    |
| **Descripción** | Verificar que VIN coincide con placa |
| **Estado**      | 🔴 Pendiente                         |

#### INTRANT-VER-003: Verificación de Propiedad

| Campo           | Valor                                    |
| --------------- | ---------------------------------------- |
| **Proceso**     | INTRANT-VER-003                          |
| **Nombre**      | Verificación de Propietario              |
| **Descripción** | Confirmar que vendedor es el propietario |
| **Estado**      | 🔴 Pendiente                             |

---

### 3.2 INTRANT-HIST: Historial del Vehículo

#### INTRANT-HIST-001: Historial de Propietarios

| Campo           | Valor                                     |
| --------------- | ----------------------------------------- |
| **Proceso**     | INTRANT-HIST-001                          |
| **Nombre**      | Cadena de Titularidad                     |
| **Descripción** | Mostrar todos los propietarios anteriores |
| **Estado**      | 🔴 Pendiente                              |

**Información a Mostrar:**

| Campo                                 | Visible para            |
| ------------------------------------- | ----------------------- |
| Número de propietarios                | Todos                   |
| Fechas de transferencia               | Compradores verificados |
| Provincias de registro                | Todos                   |
| Tipo de propietario (persona/empresa) | Todos                   |
| Nombres de propietarios               | Solo con autorización   |

#### INTRANT-HIST-002: Historial de Accidentes

| Campo           | Valor                               |
| --------------- | ----------------------------------- |
| **Proceso**     | INTRANT-HIST-002                    |
| **Nombre**      | Reporte de Siniestros               |
| **Descripción** | Accidentes registrados oficialmente |
| **Estado**      | 🔴 Pendiente                        |

---

### 3.3 INTRANT-MULTA: Multas y Deudas

#### INTRANT-MULTA-001: Consulta de Multas

| Campo           | Valor                             |
| --------------- | --------------------------------- |
| **Proceso**     | INTRANT-MULTA-001                 |
| **Nombre**      | Verificación de Multas Pendientes |
| **Descripción** | Consultar infracciones no pagadas |
| **Estado**      | 🔴 Pendiente                      |

**Datos de Multa:**

| Campo               | Descripción          |
| ------------------- | -------------------- |
| Número de multa     | Identificador        |
| Fecha de infracción | Cuándo ocurrió       |
| Tipo de infracción  | Descripción          |
| Monto original      | Valor de la multa    |
| Recargos            | Intereses acumulados |
| Monto total         | Total a pagar        |
| Estado              | Pendiente/Pagada     |

#### INTRANT-MULTA-002: Alerta de Multas

| Campo           | Valor                              |
| --------------- | ---------------------------------- |
| **Proceso**     | INTRANT-MULTA-002                  |
| **Nombre**      | Notificación de Multas             |
| **Descripción** | Alertar a compradores sobre multas |
| **Estado**      | 🔴 Pendiente                       |

**UI Propuesta:**

```
┌─────────────────────────────────────────────────────────────┐
│  ⚠️ ATENCIÓN: Este vehículo tiene multas pendientes        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Multas encontradas: 3                                      │
│  Monto total: RD$ 15,500                                    │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ • Exceso de velocidad - RD$ 5,000 (12/03/2025)      │  │
│  │ • Estacionamiento prohibido - RD$ 2,500 (05/01/2025)│  │
│  │ • Luz roja - RD$ 8,000 (22/11/2024)                 │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  ℹ️ Antes de la transferencia, estas multas deben ser      │
│     pagadas por el vendedor actual.                        │
│                                                             │
│  [Contactar Vendedor]  [Ver Detalle de Multas]             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

### 3.4 INTRANT-REV: Revisión Técnica

#### INTRANT-REV-001: Estado de Revisión

| Campo           | Valor                                      |
| --------------- | ------------------------------------------ |
| **Proceso**     | INTRANT-REV-001                            |
| **Nombre**      | Verificación de Revisión Técnica           |
| **Descripción** | Consultar vigencia de inspección vehicular |
| **Estado**      | 🔴 Pendiente                               |

**Estados de Revisión:**

| Estado               | Icono | Descripción         |
| -------------------- | ----- | ------------------- |
| Vigente              | ✅    | Revisión válida     |
| Por vencer (30 días) | ⚠️    | Próxima a vencer    |
| Vencida              | 🔴    | Requiere inspección |
| Sin revisión         | ❌    | Nunca inspeccionado |

---

### 3.5 INTRANT-TRANS: Transferencia de Propiedad

#### INTRANT-TRANS-001: Pre-verificación de Transferencia

| Campo           | Valor                                      |
| --------------- | ------------------------------------------ |
| **Proceso**     | INTRANT-TRANS-001                          |
| **Nombre**      | Verificación Pre-Transferencia             |
| **Descripción** | Validar que vehículo puede ser transferido |
| **Estado**      | 🔴 Pendiente                               |

**Checklist de Transferencia:**

| Requisito             | Verificación     |
| --------------------- | ---------------- |
| Sin multas pendientes | Consulta INTRANT |
| Sin embargos          | Consulta INTRANT |
| Revisión vigente      | Consulta INTRANT |
| No reportado robado   | Consulta INTRANT |
| Impuestos al día      | Consulta DGII    |
| Documentos en regla   | Manual           |

#### INTRANT-TRANS-002: Asistencia de Transferencia

| Campo           | Valor                                         |
| --------------- | --------------------------------------------- |
| **Proceso**     | INTRANT-TRANS-002                             |
| **Nombre**      | Guía de Transferencia                         |
| **Descripción** | Ayudar en el proceso de cambio de propietario |
| **Estado**      | 🔴 Pendiente                                  |

**Documentos Requeridos para Transferencia:**

| Documento                              | Quién Proporciona |
| -------------------------------------- | ----------------- |
| Matrícula original                     | Vendedor          |
| Cédula del vendedor                    | Vendedor          |
| Cédula del comprador                   | Comprador         |
| Contrato de compraventa                | Ambos             |
| Pago de impuesto de transferencia (2%) | Comprador         |
| Paz y salvo de multas                  | INTRANT           |

---

## 4. Integración Técnica

### 4.1 API de INTRANT

**Nota:** INTRANT no tiene API pública oficial. Opciones de integración:

| Opción                | Descripción                     | Viabilidad     |
| --------------------- | ------------------------------- | -------------- |
| **API oficial**       | Solicitar acceso institucional  | 🟡 En trámite  |
| **Web scraping**      | Extraer datos del portal        | ⚠️ Riesgoso    |
| **Proveedor tercero** | Usar servicio intermediario     | ✅ Recomendado |
| **Manual**            | Verificación por empleados OKLA | 🟡 Temporal    |

### 4.2 Servicio Backend Propuesto

```csharp
// IntrantService.cs
public interface IIntrantService
{
    Task<VehicleInfoResult> GetVehicleByPlate(string plate);
    Task<OwnershipHistoryResult> GetOwnershipHistory(string plate);
    Task<FinesResult> GetPendingFines(string plate);
    Task<InspectionResult> GetInspectionStatus(string plate);
    Task<TransferCheckResult> ValidateTransfer(string plate);
    Task<bool> IsReportedStolen(string plate);
}
```

### 4.3 Endpoints API Propuestos

| Método | Endpoint                                      | Descripción               | Auth |
| ------ | --------------------------------------------- | ------------------------- | ---- |
| `GET`  | `/api/intrant/vehicle/{plate}`                | Info básica del vehículo  | ✅   |
| `GET`  | `/api/intrant/vehicle/{plate}/history`        | Historial de propietarios | ✅   |
| `GET`  | `/api/intrant/vehicle/{plate}/fines`          | Multas pendientes         | ✅   |
| `GET`  | `/api/intrant/vehicle/{plate}/inspection`     | Estado de revisión        | ✅   |
| `GET`  | `/api/intrant/vehicle/{plate}/transfer-check` | Validar transferencia     | ✅   |
| `POST` | `/api/intrant/verify-ownership`               | Verificar propiedad       | ✅   |

---

## 5. Componentes UI Propuestos

### 5.1 Badge de Verificación INTRANT

```typescript
// IntrantBadge.tsx
interface IntrantBadgeProps {
  status: "verified" | "pending" | "issues" | "not-verified";
  finesCount?: number;
  inspectionExpired?: boolean;
}

// Estados del badge:
// ✅ verified - Todo en orden
// ⚠️ pending - Verificación en proceso
// 🔴 issues - Multas o problemas
// ❌ not-verified - No se pudo verificar
```

### 5.2 Reporte INTRANT en Detalle de Vehículo

| Sección            | Contenido                         |
| ------------------ | --------------------------------- |
| Información Básica | Placa, VIN, marca, modelo, año    |
| Estado Legal       | Multas, embargos, reporte de robo |
| Revisión Técnica   | Fecha, vigencia, centro           |
| Historial          | Número de propietarios, fechas    |
| Transferencia      | Checklist de requisitos           |

### 5.3 Verificador de Vehículo Público

Página pública donde cualquiera puede verificar un vehículo:

```
URL: /verify/vehicle?plate=A123456
```

---

## 6. Cronograma de Implementación

### Fase 1: Q1 2026 - Investigación

- [ ] Contactar INTRANT para acceso API
- [ ] Evaluar proveedores terceros
- [ ] Definir alcance de integración
- [ ] Diseñar arquitectura

### Fase 2: Q2 2026 - Backend

- [ ] Implementar IntrantService
- [ ] Integrar con proveedor de datos
- [ ] Crear endpoints API
- [ ] Tests de integración

### Fase 3: Q3 2026 - Frontend

- [ ] IntrantBadge component
- [ ] Sección INTRANT en vehicle detail
- [ ] Página de verificación pública
- [ ] Alertas de multas/revisión

### Fase 4: Q4 2026 - Producción

- [ ] Beta con dealers
- [ ] Feedback y ajustes
- [ ] Lanzamiento general
- [ ] Monitoreo y mejoras

---

## 7. Consideraciones Legales

### 7.1 Protección de Datos

| Dato                 | Sensibilidad | Mostrar                 |
| -------------------- | ------------ | ----------------------- |
| Placa                | Pública      | ✅ Todos                |
| VIN                  | Semi-pública | ✅ Todos                |
| Nombre propietario   | Privada      | Solo con autorización   |
| Cédula propietario   | Muy privada  | ❌ Nunca                |
| Multas pendientes    | Semi-privada | Compradores verificados |
| Historial accidentes | Semi-privada | Compradores verificados |

### 7.2 Consentimiento del Vendedor

El vendedor debe autorizar que OKLA:

- Consulte la información del vehículo en INTRANT
- Muestre el estado de multas a compradores potenciales
- Verifique que es el propietario registrado

---

## 8. Métricas de Éxito

| Métrica                        | Objetivo     |
| ------------------------------ | ------------ |
| % de vehículos verificados     | 90%          |
| Tiempo de verificación         | < 5 segundos |
| Reducción de fraudes           | 50%          |
| Satisfacción del comprador     | +15 NPS      |
| Vehículos bloqueados (robados) | 100%         |

---

## 9. Referencias

| Recurso            | URL                           |
| ------------------ | ----------------------------- |
| Ley 63-17          | congreso.gob.do               |
| Portal INTRANT     | intrant.gob.do                |
| Servicios en línea | servicios.intrant.gob.do      |
| Consulta de multas | consultamultas.intrant.gob.do |

---

**Última revisión:** Enero 25, 2026  
**Próxima revisión:** Abril 25, 2026  
**Responsable:** Equipo de Desarrollo OKLA  
**Prioridad:** 🟡 MEDIA (Para Q2 2026)
