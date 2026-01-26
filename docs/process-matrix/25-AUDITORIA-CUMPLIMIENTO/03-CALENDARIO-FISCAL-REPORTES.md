# 📅 Calendario Fiscal y de Reportes - OKLA

> **Propósito:** Centralizar todas las fechas límite de obligaciones regulatorias  
> **Automatización:** Este calendario debe alimentar un sistema de alertas  
> **Última actualización:** Enero 25, 2026

---

## 📊 RESUMEN ANUAL 2026

### Vista por Mes

| Mes        | DGII | UAF | Otros | Total Obligaciones |
| ---------- | ---- | --- | ----- | ------------------ |
| Enero      | 4    | 0   | 0     | 4                  |
| Febrero    | 4    | 0   | 0     | 4                  |
| Marzo      | 5    | 1   | 0     | 6                  |
| Abril      | 5    | 0   | 1     | 6                  |
| Mayo       | 4    | 0   | 0     | 4                  |
| Junio      | 4    | 1   | 0     | 5                  |
| Julio      | 4    | 0   | 0     | 4                  |
| Agosto     | 4    | 0   | 0     | 4                  |
| Septiembre | 4    | 1   | 0     | 5                  |
| Octubre    | 4    | 0   | 0     | 4                  |
| Noviembre  | 4    | 0   | 0     | 4                  |
| Diciembre  | 4    | 1   | 1     | 6                  |

---

## 📆 CALENDARIO MENSUAL RECURRENTE

### Obligaciones DGII (Cada Mes)

| Día    | Obligación                | Formulario | Período Reportado | Automatizable |
| ------ | ------------------------- | ---------- | ----------------- | ------------- |
| **10** | Retenciones ISR           | IR-17      | Mes anterior      | ✅ Sí         |
| **15** | Formato 606 (Compras)     | 606        | Mes anterior      | ✅ Sí         |
| **15** | Formato 607 (Ventas)      | 607        | Mes anterior      | ✅ Sí         |
| **15** | Formato 608 (Anulaciones) | 608        | Mes anterior      | ✅ Sí         |
| **20** | Declaración ITBIS         | IT-1       | Mes anterior      | 🟡 Semi       |
| **20** | Pago ITBIS                | -          | Mes anterior      | ❌ Manual     |

### Flujo Mensual

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    FLUJO MENSUAL DGII                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Día 1-5: Cierre del mes anterior                                       │
│  ├── Verificar todas las facturas emitidas                             │
│  ├── Verificar todas las compras registradas                           │
│  └── Conciliar secuencias NCF                                          │
│                                                                         │
│  Día 6-9: Preparación de reportes                                       │
│  ├── Generar Formato 606 (Compras)                                     │
│  ├── Generar Formato 607 (Ventas)                                      │
│  ├── Generar Formato 608 (Anulaciones)                                 │
│  └── Calcular retenciones IR-17                                        │
│                                                                         │
│  Día 10: Envío IR-17                                                    │
│  └── Enviar y pagar retenciones                                        │
│                                                                         │
│  Día 11-14: Validación de formatos                                      │
│  ├── Validar estructura 606/607/608                                    │
│  └── Corregir errores si hay                                           │
│                                                                         │
│  Día 15: Envío Formatos                                                 │
│  └── Enviar 606, 607, 608 a DGII                                       │
│                                                                         │
│  Día 16-19: Preparación ITBIS                                           │
│  ├── Calcular ITBIS cobrado (ventas)                                   │
│  ├── Calcular ITBIS pagado (compras)                                   │
│  └── Determinar ITBIS a pagar o a favor                                │
│                                                                         │
│  Día 20: Declaración y pago ITBIS                                       │
│  ├── Enviar IT-1                                                       │
│  └── Pagar diferencia si aplica                                        │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📅 CALENDARIO 2026 DETALLADO

### Enero 2026

| Fecha  | Obligación                      | Regulador | Estado                          |
| ------ | ------------------------------- | --------- | ------------------------------- |
| 10 Ene | IR-17 (Dic 2025)                | DGII      | ✅ Primer mes, puede no aplicar |
| 15 Ene | Formatos 606/607/608 (Dic 2025) | DGII      | ✅ Primer mes, puede no aplicar |
| 20 Ene | ITBIS (Dic 2025)                | DGII      | ✅ Primer mes, puede no aplicar |

### Febrero 2026

