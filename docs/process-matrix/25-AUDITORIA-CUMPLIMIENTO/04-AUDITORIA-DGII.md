# 🏛️ Auditoría DGII - Checklist Completo

> **Propósito:** Documento para preparar y responder auditorías de DGII  
> **Aplicable a:** Inspecciones fiscales, revisiones de escritorio, auditorías integrales  
> **Última actualización:** Enero 25, 2026

---

## ✅ MODELO DE NEGOCIO OKLA - CONTEXTO FISCAL

> **OKLA es una plataforma de anuncios clasificados** (similar a SuperCarros.com). Sus ingresos provienen de:
>
> - Publicación de anuncios individuales: $29/anuncio
> - Suscripciones de dealers: $49-$299/mes
> - Boosts de publicaciones
>
> **OKLA NO participa** en las transacciones de compra/venta de vehículos. Esas ocurren directamente entre dealers y compradores.
>
> **Todas las obligaciones fiscales de la DGII SÍ aplican a OKLA** como cualquier empresa dominicana que presta servicios digitales.

---

## 📋 INFORMACIÓN GENERAL DEL CONTRIBUYENTE

### Datos de Registro (Certificado Mercantil 196339PSD)

| Campo                   | Valor                                          | Documentación         |
| ----------------------- | ---------------------------------------------- | --------------------- |
| **RNC**                 | 1-33-32590-1                                   | Certificado RNC ✅    |
| **Razón Social**        | OKLA S.R.L.                                    | Acta Constitutiva ✅  |
| **Nombre Comercial**    | OKLA (Registro 842576)                         | Registro Mercantil ✅ |
| **Actividad Económica** | Comercio, Servicio - Comercio Electrónico      | RM 196339PSD          |
| **Capital Social**      | RD$100,000.00                                  | Acta Constitutiva     |
| **Fecha Constitución**  | 3 de Enero 2025                                | Acta Constitutiva ✅  |
| **Régimen Tributario**  | Ordinario                                      | DGII                  |
| **Tipo Contribuyente**  | Persona Jurídica (SRL)                         | RNC ✅                |
| **Gerente**             | Nicauris Mateo Alcántara                       | RM                    |
| **Domicilio Fiscal**    | Calle Respaldo Anacaona No. 32, Sabana Perdida | RM                    |
| **Municipio**           | Santo Domingo Norte                            | RM                    |
| **Vigencia RM**         | 30/07/2025 - 30/07/2027                        | Certificado RM ✅     |

### Obligaciones Fiscales Aplicables

| Impuesto            | Aplica | Frecuencia | Estado Actual        |
| ------------------- | ------ | ---------- | -------------------- |
| ITBIS (18%)         | ✅     | Mensual    | 🔴 Pendiente sistema |
| ISR Jurídicas (27%) | ✅     | Anual      | 🔴 Pendiente         |
| Retenciones ISR     | ✅     | Mensual    | 🔴 Pendiente sistema |
| Anticipo ISR        | ✅     | Mensual    | 🔴 Pendiente         |
| Impuesto Selectivo  | ❌     | -          | No aplica            |
| Impuesto Activos    | ✅     | Anual      | 🔴 Pendiente         |

---

## 📄 COMPROBANTES FISCALES (NCF)

### Secuencias Autorizadas

| Tipo NCF | Descripción      | Serie Autorizada        | Usados | Disponibles | Vencimiento |
| -------- | ---------------- | ----------------------- | ------ | ----------- | ----------- |
| B01      | Crédito Fiscal   | B0100000001-B0100000500 | XX     | XXX         | XX/XX/20XX  |
| B02      | Consumidor Final | B0200000001-B0200000500 | XX     | XXX         | XX/XX/20XX  |
| B04      | Nota de Crédito  | B0400000001-B0400000100 | X      | XX          | XX/XX/20XX  |
| B14      | Régimen Especial | ❌ No solicitado        | -      | -           | -           |
| B15      | Gubernamental    | ❌ No solicitado        | -      | -           | -           |

### Verificación de NCF

