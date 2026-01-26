# 📋 MÓDULO DE AUDITORÍA Y CUMPLIMIENTO - OKLA

> **Propósito:** Centralizar toda la información, reportes y evidencias que un auditor puede solicitar  
> **Reguladores:** DGII, UAF, Pro Consumidor, INDOTEL, INTRANT  
> **Última actualización:** Enero 25, 2026  
> **Estado General:** 🔴 30% Implementado

---

## 🎯 OBJETIVO DE ESTE MÓDULO

Este módulo documenta TODOS los requerimientos de auditoría que OKLA debe cumplir como:

1. **Sujeto Obligado No Financiero** (Ley 155-17 - Lavado de Activos)
2. **Responsable de Tratamiento de Datos** (Ley 172-13 - Protección de Datos)
3. **Contribuyente** (Código Tributario, Ley 11-92)
4. **Proveedor de Servicios de Comercio Electrónico** (Ley 126-02)
5. **Proveedor de Servicios al Consumidor** (Ley 358-05)

---

## 📂 ESTRUCTURA DEL MÓDULO

```
25-AUDITORIA-CUMPLIMIENTO/
├── README.md                           # Este archivo
├── 01-RESUMEN-EJECUTIVO.md             # Dashboard para auditor
├── 02-MATRIZ-OBLIGACIONES-LEGALES.md   # Todas las obligaciones por ley
├── 03-CALENDARIO-FISCAL-REPORTES.md    # Fechas límite y cronograma
├── 04-AUDITORIA-DGII.md                # Requerimientos específicos DGII
├── 05-AUDITORIA-UAF.md                 # Requerimientos UAF (Anti-lavado)
├── 06-AUDITORIA-PROTECCION-DATOS.md    # Requerimientos Ley 172-13
├── 07-AUDITORIA-PROCONSUMIDOR.md       # Requerimientos Pro Consumidor
├── 08-REPORTES-AUTOMATIZADOS.md        # Especificación de reportes automáticos
├── 09-EVIDENCIAS-CONTROLES.md          # Registro de evidencias
├── 10-MICROSERVICIOS-AUDITORIA.md      # Servicios necesarios
└── 11-DASHBOARD-AUDITORIA-UI.md        # Especificación del panel de auditoría
```

---

## 🔍 RESUMEN PARA EL AUDITOR

### ¿Qué es OKLA?

OKLA es un **marketplace digital** para compra y venta de vehículos en República Dominicana que:

| Actividad                 | Descripción                              | Implicación Legal                 |
| ------------------------- | ---------------------------------------- | --------------------------------- |
| Publica vehículos         | Dealers y particulares publican anuncios | Comercio Electrónico (Ley 126-02) |
| Cobra suscripciones       | $49-$299/mes a dealers                   | ITBIS, NCF, Retenciones           |
| Procesa pagos             | Stripe + AZUL                            | PCI-DSS, DGII                     |
| Almacena datos personales | Cédulas, RNC, fotos, documentos          | Ley 172-13                        |
| Facilita transacciones    | Conecta compradores y vendedores         | Ley 155-17 (AML)                  |
| Verifica identidades      | KYC de dealers                           | Ley 155-17 (DDC)                  |

### Volumen de Operaciones (Estimado Año 1)

| Métrica                   | Cantidad      | Monto Estimado |
| ------------------------- | ------------- | -------------- |
| Dealers suscritos         | 100-500       | $60K-$1.5M/año |
| Vehículos publicados      | 5,000-20,000  | N/A            |
| Transacciones facilitadas | 500-2,000     | $25M-$100M     |
| Usuarios registrados      | 10,000-50,000 | N/A            |

---

## 📊 ESTADO DE CUMPLIMIENTO POR REGULADOR

### 1. DGII (Dirección General de Impuestos Internos)

| Obligación                 | Estado         | Automatizado | Evidencia      |
| -------------------------- | -------------- | ------------ | -------------- |
| Emisión de NCF (B01, B02)  | 🟡 Parcial     | 🟡           | Facturas en BD |
| Declaración ITBIS (IT-1)   | 🔴 Manual      | ❌           | Pendiente      |
| Formato 606 (Compras)      | 🔴 Pendiente   | ❌           | Sin generar    |
| Formato 607 (Ventas)       | 🔴 Pendiente   | ❌           | Sin generar    |
| Formato 608 (Anulaciones)  | 🔴 Pendiente   | ❌           | Sin generar    |
| Retenciones IR-17          | 🔴 Manual      | ❌           | Pendiente      |
| e-CF (Factura Electrónica) | 🔴 No iniciado | ❌           | N/A            |