| Fecha  | Obligación        | Regulador | Período    | Estado       |
| ------ | ----------------- | --------- | ---------- | ------------ |
| 10 Feb | IR-17             | DGII      | Enero 2026 | 🔴 Pendiente |
| 15 Feb | Formato 606       | DGII      | Enero 2026 | 🔴 Pendiente |
| 15 Feb | Formato 607       | DGII      | Enero 2026 | 🔴 Pendiente |
| 15 Feb | Formato 608       | DGII      | Enero 2026 | 🔴 Pendiente |
| 20 Feb | IT-1 + Pago ITBIS | DGII      | Enero 2026 | 🔴 Pendiente |

### Marzo 2026

| Fecha  | Obligación            | Regulador | Período      | Estado             |
| ------ | --------------------- | --------- | ------------ | ------------------ |
| 10 Mar | IR-17                 | DGII      | Febrero 2026 | 🔴 Pendiente       |
| 15 Mar | Formatos 606/607/608  | DGII      | Febrero 2026 | 🔴 Pendiente       |
| 20 Mar | IT-1 + Pago ITBIS     | DGII      | Febrero 2026 | 🔴 Pendiente       |
| 31 Mar | **IR-2 (Anual)**      | DGII      | Año 2025     | ⚠️ Si inició antes |
| 31 Mar | **Informe Anual UAF** | UAF       | Año 2025     | ⚠️ Si aplica       |

### Abril 2026 - Diciembre 2026

_[Mismo patrón mensual: Día 10 IR-17, Día 15 Formatos, Día 20 ITBIS]_

### Obligaciones Especiales

| Fecha  | Obligación                     | Regulador | Descripción                        |
| ------ | ------------------------------ | --------- | ---------------------------------- |
| 31 Mar | IR-2 (Declaración Anual ISR)   | DGII      | 120 días después del cierre fiscal |
| 30 Jun | Informe Semestral Cumplimiento | UAF       | Si lo requiere la UAF              |
| 30 Sep | Informe Semestral Cumplimiento | UAF       | Si lo requiere la UAF              |
| 31 Dic | Actualización Manual AML       | UAF       | Revisión anual obligatoria         |
| 31 Dic | Renovación Patente Municipal   | Municipio | Antes de vencer                    |

---

## ⏰ SISTEMA DE ALERTAS PROPUESTO

### Niveles de Alerta

| Días Antes | Nivel          | Acción                                    |
| ---------- | -------------- | ----------------------------------------- |
| 15 días    | 🟢 Informativo | Notificar a responsable fiscal            |
| 7 días     | 🟡 Advertencia | Notificar a responsable + gerencia        |
| 3 días     | 🟠 Urgente     | Notificar a todos + bloquear otras tareas |
| 1 día      | 🔴 Crítico     | Escalamiento a dirección                  |
| 0 días     | ⚫ Vencido     | Alerta máxima + plan de remediación       |

### Configuración de Notificaciones

```yaml
# Configuración de alertas fiscales
fiscal_alerts:
  ir17:
    name: "Retenciones IR-17"
    due_day: 10
    alerts: [15, 7, 3, 1]
    recipients: ["fiscal@okla.com.do", "gerencia@okla.com.do"]

  formato_606:
    name: "Formato 606 - Compras"
    due_day: 15
    alerts: [15, 7, 3, 1]
    recipients: ["fiscal@okla.com.do"]

  formato_607:
    name: "Formato 607 - Ventas"
    due_day: 15
    alerts: [15, 7, 3, 1]
    recipients: ["fiscal@okla.com.do"]

  formato_608:
    name: "Formato 608 - Anulaciones"
    due_day: 15
    alerts: [15, 7, 3, 1]
    recipients: ["fiscal@okla.com.do"]

  itbis:
    name: "Declaración ITBIS IT-1"
    due_day: 20
    alerts: [15, 7, 3, 1]
    recipients: ["fiscal@okla.com.do", "gerencia@okla.com.do"]
```

---

## 📋 CHECKLIST POR OBLIGACIÓN

### Formato 606 (Compras)

```
□ Exportar todas las compras del período
□ Verificar RNC de proveedores
□ Clasificar por tipo de NCF recibido
□ Calcular ITBIS adelantado
□ Generar archivo en formato DGII
□ Validar estructura del archivo
□ Enviar por Oficina Virtual
□ Guardar acuse de recibo
□ Archivar copia del formato
```