| Verificación              | Estado | Observación       |
| ------------------------- | ------ | ----------------- |
| Secuencias no duplicadas  | ⚠️     | Verificar en BD   |
| Secuencias consecutivas   | ⚠️     | Verificar en BD   |
| NCF anulados documentados | 🔴     | Sin proceso       |
| Vencimiento de secuencias | ⚠️     | Verificar fechas  |
| NCF válidos en DGII       | ⚠️     | Validar uno a uno |

### Consulta para Validar NCF

```sql
-- Verificar secuencias usadas
SELECT
    ncf_type,
    MIN(ncf_number) as first_used,
    MAX(ncf_number) as last_used,
    COUNT(*) as total_used,
    COUNT(CASE WHEN status = 'cancelled' THEN 1 END) as cancelled
FROM invoices
WHERE created_at >= '2026-01-01'
GROUP BY ncf_type
ORDER BY ncf_type;

-- Verificar gaps en secuencias
SELECT
    ncf_number,
    LAG(ncf_number) OVER (ORDER BY ncf_number) as prev_ncf,
    ncf_number - LAG(ncf_number) OVER (ORDER BY ncf_number) as gap
FROM invoices
WHERE ncf_type = 'B01'
HAVING gap > 1;
```

---

## 📊 FORMATOS INFORMATIVOS DGII

### Estado de Formatos

| Formato | Descripción                   | Generación | Estado          |
| ------- | ----------------------------- | ---------- | --------------- |
| **606** | Compras de bienes y servicios | 🔴 Manual  | No automatizado |
| **607** | Ventas de bienes y servicios  | 🔴 Manual  | No automatizado |
| **608** | Comprobantes anulados         | 🔴 Manual  | No automatizado |
| **609** | Pagos al exterior             | N/A        | No aplica       |
| **623** | Retenciones y percepciones    | 🔴 Manual  | No automatizado |

### Estructura Formato 606

```
Campos requeridos:
- RNC/Cédula del proveedor
- Tipo de identificación (1=RNC, 2=Cédula)
- NCF del comprobante
- Tipo de NCF (01, 02, 04, etc.)
- Fecha del comprobante
- Fecha de pago
- Monto facturado sin ITBIS
- ITBIS facturado
- ITBIS retenido
- Tipo de retención
- Monto pagado
```

### Estructura Formato 607

```
Campos requeridos:
- RNC/Cédula del cliente (si B01)
- Tipo de identificación
- NCF
- Tipo de NCF
- Fecha del comprobante
- Fecha de vencimiento (si crédito)
- Monto facturado sin ITBIS
- ITBIS facturado
- Tipo de ingreso
```

### Estructura Formato 608

```
Campos requeridos:
- NCF anulado
- Tipo de NCF
- Fecha de anulación
- Motivo de anulación
```

---

## 💰 ITBIS (Impuesto a la Transferencia de Bienes y Servicios)

### Servicios Prestados por OKLA

| Servicio               | ITBIS Aplica | Tasa | Observación                |
| ---------------------- | ------------ | ---- | -------------------------- |
| Suscripción Dealer     | ✅           | 18%  | Servicio digital           |
| Boost de publicación   | ✅           | 18%  | Servicio publicitario      |
| Publicación individual | ✅           | 18%  | Servicio clasificado       |
| Comisión por venta     | ✅           | 18%  | Servicio de intermediación |

### Cálculo de ITBIS Mensual

```
ITBIS a Pagar = ITBIS Cobrado - ITBIS Pagado

Donde:
- ITBIS Cobrado = 18% × Ventas Gravadas
- ITBIS Pagado = 18% × Compras con NCF válido

Si resultado es negativo = Crédito fiscal a favor
Si resultado es positivo = ITBIS a pagar a DGII
```

### Consulta para Calcular ITBIS

```sql
-- ITBIS Cobrado (Ventas)
SELECT
    DATE_TRUNC('month', created_at) as period,
    SUM(subtotal) as gross_sales,
    SUM(itbis_amount) as itbis_collected
FROM invoices
WHERE status = 'completed'
GROUP BY DATE_TRUNC('month', created_at);

-- ITBIS Pagado (Compras)
SELECT
    DATE_TRUNC('month', date) as period,
    SUM(amount) as total_purchases,
    SUM(itbis) as itbis_paid
FROM expenses
WHERE has_valid_ncf = true
GROUP BY DATE_TRUNC('month', date);
```

---