**Gap crítico:** No hay generación automática de formatos DGII.

### 2. UAF (Unidad de Análisis Financiero)

| Obligación                               | Estado       | Automatizado | Evidencia           |
| ---------------------------------------- | ------------ | ------------ | ------------------- |
| Registro como Sujeto Obligado            | 🔴 Pendiente | N/A          | Sin registro        |
| Oficial de Cumplimiento                  | 🔴 Pendiente | N/A          | Sin designar        |
| Manual de Prevención                     | 🔴 Pendiente | N/A          | Sin documento       |
| Política KYC/DDC                         | 🟡 Parcial   | 🟡           | Verificación básica |
| Matriz de Riesgo                         | 🔴 Pendiente | ❌           | Sin implementar     |
| Reporte de Transacciones (RTN)           | 🔴 Pendiente | ❌           | Sin generar         |
| Reporte de Operaciones Sospechosas (ROS) | 🔴 Pendiente | ❌           | Sin workflow        |
| Capacitación del personal                | 🔴 Pendiente | N/A          | Sin registros       |

**Gap crítico:** No está registrado como Sujeto Obligado ante la UAF.

### 3. Protección de Datos (Ley 172-13)

| Obligación                 | Estado       | Automatizado | Evidencia                 |
| -------------------------- | ------------ | ------------ | ------------------------- |
| Aviso de Privacidad        | ✅ Existe    | N/A          | /privacy                  |
| Consentimiento documentado | 🟡 Parcial   | 🟡           | Registro en BD            |
| Derechos ARCO              | 🟡 Parcial   | ❌           | Solo acceso/rectificación |
| Registro de tratamientos   | 🔴 Pendiente | ❌           | Sin registro              |
| Contratos con encargados   | 🔴 Pendiente | N/A          | Sin contratos             |
| Brechas de seguridad       | 🔴 Pendiente | ❌           | Sin protocolo             |
| Evaluación de impacto      | 🔴 Pendiente | N/A          | Sin documento             |

**Gap crítico:** No hay registro formal de tratamientos de datos.

### 4. Pro Consumidor (Ley 358-05)

| Obligación               | Estado       | Automatizado | Evidencia       |
| ------------------------ | ------------ | ------------ | --------------- |
| Términos y Condiciones   | ✅ Existe    | N/A          | /terms          |
| Política de Devoluciones | 🟡 Parcial   | N/A          | En términos     |
| Canal de quejas          | 🔴 Pendiente | ❌           | Sin implementar |
| Libro de Reclamaciones   | 🔴 Pendiente | ❌           | Sin implementar |
| Garantías publicadas     | 🟡 Parcial   | N/A          | Por dealer      |

**Gap crítico:** No hay sistema de quejas y reclamaciones.

---

## 🚨 RIESGOS IDENTIFICADOS

### Riesgo Alto 🔴

| Riesgo                          | Ley               | Sanción Potencial       | Probabilidad |
| ------------------------------- | ----------------- | ----------------------- | ------------ |
| No registrado ante UAF          | Ley 155-17        | Multa + Cierre temporal | Alta         |
| Sin Oficial de Cumplimiento     | Ley 155-17        | Responsabilidad penal   | Alta         |
| Formatos DGII no enviados       | Código Tributario | Multas + Recargos       | Alta         |
| Sin e-CF cuando sea obligatorio | DGII              | Inhabilidad fiscal      | Media        |

### Riesgo Medio 🟡

| Riesgo                     | Ley               | Sanción Potencial     | Probabilidad |
| -------------------------- | ----------------- | --------------------- | ------------ |
| Derechos ARCO incompletos  | Ley 172-13        | Multas                | Media        |
| Sin libro de reclamaciones | Ley 358-05        | Multas Pro Consumidor | Media        |
| Retenciones mal aplicadas  | Código Tributario | Recargos              | Media        |

---

## 📅 PRÓXIMAS FECHAS CRÍTICAS