### Formato 607 (Ventas)

```
□ Exportar todas las ventas del período
□ Verificar NCF emitidos
□ Clasificar por tipo de NCF (B01, B02, B04)
□ Incluir RNC/Cédula de clientes
□ Calcular ITBIS cobrado
□ Generar archivo en formato DGII
□ Validar estructura del archivo
□ Enviar por Oficina Virtual
□ Guardar acuse de recibo
□ Archivar copia del formato
```

### Formato 608 (Anulaciones)

```
□ Identificar NCF anulados en el período
□ Documentar razón de anulación
□ Generar archivo en formato DGII
□ Validar estructura del archivo
□ Enviar por Oficina Virtual
□ Guardar acuse de recibo
□ Archivar copia del formato
```

---

## 📊 DASHBOARD DE CALENDARIO (UI Propuesta)

```
┌─────────────────────────────────────────────────────────────────────────┐
│  📅 CALENDARIO FISCAL - Febrero 2026                                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Lun   Mar   Mié   Jue   Vie   Sáb   Dom                               │
│  ─────────────────────────────────────────                             │
│                                    1     2                              │
│                                                                         │
│   3     4     5     6     7     8     9                                │
│                                                                         │
│  10🔴  11    12    13    14    15🔴  16                                │
│  IR-17                          606                                     │
│                                 607                                     │
│                                 608                                     │
│                                                                         │
│  17    18    19    20🔴  21    22    23                                │
│                    ITBIS                                                │
│                                                                         │
│  24    25    26    27    28                                            │
│                                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  PRÓXIMAS OBLIGACIONES                                                  │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ 🔴 10 Feb - IR-17 Enero          [15 días] [Preparar]            │  │
│  │ 🔴 15 Feb - Formato 606 Enero    [20 días] [Generar]             │  │
│  │ 🔴 15 Feb - Formato 607 Enero    [20 días] [Generar]             │  │
│  │ 🔴 15 Feb - Formato 608 Enero    [20 días] [Generar]             │  │
│  │ 🔴 20 Feb - ITBIS Enero          [25 días] [Calcular]            │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  OBLIGACIONES VENCIDAS                                                  │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ ✅ Sin obligaciones vencidas                                      │  │
│  │                                                                   │  │
│  │ Nota: UAF probablemente NO aplica a OKLA (plataforma de          │  │
│  │ clasificados). Ver 05-AUDITORIA-UAF.md para análisis completo.   │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 🔧 INTEGRACIÓN CON MICROSERVICIOS

### FiscalReportingService

El servicio debe:

1. **Generar automáticamente** los formatos 606/607/608
2. **Validar** la estructura antes del envío
3. **Alertar** sobre fechas límite
4. **Registrar** el envío y acuse de recibo
5. **Exportar** en formatos requeridos por DGII

### Endpoints Propuestos

| Endpoint                           | Descripción                | Trigger        |
| ---------------------------------- | -------------------------- | -------------- |
| `GET /api/fiscal/calendar`         | Obtener calendario del mes | Manual         |
| `GET /api/fiscal/upcoming`         | Próximas obligaciones      | Automático     |
| `POST /api/fiscal/606/generate`    | Generar Formato 606        | Día 5 del mes  |
| `POST /api/fiscal/607/generate`    | Generar Formato 607        | Día 5 del mes  |
| `POST /api/fiscal/608/generate`    | Generar Formato 608        | Día 5 del mes  |
| `POST /api/fiscal/itbis/calculate` | Calcular ITBIS             | Día 16 del mes |

---

## 📁 ARCHIVOS DE EVIDENCIA

Cada obligación cumplida debe generar:

| Obligación  | Archivos a Conservar                 |
| ----------- | ------------------------------------ |
| Formato 606 | Archivo TXT, Acuse DGII, PDF resumen |
| Formato 607 | Archivo TXT, Acuse DGII, PDF resumen |
| Formato 608 | Archivo TXT, Acuse DGII, PDF resumen |
| IT-1        | PDF declaración, Recibo de pago      |
| IR-17       | PDF declaración, Recibo de pago      |
| IR-2        | PDF declaración, Estados financieros |

**Retención:** 10 años según Código Tributario

---

**Última revisión:** Enero 25, 2026  
**Próxima revisión:** Mensual  
**Responsable:** Responsable Fiscal (pendiente designar)