## 📝 RETENCIONES ISR (IR-17)

### Tipos de Retención Aplicables

| Concepto                | Tasa | Aplica a             | Estado             |
| ----------------------- | ---- | -------------------- | ------------------ |
| Servicios profesionales | 10%  | Personas físicas     | 🔴 No implementado |
| Alquileres              | 10%  | Propietarios         | ❌ No aplica       |
| Servicios técnicos      | 2%   | Empresas             | 🔴 No implementado |
| Pagos al exterior       | 27%  | Proveedores externos | ❌ No aplica aún   |

### Proceso de Retención (A Implementar)

1. **Identificar pagos sujetos a retención**
   - Pagos a proveedores de servicios
   - Pagos a freelancers

2. **Calcular retención**
   - Aplicar tasa según concepto
   - Emitir comprobante de retención

3. **Declarar y pagar**
   - Formulario IR-17 antes del día 10
   - Pago electrónico en Oficina Virtual

---

## 📁 DOCUMENTACIÓN PARA AUDITORÍA

### Documentos Corporativos

| Documento                 | Requerido | Disponible | Ubicación          |
| ------------------------- | --------- | ---------- | ------------------ |
| Acta Constitutiva         | ✅        | ⚠️         | Archivo físico     |
| RNC (Certificado)         | ✅        | ⚠️         | DGII               |
| Registro Mercantil        | ✅        | ⚠️         | Cámara de Comercio |
| Patente Municipal         | ✅        | ⚠️         | Ayuntamiento       |
| Poderes de representación | ✅        | ⚠️         | Notaría            |

### Documentos Contables

| Documento            | Requerido | Disponible | Ubicación     |
| -------------------- | --------- | ---------- | ------------- |
| Estados Financieros  | ✅        | 🔴         | Pendiente     |
| Balance General      | ✅        | 🔴         | Pendiente     |
| Estado de Resultados | ✅        | 🔴         | Pendiente     |
| Auxiliar de Ventas   | ✅        | ✅         | Base de datos |
| Auxiliar de Compras  | ✅        | 🟡         | Parcial       |
| Libro Diario         | ✅        | 🔴         | Pendiente     |
| Libro Mayor          | ✅        | 🔴         | Pendiente     |

### Documentos Fiscales

| Documento             | Requerido | Disponible | Ubicación     |
| --------------------- | --------- | ---------- | ------------- |
| Declaraciones IT-1    | ✅        | 🔴         | Pendiente     |
| Declaraciones IR-17   | ✅        | 🔴         | Pendiente     |
| Formatos 606/607/608  | ✅        | 🔴         | Pendiente     |
| Acuses de recibo DGII | ✅        | 🔴         | Pendiente     |
| Recibos de pago DGII  | ✅        | 🔴         | Pendiente     |
| Secuencias NCF        | ✅        | ✅         | Base de datos |

---

## 🔍 CHECKLIST DE AUDITORÍA DGII

### Pre-Auditoría (Preparación)

```
□ Designar responsable de atención al auditor
□ Preparar espacio físico (si auditoría presencial)
□ Recopilar documentos corporativos
□ Imprimir estados financieros del período
□ Generar reportes de ventas y compras
□ Preparar conciliaciones bancarias
□ Verificar secuencias de NCF
□ Organizar facturas por mes
□ Preparar acceso a sistemas (solo lectura)
```

### Durante la Auditoría

```
□ Registrar hora de inicio y funcionarios presentes
□ Solicitar identificación del auditor y orden de auditoría
□ No entregar documentos originales (solo copias)
□ Documentar toda solicitud por escrito
□ Responder solo lo preguntado
□ Solicitar plazo si se requiere información adicional
□ Mantener registro de documentos entregados
□ No firmar nada sin revisar detenidamente
```

### Post-Auditoría

```
□ Revisar acta de cierre
□ Solicitar copia del informe preliminar
□ Preparar respuesta a observaciones
□ Consultar con asesor fiscal si hay hallazgos
□ Cumplir plazos de respuesta
□ Archivar toda la documentación
□ Implementar mejoras sugeridas
```

---

## ⚠️ RIESGOS FISCALES IDENTIFICADOS

### Alto Riesgo