| Fecha           | Obligación               | Regulador | Estado |
| --------------- | ------------------------ | --------- | ------ |
| **10 Feb 2026** | IR-17 Retenciones Enero  | DGII      | 🔴     |
| **15 Feb 2026** | Formatos 606/607 Enero   | DGII      | 🔴     |
| **20 Feb 2026** | ITBIS Enero              | DGII      | 🔴     |
| **28 Feb 2026** | Registro Sujeto Obligado | UAF       | 🔴     |
| **31 Mar 2026** | Declaración IR-2 (Anual) | DGII      | 🔴     |

---

## 🛠️ MICROSERVICIOS REQUERIDOS

### Nuevos Microservicios

| Servicio                   | Puerto | Propósito                          | Prioridad  |
| -------------------------- | ------ | ---------------------------------- | ---------- |
| **AuditService**           | 5070   | Centralizar auditoría y evidencias | 🔴 Crítica |
| **ComplianceService**      | 5071   | Gestión de cumplimiento (UAF, AML) | 🔴 Crítica |
| **FiscalReportingService** | 5072   | Generación de formatos DGII        | 🔴 Crítica |
| **DataProtectionService**  | 5073   | Gestión de datos personales y ARCO | 🟡 Alta    |

### Modificaciones a Servicios Existentes

| Servicio                | Modificación                                      | Prioridad |
| ----------------------- | ------------------------------------------------- | --------- |
| **BillingService**      | Integrar con FiscalReportingService para NCF/e-CF | 🔴        |
| **UserService**         | Agregar campos KYC, PEP, fuente de fondos         | 🔴        |
| **NotificationService** | Templates para alertas de cumplimiento            | 🟡        |
| **AdminService**        | Dashboard de auditoría                            | 🟡        |

---

## 📋 DOCUMENTOS DE ESTE MÓDULO

| #   | Documento                      | Descripción                    | Para Auditor De |
| --- | ------------------------------ | ------------------------------ | --------------- |
| 01  | RESUMEN-EJECUTIVO.md           | Dashboard con métricas clave   | Todos           |
| 02  | MATRIZ-OBLIGACIONES-LEGALES.md | Lista completa de obligaciones | Todos           |
| 03  | CALENDARIO-FISCAL-REPORTES.md  | Fechas límite de reportes      | DGII            |
| 04  | AUDITORIA-DGII.md              | Checklist específico DGII      | DGII            |
| 05  | AUDITORIA-UAF.md               | Checklist específico UAF       | UAF             |
| 06  | AUDITORIA-PROTECCION-DATOS.md  | Checklist Ley 172-13           | Datos           |
| 07  | AUDITORIA-PROCONSUMIDOR.md     | Checklist Pro Consumidor       | Consumidor      |
| 08  | REPORTES-AUTOMATIZADOS.md      | Especificación técnica         | Desarrollo      |
| 09  | EVIDENCIAS-CONTROLES.md        | Registro de evidencias         | Todos           |
| 10  | MICROSERVICIOS-AUDITORIA.md    | Arquitectura técnica           | Desarrollo      |
| 11  | DASHBOARD-AUDITORIA-UI.md      | Diseño del panel               | Desarrollo      |

---

## 👥 RESPONSABLES

| Rol                                 | Responsabilidad       | Designado             |
| ----------------------------------- | --------------------- | --------------------- |
| **Oficial de Cumplimiento**         | UAF, AML, KYC         | ⚠️ Pendiente designar |
| **Responsable Fiscal**              | DGII, NCF, Formatos   | ⚠️ Pendiente designar |
| **Delegado de Protección de Datos** | Ley 172-13, ARCO      | ⚠️ Pendiente designar |
| **Responsable Pro Consumidor**      | Quejas, reclamaciones | ⚠️ Pendiente designar |

---

## 🔗 REFERENCIAS CRUZADAS

| Documento          | Ubicación                                     |
| ------------------ | --------------------------------------------- |
| Normativas RD OKLA | /docs/NORMATIVAS_RD_OKLA.md                   |
| Ley 155-17 (AML)   | 08-COMPLIANCE-LEGAL-RD/01-ley-155-17.md       |
| Ley 172-13 (Datos) | 08-COMPLIANCE-LEGAL-RD/02-ley-172-13.md       |
| DGII Integration   | 08-COMPLIANCE-LEGAL-RD/03-dgii-integration.md |
| Pro Consumidor     | 08-COMPLIANCE-LEGAL-RD/04-proconsumidor.md    |

---

**Última revisión:** Enero 25, 2026  
**Próxima revisión:** Febrero 25, 2026  
**Responsable:** Dirección General OKLA  
**Criticidad:** 🔴 MÁXIMA