| Riesgo                  | Descripción                    | Sanción Potencial               | Mitigación                         |
| ----------------------- | ------------------------------ | ------------------------------- | ---------------------------------- |
| Formatos no enviados    | 606/607/608 pendientes         | Multa 0.25% ingresos            | Implementar FiscalReportingService |
| Secuencias incorrectas  | Posibles gaps en NCF           | Multa + recargos                | Validar consecutividad             |
| Sin contabilidad formal | Estados financieros pendientes | Multa + determinación de oficio | Contratar contador                 |

### Medio Riesgo

| Riesgo                   | Descripción                 | Sanción Potencial         | Mitigación          |
| ------------------------ | --------------------------- | ------------------------- | ------------------- |
| Retenciones no aplicadas | IR-17 no implementado       | Responsabilidad solidaria | Implementar sistema |
| ITBIS manual             | Posibles errores de cálculo | Recargos e intereses      | Automatizar cálculo |

### Bajo Riesgo

| Riesgo                    | Descripción  | Sanción Potencial     | Mitigación       |
| ------------------------- | ------------ | --------------------- | ---------------- |
| NCF físicos no archivados | Solo digital | Ninguna si hay backup | Mantener backups |

---

## 📊 REPORTES PARA AUDITOR

### Reporte de Ventas (Formato 607)

```sql
SELECT
    i.ncf_number as "NCF",
    CASE
        WHEN c.document_type = 'rnc' THEN '1'
        ELSE '2'
    END as "Tipo ID",
    c.document_number as "RNC/Cedula",
    i.ncf_type as "Tipo NCF",
    TO_CHAR(i.created_at, 'YYYYMMDD') as "Fecha",
    i.subtotal as "Monto sin ITBIS",
    i.itbis_amount as "ITBIS",
    i.total as "Total"
FROM invoices i
LEFT JOIN customers c ON i.customer_id = c.id
WHERE i.created_at BETWEEN '2026-01-01' AND '2026-01-31'
AND i.status = 'completed'
ORDER BY i.created_at;
```

### Reporte de Compras (Formato 606)

```sql
SELECT
    e.ncf as "NCF Proveedor",
    '1' as "Tipo ID",
    s.rnc as "RNC Proveedor",
    '02' as "Tipo NCF",
    TO_CHAR(e.date, 'YYYYMMDD') as "Fecha",
    e.amount as "Monto sin ITBIS",
    e.itbis as "ITBIS",
    e.total as "Total"
FROM expenses e
JOIN suppliers s ON e.supplier_id = s.id
WHERE e.date BETWEEN '2026-01-01' AND '2026-01-31'
AND e.has_valid_ncf = true
ORDER BY e.date;
```

---

## 🔗 INTEGRACIÓN e-CF (Comprobante Fiscal Electrónico)

### Estado Actual

| Aspecto                  | Estado | Acción Requerida                  |
| ------------------------ | ------ | --------------------------------- |
| Habilitación DGII        | 🔴     | Solicitar en Oficina Virtual      |
| Certificado Digital      | 🔴     | Adquirir con proveedor autorizado |
| Sistema Emisor           | 🔴     | Desarrollar o contratar           |
| Pruebas de Certificación | 🔴     | Pendiente habilitación            |
| Producción               | 🔴     | Después de certificación          |

### Requisitos e-CF

1. **Certificado Digital**
   - Emitido por entidad certificadora autorizada
   - Válido y no vencido
   - Instalado en el sistema

2. **Formato XML**
   - Cumplir con esquema XSD de DGII
   - Firmar digitalmente
   - Enviar en tiempo real

3. **Respuesta DGII**
   - Procesar código de autorización
   - Almacenar Track ID
   - Manejar rechazos

---

## 📞 CONTACTOS ÚTILES

| Entidad                | Teléfono     | Web             |
| ---------------------- | ------------ | --------------- |
| DGII - Información     | 809-689-3444 | dgii.gov.do     |
| DGII - Oficina Virtual | -            | ov.dgii.gov.do  |
| DGII - e-CF Soporte    | -            | ecf.dgii.gov.do |

---

**Última revisión:** Enero 25, 2026  
**Próxima revisión:** Antes de cada declaración mensual  
**Responsable:** Responsable Fiscal (pendiente designar)
